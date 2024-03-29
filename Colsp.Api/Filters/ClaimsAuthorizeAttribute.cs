﻿using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace Colsp.Api.Filters
{
	public class ClaimsAuthorizeAttribute : AuthorizeAttribute
	{
		public string[] Permission { get; set; }

        protected override bool IsAuthorized(HttpActionContext actionContext)
		{
			if (!(HttpContext.Current.User.Identity is ClaimsIdentity))
			{
				return false;
			}
			if (Permission == null)
			{
				return false;
			}

			// Fetch permission from attribute
			var permissions = Permission;

			// Match with the user's permission list
			var claimsIdentity = HttpContext.Current.User.Identity as ClaimsIdentity;
			//bool exist = true;
			//foreach (var permission in permissions)
			//{
			//	if(!claimsIdentity.Claims.Any(a=>a.Type.Equals("Permission") && a.Value.Equals(permission)))
			//	{
			//		exist = false;
			//		break;
			//	}
			//}
			var exist = claimsIdentity.HasClaim((c) =>
			{
				return c.Type.Equals("Permission") &&
					   permissions.Contains(c.Value);
			});
			return exist;
		}
	}
}