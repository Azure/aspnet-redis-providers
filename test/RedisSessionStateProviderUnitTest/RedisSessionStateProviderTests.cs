//
// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.
//

using System;
using Xunit;
using FakeItEasy;
using System.Collections.Specialized;
using System.Configuration.Provider;
using System.Web.SessionState;
using System.Collections.Generic;
using System.Web.Configuration;

namespace Microsoft.Web.Redis.Tests
{
    public class RedisSessionStateProviderTests
    {
        [Fact]
        public void Initialize_WithNullConfig()
        {
            RedisSessionStateProvider sessionStateStore = new RedisSessionStateProvider();
            Assert.Throws<ArgumentNullException>(() => sessionStateStore.Initialize(null, null));
        }

        [Fact]
        public void EndRequest_Successful()
        {
            Utility.SetConfigUtilityToDefault();
            var mockCache = A.Fake<ICacheConnection>();
            RedisSessionStateProvider sessionStateStore = new RedisSessionStateProvider();
            sessionStateStore.sessionId = "session-id";
            sessionStateStore.sessionLockId = "session-lock-id";
            sessionStateStore.cache = mockCache;
            sessionStateStore.EndRequest(null);
            A.CallTo(() => mockCache.TryReleaseLockIfLockIdMatch(A<object>.Ignored, A<int>.Ignored)).MustHaveHappened();
        }

        [Fact]
        public void CreateNewStoreData_WithEmptyStore()
        {
            Utility.SetConfigUtilityToDefault();
            SessionStateStoreData sssd = new SessionStateStoreData(new ChangeTrackingSessionStateItemCollection(), null, 900);
            RedisSessionStateProvider sessionStateStore = new RedisSessionStateProvider();
            Assert.Equal(true, Utility.CompareSessionStateStoreData(sessionStateStore.CreateNewStoreData(null, 900),sssd));
        }

        [Fact]
        public void CreateUninitializedItem_Successful()
        {
            Utility.SetConfigUtilityToDefault(); 
            string id = "session-id"; 
            var mockCache = A.Fake<ICacheConnection>();
            RedisSessionStateProvider sessionStateStore = new RedisSessionStateProvider();
            sessionStateStore.cache = mockCache;
            sessionStateStore.CreateUninitializedItem(null, id, 15);
            A.CallTo(() => mockCache.Set(A<ISessionStateItemCollection>.That.Matches(
                o => o.Count == 1 && SessionStateActions.InitializeItem.Equals(o["SessionStateActions"]) 
                ), 900)).MustHaveHappened();
        }

        [Fact]
        public void GetItem_NullFromStore()
        {
            Utility.SetConfigUtilityToDefault(); 
            string id = "session-id";
            bool locked;
            TimeSpan lockAge; 
            object lockId = null;
            SessionStateActions actions;

            object mockLockId = 0;
            ISessionStateItemCollection sessionData = null;
            int sessionTimeout;
            var mockCache = A.Fake<ICacheConnection>();
            A.CallTo(() => mockCache.TryCheckWriteLockAndGetData(out mockLockId, out sessionData, out sessionTimeout)).Returns(true); 
            
            RedisSessionStateProvider sessionStateStore = new RedisSessionStateProvider();
            sessionStateStore.cache = mockCache;
            SessionStateStoreData sessionStateStoreData = sessionStateStore.GetItem(null, id, out locked, out lockAge, out lockId, out actions);
            A.CallTo(() => mockCache.TryCheckWriteLockAndGetData(out mockLockId, out sessionData, out sessionTimeout)).MustHaveHappened();
            A.CallTo(() => mockCache.TryReleaseLockIfLockIdMatch(mockLockId, A<int>.Ignored)).MustHaveHappened(); 
            
            Assert.Equal(null, sessionStateStoreData);
            Assert.Equal(false, locked);
            Assert.Equal(TimeSpan.Zero, lockAge);
            Assert.Equal(0, lockId);
        }

