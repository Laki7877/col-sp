using System.Collections.Generic;

namespace Colsp.Model.Requests
{
    public class AttributeRequest : PaginatedRequest
    {
        public int AttributeId { get; set; }
        public string AttributeNameEn { get; set; }
        public string AttributeDescriptionEn { get; set; }
        public string AttributeNameTh { get; set; }
        public string DisplayNameEn { get; set; }
        public string DisplayNameTh { get; set; }
        public string Status { get; set; }

        public bool VariantStatus { get; set; }
        public string DataType { get; set; }
        public string DataValidation { get; set; }
        public string VariantDataType { get; set; }
        public string DefaultValue { get; set; }
        public bool ShowAdminFlag { get; set; }
        public bool ShowGlobalSearchFlag { get; set; }
        public bool ShowLocalSearchFlag { get; set; }
        public bool ShowGlobalFilterFlag { get; set; }
        public bool ShowLocalFilterFlag { get; set; }
        public bool AllowHtmlFlag { get; set; }
        public bool Filterable { get; set; }
        public bool Required { get; set; }
        public List<AttributeValueRequest> AttributeValues { get; set; }
        public string ValueEn { get; set; }
        public string ValueTh { get; set; }
        public string SearchText { get; set; }
        public int ProductCount { get; set; }
        public bool DefaultAttribute { get; set; }
        public string VisibleTo { get; set; }

        public AttributeRequest()
        {
            AttributeId = 0;
            AttributeNameEn = string.Empty;
            AttributeDescriptionEn = string.Empty;
            AttributeNameTh = string.Empty;
            DisplayNameEn = string.Empty;
            DisplayNameTh = string.Empty;
            Status = string.Empty;
            VariantStatus = false;
            DataType = string.Empty;
            DataValidation = string.Empty;
            VariantDataType = string.Empty;
            DefaultValue = string.Empty;
            ShowAdminFlag = false;
            ShowGlobalSearchFlag = false;
            ShowLocalSearchFlag  = false;
            ShowGlobalFilterFlag = false;
            ShowLocalFilterFlag = false;
            AllowHtmlFlag = false;
            Filterable = false;
            Required = false;
            ValueEn = string.Empty;
            SearchText = string.Empty;
            ProductCount = 0;
            DefaultAttribute = false;
            AttributeValues = new List<AttributeValueRequest>();
            ValueTh = string.Empty;
        }

        public override void DefaultOnNull()
        {
            AttributeId = GetValueOrDefault(AttributeId, 0);
            ValueEn = GetValueOrDefault(ValueEn, null);
            SearchText = GetValueOrDefault(SearchText, null);
            _order = GetValueOrDefault(_order, "AttributeId");
            base.DefaultOnNull();
        }
    }
}
