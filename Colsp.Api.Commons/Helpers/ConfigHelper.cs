using System;
using System.Configuration;

namespace Colsp.Api.Commons
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

		#region Cache
		[ConfigurationProperty("cache")]
		public CacheElement Cache
		{
			get
			{
				return (CacheElement)this["cache"];
			}	
			set
			{
				this["cache"] = value;
			}
		}
		public class CacheElement : ConfigurationElement
		{
			[ConfigurationProperty("expire")]
			public CacheExpireElement Expire
			{
				get
				{
					return (CacheExpireElement)this["expire"];
				}
				set
				{
					this["expire"] = value;
				}
			}
		}
		public class CacheExpireElement : ConfigurationElement
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