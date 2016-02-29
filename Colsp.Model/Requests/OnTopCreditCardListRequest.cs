using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colsp.Model.Requests
{
   public class OnTopCreditCardListRequest : PaginatedRequest
    {

        public string SearchText { get; set; }
        public string CardTypeCode { get; set; }
        public string BankName { get; set; }
        public string Name { get; set; }
        public decimal? DiscountFrom { get; set; }
        public decimal? DiscountTo { get; set; }
        public DateTime EffectiveDate { get; set; }
        public DateTime ExpireDate { get; set; }

        public override void DefaultOnNull()
        {
            CardTypeCode = GetValueOrDefault(CardTypeCode, null);
            BankName = GetValueOrDefault(BankName, null);
            Name = GetValueOrDefault(Name, null);
            DiscountFrom = GetValueOrDefault(DiscountFrom, null);
            DiscountTo = GetValueOrDefault(DiscountTo, null);
            EffectiveDate = GetValueOrDefault(EffectiveDate, default(DateTime));
            ExpireDate = GetValueOrDefault(ExpireDate, default(DateTime));
            SearchText = GetValueOrDefault(SearchText, null);
            _order = GetValueOrDefault(_order, "ProductId");
            base.DefaultOnNull();
        }
    }
}
