using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colsp.Logic
{
    public class ProductCondition
    {
        public SearchOption SearchBy { get; set; }
        public string SearchText { get; set; }
        public string Tag { get; set; }
        public int CategoryId { get; set; }
        public int BrandId { get; set; }
    }

    public enum SearchOption
    {
        [StringValue("PID")]
        PID,

        [StringValue("SKU")]
        SKU,

        [StringValue("ProductName")]
        ProductName
    }
}
