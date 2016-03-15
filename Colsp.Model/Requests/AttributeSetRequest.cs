using System.Collections.Generic;

namespace Colsp.Model.Requests
{
    public class AttributeSetRequest : PaginatedRequest
    {
        public int AttributeSetId { get; set; }
        public string AttributeSetNameEn { get; set; }
        public string AttributeSetNameTh { get; set; }
        public string AttributeSetDescriptionEn { get; set; }
        //public string AttributeSetDescriptionTh { get; set; }
        public bool Visibility { get; set; }
        public string Status { get; set; }
        public List<AttributeRequest> Attributes { get; set; }
        public string SearchText { get; set; }
        public List<TagRequest> Tags { get; set; }
        public List<CategoryRequest> Category { get; set; }
        public int ProductCount { get; set; }

        public AttributeSetRequest()
        {
            AttributeSetId = 0;
            AttributeSetNameEn = string.Empty;
            AttributeSetNameTh = string.Empty;
            AttributeSetDescriptionEn = string.Empty;
            //AttributeSetDescriptionTh = string.Empty;
            Visibility = false;
            Status = string.Empty;
            SearchText = string.Empty;
            Attributes = new List<AttributeRequest>();
            Category = new List<CategoryRequest>();
            Tags = new List<TagRequest>();
        }

        public override void DefaultOnNull()
        {
            AttributeSetId = GetValueOrDefault(AttributeSetId, 0);
            SearchText = GetValueOrDefault(SearchText, null);
            _order = GetValueOrDefault(_order, "UpdatedDt");
            base.DefaultOnNull();
        }
    }
}
