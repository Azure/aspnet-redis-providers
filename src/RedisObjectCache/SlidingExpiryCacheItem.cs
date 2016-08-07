using System;

namespace Microsoft.Web.Redis
{
	[Serializable]
	internal class SlidingExpiryCacheItem
	{
		public SlidingExpiryCacheItem(object value, TimeSpan slidingExpiration)
		{
			Value = value;
			SlidingExpiration = slidingExpiration;
		}

		public object Value { get; }

		public TimeSpan SlidingExpiration { get; }
	}
}