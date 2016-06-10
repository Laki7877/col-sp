using System.Collections.Generic;

namespace Colsp.Model.Requests
{
    public class ShopTypeRequest : PaginatedRequest
    {
        public int ShopTypeId { get; set; }
        public string ShopTypeNameEn { get; set; }
        public string SearchText { get; set; }
        public List<PermissionRequest> Permission { get; set; }
        public List<ThemeRequest> Themes { get; set; }
        public List<ShippingRequest> Shippings { get; set; }


        public ShopTypeRequest()
        {
            ShopTypeId = 0;
            ShopTypeNameEn = string.Empty;
            SearchText = string.Empty;
            Permission = new List<PermissionRequest>();
            Themes = new List<ThemeRequest>();
            Shippings = new List<ShippingRequest>();
        }

        public override void DefaultOnNull()
        {
            SearchText = GetValueOrDefault(SearchText, string.Empty);
            _order = GetValueOrDefault(_order, "UpdateOn");
            base.DefaultOnNull();
        }
    }

    public class ThemeRequest
    {
        public int ThemeId { get; set; }
        public string ThemeName { get; set; }
        public string ThemeImage { get; set; }

        public ThemeRequest()
        {
            ThemeId = 0;
            ThemeName = string.Empty;
            ThemeImage = string.Empty;
        }
    }

    public class ShippingRequest
    {
        public int ShippingId { get; set; }
        public string ShippingMethodEn { get; set; }

        public ShippingRequest()
        {
            ShippingId = 0;
            ShippingMethodEn = string.Empty;
        }
    }
}
