namespace Colsp.Model.Requests
{
    public class ShopCommission
    {
        public int CategoryId { get; set; }
        public decimal Commission { get; set; }

        public ShopCommission()
        {
            CategoryId = 0;
            Commission = 0;
        }
    }
}
