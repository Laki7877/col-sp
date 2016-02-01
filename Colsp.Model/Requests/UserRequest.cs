using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colsp.Model.Requests
{
	public class UserRequest : PaginatedRequest
	{
        public string SearchText { get; set; }
        public string NameEn { get; set; }
        public string NameTh { get; set; }
        public int? UserId { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Password { get; set; }
        public string EmployeeId { get; set; }
        public string Position { get; set; }
        public string Division { get; set; }
        public List<UserGroupRequest> UserGroup { get; set; }

        public override void DefaultOnNull()
		{
            SearchText = GetValueOrDefault(SearchText, "");
			_order = GetValueOrDefault(_order, "UserId");
			base.DefaultOnNull();
		}

        public UserRequest()
        {
            UserGroup = new List<UserGroupRequest>();
        }
    }
}
