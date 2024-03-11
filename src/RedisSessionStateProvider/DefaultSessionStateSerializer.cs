using System.IO;
using System.Web.SessionState;

namespace Microsoft.Web.RedisSessionStateProvider
{
    /// <summary>
    /// Provides methods to serialize and deserialize session state data using binary serialization.
    /// This is the default implementation of the ISessionDataSerializer class.
    /// The implementation uses the serialization and deserialization methods provided by the SessionStateItemCollection class. 
    /// </summary>
    internal class DefaultSessionStateSerializer : ISessionStateSerializer
    {
        /// <summary>
        /// Deserializes the session state data.
        /// </summary>
        /// <param name="data">The serialized session state data as a byte array.</param>
        /// <returns>The deserialized session state data.</returns>
        public SessionStateItemCollection Deserialize(byte[] data)
        {
            MemoryStream ms = new MemoryStream(data);
            BinaryReader reader = new BinaryReader(ms);
            return SessionStateItemCollection.Deserialize(reader);
        }

        /// <summary>
        /// Serializes the session state data.
        /// </summary>
        /// <param name="data">The session state data to serialize.</param>
        /// <returns>The serialized session state data as a byte array.</returns>
        public byte[] Serialize(SessionStateItemCollection data)
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(ms);
            data.Serialize(writer);
            writer.Close();
            return ms.ToArray();
        }
    }
}
