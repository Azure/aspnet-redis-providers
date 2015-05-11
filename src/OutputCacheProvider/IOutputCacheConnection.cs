//
// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.
//

using System;

namespace Microsoft.Web.Redis
{
    internal interface IOutputCacheConnection
    {
        void Set(string key, object entry, DateTime utcExpiry);        
        object Add(string key, object entry, DateTime utcExpiry);
        object Get(string key);
        void Remove(string key);   
    }
}
