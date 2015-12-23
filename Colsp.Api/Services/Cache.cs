using System;
using System.Runtime.Caching;

namespace Colsp.Api.Services
{
	public static class Cache
	{
		public static object Get(string key)
		{
			// Do not use cache
			if(!Config.Settings.Cache.Use)
			{
				return null;
			}
			var memoryCache = MemoryCache.Default;
			return memoryCache.Get(key);
		}

		public static bool Add(string key, object value)
		{
			// Do not use cache
			if(!Config.Settings.Cache.Use)
			{
				return false;
			}
			var memoryCache = MemoryCache.Default;
			return memoryCache.Add(key, value, DateTimeOffset.UtcNow.AddMinutes(Config.Settings.Cache.Expire));
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