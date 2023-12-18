using System.IO;
using System.Web.SessionState;

namespace Microsoft.Web.RedisSessionStateProvider
{
    /// <summary>
    /// The default, "safe" serialization.  
    /// Hint. It's not safe. But it is what MS allows today.
    /// Better is to use json or MessagePack.
    /// </summary>
    internal class BinaryFormattingSessionSerializer : ISessionDataSerializer
    {
        public string StorageTypeName { get => "SessionStateItemCollection"; }

        public SessionStateItemCollection Deserialize(byte[] data)
        {
            MemoryStream ms = new MemoryStream((byte[])data);
            BinaryReader reader = new BinaryReader(ms);
            return SessionStateItemCollection.Deserialize(reader);

        }

        public byte[] Serialize(SessionStateItemCollection data)
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(ms);
            ((SessionStateItemCollection)data).Serialize(writer);
            writer.Close();
            return ms.ToArray();
        }
    }
}
