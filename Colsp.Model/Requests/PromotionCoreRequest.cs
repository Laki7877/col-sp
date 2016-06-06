using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colsp.Model.Requests
{
	public class PromotionCoreRequest
	{
		public string PromotionCode { get; set; }
		public int PaymentId { get; set; }
		public string Description { get; set; }
		public DateTime? EffectiveDateTime { get; set; }
		public DateTime? ExpireDateTime { get; set; }
		public List<string> CreditCardCodes { get; set; }
		public decimal DiscountBaht { get; set; }
		public decimal DiscountPercent { get; set; }
		public decimal MaximumDiscount { get; set; }
		public decimal MinimumOrderAmount { get; set; }
		public int Limit { get; set; }
		public int LimitPerUser { get; set; }

		public PromotionCoreRequest()
		{
			PromotionCode = string.Empty;
			PaymentId = 0;
			Description = string.Empty;
			EffectiveDateTime = null;
			ExpireDateTime = null;
			CreditCardCodes = new List<string>();
			DiscountBaht = 0;
			DiscountPercent = 0;
			MaximumDiscount = 0;
			MinimumOrderAmount = 0;
			Limit = 0;
			LimitPerUser = 0;
		}
	}

	 
}
