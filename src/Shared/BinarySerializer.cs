﻿//
// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.
//

using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.SessionState;
using ProtoBuf;

namespace Microsoft.Web.Redis
{

    public class BinarySerializer : ISerializer
    {

        public enum DataTypes : byte
        {
            redisNull,
            initializeItem,
            str,
            byteArray,
            obj
        }

        public byte[] Serialize(object data)
        {
            switch (data)
            {
                case null:
                    return new byte[] { (byte)DataTypes.redisNull };
                case SessionStateActions.InitializeItem:
                    return new byte[] { (byte)DataTypes.initializeItem };
                case string:
                    return PrependHeaderToData(Encoding.UTF8.GetBytes((string)data), (byte)DataTypes.str);
                case byte[]:
                    return PrependHeaderToData((byte[])data, (byte)DataTypes.byteArray);
                default:
                    try
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            memoryStream.WriteByte((byte)DataTypes.obj);
                            Type type = data.GetType();
                            var id = Encoding.UTF8.GetBytes(type.AssemblyQualifiedName + '|');
                            memoryStream.Write(id, 0, id.Length);
                            Serializer.Serialize(memoryStream, data);
                            return memoryStream.ToArray();
                        }
                    }
                    catch
                    {
                        throw new ArgumentException("Object type is not supported.");
                    }
            }
        }

        private static byte[] PrependHeaderToData(byte[] data, byte header)
        {
            byte[] newArray = new byte[data.Length + 1];
            data.CopyTo(newArray, 1);
            newArray[0] = header;
            return newArray;
        }

        public object Deserialize(byte[] data)
        {
            if (data is null)
            {
                return null;
            }

            if (data.Length == 0)
            {
                throw new ArgumentException("Header not present in data.");
            }

            byte headerByte = data[0];

            switch (headerByte)
            {
                case (byte)DataTypes.redisNull:
                    return null;
                case (byte)DataTypes.initializeItem:
                    return SessionStateActions.InitializeItem;
                case (byte)DataTypes.str:
                    return Encoding.UTF8.GetString(data.Skip(1).ToArray());
                case (byte)DataTypes.byteArray:
                    return data.Skip(1).ToArray();
                default:
                    try
                    {
                        object retObject = null;
                        using (var memoryStream = new MemoryStream(data))
                        {
                            var pipeIndex = Array.IndexOf(data, (byte)'|');
                            if (pipeIndex >= 0)
                            {
                                var typeName = Encoding.UTF8.GetString(data, 0, pipeIndex);
                                Type deserializationType = Type.GetType(typeName);
                                memoryStream.Position = pipeIndex + 1;
                                retObject = Serializer.Deserialize(deserializationType, memoryStream);
                            }
                        }
                        return retObject;
                    }
                    catch
                    {
                        throw new ArgumentException("Unknown type.");
                    }

            }
        }
    }
}
