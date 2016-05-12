using System.Collections.Generic;

namespace Colsp.Model.Requests
{
    public class PendingProduct
    {
        public CategoryRequest Category { get; set; }
        public AttributeSetRequest AttributeSet { get; set; }
        public List<VariantRequest> Variants { get; set; }
        public ShopRequest Shop { get; set; }

        public PendingProduct()
        {
            Category = new CategoryRequest();
            AttributeSet = new AttributeSetRequest();
            Variants = new List<VariantRequest>();
            Shop = new ShopRequest();
        }
    }
}
