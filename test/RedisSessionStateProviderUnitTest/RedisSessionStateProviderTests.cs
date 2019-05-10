﻿//
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
using System.Threading.Tasks;
using System.Threading;
#if DOTNET_462
using Microsoft.AspNet.SessionState;
#endif

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
#if DOTNET_462
        public async Task EndRequest_Successful()
#else
        public void EndRequest_Successful()
#endif
        {
            Utility.SetConfigUtilityToDefault();
            var mockCache = A.Fake<ICacheConnection>();
            RedisSessionStateProvider sessionStateStore = new RedisSessionStateProvider();
            sessionStateStore.sessionId = "session-id";
            sessionStateStore.sessionLockId = "session-lock-id";
            sessionStateStore.cache = mockCache;
#if DOTNET_462
            await sessionStateStore.EndRequestAsync(null);
#else
            sessionStateStore.EndRequest(null);
#endif
            A.CallTo(() => mockCache.TryReleaseLockIfLockIdMatch(A<object>.Ignored, A<int>.Ignored)).MustHaveHappened();
        }

        [Fact]
        public void CreateNewStoreData_WithEmptyStore()
        {
            Utility.SetConfigUtilityToDefault();
            SessionStateStoreData sssd = new SessionStateStoreData(Utility.GetChangeTrackingSessionStateItemCollection(), null, 900);
            RedisSessionStateProvider sessionStateStore = new RedisSessionStateProvider();
            Assert.Equal(true, Utility.CompareSessionStateStoreData(sessionStateStore.CreateNewStoreData(null, 900),sssd));
        }

        [Fact]
#if DOTNET_462
        public async Task CreateUninitializedItem_Successful()
#else
        public void CreateUninitializedItem_Successful()
#endif
        {
            Utility.SetConfigUtilityToDefault(); 
            string id = "session-id"; 
            var mockCache = A.Fake<ICacheConnection>();
            RedisSessionStateProvider sessionStateStore = new RedisSessionStateProvider();
            sessionStateStore.cache = mockCache;
#if DOTNET_462
            await sessionStateStore.CreateUninitializedItemAsync(null, id, 15, CancellationToken.None);
#else
            sessionStateStore.CreateUninitializedItem(null, id, 15); 
#endif
            A.CallTo(() => mockCache.Set(A<ISessionStateItemCollection>.That.Matches(
                o => o.Count == 1 && SessionStateActions.InitializeItem.Equals(o["SessionStateActions"]) 
                ), 900)).MustHaveHappened();
        }

        [Fact]
#if DOTNET_462
        public async Task GetItem_NullFromStore()
#else
        public void GetItem_NullFromStore()
#endif
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

            SessionStateStoreData sessionStateStoreData = null;
#if DOTNET_462
            GetItemResult data = await sessionStateStore.GetItemAsync(null, id, CancellationToken.None);
            sessionStateStoreData = data.Item;
            locked = data.Locked;
            lockAge = data.LockAge;
            lockId = data.LockId;
            actions = data.Actions;
#else
            sessionStateStoreData = sessionStateStore.GetItem(null, id, out locked, out lockAge, out lockId, out actions);
#endif
            A.CallTo(() => mockCache.TryCheckWriteLockAndGetData(out mockLockId, out sessionData, out sessionTimeout)).MustHaveHappened();
            A.CallTo(() => mockCache.TryReleaseLockIfLockIdMatch(mockLockId, A<int>.Ignored)).MustHaveHappened(); 
            
            Assert.Equal(null, sessionStateStoreData);
            Assert.Equal(false, locked);
            Assert.Equal(TimeSpan.Zero, lockAge);
            Assert.Equal(0, lockId);
        }

        [Fact]
#if DOTNET_462
        public async Task GetItem_RecordLocked()
#else
        public void GetItem_RecordLocked()
#endif
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
            SessionStateStoreData sessionStateStoreData;

#if DOTNET_462
            GetItemResult data = await sessionStateStore.GetItemAsync(null, id, CancellationToken.None);
            sessionStateStoreData = data.Item;
            locked = data.Locked;
            lockAge = data.LockAge;
            lockId = data.LockId;
            actions = data.Actions;
#else
            sessionStateStoreData = sessionStateStore.GetItem(null, id, out locked, out lockAge, out lockId, out actions);
#endif
            A.CallTo(() => mockCache.TryCheckWriteLockAndGetData(out mockLockId, out sessionData, out sessionTimeout)).MustHaveHappened();
            A.CallTo(() => mockCache.GetLockAge(A<object>.Ignored)).MustHaveHappened();
            
            Assert.Equal(null, sessionStateStoreData);
            Assert.Equal(true, locked);
        }

        [Fact]
