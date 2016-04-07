using System.Collections.Generic;

namespace Colsp.Model.Requests
{
    public class MasterProductRequest
    {
        public ProductStageRequest MasterProduct { get; set; }
        public List<ProductStageRequest> ChildProducts { get; set; }

        public MasterProductRequest()
        {
            MasterProduct = new ProductStageRequest();
            ChildProducts = new List<ProductStageRequest>();
        }
    }
}
