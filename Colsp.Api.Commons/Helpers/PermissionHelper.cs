using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Web;

namespace Colsp.Api.Commons
{
	public static class PermissionHelper
	{
		public static bool Check(IPrincipal user, String permission)
		{
			if (user is ClaimsPrincipal)
			{
				return ((ClaimsPrincipal)user).HasClaim("Permission", permission);
			}
			else
			{
				return false;
			}
		}
	}
}