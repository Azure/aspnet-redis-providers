//
// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.
//

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
                provider.Initialize("name", config);
                
                DateTime utxExpiry = DateTime.UtcNow.AddMinutes(3);
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
                provider.Initialize("name", config);
                
                DateTime utxExpiry = DateTime.UtcNow.AddMinutes(3); 
                provider.Set("key2", "data2", utxExpiry);
                object data = provider.Get("key2");
                Assert.Equal("data2", data);
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
                provider.Initialize("name", config);
                
                DateTime utxExpiry = DateTime.UtcNow.AddMinutes(3);
                provider.Set("key3", "data3", utxExpiry);
                provider.Add("key3", "data3.1", utxExpiry);
                object data = provider.Get("key3");
                Assert.Equal("data3", data);
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
                provider.Initialize("name", config);
                
                DateTime utxExpiry = DateTime.UtcNow.AddMinutes(3);
                provider.Add("key4", "data4", utxExpiry);
                object data = provider.Get("key4");
                Assert.Equal("data4", data);
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
                provider.Initialize("name", config);

                DateTime utxExpiry = DateTime.UtcNow.AddSeconds(1);
                provider.Set("key5", "data5", utxExpiry);
                object data = provider.Get("key5");
                Assert.Equal("data5", data);

                // Wait for 1.1 seconds so that data will expire
                System.Threading.Thread.Sleep(1100);
                utxExpiry = DateTime.UtcNow.AddMinutes(3);
                provider.Add("key5", "data5.1", utxExpiry);
                data = provider.Get("key5");
                Assert.Equal("data5.1", data);
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
                provider.Initialize("name", config);
                provider.Remove("key6");
                object data = provider.Get("key6");
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
                provider.Initialize("name", config);
                
                DateTime utxExpiry = DateTime.UtcNow.AddMinutes(3);
                provider.Set("key7", "data7", utxExpiry);
                provider.Remove("key7");
                object data = provider.Get("key7");
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
                provider.Initialize("name", config);
                
                DateTime utxExpiry = DateTime.UtcNow.AddSeconds(1);
                provider.Set("key8", "data8", utxExpiry);
                // Wait for 1.1 seconds so that data will expire
                System.Threading.Thread.Sleep(1100);
                object data = provider.Get("key8");
                Assert.Equal(null, data);
            }
        }

        [Fact]
        public void AddScriptFixForExpiryTest()
        {
            using (RedisServer Server = new RedisServer())
            {
                RedisOutputCacheProvider provider = new RedisOutputCacheProvider();
                NameValueCollection config = new NameValueCollection();
                config.Add("ssl", "false");
                provider.Initialize("name", config);

                DateTime utxExpiry = DateTime.UtcNow.AddSeconds(1);
                provider.Add("key9", "data9", utxExpiry);
                object data = provider.Get("key9");
                Assert.Equal("data9", data);
                // Wait for 1.1 seconds so that data will expire
                System.Threading.Thread.Sleep(1100);
                data = provider.Get("key9");
                Assert.Equal(null, data);
            }
        }

        
    }
}
