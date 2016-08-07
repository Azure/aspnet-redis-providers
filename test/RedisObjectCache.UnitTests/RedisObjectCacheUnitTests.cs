using System;
using System.Collections.Specialized;
using System.Runtime.Caching;
using FakeItEasy;
using Xunit;

namespace Microsoft.Web.Redis.UnitTests
{
	public class RedisObjectCacheUnitTests
	{
		[Fact]
		public void TryGet()
		{
			var fake = A.Fake<IObjectCacheConnection>();
			A.CallTo(() => fake.Get("key1", null)).Returns(new ArgumentException("foo"));
			RedisObjectCache cache = new RedisObjectCache("unitTest", new NameValueCollection());
			cache.cache = fake;
			var obj = cache.Get("key1");
			Assert.IsType<ArgumentException>(obj);
		}

		[Fact]
		public void GetWithSlidingExpiration()
		{
			var fake = A.Fake<IObjectCacheConnection>();
			A.CallTo(() => fake.Get("key1", null)).Returns(new SlidingExpiryCacheItem("foo", TimeSpan.FromMinutes(1)));
			RedisObjectCache cache = new RedisObjectCache("unitTest", new NameValueCollection());
			cache.cache = fake;
			var obj = cache.Get("key1");
			Assert.Equal("foo", obj);
			A.CallTo(() => fake.ResetExpiry("key1", A<DateTime>.Ignored, null)).MustHaveHappened();
		}

		[Fact]
		public void TryAdd()
		{
			var fake = A.Fake<IObjectCacheConnection>();
			DateTime utcExpiry = DateTime.Now;
			A.CallTo(() => fake.AddOrGetExisting("key1", "object", A<DateTime>.Ignored, null)).Returns(null);
			RedisObjectCache cache = new RedisObjectCache("unitTest", new NameValueCollection());
			cache.cache = fake;
			var obj = cache.Add("key1", "object", utcExpiry);
			Assert.True(obj);
		}

		[Fact]
		public void AddWithSlidingExperation()
		{
			var fake = A.Fake<IObjectCacheConnection>();
			A.CallTo(() => fake.AddOrGetExisting("key1", A<SlidingExpiryCacheItem>.Ignored, A<DateTime>.Ignored, null)).Returns(null);
			RedisObjectCache cache = new RedisObjectCache("unitTest", new NameValueCollection());
			cache.cache = fake;
			var obj = cache.Add("key1", "object", new CacheItemPolicy { SlidingExpiration = TimeSpan.FromMinutes(5) });
			//Assert.IsType<bool>(obj);
			//Assert.True(obj);
			A.CallTo(() => fake.AddOrGetExisting("key1", A<SlidingExpiryCacheItem>.That.Matches(s => (string)s.Value == "object"), A<DateTime>.Ignored, null)).MustHaveHappened();
		}

		[Fact]
		public void TrySet()
		{
			var fake = A.Fake<IObjectCacheConnection>();
			A.CallTo(() => fake.Set("key1", "object", A<DateTime>.Ignored, null));
			DateTime utcExpiry = DateTime.Now;
			RedisObjectCache cache = new RedisObjectCache("unitTest", new NameValueCollection());
			cache.cache = fake;
			cache.Set("key1", "object", utcExpiry);
			A.CallTo(() => fake.Set("key1", "object", A<DateTime>.Ignored, null)).MustHaveHappened();
		}
		[Fact]
		public void TryRemove()
		{
			var fake = A.Fake<IObjectCacheConnection>();
			A.CallTo(() => fake.Remove("key1", null));
			DateTime utcExpiry = DateTime.Now;
			RedisObjectCache cache = new RedisObjectCache("unitTest", new NameValueCollection());
			cache.cache = fake;
			cache.Remove("key1");
			A.CallTo(() => fake.Remove("key1", null)).MustHaveHappened();
		}
		//[Fact]
		//public void TryInitialize()
		//{
		//	var fake = A.Fake<IObjectCacheConnection>();
		//	RedisObjectCache cache = new RedisObjectCache("unitTest", new NameValueCollection());
		//	cache.cache = fake;
		//	NameValueCollection config = new NameValueCollection();
		//	config.Add("host", "localhost");
		//	config.Add("port", "1234");
		//	config.Add("accessKey", "hello world");
		//	config.Add("ssl", "true");
		//	cache.Initialize("name", config);

		//	Assert.Equal(RedisOutputCacheProvider.configuration.Host, "localhost");
		//	Assert.Equal(RedisOutputCacheProvider.configuration.Port, 1234);
		//	Assert.Equal(RedisOutputCacheProvider.configuration.AccessKey, "hello world");
		//	Assert.Equal(RedisOutputCacheProvider.configuration.UseSsl, true);
		//}
	}
}
