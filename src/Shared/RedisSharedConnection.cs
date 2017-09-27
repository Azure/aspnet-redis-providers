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
        private ProviderConfiguration _configuration;
        private ConfigurationOptions _configOption;
        private ConnectionMultiplexer _redisMultiplexer;

        internal static DateTimeOffset lastReconnectTime = DateTimeOffset.MinValue;
        internal static DateTimeOffset lastErrorTime = DateTimeOffset.MinValue;
        static object reconnectLock = new object();
        internal static TimeSpan ReconnectFrequency = TimeSpan.FromSeconds(60);
        internal static TimeSpan ReconnectErrorFrequency = TimeSpan.FromSeconds(31);

        // Used for mocking in testing
        internal RedisSharedConnection()
        { }

        public RedisSharedConnection(ProviderConfiguration configuration)
        {
            _configuration = configuration;
            
            // If connection string is given then use it otherwise use individual options
            if (!string.IsNullOrEmpty(configuration.ConnectionString))
            {
                _configOption = ConfigurationOptions.Parse(configuration.ConnectionString);
                // Setting explicitly 'abortconnect' to false. It will overwrite customer provided value for 'abortconnect'
                // As it doesn't make sense to allow to customer to set it to true as we don't give them access to ConnectionMultiplexer
                // in case of failure customer can not create ConnectionMultiplexer so right choice is to automatically create it by providing AbortOnConnectFail = false
                _configOption.AbortOnConnectFail = false;
            }
            else
            {
                _configOption = new ConfigurationOptions();
                if (configuration.Port == 0)
                {
                    _configOption.EndPoints.Add(configuration.Host);
                }
                else
                {
                    _configOption.EndPoints.Add(configuration.Host + ":" + configuration.Port);
                }
                _configOption.Password = configuration.AccessKey;
                _configOption.Ssl = configuration.UseSsl;
                _configOption.AbortOnConnectFail = false;

                if (configuration.ConnectionTimeoutInMilliSec != 0)
                {
                    _configOption.ConnectTimeout = configuration.ConnectionTimeoutInMilliSec;
                }

                if (configuration.OperationTimeoutInMilliSec != 0)
                {
                    _configOption.SyncTimeout = configuration.OperationTimeoutInMilliSec;
                }
            }
            CreateMultiplexer();
        }

        public IDatabase Connection
        {
            get { return _redisMultiplexer.GetDatabase(_configOption.DefaultDatabase ?? _configuration.DatabaseId); }
        }

        public void ForceReconnect()
        {
            DateTimeOffset currentErrorTime = DateTimeOffset.UtcNow;
            TimeSpan errorTimeDiff = currentErrorTime - lastErrorTime;
            lastErrorTime = currentErrorTime;
            if (errorTimeDiff < ReconnectErrorFrequency)
            {
                var previousReconnect = lastReconnectTime;
                var elapsedTime = DateTimeOffset.UtcNow - previousReconnect;
                
                // If mulitple threads call ForceReconnect at the same time, we only want to honor one of them. 
                if (elapsedTime > ReconnectFrequency)
                {
                    lock (reconnectLock)
                    {
                        elapsedTime = DateTimeOffset.UtcNow - lastReconnectTime;
                        if (elapsedTime < ReconnectFrequency)
                        {
                            return; // Some other thread made it through the check and the lock, so nothing to do. 
                        }

                        var oldMultiplexer = _redisMultiplexer;
                        CloseMultiplexer(oldMultiplexer);
                        CreateMultiplexer();
                    }
                }
            }
        }

        private void CreateMultiplexer()
        {
            if (LogUtility.logger == null)
            {
                _redisMultiplexer = ConnectionMultiplexer.Connect(_configOption);
            }
            else
            {
                _redisMultiplexer = ConnectionMultiplexer.Connect(_configOption, LogUtility.logger);
            }
            lastReconnectTime = DateTimeOffset.UtcNow;
        }

        private void CloseMultiplexer(ConnectionMultiplexer oldMultiplexer)
        {
            if (oldMultiplexer != null)
            {
                try
                {
                    oldMultiplexer.Close();
                }
                catch (Exception)
                {
                    // Example error condition: if accessing old.Value causes a connection attempt and that fails. 
                }
            }
        }

    }
}
