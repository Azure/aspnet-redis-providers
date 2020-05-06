using System.Security.Cryptography;
using System.Text;

namespace Microsoft.Web.Redis.Tests
{
    class TestKeyGenerator : IKeyGenerator
    {
        private string id;
        private string applicationName;
        private static string salt = "totally random salt";
        public string DataKey { get; private set; }
        public string LockKey { get; private set; }
        public string InternalKey { get; private set; }

        public void GenerateKeys(string id, string applicationName)
        {
            if(!string.Equals(this.id, id) || !string.Equals(this.applicationName, applicationName)) {
                this.id = id;
                this.applicationName = applicationName;
                DataKey = StringHash("{" + applicationName + "_" + id + "}_Data" + salt);
                LockKey = StringHash("{" + applicationName + "_" + id + "}_Write_Lock" + salt);
                InternalKey = StringHash("{" + applicationName + "_" + id + "}_Internal" + salt);
            }
        }

        private string StringHash(string s)
        {
            return Encoding.ASCII.GetString(SHA512.Create().ComputeHash(Encoding.ASCII.GetBytes(s)));
        }
    }
}
