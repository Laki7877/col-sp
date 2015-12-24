using Colsp.Model.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colsp.Model.Responses
{
	public class PaginatedResponse
	{
		// Factory method
		public static PaginatedResponse CreateResponse(IQueryable data, PaginatedRequest pagination, int total)
		{
			return CreateResponse(data, (int)pagination._offset, (int)pagination._limit, total, pagination._order);
		}
		public static PaginatedResponse CreateResponse(IQueryable data, int offset, int limit, int total, String order)
		{
			var response = new PaginatedResponse();
			response.data = data;
			response.offset = offset;
			response.limit = limit;
			response.total = total;
			response.order = order;
			return response;
		}
		public int total { get; set; }
		public int offset { get; set; }
		public int limit { get; set; }
		public string order { get; set; }
		public IQueryable data { get; set; }
	}
}
