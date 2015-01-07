using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.SessionState;
using Xunit;

namespace Microsoft.Web.Redis.Tests
{
    public class RedisUtilityTests
    {
        [Fact]
        public void AppendRemoveItemsInList_EmptySessionItems()
        {
            ChangeTrackingSessionStateItemCollection sessionItems = new ChangeTrackingSessionStateItemCollection();
            Assert.Equal(0, RedisUtility.AppendRemoveItemsInList(sessionItems, null));
        }

        [Fact]
        public void AppendRemoveItemsInList_NothingDeleted()
        {
            ChangeTrackingSessionStateItemCollection sessionItems = new ChangeTrackingSessionStateItemCollection();
            sessionItems["key"] = "val";
            Assert.Equal(0, RedisUtility.AppendRemoveItemsInList(sessionItems, null));
        }

        [Fact]
        public void AppendRemoveItemsInList_SuccessfulDeleted()
        {
            ChangeTrackingSessionStateItemCollection sessionItems = new ChangeTrackingSessionStateItemCollection();
            List<object> list = new List<object>();
            sessionItems["key"] = "val";
            sessionItems.Remove("key");
            Assert.Equal(1, RedisUtility.AppendRemoveItemsInList(sessionItems, list));
            Assert.Equal(1, list.Count);
        }

        [Fact]
        public void AppendUpdatedOrNewItemsInList_EmptySessionItems()
        {
            ChangeTrackingSessionStateItemCollection sessionItems = new ChangeTrackingSessionStateItemCollection();
            Assert.Equal(0, RedisUtility.AppendUpdatedOrNewItemsInList(sessionItems, null));
        }

        [Fact]
        public void AppendUpdatedOrNewItemsInList_NothingUpdated()
        {
            ChangeTrackingSessionStateItemCollection sessionItems = new ChangeTrackingSessionStateItemCollection();
            sessionItems["key"] = "val";
            sessionItems.Remove("key");
            Assert.Equal(0, RedisUtility.AppendUpdatedOrNewItemsInList(sessionItems, null));
        }

        [Fact]
        public void AppendUpdatedOrNewItemsInList_SuccessfulUpdated()
        {
            ChangeTrackingSessionStateItemCollection sessionItems = new ChangeTrackingSessionStateItemCollection();
            List<object> list = new List<object>();
            sessionItems["key"] = "val";
            Assert.Equal(1, RedisUtility.AppendUpdatedOrNewItemsInList(sessionItems, list));
            Assert.Equal(2, list.Count);
        }

        [Fact]
        public void GetNewItemsAsList_EmptySessionData()
        {
            ChangeTrackingSessionStateItemCollection sessionItems = new ChangeTrackingSessionStateItemCollection();
            List<object> list = RedisUtility.GetNewItemsAsList(sessionItems);
            Assert.Equal(0, list.Count);
        }

        [Fact]
        public void GetNewItemsAsList_WithSessionData()
        {
            ChangeTrackingSessionStateItemCollection sessionItems = new ChangeTrackingSessionStateItemCollection();
            sessionItems["key"] = "val";
            sessionItems["key1"] = "val1";
            List<object> list = RedisUtility.GetNewItemsAsList(sessionItems);
            Assert.Equal(4, list.Count);
        }

        [Fact]
        public void GetNewItemsAsList_WithNullSessionData()
        {
            ChangeTrackingSessionStateItemCollection sessionItems = new ChangeTrackingSessionStateItemCollection();
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
    }
}
