using System.Linq;
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
			if (this.Permission == null)
			{
				return false;
			}

			// Fetch permission from attribute
			var permissions = this.Permission;

			// Match with the user's permission list
			var claimsIdentity = HttpContext.Current.User.Identity as ClaimsIdentity;
			var exist = claimsIdentity.HasClaim((c) => {
				return c.Type.Equals("Permission") &&
					   permissions.Contains(c.Value);
			});
			return exist;
		}
	}
}