        [Fact]
        public void GetItem_RecordLocked()
        {
            Utility.SetConfigUtilityToDefault(); 
            string id = "session-id";
            bool locked;
            TimeSpan lockAge;
            object lockId = null;
            SessionStateActions actions;
            
            object mockLockId = 0;
            ISessionStateItemCollection sessionData = null;
            int sessionTimeout;
            var mockCache = A.Fake<ICacheConnection>();
            A.CallTo(() => mockCache.TryCheckWriteLockAndGetData(out mockLockId, out sessionData, out sessionTimeout)).Returns(false);
            A.CallTo(() => mockCache.GetLockAge(A<object>.Ignored)).Returns(TimeSpan.Zero);

            RedisSessionStateProvider sessionStateStore = new RedisSessionStateProvider();
            sessionStateStore.cache = mockCache;
            SessionStateStoreData sessionStateStoreData = sessionStateStore.GetItem(null, id, out locked, out lockAge, out lockId, out actions);
            A.CallTo(() => mockCache.TryCheckWriteLockAndGetData(out mockLockId, out sessionData, out sessionTimeout)).MustHaveHappened();
            A.CallTo(() => mockCache.GetLockAge(A<object>.Ignored)).MustHaveHappened();
            
            Assert.Equal(null, sessionStateStoreData);
            Assert.Equal(true, locked);
        }

        [Fact]
        public void GetItem_RecordFound()
        {
            Utility.SetConfigUtilityToDefault(); 
            string id = "session-id";
            bool locked;
            TimeSpan lockAge;
            object lockId = null;
            SessionStateActions actions;

            ISessionStateItemCollection sessionStateItemCollection = new ChangeTrackingSessionStateItemCollection();
            sessionStateItemCollection["session-key"] = "session-value";
            sessionStateItemCollection["SessionStateActions"] = SessionStateActions.None;
            SessionStateStoreData sssd = new SessionStateStoreData(sessionStateItemCollection, null, 15);

            ISessionStateItemCollection sessionData = new ChangeTrackingSessionStateItemCollection();
            sessionData["session-key"] = "session-value";
            sessionData["SessionStateActions"] = SessionStateActions.None;
            ISessionStateItemCollection mockSessionData = null;
            object mockLockId = 0;
            int mockSessionTimeout;
            int sessionTimeout = (int)RedisSessionStateProvider.configuration.SessionTimeout.TotalMinutes;
            var mockCache = A.Fake<ICacheConnection>();
            A.CallTo(() => mockCache.TryCheckWriteLockAndGetData(out mockLockId, out mockSessionData, out mockSessionTimeout)).Returns(true).AssignsOutAndRefParameters(0, sessionData, (int)RedisSessionStateProvider.configuration.SessionTimeout.TotalMinutes);

            RedisSessionStateProvider sessionStateStore = new RedisSessionStateProvider();
            sessionStateStore.cache = mockCache;
            SessionStateStoreData sessionStateStoreData = sessionStateStore.GetItem(null, id, out locked, out lockAge, out lockId, out actions);
            A.CallTo(() => mockCache.TryCheckWriteLockAndGetData(out mockLockId, out sessionData, out sessionTimeout)).MustHaveHappened();
            
            
            Assert.Equal(true, Utility.CompareSessionStateStoreData(sessionStateStoreData, sssd));
            Assert.Equal(false, locked);
            Assert.Equal(TimeSpan.Zero, lockAge);
            Assert.Equal(actions, SessionStateActions.None);
        }

