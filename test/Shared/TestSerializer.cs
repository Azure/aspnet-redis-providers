//
// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.
//

using System.Text;

namespace Microsoft.Web.Redis.Tests
{
    public class TestSerializer : ISerializer
    {
        internal int SerializeCount;
        internal int DeserializeCount;

        public object Deserialize(byte[] data)
        {
            DeserializeCount++;
            return Encoding.UTF8.GetString(data);
        }

        public byte[] Serialize(object data)
        {
            SerializeCount++;
            return Encoding.UTF8.GetBytes(data.ToString());
        }
    }
}