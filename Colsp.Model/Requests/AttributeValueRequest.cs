using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colsp.Model.Requests
{
    public class AttributeValueRequest
    {
        public int? AttributeValueId { get; set; }
        public string AttributeValueEn { get; set; }
        public string AttributeValueTh { get; set; }
        public string Status { get; set; }
    }
}
