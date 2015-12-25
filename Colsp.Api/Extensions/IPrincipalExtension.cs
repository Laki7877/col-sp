using Colsp.Api.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Web;

namespace Colsp.Api.Extensions
{
	public static class IPrincipalExtension
	{
		public static bool HasPermission(this IPrincipal p, string permission)
		{
			if(p is ClaimsPrincipal)
			{
				return ((ClaimsPrincipal)p).HasClaim("Permission", permission);
			}
			return false;
		}
		public static int? UserId(this IPrincipal p)
		{
			if(p is UsersPrincipal)
			{
				return ((UsersPrincipal)p).UserId;
			}
			return null;
		}
		public static List<int> ShopIds(this IPrincipal p)
		{
			if(p is UsersPrincipal)
			{
				return ((UsersPrincipal)p).Shops;
			}
			return new List<int>();
		}
	}
}