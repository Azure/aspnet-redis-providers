//
// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.
//

using FakeItEasy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.SessionState;
using Xunit;

namespace Microsoft.Web.Redis.Tests
{
    public class RedisSharedConnectionTests
    {
        [Fact]
        public void TryGetConnection_CreateNewConnection()
        {
            Utility.SetConfigUtilityToDefault();
            RedisSharedConnection redisSharedConnection = new RedisSharedConnection(RedisSessionStateProvider.configuration,
                () => new FakeRedisClientConnection());
            Assert.Null(redisSharedConnection.connection);
            IRedisClientConnection connection = redisSharedConnection.TryGetConnection();
            Assert.NotNull(connection);
            Assert.NotNull(redisSharedConnection.connection);
        }

        [Fact]
        public void TryGetConnection_ConnectionSharing()
        {
            Utility.SetConfigUtilityToDefault();
            RedisSharedConnection redisSharedConnection = new RedisSharedConnection(RedisSessionStateProvider.configuration,
                () => new FakeRedisClientConnection());
            IRedisClientConnection connection = redisSharedConnection.TryGetConnection();
            IRedisClientConnection connection2 = redisSharedConnection.TryGetConnection();
            Assert.Equal(connection, connection2);
        }

    }

    class FakeRedisClientConnection : IRedisClientConnection
    {
        public FakeRedisClientConnection()
        { }

        public void Open()
        { }
        public void Close()
        { }

        public bool Expiry(string key, int timeInSeconds)
        {
            return false;
        }

        public object Eval(string script, string[] keyArgs, object[] valueArgs)
        {
            return null;
        }

        public string GetLockId(object rowDataFromRedis)
        {
            return null;
        }

        public bool IsLocked(object rowDataFromRedis)
        {
            return false;
        }

        public int GetSessionTimeout(object rowDataFromRedis)
        {
            return 1200;
        }

        public ISessionStateItemCollection GetSessionData(object rowDataFromRedis)
        {
            return null;
        }

        public void Set(string key, byte[] data, DateTime utcExpiry)
        { }

        public byte[] Get(string key)
        {
            return null;
        }

        public void Remove(string key)
        { }

        public byte[] GetOutputCacheDataFromResult(object rowDataFromRedis)
        {
            return null;
        }
    }
}
