using System;
using System.Collections.Generic;

namespace Colsp.Model.Requests
{
    public class ConditionRequest
    {
        public List<OrderRequest> Order { get; set; }
        public FilterByRequest FilterBy { get; set; }
        public List<String> Include { get; set; }
		public List<String> Exclude { get; set; }



		public ConditionRequest()
        {
            Order = new List<OrderRequest>();
            FilterBy = new FilterByRequest();
			Include = new List<string>();
			Exclude = new List<string>();
		}

	}
}
