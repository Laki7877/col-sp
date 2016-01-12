using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colsp.Model.Requests
{
    public class AttributeRequest : PaginatedRequest
    {
        public int? AttributeId { get; set; }
        public string ValueEn { get; set; }
        public string SearchText { get; set; }

        public override void DefaultOnNull()
        {
            AttributeId = GetValueOrDefault(AttributeId, null);
            ValueEn = GetValueOrDefault(ValueEn, null);
            SearchText = GetValueOrDefault(SearchText, null);
            _order = GetValueOrDefault(_order, "AttributeId");
            base.DefaultOnNull();
        }
    }
}
