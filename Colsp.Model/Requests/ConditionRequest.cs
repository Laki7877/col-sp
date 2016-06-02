using System;
using System.Collections.Generic;

namespace Colsp.Model.Requests
{
    public class ConditionRequest
    {
        public List<OrderRequest> Order { get; set; }
        public FilterByRequest FilterBy { get; set; }
        public List<ProductRequest> Include { get; set; }
        public List<ProductRequest> Exclude { get; set; }


        public ConditionRequest()
        {
            Order = new List<OrderRequest>();
            FilterBy = new FilterByRequest();
            Include = new List<ProductRequest>();
            Exclude = new List<ProductRequest>();
        }

    }
}
