using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colsp.Model.Requests
{
	public class ProductRequest : PaginatedRequest
	{
		public int? SellerId { get; set; }
		public string SearchText { get; set; }
        public int? CategoryId { get; set; }
        public int? GlobalCatId { get; set; }
        public int? LocalCatId { get; set; }
        public int? AttributeSetId { get; set; }
        public int? AttributeId { get; set; }

        public override void DefaultOnNull()
		{
			SellerId = GetValueOrDefault(SellerId, null);
            CategoryId = GetValueOrDefault(CategoryId, null);
            GlobalCatId = GetValueOrDefault(GlobalCatId, null);
            LocalCatId = GetValueOrDefault(LocalCatId, null);
            AttributeSetId = GetValueOrDefault(AttributeSetId, null);
            AttributeId = GetValueOrDefault(AttributeId, null);
            SearchText = GetValueOrDefault(SearchText, null);
			_order = GetValueOrDefault(_order, "ProductId");
			base.DefaultOnNull();
		}
	}
}