#if DOTNET_462
        public async Task GetItem_RecordFound()
#else
        public void GetItem_RecordFound()
#endif
        {
            Utility.SetConfigUtilityToDefault(); 
            string id = "session-id";
            bool locked;
            TimeSpan lockAge;
            object lockId = null;
            SessionStateActions actions;

            ISessionStateItemCollection sessionStateItemCollection = Utility.GetChangeTrackingSessionStateItemCollection();
            sessionStateItemCollection["session-key"] = "session-value";
            sessionStateItemCollection["SessionStateActions"] = SessionStateActions.None;
            SessionStateStoreData sssd = new SessionStateStoreData(sessionStateItemCollection, null, 15);

            ISessionStateItemCollection sessionData = Utility.GetChangeTrackingSessionStateItemCollection();
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
            SessionStateStoreData sessionStateStoreData;

#if DOTNET_462
            GetItemResult data = await sessionStateStore.GetItemAsync(null, id, CancellationToken.None);
            sessionStateStoreData = data.Item;
            locked = data.Locked;
            lockAge = data.LockAge;
            lockId = data.LockId;
            actions = data.Actions;
#else
            sessionStateStoreData = sessionStateStore.GetItem(null, id, out locked, out lockAge, out lockId, out actions);
#endif
            A.CallTo(() => mockCache.TryCheckWriteLockAndGetData(out mockLockId, out sessionData, out sessionTimeout)).MustHaveHappened();
            Assert.Equal(true, Utility.CompareSessionStateStoreData(sessionStateStoreData, sssd));
            Assert.Equal(false, locked);
            Assert.Equal(TimeSpan.Zero, lockAge);
            Assert.Equal(actions, SessionStateActions.None);
        }

        [Fact]
#if DOTNET_462
        public async Task GetItemExclusive_RecordLocked()
#else
        public void GetItemExclusive_RecordLocked()
#endif
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
            SessionStateStoreData sessionStateStoreData;

#if DOTNET_462
            GetItemResult data = await sessionStateStore.GetItemExclusiveAsync(null, id, CancellationToken.None);
            sessionStateStoreData = data.Item;
            locked = data.Locked;
            lockAge = data.LockAge;
            lockId = data.LockId;
            actions = data.Actions;
#else
            sessionStateStoreData = sessionStateStore.GetItemExclusive(null, id, out locked, out lockAge, out lockId, out actions);
#endif
            A.CallTo(() => mockCache.TryTakeWriteLockAndGetData(A<DateTime>.Ignored, 90, out mockLockId, out sessionData, out sessionTimeout)).MustHaveHappened();
            A.CallTo(() => mockCache.GetLockAge(A<object>.Ignored)).MustHaveHappened();

            Assert.Equal(null, sessionStateStoreData);
            Assert.Equal(true, locked);
        }

        [Fact]
#if DOTNET_462
        public async Task GetItemExclusive_RecordFound()
#else
        public void GetItemExclusive_RecordFound()
#endif
        {
            Utility.SetConfigUtilityToDefault(); 
            string id = "session-id";
            bool locked;
            TimeSpan lockAge;
            object lockId = null;
            SessionStateActions actions;

            ISessionStateItemCollection sessionStateItemCollection = Utility.GetChangeTrackingSessionStateItemCollection();
            sessionStateItemCollection["session-key"] = "session-value";
            SessionStateStoreData sssd = new SessionStateStoreData(sessionStateItemCollection, null, 15);

            ISessionStateItemCollection sessionData = Utility.GetChangeTrackingSessionStateItemCollection();
            sessionData["session-key"] = "session-value";
            
            ISessionStateItemCollection mockSessionData = null;
            object mockLockId = 0;
            int mockSessionTimeout;
            int sessionTimeout = (int)RedisSessionStateProvider.configuration.SessionTimeout.TotalMinutes;
            var mockCache = A.Fake<ICacheConnection>();
            A.CallTo(() => mockCache.TryTakeWriteLockAndGetData(A<DateTime>.Ignored, 90, out mockLockId, out mockSessionData, out mockSessionTimeout)).Returns(true).AssignsOutAndRefParameters(0, sessionData, (int)RedisSessionStateProvider.configuration.SessionTimeout.TotalMinutes);
            
            RedisSessionStateProvider sessionStateStore = new RedisSessionStateProvider();
            sessionStateStore.cache = mockCache;
            SessionStateStoreData sessionStateStoreData;
#if DOTNET_462
            GetItemResult data = await sessionStateStore.GetItemExclusiveAsync(null, id, CancellationToken.None);
            sessionStateStoreData = data.Item;
            locked = data.Locked;
            lockAge = data.LockAge;
            lockId = data.LockId;
            actions = data.Actions;
#else
            sessionStateStoreData = sessionStateStore.GetItemExclusive(null, id, out locked, out lockAge, out lockId, out actions);
#endif
            A.CallTo(() => mockCache.TryTakeWriteLockAndGetData(A<DateTime>.Ignored, 90, out mockLockId, out sessionData, out sessionTimeout)).MustHaveHappened();

            Assert.Equal(true, Utility.CompareSessionStateStoreData(sessionStateStoreData, sssd));
            Assert.Equal(false, locked);
            Assert.Equal(TimeSpan.Zero, lockAge);
            Assert.Equal(actions, SessionStateActions.None);
        }

        [Fact]
