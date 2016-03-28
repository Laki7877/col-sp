using Colsp.Model.Requests;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Principal;

namespace Colsp.Api.Security
{
	public class UsersPrincipal : ClaimsPrincipal
	{
        public List<ShopRequest> Shops { get; set; }
        public UserRequest User { get; set; }
        public DateTime LoginDt { get; set; }

        public UsersPrincipal(IIdentity identity, List<ShopRequest> shops, UserRequest user, DateTime loginDt) : base(identity)
		{
            Shops = shops;
            User = user;
            LoginDt = loginDt;
        }
	}
}