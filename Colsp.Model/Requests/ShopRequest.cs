using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colsp.Model.Requests
{
    public class ShopRequest : PaginatedRequest
    {
        public int? ShopId { get; set; }
        public string ShopNameEn { get; set; }
        public string ShopNameTh { get; set; }
        public string Status { get; set; }
        public UserRequest ShopOwner { get; set; }
        public List<UserRequest> Users { get; set; }
        public ShopTypeRequest ShopType { get; set; }
        public decimal? Commission { get; set; }

        public override void DefaultOnNull()
        {
            ShopNameEn = GetValueOrDefault(ShopNameEn, "");
            _order = GetValueOrDefault(_order, "ShopId");
            base.DefaultOnNull();
        }

        public ShopRequest()
        {
            ShopOwner = new UserRequest();
            Users = new List<UserRequest>();
            ShopType = new ShopTypeRequest();
        }
    }
}
