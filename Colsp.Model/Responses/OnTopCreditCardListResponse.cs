using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colsp.Model.Responses
{
    public class OnTopCreditCardListResponse
    {
        public int OnTopCreditCardId { get; set; }
        public string NameEN { get; set; }
        public string NameTH { get; set; }
        public string BankNameEN { get; set; }
        public string BankNameTH { get; set; }
        public decimal MinimumOrderAmount { get; set; }
        public decimal MaximumDiscountAmount { get; set; }
        public string Status { get; set; }
        public bool Visibility { get; set; }
        public DateTime UpdateDate { get; set; }
        public int ShopId { get; set; }
        public string DiscountType { get; set; }
        public string DiscountTypeText
        {
            get
            {
                string value = "";
                if (!string.IsNullOrWhiteSpace(this.DiscountType))
                {
                    switch (this.DiscountType.ToUpper())
                    {
                        case "PER":
                            value = "Percentage";
                            break;
                        case "FIX":
                            value = "Fix rate";
                            break;
                        default:
                            break;
                    }
                }
                return value;
            }
            set
            {
                this.DiscountTypeText = value;
            }
        }
    }
}
