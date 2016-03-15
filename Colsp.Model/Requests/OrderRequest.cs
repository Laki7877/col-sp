namespace Colsp.Model.Requests
{
    public class OrderRequest
    {
        public string Type { get; set; }
        public decimal Value { get; set; }

        public OrderRequest()
        {
            Type = string.Empty;
            Value = 0;
        }
    }
}
