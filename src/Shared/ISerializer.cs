namespace Microsoft.Web.Redis
{
    public interface ISerializer
    {
        byte[] Serialize(object data);
        object Deserialize(byte[] data);
    }
}