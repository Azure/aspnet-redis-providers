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
            return Serialize(data);
        }

        public object Deserialize(byte[] data)
        {
            using (var memoryStream = new MemoryStream(data))
            {
                return Serializer.Deserialize<byte[]>(memoryStream);
            }
        }
    }
}
