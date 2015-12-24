using System.Security.Claims;
using System.Security.Principal;

namespace Colsp.Api.Security
{
	public class UsersPrincipal : ClaimsPrincipal
	{
		public int UserId { get; set; }
		public string NameEn { get; set; }
		public string NameTh { get; set; }
		public string Email { get; set; }
		public UsersPrincipal(IIdentity identity, int userId, string nameEn, string nameTh, string email) : base(identity)
		{
			UserId = userId;
			NameEn = nameEn;
			NameTh = nameTh;
			Email = email;
		}
	}
}