using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colsp.Logic
{
    public class BrandCondition : BaseCondition
    {
        public int? BrandId { get; set; }
        public string BrandNameTh { get; set; }
        public string BrandNameEn { get; set; }
    }
}
