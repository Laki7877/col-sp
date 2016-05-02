using System;
using System.Collections.Specialized;
using System.Runtime.Caching;

namespace Colsp.Api.Services
{
	public static class Cache
	{

        public static MemoryCache getActiveCache()
        {
            /*if(Cache.memoryCache == null)
            {
                Cache.memoryCache = new MemoryCache(
                "LoginCache",
                new NameValueCollection
                {
                    { "CacheMemoryLimitMegabytes", "400" },
                    { "PhysicalMemoryLimitPercentage", "10" },
                    { "PollingInterval", "00:00:05" }
                });
            }*/

            return MemoryCache.Default;
        }

		public static object Get(string key)
		{

			// Do not use cache
			if(!Config.Settings.Cache.Use)
			{
				return null;
			}

            //var memoryCache = MemoryCache.Default;
            return Cache.getActiveCache().Get(key);
		}

		public static bool Add(string key, object value)
		{
			// Do not use cache
			if(!Config.Settings.Cache.Use)
			{
				return false;
			}

            //Expire after 1 hour (sliding) after any inactivity 
            CacheItemPolicy policy = new CacheItemPolicy();
            policy.SlidingExpiration = new TimeSpan(1,0,0);
            //var memoryCache = MemoryCache.Default;
            bool addSuc =  Cache.getActiveCache().Add(key, value, policy);

            if(Cache.Get(key) == null)
            {
                return false;
            }
            return addSuc;
		}

        public static CacheItem Probe(string key)
        {
            // Do not use cache
            if (!Config.Settings.Cache.Use)
            {
                return null;
            }

            return Cache.getActiveCache().GetCacheItem(key);
        }


        public static void Clear()
        {
            //var memoryCache = MemoryCache.Default;
            Cache.getActiveCache().Dispose();
        }

        public static void Delete(string key)
		{
            //var memoryCache = MemoryCache.Default;
            if (Cache.getActiveCache().Contains(key))
			{
                Cache.getActiveCache().Remove(key);
            }
		}
	}
}