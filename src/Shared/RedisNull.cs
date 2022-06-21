//
// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.
//

using System;
using System.Runtime.Serialization;

namespace Microsoft.Web.Redis
{
    [Serializable]
    internal class RedisNull : ISerializable
    {
        public RedisNull() 
        {}
        protected RedisNull(SerializationInfo info, StreamingContext context)
        {}
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {}
    } 
}
