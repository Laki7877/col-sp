using System.Collections.Generic;

namespace Colsp.Model.Requests
{
    public class NewsletterRequest : PaginatedRequest
    {
        public int NewsletterId { get; set; }
        public string Subject { get; set; }
        public string Description { get; set; }
        public string VisbleShopGroup { get; set; }
        public ImageRequest Image { get; set; }
        public List<ShopRequest> Shops { get; set; }

        public NewsletterRequest()
        {
            NewsletterId = 0;
            Subject = string.Empty;
            Description = string.Empty;
            VisbleShopGroup = string.Empty;
            Shops = new List<ShopRequest>();
            Image = new ImageRequest();
        }

        public override void DefaultOnNull()
        {
            _order = GetValueOrDefault(_order, "UpdatedDt");
            base.DefaultOnNull();
        }
    }
}
