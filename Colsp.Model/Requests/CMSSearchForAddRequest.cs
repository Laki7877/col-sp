using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colsp.Model.Requests
{
    public class CMSSearchForAddRequest : PaginatedRequest
    {
        public string SearchType { get; set; }
        public string SearchText { get; set; }
        public int? CategoryId { get; set; }
        public override void DefaultOnNull()
        {
            SearchType = GetValueOrDefault(SearchType, null);
            SearchText = GetValueOrDefault(SearchText, null);
            base.DefaultOnNull();
        }
    }
}
