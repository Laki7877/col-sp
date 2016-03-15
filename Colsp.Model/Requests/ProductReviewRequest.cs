namespace Colsp.Model.Requests
{
    public class ProductReviewRequest : PaginatedRequest
    {
        public int ProductReviewId { get; set; }
        public string Status { get; set; }
        public string Pid { get; set;}
        public string SearchText { get; set; }

        public ProductReviewRequest()
        {
            ProductReviewId = 0;
            Status = string.Empty;
            Pid = string.Empty;
            SearchText = string.Empty;
        }

        public override void DefaultOnNull()
        {
            SearchText = GetValueOrDefault(SearchText, null);
            Pid = GetValueOrDefault(Pid, null);
            _order = GetValueOrDefault(_order, "Pid");
            base.DefaultOnNull();
        }
    }
}