#if DOTNET_462
        public async Task ResetItemTimeout_Successful()
#else
        public void ResetItemTimeout_Successful()
#endif
        {
            Utility.SetConfigUtilityToDefault(); 
            string id = "session-id";
            var mockCache = A.Fake<ICacheConnection>();
            
            RedisSessionStateProvider sessionStateStore = new RedisSessionStateProvider();
            sessionStateStore.cache = mockCache;
#if DOTNET_462
            await sessionStateStore.ResetItemTimeoutAsync(null, id, CancellationToken.None);
#else
            sessionStateStore.ResetItemTimeout(null, id); 
#endif
            A.CallTo(() => mockCache.UpdateExpiryTime(900)).MustHaveHappened();
        }

        [Fact]
#if DOTNET_462
        public async Task RemoveItem_Successful()
#else
        public void RemoveItem_Successful()
#endif
        {
            Utility.SetConfigUtilityToDefault();
            string id = "session-id";
            var mockCache = A.Fake<ICacheConnection>();
            RedisSessionStateProvider sessionStateStore = new RedisSessionStateProvider();
            sessionStateStore.cache = mockCache;
#if DOTNET_462
            await sessionStateStore.RemoveItemAsync(null, id, "lockId", null, CancellationToken.None);
#else
            sessionStateStore.RemoveItem(null, id, "lockId", null); 
#endif
            A.CallTo(() => mockCache.TryRemoveAndReleaseLock(A<object>.Ignored)).MustHaveHappened();
        }

        [Fact]
#if DOTNET_462
        public async Task ReleaseItemExclusive_Successful()
#else
        public void ReleaseItemExclusive_Successful()
#endif
        {
            Utility.SetConfigUtilityToDefault(); 
            string id = "session-id";
            var mockCache = A.Fake<ICacheConnection>();
            RedisSessionStateProvider sessionStateStore = new RedisSessionStateProvider();
            sessionStateStore.cache = mockCache;
#if DOTNET_462
            await sessionStateStore.ReleaseItemExclusiveAsync(null, id, "lockId", CancellationToken.None);
#else
            sessionStateStore.ReleaseItemExclusive(null, id, "lockId"); 
#endif
            A.CallTo(() => mockCache.TryReleaseLockIfLockIdMatch(A<object>.Ignored, A<int>.Ignored)).MustHaveHappened();
        }

        [Fact]
#if DOTNET_462
        public async Task SetAndReleaseItemExclusive_NewItemNullItems()
#else
        public void SetAndReleaseItemExclusive_NewItemNullItems()
#endif
        {
            Utility.SetConfigUtilityToDefault(); 
            string id = "session-id";
            SessionStateStoreData sssd = new SessionStateStoreData(null, null, 15);

            var mockCache = A.Fake<ICacheConnection>();
            RedisSessionStateProvider sessionStateStore = new RedisSessionStateProvider();
            sessionStateStore.cache = mockCache;
#if DOTNET_462
            await sessionStateStore.SetAndReleaseItemExclusiveAsync(null, id, sssd, null, true, CancellationToken.None);
#else
            sessionStateStore.SetAndReleaseItemExclusive(null, id, sssd, null, true); 
#endif
            A.CallTo(() => mockCache.Set(A<ISessionStateItemCollection>.That.Matches(o => o.Count == 0), 900)).MustHaveHappened();
        }

        [Fact]
#if DOTNET_462
        public async Task SetAndReleaseItemExclusive_NewItemValidItems()
#else
        public void SetAndReleaseItemExclusive_NewItemValidItems()
