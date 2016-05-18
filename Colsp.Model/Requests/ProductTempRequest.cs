namespace Colsp.Model.Requests
{
    public class UnGroupProductRequest : PaginatedRequest
    {
        public long ProductId { get; set; }
        public string Pid { get; set; }
        public string Sku { get; set; }
        public int ShopId { get; set; }
        public int CategoryId { get; set; }
        public string ProductNameEn { get; set; }
        public string ProductNameTh { get; set; }
        public string UrlEn { get; set; }
        public decimal OriginalPrice { get; set; }
        public decimal SalePrice  { get; set; }
        public int Quantity  { get; set; }
        public string JdaCategoryId { get; set; }
        public int OnHold { get; set; }
        public int AttributeSetId { get; set; }
        public string SearchText { get; set; }

        public override void DefaultOnNull()
        {
            SearchText = GetValueOrDefault(SearchText, string.Empty);
            _order = GetValueOrDefault(_order, "Pid");
            base.DefaultOnNull();
        }

    }
}
