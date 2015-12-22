using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Colsp.Api.Models
{
	public class PaginatedRequest
	{
		public int _limit { get; set; }
		public int _offset { get; set; }
		public string _order { get; set; }
		public string _direction { get; set; }
	}
}