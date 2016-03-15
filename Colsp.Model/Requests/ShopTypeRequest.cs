using System.Collections.Generic;

namespace Colsp.Model.Requests
{
    public class ShopTypeRequest : PaginatedRequest
    {
        public int ShopTypeId { get; set; }
        public string ShopTypeNameEn { get; set; }
        public string SearchText { get; set; }
        public List<PermissionRequest> Permission { get; set; }

        public ShopTypeRequest()
        {
            ShopTypeId = 0;
            ShopTypeNameEn = string.Empty;
            SearchText = string.Empty;
            Permission = new List<PermissionRequest>();
        }

        public override void DefaultOnNull()
        {
            SearchText = GetValueOrDefault(SearchText, string.Empty);
            _order = GetValueOrDefault(_order, "UpdatedDt");
            base.DefaultOnNull();
        }
    }
}
