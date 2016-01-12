using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colsp.Model.Requests
{
    public class AttributeSetRequest : PaginatedRequest
    {
        public int? AttributeSetId { get; set; }
        public string SearchText { get; set; }

        public override void DefaultOnNull()
        {
            AttributeSetId = GetValueOrDefault(AttributeSetId, null);
            SearchText = GetValueOrDefault(SearchText, null);
            _order = GetValueOrDefault(_order, "AttributeSetId");
            base.DefaultOnNull();
        }
    }
}
