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
        public string AttributeSetNameEn { get; set; }
        public string AttributeSetNameTh { get; set; }
        public string AttributeSetDescriptionEn { get; set; }
        public string AttributeSetDescriptionTh { get; set; }
        public string Status { get; set; }
        public List<AttributeRequest> Attributes { get; set; }
        public string SearchText { get; set; }
        public List<TagRequest> Tags { get; set; }

        public override void DefaultOnNull()
        {
            AttributeSetId = GetValueOrDefault(AttributeSetId, null);
            SearchText = GetValueOrDefault(SearchText, null);
            _order = GetValueOrDefault(_order, "AttributeSetId");
            base.DefaultOnNull();
        }
    }
}
