using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colsp.Model.Requests
{
    public class SEORequest
    {
        public string MetaTitleEn { get; set; }
        public string MetaTitleTh { get; set; }
        public string MetaDescriptionEn { get; set; }
        public string MetaDescriptionTh { get; set; }
        public string MetaKeywordEn { get; set; }
        public string MetaKeywordTh { get; set; }
        public string ProductUrlKeyTh { get; set; }
        public string ProductUrlKeyEn { get; set; }
        public int? ProductBoostingWeight { get; set; }
    }
}
