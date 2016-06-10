using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colsp.Model.Requests
{
	public class EVoucherRequest
	{
		public string CouponName { get; set; }
		public string CouponCode { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime ExpiredDate { get; set; }
		public bool Status { get; set; }
		public int DiscountType { get; set; }
		public decimal? DiscountBaht { get; set; }
		public decimal? DiscountPercent { get; set; }
		public decimal? MaximumDiscount { get; set; }
		public int UsesPerCustomer { get; set; }
		public int MaximumUses { get; set; }
		public int CartCriteria { get; set; }
		public decimal CriteriaPrice { get; set; }
		public int CriteriaQty { get; set; }
		public int? IncludeProductCriteria { get; set; }
		public List<int> IncludeGlobalCategories { get; set; }
		public List<int> IncludeShopIds { get; set; }
		public List<int> IncludeLocalCategories { get; set; }
		public List<string> IncludeProductIds { get; set; }
		public int? ExcludeProductCriteria { get; set; }
		public List<int> ExcludeIncludeGlobalCategories { get; set; }
		public List<int> ExcludeIncludeShopIds { get; set; }
		public List<int> ExcludeIncludeLocalCategories { get; set; }
		public List<string> ExcludeProductIds { get; set; }

		public EVoucherRequest()
		{

		}
	}

	 
}
