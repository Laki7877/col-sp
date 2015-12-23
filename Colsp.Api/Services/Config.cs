using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace Colsp.Api.Services
{
	public class Config : ConfigurationSection
	{
		#region Global
		public static readonly Config config = (Config)ConfigurationManager.GetSection("colsp");
		public static Config Settings
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
			[ConfigurationProperty("use", DefaultValue = false, IsRequired = true)]
			public bool Use
			{
				get
				{
					return (bool)this["use"];
				}
				set
				{
					this["use"] = value;
				}
			}
			[ConfigurationProperty("expire", DefaultValue = 30.0, IsRequired = false)]
			public double Expire
			{
				get
				{
					return (double)this["expire"];
				}
				set
				{
					this["expire"] = value;
				}
			}
		}

		#endregion
	}
}