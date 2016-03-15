namespace Colsp.Model.Requests
{
    public class CouponRequest : PaginatedRequest
    {
        public int CouponId { get; set; }
        public string CouponCode { get; set; }
        public string CouponName { get; set; }

        public string StartDate { get; set; }
        public string ExpireDate { get; set; }

        public string Status { get; set; }
        public int Remaining { get; set; }
        public string SearchText { get; set; }
        public ActionRequest Action { get; set; }
        public ConditionRequest Conditions { get; set; }
        public int UsagePerCustomer { get; set; }
        public int MaximumUser { get; set; }
        public bool IsGlobalCoupon { get; set; }

        public override void DefaultOnNull()
        {
            SearchText = GetValueOrDefault(SearchText, null);
            _order = GetValueOrDefault(_order, "CouponName");
            base.DefaultOnNull();
        }

        public CouponRequest()
        {
            CouponId = 0;
            CouponCode = string.Empty;
            CouponName = string.Empty;
            Status = string.Empty;
            Remaining = 0;
            SearchText = string.Empty;

            Action = new ActionRequest();
            Conditions = new ConditionRequest();
            UsagePerCustomer = 0;
            MaximumUser = 0;
        }
    }

}
