using System.Collections.Generic;

namespace Colsp.Model.Requests
{
    /// <summary>
    /// List of fields which can be exported
    /// This should be kept as a configuration
    /// </summary>
    public class ExportRequest
    {
        public List<string> Options { get; set; }
        public List<ProductStageRequest> ProductList { get; set; }
        public List<AttributeSetRequest> AttributeSets { get; set; }

        //Constructor
        public ExportRequest()
        {
            Options = new List<string>();
            ProductList = new List<ProductStageRequest>();
            AttributeSets = new List<AttributeSetRequest>();
        }
    }
}
