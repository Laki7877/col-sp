namespace Colsp.Model.Requests
{
    public class ControlFlagRequest
    {
        public bool Flag1 { get; set; }
        public bool Flag2 { get; set; }
        public bool Flag3 { get; set; }

        public ControlFlagRequest()
        {
            Flag1 = false;
            Flag2 = false;
            Flag3 = false;
        }
    }
}
