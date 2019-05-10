using System;
using StackExchange.Redis;

namespace Microsoft.Web.Redis
{
    internal class ConnectionMultiplexerFactory : IConnectionMultiplexerFactory
    {
        private readonly ConfigurationOptions _configOption;

        internal static DateTimeOffset lastReconnectTime = DateTimeOffset.MinValue;
        internal static DateTimeOffset firstErrorTime = DateTimeOffset.MinValue;
        internal static DateTimeOffset previousErrorTime = DateTimeOffset.MinValue;
        static readonly object reconnectLock = new object();
        internal static TimeSpan ReconnectFrequency = TimeSpan.FromSeconds(60);
        internal static TimeSpan ReconnectErrorThreshold = TimeSpan.FromSeconds(30);

        public ConnectionMultiplexerFactory(ProviderConfiguration configuration)
        {
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
        }

        public IConnectionMultiplexer CreateMultiplexer()
        {
            lastReconnectTime = DateTimeOffset.UtcNow;
            return LogUtility.logger == null
                ? ConnectionMultiplexer.Connect(_configOption)
                : ConnectionMultiplexer.Connect(_configOption, LogUtility.logger);
        }

        private static void CloseMultiplexer(IConnectionMultiplexer oldMultiplexer)
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

        public IConnectionMultiplexer RestartMultiplexer(IConnectionMultiplexer connectionMultiplexer)
        {
            var previousReconnect = lastReconnectTime;
            var elapsedSinceLastReconnect = DateTimeOffset.UtcNow - previousReconnect;

            // If mulitple threads call RestartMultiplexer at the same time, we only want to honor one of them. 
            if (elapsedSinceLastReconnect > ReconnectFrequency)
            {
                lock (reconnectLock)
                {
                    var utcNow = DateTimeOffset.UtcNow;
                    elapsedSinceLastReconnect = utcNow - lastReconnectTime;

                    if (elapsedSinceLastReconnect < ReconnectFrequency)
                    {
                        return connectionMultiplexer; // Some other thread made it through the check and the lock, so nothing to do. 
                    }

                    if (firstErrorTime == DateTimeOffset.MinValue)
                    {
                        // We got error first time after last reconnect
                        firstErrorTime = utcNow;
                        previousErrorTime = utcNow;
                        return connectionMultiplexer;
                    }

                    var elapsedSinceFirstError = utcNow - firstErrorTime;
                    var elapsedSinceMostRecentError = utcNow - previousErrorTime;
                    previousErrorTime = utcNow;

                    if ((elapsedSinceFirstError >= ReconnectErrorThreshold) && (elapsedSinceMostRecentError <= ReconnectErrorThreshold))
                    {
                        LogUtility.LogInfo($"RestartMultiplexer: now: {utcNow.ToString()}");
                        LogUtility.LogInfo($"RestartMultiplexer: elapsedSinceLastReconnect: {elapsedSinceLastReconnect.ToString()}, ReconnectFrequency: {ReconnectFrequency.ToString()}");
                        LogUtility.LogInfo($"RestartMultiplexer: elapsedSinceFirstError: {elapsedSinceFirstError.ToString()}, elapsedSinceMostRecentError: {elapsedSinceMostRecentError.ToString()}, ReconnectErrorThreshold: {ReconnectErrorThreshold.ToString()}");

                        firstErrorTime = DateTimeOffset.MinValue;
                        previousErrorTime = DateTimeOffset.MinValue;

                        //var oldMultiplexer = _redisMultiplexer;
                        CloseMultiplexer(connectionMultiplexer);
                        return CreateMultiplexer();
                    }

                    return connectionMultiplexer;
                }
            }
            return connectionMultiplexer;
        }
    }
}