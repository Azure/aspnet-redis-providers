//
// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.
//

using System;
using System.IO;
using System.Reflection;
using System.Text;
using ProtoBuf;
using System.Web.SessionState;
using System.Runtime.CompilerServices;

namespace Microsoft.Web.Redis
{
    [ProtoContract]
    public class Message
    {
        private readonly object data;

        public Message() { }

        public Message(object data)
        {
            this.data = data;
        }

        public object GetObject() { return data; }

    }

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

                if (type.IsEnum)
                {
                    data = new Message(data);
                    type = data.GetType();
                }

                var id = System.Text.ASCIIEncoding.ASCII.GetBytes(type.FullName + '|');
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

                if (retObject.GetType() == typeof(Message))
                {
                    Message message = (Message)retObject;
                    retObject = message.GetObject();
                }

                return retObject;
            }
        }
    }
}
