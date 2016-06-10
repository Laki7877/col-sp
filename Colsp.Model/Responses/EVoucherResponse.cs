using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colsp.Model.Responses
{
	public class EVoucherResponse
	{
		public EVoucher evoucher { get; set; }
		public string returncode { get; set; }
		public  Dictionary<string,string> message { get; set; }
	}

	public class EVoucher
	{
		public string id { get; set; }
		public List<string> voucherno { get; set; }
		public string vouchername { get; set; }
		public List<string> email { get; set; }
		//voucheruser
		public DateTime? effectivedate { get; set; }
		public DateTime? expireddate { get; set; }
		public int? limit { get; set; }
		public int? limitperuser { get; set; }
		public decimal? discountbaht { get; set; }
		public decimal? discountpercent { get; set; }
		public decimal? maximumdiscount { get; set; }
		public bool? status { get; set; }
		public List<Criteria> includecriteria { get; set; }
		public List<Criteria> excludecriteria { get; set; }
	}

	public class Criteria
	{
		public int? quantity { get; set; }
		public decimal? amount { get; set; }
		public List<string> productids { get; set; }
		public List<int> localcategories { get; set; }
		public List<int> shopids { get; set; }
		public List<int> globalcategories { get; set; }
	}
}
