using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colsp.Model.Requests
{
    public class SEORequest
    {
        public string MetaTitle { get; set; }
        public string MetaDescription { get; set; }
        public string MetaKeywords { get; set; }
        public string ProductUrlKeyTh { get; set; }
        public string ProductUrlKeyEn { get; set; }
        public string ProductBoostingWeight { get; set; }
    }
}
