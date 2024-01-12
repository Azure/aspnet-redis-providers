//
// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.
//

namespace Microsoft.Web.Redis
{
    internal class KeyGenerator
    {
        private string id;
        public string DataKey { get; private set; }
        public string LockKey { get; private set; }
        public string InternalKey { get; private set; }

        private void GenerateKeys(string id, string app)
        {
            this.id = id;
            DataKey = $"{{{app}_{id}}}_SessionStateItemCollection";
            LockKey = $"{{{app}_{id}}}_WriteLock";
            InternalKey = $"{{{app}_{id}}}_SessionTimeout";
        }

        public KeyGenerator(string sessionId, string applicationName)
        {
            GenerateKeys(sessionId, applicationName);
        }

        public void RegenerateKeyStringIfIdModified(string sessionId, string applicationName)
        {
            if (!sessionId.Equals(this.id))
            {
                GenerateKeys(sessionId, applicationName);
            }
        }
    }
}