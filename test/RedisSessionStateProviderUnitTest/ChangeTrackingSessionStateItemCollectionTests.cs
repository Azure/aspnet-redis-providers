using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Web.Redis.Tests
{
    public class ChangeTrackingSessionStateItemCollectionTests
    {
        [Fact]
        public void SetItem_NewItem()
        {
            ChangeTrackingSessionStateItemCollection items = new ChangeTrackingSessionStateItemCollection();
            items["key"] = "value";
            items[0] = "value2";
            Assert.Equal(1, items.Count);
            Assert.Equal(1, items.GetModifiedKeys().Count);
        }

        [Fact]
        public void Remove_Successful()
        {
            ChangeTrackingSessionStateItemCollection items = new ChangeTrackingSessionStateItemCollection();
            items["key"] = "value";
            Assert.Equal(1, items.Count);
            Assert.Equal(0, items.GetDeletedKeys().Count);
            Assert.Equal(1, items.GetModifiedKeys().Count);
            items.Remove("key");
            Assert.Equal(0, items.Count);
            Assert.Equal(1, items.GetDeletedKeys().Count);
            Assert.Equal(0, items.GetModifiedKeys().Count);
        }

        [Fact]
        public void Remove_WrongKey()
        {
            ChangeTrackingSessionStateItemCollection items = new ChangeTrackingSessionStateItemCollection();
            items["key"] = "value";
            Assert.Equal(1, items.Count);
            Assert.Equal(0, items.GetDeletedKeys().Count);
            Assert.Equal(1, items.GetModifiedKeys().Count);
            items.Remove("key1");
            Assert.Equal(1, items.Count);
            Assert.Equal(0, items.GetDeletedKeys().Count);
            Assert.Equal(1, items.GetModifiedKeys().Count);
        }

        [Fact]
        public void RemoveAt_Successful()
        {
            ChangeTrackingSessionStateItemCollection items = new ChangeTrackingSessionStateItemCollection();
            items["key"] = "value";
            Assert.Equal(1, items.Count);
            Assert.Equal(0, items.GetDeletedKeys().Count);
            Assert.Equal(1, items.GetModifiedKeys().Count);
            items.RemoveAt(0);
            Assert.Equal(0, items.Count);
            Assert.Equal(1, items.GetDeletedKeys().Count);
            Assert.Equal(0, items.GetModifiedKeys().Count);
        }

        [Fact]
        public void RemoveAt_WrongKeyIndex()
        {
            ChangeTrackingSessionStateItemCollection items = new ChangeTrackingSessionStateItemCollection();
            items["key"] = "value";
            Assert.Equal(1, items.Count);
            Assert.Equal(0, items.GetDeletedKeys().Count);
            Assert.Equal(1, items.GetModifiedKeys().Count);
            Assert.Throws<ArgumentOutOfRangeException>(() => items.RemoveAt(1));
        }

        [Fact]
        public void Clear_EmptyCollection()
        {
            ChangeTrackingSessionStateItemCollection items = new ChangeTrackingSessionStateItemCollection();
            Assert.Equal(0, items.Count);
            Assert.Equal(0, items.GetDeletedKeys().Count);
            Assert.Equal(0, items.GetModifiedKeys().Count);
            items.Clear();
            Assert.Equal(0, items.Count);
            Assert.Equal(0, items.GetDeletedKeys().Count);
            Assert.Equal(0, items.GetModifiedKeys().Count);
        }
        
        [Fact]
        public void Clear_Successful()
        {
            ChangeTrackingSessionStateItemCollection items = new ChangeTrackingSessionStateItemCollection();
            items["key1"] = "value1";
            items["key2"] = "value2";
            items["key3"] = "value3";
            Assert.Equal(3, items.Count);
            Assert.Equal(0, items.GetDeletedKeys().Count);
            Assert.Equal(3, items.GetModifiedKeys().Count);
            items.Clear();
            Assert.Equal(0, items.Count);
            Assert.Equal(3, items.GetDeletedKeys().Count);
            Assert.Equal(0, items.GetModifiedKeys().Count);
        }

        [Fact]
        public void Dirty_SetTrue()
        {
            ChangeTrackingSessionStateItemCollection items = new ChangeTrackingSessionStateItemCollection();
            items["key1"] = "value1";
            items["key2"] = "value2";
            items["key3"] = "value3";
            Assert.Equal(3, items.Count);
            Assert.Equal(0, items.GetDeletedKeys().Count);
            Assert.Equal(3, items.GetModifiedKeys().Count);
            items.Dirty = true;
            Assert.Equal(3, items.Count);
            Assert.Equal(0, items.GetDeletedKeys().Count);
            Assert.Equal(3, items.GetModifiedKeys().Count);
        }

        [Fact]
        public void Dirty_SetFalse()
        {
            ChangeTrackingSessionStateItemCollection items = new ChangeTrackingSessionStateItemCollection();
            items["key1"] = "value1";
            items["key2"] = "value2";
            items["key3"] = "value3";
            Assert.Equal(3, items.Count);
            Assert.Equal(0, items.GetDeletedKeys().Count);
            Assert.Equal(3, items.GetModifiedKeys().Count);
            items.Dirty = false;
            Assert.Equal(3, items.Count);
            Assert.Equal(0, items.GetDeletedKeys().Count);
            Assert.Equal(0, items.GetModifiedKeys().Count);
        }

        [Fact]
        public void InsertRemoveUpdate_Sequence()
        {
            // Initial insert to set up value
            ChangeTrackingSessionStateItemCollection items = new ChangeTrackingSessionStateItemCollection();
            items["key1"] = "value1";
            Assert.Equal("value1", items[0]);
            Assert.Equal(true, items.Dirty);
            items.Dirty = false;

            // remove key
            items.Remove("key1");
            Assert.Equal(0, items.Count);
            Assert.Equal(1, items.GetDeletedKeys().Count);
            Assert.Equal(0, items.GetModifiedKeys().Count);
            
            // in same transaction insert same key than it should be update
            items["key1"] = "value1";
            Assert.Equal(1, items.Count);
            Assert.Equal(0, items.GetDeletedKeys().Count);
            Assert.Equal(1, items.GetModifiedKeys().Count);
        }

        [Fact]
        public void MutableObject_FetchMarksDirty()
        {
            // Initial insert to set up value
            ChangeTrackingSessionStateItemCollection items = new ChangeTrackingSessionStateItemCollection();
            items["key1"] = new StringBuilder("value1");
            items.Dirty = false;
            Assert.Equal(1, items.Count);
            Assert.Equal(false, items.Dirty);
            Assert.Equal(0, items.GetModifiedKeys().Count);
            Assert.Equal(0, items.GetDeletedKeys().Count);
            
            // update value 
            StringBuilder sb = (StringBuilder)items["key1"];
            Assert.Equal(1, items.Count);
            Assert.Equal(1, items.GetModifiedKeys().Count);
        }

        [Fact]
        public void ImmutableObject_FetchDoNotMarksDirty()
        {
            // Initial insert to set up value
            ChangeTrackingSessionStateItemCollection items = new ChangeTrackingSessionStateItemCollection();
            items["key1"] = "value1";
            items["key2"] = 10;
            items.Dirty = false;
            Assert.Equal(2, items.Count);
            Assert.Equal(false, items.Dirty);
            Assert.Equal(0, items.GetModifiedKeys().Count);
            Assert.Equal(0, items.GetDeletedKeys().Count);

            // update value 
            string key1 = (string) items["key1"];
            int key2 = (int)items["key2"];
            Assert.Equal(2, items.Count);
            Assert.Equal(0, items.GetModifiedKeys().Count);
        }
    }
}
