using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colsp.Model.Requests
{
    public class CollectionListRequest : PaginatedRequest
    {
        public int? SellerId { get; set; }
        public string SearchText { get; set; }
        public int? CategoryId { get; set; }

        public override void DefaultOnNull()
        {
            SellerId = GetValueOrDefault(SellerId, null);
            CategoryId = GetValueOrDefault(CategoryId, null);
            SearchText = GetValueOrDefault(SearchText, null);
            _order = GetValueOrDefault(_order, "ProductId");
            base.DefaultOnNull();
        }
    }
}
