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
    public class RedisConnectionWrapperFunctionalTests
    {
        static int uniqueSessionNumber = 1;
        private static RedisUtility RedisUtility = new RedisUtility(Utility.GetDefaultConfigUtility());

        private RedisConnectionWrapper GetRedisConnectionWrapperWithUniqueSession()
        {
            
            return GetRedisConnectionWrapperWithUniqueSession(Utility.GetDefaultConfigUtility());
        }

        private RedisConnectionWrapper GetRedisConnectionWrapperWithUniqueSession(ProviderConfiguration pc)
        {
            string id = Guid.NewGuid().ToString();
            uniqueSessionNumber++;
            // Initial connection with redis
            RedisConnectionWrapper.sharedConnection = null;
            RedisConnectionWrapper redisConn = new RedisConnectionWrapper(pc, id);
            return redisConn;
        }

        private void DisposeRedisConnectionWrapper(RedisConnectionWrapper redisConn)
        {
            RedisConnectionWrapper.sharedConnection = null;
        }

        [Fact]
        public void Set_ValidData_WithCustomSerializer()
        {
            
            // this also tests host:port config part
            ProviderConfiguration pc = Utility.GetDefaultConfigUtility();
            pc.RedisSerializerType = typeof(TestSerializer).AssemblyQualifiedName;
            pc.ApplicationName = "APPTEST";
            pc.Port = 6379;
            RedisUtility testSerializerRedisUtility = new RedisUtility(pc);

            using (RedisServer redisServer = new RedisServer())
            {
                RedisConnectionWrapper redisConn = GetRedisConnectionWrapperWithUniqueSession(pc);
            
                // Inserting data into redis server
                ChangeTrackingSessionStateItemCollection data = new ChangeTrackingSessionStateItemCollection(testSerializerRedisUtility);
                data["key"] = "value";
                data["key1"] = "value1";
                redisConn.Set(data, 900);

                // Get actual connection and get data blob from redis
                IDatabase actualConnection = GetRealRedisConnection(redisConn);
                HashEntry[] sessionDataFromRedis = actualConnection.HashGetAll(redisConn.Keys.DataKey);

                // Check that data shoud be same as what inserted
                Assert.Equal(2, sessionDataFromRedis.Length);
                ChangeTrackingSessionStateItemCollection dataFromRedis = new ChangeTrackingSessionStateItemCollection(testSerializerRedisUtility);
                foreach (HashEntry entry in sessionDataFromRedis)
                {
                    dataFromRedis[entry.Name] = testSerializerRedisUtility.GetObjectFromBytes(entry.Value).ToString();
                }
                Assert.Equal("value", dataFromRedis["key"]);
                Assert.Equal("value1", dataFromRedis["key1"]);

                // remove data from redis
                actualConnection.KeyDelete(redisConn.Keys.DataKey);
                DisposeRedisConnectionWrapper(redisConn);
            }
        }

        [Fact]
        public void Set_ValidData()
        {
            // this also tests host:port config part
            ProviderConfiguration pc = Utility.GetDefaultConfigUtility();
            pc.ApplicationName = "APPTEST";
            pc.Port = 6379;

            using (RedisServer redisServer = new RedisServer())
            {
                RedisConnectionWrapper redisConn = GetRedisConnectionWrapperWithUniqueSession(pc);

                // Inserting data into redis server
                ChangeTrackingSessionStateItemCollection data = new ChangeTrackingSessionStateItemCollection(new RedisUtility(pc));
                data["key"] = "value";
                data["key1"] = "value1";
                redisConn.Set(data, 900);

                // Get actual connection and get data blob from redis
                IDatabase actualConnection = GetRealRedisConnection(redisConn);
                HashEntry[] sessionDataFromRedis = actualConnection.HashGetAll(redisConn.Keys.DataKey);

                // Check that data shoud be same as what inserted
                Assert.Equal(2, sessionDataFromRedis.Length);
                ChangeTrackingSessionStateItemCollection dataFromRedis = new ChangeTrackingSessionStateItemCollection(new RedisUtility(pc));
                foreach (HashEntry entry in sessionDataFromRedis)
                {
                    dataFromRedis[entry.Name] = RedisUtility.GetObjectFromBytes(entry.Value).ToString();
                }
                Assert.Equal("value", dataFromRedis["key"]);
                Assert.Equal("value1", dataFromRedis["key1"]);

                // remove data from redis
                actualConnection.KeyDelete(redisConn.Keys.DataKey);
                DisposeRedisConnectionWrapper(redisConn);
            }
        }

        [Fact]
        public void Set_NullData()
        {
            // this also tests host:port config part
            ProviderConfiguration pc = Utility.GetDefaultConfigUtility();
            pc.ApplicationName = "APPTEST";
            pc.Port = 6379;

            using (RedisServer redisServer = new RedisServer())
            {
                RedisConnectionWrapper redisConn = GetRedisConnectionWrapperWithUniqueSession(pc);

                // Inserting data into redis server
                ChangeTrackingSessionStateItemCollection data = new ChangeTrackingSessionStateItemCollection(new RedisUtility(pc));
                data["key"] = "value";
                data["key1"] = null;
                redisConn.Set(data, 900);

                // Get actual connection and get data blob from redis
                IDatabase actualConnection = GetRealRedisConnection(redisConn);
                HashEntry[] sessionDataFromRedis = actualConnection.HashGetAll(redisConn.Keys.DataKey);

                // Check that data shoud be same as what inserted
                Assert.Equal(2, sessionDataFromRedis.Length);
                ChangeTrackingSessionStateItemCollection dataFromRedis = new ChangeTrackingSessionStateItemCollection(new RedisUtility(pc));
                foreach (HashEntry entry in sessionDataFromRedis)
                {
                    dataFromRedis[entry.Name] = RedisUtility.GetObjectFromBytes(entry.Value);
                }
                Assert.Equal("value", dataFromRedis["key"]);
                Assert.Equal(null, dataFromRedis["key1"]);

                // remove data from redis
                actualConnection.KeyDelete(redisConn.Keys.DataKey);
                DisposeRedisConnectionWrapper(redisConn);
            }
        }

        [Fact]
        public void Set_ExpireData()
        {
            ProviderConfiguration pc = Utility.GetDefaultConfigUtility();
            using (RedisServer redisServer = new RedisServer())
            {
                RedisConnectionWrapper redisConn = GetRedisConnectionWrapperWithUniqueSession();
                // Inserting data into redis server that expires after 1 second
                ChangeTrackingSessionStateItemCollection data = new ChangeTrackingSessionStateItemCollection(new RedisUtility(pc));
                data["key"] = "value";
                redisConn.Set(data, 1);

                // Wait for 2 seconds so that data will expire
                System.Threading.Thread.Sleep(1100);

                // Get actual connection and get data blob from redis
                IDatabase actualConnection = GetRealRedisConnection(redisConn);
                HashEntry[] sessionDataFromRedis = actualConnection.HashGetAll(redisConn.Keys.DataKey);

                // Check that data shoud not be there
                Assert.Equal(0, sessionDataFromRedis.Length);
                DisposeRedisConnectionWrapper(redisConn);
            }
        }

        [Fact]
        public void TryTakeWriteLockAndGetData_WithNullData()
        {
            ProviderConfiguration pc = Utility.GetDefaultConfigUtility();
            using (RedisServer redisServer = new RedisServer())
            {
                RedisConnectionWrapper redisConn = GetRedisConnectionWrapperWithUniqueSession();

                // Inserting data into redis server
                ChangeTrackingSessionStateItemCollection data = new ChangeTrackingSessionStateItemCollection(new RedisUtility(pc));
                data["key"] = null;
                redisConn.Set(data, 900);

                DateTime lockTime = DateTime.Now;
                int lockTimeout = 900;
                object lockId;
                ISessionStateItemCollection dataFromRedis;
                int sessionTimeout;
                Assert.True(redisConn.TryTakeWriteLockAndGetData(lockTime, lockTimeout, out lockId, out dataFromRedis, out sessionTimeout));
                Assert.Equal(lockTime.Ticks.ToString(), lockId.ToString());
                Assert.Equal(1, dataFromRedis.Count);
                Assert.Null(dataFromRedis["key"]);

                // Get actual connection and get data lock from redis
                IDatabase actualConnection = GetRealRedisConnection(redisConn);
                string lockValueFromRedis = actualConnection.StringGet(redisConn.Keys.LockKey);
                Assert.Equal(lockTime.Ticks.ToString(), lockValueFromRedis);

                // remove data and lock from redis
                actualConnection.KeyDelete(redisConn.Keys.DataKey);
                actualConnection.KeyDelete(redisConn.Keys.LockKey);
                DisposeRedisConnectionWrapper(redisConn);
            }
        }

        [Fact]
        public void TryTakeWriteLockAndGetData_WriteLockWithoutAnyOtherLock()
        {
            ProviderConfiguration pc = Utility.GetDefaultConfigUtility();
            using (RedisServer redisServer = new RedisServer())
            {
                RedisConnectionWrapper redisConn = GetRedisConnectionWrapperWithUniqueSession();

                // Inserting data into redis server
                ChangeTrackingSessionStateItemCollection data = new ChangeTrackingSessionStateItemCollection(new RedisUtility(pc));
                data["key"] = "value";
                redisConn.Set(data, 900);

                DateTime lockTime = DateTime.Now;
                int lockTimeout = 900;
                object lockId;
                ISessionStateItemCollection dataFromRedis;
                int sessionTimeout;
                Assert.True(redisConn.TryTakeWriteLockAndGetData(lockTime, lockTimeout, out lockId, out dataFromRedis, out sessionTimeout));
                Assert.Equal(lockTime.Ticks.ToString(), lockId.ToString());

                ChangeTrackingSessionStateItemCollection dataFromGet = (ChangeTrackingSessionStateItemCollection)dataFromRedis;
                Assert.Null(((ValueWrapper)dataFromGet.innerCollection["key"]).GetActualValue());
                Assert.NotNull(((ValueWrapper)dataFromGet.innerCollection["key"]).GetSerializedvalue());
                Assert.Equal(1, dataFromRedis.Count);

                // this will desirialize value
                Assert.Equal("value", dataFromRedis["key"]);
                Assert.Equal("value", ((ValueWrapper)dataFromGet.innerCollection["key"]).GetActualValue());

                // Get actual connection and get data lock from redis
                IDatabase actualConnection = GetRealRedisConnection(redisConn);
                string lockValueFromRedis = actualConnection.StringGet(redisConn.Keys.LockKey);
                Assert.Equal(lockTime.Ticks.ToString(), lockValueFromRedis);

                // remove data and lock from redis
                actualConnection.KeyDelete(redisConn.Keys.DataKey);
                actualConnection.KeyDelete(redisConn.Keys.LockKey);
                DisposeRedisConnectionWrapper(redisConn);
            }
        }

        [Fact]
        public void TryTakeWriteLockAndGetData_WriteLockWithOtherWriteLock()
        {
            ProviderConfiguration pc = Utility.GetDefaultConfigUtility();
            using (RedisServer redisServer = new RedisServer())
            {
                RedisConnectionWrapper redisConn = GetRedisConnectionWrapperWithUniqueSession();

                // Inserting data into redis server
                ChangeTrackingSessionStateItemCollection data = new ChangeTrackingSessionStateItemCollection(new RedisUtility(pc));
                data["key"] = "value";
                redisConn.Set(data, 900);

                int lockTimeout = 900;
                
                // Takewrite lock successfully first time
                DateTime lockTime_1 = DateTime.Now;
                object lockId_1;
                ISessionStateItemCollection dataFromRedis_1;
                int sessionTimeout;
                Assert.True(redisConn.TryTakeWriteLockAndGetData(lockTime_1, lockTimeout, out lockId_1, out dataFromRedis_1, out sessionTimeout));
                Assert.Equal(lockTime_1.Ticks.ToString(), lockId_1.ToString());
                Assert.Equal(1, dataFromRedis_1.Count);

                // try to take write lock and fail and get earlier lock id
                DateTime lockTime_2 = lockTime_1.AddSeconds(1);
                object lockId_2;
                ISessionStateItemCollection dataFromRedis_2;
                Assert.False(redisConn.TryTakeWriteLockAndGetData(lockTime_2, lockTimeout, out lockId_2, out dataFromRedis_2, out sessionTimeout));
                Assert.Equal(lockTime_1.Ticks.ToString(), lockId_2.ToString());
                Assert.Equal(null, dataFromRedis_2);

                // Get actual connection
                IDatabase actualConnection = GetRealRedisConnection(redisConn);
                // remove data and lock from redis
                actualConnection.KeyDelete(redisConn.Keys.DataKey);
                actualConnection.KeyDelete(redisConn.Keys.LockKey);
                DisposeRedisConnectionWrapper(redisConn);
            }
        }

        [Fact]
        public void TryTakeWriteLockAndGetData_WriteLockWithOtherWriteLockWithSameLockId()
        {
            ProviderConfiguration pc = Utility.GetDefaultConfigUtility();
            using (RedisServer redisServer = new RedisServer())
            {
                RedisConnectionWrapper redisConn = GetRedisConnectionWrapperWithUniqueSession();

                // Inserting data into redis server
                ChangeTrackingSessionStateItemCollection data = new ChangeTrackingSessionStateItemCollection(new RedisUtility(pc));
                data["key"] = "value";
                redisConn.Set(data, 900);

                int lockTimeout = 900;
                // Same LockId 
                DateTime lockTime = DateTime.Now;

                // Takewrite lock successfully first time
                object lockId_1;
                ISessionStateItemCollection dataFromRedis_1;
                int sessionTimeout;
                Assert.True(redisConn.TryTakeWriteLockAndGetData(lockTime, lockTimeout, out lockId_1, out dataFromRedis_1, out sessionTimeout));
                Assert.Equal(lockTime.Ticks.ToString(), lockId_1.ToString());
                Assert.Equal(1, dataFromRedis_1.Count);

                // try to take write lock and fail and get earlier lock id
                object lockId_2;
                ISessionStateItemCollection dataFromRedis_2;
                Assert.False(redisConn.TryTakeWriteLockAndGetData(lockTime, lockTimeout, out lockId_2, out dataFromRedis_2, out sessionTimeout));
                Assert.Equal(lockTime.Ticks.ToString(), lockId_2.ToString());
                Assert.Equal(null, dataFromRedis_2);

                // Get actual connection
                IDatabase actualConnection = GetRealRedisConnection(redisConn);
                // remove data and lock from redis
                actualConnection.KeyDelete(redisConn.Keys.DataKey);
                actualConnection.KeyDelete(redisConn.Keys.LockKey);
                DisposeRedisConnectionWrapper(redisConn);
            }
        }

        [Fact]
        public void TryTakeReadLockAndGetData_WithoutAnyLock()
        {
            ProviderConfiguration pc = Utility.GetDefaultConfigUtility();
            using (RedisServer redisServer = new RedisServer())
            {
                RedisConnectionWrapper redisConn = GetRedisConnectionWrapperWithUniqueSession();

                // Inserting data into redis server
                ChangeTrackingSessionStateItemCollection data = new ChangeTrackingSessionStateItemCollection(new RedisUtility(pc));
                data["key"] = "value";
                redisConn.Set(data, 900);

                object lockId;
                ISessionStateItemCollection dataFromRedis;
                int sessionTimeout;
                Assert.True(redisConn.TryCheckWriteLockAndGetData(out lockId, out dataFromRedis, out sessionTimeout));
                Assert.Equal(null, lockId);
                Assert.Equal(1, dataFromRedis.Count);
                Assert.Equal("value", dataFromRedis["key"]);

                // Get actual connection
                // remove data from redis
                IDatabase actualConnection = GetRealRedisConnection(redisConn);
                actualConnection.KeyDelete(redisConn.Keys.DataKey);
                DisposeRedisConnectionWrapper(redisConn);
            }
        }

        [Fact]
        public void TryTakeReadLockAndGetData_WithOtherWriteLock()
        {
            ProviderConfiguration pc = Utility.GetDefaultConfigUtility();
            using (RedisServer redisServer = new RedisServer())
            {
                RedisConnectionWrapper redisConn = GetRedisConnectionWrapperWithUniqueSession();

                // Inserting data into redis server
                ChangeTrackingSessionStateItemCollection data = new ChangeTrackingSessionStateItemCollection(new RedisUtility(pc));
                data["key"] = "value";
                redisConn.Set(data, 900);

                int lockTimeout = 900;
                
                DateTime lockTime_1 = DateTime.Now;
                object lockId_1;
                ISessionStateItemCollection dataFromRedis_1;
                int sessionTimeout;
                Assert.True(redisConn.TryTakeWriteLockAndGetData(lockTime_1, lockTimeout, out lockId_1, out dataFromRedis_1, out sessionTimeout));
                Assert.Equal(lockTime_1.Ticks.ToString(), lockId_1.ToString());
                Assert.Equal(1, dataFromRedis_1.Count);

                object lockId_2;
                ISessionStateItemCollection dataFromRedis_2;
                Assert.False(redisConn.TryCheckWriteLockAndGetData(out lockId_2, out dataFromRedis_2, out sessionTimeout));
                Assert.Equal(lockTime_1.Ticks.ToString(), lockId_2.ToString());
                Assert.Equal(null, dataFromRedis_2);

                // Get actual connection
                // remove data and lock from redis
                IDatabase actualConnection = GetRealRedisConnection(redisConn);
                actualConnection.KeyDelete(redisConn.Keys.DataKey);
                actualConnection.KeyDelete(redisConn.Keys.LockKey);
                DisposeRedisConnectionWrapper(redisConn);
            }
        }

        [Fact]
        public void TryTakeWriteLockAndGetData_ExpireWriteLock()
        {
            ProviderConfiguration pc = Utility.GetDefaultConfigUtility();
            using (RedisServer redisServer = new RedisServer())
            {
                RedisConnectionWrapper redisConn = GetRedisConnectionWrapperWithUniqueSession();

                // Inserting data into redis server
                ChangeTrackingSessionStateItemCollection data = new ChangeTrackingSessionStateItemCollection(new RedisUtility(pc));
                data["key"] = "value";
                redisConn.Set(data, 900);

                int lockTimeout = 1;
                
                DateTime lockTime = DateTime.Now;
                object lockId;
                ISessionStateItemCollection dataFromRedis;
                int sessionTimeout;
                Assert.True(redisConn.TryTakeWriteLockAndGetData(lockTime, lockTimeout, out lockId, out dataFromRedis, out sessionTimeout));
                Assert.Equal(lockTime.Ticks.ToString(), lockId.ToString());
                Assert.Equal(1, dataFromRedis.Count);

                // Wait for 2 seconds so that lock will expire
                System.Threading.Thread.Sleep(1100);

                // Get actual connection and check that lock do not exists
                IDatabase actualConnection = GetRealRedisConnection(redisConn);
                string lockValueFromRedis = actualConnection.StringGet(redisConn.Keys.LockKey);
                Assert.Equal(null, lockValueFromRedis);

                // remove data from redis
                actualConnection.KeyDelete(redisConn.Keys.DataKey);
                DisposeRedisConnectionWrapper(redisConn);
            }
        }

        [Fact]
        public void TryReleaseLockIfLockIdMatch_ValidWriteLockRelease()
        {
            ProviderConfiguration pc = Utility.GetDefaultConfigUtility();
            using (RedisServer redisServer = new RedisServer())
            {
                RedisConnectionWrapper redisConn = GetRedisConnectionWrapperWithUniqueSession();
            
                // Inserting data into redis server
                ChangeTrackingSessionStateItemCollection data = new ChangeTrackingSessionStateItemCollection(new RedisUtility(pc));
                data["key"] = "value";
                redisConn.Set(data, 900);

                int lockTimeout = 900;
                
                DateTime lockTime = DateTime.Now;
                object lockId;
                ISessionStateItemCollection dataFromRedis;
                int sessionTimeout;
                Assert.True(redisConn.TryTakeWriteLockAndGetData(lockTime, lockTimeout, out lockId, out dataFromRedis, out sessionTimeout));
                Assert.Equal(lockTime.Ticks.ToString(), lockId.ToString());
                Assert.Equal(1, dataFromRedis.Count);

                redisConn.TryReleaseLockIfLockIdMatch(lockId, 900);

                // Get actual connection and check that lock do not exists
                IDatabase actualConnection = GetRealRedisConnection(redisConn);
                string lockValueFromRedis = actualConnection.StringGet(redisConn.Keys.LockKey);
                Assert.Equal(null, lockValueFromRedis);

                // remove data from redis
                actualConnection.KeyDelete(redisConn.Keys.DataKey);
                DisposeRedisConnectionWrapper(redisConn);
            }
        }

        [Fact]
        public void TryReleaseLockIfLockIdMatch_InvalidWriteLockRelease()
        {
            ProviderConfiguration pc = Utility.GetDefaultConfigUtility();
            using (RedisServer redisServer = new RedisServer())
            {
                RedisConnectionWrapper redisConn = GetRedisConnectionWrapperWithUniqueSession();
            
                // Inserting data into redis server
                ChangeTrackingSessionStateItemCollection data = new ChangeTrackingSessionStateItemCollection(new RedisUtility(pc));
                data["key"] = "value";
                redisConn.Set(data, 900);

                int lockTimeout = 900;
                
                DateTime lockTime = DateTime.Now;
                object lockId;
                ISessionStateItemCollection dataFromRedis;
                int sessionTimeout;
                Assert.True(redisConn.TryTakeWriteLockAndGetData(lockTime, lockTimeout, out lockId, out dataFromRedis, out sessionTimeout));
                Assert.Equal(lockTime.Ticks.ToString(), lockId.ToString());
                Assert.Equal(1, dataFromRedis.Count);

                object wrongLockId = lockTime.AddSeconds(1).Ticks.ToString();
                redisConn.TryReleaseLockIfLockIdMatch(wrongLockId, 900);

                // Get actual connection and check that lock do not exists
                IDatabase actualConnection = GetRealRedisConnection(redisConn);
                string lockValueFromRedis = actualConnection.StringGet(redisConn.Keys.LockKey);
                Assert.Equal(lockId, lockValueFromRedis);

                // remove data from redis
                actualConnection.KeyDelete(redisConn.Keys.DataKey);
                actualConnection.KeyDelete(redisConn.Keys.LockKey);
                DisposeRedisConnectionWrapper(redisConn);
            }
        }

        [Fact]
        public void TryRemoveIfLockIdMatch_ValidLockIdAndRemove()
        {
            ProviderConfiguration pc = Utility.GetDefaultConfigUtility();
            using (RedisServer redisServer = new RedisServer())
            {
                RedisConnectionWrapper redisConn = GetRedisConnectionWrapperWithUniqueSession();
            
                // Inserting data into redis server
                ChangeTrackingSessionStateItemCollection data = new ChangeTrackingSessionStateItemCollection(new RedisUtility(pc));
                data["key"] = "value";
                redisConn.Set(data, 900);

                int lockTimeout = 900;
                DateTime lockTime = DateTime.Now;
                object lockId;
                ISessionStateItemCollection dataFromRedis;
                int sessionTimeout;
                Assert.True(redisConn.TryTakeWriteLockAndGetData(lockTime, lockTimeout, out lockId, out dataFromRedis, out sessionTimeout));
                Assert.Equal(lockTime.Ticks.ToString(), lockId.ToString());
                Assert.Equal(1, dataFromRedis.Count);

                redisConn.TryRemoveAndReleaseLockIfLockIdMatch(lockId);

                // Get actual connection and get data from redis
                IDatabase actualConnection = GetRealRedisConnection(redisConn);
                Assert.False(actualConnection.KeyExists(redisConn.Keys.DataKey));
                
                // check lock removed from redis
                Assert.False(actualConnection.KeyExists(redisConn.Keys.LockKey));
                DisposeRedisConnectionWrapper(redisConn);
            }
        }

        [Fact]
        public void TryUpdateIfLockIdMatch_WithValidUpdateAndDelete()
        {
            ProviderConfiguration pc = Utility.GetDefaultConfigUtility();
            using (RedisServer redisServer = new RedisServer())
            {
                RedisConnectionWrapper redisConn = GetRedisConnectionWrapperWithUniqueSession();
            
                // Inserting data into redis server
                ChangeTrackingSessionStateItemCollection data = new ChangeTrackingSessionStateItemCollection(new RedisUtility(pc));
                data["key1"] = "value1";
                data["key2"] = "value2";
                data["key3"] = "value3";
                redisConn.Set(data, 900);

                int lockTimeout = 900;
                DateTime lockTime = DateTime.Now;
                object lockId;
                ISessionStateItemCollection dataFromRedis;
                int sessionTimeout;
                Assert.True(redisConn.TryTakeWriteLockAndGetData(lockTime, lockTimeout, out lockId, out dataFromRedis, out sessionTimeout));
                Assert.Equal(lockTime.Ticks.ToString(), lockId.ToString());
                Assert.Equal(3, dataFromRedis.Count);
                Assert.Equal("value1", dataFromRedis["key1"]);
                Assert.Equal("value2", dataFromRedis["key2"]);
                Assert.Equal("value3", dataFromRedis["key3"]);

                dataFromRedis["key2"] = "value2-updated";
                dataFromRedis.Remove("key3");
                redisConn.TryUpdateAndReleaseLockIfLockIdMatch(lockId, dataFromRedis, 900);

                // Get actual connection and get data from redis
                IDatabase actualConnection = GetRealRedisConnection(redisConn);
                HashEntry[] sessionDataFromRedis = actualConnection.HashGetAll(redisConn.Keys.DataKey);
                Assert.Equal(2, sessionDataFromRedis.Length);
                ChangeTrackingSessionStateItemCollection sessionDataFromRedisAsCollection = new ChangeTrackingSessionStateItemCollection(new RedisUtility(pc));
                foreach (HashEntry entry in sessionDataFromRedis)
                {
                    sessionDataFromRedisAsCollection[entry.Name] = RedisUtility.GetObjectFromBytes(entry.Value).ToString();
                }
                Assert.Equal("value1", sessionDataFromRedisAsCollection["key1"]);
                Assert.Equal("value2-updated", sessionDataFromRedisAsCollection["key2"]);

                // check lock removed and remove data from redis
                actualConnection.KeyDelete(redisConn.Keys.DataKey);
                Assert.False(actualConnection.KeyExists(redisConn.Keys.LockKey));
                DisposeRedisConnectionWrapper(redisConn);
            }
        }

        [Fact]
        public void TryUpdateIfLockIdMatch_WithOnlyUpdateAndNoDelete()
        {
            ProviderConfiguration pc = Utility.GetDefaultConfigUtility();
            using (RedisServer redisServer = new RedisServer())
            {
                RedisConnectionWrapper redisConn = GetRedisConnectionWrapperWithUniqueSession();
            
                // Inserting data into redis server
                ChangeTrackingSessionStateItemCollection data = new ChangeTrackingSessionStateItemCollection(new RedisUtility(pc));
                data["key1"] = "value1";
                data["key2"] = "value2";
                data["key3"] = "value3";
                redisConn.Set(data, 900);

                int lockTimeout = 900;
                DateTime lockTime = DateTime.Now;
                object lockId;
                ISessionStateItemCollection dataFromRedis;
                int sessionTimeout;
                Assert.True(redisConn.TryTakeWriteLockAndGetData(lockTime, lockTimeout, out lockId, out dataFromRedis, out sessionTimeout));
                Assert.Equal(lockTime.Ticks.ToString(), lockId.ToString());
                Assert.Equal(3, dataFromRedis.Count);
                Assert.Equal("value1", dataFromRedis["key1"]);
                Assert.Equal("value2", dataFromRedis["key2"]);
                Assert.Equal("value3", dataFromRedis["key3"]);

                dataFromRedis["key2"] = "value2-updated";
                redisConn.TryUpdateAndReleaseLockIfLockIdMatch(lockId, dataFromRedis, 900);

                // Get actual connection and get data from redis
                IDatabase actualConnection = GetRealRedisConnection(redisConn);
                HashEntry[] sessionDataFromRedis = actualConnection.HashGetAll(redisConn.Keys.DataKey);
                Assert.Equal(3, sessionDataFromRedis.Length);
                ChangeTrackingSessionStateItemCollection sessionDataFromRedisAsCollection = new ChangeTrackingSessionStateItemCollection(new RedisUtility(pc));
                foreach (HashEntry entry in sessionDataFromRedis)
                {
                    sessionDataFromRedisAsCollection[entry.Name] = RedisUtility.GetObjectFromBytes(entry.Value).ToString();
                }
                Assert.Equal("value1", sessionDataFromRedisAsCollection["key1"]);
                Assert.Equal("value2-updated", sessionDataFromRedisAsCollection["key2"]);
                Assert.Equal("value3", sessionDataFromRedisAsCollection["key3"]);

                // check lock removed and remove data from redis
                actualConnection.KeyDelete(redisConn.Keys.DataKey);
                Assert.False(actualConnection.KeyExists(redisConn.Keys.LockKey));
                DisposeRedisConnectionWrapper(redisConn);
            }
        }

        [Fact]
        public void TryUpdateIfLockIdMatch_WithNoUpdateAndOnlyDelete()
        {
            ProviderConfiguration pc = Utility.GetDefaultConfigUtility();
            using (RedisServer redisServer = new RedisServer())
            {
                RedisConnectionWrapper redisConn = GetRedisConnectionWrapperWithUniqueSession();
            
                // Inserting data into redis server
                ChangeTrackingSessionStateItemCollection data = new ChangeTrackingSessionStateItemCollection(new RedisUtility(pc));
                data["key1"] = "value1";
                data["key2"] = "value2";
                data["key3"] = "value3";
                redisConn.Set(data, 900);

                int lockTimeout = 900;
                DateTime lockTime = DateTime.Now;
                object lockId;
                ISessionStateItemCollection dataFromRedis;
                int sessionTimeout;
                Assert.True(redisConn.TryTakeWriteLockAndGetData(lockTime, lockTimeout, out lockId, out dataFromRedis, out sessionTimeout));
                Assert.Equal(lockTime.Ticks.ToString(), lockId.ToString());
                Assert.Equal(3, dataFromRedis.Count);
                Assert.Equal("value1", dataFromRedis["key1"]);
                Assert.Equal("value2", dataFromRedis["key2"]);
                Assert.Equal("value3", dataFromRedis["key3"]);

                dataFromRedis.Remove("key3");
                redisConn.TryUpdateAndReleaseLockIfLockIdMatch(lockId, dataFromRedis, 900);

                // Get actual connection and get data from redis
                IDatabase actualConnection = GetRealRedisConnection(redisConn);
                HashEntry[] sessionDataFromRedis = actualConnection.HashGetAll(redisConn.Keys.DataKey);
                Assert.Equal(2, sessionDataFromRedis.Length);
                ChangeTrackingSessionStateItemCollection sessionDataFromRedisAsCollection = new ChangeTrackingSessionStateItemCollection(new RedisUtility(pc));
                foreach (HashEntry entry in sessionDataFromRedis)
                {
                    sessionDataFromRedisAsCollection[entry.Name] = RedisUtility.GetObjectFromBytes(entry.Value).ToString();
                }
                Assert.Equal("value1", sessionDataFromRedisAsCollection["key1"]);
                Assert.Equal("value2", sessionDataFromRedisAsCollection["key2"]);

                // check lock removed and remove data from redis
                actualConnection.KeyDelete(redisConn.Keys.DataKey);
                Assert.False(actualConnection.KeyExists(redisConn.Keys.LockKey));
                DisposeRedisConnectionWrapper(redisConn);
            }
        }

        [Fact]
        public void TryUpdateIfLockIdMatch_ExpiryTime_OnValidData()
        {
            ProviderConfiguration pc = Utility.GetDefaultConfigUtility();
            using (RedisServer redisServer = new RedisServer())
            {
                RedisConnectionWrapper redisConn = GetRedisConnectionWrapperWithUniqueSession();
            
                // Inserting data into redis server
                ChangeTrackingSessionStateItemCollection data = new ChangeTrackingSessionStateItemCollection(new RedisUtility(pc));
                data["key"] = "value";
                data["key1"] = "value1";
                redisConn.Set(data, 900);

                // Check that data shoud exists
                int lockTimeout = 90;
                DateTime lockTime = DateTime.Now;
                object lockId;
                ISessionStateItemCollection dataFromRedis;
                int sessionTimeout;
                redisConn.TryTakeWriteLockAndGetData(lockTime, lockTimeout, out lockId, out dataFromRedis, out sessionTimeout);
                Assert.Equal(2, dataFromRedis.Count);

                // Update expiry time to only 1 sec and than verify that.
                redisConn.TryUpdateAndReleaseLockIfLockIdMatch(lockId, dataFromRedis, 1);
                
                // Wait for 1.1 seconds so that data will expire
                System.Threading.Thread.Sleep(1100);

                // Get data blob from redis
                IDatabase actualConnection = GetRealRedisConnection(redisConn);
                HashEntry[] sessionDataFromRedisAfterExpire = actualConnection.HashGetAll(redisConn.Keys.DataKey);

                // Check that data shoud not be there
                Assert.Equal(0, sessionDataFromRedisAfterExpire.Length);
                DisposeRedisConnectionWrapper(redisConn);
            }
        }

        [Fact]
        public void TryUpdateAndReleaseLockIfLockIdMatch_LargeLockTime_ExpireManuallyTest()
        {
            ProviderConfiguration pc = Utility.GetDefaultConfigUtility();
            using (RedisServer redisServer = new RedisServer())
            {
                RedisConnectionWrapper redisConn = GetRedisConnectionWrapperWithUniqueSession();

                // Inserting data into redis server
                ChangeTrackingSessionStateItemCollection data = new ChangeTrackingSessionStateItemCollection(new RedisUtility(pc));
                data["key1"] = "value1";
                redisConn.Set(data, 900);

                int lockTimeout = 120000;
                DateTime lockTime = DateTime.Now;
                object lockId;
                ISessionStateItemCollection dataFromRedis;
                int sessionTimeout;
                Assert.True(redisConn.TryTakeWriteLockAndGetData(lockTime, lockTimeout, out lockId, out dataFromRedis, out sessionTimeout));
                redisConn.TryUpdateAndReleaseLockIfLockIdMatch(lockId, dataFromRedis, 900);

                // Get actual connection and check that lock is released
                IDatabase actualConnection = GetRealRedisConnection(redisConn);
                Assert.False(actualConnection.KeyExists(redisConn.Keys.LockKey));
                actualConnection.KeyDelete(redisConn.Keys.DataKey); 
                DisposeRedisConnectionWrapper(redisConn);
            }
        }

        private IDatabase GetRealRedisConnection(RedisConnectionWrapper redisConn)
        {
            return (IDatabase)((StackExchangeClientConnection)redisConn.redisConnection).RealConnection;
        }
    }
}
