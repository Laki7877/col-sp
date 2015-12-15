using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace Colsp.Filters
{
	public class ClaimsAuthorizeAttribute :	AuthorizeAttribute	
	{
		public string Permission { get; set; }
		protected override bool IsAuthorized(HttpActionContext actionContext)
		{
			if (!(HttpContext.Current.User.Identity is ClaimsIdentity))
			{
				return false;
			}
			if (this.Permission == null)
			{
				return false;
			}
			var permissions = this.Permission.Replace(" ", "").Split(',');
			var claimsIdentity = HttpContext.Current.User.Identity as ClaimsIdentity;
			var exist = claimsIdentity.HasClaim((c) => { return c.Type.Equals("Permission") && 
																permissions.Contains(c.Value); });
			return exist;
		}
	}
}