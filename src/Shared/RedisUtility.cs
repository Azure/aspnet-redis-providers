//
// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Web.SessionState;

namespace Microsoft.Web.Redis
{
    internal static class RedisUtility
    {
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
                    var byteArray = GetBytesFromObject(sessionItems[key]);
                    list.Add(key);
                    list.Add(byteArray);
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
            if (data == null)
            {
                data = new RedisNull();
            }

            if (data is MainStreet.BusinessFlow.SDK.Cart)
            {
                data = new SerializableCart((MainStreet.BusinessFlow.SDK.Cart)data);
            }

            BinaryFormatter binaryFormatter = new BinaryFormatter();
            using (MemoryStream memoryStream = new MemoryStream())
            {
                binaryFormatter.Serialize(memoryStream, data);
                byte[] objectDataAsStream = memoryStream.ToArray();

                return objectDataAsStream;
            }
        }

        internal static object GetObjectFromBytes(byte[] dataAsBytes)
        {
            if (dataAsBytes == null)
            {
                return null;
            }

            BinaryFormatter binaryFormatter = new BinaryFormatter();
            using (MemoryStream memoryStream = new MemoryStream(dataAsBytes, 0, dataAsBytes.Length))
            {
                memoryStream.Seek(0, System.IO.SeekOrigin.Begin);
                object retObject = (object)binaryFormatter.Deserialize(memoryStream);

                if (retObject.GetType() == typeof(RedisNull))
                {
                    return null;
                }
                else if (retObject.GetType() == typeof(SerializableCart))
                {
                    return ((SerializableCart)retObject).ToCart();
                }
                return retObject;
            }
        }
    }
}
