//
// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.
//

namespace Microsoft.Web.Redis
{
    public interface ISerializer
    {
        byte[] Serialize(object data);
        object Deserialize(byte[] data);
    }
}