using System;
using System.Collections.Specialized;
using System.Runtime.Caching;
using Xunit;

namespace Microsoft.Web.Redis.FunctionalTests
{
	public class RedisObjectCacheFunctionalTests
	{
		[Fact]
		public void NameTest()
		{
			NameValueCollection config = new NameValueCollection();
			config.Add("ssl", "false");
			RedisObjectCache provider = new RedisObjectCache("test", config);

			Assert.Equal("test", provider.Name);
		}

		[Fact]
		public void GetWithoutSetTest()
		{
			using (new RedisServer())
			{
				NameValueCollection config = new NameValueCollection();
				config.Add("ssl", "false");
				RedisObjectCache provider = new RedisObjectCache("test", config);

				Assert.Equal(null, provider.Get("key1"));
			}
		}

		[Fact]
		public void SetGetTest()
		{
			using (new RedisServer())
			{
				NameValueCollection config = new NameValueCollection();
				config.Add("ssl", "false");
				RedisObjectCache provider = new RedisObjectCache("test", config);

				DateTime utxExpiry = DateTime.UtcNow.AddSeconds(3);
				provider.Set("key2", "data2", utxExpiry, "testRegion");
				object data = provider.Get("key2", "testRegion");
				Assert.Equal("data2", data);
			}
		}

		[Fact]
		public void SetGetIndexerTest()
		{
			using (new RedisServer())
			{
				NameValueCollection config = new NameValueCollection();
				config.Add("ssl", "false");
				RedisObjectCache provider = new RedisObjectCache("test", config);

				DateTime utxExpiry = DateTime.UtcNow.AddSeconds(3);
				provider["key2"] = "data2";
				object data = provider["key2"];
				Assert.Equal("data2", data);
			}
		}

		[Fact]
		public void AddWithExistingSetTest()
		{
			using (new RedisServer())
			{
				NameValueCollection config = new NameValueCollection();
				config.Add("ssl", "false");
				RedisObjectCache provider = new RedisObjectCache("test", config);

				DateTime utxExpiry = DateTime.UtcNow.AddSeconds(3);
				provider.Set("key3", "data3", utxExpiry, "testRegion");
				Assert.False(provider.Add("key3", "data3.1", utxExpiry, "testRegion"));
				object data = provider.Get("key3", "testRegion");
				Assert.Equal("data3", data);
			}
		}

		[Fact]
		public void AddWithoutSetTest()
		{
			using (new RedisServer())
			{
				NameValueCollection config = new NameValueCollection();
				config.Add("ssl", "false");

				RedisObjectCache provider = new RedisObjectCache("test", config);

				DateTime utxExpiry = DateTime.UtcNow.AddSeconds(3);
				Assert.True(provider.Add("key4", "data4", utxExpiry, "testRegion"));
				object data = provider.Get("key4", "testRegion");
				Assert.Equal("data4", data);
			}
		}

		[Fact]
		public void AddWhenSetExpiresTest()
		{
			using (new RedisServer())
			{
				NameValueCollection config = new NameValueCollection();
				config.Add("ssl", "false");
				RedisObjectCache provider = new RedisObjectCache("test", config);

				DateTime utxExpiry = DateTime.UtcNow.AddSeconds(1);
				provider.Set("key5", "data5", utxExpiry, "testRegion");
				object data = provider.Get("key5", "testRegion");
				Assert.Equal("data5", data);

				// Wait for 1.1 seconds so that data will expire
				System.Threading.Thread.Sleep(1100);
				utxExpiry = DateTime.UtcNow.AddSeconds(3);
				Assert.True(provider.Add("key5", "data5.1", utxExpiry, "testRegion"));
				data = provider.Get("key5", "testRegion");
				Assert.Equal("data5.1", data);
			}
		}

		[Fact]
		public void RemoveWithoutSetTest()
		{
			using (new RedisServer())
			{
				NameValueCollection config = new NameValueCollection();
				config.Add("ssl", "false");
				RedisObjectCache provider = new RedisObjectCache("test", config);

				provider.Remove("key6", "testRegion");
				object data = provider.Get("key6", "testRegion");
				Assert.Equal(null, data);
			}
		}

