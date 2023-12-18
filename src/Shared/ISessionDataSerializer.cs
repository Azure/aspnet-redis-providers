using System.Web.SessionState;

namespace Microsoft.Web.RedisSessionStateProvider
{
    public interface ISessionDataSerializer
    {
        string StorageTypeName { get; }
        byte[] Serialize(SessionStateItemCollection data);
        SessionStateItemCollection Deserialize(byte[] data);
    }
}
