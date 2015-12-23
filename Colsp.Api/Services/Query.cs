using System;
using System.Linq;
using System.Linq.Expressions;
using System.Linq.Dynamic;

namespace Colsp.Api.Services
{
	public static class Query
	{
		public static IQueryable<DbModel> ChainPaginationFilter<DbModel>(IQueryable<DbModel> iq, String _order, int _start, int _max, bool? _reverse)
		{
			//Generate Lambda Expression for LINQ that will sort
			//It is equivalent to doing OrderBy(p => p.Name) which will order by Name,
			//but dynamically at runtime

			var _dir = _reverse == true ? "DESC" : "ASC";
			iq = iq.OrderBy(_order + " " + _dir);
				
			return iq.Skip(_start).Take(_max);
		}
	}
}