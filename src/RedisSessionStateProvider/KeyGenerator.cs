using System;

namespace Microsoft.Web.Redis
{
    internal class KeyGenerator
    {
        private string id;
        public string DataKey { get; private set; }
        public string LockKey { get; private set; }
        public string InternalKey { get; private set; }

        public KeyGenerator(string id, string applicationName)
        {
            this.id = id;
            DataKey = applicationName + "_" + id + "_Data";
            LockKey = applicationName + "_" + id + "_Write_Lock";
            InternalKey = applicationName + "_" + id + "_Internal";
        }

        public void RegenerateKeyStringIfIdModified(string id, string applicationName)
        {
            if (!id.Equals(this.id))
            {
                this.id = id;
                DataKey = applicationName + "_" + id + "_Data";
                LockKey = applicationName + "_" + id + "_Write_Lock";
                InternalKey = applicationName + "_" + id + "_Internal";
            }
        }

    }
}
