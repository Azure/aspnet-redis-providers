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

        private string format;
        private string keyFormat;

        public KeyGenerator(string id, string applicationName, string format, string keyFormat)
        {
            this.id = id;
            this.format = format;
            this.keyFormat = keyFormat;
            DataKey = string.Format(format, string.Format(keyFormat, applicationName, id), "Data");
            LockKey = string.Format(format, string.Format(keyFormat, applicationName, id), "Write_Lock");
            InternalKey = string.Format(format, string.Format(keyFormat, applicationName, id), "Internal");
        }

        public void RegenerateKeyStringIfIdModified(string id, string applicationName)
        {
            if (!id.Equals(this.id))
            {
                this.id = id;
                DataKey = string.Format(format, string.Format(keyFormat, applicationName, id), "Data");
                LockKey = string.Format(format, string.Format(keyFormat, applicationName, id), "Write_Lock");
                InternalKey = string.Format(format, string.Format(keyFormat, applicationName, id), "Internal");
            }
        }

    }
}
