using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Globalization;
using System.Runtime.Caching;

namespace Microsoft.Web.Redis
{
	public class RedisObjectCache : ObjectCache
	{
		// static holder for instance, need to use lambda to construct since constructor private
		private static readonly Lazy<RedisObjectCache> Instance = new Lazy<RedisObjectCache>(() => new RedisObjectCache());

		private static readonly TimeSpan OneYear = TimeSpan.FromDays(365);

		internal IObjectCacheConnection cache;
		private readonly ProviderConfiguration configuration;

		private RedisObjectCache()
			: this("Default")
		{
		}

		public RedisObjectCache(string name)
			: this (name, GetConfig(name))
		{
		}

		public RedisObjectCache(string name, string connectionString)
			: this (name, new NameValueCollection { { "connectionString", connectionString } })
		{
		}

		internal RedisObjectCache(string name, NameValueCollection config)
		{
			if (string.IsNullOrEmpty(name))
				throw new ArgumentNullException(nameof(name));
			if (config == null)
				throw new ArgumentNullException(nameof(config));

			Name = name;
			configuration = ProviderConfiguration.ProviderConfigurationForObjectCache(config, name);
			cache = new RedisObjectCacheConnectionWrapper(configuration, name);
		}

		private static NameValueCollection GetConfig(string name)
		{
			if (string.IsNullOrEmpty(name))
				throw new ArgumentNullException(nameof(name));

			ProviderSettings providerSettings = RedisObjectCacheConfiguration.Instance.Caches[name];

			if (providerSettings == null)
				throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, RedisProviderResource.NoConfigForCache, name), nameof(name));

