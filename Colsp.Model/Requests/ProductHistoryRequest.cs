using System;

namespace Colsp.Model.Requests
{
    public class ProductHistoryRequest
    {
        public long HistoryId { get; set; }
        public DateTime? ApproveOn { get; set; }
        public DateTime? SubmitOn { get; set; }
        public string SubmitBy { get; set; }

        public ProductHistoryRequest()
        {
            SubmitBy = string.Empty;
        }
    }
}
