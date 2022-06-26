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
                var id = System.Text.ASCIIEncoding.ASCII.GetBytes(type.AssemblyQualifiedName + '|');
                memoryStream.Write(id, 0, id.Length);
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

            StringBuilder stringBuilder = new StringBuilder();
            using (var memoryStream = new MemoryStream(data))
            {
                // read the type information from the serialized data
                while (true)
                {
                    var currentChar = (char)memoryStream.ReadByte();
                    if (currentChar == '|')
                    {
                        break;
                    }

                    stringBuilder.Append(currentChar);
                }

                string typeName = stringBuilder.ToString();
                Type deserializationType = Type.GetType(typeName);

                object retObject = Serializer.Deserialize(deserializationType, memoryStream);

                if (retObject.GetType() == typeof(RedisNull))
                {
                    return null;
                }

                return retObject;
            }
        }
    }
}
