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
        public string AttributeNameEn { get; set; }
        public string AttributeNameTh { get; set; }
        public string AttributeUnitEn { get; set; }
        public string AttributeUnitTh { get; set; }
        public string DisplayNameEn { get; set; }
        public string DisplayNameTh { get; set; }
        public string Status { get; set; }

        public bool? VariantStatus { get; set; }
        public string DataType { get; set; }
        public string DataValidation { get; set; }
        public string VariantDataType { get; set; }
        public string DefaultValue { get; set; }
        public bool? ShowAdminFlag { get; set; }
        public bool? ShowGlobalSearchFlag { get; set; }
        public bool? ShowLocalSearchFlag { get; set; }
        public bool? ShowGlobalFilterFlag { get; set; }
        public bool? ShowLocalFilterFlag { get; set; }
        public bool? AllowHtmlFlag { get; set; }
        public bool Filterable { get; set; }
        public bool Required { get; set; }
        public List<AttributeValueRequest> AttributeValues { get; set; }
        public string ValueEn { get; set; }
        public string SearchText { get; set; }
        public int? ProductCount { get; set; }

        public AttributeRequest()
        {
            AttributeValues = new List<AttributeValueRequest>();
        }

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
