using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Web.Redis;
using System.Collections.Specialized;


namespace Microsoft.Web.Redis.FunctionalTests
{
    public class RedisOutputCacheFunctionalTests
    {
        [Fact]
        public void GetWithoutSetTest()
        {
            using (RedisServer Server = new RedisServer())
            {
                RedisOutputCacheProvider provider = new RedisOutputCacheProvider();
                NameValueCollection config = new NameValueCollection();
                config.Add("ssl", "false");
                DateTime utxExpiry = DateTime.UtcNow.AddMinutes(3);
                provider.Initialize("name", config);
                Assert.Equal(null, provider.Get("key1"));
            }
        }
        
        [Fact]
        public void SetGetTest()
        {
            using (RedisServer Server = new RedisServer())
            {
                RedisOutputCacheProvider provider = new RedisOutputCacheProvider();
                NameValueCollection config = new NameValueCollection();
                config.Add("ssl", "false");
                DateTime utxExpiry = DateTime.UtcNow.AddMinutes(3); 
                
                provider.Initialize("name", config);
                provider.Set("key1", "data1", utxExpiry);
                object data = provider.Get("key1");
                Assert.Equal("data1", data);
            }
        }

        [Fact]
        public void AddWithExistingSetTest()
        {
            using (RedisServer Server = new RedisServer())
            {
                RedisOutputCacheProvider provider = new RedisOutputCacheProvider();
                NameValueCollection config = new NameValueCollection();
                config.Add("ssl", "false");
                DateTime utxExpiry = DateTime.UtcNow.AddMinutes(3);

                provider.Initialize("name", config);
                provider.Set("key1", "data1", utxExpiry);
                provider.Add("key1", "data3", utxExpiry);
                object data = provider.Get("key1");
                Assert.Equal("data1", data);
            }            
        }

        [Fact]
        public void AddWithoutSetTest()
        {
            using (RedisServer Server = new RedisServer())
            {
                RedisOutputCacheProvider provider = new RedisOutputCacheProvider();
                NameValueCollection config = new NameValueCollection();
                config.Add("ssl", "false");
                DateTime utxExpiry = DateTime.UtcNow.AddMinutes(3);

                provider.Initialize("name", config);
                provider.Add("key1", "data1", utxExpiry);
                object data = provider.Get("key1");
                Assert.Equal("data1", data);
            }
        }

        [Fact]
        public void AddWhenSetExpiresTest()
        {
            using (RedisServer Server = new RedisServer())
            {
                RedisOutputCacheProvider provider = new RedisOutputCacheProvider();
                NameValueCollection config = new NameValueCollection();
                config.Add("ssl", "false");
                DateTime utxExpiry = DateTime.UtcNow.AddSeconds(1);
                
                provider.Initialize("name", config);
                provider.Set("key1", "data1", utxExpiry);
                object data = provider.Get("key1");
                Assert.Equal("data1", data);

                // Wait for 1.1 seconds so that data will expire
                System.Threading.Thread.Sleep(1100);
                utxExpiry = DateTime.UtcNow.AddMinutes(3);
                provider.Add("key1", "data3", utxExpiry);
                data = provider.Get("key1");
                Assert.Equal("data3", data);
            }
        }

        [Fact]
        public void RemoveWithoutSetTest()
        {
            using (RedisServer Server = new RedisServer())
            {
                RedisOutputCacheProvider provider = new RedisOutputCacheProvider();
                NameValueCollection config = new NameValueCollection();
                config.Add("ssl", "false");
                DateTime utxExpiry = DateTime.UtcNow.AddMinutes(3);

                provider.Initialize("name", config);
                provider.Remove("key1");
                object data = provider.Get("key1");
                Assert.Equal(null, data);
            }
        }

        [Fact]
        public void RemoveTest()
        {
            using (RedisServer Server = new RedisServer())
            {
                RedisOutputCacheProvider provider = new RedisOutputCacheProvider();
                NameValueCollection config = new NameValueCollection();
                config.Add("ssl", "false");
                DateTime utxExpiry = DateTime.UtcNow.AddMinutes(3);

                provider.Initialize("name", config);
                provider.Set("key1", "data1", utxExpiry);
                provider.Remove("key1");
                object data = provider.Get("key1");
                Assert.Equal(null, data);
            }
        }

        [Fact]
        public void ExpiryTest()
        {
            using (RedisServer Server = new RedisServer())
            {
                RedisOutputCacheProvider provider = new RedisOutputCacheProvider();
                NameValueCollection config = new NameValueCollection();
                config.Add("ssl", "false");
                DateTime utxExpiry = DateTime.UtcNow.AddSeconds(1);

                provider.Initialize("name", config);
                provider.Set("key1", "data1", utxExpiry);
                // Wait for 1.1 seconds so that data will expire
                System.Threading.Thread.Sleep(1100);
                object data = provider.Get("key1");
                Assert.Equal(null, data);
            }
        }
    }
}