#endif
        {
            Utility.SetConfigUtilityToDefault();
            string id = "session-id";
            ChangeTrackingSessionStateItemCollection sessionStateItemCollection = Utility.GetChangeTrackingSessionStateItemCollection();
            sessionStateItemCollection["session-key"] = "session-value";
            SessionStateStoreData sssd = new SessionStateStoreData(sessionStateItemCollection, null, 15);

            var mockCache = A.Fake<ICacheConnection>();
            RedisSessionStateProvider sessionStateStore = new RedisSessionStateProvider();
            sessionStateStore.cache = mockCache;
#if DOTNET_462
            await sessionStateStore.SetAndReleaseItemExclusiveAsync(null, id, sssd, null, true, CancellationToken.None);
#else
            sessionStateStore.SetAndReleaseItemExclusive(null, id, sssd, null, true); 
#endif
            A.CallTo(() => mockCache.Set(A<ISessionStateItemCollection>.That.Matches(
                o => o.Count == 1 && o["session-key"] != null
                ), 900)).MustHaveHappened();
        }

        [Fact]
#if DOTNET_462
        public async Task SetAndReleaseItemExclusive_OldItemNullItems()
#else
        public void SetAndReleaseItemExclusive_OldItemNullItems()
#endif
        {
            Utility.SetConfigUtilityToDefault();
            string id = "session-id";
            SessionStateStoreData sssd = new SessionStateStoreData(null, null, 900);

            var mockCache = A.Fake<ICacheConnection>();
            RedisSessionStateProvider sessionStateStore = new RedisSessionStateProvider();
            sessionStateStore.cache = mockCache;
#if DOTNET_462
            await sessionStateStore.SetAndReleaseItemExclusiveAsync(null, id, sssd, 7, false, CancellationToken.None);
#else
            sessionStateStore.SetAndReleaseItemExclusive(null, id, sssd, 7, false); 
#endif
            A.CallTo(() => mockCache.TryUpdateAndReleaseLock(A<object>.Ignored, A<ISessionStateItemCollection>.Ignored, 900)).MustNotHaveHappened();
        }

        [Fact]
#if DOTNET_462
        public async Task SetAndReleaseItemExclusive_OldItemRemovedItems()
#else
        public void SetAndReleaseItemExclusive_OldItemRemovedItems()
#endif
        {
            Utility.SetConfigUtilityToDefault();
            string id = "session-id";
            ChangeTrackingSessionStateItemCollection sessionStateItemCollection = Utility.GetChangeTrackingSessionStateItemCollection();
            sessionStateItemCollection["session-key"] = "session-val";
            sessionStateItemCollection.Remove("session-key");
            SessionStateStoreData sssd = new SessionStateStoreData(sessionStateItemCollection, null, 15);

            var mockCache = A.Fake<ICacheConnection>();
            RedisSessionStateProvider sessionStateStore = new RedisSessionStateProvider();
            sessionStateStore.cache = mockCache;
#if DOTNET_462
            await sessionStateStore.SetAndReleaseItemExclusiveAsync(null, id, sssd, 7, false, CancellationToken.None);
#else
            sessionStateStore.SetAndReleaseItemExclusive(null, id, sssd, 7, false); 
#endif
            A.CallTo(() => mockCache.TryUpdateAndReleaseLock(A<object>.Ignored, 
                A<ChangeTrackingSessionStateItemCollection>.That.Matches(o => o.Count == 0 && o.GetModifiedKeys().Count == 0 && o.GetDeletedKeys().Count == 1), 900)).MustHaveHappened();
        }

        [Fact]
#if DOTNET_462
        public async Task SetAndReleaseItemExclusive_OldItemInsertedItems()
#else
        public void SetAndReleaseItemExclusive_OldItemInsertedItems()
#endif
        {
            Utility.SetConfigUtilityToDefault();
            string id = "session-id";
            ChangeTrackingSessionStateItemCollection sessionStateItemCollection = Utility.GetChangeTrackingSessionStateItemCollection();
            sessionStateItemCollection["session-key"] = "session-value";
            SessionStateStoreData sssd = new SessionStateStoreData(sessionStateItemCollection, null, 15);

            var mockCache = A.Fake<ICacheConnection>();
            RedisSessionStateProvider sessionStateStore = new RedisSessionStateProvider();
            sessionStateStore.cache = mockCache;
#if DOTNET_462
            await sessionStateStore.SetAndReleaseItemExclusiveAsync(null, id, sssd, 7, false, CancellationToken.None);
#else
            sessionStateStore.SetAndReleaseItemExclusive(null, id, sssd, 7, false); 
#endif
            A.CallTo(() => mockCache.TryUpdateAndReleaseLock(A<object>.Ignored, 
                A<ChangeTrackingSessionStateItemCollection>.That.Matches(o => o.Count == 1 && o.GetModifiedKeys().Count == 1 && o.GetDeletedKeys().Count == 0), 900)).MustHaveHappened();  
        }
    }
}
