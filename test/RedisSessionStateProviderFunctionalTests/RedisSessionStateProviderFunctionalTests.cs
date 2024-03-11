//
// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.
//

using Microsoft.Web.Redis.Tests;
using StackExchange.Redis;
using System;
using System.Threading.Tasks;
using Xunit;
using System.Threading;
using Microsoft.AspNet.SessionState;

namespace Microsoft.Web.Redis.FunctionalTests
{
    public class RedisSessionStateProviderFunctionalTests
    {
        private string ResetRedisConnectionWrapperAndConfiguration()
        {
            RedisSessionStateConnectionWrapper.sharedConnection = null;
            RedisSessionStateProvider.configuration = Utility.GetDefaultConfigUtility();
            return Guid.NewGuid().ToString();
        }

        private void DisposeRedisConnectionWrapper()
        {
            RedisSessionStateConnectionWrapper.sharedConnection = null;
        }

        [Fact()]
        public async Task SessionWriteCycle_Valid()
        {
            using (RedisServer redisServer = new RedisServer())
            {
                string sessionId = ResetRedisConnectionWrapperAndConfiguration();

                // Inserting empty session with "SessionStateActions.InitializeItem" flag into redis server
                RedisSessionStateProvider ssp = new RedisSessionStateProvider();
                await ssp.CreateUninitializedItemAsync(null, sessionId, (int)RedisSessionStateProvider.configuration.SessionTimeout.TotalMinutes, CancellationToken.None);

                // Get write lock and session from cache
                GetItemResult data = await ssp.GetItemExclusiveAsync(null, sessionId, CancellationToken.None);

                // Get actual connection and varify lock and session timeout
                IDatabase actualConnection = GetRealRedisConnection();
                Assert.Equal(data.LockId.ToString(), actualConnection.StringGet(ssp.cache.Keys.LockKey).ToString());
                Assert.Equal(((int)RedisSessionStateProvider.configuration.SessionTimeout.TotalSeconds).ToString(), actualConnection.StringGet(ssp.cache.Keys.InternalKey).ToString());

                // setting data as done by any normal session operation
                data.Item.Items["key"] = "value";

                // session update
                await ssp.SetAndReleaseItemExclusiveAsync(null, sessionId, data.Item, data.LockId, false, CancellationToken.None);
                Assert.NotNull(actualConnection.StringGet(ssp.cache.Keys.DataKey));

                // reset sessions timoue
                await ssp.ResetItemTimeoutAsync(null, sessionId, CancellationToken.None);

                // End request
                await ssp.EndRequestAsync(null);

                // remove data and lock from redis
                DisposeRedisConnectionWrapper();
            }
        }

        [Fact()]
        public async Task SessionReadCycle_Valid()
        {
            using (RedisServer redisServer = new RedisServer())
            {
                string sessionId = ResetRedisConnectionWrapperAndConfiguration();

                // Inserting empty session with "SessionStateActions.InitializeItem" flag into redis server
                RedisSessionStateProvider ssp = new RedisSessionStateProvider();
                await ssp.CreateUninitializedItemAsync(null, sessionId, (int)RedisSessionStateProvider.configuration.SessionTimeout.TotalMinutes, CancellationToken.None);

                // Get write lock and session from cache
                GetItemResult data = await ssp.GetItemAsync(null, sessionId, CancellationToken.None);

                // Get actual connection and varify lock and session timeout
                IDatabase actualConnection = GetRealRedisConnection();
                Assert.True(actualConnection.StringGet(ssp.cache.Keys.LockKey).IsNull);
                Assert.Equal(((int)RedisSessionStateProvider.configuration.SessionTimeout.TotalSeconds).ToString(), actualConnection.StringGet(ssp.cache.Keys.InternalKey).ToString());

                // reset sessions timoue
                await ssp.ResetItemTimeoutAsync(null, sessionId, CancellationToken.None);

                // End request
                await ssp.EndRequestAsync(null);

                // remove data and lock from redis
                DisposeRedisConnectionWrapper();
            }
        }