        [Fact]
        public void GetItemExclusive_RecordLocked()
        {
            Utility.SetConfigUtilityToDefault(); 
            string id = "session-id";
            bool locked;
            TimeSpan lockAge;
            object lockId = null;
            SessionStateActions actions;

            object mockLockId = 0;
            ISessionStateItemCollection sessionData = null;
            int sessionTimeout;
            var mockCache = A.Fake<ICacheConnection>();
            A.CallTo(() => mockCache.TryTakeWriteLockAndGetData(A<DateTime>.Ignored, 90, out mockLockId, out sessionData, out sessionTimeout)).Returns(false);
            A.CallTo(() => mockCache.GetLockAge(A<object>.Ignored)).Returns(TimeSpan.Zero);


            RedisSessionStateProvider sessionStateStore = new RedisSessionStateProvider();
            sessionStateStore.cache = mockCache;
            SessionStateStoreData sessionStateStoreData = sessionStateStore.GetItemExclusive(null, id, out locked, out lockAge, out lockId, out actions);
            A.CallTo(() => mockCache.TryTakeWriteLockAndGetData(A<DateTime>.Ignored, 90, out mockLockId, out sessionData, out sessionTimeout)).MustHaveHappened();
            A.CallTo(() => mockCache.GetLockAge(A<object>.Ignored)).MustHaveHappened();

            Assert.Equal(null, sessionStateStoreData);
            Assert.Equal(true, locked);
        }

        [Fact]
        public void GetItemExclusive_LockedRecordUpdated()
        {
            Utility.SetConfigUtilityToDefault(); 
            string id = "session-id";
            bool locked;
            TimeSpan lockAge = TimeSpan.FromSeconds(6);
            object lockId = null;
            SessionStateActions actions;

            ISessionStateItemCollection sessionStateItemCollection = new ChangeTrackingSessionStateItemCollection();
            sessionStateItemCollection["session-key"] = "session-value";
            SessionStateStoreData sssd = new SessionStateStoreData(sessionStateItemCollection, null, 15);

            ISessionStateItemCollection sessionData = new ChangeTrackingSessionStateItemCollection();
            sessionData["session-key"] = "session-value";
            
            ISessionStateItemCollection mockSessionData = null;
            object mockLockId = 0;
            int mockSessionTimeout;
            int sessionTimeout = (int)RedisSessionStateProvider.configuration.SessionTimeout.TotalMinutes;
            var mockCache = A.Fake<ICacheConnection>();

            A.CallTo(() => mockCache.TryTakeWriteLockAndGetData(A<DateTime>.Ignored, 90, out lockId, out sessionData, out sessionTimeout)).Returns(false);
            A.CallTo(() => mockCache.GetLockAge(A<object>.Ignored)).Returns(TimeSpan.FromSeconds(6));

            RedisSessionStateProvider sessionStateStore = new RedisSessionStateProvider();
            sessionStateStore.cache = mockCache;
            SessionStateStoreData sessionStateStoreData = sessionStateStore.GetItemExclusive(null, id, out locked, out lockAge, out lockId, out actions);
            Assert.Equal(null, sessionStateStoreData);

            A.CallTo(() => mockCache.TryTakeWriteLockAndGetData(A<DateTime>.Ignored, 90, out mockLockId, out mockSessionData, out mockSessionTimeout)).Returns(true).AssignsOutAndRefParameters(0, sessionData, (int)RedisSessionStateProvider.configuration.SessionTimeout.TotalMinutes);
            sessionStateStoreData = sessionStateStore.GetItemExclusive(null, id, out locked, out lockAge, out lockId, out actions);

            Assert.Equal(true, Utility.CompareSessionStateStoreData(sessionStateStoreData, sssd));
            Assert.Equal(TimeSpan.Zero, lockAge);
        }

