using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colsp.Model.Requests
{
    public class ImportHeaderRequest
    {
        public int? ImportHeaderId { get; set; }
        public string HeaderName { get; set; }
        public string Description { get; set; }
        public string AcceptedValue { get; set; }
        public string Example { get; set; }
        public string Note { get; set; }
        public string GroupName { get; set; }
        public object AttributeValue { get; set; }
        public bool IsAttribute { get; set; } 
        public string AttributeType { get; set; }
        public bool? IsVariant { get; set; }

        public ImportHeaderRequest()
        {
            AttributeValue = new object();
        }
    }
}
