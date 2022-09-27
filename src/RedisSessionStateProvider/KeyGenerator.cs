//
// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.
//

using System;

namespace Microsoft.Web.Redis
{
    internal class KeyGenerator
    {
        private string id;
        public string DataKey { get; private set; }
        public string LockKey { get; private set; }
        public string InternalKey { get; private set; }

        public KeyGenerator(string sessionId, string applicationName)
        {
            this.id = sessionId;
            DataKey = "{" + applicationName + "_" + sessionId + "}_Data";
            LockKey = "{" + applicationName + "_" + sessionId + "}_Write_Lock";
            InternalKey = "{" + applicationName + "_" + sessionId + "}_Internal";
        }

        public void RegenerateKeyStringIfIdModified(string sessionId, string applicationName)
        {
            if (!sessionId.Equals(this.id))
            {
                this.id = sessionId;
                DataKey = "{" + applicationName + "_" + sessionId + "}_Data";
                LockKey = "{" + applicationName + "_" + sessionId + "}_Write_Lock";
                InternalKey = "{" + applicationName + "_" + sessionId + "}_Internal";
            }
        }
    }
}