        [Fact]
        public void GetItemExclusive_RecordFound()
        {
            Utility.SetConfigUtilityToDefault(); 
            string id = "session-id";
            bool locked;
            TimeSpan lockAge;
            object lockId = null;
            SessionStateActions actions;

            ISessionStateItemCollection sessionStateItemCollection = new ChangeTrackingSessionStateItemCollection();
            sessionStateItemCollection["session-key"] = "session-value";
            SessionStateStoreData sssd = new SessionStateStoreData(sessionStateItemCollection, null, 15);

            ISessionStateItemCollection sessionData = new ChangeTrackingSessionStateItemCollection();
            sessionData["session-key"] = "session-value";
            
            ISessionStateItemCollection mockSessionData = null;
            object mockLockId = 0;
            int mockSessionTimeout;
            int sessionTimeout = (int)RedisSessionStateProvider.configuration.SessionTimeout.TotalMinutes;
            var mockCache = A.Fake<ICacheConnection>();
            A.CallTo(() => mockCache.TryTakeWriteLockAndGetData(A<DateTime>.Ignored, 90, out mockLockId, out mockSessionData, out mockSessionTimeout)).Returns(true).AssignsOutAndRefParameters(0, sessionData, (int)RedisSessionStateProvider.configuration.SessionTimeout.TotalMinutes);
            
            RedisSessionStateProvider sessionStateStore = new RedisSessionStateProvider();
            sessionStateStore.cache = mockCache;
            SessionStateStoreData sessionStateStoreData = sessionStateStore.GetItemExclusive(null, id, out locked, out lockAge, out lockId, out actions);
            A.CallTo(() => mockCache.TryTakeWriteLockAndGetData(A<DateTime>.Ignored, 90, out mockLockId, out sessionData, out sessionTimeout)).MustHaveHappened();

            Assert.Equal(true, Utility.CompareSessionStateStoreData(sessionStateStoreData, sssd));
            Assert.Equal(false, locked);
            Assert.Equal(TimeSpan.Zero, lockAge);
            Assert.Equal(actions, SessionStateActions.None);
        }

        [Fact]
        public void ResetItemTimeout_Successful()
        {
            Utility.SetConfigUtilityToDefault(); 
            string id = "session-id";
            var mockCache = A.Fake<ICacheConnection>();
            
            RedisSessionStateProvider sessionStateStore = new RedisSessionStateProvider();
            sessionStateStore.cache = mockCache;
            sessionStateStore.ResetItemTimeout(null, id);
            A.CallTo(() => mockCache.UpdateExpiryTime(900)).MustHaveHappened();
        }

        [Fact]
        public void RemoveItem_Successful()
        {
            Utility.SetConfigUtilityToDefault();
            string id = "session-id";
            var mockCache = A.Fake<ICacheConnection>();
            RedisSessionStateProvider sessionStateStore = new RedisSessionStateProvider();
            sessionStateStore.cache = mockCache;
            sessionStateStore.RemoveItem(null, id, "lockId", null);
            A.CallTo(() => mockCache.TryRemoveAndReleaseLockIfLockIdMatch(A<object>.Ignored)).MustHaveHappened();
        }

        [Fact]
        public void ReleaseItemExclusive_Successful()
        {
            Utility.SetConfigUtilityToDefault(); 
            string id = "session-id";
            var mockCache = A.Fake<ICacheConnection>();
            RedisSessionStateProvider sessionStateStore = new RedisSessionStateProvider();
            sessionStateStore.cache = mockCache;
            sessionStateStore.ReleaseItemExclusive(null, id, "lockId");
            A.CallTo(() => mockCache.TryReleaseLockIfLockIdMatch(A<object>.Ignored, A<int>.Ignored)).MustHaveHappened();
        }

        [Fact]
        public void SetAndReleaseItemExclusive_NewItemNullItems()
        {
            Utility.SetConfigUtilityToDefault(); 
            string id = "session-id";
            SessionStateStoreData sssd = new SessionStateStoreData(null, null, 15);

            var mockCache = A.Fake<ICacheConnection>();
            RedisSessionStateProvider sessionStateStore = new RedisSessionStateProvider();
            sessionStateStore.cache = mockCache;
            sessionStateStore.SetAndReleaseItemExclusive(null, id, sssd, null, true);
            A.CallTo(() => mockCache.Set(A<ISessionStateItemCollection>.That.Matches(o => o.Count == 0), 900)).MustHaveHappened();
        }

