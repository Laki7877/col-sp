using System;
using System.Configuration;

namespace Colsp.Helpers
{
	public class ConfigHelper : ConfigurationSection
	{
		#region Global Config Object

		// use ConfigHelper.Settings.XXX
		public static readonly ConfigHelper config = (ConfigHelper)ConfigurationManager.GetSection("colsp");
		public static ConfigHelper Settings
		{
			get
			{
				return config;
			}
		}

		#endregion

		#region Cache Expiration
		[ConfigurationProperty("cacheExpiration")]
		public CacheExpirationElement CacheExpiration
		{
			get
			{
				return (CacheExpirationElement)this["cacheExpiration"];
			}	
			set
			{
				this["cacheExpiration"] = value;
			}
		}
		public class CacheExpirationElement : ConfigurationElement
		{
			[ConfigurationProperty("value", DefaultValue = 30.0, IsRequired = true)]
			public double Value
			{
				get
				{
					return (double)this["value"];
				}
				set
				{
					this["value"] = value;
				}
			}
		}
		
		#endregion
	}
}