		[Fact]
		public void RemoveTest()
		{
			using (new RedisServer())
			{
				NameValueCollection config = new NameValueCollection();
				config.Add("ssl", "false");
				RedisObjectCache provider = new RedisObjectCache("test", config);

				DateTime utxExpiry = DateTime.UtcNow.AddSeconds(3);
				provider.Set("key7", "data7", utxExpiry, "testRegion");
				provider.Remove("key7", "testRegion");
				object data = provider.Get("key7", "testRegion");
				Assert.Equal(null, data);
			}
		}

		[Fact]
		public void ExpiryTest()
		{
			using (new RedisServer())
			{
				NameValueCollection config = new NameValueCollection();
				config.Add("ssl", "false");
				RedisObjectCache provider = new RedisObjectCache("test", config);

				DateTime utxExpiry = DateTime.UtcNow.AddSeconds(1);
				provider.Set("key8", "data8", utxExpiry, "testRegion");
				// Wait for 1.1 seconds so that data will expire
				System.Threading.Thread.Sleep(1100);
				object data = provider.Get("key8", "testRegion");
				Assert.Equal(null, data);
			}
		}

		[Fact]
		public void AddScriptFixForExpiryTest()
		{
			using (new RedisServer())
			{
				NameValueCollection config = new NameValueCollection();
				config.Add("ssl", "false");
				RedisObjectCache provider = new RedisObjectCache("test", config);

				DateTime utxExpiry = DateTime.UtcNow.AddSeconds(1);
				provider.Add("key9", "data9", utxExpiry, "testRegion");
				object data = provider.Get("key9", "testRegion");
				Assert.Equal("data9", data);
				// Wait for 1.1 seconds so that data will expire
				System.Threading.Thread.Sleep(1100);
				data = provider.Get("key9", "testRegion");
				Assert.Equal(null, data);
			}
		}

		[Fact]
		public void SlidingExpiryTest()
		{
			using (new RedisServer())
			{
				NameValueCollection config = new NameValueCollection();
				config.Add("ssl", "false");
				RedisObjectCache provider = new RedisObjectCache("test", config);

				CacheItemPolicy policy = new CacheItemPolicy
				{
					SlidingExpiration = TimeSpan.FromSeconds(4)
				};

				provider.Set("key8", "data8", policy, "testRegion");
				// Wait for 500 seconds, get the data to reset the exiration
				System.Threading.Thread.Sleep(2000);
				object data = provider.Get("key8", "testRegion");
				Assert.Equal("data8", data);

				// 1.1 sec after intial insert, but should still be there.
				System.Threading.Thread.Sleep(2400);
				data = provider.Get("key8", "testRegion");
				Assert.Equal("data8", data);

				// Wait for 1.1 seconds so that data will expire
				System.Threading.Thread.Sleep(4400);
				data = provider.Get("key8", "testRegion");
				Assert.Equal(null, data);

			}
		}

		[Fact]
		public void ContainsTest()
		{
			using (new RedisServer())
			{
				NameValueCollection config = new NameValueCollection();
				config.Add("ssl", "false");
				RedisObjectCache provider = new RedisObjectCache("test", config);

				DateTime utxExpiry = DateTime.UtcNow.AddSeconds(1);
				provider.Set("key10", "data10", utxExpiry, "testRegion");
				Assert.True(provider.Contains("key10", "testRegion"));
			}
		}

		[Fact]
		public void DoesNotContainsTest()
		{
			using (new RedisServer())
			{
				NameValueCollection config = new NameValueCollection();
				config.Add("ssl", "false");
				RedisObjectCache provider = new RedisObjectCache("test", config);

				DateTime utxExpiry = DateTime.UtcNow.AddMinutes(3);
				Assert.False(provider.Contains("key10", "testRegion"));
			}
		}

		[Fact]
		public void RegionsTest()
		{
			using (new RedisServer())
			{
				NameValueCollection config = new NameValueCollection();
				config.Add("ssl", "false");
				RedisObjectCache provider = new RedisObjectCache("test", config);

				DateTime utxExpiry = DateTime.UtcNow.AddMinutes(3);
				provider.Set("key11", "data11.1", utxExpiry, "region1");
				provider.Set("key11", "data11.2", utxExpiry, "region2");

				object region1Data = provider.Get("key11", "region1");
				object region2Data = provider.Get("key11", "region2");
				object regionlessData = provider.Get("key11");

				Assert.Equal("data11.1", region1Data);
				Assert.Equal("data11.2", region2Data);
				Assert.Equal(null, regionlessData);
			}
		}
	}
}