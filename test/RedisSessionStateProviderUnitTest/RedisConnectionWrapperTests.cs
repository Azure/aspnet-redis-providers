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
            RedisConnectionWrapper.sharedConnection = A.Fake<RedisSharedConnection>();
            RedisConnectionWrapper redisConn = new RedisConnectionWrapper(Utility.GetDefaultConfigUtility(), sessionId);
            redisConn.redisConnection = A.Fake<IRedisClientConnection>();
            redisConn.UpdateExpiryTime(90);
            A.CallTo(() => redisConn.redisConnection.Eval(A<string>.Ignored, A<string[]>.That.Matches(s => s.Length == 2),
                A<object[]>.That.Matches(o => o.Length == 1))).MustHaveHappened();
        }

        [Fact]
        public void GetLockAge_ValidTicks()
        {
            RedisConnectionWrapper.sharedConnection = A.Fake<RedisSharedConnection>();
            RedisConnectionWrapper redisConn = new RedisConnectionWrapper(Utility.GetDefaultConfigUtility(), "");
            redisConn.redisConnection = A.Fake<IRedisClientConnection>();
            Assert.NotNull(redisConn.GetLockAge(DateTime.Now.Ticks));
        }

        [Fact]
        public void GetLockAge_InValidTicks()
        {
            RedisConnectionWrapper.sharedConnection = A.Fake<RedisSharedConnection>();
            RedisConnectionWrapper redisConn = new RedisConnectionWrapper(Utility.GetDefaultConfigUtility(), "");
            redisConn.redisConnection = A.Fake<IRedisClientConnection>();
            Assert.NotEqual(0, redisConn.GetLockAge("Invalid-tics").TotalHours);
        }

        [Fact]
        public void Set_NullData()
        {
            RedisConnectionWrapper.sharedConnection = A.Fake<RedisSharedConnection>();
            string sessionId = "session_id";
            RedisConnectionWrapper redisConn = new RedisConnectionWrapper(Utility.GetDefaultConfigUtility(), sessionId);
            redisConn.redisConnection = A.Fake<IRedisClientConnection>();
            redisConn.Set(null, 90);
            A.CallTo(() => redisConn.redisConnection.Eval(A<string>.Ignored, A<string[]>.Ignored, A<object[]>.Ignored)).MustNotHaveHappened();
        }

        [Fact]
        public void Set_ValidData()
        {
            string sessionId = "session_id";
            RedisConnectionWrapper.sharedConnection = A.Fake<RedisSharedConnection>();
            RedisConnectionWrapper.redisUtility = new RedisUtility(Utility.GetDefaultConfigUtility()); 
            RedisConnectionWrapper redisConn = new RedisConnectionWrapper(Utility.GetDefaultConfigUtility(), sessionId);
            redisConn.redisConnection = A.Fake<IRedisClientConnection>();
            ChangeTrackingSessionStateItemCollection data = new ChangeTrackingSessionStateItemCollection(RedisConnectionWrapper.redisUtility);
            data["key"] = "value";
            redisConn.Set(data, 90);
            A.CallTo(() => redisConn.redisConnection.Eval(A<string>.Ignored, A<string[]>.That.Matches(s => s.Length == 2), 
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

            RedisConnectionWrapper.sharedConnection = A.Fake<RedisSharedConnection>();
            RedisConnectionWrapper redisConn = new RedisConnectionWrapper(Utility.GetDefaultConfigUtility(), id);
            redisConn.redisConnection = A.Fake<IRedisClientConnection>();

            A.CallTo(() => redisConn.redisConnection.Eval(A<string>.Ignored, A<string[]>.That.Matches(s => s.Length == 3),
                 A<object[]>.That.Matches(o => o.Length == 2))).Returns(returnFromRedis);
            A.CallTo(() => redisConn.redisConnection.GetLockId(A<object>.Ignored)).Returns("Diff-lock-id");
            A.CallTo(() => redisConn.redisConnection.IsLocked(A<object>.Ignored)).Returns(true);
            A.CallTo(() => redisConn.redisConnection.GetSessionTimeout(A<object>.Ignored)).Returns(15);

            int sessionTimeout;
            Assert.False(redisConn.TryTakeWriteLockAndGetData(lockTime, lockTimeout, out lockId, out data, out sessionTimeout));
            Assert.Equal("Diff-lock-id", lockId);
            Assert.Null(data);
            Assert.Equal(15, sessionTimeout);
            A.CallTo(() => redisConn.redisConnection.Eval(A<string>.Ignored, A<string[]>.That.Matches(s => s.Length == 3),
                A<object[]>.That.Matches(o => o.Length == 2))).MustHaveHappened();
            A.CallTo(() => redisConn.redisConnection.GetLockId(A<object>.Ignored)).MustHaveHappened();
            A.CallTo(() => redisConn.redisConnection.IsLocked(A<object>.Ignored)).MustHaveHappened();
            A.CallTo(() => redisConn.redisConnection.GetSessionData(A<object>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => redisConn.redisConnection.GetSessionTimeout(A<object>.Ignored)).MustHaveHappened();
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

            RedisConnectionWrapper.sharedConnection = A.Fake<RedisSharedConnection>();
            RedisConnectionWrapper redisConn = new RedisConnectionWrapper(Utility.GetDefaultConfigUtility(), id);
            redisConn.redisConnection = A.Fake<IRedisClientConnection>();

            A.CallTo(() => redisConn.redisConnection.Eval(A<string>.Ignored, A<string[]>.That.Matches(s => s.Length == 3),
                 A<object[]>.That.Matches(o => o.Length == 2))).Returns(returnFromRedis);
            A.CallTo(() => redisConn.redisConnection.GetLockId(A<object>.Ignored)).Returns(lockTime.Ticks.ToString());
            A.CallTo(() => redisConn.redisConnection.IsLocked(A<object>.Ignored)).Returns(true);
            A.CallTo(() => redisConn.redisConnection.GetSessionTimeout(A<object>.Ignored)).Returns(15);

            
            int sessionTimeout;
            Assert.False(redisConn.TryTakeWriteLockAndGetData(lockTime, lockTimeout, out lockId, out data, out sessionTimeout));
            Assert.Equal(lockTime.Ticks.ToString(), lockId);
            Assert.Null(data);
            Assert.Equal(15, sessionTimeout);
            A.CallTo(() => redisConn.redisConnection.Eval(A<string>.Ignored, A<string[]>.That.Matches(s => s.Length == 3),
                A<object[]>.That.Matches(o => o.Length == 2))).MustHaveHappened();
            A.CallTo(() => redisConn.redisConnection.GetLockId(A<object>.Ignored)).MustHaveHappened();
            A.CallTo(() => redisConn.redisConnection.IsLocked(A<object>.Ignored)).MustHaveHappened();
            A.CallTo(() => redisConn.redisConnection.GetSessionData(A<object>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => redisConn.redisConnection.GetSessionTimeout(A<object>.Ignored)).MustHaveHappened();
        }

        [Fact]
        public void TryTakeWriteLockAndGetData_Valid()
        {
            string id = "session_id";
            DateTime lockTime = DateTime.Now;
            int lockTimeout = 90;
            object lockId;
            ISessionStateItemCollection data;

            RedisConnectionWrapper.sharedConnection = A.Fake<RedisSharedConnection>();
            RedisConnectionWrapper redisConn = new RedisConnectionWrapper(Utility.GetDefaultConfigUtility(), id);
            redisConn.redisConnection = A.Fake<IRedisClientConnection>();

            object[] sessionData = { "Key", RedisUtility.GetBytesFromObject("value") };
            object[] returnFromRedis = { lockTime.Ticks.ToString(), sessionData, "15", false };
            ChangeTrackingSessionStateItemCollection sessionDataReturn = Utility.GetChangeTrackingSessionStateItemCollection();
            sessionDataReturn["key"] = "value";

            A.CallTo(() => redisConn.redisConnection.Eval(A<string>.Ignored, A<string[]>.That.Matches(s => s.Length == 3),
                 A<object[]>.That.Matches(o => o.Length == 2))).Returns(returnFromRedis);
            A.CallTo(() => redisConn.redisConnection.GetLockId(A<object>.Ignored)).Returns(lockTime.Ticks.ToString());
            A.CallTo(() => redisConn.redisConnection.IsLocked(A<object>.Ignored)).Returns(false);
            A.CallTo(() => redisConn.redisConnection.GetSessionData(A<object>.Ignored)).Returns(sessionDataReturn);
            A.CallTo(() => redisConn.redisConnection.GetSessionTimeout(A<object>.Ignored)).Returns(15);

            int sessionTimeout;
            Assert.True(redisConn.TryTakeWriteLockAndGetData(lockTime, lockTimeout, out lockId, out data, out sessionTimeout));
            Assert.Equal(lockTime.Ticks.ToString(), lockId);
            Assert.Equal(1, data.Count);
            Assert.Equal(15, sessionTimeout);
            A.CallTo(() => redisConn.redisConnection.Eval(A<string>.Ignored, A<string[]>.That.Matches(s => s.Length == 3),
                A<object[]>.That.Matches(o => o.Length == 2))).MustHaveHappened();
            A.CallTo(() => redisConn.redisConnection.GetLockId(A<object>.Ignored)).MustHaveHappened();
            A.CallTo(() => redisConn.redisConnection.IsLocked(A<object>.Ignored)).MustHaveHappened();
            A.CallTo(() => redisConn.redisConnection.GetSessionData(A<object>.Ignored)).MustHaveHappened();
            A.CallTo(() => redisConn.redisConnection.GetSessionTimeout(A<object>.Ignored)).MustHaveHappened();
        }

        [Fact]
        public void TryCheckWriteLockAndGetData_Valid()
        {
            string id = "session_id";
            object lockId;
            ISessionStateItemCollection data;

            RedisConnectionWrapper.sharedConnection = A.Fake<RedisSharedConnection>();
            RedisConnectionWrapper redisConn = new RedisConnectionWrapper(Utility.GetDefaultConfigUtility(), id);
            redisConn.redisConnection = A.Fake<IRedisClientConnection>();

            object[] sessionData = { "Key", RedisUtility.GetBytesFromObject("value") };
            object[] returnFromRedis = { "", sessionData, "15" };
            ChangeTrackingSessionStateItemCollection sessionDataReturn = Utility.GetChangeTrackingSessionStateItemCollection();
            sessionDataReturn["key"] = "value";

            A.CallTo(() => redisConn.redisConnection.Eval(A<string>.Ignored, A<string[]>.That.Matches(s => s.Length == 3),
                 A<object[]>.That.Matches(o => o.Length == 0))).Returns(returnFromRedis);
            A.CallTo(() => redisConn.redisConnection.GetLockId(A<object>.Ignored)).Returns("");
            A.CallTo(() => redisConn.redisConnection.GetSessionData(A<object>.Ignored)).Returns(sessionDataReturn);
            A.CallTo(() => redisConn.redisConnection.GetSessionTimeout(A<object>.Ignored)).Returns(15);

            int sessionTimeout;
            Assert.True(redisConn.TryCheckWriteLockAndGetData(out lockId, out data, out sessionTimeout));
            Assert.Equal(null, lockId);
            Assert.Equal(1, data.Count);
            Assert.Equal(15, sessionTimeout);
            A.CallTo(() => redisConn.redisConnection.Eval(A<string>.Ignored, A<string[]>.That.Matches(s => s.Length == 3),
                A<object[]>.That.Matches(o => o.Length == 0))).MustHaveHappened();
            A.CallTo(() => redisConn.redisConnection.GetLockId(A<object>.Ignored)).MustHaveHappened();
            A.CallTo(() => redisConn.redisConnection.GetSessionData(A<object>.Ignored)).MustHaveHappened();
            A.CallTo(() => redisConn.redisConnection.GetSessionTimeout(A<object>.Ignored)).MustHaveHappened();
        }

        [Fact]
        public void TryReleaseLockIfLockIdMatch_WriteLock()
        {
            string id = "session_id";
            object lockId = DateTime.Now.Ticks;

            RedisConnectionWrapper.sharedConnection = A.Fake<RedisSharedConnection>();
            RedisConnectionWrapper redisConn = new RedisConnectionWrapper(Utility.GetDefaultConfigUtility(), id);
            redisConn.redisConnection = A.Fake<IRedisClientConnection>();
            
            redisConn.TryReleaseLockIfLockIdMatch(lockId, 900);
            A.CallTo(() => redisConn.redisConnection.Eval(A<string>.Ignored, A<string[]>.That.Matches(s => s.Length == 3 && s[0].Equals(redisConn.Keys.LockKey)),
                 A<object[]>.That.Matches(o => o.Length == 2))).MustHaveHappened();
        }

        [Fact]
        public void TryRemoveIfLockIdMatch_Valid()
        {
            string id = "session_id";
            object lockId = DateTime.Now.Ticks;

            RedisConnectionWrapper.sharedConnection = A.Fake<RedisSharedConnection>();
            RedisConnectionWrapper redisConn = new RedisConnectionWrapper(Utility.GetDefaultConfigUtility(), id);
            redisConn.redisConnection = A.Fake<IRedisClientConnection>();

            redisConn.TryRemoveAndReleaseLockIfLockIdMatch(lockId);
            A.CallTo(() => redisConn.redisConnection.Eval(A<string>.Ignored, A<string[]>.That.Matches(s => s.Length == 3),
                 A<object[]>.That.Matches(o => o.Length == 1))).MustHaveHappened();
        }

        [Fact]
        public void TryUpdateIfLockIdMatchPrepare_NoUpdateNoDelete()
        {
            string id = "session_id";
            int sessionTimeout = 900;
            object lockId = DateTime.Now.Ticks;
            ChangeTrackingSessionStateItemCollection data = Utility.GetChangeTrackingSessionStateItemCollection();

            RedisConnectionWrapper.redisUtility = new RedisUtility(Utility.GetDefaultConfigUtility());
            RedisConnectionWrapper.sharedConnection = A.Fake<RedisSharedConnection>();
            RedisConnectionWrapper redisConn = new RedisConnectionWrapper(Utility.GetDefaultConfigUtility(), id);
            redisConn.redisConnection = A.Fake<IRedisClientConnection>();

            redisConn.TryUpdateAndReleaseLockIfLockIdMatch(lockId, data, sessionTimeout);

            A.CallTo(() => redisConn.redisConnection.Eval(A<string>.Ignored, A<string[]>.That.Matches(s => s.Length == 3), A<object[]>.That.Matches(
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

            RedisConnectionWrapper.redisUtility = new RedisUtility(Utility.GetDefaultConfigUtility());
            RedisConnectionWrapper.sharedConnection = A.Fake<RedisSharedConnection>();
            RedisConnectionWrapper redisConn = new RedisConnectionWrapper(Utility.GetDefaultConfigUtility(), id);
            redisConn.redisConnection = A.Fake<IRedisClientConnection>();

            redisConn.TryUpdateAndReleaseLockIfLockIdMatch(lockId, data, sessionTimeout);

            A.CallTo(() => redisConn.redisConnection.Eval(A<string>.Ignored, A<string[]>.That.Matches(s => s.Length == 3), A<object[]>.That.Matches(
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
