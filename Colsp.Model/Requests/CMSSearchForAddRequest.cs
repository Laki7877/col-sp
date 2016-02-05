using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colsp.Model.Requests
{
    public class CMSSearchForAddRequest : PaginatedRequest
    {
        public int? KeyId { get; set; }
        public string SearchType { get; set; }
        public string SearchText { get; set; }

        public override void DefaultOnNull()
        {
            KeyId = GetValueOrDefault(KeyId, null);
            SearchType = GetValueOrDefault(SearchType, null);
            SearchText = GetValueOrDefault(SearchText, null);
            base.DefaultOnNull();
        }
    }
}
