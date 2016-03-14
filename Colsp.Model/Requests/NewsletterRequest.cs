using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colsp.Model.Requests
{
    public class NewsletterRequest : PaginatedRequest
    {
        public int NewsletterId { get; set; }
        public string Subject { get; set; }
        public string Description { get; set; }
        public string VisbleShopGroup { get; set; }
        public ImageRequest Image { get; set; }


        public override void DefaultOnNull()
        {
            _order = GetValueOrDefault(_order, "UpdatedDt");
            base.DefaultOnNull();
        }
    }
}
