//
// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.
//

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web.SessionState;

namespace Microsoft.Web.Redis
{
    /* We can not use SessionStateItemCollection as it is as we need a way to track if item inside session was modified or not in any given request.
       so we use SessionStateItemCollection as backbon for storing session information and keep list of items that are deleted and updated/inserted
       during any request cycle. We use this list to indentify if we want to change any session item or not.*/
    internal class ChangeTrackingSessionStateItemCollection : NameObjectCollectionBase, ISessionStateItemCollection, ICollection, IEnumerable
    {
        // innerCollection will just contains keys now. value is alwasys empty string.
        // actual value will be inside innerSerializeCollection and innerDeserializeCollection
        internal SessionStateItemCollection innerCollection;
        // This is data we got from redis without deserialization
        // Key(string) is key
        // byte[] is actual value but serialize
        internal Dictionary<string, byte[]> innerSerializeCollection;
        // This is data we that is desirialized from innerSerializeCollection
        // Key(string) is key
        // object is actual value
        internal Dictionary<string, object> innerDeserializeCollection;

        // key is "session key in lowercase" and value is "actual session key in actual case"
        Dictionary<string, string> allKeys = new Dictionary<string, string>();
        HashSet<string> modifiedKeys = new HashSet<string>();
        HashSet<string> deletedKeys = new HashSet<string>();
        RedisUtility _utility = null;

        private string GetSessionNormalizedKeyToUse(string name)
        { 
            string actualNameStoredEarlier;
            if (allKeys.TryGetValue(name.ToUpperInvariant(), out actualNameStoredEarlier))
            {
                return actualNameStoredEarlier;
            }
            allKeys.Add(name.ToUpperInvariant(), name);
            return name;
        }

        private void addInModifiedKeys(string key)
        {
            Dirty = true;
            if (deletedKeys.Contains(key))
            {
                deletedKeys.Remove(key);
            }
            modifiedKeys.Add(key);
        }

        private void addInDeletedKeys(string key)
        {
            Dirty = true;
            if (modifiedKeys.Contains(key))
            {
                modifiedKeys.Remove(key);
            }
            deletedKeys.Add(key);
        }
        
        public HashSet<string> GetModifiedKeys()
        {
            return modifiedKeys;
        }
        
        public HashSet<string> GetDeletedKeys()
        {
            return deletedKeys;
        }

        public ChangeTrackingSessionStateItemCollection(RedisUtility utility)
        {
            _utility = utility;
            innerCollection = new SessionStateItemCollection();
            innerSerializeCollection = new Dictionary<string, byte[]>();
            innerDeserializeCollection = new Dictionary<string, object>();
        }

        public void Clear()
        {
            foreach (string key in innerCollection.Keys) 
            {
                addInDeletedKeys(key);
            }
            innerCollection.Clear();
        }

        public bool Dirty
        {
            get
            {
                return innerCollection.Dirty;
            }
            set
            {
                innerCollection.Dirty = value;
                if (!value)
                {
                    modifiedKeys.Clear();
                    deletedKeys.Clear();
                }
            }
        }

        public override NameObjectCollectionBase.KeysCollection Keys
        {
            get { return innerCollection.Keys; }
        }

        public void Remove(string name)
        {
            name = GetSessionNormalizedKeyToUse(name);
            RemoveOperation(name);
        }

        public void RemoveAt(int index)
        {
            string name = innerCollection.Keys[index];
            RemoveOperation(name);
        }

        private void RemoveOperation(string normalizedName)
        {
            if (innerSerializeCollection.ContainsKey(normalizedName))
            {
                innerSerializeCollection.Remove(normalizedName);
            }

            if (innerDeserializeCollection.ContainsKey(normalizedName))
            {
                innerDeserializeCollection.Remove(normalizedName);
            }
            
            if (innerCollection[normalizedName] != null)
            {
                addInDeletedKeys(normalizedName);
            }
            innerCollection.Remove(normalizedName);
        }

        public object this[int index]
        {
            get
            {
                string name = innerCollection.Keys[index];
                return GetOperation(name);
            }
            set
            {
                string name = innerCollection.Keys[index];
                SetOperation(name, value);
            }
        }

        public object this[string name]
        {
            get
            {
                name = GetSessionNormalizedKeyToUse(name);
                return GetOperation(name);
            }
            set
            {
                name = GetSessionNormalizedKeyToUse(name);
                SetOperation(name, value);
            }
        }

        private object GetOperation(string normalizedName)
        {
            DeserializeSpecificItem(normalizedName);
            if (IsMutable(innerDeserializeCollection[normalizedName]))
            {
                addInModifiedKeys(normalizedName);
            }
            return innerDeserializeCollection[normalizedName];
        }

        private void SetOperation(string normalizedName, object value)
        {
            DeserializeSpecificItem(normalizedName);
            addInModifiedKeys(normalizedName);
            innerDeserializeCollection[normalizedName] = value;
            innerCollection[normalizedName] = "";
        }

        internal void AddSerializeData(string name, byte[] value)
        {
            name = GetSessionNormalizedKeyToUse(name);
            innerCollection[name] = "";
            innerSerializeCollection[name] = value;
        }

        private void DeserializeSpecificItem(string normalizedName)
        {
            // deserializa if accessing first time
            if (innerSerializeCollection.ContainsKey(normalizedName))
            {
                innerDeserializeCollection[normalizedName] = _utility.GetObjectFromBytes(innerSerializeCollection[normalizedName]);
                innerSerializeCollection.Remove(normalizedName);
            }
        }

        private bool IsMutable(object data)
        {
            if (data != null && !data.GetType().IsValueType && data.GetType() != typeof(string))
            {
                return true;
            }
            return false;
        }

        public override IEnumerator GetEnumerator()
        {
            return innerCollection.GetEnumerator();
        }

        public override int Count
        {
            get { return innerCollection.Count; }
        }
    }
}
