using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colsp.Model.Responses
{
	public class PaginatedResponse<DbModel>
	{
		// Factory method
		public static PaginatedResponse<DbModel> CreateResponse(IQueryable<DbModel> data, int offset, int limit, int total, String order)
		{
			var response = new PaginatedResponse<DbModel>();
			response.data = data;
			response.offset = offset;
			response.limit = limit;
			response.total = total;
			response.order = order;
			return response;
		}
		public int total;
		public int offset;
		public int limit;
		public String order;
		public IQueryable<DbModel> data;
	}
}
