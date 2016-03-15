namespace Colsp.Model.Requests
{
    public class OnBoardRequest
    {
        public bool ChangePassword  { get; set; }
        public bool SetUpShop       { get; set; }
        public bool AddProduct      { get; set; }
        public bool ProductApprove  { get; set; }
        public bool DecorateStore   { get; set; }

        public OnBoardRequest()
        {
            ChangePassword = false;
            SetUpShop      = false;
            AddProduct     = false;
            ProductApprove = false;
            DecorateStore  = false;
        }
    }
}
