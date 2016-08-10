//
// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.
//

using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

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
            var binaryFormatter = new BinaryFormatter();
            using (var memoryStream = new MemoryStream())
            {
                binaryFormatter.Serialize(memoryStream, data);
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
            var binaryFormatter = new BinaryFormatter();
            using (var memoryStream = new MemoryStream(data, 0, data.Length))
            {
                memoryStream.Seek(0, SeekOrigin.Begin);
                object retObject = (object)binaryFormatter.Deserialize(memoryStream);
                if (retObject.GetType() == typeof(RedisNull))
                {
                    return null;
                }
                return retObject;
            }
        }
    }
}