        [Fact()]
        public async Task SessionTimoutChangeFromGlobalAspx()
        {
            using (RedisServer redisServer = new RedisServer())
            {
                string sessionId = ResetRedisConnectionWrapperAndConfiguration();

                // Inserting empty session with "SessionStateActions.InitializeItem" flag into redis server
                RedisSessionStateProvider ssp = new RedisSessionStateProvider();
                await ssp.CreateUninitializedItemAsync(null, sessionId, (int)RedisSessionStateProvider.configuration.SessionTimeout.TotalMinutes, CancellationToken.None);

                // Get write lock and session from cache
                GetItemResult data = await ssp.GetItemExclusiveAsync(null, sessionId, CancellationToken.None);

                // Get actual connection and varify lock and session timeout
                IDatabase actualConnection = GetRealRedisConnection();
                Assert.Equal(data.LockId.ToString(), actualConnection.StringGet(ssp.cache.Keys.LockKey).ToString());
                Assert.Equal(((int)RedisSessionStateProvider.configuration.SessionTimeout.TotalSeconds).ToString(), actualConnection.StringGet(ssp.cache.Keys.InternalKey).ToString());

                // setting data as done by any normal session operation
                data.Item.Items["key"] = "value";
                data.Item.Timeout = 5;

                // session update
                await ssp.SetAndReleaseItemExclusiveAsync(null, sessionId, data.Item, data.LockId, false, CancellationToken.None);
                Assert.Equal("300", actualConnection.StringGet(ssp.cache.Keys.InternalKey).ToString());

                // reset sessions timoue
                await ssp.ResetItemTimeoutAsync(null, sessionId, CancellationToken.None);

                // End request
                await ssp.EndRequestAsync(null);

                // Verify that GetItemExclusive returns timeout from redis
                GetItemResult data_1 = await ssp.GetItemExclusiveAsync(null, sessionId, CancellationToken.None);
                Assert.Equal(5, data.Item.Timeout);

                // remove data and lock from redis
                DisposeRedisConnectionWrapper();
            }
        }

        [Fact()]
        public async Task ReleaseItemExclusiveWithNullLockId()
        {
            using (RedisServer redisServer = new RedisServer())
            {
                string sessionId = ResetRedisConnectionWrapperAndConfiguration();
                RedisSessionStateProvider ssp = new RedisSessionStateProvider();
                await ssp.ReleaseItemExclusiveAsync(null, sessionId, null, CancellationToken.None);
                DisposeRedisConnectionWrapper();
            }
        }

        [Fact()]
        public async Task RemoveItemWithNullLockId()
        {
            using (RedisServer redisServer = new RedisServer())
            {
                string sessionId = ResetRedisConnectionWrapperAndConfiguration();
                RedisSessionStateProvider ssp = new RedisSessionStateProvider();
                await ssp.RemoveItemAsync(null, sessionId, null, null, CancellationToken.None);
                DisposeRedisConnectionWrapper();
            }
        }

        private IDatabase GetRealRedisConnection()
        {
            return RedisSessionStateConnectionWrapper.sharedConnection.Connection;
        }

        [Fact(Skip = "Only used to evaluate performance")]
        public async Task TestThroughputAsync()
        {
            // Test to compare efficiency between code changes; reads and writes 10000 items to Redis
            using (RedisServer redisServer = new RedisServer())
            {
                string sessionId = ResetRedisConnectionWrapperAndConfiguration();

                // Inserting empty session with "SessionStateActions.InitializeItem" flag into redis server
                RedisSessionStateProvider ssp = new RedisSessionStateProvider();
                await ssp.CreateUninitializedItemAsync(null, sessionId, (int)RedisSessionStateProvider.configuration.SessionTimeout.TotalMinutes, CancellationToken.None);

                // Get write lock and session from cache
                GetItemResult data = await ssp.GetItemExclusiveAsync(null, sessionId, CancellationToken.None);

                // Get actual connection and varify lock and session timeout
                IDatabase actualConnection = GetRealRedisConnection();
                Assert.Equal(data.LockId.ToString(), actualConnection.StringGet(ssp.cache.Keys.LockKey).ToString());
                Assert.Equal(((int)RedisSessionStateProvider.configuration.SessionTimeout.TotalSeconds).ToString(), actualConnection.HashGet(ssp.cache.Keys.InternalKey, "SessionTimeout").ToString());

                var watch = new System.Diagnostics.Stopwatch();

                watch.Start();

                for (int i = 0; i < 10000; i++)
                {
                    data.Item.Items["key" + i.ToString()] = "value" + i.ToString();

                    // session update
                    await ssp.SetAndReleaseItemExclusiveAsync(null, sessionId, data.Item, data.LockId, false, CancellationToken.None);
                }

                for (int i = 0; i < 10000; i++)
                {
                    var result = data.Item.Items["key" + i.ToString()];
                    Assert.Equal("value" + i.ToString(), result);
                }

                watch.Stop();

                Console.WriteLine($"Execution Time: {watch.ElapsedMilliseconds} ms");

                // reset sessions timoue
                await ssp.ResetItemTimeoutAsync(null, sessionId, CancellationToken.None);

                // End request
                await ssp.EndRequestAsync(null);

                // remove data and lock from redis
                DisposeRedisConnectionWrapper();
            }
        }
    }
}