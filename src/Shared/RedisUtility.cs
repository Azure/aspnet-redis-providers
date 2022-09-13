//
// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.
//

using System;
using System.Collections.Generic;

namespace Microsoft.Web.Redis
{
    internal sealed class RedisUtility
    {
        private readonly ProviderConfiguration _configuration;

        public RedisUtility(ProviderConfiguration configuration)
        {
            _configuration = configuration;
        }

        public int AppendRemoveItemsInList(ChangeTrackingSessionStateItemCollection sessionItems, List<object> list)
        {
            int noOfItemsRemoved = 0;
            if (sessionItems.GetDeletedKeys() != null && sessionItems.GetDeletedKeys().Count != 0)
            {
                foreach (string delKey in sessionItems.GetDeletedKeys())
                {
                    list.Add(delKey);
                    noOfItemsRemoved++;
                }
            }
            return noOfItemsRemoved;
        }

        public int AppendUpdatedOrNewItemsInList(ChangeTrackingSessionStateItemCollection sessionItems, List<object> list)
        {
            int noOfItemsUpdated = 0;
            if (sessionItems.GetModifiedKeys() != null && sessionItems.GetModifiedKeys().Count != 0)
            {
                foreach (string key in sessionItems.GetModifiedKeys())
                {
                    list.Add(key);
                    list.Add(GetBytesFromObject(sessionItems.GetDataWithoutUpdatingModifiedKeys(key)));
                    noOfItemsUpdated++;
                }
            }
            return noOfItemsUpdated;
        }

        public List<object> GetNewItemsAsList(ChangeTrackingSessionStateItemCollection sessionItems)
        {
            List<object> list = new List<object>(sessionItems.Keys.Count * 2);
            foreach (string key in sessionItems.Keys)
            {
                list.Add(key);
                list.Add(GetBytesFromObject(sessionItems.GetDataWithoutUpdatingModifiedKeys(key)));
            }
            return list;
        }
    }
}
