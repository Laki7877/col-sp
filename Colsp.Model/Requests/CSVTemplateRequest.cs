using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colsp.Model.Requests
{
    public class CSVTemplateRequest
    {
        public List<CategoryRequest> GlobalCategories { get; set; }
        public List<AttributeSetRequest> AttributeSets { get; set; }
    }
}
