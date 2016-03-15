using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colsp.Model.Requests
{
    public class CMSCategoryProductGetListRequest : PaginatedRequest

    {
        public string SearchText { get; set; }
        public override void DefaultOnNull()
        {
            SearchText = GetValueOrDefault(SearchText, null);
            base.DefaultOnNull();
        }
    }
}
