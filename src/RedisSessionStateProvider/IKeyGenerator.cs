//
// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.
//

namespace Microsoft.Web.Redis
{
    public interface IKeyGenerator
    {
        string DataKey { get; }
        string LockKey { get; }
        string InternalKey { get; }
        void GenerateKeys(string id, string applicationName);
    }
}
