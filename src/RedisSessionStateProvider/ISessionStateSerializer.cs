using System.Web.SessionState;

/// <summary>
/// Provides methods to serialize and deserialize session state data.
/// </summary>
public interface ISessionStateSerializer
{
    /// <summary>
    /// Serializes the session state data.
    /// </summary>
    /// <param name="data">The session state data to serialize.</param>
    /// <returns>The serialized session state data as a byte array.</returns>
    byte[] Serialize(SessionStateItemCollection data);

    /// <summary>
    /// Deserializes the session state data.
    /// </summary>
    /// <param name="data">The serialized session state data as a byte array.</param>
    /// <returns>The deserialized session state data.</returns>
    SessionStateItemCollection Deserialize(byte[] data);
}
