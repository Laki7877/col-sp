namespace Colsp.Model.Requests
{
    public class ControlFlagRequest
    {
        public bool IsNew { get; set; }
        public bool IsClearance { get; set; }
        public bool IsBestSeller { get; set; }
        public bool IsOnlineExclusive { get; set; }
        public bool IsOnlyAt { get; set; }

        public ControlFlagRequest()
        {
            IsNew = false;
            IsClearance = false;
            IsBestSeller = false;
            IsOnlineExclusive = false;
            IsOnlyAt = false;
        }
    }
}
