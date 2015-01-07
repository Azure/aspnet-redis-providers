using System;

namespace Microsoft.Web.Redis
{
    internal class RedisOutputCacheConnectionWrapper : IOutputCacheConnection
    {
        internal static RedisSharedConnection sharedConnection;
        static object lockForSharedConnection = new object();

        internal IRedisClientConnection redisConnection;
        ProviderConfiguration configuration;
        
        public RedisOutputCacheConnectionWrapper(ProviderConfiguration configuration)
        {
            this.configuration = configuration;
            
            // Shared connection is created by server when it starts. don't want to lock everytime when check == null.
            // so that is why pool == null exists twice.
            if (sharedConnection == null)
            {
                lock (lockForSharedConnection)
                {
                    if (sharedConnection == null)
                    {
                        sharedConnection = new RedisSharedConnection(configuration,() => new StackExchangeClientConnection(configuration));
                    }
                }
            }
            redisConnection = sharedConnection.TryGetConnection();
        }

/*-------Start of Add operation-----------------------------------------------------------------------------------------------------------------------------------------------*/
        // KEYS = { key }
        // ARGV = { page data, expiry time in miliseconds } 
        // retArray = { page data from cache or new }
        static readonly string addScript = (@"
                    local retVal = redis.call('GET',KEYS[1])
                    if retVal == false then
                       redis.call('SET',KEYS[1], ARGV[1])
                       redis.call('PEXPIRE',KEYS[1], ARGV[2])
                       retVal = ARGV[1]
                    end
                    return retVal
                    ");

        public object Add(string key, object entry, DateTime utcExpiry)
        {
            key = GetKeyForRedis(key);
            TimeSpan expiryTime = utcExpiry - DateTime.UtcNow;
            string[] keyArgs = new string[] { key };
            object[] valueArgs = new object[] { RedisUtility.GetBytesFromObject(entry), expiryTime.TotalMilliseconds };

            object rowDataFromRedis = redisConnection.Eval(addScript, keyArgs, valueArgs);
            return RedisUtility.GetObjectFromBytes(redisConnection.GetOutputCacheDataFromResult(rowDataFromRedis));
        }

/*-------End of Add operation-----------------------------------------------------------------------------------------------------------------------------------------------*/

        public void Set(string key, object entry, DateTime utcExpiry)
        {
            key = GetKeyForRedis(key);
            byte[] data = RedisUtility.GetBytesFromObject(entry);
            redisConnection.Set(key, data, utcExpiry);
        }

        public object Get(string key)
        {
            key = GetKeyForRedis(key);
            byte[] data = redisConnection.Get(key);
            return RedisUtility.GetObjectFromBytes(data);
        }

        public void Remove(string key)
        {
            key = GetKeyForRedis(key);
            redisConnection.Remove(key);
        }

        private string GetKeyForRedis(string key)
        {
            return configuration.ApplicationName + "_" + key;
        }
    }
}
