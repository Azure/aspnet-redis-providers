//
// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.
//

using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Microsoft.Web.Redis
{
    internal static class RedisUtility
    {
        internal static ISerializer Serializer;

        static RedisUtility()
        {
            var serializerType = ConfigurationManager.AppSettings["RedisSerializerType"];
            Serializer = string.IsNullOrWhiteSpace(serializerType)
                ? new BinarySerializer()     
                : (ISerializer)Activator.CreateInstance(Type.GetType(serializerType));
        }

        public static int AppendRemoveItemsInList(ChangeTrackingSessionStateItemCollection sessionItems, List<object> list)
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

        public static int AppendUpdatedOrNewItemsInList(ChangeTrackingSessionStateItemCollection sessionItems, List<object> list)
        {
            int noOfItemsUpdated = 0;
            if (sessionItems.GetModifiedKeys() != null && sessionItems.GetModifiedKeys().Count != 0)
            {
                foreach (string key in sessionItems.GetModifiedKeys())
                {
                    list.Add(key);
                    list.Add(GetBytesFromObject(sessionItems[key]));
                    noOfItemsUpdated++;
                }
            }
            return noOfItemsUpdated;
        }

        public static List<object> GetNewItemsAsList(ChangeTrackingSessionStateItemCollection sessionItems)
        {
            List<object> list = new List<object>();
            foreach (string key in sessionItems.Keys)
            {
                list.Add(key);
                list.Add(GetBytesFromObject(sessionItems[key]));
            }
            return list;
        }

        internal static byte[] GetBytesFromObject(object data)
        {
            return Serializer.Serialize(data);
        }

        internal static object GetObjectFromBytes(byte[] dataAsBytes)
        {
            return Serializer.Deserialize(dataAsBytes);
        }
    }
}
