using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Colsp.Api.Models
{
	public class UserRequest : PaginatedRequest
	{
		public string Name { get; set; }
		public int ShopId { get; set; }
	}
}