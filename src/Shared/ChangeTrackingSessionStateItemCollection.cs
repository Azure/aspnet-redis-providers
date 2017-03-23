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
        // innerCollection will just contains keys now. value is alwasys of type ValueWrapper.
        internal SessionStateItemCollection innerCollection;
        
        // key is "session key in lowercase" and value is "actual session key in actual case"
        Dictionary<string, string> allKeys = new Dictionary<string, string>();
        HashSet<string> modifiedKeys = new HashSet<string>();
        HashSet<string> deletedKeys = new HashSet<string>();
        RedisUtility _utility = null;
        bool dirtyFlag = false;

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
            dirtyFlag = true;
            if (deletedKeys.Contains(key))
            {
                deletedKeys.Remove(key);
            }
            modifiedKeys.Add(key);
        }

        private void addInDeletedKeys(string key)
        {
            dirtyFlag = true;
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
            ValueWrapper value = (ValueWrapper) innerCollection[normalizedName];
            if (value != null)
            {
                object actualValue = value.GetActualValue(_utility);
                if (IsMutable(actualValue))
                {
                    addInModifiedKeys(normalizedName);
                }
                return actualValue;
            }
            return null;
        }

        private void SetOperation(string normalizedName, object value)
        {
            addInModifiedKeys(normalizedName);
            innerCollection[normalizedName] = new ValueWrapper() { ActualValue = value, Serializedvalue = null };
        }

        internal void AddSerializeData(string name, byte[] value)
        {
            name = GetSessionNormalizedKeyToUse(name);
            innerCollection[name] = new ValueWrapper() { ActualValue = null, Serializedvalue = value };
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
