namespace Colsp.Model.Requests
{
    public class ProductTempRequest : PaginatedRequest
    {
        public long ProductId { get; set; }
        public string Pid { get; set; }
        public string Sku { get; set; }
        public int ShopId { get; set; }
        public string ProductNameEn { get; set; }
        public string ProductNameTh { get; set; }
        public string UrlEn { get; set; }
        public decimal OriginalPrice { get; set; }
        public decimal SalePrice  { get; set; }
        public int Quantity  { get; set; }
        public string JdaCategoryId { get; set; }
        public int OnHold { get; set; }

    }
}
