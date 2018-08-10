using System;
using System.Threading;
using FakeItEasy;
using StackExchange.Redis;
using Xunit;

namespace Microsoft.Web.Redis.Tests
{
    public class RedisSharedConnectionTests
    {
        private class TestingConnectionMultiplexerFactory : IConnectionMultiplexerFactory
        {
            public TestingConnectionMultiplexerFactory()
            {
                SetFactory();
            }

#if DOTNET_462
            private static AsyncLocal<IConnectionMultiplexerFactory> _factoryProxy { get; } = new AsyncLocal<IConnectionMultiplexerFactory>();
#else
            [ThreadStatic]
            private static Lazy<IConnectionMultiplexerFactory> _factoryProxy;
#endif

            public static IConnectionMultiplexerFactory FactoryProxy => _factoryProxy.Value;

            private static void SetFactory()
            {
                var factory = A.Fake<IConnectionMultiplexerFactory>();
#if DOTNET_462
                _factoryProxy.Value = factory;
#else
                _factoryProxy = new Lazy<IConnectionMultiplexerFactory>(() => factory);
#endif
            }

            public IConnectionMultiplexer CreateMultiplexer()
            {
                return _factoryProxy.Value.CreateMultiplexer();
            }

            public IConnectionMultiplexer RestartMultiplexer(IConnectionMultiplexer connectionMultiplexer)
            {
                return _factoryProxy.Value.RestartMultiplexer(connectionMultiplexer);
            }
        }

        [Fact(DisplayName = "ConnectionMultiplexerFactory should Be Created When Accessing Shared Connection")]
        public void ConnectionMultiplexerFactory_Should_CreateConnection()
        {
            // arrange
            var configuration = new ProviderConfiguration
            {
                ConnectionMultiplexerFactoryType = typeof(TestingConnectionMultiplexerFactory).AssemblyQualifiedName
            };

            // act
            var sharedConnection = new RedisSharedConnection(configuration);
            var connection = sharedConnection.Connection; // getting a connection calls IConnectionMultiplexerFactory.CreateMultiplexer()

            // assert
            var connectionFactory = TestingConnectionMultiplexerFactory.FactoryProxy;
            A.CallTo(() => connectionFactory.CreateMultiplexer()).MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => connectionFactory.RestartMultiplexer(A<IConnectionMultiplexer>.Ignored)).MustNotHaveHappened();
        }

        public void ConnectionMultiplexerFactory_ForceReconnect()
        {
            // arrange
            var connectionFactory = TestingConnectionMultiplexerFactory.FactoryProxy;
            var connectionMultiplexer = A.Fake<IConnectionMultiplexer>();
            A.CallTo(() => connectionFactory.CreateMultiplexer()).Returns(connectionMultiplexer);
            var configuration = new ProviderConfiguration
            {
                ConnectionMultiplexerFactoryType = typeof(TestingConnectionMultiplexerFactory).AssemblyQualifiedName
            };

            // act
            var sharedConnection = new RedisSharedConnection(configuration);
            var connection = sharedConnection.Connection;
            sharedConnection.ForceReconnect();

            // assert
            A.CallTo(() => connectionFactory.CreateMultiplexer()).MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => connectionFactory.RestartMultiplexer(connectionMultiplexer)).MustHaveHappened(Repeated.Exactly.Once);
        }
    }
}