        [Fact]
        public void SetAndReleaseItemExclusive_NewItemValidItems()
        {
            Utility.SetConfigUtilityToDefault();
            string id = "session-id";
            ChangeTrackingSessionStateItemCollection sessionStateItemCollection = new ChangeTrackingSessionStateItemCollection();
            sessionStateItemCollection["session-key"] = "session-value";
            SessionStateStoreData sssd = new SessionStateStoreData(sessionStateItemCollection, null, 15);

            var mockCache = A.Fake<ICacheConnection>();
            RedisSessionStateProvider sessionStateStore = new RedisSessionStateProvider();
            sessionStateStore.cache = mockCache;
            sessionStateStore.SetAndReleaseItemExclusive(null, id, sssd, null, true);
            A.CallTo(() => mockCache.Set(A<ISessionStateItemCollection>.That.Matches(
                o => o.Count == 1 && o["session-key"] != null
                ), 900)).MustHaveHappened();
        }

        [Fact]
        public void SetAndReleaseItemExclusive_OldItemNullItems()
        {
            Utility.SetConfigUtilityToDefault();
            string id = "session-id";
            SessionStateStoreData sssd = new SessionStateStoreData(null, null, 900);

            var mockCache = A.Fake<ICacheConnection>();
            RedisSessionStateProvider sessionStateStore = new RedisSessionStateProvider();
            sessionStateStore.cache = mockCache;
            sessionStateStore.SetAndReleaseItemExclusive(null, id, sssd, 7, false);
            A.CallTo(() => mockCache.TryUpdateAndReleaseLockIfLockIdMatch(A<object>.Ignored, A<ISessionStateItemCollection>.Ignored, 900)).MustNotHaveHappened();
        }

        [Fact]
        public void SetAndReleaseItemExclusive_OldItemRemovedItems()
        {
            Utility.SetConfigUtilityToDefault();
            string id = "session-id";
            ChangeTrackingSessionStateItemCollection sessionStateItemCollection = new ChangeTrackingSessionStateItemCollection();
            sessionStateItemCollection["session-key"] = "session-val";
            sessionStateItemCollection.Remove("session-key");
            SessionStateStoreData sssd = new SessionStateStoreData(sessionStateItemCollection, null, 15);

            var mockCache = A.Fake<ICacheConnection>();
            RedisSessionStateProvider sessionStateStore = new RedisSessionStateProvider();
            sessionStateStore.cache = mockCache;
            sessionStateStore.SetAndReleaseItemExclusive(null, id, sssd, 7, false);
            A.CallTo(() => mockCache.TryUpdateAndReleaseLockIfLockIdMatch(A<object>.Ignored, 
                A<ChangeTrackingSessionStateItemCollection>.That.Matches(o => o.Count == 0 && o.GetModifiedKeys().Count == 0 && o.GetDeletedKeys().Count == 1), 900)).MustHaveHappened();
        }

        [Fact]
        public void SetAndReleaseItemExclusive_OldItemInsertedItems()
        {
            Utility.SetConfigUtilityToDefault();
            string id = "session-id";
            ChangeTrackingSessionStateItemCollection sessionStateItemCollection = new ChangeTrackingSessionStateItemCollection();
            sessionStateItemCollection["session-key"] = "session-value";
            SessionStateStoreData sssd = new SessionStateStoreData(sessionStateItemCollection, null, 15);

            var mockCache = A.Fake<ICacheConnection>();
            RedisSessionStateProvider sessionStateStore = new RedisSessionStateProvider();
            sessionStateStore.cache = mockCache;
            sessionStateStore.SetAndReleaseItemExclusive(null, id, sssd, 7, false);
            A.CallTo(() => mockCache.TryUpdateAndReleaseLockIfLockIdMatch(A<object>.Ignored, 
                A<ChangeTrackingSessionStateItemCollection>.That.Matches(o => o.Count == 1 && o.GetModifiedKeys().Count == 1 && o.GetDeletedKeys().Count == 0), 900)).MustHaveHappened();  
        }
    }
}
