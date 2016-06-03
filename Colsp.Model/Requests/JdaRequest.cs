using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colsp.Model.Requests
{
    public class JdaRequest
    {
        public string Sku { get; set; }
        public decimal Price { get; set; }
        public string Barcode { get; set; }
        public string BrandName { get; set; }
        public string ShortDescriptionTh { get; set; }
        public string ShortDescriptionEn { get; set; }
        public int ShopId { get; set; }
        public string Pid { get; set; }
        public int Quantity { get; set; }
        public string JDADept { get; set; }
        public string JDASubDept { get; set; }
        public decimal PromotionPrice { get; set; }
        public DateTime? EffectiveDatePromotion { get; set; }
        public DateTime? ExpireDatePromotion { get; set; }

        public JdaRequest()
        {
            Sku = string.Empty;
            Price = 0;
            Barcode = string.Empty;
            BrandName = string.Empty;
            ShortDescriptionTh = string.Empty;
            ShortDescriptionEn = string.Empty;
            ShopId = 0;
            Pid = string.Empty;
            Quantity = 0;
            JDADept = string.Empty;
            JDASubDept = string.Empty;
            PromotionPrice = 0;
            EffectiveDatePromotion = null;
            ExpireDatePromotion = null;
        }
    }
}
