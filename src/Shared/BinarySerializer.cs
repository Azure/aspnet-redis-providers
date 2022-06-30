//
// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.
//

using System;
using System.IO;
using System.Text;
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
                // prepend type information to serialized data
                Type type = data.GetType();
                var id = Encoding.UTF8.GetBytes(type.AssemblyQualifiedName + '|');
                memoryStream.Write(id, 0, id.Length);
                Serializer.Serialize(memoryStream, data);
                return memoryStream.ToArray();
            }
        }

        public object Deserialize(byte[] data)
        {
            if (data == null)
            {
                return null;
            }

            using (var memoryStream = new MemoryStream(data))
            {
                object retObject = null;

                var pipeIndex = Array.IndexOf(data, (byte)'|');
                if (pipeIndex >= 0)
                {
                    var typeName = Encoding.UTF8.GetString(data, 0, pipeIndex);
                    Type deserializationType = Type.GetType(typeName);
                    memoryStream.Position = pipeIndex + 1;
                    retObject = Serializer.Deserialize(deserializationType, memoryStream);
                }

                if (retObject.GetType() == typeof(RedisNull))
                {
                    return null;
                }

                return retObject;
            }
        }
    }
}
