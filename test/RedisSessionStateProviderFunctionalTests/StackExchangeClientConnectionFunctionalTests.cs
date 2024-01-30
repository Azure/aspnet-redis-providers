using Microsoft.Web.Redis.Tests;

using Xunit;

namespace Microsoft.Web.Redis.FunctionalTests
{
    public class StackExchangeClientConnectionFunctionalTests
    {
        [Fact()]
        public void Constructor_DatabaseIdFromConfigurationProperty()
        {
            using (RedisServer redisServer = new RedisServer())
            {
                int databaseId = 7;
                IProviderConfiguration configuration = Utility.GetDefaultConfigUtility();
                configuration.DatabaseId = databaseId;

                StackExchangeClientConnection connection = GetStackExchangeClientConnection(configuration);

                Assert.Equal(databaseId, connection.RealConnection.Database);
            }
        }

        [Fact()]
        public void Constructor_DatabaseIdFromConnectionString()
        {
            using (RedisServer redisServer = new RedisServer())
            {
                int databaseId = 3;
                IProviderConfiguration configuration = Utility.GetDefaultConfigUtility();
                configuration.ConnectionString = string.Format("localhost, defaultDatabase={0}", databaseId);

                StackExchangeClientConnection connection = GetStackExchangeClientConnection(configuration);

                Assert.Equal(databaseId, connection.RealConnection.Database);
            }
        }

        [Fact()]
        public void Constructor_DatabaseIdFromConfigurationPropertyWhenNotSetInConnectionString()
        {
            using (RedisServer redisServer = new RedisServer())
            {
                int databaseId = 5;
                IProviderConfiguration configuration = Utility.GetDefaultConfigUtility();
                configuration.DatabaseId = databaseId;
                configuration.ConnectionString = string.Format("localhost");

                StackExchangeClientConnection connection = GetStackExchangeClientConnection(configuration);

                Assert.Equal(databaseId, connection.RealConnection.Database);
            }
        }

        private StackExchangeClientConnection GetStackExchangeClientConnection(IProviderConfiguration configuration)
        {
            var sharedConnection = new RedisSharedConnection(configuration);
            return new StackExchangeClientConnection(configuration, sharedConnection);
        }
    }
}