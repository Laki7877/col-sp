using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
