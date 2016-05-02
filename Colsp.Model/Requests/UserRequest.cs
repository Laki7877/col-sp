using System.Collections.Generic;

namespace Colsp.Model.Requests
{
    public class UserRequest : PaginatedRequest
	{
        public string SearchText { get; set; }
        public string NameEn { get; set; }
        public string NameTh { get; set; }
        public int UserId { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Password { get; set; }
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
        public string EmployeeId { get; set; }
        public string Position { get; set; }
        public string Division { get; set; }
        public List<UserGroupRequest> UserGroup { get; set; }
        public bool IsPasswordChange { get; set; }
        public string Type { get; set; }
        public string Mobile { get; set; }
        public string Fax { get; set; }
        public bool IsAdmin { get; set; }
        public string Token { get; set; }
        public List<BrandRequest> Brands { get; set; }

        public UserRequest()
        {
            SearchText = string.Empty;
            NameEn = string.Empty;
            NameTh = string.Empty;
            UserId = 0;
            Email = string.Empty;
            Phone = string.Empty;
            Password = string.Empty;
            OldPassword = string.Empty;
            OldPassword = string.Empty;
            NewPassword = string.Empty;
            EmployeeId = string.Empty;
            Position = string.Empty;
            Division = string.Empty;
            IsPasswordChange = false;
            Type = string.Empty;
            Mobile = string.Empty;
            Fax = string.Empty;
            UserGroup = new List<UserGroupRequest>();
            Brands = new List<BrandRequest>();
        }


        public override void DefaultOnNull()
		{
            SearchText = GetValueOrDefault(SearchText, string.Empty);
			_order = GetValueOrDefault(_order, "UserId");
			base.DefaultOnNull();
		}

        
    }
}
