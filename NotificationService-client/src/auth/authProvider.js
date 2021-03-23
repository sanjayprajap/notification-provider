// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import axios from 'axios';
import {loginRequest} from '../configuration/auth-config';
import { PublicClientApplication, InteractionRequiredAuthError } from "@azure/msal-browser";
import React, { useState, useContext, useEffect } from "react";

export const instance = axios.create();
export const MsalContext = React.createContext();
export const useMsal = () => useContext(MsalContext);
export const MsalProvider = ({
    children,
    config
}) => {
    const [isAuthenticated, setIsAuthenticated] = useState(false);
    const [user, setUser] = useState();
    instance.interceptors.request.use(config1 => {    
        if (sessionStorage.getItem('msal.idtoken')) {
            config1.headers.Authorization = `Bearer ${sessionStorage.getItem('msal.idtoken')}`;
        }
        else {
            getToken();
        }
        return Promise.resolve(config1);       
    });

    instance.interceptors.response.use(response => {
        return Promise.resolve(response);
    },error => {
        signIn();
    });

    const myMSALObj = new PublicClientApplication(config); 
    useEffect(() => {
        myMSALObj.handleRedirectPromise().then((resp) => {
            if (resp !== null) {
                myMSALObj.setActiveAccount(resp.account);
                sessionStorage.setItem('msal.idtoken', resp.idToken);
                setIsAuthenticated(true);
                setUser(resp.account);
                myMSALObj.setActiveAccount(resp.account);
            } else {
                const currentAccount = GetAccount();
                if(!currentAccount){
                    signIn();
                }
            }
            Promise.resolve(resp);
        }).catch(error => {
            console.log("authentication error : " + error);
        });
    },[]);
    
    const signIn = async () => {
        myMSALObj.loginRedirect(loginRequest);  
    };

    const signOut = async() => {
        const logoutRequest = {
            account: GetAccount()
        };
        myMSALObj.logout(logoutRequest);
    };

    const GetAccount = () => {
        const account = myMSALObj.getActiveAccount();
        return account;
    };

    const getToken = async () => {
        const account = GetAccount();
        const request = {...loginRequest, account: account};
        const response =  await myMSALObj.acquireTokenSilent(request).catch(async (error) => {
            if(error instanceof InteractionRequiredAuthError){
                return myMSALObj.acquireTokenRedirect({...loginRequest});
            }
        });
        if(response && response.idToken){
            setIsAuthenticated(true);
            sessionStorage.setItem('msal.idtoken', response.idToken);
        }
        return response;
    };

    return (
        <MsalContext.Provider
            value={{
            isAuthenticated,
            user,
            signIn,
            signOut,
            getToken
            }}
        >
            {children}
        </MsalContext.Provider>
    );
};