using Microsoft.Web.RedisSessionStateProvider;
using System.IO;
using System.Web.SessionState;
using Xunit;

namespace Microsoft.Web.Redis.Tests
{
    public class DefaultSessionStateSerializerTests
    {
        [Fact]
        public void Deserialize_ShouldReturnDeserializedSessionStateData()
        {
            // Arrange
            var serializer = new DefaultSessionStateSerializer();
            var sessionStateData = new SessionStateItemCollection();
            sessionStateData["Key1"] = "Value1";
            sessionStateData["Key2"] = 123;

            byte[] serializedData;
            using (var ms = new MemoryStream())
            {
                using (var writer = new BinaryWriter(ms))
                {
                    sessionStateData.Serialize(writer);
                }
                serializedData = ms.ToArray();
            }

            // Act
            var deserializedData = serializer.Deserialize(serializedData);

            // Assert
            Assert.Equal(sessionStateData.Count, deserializedData.Count);
            Assert.Equal(sessionStateData["Key1"], deserializedData["Key1"]);
            Assert.Equal(sessionStateData["Key2"], deserializedData["Key2"]);
        }

        [Fact]
        public void Serialize_ShouldReturnSerializedSessionStateData()
        {
            // Arrange
            var serializer = new DefaultSessionStateSerializer();
            var sessionStateData = new SessionStateItemCollection();
            sessionStateData["Key1"] = "Value1";
            sessionStateData["Key2"] = 123;

            byte[] expectedSerializedData;
            using (var ms = new MemoryStream())
            {
                using (var writer = new BinaryWriter(ms))
                {
                    sessionStateData.Serialize(writer);
                }
                expectedSerializedData = ms.ToArray();
            }

            // Act
            var serializedData = serializer.Serialize(sessionStateData);

            // Assert
            Assert.Equal(expectedSerializedData, serializedData);
        }
    }
}
