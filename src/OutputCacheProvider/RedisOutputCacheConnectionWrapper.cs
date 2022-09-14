//
// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.
//

using System;
using System.IO;
using System.Web.Caching;

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

            // only single object of RedisSharedConnection will be created and then reused
            if (sharedConnection == null)
            {
                lock (lockForSharedConnection)
                {
                    if (sharedConnection == null)
                    {
                        sharedConnection = new RedisSharedConnection(configuration);
                    }
                }
            }
            redisConnection = new StackExchangeClientConnection(configuration, sharedConnection);
        }

        /*-------Start of Add operation-----------------------------------------------------------------------------------------------------------------------------------------------*/
        // KEYS = { key }
        // ARGV = { page data, expiry time in miliseconds } 
        // retArray = { page data from cache or new }
        static readonly string addScript = (@"
                    local retVal = redis.call('GET',KEYS[1])
                    if retVal == false then
                       redis.call('PSETEX',KEYS[1],ARGV[2],ARGV[1])
                       retVal = ARGV[1]
                    end
                    return retVal
                    ");

        public object Add(string key, object entry, DateTime utcExpiry)
        {
            key = GetKeyForRedis(key);
            TimeSpan expiryTime = utcExpiry - DateTime.UtcNow;
            string[] keyArgs = new string[] { key };
            object[] valueArgs = new object[] {
                SerializeOutputCacheEntry(entry),
                (long) expiryTime.TotalMilliseconds };

            object rowDataFromRedis = redisConnection.Eval(addScript, keyArgs, valueArgs);
            return DeserializeOutputCacheEntry((byte[])rowDataFromRedis);
        }

        /*-------End of Add operation-----------------------------------------------------------------------------------------------------------------------------------------------*/

        public void Set(string key, object entry, DateTime utcExpiry)
        {
            key = GetKeyForRedis(key);

            MemoryStream ms = new MemoryStream();
            OutputCache.Serialize(ms, entry);
            byte[] data = ms.ToArray();

            redisConnection.Set(key, data, utcExpiry);
        }

        public object Get(string key)
        {
            key = GetKeyForRedis(key);

            byte[] data = redisConnection.Get(key);
            return DeserializeOutputCacheEntry(data);
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
        private byte[] SerializeOutputCacheEntry(object outputCacheEntry)
        {
            try
            {
                MemoryStream ms = new MemoryStream();
                OutputCache.Serialize(ms, outputCacheEntry);
                return ms.ToArray();
            }
            catch (ArgumentException)
            {
                LogUtility.LogWarning("{0} is not one of the specified output-cache types.", outputCacheEntry);
                return null;
            }
        }

        private object DeserializeOutputCacheEntry(byte[] serializedOutputCacheEntry)
        {
            try
            {
                MemoryStream ms = new MemoryStream(serializedOutputCacheEntry);
                return OutputCache.Deserialize(ms);
            }
            catch (ArgumentException)
            {
                LogUtility.LogWarning("The output cache entry is not one of the specified output-cache types.");
                return null;
            }
        }
    }
}
