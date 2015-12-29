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
		public override void DefaultOnNull()
		{
			SellerId = GetValueOrDefault(SellerId, null);
			SearchText = GetValueOrDefault(SearchText, null);
			_order = GetValueOrDefault(_order, "ProductId");
			base.DefaultOnNull();
		}
	}
}
