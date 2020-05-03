//
// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.
//

using System.Security.Cryptography;
using System.Text;

namespace Microsoft.Web.Redis
{
    internal class KeyGenerator
    {
        private static HashAlgorithm Algorithm = SHA256.Create();
        private string id;
        public string DataKey { get; private set; }
        public string LockKey { get; private set; }
        public string InternalKey { get; private set; }

        public KeyGenerator(string id, string applicationName)
        {
            Initialize(id, applicationName);
        }

        public void RegenerateKeyStringIfIdModified(string id, string applicationName)
        {
            if (!id.Equals(this.id))
            {
                Initialize(id, applicationName);
            }
        }

        private void Initialize(string id, string applicationName)
        {
            this.id = id;
            DataKey = StringHash("{" + applicationName + "_" + id + "}_Data");
            LockKey = StringHash("{" + applicationName + "_" + id + "}_Write_Lock");
            InternalKey = StringHash("{" + applicationName + "_" + id + "}_Internal");
        }

        private string StringHash(string key)
        {
            return Encoding.ASCII.GetString(Algorithm.ComputeHash(Encoding.ASCII.GetBytes(key)));
        }

    }
}
