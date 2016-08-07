using System;
using System.Globalization;

namespace Microsoft.Web.Redis
{
	internal interface IObjectCacheConnection
	{
		void ResetExpiry(string key, DateTime utcExpiry, string regionName = null);
		bool Exists(string key, string regionName = null);
		void Set(string key, object entry, DateTime utcExpiry, string regionName = null);
		object AddOrGetExisting(string key, object entry, DateTime utcExpiry, string regionName = null);
		object Get(string key, string regionName = null);
		object Remove(string key, string regionName = null);
	}
}