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
        internal SessionStateItemCollection innerCollection;
        
        // key is "session key in uppercase" and value is "actual session key in actual case"
        Dictionary<string, string> allKeys = new Dictionary<string, string>();
        HashSet<string> modifiedKeys = new HashSet<string>();
        HashSet<string> deletedKeys = new HashSet<string>();
        RedisUtility _utility = null;
        bool dirtyFlag = false;

        private string GetSessionNormalizedKeyToUse(string name)
        { 
            string actualNameStoredEarlier;
            string upperCaseValue = name.ToUpperInvariant();
            if (allKeys.TryGetValue(upperCaseValue, out actualNameStoredEarlier))
            {
                return actualNameStoredEarlier;
            }
            allKeys.Add(upperCaseValue, name);
            return name;
        }

        private void AddInModifiedKeys(string key)
        {
            dirtyFlag = true;
            deletedKeys.Remove(key);
            modifiedKeys.Add(key);
        }

        private void AddInDeletedKeys(string key)
        {
            dirtyFlag = true;
            modifiedKeys.Remove(key);
            deletedKeys.Add(key);
        }
        
        internal HashSet<string> GetModifiedKeys()
        {
            return modifiedKeys;
        }
        
        internal HashSet<string> GetDeletedKeys()
        {
            return deletedKeys;
        }

        public ChangeTrackingSessionStateItemCollection(RedisUtility utility)
        {
            _utility = utility;
            innerCollection = new SessionStateItemCollection();
        }

        public void Clear()
        {
            foreach (string key in innerCollection.Keys) 
            {
                AddInDeletedKeys(key);
            }
            innerCollection.Clear();
        }

        public bool Dirty
        {
            get
            {
                return dirtyFlag;
            }
            set
            {
                dirtyFlag = value;
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
            if (innerCollection[normalizedName] != null)
            {
                AddInDeletedKeys(normalizedName);
            }
            innerCollection.Remove(normalizedName);
        }

        public object this[int index]
        {
            get
            {
                string name = innerCollection.Keys[index];
                return GetData(name);
            }
            set
            {
                string name = innerCollection.Keys[index];
                SetData(name, value);
            }
        }

        public object this[string name]
        {
            get
            {
                name = GetSessionNormalizedKeyToUse(name);
                return GetData(name);
            }
            set
            {
                name = GetSessionNormalizedKeyToUse(name);
                SetData(name, value);
            }
        }

        private object GetData(string normalizedName)
        {
            object value = innerCollection[normalizedName];
            if (value != null && !value.GetType().IsValueType && value.GetType() != typeof(string))
            {
                AddInModifiedKeys(normalizedName);
            }
            return value;
        }

        private void SetData(string normalizedName, object value)
        {
            AddInModifiedKeys(normalizedName);
            innerCollection[normalizedName] = value;
        }

        internal void SetDataWithoutUpdatingModifiedKeys(string name, byte[] value)
        {
            name = GetSessionNormalizedKeyToUse(name);
            innerCollection[name] = _utility.GetObjectFromBytes(value);
        }

        internal object GetDataWithoutUpdatingModifiedKeys(string name)
        {
            name = GetSessionNormalizedKeyToUse(name);
            return innerCollection[name];
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
