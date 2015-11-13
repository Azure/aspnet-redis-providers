﻿//
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

        public KeyGenerator(string id, string applicationName, string prefix)
        {
            SetKeys(id, applicationName, prefix);
        }

        public void RegenerateKeyStringIfIdModified(string id, string applicationName, string prefix)
        {
            if (!id.Equals(this.id))
            {
                SetKeys(id, applicationName, prefix);
            }
        }

        private void SetKeys(string id, string applicationName, string prefix)
        {
            this.id = id;
            DataKey = prefix + "{" + applicationName + "_" + id + "}_Data";
            LockKey = prefix + "{" + applicationName + "_" + id + "}_Write_Lock";
            InternalKey = prefix + "{" + applicationName + "_" + id + "}_Internal";
        }
    }
}
