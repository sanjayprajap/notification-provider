﻿// <autogenerated />
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.UnitTests.Data.Repositories.WebNotificationItemEntityRepository_Tests
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Net;
    using System.Threading.Tasks;
    using NotificationService.Common.Exceptions;
    using NotificationService.Contracts;
    using NotificationService.Contracts.Entities.Web;
    using NUnit.Framework;

    /// <summary>
    /// For test
    /// </summary>
    /// <seealso cref="WebNotificationItemEntityBaseTests" />
    [ExcludeFromCodeCoverage]
    public class ReadAsync_WebNotificationItemRepository_Tests : WebNotificationItemEntityBaseTests
    {
        /// <summary>
        /// Initializes this instance.
        /// </summary>
        [SetUp]
        public void Initialize()
        {
            this.SetupTestBase();
            _ = this.MockFeedResponse.SetupGet(fr => fr.Resource).Returns(this.NotificationEntities.Where(nt => nt.NotificationId.Equals(this.NotificationId, StringComparison.Ordinal)));
        }

        /// <summary>
        /// Reads the entity with invalid entity identifier.
        /// </summary>
        /// <param name="entityId">The entity identifier.</param>
        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public void ReadAsync_WithInvalidEntityId(string entityId)
        {
            var ex = Assert.ThrowsAsync<ArgumentException>(async () => await this.NotificationRepository.ReadAsync(entityId).ConfigureAwait(false));
            Assert.IsTrue(ex.Message.StartsWith("The entity Id is not specified.", StringComparison.Ordinal));
        }

        /// <summary>
        /// Reads the entity with non existent entity identifier.
        /// </summary>
        [Test]
        public void ReadAsync_WithNonExistentEntityId()
        {
            this.NotificationId = "Notification Id #10";
            var ex = Assert.ThrowsAsync<NotificationServiceException>(async () => await this.NotificationRepository.ReadAsync(this.NotificationId).ConfigureAwait(false));
            Assert.IsTrue(typeof(NotificationServiceException).FullName.Equals(ex.GetType().FullName, StringComparison.Ordinal));
            Assert.IsTrue(ex.Message.Equals($"The notification with notificationId '{this.NotificationId}' is not found.", StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Reads the entity with existent entity identifier.
        /// </summary>
        [Test]
        public async Task ReadAsync_WithExistentEntityId()
        {
            this.NotificationId = "Notification Id #2";
            WebNotificationItemEntity webNotificationItemEntity = this.NotificationEntities.Where(nt => nt.NotificationId.Equals(this.NotificationId, StringComparison.Ordinal)).FirstOrDefault();
            _ = this.MockItemResponse.SetupGet(ir => ir.Resource).Returns(webNotificationItemEntity);
            _ = this.MockItemResponse.SetupGet(ir => ir.StatusCode).Returns(webNotificationItemEntity == null ? HttpStatusCode.BadRequest : HttpStatusCode.OK);
            var webNotificationEntity = await this.NotificationRepository.ReadAsync(this.NotificationId).ConfigureAwait(false);
            Assert.IsTrue(webNotificationEntity.NotificationId.Equals(webNotificationItemEntity.NotificationId, StringComparison.Ordinal));
        }

        /// <summary>
        /// Reads the entities without filter and  order given page size.
        /// </summary>
        /// <param name="pageSize">Size of the page.</param>
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(5)]
        public async Task ReadAsync_WithoutFilterOrderGivenPageSize(int pageSize)
        {
            Expression<Func<WebNotificationItemEntity, bool>> filterExpression = null;
            Expression<Func<WebNotificationItemEntity, NotificationPriority>> orderExpression = null;
            _ = this.MockFeedResponse.SetupGet(fr => fr.Resource).Returns(this.NotificationEntities.OrderByDescending(nt => nt.PublishOnUTCDate).Take(pageSize));
            var continuationToken = (this.NotificationEntities.Count - this.NotificationEntities.OrderByDescending(nt => nt.PublishOnUTCDate).Take(pageSize).Count()) > 0 ? "page 2" : null;
            _ = this.MockFeedResponse.SetupGet(fr => fr.ContinuationToken).Returns(continuationToken);
            var notificationsResponse = await this.NotificationRepository.ReadAsync(filterExpression, orderExpression, nextPageId: null, pageSize).ConfigureAwait(false);
            Assert.IsTrue(notificationsResponse.Items.Count() <= pageSize);
            Assert.AreEqual(continuationToken, notificationsResponse.NextPageId);
            Assert.IsTrue(notificationsResponse.Items.First().PublishOnUTCDate > notificationsResponse.Items.ElementAt(1).PublishOnUTCDate);
        }

        /// <summary>
        /// Reads the entities without filter but with order given page size.
        /// </summary>
        /// <param name="pageSize">Size of the page.</param>
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(5)]
        public async Task ReadAsync_WithoutFilterWithOrderGivenPageSize(int pageSize)
        {
            Expression<Func<WebNotificationItemEntity, bool>> filterExpression = null;
            Expression<Func<WebNotificationItemEntity, NotificationPriority>> orderExpression = notification => notification.Priority;
            _ = this.MockFeedResponse.SetupGet(fr => fr.Resource).Returns(this.NotificationEntities.AsQueryable().OrderBy(orderExpression).ThenByDescending(nt => nt.PublishOnUTCDate).Take(pageSize));
            var continuationToken = (this.NotificationEntities.Count - this.NotificationEntities.AsQueryable().OrderBy(orderExpression).ThenByDescending(nt => nt.PublishOnUTCDate).Take(pageSize).Count()) > 0 ? "page 2" : null;
            _ = this.MockFeedResponse.SetupGet(fr => fr.ContinuationToken).Returns(continuationToken);
            var notificationsResponse = await this.NotificationRepository.ReadAsync(filterExpression, orderExpression, nextPageId: null, pageSize).ConfigureAwait(false);
            Assert.IsTrue(notificationsResponse.Items.Count() <= pageSize);
            Assert.AreEqual(continuationToken, notificationsResponse.NextPageId);
            Assert.IsTrue(notificationsResponse.Items.First().Priority <= notificationsResponse.Items.Last().Priority);
        }

        /// <summary>
        /// Reads the entities with filter and  order given page size.
        /// </summary>
        /// <param name="pageSize">Size of the page.</param>
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(5)]
        public async Task ReadAsync_WithFilterOrderGivenPageSize(int pageSize)
        {
            DateTime utcDate = DateTime.UtcNow;
            Expression<Func<WebNotificationItemEntity, bool>> filterExpression = notification => notification.ExpiresOnUTCDate > utcDate && notification.ReadStatus == NotificationReadStatus.New;
            Expression<Func<WebNotificationItemEntity, NotificationPriority>> orderExpression = notification => notification.Priority;
            _ = this.MockFeedResponse.SetupGet(fr => fr.Resource).Returns(this.NotificationEntities.AsQueryable().Where(filterExpression).OrderBy(orderExpression).ThenByDescending(nt => nt.PublishOnUTCDate).Take(pageSize));
            var continuationToken = (this.NotificationEntities.Count - this.NotificationEntities.AsQueryable().Where(filterExpression).OrderBy(orderExpression).ThenByDescending(nt => nt.PublishOnUTCDate).Take(pageSize).Count()) > 0 ? "page 2" : null;
            _ = this.MockFeedResponse.SetupGet(fr => fr.ContinuationToken).Returns(continuationToken);
            var notificationsResponse = await this.NotificationRepository.ReadAsync(filterExpression, orderExpression, nextPageId: null, pageSize).ConfigureAwait(false);
            Assert.IsTrue(notificationsResponse.Items.Count() <= pageSize);
            Assert.AreEqual(continuationToken, notificationsResponse.NextPageId);
            Assert.IsTrue(notificationsResponse.Items.First().Priority <= notificationsResponse.Items.Last().Priority);
            Assert.IsTrue(notificationsResponse.Items.First().ReadStatus == NotificationReadStatus.New);
            Assert.IsTrue(notificationsResponse.Items.First().ExpiresOnUTCDate > utcDate);
        }
    }
}