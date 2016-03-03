using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Colsp.Entity.Models;
using System.Web;

namespace Colsp.Model.Requests
{

    public class OnTopCreditCardRequest
    {
        public string OnTopCreditCardId { get; set; }
        public string NameEN { get; set; }
        public string NameTH { get; set; }
        public string BankNameEN { get; set; }
        public string BankNameTH { get; set; }
        public string PromotionCode { get; set; }
        public string EffectiveDate { get; set; }
        public string EffectiveTime { get; set; }
        public string ExpiryDate { get; set; }
        public string ExpiryTime { get; set; }
        public string PaymentId { get; set; }
        public int ShopId { get; set; }
        public string DiscountType { get; set; }
        public bool FreeShipping { get; set; }
        public int DiscountValue { get; set; }
        public int MinimumOrderAmount { get; set; }
        public int MaximumDiscountAmount { get; set; }
        public string ShortDescriptionTH { get; set; }
        public string ShortDescriptionEN { get; set; }
        public string IconURLTH { get; set; }
        public string IconURLEN { get; set; }
        public int Sequence { get; set; }
        public string Status { get; set; }
        public bool Visibility { get; set; }
        public int CreateBy { get; set; }
        public string CreateIP { get; set; }
        public List<Carditemlist> CardItemList { get; set; }

    }

    public class Carditemlist
    {
        #region Properties
        private string GetTypeText(string code)
        {
            string value = "";
            try
            {
                switch (code)
                {
                    case "15A": value = "American Express - (15 digit)"; break;
                    case "14D": value = "Diners Club - (14 digit)"; break;
                    case "14C": value = "Carte Blanche - (14 digit)"; break;
                    case "16D": value = "Discover - (16 digit)"; break;
                    case "15E": value = "EnRoute - (15 digit)"; break;
                    case "16J": value = "JCB - (16 digit)"; break;
                    case "15J": value = "JCB - (15 digit)"; break;
                    case "16M": value = "Master Card - (16 digit)"; break;
                    case "13V": value = "Visa - (13 digit)"; break;
                    case "16V": value = "Visa - (16 digit)"; break;
                    default:
                        value = "";
                        break;
                }
                return value;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public List<CreditCardType> Typename { get; set; }
        #endregion
        public int OnTopCreditNumberId { get; set; }
        public int OnTopCreditCardId { get; set; }
        public string CreditCardTypeCode { get; set; }
        public string CreditCardTypeText { get { return GetTypeText(this.CreditCardTypeCode); } set { } }
        public string CreditNumberFormat { get; set; }
        public int Digit { get; set; }
        public bool Visibility { get; set; }
        public bool Status { get; set; }
    }


}
