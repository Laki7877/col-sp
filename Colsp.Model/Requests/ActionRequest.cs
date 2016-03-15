namespace Colsp.Model.Requests
{
    public class ActionRequest
    {
        public string Type { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal MaximumAmount { get; set; }

        public ActionRequest()
        {
            Type = string.Empty;
            DiscountAmount = 0;
            MaximumAmount = 0;
        }

    }
}
