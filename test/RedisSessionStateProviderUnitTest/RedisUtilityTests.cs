//
// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.SessionState;
using Xunit;

namespace Microsoft.Web.Redis.Tests
{
    public class RedisUtilityTests
    {
        private static RedisUtility RedisUtility = new RedisUtility(Utility.GetDefaultConfigUtility());

        [Fact]
        public void AppendRemoveItemsInList_EmptySessionItems()
        {
            ChangeTrackingSessionStateItemCollection sessionItems = Utility.GetChangeTrackingSessionStateItemCollection();
            Assert.Equal(0, RedisUtility.AppendRemoveItemsInList(sessionItems, null));
        }

        [Fact]
        public void AppendRemoveItemsInList_NothingDeleted()
        {
            ChangeTrackingSessionStateItemCollection sessionItems = Utility.GetChangeTrackingSessionStateItemCollection();
            sessionItems["key"] = "val";
            Assert.Equal(0, RedisUtility.AppendRemoveItemsInList(sessionItems, null));
        }

        [Fact]
        public void AppendRemoveItemsInList_SuccessfulDeleted()
        {
            ChangeTrackingSessionStateItemCollection sessionItems = Utility.GetChangeTrackingSessionStateItemCollection();
            List<object> list = new List<object>();
            sessionItems["key"] = "val";
            sessionItems.Remove("key");
            Assert.Equal(1, RedisUtility.AppendRemoveItemsInList(sessionItems, list));
            Assert.Equal(1, list.Count);
        }

        [Fact]
        public void AppendUpdatedOrNewItemsInList_EmptySessionItems()
        {
            ChangeTrackingSessionStateItemCollection sessionItems = Utility.GetChangeTrackingSessionStateItemCollection();
            Assert.Equal(0, RedisUtility.AppendUpdatedOrNewItemsInList(sessionItems, null));
        }

        [Fact]
        public void AppendUpdatedOrNewItemsInList_NothingUpdated()
        {
            ChangeTrackingSessionStateItemCollection sessionItems = Utility.GetChangeTrackingSessionStateItemCollection();
            sessionItems["key"] = "val";
            sessionItems.Remove("key");
            Assert.Equal(0, RedisUtility.AppendUpdatedOrNewItemsInList(sessionItems, null));
        }

        [Fact]
        public void AppendUpdatedOrNewItemsInList_SuccessfulUpdated()
        {
            ChangeTrackingSessionStateItemCollection sessionItems = Utility.GetChangeTrackingSessionStateItemCollection();
            List<object> list = new List<object>();
            sessionItems["key"] = "val";
            Assert.Equal(1, RedisUtility.AppendUpdatedOrNewItemsInList(sessionItems, list));
            Assert.Equal(2, list.Count);
        }

        [Fact]
        public void GetNewItemsAsList_EmptySessionData()
        {
            ChangeTrackingSessionStateItemCollection sessionItems = Utility.GetChangeTrackingSessionStateItemCollection();
            List<object> list = RedisUtility.GetNewItemsAsList(sessionItems);
            Assert.Equal(0, list.Count);
        }

        [Fact]
        public void GetNewItemsAsList_WithSessionData()
        {
            ChangeTrackingSessionStateItemCollection sessionItems = Utility.GetChangeTrackingSessionStateItemCollection();
            sessionItems["key"] = "val";
            sessionItems["key1"] = "val1";
            List<object> list = RedisUtility.GetNewItemsAsList(sessionItems);
            Assert.Equal(4, list.Count);
        }

        [Fact]
        public void GetNewItemsAsList_WithNullSessionData()
        {
            ChangeTrackingSessionStateItemCollection sessionItems = Utility.GetChangeTrackingSessionStateItemCollection();
            sessionItems["key"] = "val";
            sessionItems["key1"] = null;
            List<object> list = RedisUtility.GetNewItemsAsList(sessionItems);
            Assert.Equal(4, list.Count);
        }

        [Fact]
        public void GetBytesFromObject_WithNullObject()
        {
            Assert.IsType<Byte[]>(RedisUtility.GetBytesFromObject(null));
        }

        [Fact]
        public void GetBytesFromObject_WithValidObject()
        {
            Object obj = "hi";
            Assert.NotEqual(null, RedisUtility.GetBytesFromObject(obj));
        }

        [Fact]
        public void GetObjectFromBytes_WithNullObject()
        {
            Assert.Equal(null, RedisUtility.GetObjectFromBytes(null));
        }

        [Fact]
        public void GetObjectFromBytes_WithValidObject()
        {
            Object obj = "hi";
            byte[] data = RedisUtility.GetBytesFromObject(obj);

            Assert.Equal(obj.ToString(), RedisUtility.GetObjectFromBytes(data).ToString());
        }

        [Fact]
        public void GetObjectFromBytes_GetBytesFromObject_WithByteArray()
        {
            byte[] data = new byte[1];
            data[0] = 0;

            byte[] serializedData = RedisUtility.GetBytesFromObject(data);
            Assert.NotNull(serializedData);
            
            byte[] deserializedData = (byte[]) RedisUtility.GetObjectFromBytes(serializedData);
            Assert.NotNull(deserializedData);
            Assert.Equal(deserializedData.Length, data.Length);
            Assert.Equal(data[0], deserializedData[0]);
        }

        [Fact]
        public void GetObjectFromBytes_GetBytesFromObject_WithEmptyByteArray()
        {
            byte[] data = new byte[0];
            byte[] serializedData = RedisUtility.GetBytesFromObject(data);
            Assert.NotNull(serializedData);
            byte[] deserializedData = (byte[])RedisUtility.GetObjectFromBytes(serializedData);
            Assert.NotNull(deserializedData);
            Assert.Equal(deserializedData.Length, data.Length);
        }


        [Fact]
        public void CustomSerializer_ByAssemblyQualifiedName()
        {
            var serTypeName = typeof(TestSerializer).AssemblyQualifiedName;
            var utility = new RedisUtility(new ProviderConfiguration() { RedisSerializerType = serTypeName });
            Assert.IsType<TestSerializer>(utility._serializer);
        }

        [Fact]
        public void GetObjectFromBytes_GetBytesFromObject_CustomSerializer()
        {
            var serTypeName = typeof(TestSerializer).AssemblyQualifiedName;
            var utility = new RedisUtility(new ProviderConfiguration() { RedisSerializerType = serTypeName });

            var bytes = utility.GetBytesFromObject("test");
            var obj = utility.GetObjectFromBytes(bytes);
            var testSerializer = (TestSerializer) utility._serializer;
            Assert.Equal("test", obj);
            Assert.Equal(1, testSerializer.DeserializeCount);
            Assert.Equal(1, testSerializer.SerializeCount);
        }

        [Fact]
        public void CustomSerializer_NotExistingType()
        {
            var serTypeName = "This.Type.Does.Not.Exists";
            Assert.Throws<TypeLoadException>(() =>
            {
                new RedisUtility(new ProviderConfiguration() {RedisSerializerType = serTypeName});
            });
        }

        [Fact]
        public void CustomSerializer_ExistingTypeNotImplementingISerializer()
        {
            var serTypeName = this.GetType().AssemblyQualifiedName;
            Assert.Throws<InvalidCastException>(() =>
            {
                new RedisUtility(new ProviderConfiguration() { RedisSerializerType = serTypeName });
            });
        }

    }
}
