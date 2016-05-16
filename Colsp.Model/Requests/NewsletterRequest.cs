using System;
using System.Collections.Generic;

namespace Colsp.Model.Requests
{
    public class NewsletterRequest : PaginatedRequest
    {
        public int NewsletterId { get; set; }
        public string Subject { get; set; }
        public string Description { get; set; }
        public string VisibleShopGroup { get; set; }
        public ImageRequest Image { get; set; }
        public List<ShopRequest> Shops { get; set; }
        public DateTime? PublishedDt { get; set; }
        public List<ShopRequest> IncludeShop { get; set; }
        public List<ShopRequest> ExcludeShop { get; set; }

        public NewsletterRequest()
        {
            NewsletterId = 0;
            Subject = string.Empty;
            Description = string.Empty;
            VisibleShopGroup = string.Empty;
            Shops = new List<ShopRequest>();
            Image = new ImageRequest();
            IncludeShop = new List<ShopRequest>();
            ExcludeShop = new List<ShopRequest>();
        }

        public override void DefaultOnNull()
        {
            _order = GetValueOrDefault(_order, "UpdateOn");
            base.DefaultOnNull();
        }
    }
}
