using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colsp.Model.Requests
{
    public class ProductReviewRequest : PaginatedRequest
    {
        public string Pid { get; set;}
        public string SearchText { get; set; }

        public override void DefaultOnNull()
        {
            SearchText = GetValueOrDefault(SearchText, null);
            Pid = GetValueOrDefault(Pid, null);
            _order = GetValueOrDefault(_order, "Pid");
            base.DefaultOnNull();
        }
    }
}
