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
        private readonly byte redisNull = 1;
        private readonly byte initializeItem = 2;
        private readonly byte str = 3;
        public byte[] Serialize(object data)
        {
            using (var memoryStream = new MemoryStream())
            {
                if (data is null)
                {
                    return new byte[] { redisNull };
                }
                if (data is SessionStateActions.InitializeItem)
                {
                    return new byte[] { initializeItem };
                }
                if (data is string)
                {
                    memoryStream.WriteByte(str);
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

            byte firstByte = data[0];

            if (firstByte == redisNull)
            {
                return null;
            }

            if (firstByte == initializeItem)
            {
                return SessionStateActions.InitializeItem;
            }

            using (var memoryStream = new MemoryStream(data))
            {
                if (firstByte == str)
                {
                    memoryStream.ReadByte();
                }

                byte[] retObject = Serializer.Deserialize<byte[]>(memoryStream);


                if (firstByte == str)
                {
                    return Encoding.Default.GetString(retObject);
                }
                else
                {
                    return retObject;
                }

            }
        }
    }
}
