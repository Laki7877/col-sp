using System;
using System.Runtime.Caching;

namespace Colsp.Helpers
{
	public static class CacheHelper
	{
		public static object Get(string key)
		{
			var memoryCache = MemoryCache.Default;
			return memoryCache.Get(key);
		}

		public static bool Add(string key, object value)
		{
			var memoryCache = MemoryCache.Default;
			return memoryCache.Add(key, value, DateTimeOffset.UtcNow.AddMinutes(ConfigHelper.Settings.Cache.Expire.Value));
		}

		public static void Delete(string key)
		{
			var memoryCache = MemoryCache.Default;
			if (memoryCache.Contains(key))
			{
				memoryCache.Remove(key);
			}
		}
	}
}