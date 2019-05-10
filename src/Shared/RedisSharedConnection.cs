//
// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.
//

using System;
using StackExchange.Redis;

namespace Microsoft.Web.Redis
{
    internal class RedisSharedConnection
    {
        private readonly ProviderConfiguration _configuration;
        private readonly IConnectionMultiplexerFactory _factory;
        private Lazy<IConnectionMultiplexer> _connectionMultiplexer;

        // Used for mocking in testing
        internal RedisSharedConnection()
        { }

        public RedisSharedConnection(ProviderConfiguration configuration)
        {
            _configuration = configuration;
            _factory = string.IsNullOrEmpty(configuration.ConnectionMultiplexerFactoryType)
                ? new ConnectionMultiplexerFactory(configuration)
                : CreateMultiplexerFactory(configuration.ConnectionMultiplexerFactoryType);

            _connectionMultiplexer = new Lazy<IConnectionMultiplexer>(_factory.CreateMultiplexer);
        }

        public IDatabase Connection
        {
            get
            {
                var db = _configuration.DatabaseId;
                return db == default(int)
                    ? _connectionMultiplexer.Value.GetDatabase()
                    : _connectionMultiplexer.Value.GetDatabase(db);
            }
        }

        public void ForceReconnect()
        {
            var cm = _factory.RestartMultiplexer(_connectionMultiplexer.Value);
            _connectionMultiplexer = new Lazy<IConnectionMultiplexer>(() => cm);
        }

        private static IConnectionMultiplexerFactory CreateMultiplexerFactory(string connectionMultiplexerFactoryType)
        {
            var serializerType = Type.GetType(connectionMultiplexerFactoryType, true);
            return (IConnectionMultiplexerFactory)Activator.CreateInstance(serializerType);
        }
    }
}