using System;

namespace Colsp.Model.Requests
{
    public class ProductHistoryRequest
    {
        public long HistoryId { get; set; }
        public DateTime? ApprovedDt { get; set; }
        public DateTime? SubmittedDt { get; set; }
        public string SubmittedBy { get; set; }

        public ProductHistoryRequest()
        {
            SubmittedBy = string.Empty;
        }
    }
}
