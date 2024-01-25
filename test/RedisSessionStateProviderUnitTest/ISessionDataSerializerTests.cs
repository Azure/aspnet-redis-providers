using System.Web.SessionState;
using Xunit;
using FakeItEasy;


namespace Microsoft.Web.Redis.Tests
{
    public class ISessionDataSerializerTests
    {

        [Fact]
        public void Serialize_ValidData_ReturnsByteArray()
        {
            // Arrange
            ISessionDataSerializer serializer = A.Fake<ISessionDataSerializer>();
            A.CallTo(() => serializer.Serialize(A<SessionStateItemCollection>.That.IsNotNull())).Returns(new byte[1]);
            var sessionData = new SessionStateItemCollection();
            // Add session state items to the collection
            sessionData["Key1"] = "Value1";
            sessionData["Key2"] = 123;

            // Act
            byte[] serializedData = serializer.Serialize(sessionData);

            // Assert
            Assert.NotNull(serializedData);
            Assert.True(serializedData.Length > 0);
        }

        [Fact]
        public void Deserialize_ValidData_ReturnsSessionStateItemCollection()
        {
            // Arrange
            ISessionDataSerializer serializer = A.Fake<ISessionDataSerializer>();
            A.CallTo(() => serializer.Deserialize(A<byte[]>.Ignored)).Returns(new SessionStateItemCollection());
            byte[] serializedData = new byte[] { /* Serialized session state data */ };

            // Act
            SessionStateItemCollection deserializedData = serializer.Deserialize(serializedData);

            // Assert
            Assert.NotNull(deserializedData);
            // Assert specific session state items in the deserializedData
        }
    }
}