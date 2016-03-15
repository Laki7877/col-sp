using Colsp.Api.Security;
using Colsp.Model.Requests;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Principal;

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

        public static UserRequest UserRequest(this IPrincipal p)
        {
            if (p is UsersPrincipal)
            {
                return ((UsersPrincipal)p).User;
            }
            return null;
        }
            
        public static List<ShopRequest> Shops(this IPrincipal p)
        {
            if (p is UsersPrincipal)
            {
                return ((UsersPrincipal)p).Shops;
            }
            return new List<ShopRequest>();
        }

        public static ShopRequest ShopRequest(this IPrincipal p)
        {
            if (p is UsersPrincipal)
            {
                var list = ((UsersPrincipal)p).Shops;
                if (list != null && list.Count > 0)
                {
                    return list[0];
                }
            }
            return null;
        }


    }
}