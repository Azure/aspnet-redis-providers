//
// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.
//

namespace Microsoft.Web.Redis
{
    internal class SimpleKeyGenerator : IKeyGenerator
    {
        private string id;
        public string DataKey { get; private set; }
        public string LockKey { get; private set; }
        public string InternalKey { get; private set; }

        public void GenerateKeys(string id, string applicationName)
        {
            if (this.id != id)
            {
                this.id = id;
                DataKey = "{" + applicationName + "_" + id + "}_Data";
                LockKey = "{" + applicationName + "_" + id + "}_Write_Lock";
                InternalKey = "{" + applicationName + "_" + id + "}_Internal";
            }
        }
    }
}
