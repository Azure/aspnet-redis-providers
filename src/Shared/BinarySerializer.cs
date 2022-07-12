//
// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.
//

using System.IO;
using System.Text;
using System.Web.SessionState;
using ProtoBuf;

namespace Microsoft.Web.Redis
{
    public class BinarySerializer : ISerializer
    {
        private readonly string redisNull = "REDIS_NULL";
        private readonly string initializeItem = "INITIALIZE_INSTANCE";
        public byte[] Serialize(object data)
        {
            using (var memoryStream = new MemoryStream())
            {
                if (data is null)
                {
                    data = redisNull;
                }
                if (data is SessionStateActions.InitializeItem)
                {
                    data = initializeItem;
                }
                Serializer.Serialize(memoryStream, data);
                return memoryStream.ToArray();
            }
            
        }

        public object Deserialize(byte[] data)
        {
            if (data is null)
            {
                return null;
            }

            using (var memoryStream = new MemoryStream(data))
            {
                object retObject = Serializer.Deserialize<byte[]>(memoryStream);
                
                try
                {
                    string retString = Encoding.Default.GetString((byte[])retObject);
                    if (retString == redisNull)
                    {
                        return null;
                    }
                    else if (retString == initializeItem)
                    {
                        return SessionStateActions.InitializeItem;
                    }
                    return retString;
                }
                catch
                {
                    return retObject;
                }

            }
        }
    }
}
