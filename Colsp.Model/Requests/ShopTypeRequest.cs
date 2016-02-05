using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colsp.Model.Requests
{
    public class ShopTypeRequest : PaginatedRequest
    {
        public int? ShopTypeId { get; set; }
        public string ShopTypeNameEn { get; set; }
        public List<PermissionRequest> Permission { get; set; }

        public override void DefaultOnNull()
        {
            _order = GetValueOrDefault(_order, "ShopTypeId");
            base.DefaultOnNull();
        }
    }
}
