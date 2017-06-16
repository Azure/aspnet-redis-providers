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
    public class RedisConnectionWrapperTests
    {
        private static RedisUtility RedisUtility = new RedisUtility(Utility.GetDefaultConfigUtility());

        [Fact]
        public void UpdateExpiryTime_Valid()
        {
            string sessionId = "session_id";
            var mockRedisClient = A.Fake<IRedisClientConnection>();
            RedisConnectionWrapper.sharedConnection = new RedisSharedConnection(null, null);
            RedisConnectionWrapper.sharedConnection.connection = mockRedisClient;
            RedisConnectionWrapper redisConn = new RedisConnectionWrapper(Utility.GetDefaultConfigUtility(), sessionId);
            redisConn.UpdateExpiryTime(90);
            A.CallTo(() => mockRedisClient.Eval(A<string>.Ignored, A<string[]>.That.Matches(s => s.Length == 2),
                A<object[]>.That.Matches(o => o.Length == 1))).MustHaveHappened();
        }

        [Fact]
        public void GetLockAge_ValidTicks()
        {
            var mockRedisClient = A.Fake<IRedisClientConnection>();
            RedisConnectionWrapper.sharedConnection = new RedisSharedConnection(null, null);
            RedisConnectionWrapper.sharedConnection.connection = mockRedisClient;

            RedisConnectionWrapper redisConn = new RedisConnectionWrapper(Utility.GetDefaultConfigUtility(), "");
            Assert.NotNull(redisConn.GetLockAge(DateTime.Now.Ticks));
        }

        [Fact]
        public void GetLockAge_InValidTicks()
        {
            var mockRedisClient = A.Fake<IRedisClientConnection>();
            RedisConnectionWrapper.sharedConnection = new RedisSharedConnection(null, null);
            RedisConnectionWrapper.sharedConnection.connection = mockRedisClient;

            RedisConnectionWrapper redisConn = new RedisConnectionWrapper(Utility.GetDefaultConfigUtility(), "");
            Assert.NotEqual(0, redisConn.GetLockAge("Invalid-tics").TotalHours);
        }

        [Fact]
        public void Set_NullData()
        {
            var mockRedisClient = A.Fake<IRedisClientConnection>();
            RedisConnectionWrapper.sharedConnection = new RedisSharedConnection(null, null);
            RedisConnectionWrapper.sharedConnection.connection = mockRedisClient;
            
            string sessionId = "session_id";
            RedisConnectionWrapper redisConn = new RedisConnectionWrapper(Utility.GetDefaultConfigUtility(), sessionId);
            redisConn.Set(null, 90);
            A.CallTo(() => mockRedisClient.Eval(A<string>.Ignored, A<string[]>.Ignored, A<object[]>.Ignored)).MustNotHaveHappened();
        }

        [Fact]
        public void Set_ValidData()
        {
            string sessionId = "session_id";
            var mockRedisClient = A.Fake<IRedisClientConnection>();
            RedisConnectionWrapper.redisUtility = new RedisUtility(Utility.GetDefaultConfigUtility()); 
            RedisConnectionWrapper.sharedConnection = new RedisSharedConnection(null, null);
            RedisConnectionWrapper.sharedConnection.connection = mockRedisClient;
            RedisConnectionWrapper redisConn = new RedisConnectionWrapper(Utility.GetDefaultConfigUtility(), sessionId);
            ChangeTrackingSessionStateItemCollection data = new ChangeTrackingSessionStateItemCollection(RedisConnectionWrapper.redisUtility);
            data["key"] = "value";
            redisConn.Set(data, 90);
            A.CallTo(() => mockRedisClient.Eval(A<string>.Ignored, A<string[]>.That.Matches(s => s.Length == 2), 
                A<object[]>.That.Matches(o => o.Length == 4))).MustHaveHappened();
        }

        [Fact]
        public void TryTakeWriteLockAndGetData_UnableToLock()
        {
            string id = "session_id";
            DateTime lockTime = DateTime.Now;
            int lockTimeout = 90;
            object lockId;
            ISessionStateItemCollection data;

            object[] returnFromRedis = { "Diff-lock-id", "", "15", true };

            var mockRedisClient = A.Fake<IRedisClientConnection>();
            A.CallTo(() => mockRedisClient.Eval(A<string>.Ignored, A<string[]>.That.Matches(s => s.Length == 3),
                 A<object[]>.That.Matches(o => o.Length == 2))).Returns(returnFromRedis);
            A.CallTo(() => mockRedisClient.GetLockId(A<object>.Ignored)).Returns("Diff-lock-id");
            A.CallTo(() => mockRedisClient.IsLocked(A<object>.Ignored)).Returns(true);
            A.CallTo(() => mockRedisClient.GetSessionTimeout(A<object>.Ignored)).Returns(15);

            RedisConnectionWrapper.sharedConnection = new RedisSharedConnection(null, null);
            RedisConnectionWrapper.sharedConnection.connection = mockRedisClient;
            RedisConnectionWrapper redisConn = new RedisConnectionWrapper(Utility.GetDefaultConfigUtility(), id);

            int sessionTimeout;
            Assert.False(redisConn.TryTakeWriteLockAndGetData(lockTime, lockTimeout, out lockId, out data, out sessionTimeout));
            Assert.Equal("Diff-lock-id", lockId);
            Assert.Null(data);
            Assert.Equal(15, sessionTimeout);
            A.CallTo(() => mockRedisClient.Eval(A<string>.Ignored, A<string[]>.That.Matches(s => s.Length == 3),
                A<object[]>.That.Matches(o => o.Length == 2))).MustHaveHappened();
            A.CallTo(() => mockRedisClient.GetLockId(A<object>.Ignored)).MustHaveHappened();
            A.CallTo(() => mockRedisClient.IsLocked(A<object>.Ignored)).MustHaveHappened();
            A.CallTo(() => mockRedisClient.GetSessionData(A<object>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => mockRedisClient.GetSessionTimeout(A<object>.Ignored)).MustHaveHappened();
        }

        [Fact]
        public void TryTakeWriteLockAndGetData_UnableToLockWithSameLockId()
        {
            string id = "session_id";
            DateTime lockTime = DateTime.Now;
            int lockTimeout = 90;
            object lockId;
            ISessionStateItemCollection data;

            object[] returnFromRedis = { lockTime.Ticks.ToString(), "", "15", true };

            var mockRedisClient = A.Fake<IRedisClientConnection>();
            A.CallTo(() => mockRedisClient.Eval(A<string>.Ignored, A<string[]>.That.Matches(s => s.Length == 3),
                 A<object[]>.That.Matches(o => o.Length == 2))).Returns(returnFromRedis);
            A.CallTo(() => mockRedisClient.GetLockId(A<object>.Ignored)).Returns(lockTime.Ticks.ToString());
            A.CallTo(() => mockRedisClient.IsLocked(A<object>.Ignored)).Returns(true);
            A.CallTo(() => mockRedisClient.GetSessionTimeout(A<object>.Ignored)).Returns(15);

            RedisConnectionWrapper.sharedConnection = new RedisSharedConnection(null, null);
            RedisConnectionWrapper.sharedConnection.connection = mockRedisClient;
            RedisConnectionWrapper redisConn = new RedisConnectionWrapper(Utility.GetDefaultConfigUtility(), id);

            int sessionTimeout;
            Assert.False(redisConn.TryTakeWriteLockAndGetData(lockTime, lockTimeout, out lockId, out data, out sessionTimeout));
            Assert.Equal(lockTime.Ticks.ToString(), lockId);
            Assert.Null(data);
            Assert.Equal(15, sessionTimeout);
            A.CallTo(() => mockRedisClient.Eval(A<string>.Ignored, A<string[]>.That.Matches(s => s.Length == 3),
                A<object[]>.That.Matches(o => o.Length == 2))).MustHaveHappened();
            A.CallTo(() => mockRedisClient.GetLockId(A<object>.Ignored)).MustHaveHappened();
            A.CallTo(() => mockRedisClient.IsLocked(A<object>.Ignored)).MustHaveHappened();
            A.CallTo(() => mockRedisClient.GetSessionData(A<object>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => mockRedisClient.GetSessionTimeout(A<object>.Ignored)).MustHaveHappened();
        }

        [Fact]
        public void TryTakeWriteLockAndGetData_Valid()
        {
            string id = "session_id";
            DateTime lockTime = DateTime.Now;
            int lockTimeout = 90;
            object lockId;
            ISessionStateItemCollection data;

            object[] sessionData = { "Key", RedisUtility.GetBytesFromObject("value") };
            object[] returnFromRedis = { lockTime.Ticks.ToString(), sessionData, "15", false };
            ChangeTrackingSessionStateItemCollection sessionDataReturn = Utility.GetChangeTrackingSessionStateItemCollection();
            sessionDataReturn["key"] = "value";

            var mockRedisClient = A.Fake<IRedisClientConnection>();
            A.CallTo(() => mockRedisClient.Eval(A<string>.Ignored, A<string[]>.That.Matches(s => s.Length == 3),
                 A<object[]>.That.Matches(o => o.Length == 2))).Returns(returnFromRedis);
            A.CallTo(() => mockRedisClient.GetLockId(A<object>.Ignored)).Returns(lockTime.Ticks.ToString());
            A.CallTo(() => mockRedisClient.IsLocked(A<object>.Ignored)).Returns(false);
            A.CallTo(() => mockRedisClient.GetSessionData(A<object>.Ignored)).Returns(sessionDataReturn);
            A.CallTo(() => mockRedisClient.GetSessionTimeout(A<object>.Ignored)).Returns(15);

            RedisConnectionWrapper.sharedConnection = new RedisSharedConnection(null, null);
            RedisConnectionWrapper.sharedConnection.connection = mockRedisClient;
            RedisConnectionWrapper redisConn = new RedisConnectionWrapper(Utility.GetDefaultConfigUtility(), id);

            int sessionTimeout;
            Assert.True(redisConn.TryTakeWriteLockAndGetData(lockTime, lockTimeout, out lockId, out data, out sessionTimeout));
            Assert.Equal(lockTime.Ticks.ToString(), lockId);
            Assert.Equal(1, data.Count);
            Assert.Equal(15, sessionTimeout);
            A.CallTo(() => mockRedisClient.Eval(A<string>.Ignored, A<string[]>.That.Matches(s => s.Length == 3),
                A<object[]>.That.Matches(o => o.Length == 2))).MustHaveHappened();
            A.CallTo(() => mockRedisClient.GetLockId(A<object>.Ignored)).MustHaveHappened();
            A.CallTo(() => mockRedisClient.IsLocked(A<object>.Ignored)).MustHaveHappened();
            A.CallTo(() => mockRedisClient.GetSessionData(A<object>.Ignored)).MustHaveHappened();
            A.CallTo(() => mockRedisClient.GetSessionTimeout(A<object>.Ignored)).MustHaveHappened();
        }

        [Fact]
        public void TryCheckWriteLockAndGetData_Valid()
        {
            string id = "session_id";
            object lockId;
            ISessionStateItemCollection data;

            object[] sessionData = { "Key", RedisUtility.GetBytesFromObject("value") };
            object[] returnFromRedis = { "", sessionData, "15" };
            ChangeTrackingSessionStateItemCollection sessionDataReturn = Utility.GetChangeTrackingSessionStateItemCollection();
            sessionDataReturn["key"] = "value";

            var mockRedisClient = A.Fake<IRedisClientConnection>();
            A.CallTo(() => mockRedisClient.Eval(A<string>.Ignored, A<string[]>.That.Matches(s => s.Length == 3),
                 A<object[]>.That.Matches(o => o.Length == 0))).Returns(returnFromRedis);
            A.CallTo(() => mockRedisClient.GetLockId(A<object>.Ignored)).Returns("");
            A.CallTo(() => mockRedisClient.GetSessionData(A<object>.Ignored)).Returns(sessionDataReturn);
            A.CallTo(() => mockRedisClient.GetSessionTimeout(A<object>.Ignored)).Returns(15);

            RedisConnectionWrapper.sharedConnection = new RedisSharedConnection(null, null);
            RedisConnectionWrapper.sharedConnection.connection = mockRedisClient;
            RedisConnectionWrapper redisConn = new RedisConnectionWrapper(Utility.GetDefaultConfigUtility(), id);

            int sessionTimeout;
            Assert.True(redisConn.TryCheckWriteLockAndGetData(out lockId, out data, out sessionTimeout));
            Assert.Equal(null, lockId);
            Assert.Equal(1, data.Count);
            Assert.Equal(15, sessionTimeout);
            A.CallTo(() => mockRedisClient.Eval(A<string>.Ignored, A<string[]>.That.Matches(s => s.Length == 3),
                A<object[]>.That.Matches(o => o.Length == 0))).MustHaveHappened();
            A.CallTo(() => mockRedisClient.GetLockId(A<object>.Ignored)).MustHaveHappened();
            A.CallTo(() => mockRedisClient.GetSessionData(A<object>.Ignored)).MustHaveHappened();
            A.CallTo(() => mockRedisClient.GetSessionTimeout(A<object>.Ignored)).MustHaveHappened();
        }

        [Fact]
        public void TryReleaseLockIfLockIdMatch_WriteLock()
        {
            string id = "session_id";
            object lockId = DateTime.Now.Ticks;
            
            var mockRedisClient = A.Fake<IRedisClientConnection>();
            RedisConnectionWrapper.sharedConnection = new RedisSharedConnection(null, null);
            RedisConnectionWrapper.sharedConnection.connection = mockRedisClient;
            RedisConnectionWrapper redisConn = new RedisConnectionWrapper(Utility.GetDefaultConfigUtility(), id);
            
            redisConn.TryReleaseLockIfLockIdMatch(lockId, 900);
            A.CallTo(() => mockRedisClient.Eval(A<string>.Ignored, A<string[]>.That.Matches(s => s.Length == 3 && s[0].Equals(redisConn.Keys.LockKey)),
                 A<object[]>.That.Matches(o => o.Length == 2))).MustHaveHappened();
        }

        [Fact]
        public void TryRemoveIfLockIdMatch_Valid()
        {
            string id = "session_id";
            object lockId = DateTime.Now.Ticks;

            var mockRedisClient = A.Fake<IRedisClientConnection>();
            RedisConnectionWrapper.sharedConnection = new RedisSharedConnection(null, null);
            RedisConnectionWrapper.sharedConnection.connection = mockRedisClient;
            RedisConnectionWrapper redisConn = new RedisConnectionWrapper(Utility.GetDefaultConfigUtility(), id);
            
            redisConn.TryRemoveAndReleaseLockIfLockIdMatch(lockId);
            A.CallTo(() => mockRedisClient.Eval(A<string>.Ignored, A<string[]>.That.Matches(s => s.Length == 3),
                 A<object[]>.That.Matches(o => o.Length == 1))).MustHaveHappened();
        }

        [Fact]
        public void TryUpdateIfLockIdMatchPrepare_NoUpdateNoDelete()
        {
            string id = "session_id";
            int sessionTimeout = 900;
            object lockId = DateTime.Now.Ticks;
            ChangeTrackingSessionStateItemCollection data = Utility.GetChangeTrackingSessionStateItemCollection();
            
            var mockRedisClient = A.Fake<IRedisClientConnection>();
            RedisConnectionWrapper.redisUtility = new RedisUtility(Utility.GetDefaultConfigUtility());
            RedisConnectionWrapper.sharedConnection = new RedisSharedConnection(null, null);
            RedisConnectionWrapper.sharedConnection.connection = mockRedisClient;
            RedisConnectionWrapper redisConn = new RedisConnectionWrapper(Utility.GetDefaultConfigUtility(), id);
            redisConn.TryUpdateAndReleaseLockIfLockIdMatch(lockId, data, sessionTimeout);

            A.CallTo(() => mockRedisClient.Eval(A<string>.Ignored, A<string[]>.That.Matches(s => s.Length == 3), A<object[]>.That.Matches(
               o => o.Length == 8 &&
                    o[2].Equals(0) &&
                    o[3].Equals(9) &&
                    o[4].Equals(8) &&
                    o[5].Equals(0) &&
                    o[6].Equals(9) &&
                    o[7].Equals(8)
                ))).MustHaveHappened();
        }

        [Fact]
        public void TryUpdateIfLockIdMatchPrepare_Valid_OneUpdateOneDelete()
        {
            string id = "session_id";
            int sessionTimeout = 900;
            object lockId = DateTime.Now.Ticks;
            ChangeTrackingSessionStateItemCollection data = Utility.GetChangeTrackingSessionStateItemCollection();
            data["KeyDel"] = "valueDel";
            data["Key"] = "value";
            data.Remove("KeyDel");

            
            var mockRedisClient = A.Fake<IRedisClientConnection>();
            RedisConnectionWrapper.redisUtility = new RedisUtility(Utility.GetDefaultConfigUtility());
            RedisConnectionWrapper.sharedConnection = new RedisSharedConnection(null, null);
            RedisConnectionWrapper.sharedConnection.connection = mockRedisClient;
            RedisConnectionWrapper redisConn = new RedisConnectionWrapper(Utility.GetDefaultConfigUtility(), id);
            redisConn.TryUpdateAndReleaseLockIfLockIdMatch(lockId, data, sessionTimeout);

            A.CallTo(() => mockRedisClient.Eval(A<string>.Ignored, A<string[]>.That.Matches(s => s.Length == 3), A<object[]>.That.Matches(
               o => o.Length == 11 &&
                    o[2].Equals(1) &&
                    o[3].Equals(9) &&
                    o[4].Equals(9) &&
                    o[5].Equals(1) &&
                    o[6].Equals(10) &&
                    o[7].Equals(11)
                ))).MustHaveHappened();
        }

    }
}
