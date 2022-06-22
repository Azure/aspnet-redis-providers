//
// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.
//

using System.IO;
using ProtoBuf;

namespace Microsoft.Web.Redis
{
    public class BinarySerializer : ISerializer
    {
        public byte[] Serialize(object data)
        {
            if (data == null)
            {
                data = new RedisNull();
            }
            using (var memoryStream = new MemoryStream())
            {
                Serializer.Serialize(memoryStream, data);
                byte[] objectDataAsStream = memoryStream.ToArray();
                return objectDataAsStream;
            }
        }

        public object Deserialize(byte[] data)
        {
            if (data == null)
            {
                return null;
            }
            using (var memoryStream = new MemoryStream(data, 0, data.Length))
            {
                memoryStream.Seek(0, SeekOrigin.Begin);
                object retObject = Serializer.Deserialize<object>(memoryStream);
                if (retObject.GetType() == typeof(RedisNull))
                {
                    return null;
                }
                return retObject;
            }
        }
    }
}
