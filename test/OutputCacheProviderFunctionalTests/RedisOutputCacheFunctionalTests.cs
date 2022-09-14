//
// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.
//

using System;
using Xunit;
using System.Collections.Specialized;
using System.Web.Caching;

namespace Microsoft.Web.Redis.FunctionalTests
{
    public class RedisOutputCacheFunctionalTests
    {
        [Fact()]
        public void GetWithoutSetTest()
        {
            using (RedisServer Server = new RedisServer())
            {
                RedisOutputCacheProvider provider = new RedisOutputCacheProvider();
                NameValueCollection config = new NameValueCollection();
                config.Add("ssl", "false");
                provider.Initialize("name", config);
                
                DateTime utxExpiry = DateTime.UtcNow.AddMinutes(3);
                Assert.Null(provider.Get("key1"));
            }
        }
        
        [Fact()]
        public void SetGetTest()
        {
            using (RedisServer Server = new RedisServer())
            {
                RedisOutputCacheProvider provider = new RedisOutputCacheProvider();
                NameValueCollection config = new NameValueCollection();
                config.Add("ssl", "false");
                provider.Initialize("name", config);
                
                DateTime utxExpiry = DateTime.UtcNow.AddMinutes(3); 
                provider.Set("key2", new FileResponseElement("data2", 0, 0), utxExpiry);
                object data = provider.Get("key2");
                Assert.Equal("data2", ((FileResponseElement)data).Path);
            }
        }

        [Fact()]
        public void AddWithExistingSetTest()
        {
            using (RedisServer Server = new RedisServer())
            {
                RedisOutputCacheProvider provider = new RedisOutputCacheProvider();
                NameValueCollection config = new NameValueCollection();
                config.Add("ssl", "false");
                provider.Initialize("name", config);
                
                DateTime utxExpiry = DateTime.UtcNow.AddMinutes(3);
                provider.Set("key3", new FileResponseElement("data3", 0, 0), utxExpiry);
                provider.Add("key3", new FileResponseElement("data3.1", 0, 0), utxExpiry);
                object data = provider.Get("key3");
                Assert.Equal("data3", ((FileResponseElement)data).Path);
            }            
        }

        [Fact()]
        public void AddWithoutSetTest()
        {
            using (RedisServer Server = new RedisServer())
            {
                RedisOutputCacheProvider provider = new RedisOutputCacheProvider();
                NameValueCollection config = new NameValueCollection();
                config.Add("ssl", "false");
                provider.Initialize("name", config);
                
                DateTime utxExpiry = DateTime.UtcNow.AddMinutes(3);
                provider.Add("key4", new FileResponseElement("data4", 0, 0), utxExpiry);
                object data = provider.Get("key4");
                Assert.Equal("data4", ((FileResponseElement)data).Path);
            }
        }

        [Fact()]
        public void AddWhenSetExpiresTest()
        {
            using (RedisServer Server = new RedisServer())
            {
                RedisOutputCacheProvider provider = new RedisOutputCacheProvider();
                NameValueCollection config = new NameValueCollection();
                config.Add("ssl", "false");
                provider.Initialize("name", config);

                DateTime utxExpiry = DateTime.UtcNow.AddSeconds(1);
                provider.Set("key5", new FileResponseElement("data5", 0, 0), utxExpiry);
                object data = provider.Get("key5");
                Assert.Equal("data5", ((FileResponseElement)data).Path);

                // Wait for 1.1 seconds so that data will expire
                System.Threading.Thread.Sleep(1100);
                utxExpiry = DateTime.UtcNow.AddMinutes(3);
                provider.Add("key5", new FileResponseElement("data5.1", 0, 0), utxExpiry);
                data = provider.Get("key5");
                Assert.Equal("data5.1", ((FileResponseElement)data).Path);
            }
        }

        [Fact()]
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
                Assert.Null(data);
            }
        }

        [Fact()]
        public void RemoveTest()
        {
            using (RedisServer Server = new RedisServer())
            {
                RedisOutputCacheProvider provider = new RedisOutputCacheProvider();
                NameValueCollection config = new NameValueCollection();
                config.Add("ssl", "false");
                provider.Initialize("name", config);
                
                DateTime utxExpiry = DateTime.UtcNow.AddMinutes(3);
                provider.Set("key7", new FileResponseElement("data7", 0, 0), utxExpiry);
                provider.Remove("key7");
                object data = provider.Get("key7");
                Assert.Null(data);
            }
        }

        [Fact()]
        public void ExpiryTest()
        {
            using (RedisServer Server = new RedisServer())
            {
                RedisOutputCacheProvider provider = new RedisOutputCacheProvider();
                NameValueCollection config = new NameValueCollection();
                config.Add("ssl", "false");
                provider.Initialize("name", config);
                
                DateTime utxExpiry = DateTime.UtcNow.AddSeconds(1);
                provider.Set("key8", new FileResponseElement("data8", 0, 0), utxExpiry);
                // Wait for 1.1 seconds so that data will expire
                System.Threading.Thread.Sleep(1100);
                object data = provider.Get("key8");
                Assert.Null(data);
            }
        }

        [Fact()]
        public void AddScriptFixForExpiryTest()
        {
            using (RedisServer Server = new RedisServer())
            {
                RedisOutputCacheProvider provider = new RedisOutputCacheProvider();
                NameValueCollection config = new NameValueCollection();
                config.Add("ssl", "false");
                provider.Initialize("name", config);

                DateTime utxExpiry = DateTime.UtcNow.AddSeconds(1);
                provider.Add("key9", new FileResponseElement("data9", 0, 0), utxExpiry);
                object data = provider.Get("key9");
                Assert.Equal("data9", ((FileResponseElement)data).Path);
                // Wait for 1.1 seconds so that data will expire
                System.Threading.Thread.Sleep(1100);
                data = provider.Get("key9");
                Assert.Null(data);
            }
        }

        
    }
}
