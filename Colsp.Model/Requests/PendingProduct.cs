﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colsp.Model.Requests
{
    public class PendingProduct
    {
        public CategoryRequest Category { get; set; }
        public AttributeSetRequest AttributeSet { get; set; }
        public List<VariantRequest> Variants { get; set; }

        public PendingProduct()
        {
            Category = new CategoryRequest();
            AttributeSet = new AttributeSetRequest();
            Variants = new List<VariantRequest>();
        }
    }
}
