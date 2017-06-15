using System;
using System.Collections.Generic;

namespace Microsoft.Web.Redis
{
    internal class RedisObjectCacheConnectionWrapper : IObjectCacheConnection
    {
        private static Dictionary<string, RedisSharedConnection> sharedConnections = new Dictionary<string, RedisSharedConnection>();
		private static object lockForSharedConnection = new object();
		internal static RedisUtility redisUtility;

		private IRedisClientConnection redisConnection;
		private ProviderConfiguration configuration;

		public RedisObjectCacheConnectionWrapper(ProviderConfiguration configuration, string name)
		{
            this.configuration = configuration;

            // Shared connection is created by server when it starts. don't want to lock everytime when check == null.
            // so that is why pool == null exists twice.
            if (!sharedConnections.ContainsKey(name))
            {
                lock (lockForSharedConnection)
                {
					if (!sharedConnections.ContainsKey(name))
                    {
						sharedConnections[name] = new RedisSharedConnection(configuration, () => new StackExchangeClientConnection(configuration));
						redisUtility = new RedisUtility(configuration);
					}
                }
            }
            redisConnection = sharedConnections[name].TryGetConnection();
        }

		/*-------Start of Add operation-----------------------------------------------------------------------------------------------------------------------------------------------*/
		// KEYS = { key }
		// ARGV = { page data, expiry time in miliseconds } 
		// retArray = { page data from cache or new }
		static readonly string addScript = (@"
                    local retVal = redis.call('GET',KEYS[1])
                    if retVal == false then
                       redis.call('PSETEX',KEYS[1],ARGV[2],ARGV[1])
                       retVal = nil
                    end
                    return retVal
                    ");

		public object AddOrGetExisting(string key, object entry, DateTime utcExpiry, string regionName = null)
        {
            key = GetKeyForRedis(key, regionName);
            TimeSpan expiryTime = utcExpiry - DateTime.UtcNow;
            string[] keyArgs = new string[] { key };
            object[] valueArgs = new object[] { redisUtility.GetBytesFromObject(entry), (long)expiryTime.TotalMilliseconds };

            object rowDataFromRedis = redisConnection.Eval(addScript, keyArgs, valueArgs);
            return redisUtility.GetObjectFromBytes(redisConnection.GetOutputCacheDataFromResult(rowDataFromRedis));
        }

		/*-------End of Add operation-----------------------------------------------------------------------------------------------------------------------------------------------*/

		public void ResetExpiry(string key, DateTime utcExpiry, string regionName = null)
	    {
			key = GetKeyForRedis(key, regionName);
			redisConnection.Expiry(key, (int)(utcExpiry - DateTime.UtcNow).TotalSeconds);
	    }

	    public bool Exists(string key, string regionName = null)
	    {
		    key = GetKeyForRedis(key, regionName);
		    return redisConnection.Exists(key);
	    }

        public void Set(string key, object entry, DateTime utcExpiry, string regionName = null)
        {
            key = GetKeyForRedis(key, regionName);
            byte[] data = redisUtility.GetBytesFromObject(entry);
            redisConnection.Set(key, data, utcExpiry);
        }

        public object Get(string key, string regionName = null)
        {
            key = GetKeyForRedis(key, regionName);
            byte[] data = redisConnection.Get(key);
            return redisUtility.GetObjectFromBytes(data);
        }

        public object Remove(string key, string regionName = null)
        {
            key = GetKeyForRedis(key, regionName);
            object value = Get(key, regionName);
            redisConnection.Remove(key);
            return value;
        }

        private string GetKeyForRedis(string key, string regionName = null)
        {
            if (regionName != null)
                regionName += "_";

            return configuration.ApplicationName + "_" + regionName + key;
        }
    }
}
