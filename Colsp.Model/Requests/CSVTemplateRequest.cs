using System.Collections.Generic;

namespace Colsp.Model.Requests
{
    public class CSVTemplateRequest
    {
        public List<CategoryRequest> GlobalCategories { get; set; }
        public List<AttributeSetRequest> AttributeSets { get; set; }
            
        public CSVTemplateRequest()
        {
            GlobalCategories = new List<CategoryRequest>();
            AttributeSets = new List<AttributeSetRequest>();
        }
    }
}
