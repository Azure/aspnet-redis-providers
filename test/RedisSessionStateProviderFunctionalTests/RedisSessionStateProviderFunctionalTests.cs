//
// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.
//

using FakeItEasy;
using Microsoft.Web.Redis.Tests;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.SessionState;
using Xunit;

namespace Microsoft.Web.Redis.FunctionalTests
{
    public class RedisSessionStateProviderFunctionalTests
    {
        private string ResetRedisConnectionWrapperAndConfiguration()
        {
            RedisConnectionWrapper.sharedConnection = null;
            RedisSessionStateProvider.configuration = Utility.GetDefaultConfigUtility();
            return Guid.NewGuid().ToString();
        }

        private void DisposeRedisConnectionWrapper()
        {
            RedisConnectionWrapper.sharedConnection = null;
        }

        [Fact]
        public void SessionWriteCycle_Valid()
        {
            using (RedisServer redisServer = new RedisServer())
            {
                string sessionId = ResetRedisConnectionWrapperAndConfiguration();

                // Inserting empty session with "SessionStateActions.InitializeItem" flag into redis server
                RedisSessionStateProvider ssp = new RedisSessionStateProvider();
                ssp.CreateUninitializedItem(null, sessionId, (int)RedisSessionStateProvider.configuration.SessionTimeout.TotalMinutes);

                // Get write lock and session from cache
                bool locked;
                TimeSpan lockAge;
                object lockId;
                SessionStateActions actions;
                SessionStateStoreData storeData = ssp.GetItemExclusive(null, sessionId, out locked, out lockAge, out lockId, out actions);

                // Get actual connection and varify lock and session timeout
                IDatabase actualConnection = GetRealRedisConnection();
                Assert.Equal(lockId.ToString(), actualConnection.StringGet(ssp.cache.Keys.LockKey).ToString());
                Assert.Equal(((int)RedisSessionStateProvider.configuration.SessionTimeout.TotalSeconds).ToString(), actualConnection.HashGet(ssp.cache.Keys.InternalKey, "SessionTimeout").ToString());

                // setting data as done by any normal session operation
                storeData.Items["key"] = "value";

                // session update
                ssp.SetAndReleaseItemExclusive(null, sessionId, storeData, lockId, false);
                Assert.Equal(1, actualConnection.HashGetAll(ssp.cache.Keys.DataKey).Length);

                // reset sessions timoue
                ssp.ResetItemTimeout(null, sessionId);

                // End request
                ssp.EndRequest(null);

                // remove data and lock from redis
                DisposeRedisConnectionWrapper();
            }
        }

        [Fact]
        public void SessionReadCycle_Valid()
        {
            using (RedisServer redisServer = new RedisServer())
            {
                string sessionId = ResetRedisConnectionWrapperAndConfiguration();

                // Inserting empty session with "SessionStateActions.InitializeItem" flag into redis server
                RedisSessionStateProvider ssp = new RedisSessionStateProvider();
                ssp.CreateUninitializedItem(null, sessionId, (int)RedisSessionStateProvider.configuration.SessionTimeout.TotalMinutes);

                // Get write lock and session from cache
                bool locked;
                TimeSpan lockAge;
                object lockId;
                SessionStateActions actions;
                SessionStateStoreData storeData = ssp.GetItem(null, sessionId, out locked, out lockAge, out lockId, out actions);

                // Get actual connection and varify lock and session timeout
                IDatabase actualConnection = GetRealRedisConnection();
                Assert.True(actualConnection.StringGet(ssp.cache.Keys.LockKey).IsNull);
                Assert.Equal(((int)RedisSessionStateProvider.configuration.SessionTimeout.TotalSeconds).ToString(), actualConnection.HashGet(ssp.cache.Keys.InternalKey, "SessionTimeout").ToString());

                // reset sessions timoue
                ssp.ResetItemTimeout(null, sessionId);

                // End request
                ssp.EndRequest(null);

                // remove data and lock from redis
                DisposeRedisConnectionWrapper();
            }
        }

        [Fact]
        public void SessionTimoutChangeFromGlobalAspx()
        {
            using (RedisServer redisServer = new RedisServer())
            {
                string sessionId = ResetRedisConnectionWrapperAndConfiguration();

                // Inserting empty session with "SessionStateActions.InitializeItem" flag into redis server
                RedisSessionStateProvider ssp = new RedisSessionStateProvider();
                ssp.CreateUninitializedItem(null, sessionId, (int)RedisSessionStateProvider.configuration.SessionTimeout.TotalMinutes);

                // Get write lock and session from cache
                bool locked;
                TimeSpan lockAge;
                object lockId;
                SessionStateActions actions;
                SessionStateStoreData storeData = ssp.GetItemExclusive(null, sessionId, out locked, out lockAge, out lockId, out actions);

                // Get actual connection and varify lock and session timeout
                IDatabase actualConnection = GetRealRedisConnection();
                Assert.Equal(lockId.ToString(), actualConnection.StringGet(ssp.cache.Keys.LockKey).ToString());
                Assert.Equal(((int)RedisSessionStateProvider.configuration.SessionTimeout.TotalSeconds).ToString(), actualConnection.HashGet(ssp.cache.Keys.InternalKey, "SessionTimeout").ToString());

                // setting data as done by any normal session operation
                storeData.Items["key"] = "value";
                storeData.Timeout = 5;

                // session update
                ssp.SetAndReleaseItemExclusive(null, sessionId, storeData, lockId, false);
                Assert.Equal(1, actualConnection.HashGetAll(ssp.cache.Keys.DataKey).Length);
                Assert.Equal("300", actualConnection.HashGet(ssp.cache.Keys.InternalKey, "SessionTimeout").ToString());

                // reset sessions timoue
                ssp.ResetItemTimeout(null, sessionId);

                // End request
                ssp.EndRequest(null);

                // Verify that GetItemExclusive returns timeout from redis
                bool locked_1;
                TimeSpan lockAge_1;
                object lockId_1;
                SessionStateActions actions_1;
                SessionStateStoreData storeData_1 = ssp.GetItemExclusive(null, sessionId, out locked_1, out lockAge_1, out lockId_1, out actions_1);
                Assert.Equal(5, storeData_1.Timeout);

                // remove data and lock from redis
                DisposeRedisConnectionWrapper();
            }
        }

        [Fact]
        public void ReleaseItemExclusiveWithNullLockId()
        {
            using (RedisServer redisServer = new RedisServer())
            {
                string sessionId = ResetRedisConnectionWrapperAndConfiguration();
                RedisSessionStateProvider ssp = new RedisSessionStateProvider();
                ssp.ReleaseItemExclusive(null, sessionId, null);
                DisposeRedisConnectionWrapper();
            }
        }

        [Fact]
        public void RemoveItemWithNullLockId()
        {
            using (RedisServer redisServer = new RedisServer())
            {
                string sessionId = ResetRedisConnectionWrapperAndConfiguration();
                RedisSessionStateProvider ssp = new RedisSessionStateProvider();
                ssp.RemoveItem(null, sessionId, null, null);
                DisposeRedisConnectionWrapper();
            }
        }

        private IDatabase GetRealRedisConnection()
        {
            return RedisConnectionWrapper.sharedConnection.Connection;
        }
    }
}