			NameValueCollection pars = providerSettings.Parameters;
			NameValueCollection config = new NameValueCollection(pars.Count, StringComparer.Ordinal);
			foreach (string key in pars)
				config[key] = pars[key];
			return config;
		}

		public static RedisObjectCache Default => Instance.Value;

		public override DefaultCacheCapabilities DefaultCacheCapabilities { get; } = DefaultCacheCapabilities.AbsoluteExpirations
																				   | DefaultCacheCapabilities.SlidingExpirations
																				   | DefaultCacheCapabilities.OutOfProcessProvider
																				   | DefaultCacheCapabilities.CacheRegions;

		public override string Name { get; }

		public override object this[string key]
		{
			get { return Get(key); }
			set { Set(key, value, InfiniteAbsoluteExpiration); }
		}

		public override bool Contains(string key, string regionName = null)
		{
			return cache.Exists(key, regionName);
		}

		public override object AddOrGetExisting(string key, object value, DateTimeOffset absoluteExpiration, string regionName = null)
		{
			CacheItemPolicy policy = new CacheItemPolicy { AbsoluteExpiration = absoluteExpiration };
			return AddOrGetExisting(key, value, policy, regionName);
		}

		public override CacheItem AddOrGetExisting(CacheItem value, CacheItemPolicy policy)
		{
			return new CacheItem(value.Key, AddOrGetExisting(value.Key, value.Value, policy), value.RegionName);
		}

		public override object AddOrGetExisting(string key, object value, CacheItemPolicy policy, string regionName = null)
		{
			ValidatePolicy(policy);
			try
			{
				DateTime utcExpiry;
				if (policy.SlidingExpiration != NoSlidingExpiration)
				{
					utcExpiry = DateTime.UtcNow + policy.SlidingExpiration;
					value = new SlidingExpiryCacheItem(value, policy.SlidingExpiration);
				}
				else
					utcExpiry = policy.AbsoluteExpiration.UtcDateTime;

				object oldValue = cache.AddOrGetExisting(key, value, utcExpiry, regionName);
				oldValue = HandleSlidingExpiry(key, oldValue, regionName);
				return oldValue;
			}
			catch (Exception e)
			{
				LogUtility.LogError("Error in AddOrGetExisting: " + e.Message);
				if (configuration.ThrowOnError)
					throw;
			}
			return null;
		}

		public override CacheItem GetCacheItem(string key, string regionName = null)
		{
			return new CacheItem(key, Get(key, regionName), regionName);
		}

		public override object Get(string key, string regionName = null)
		{
			try
			{
				object value = cache.Get(key, regionName);
				value = HandleSlidingExpiry(key, value, regionName);
				return value;
			}
			catch (Exception e)
			{
				LogUtility.LogError("Error in Get: " + e.Message);
				if (configuration.ThrowOnError)
					throw;
			}
			return null;
		}

		public override void Set(string key, object value, DateTimeOffset absoluteExpiration, string regionName = null)
		{
			CacheItemPolicy policy = new CacheItemPolicy { AbsoluteExpiration = absoluteExpiration };
			Set(key, value, policy, regionName);
		}
		public override void Set(CacheItem item, CacheItemPolicy policy)
		{
			Set(item.Key, item.Value, policy, item.RegionName);
		}

		public override void Set(string key, object value, CacheItemPolicy policy, string regionName = null)
		{
			ValidatePolicy(policy);
			try
			{
				DateTime utcExpiry;
				if (policy.SlidingExpiration != NoSlidingExpiration)
				{
					utcExpiry = DateTime.UtcNow + policy.SlidingExpiration;
					value = new SlidingExpiryCacheItem(value, policy.SlidingExpiration);
				}
				else
					utcExpiry = policy.AbsoluteExpiration.UtcDateTime;

				cache.Set(key, value, utcExpiry, regionName);
			}
			catch (Exception e)
			{
				LogUtility.LogError("Error in Set: " + e.Message);
				if (configuration.ThrowOnError)
					throw;
			}
		}

		public override object Remove(string key, string regionName = null)
		{
			try
			{
				return cache.Remove(key, regionName);
			}
			catch (Exception e)
			{
				LogUtility.LogError("Error in Remove: " + e.Message);
				if (configuration.ThrowOnError)
					throw;
			}
			return null;
		}

		private object HandleSlidingExpiry(string key, object value, string regionName)
		{
			SlidingExpiryCacheItem item = value as SlidingExpiryCacheItem;

			if (item == null)
				return value;

			cache.ResetExpiry(key, DateTime.UtcNow + item.SlidingExpiration, regionName);
			return item.Value;
		}

		private void ValidatePolicy(CacheItemPolicy policy)
		{
			if (policy.RemovedCallback != null)
				throw new NotSupportedException(RedisProviderResource.RemovedCallbackNotSupported);

			if (policy.UpdateCallback != null)
				throw new NotSupportedException(RedisProviderResource.UpdateCallbackNotSupported);

			if (policy.ChangeMonitors.Count != 0)
				throw new NotSupportedException(RedisProviderResource.ChangeMonitorsNotSupported);

			if (policy.Priority == CacheItemPriority.NotRemovable)
				throw new NotSupportedException(RedisProviderResource.NotRemovableNotSupported);

			// copied from MemoryCache.ValidatPolicy()
			if (policy.AbsoluteExpiration != InfiniteAbsoluteExpiration && policy.SlidingExpiration != NoSlidingExpiration)
				throw new ArgumentException(RedisProviderResource.InvalidExpirationPolicy , nameof(policy));

			if (policy.SlidingExpiration < NoSlidingExpiration || OneYear < policy.SlidingExpiration)
				throw new ArgumentOutOfRangeException(nameof(policy));

			if (policy.Priority != CacheItemPriority.Default && policy.Priority != CacheItemPriority.NotRemovable)
				throw new ArgumentOutOfRangeException(nameof(policy));
		}

		#region Not Implemented

		public override IDictionary<string, object> GetValues(IEnumerable<string> keys, string regionName = null)
		{
			throw new NotImplementedException();
		}

		public override long GetCount(string regionName = null)
		{
			throw new NotImplementedException();
		}

		protected override IEnumerator<KeyValuePair<string, object>> GetEnumerator()
		{
			throw new NotImplementedException();
		}

		public override CacheEntryChangeMonitor CreateCacheEntryChangeMonitor(IEnumerable<string> keys, string regionName = null)
		{
			throw new NotSupportedException(RedisProviderResource.ChangeMonitorsNotSupported);
		}

		#endregion
	}
}
