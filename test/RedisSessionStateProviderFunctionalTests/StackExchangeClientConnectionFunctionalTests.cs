using Microsoft.Web.Redis.Tests;

using Xunit;

namespace Microsoft.Web.Redis.FunctionalTests
{
    public class StackExchangeClientConnectionFunctionalTests
    {
        [Fact]
        public void Constructor_DatabaseIdFromConfigurationProperty()
        {
            using (RedisServer redisServer = new RedisServer())
            {
                int databaseId = 7;
                ProviderConfiguration configuration = Utility.GetDefaultConfigUtility();
                configuration.DatabaseId = databaseId;

                StackExchangeClientConnection connection = GetStackExchangeClientConnection(configuration);

                Assert.Equal(databaseId, connection.RealConnection.Database);
            }
        }

        [Fact]
        public void Constructor_DatabaseIdFromConnectionString()
        {
            using (RedisServer redisServer = new RedisServer())
            {
                int databaseId = 3;
                ProviderConfiguration configuration = Utility.GetDefaultConfigUtility();
                configuration.ConnectionString = string.Format("localhost, defaultDatabase={0}", databaseId);

                StackExchangeClientConnection connection = GetStackExchangeClientConnection(configuration);

                Assert.Equal(databaseId, connection.RealConnection.Database);
            }
        }

        [Fact]
        public void Constructor_DatabaseIdFromConfigurationPropertyWhenNotSetInConnectionString()
        {
            using (RedisServer redisServer = new RedisServer())
            {
                int databaseId = 5;
                ProviderConfiguration configuration = Utility.GetDefaultConfigUtility();
                configuration.DatabaseId = databaseId;
                configuration.ConnectionString = string.Format("localhost");

                StackExchangeClientConnection connection = GetStackExchangeClientConnection(configuration);

                Assert.Equal(databaseId, connection.RealConnection.Database);
            }
        }

        private StackExchangeClientConnection GetStackExchangeClientConnection(ProviderConfiguration configuration)
        {
            var sharedConnection = new RedisSharedConnection(configuration);
            var redisUtility = new RedisUtility(configuration);
            return new StackExchangeClientConnection(configuration, redisUtility, sharedConnection);
        }
    }
}