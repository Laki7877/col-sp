using Colsp.Model.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colsp.Model.Responses
{
    public class OnTopCreditCardResponse
    {
        public int? OnTopCreditCardId { get; set; }
        public string NameEN { get; set; }
        public string NameTH { get; set; }
        public string BankNameEN { get; set; }
        public string BankNameTH { get; set; }
        public string PromotionCode { get; set; }
        public DateTime EffectiveDate { get; set; }
        public TimeSpan EffectiveTime { get; set; }
        public DateTime ExpiryDate { get; set; }
        public TimeSpan ExpiryTime { get; set; }
        public string PaymentId { get; set; }
        public int? ShopId { get; set; }
        public string DiscountType { get; set; }
        public bool FreeShipping { get; set; }
        public decimal DiscountValue { get; set; }
        public decimal MinimumOrderAmount { get; set; }
        public decimal MaximumDiscountAmount { get; set; }
        public string ShortDescriptionTH { get; set; }
        public string ShortDescriptionEN { get; set; }
        public string IconURLTH { get; set; }
        public string IconURLEN { get; set; }
        public int? Sequence { get; set; }
        public string Status { get; set; }
        public bool Visibility { get; set; }
        public int CreateBy { get; set; }
        public string CreateIP { get; set; }
        public List<Carditemlist> CardItemList { get; set; }
    }
}
