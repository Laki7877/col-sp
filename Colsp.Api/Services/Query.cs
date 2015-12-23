using System;
using System.Linq;
using System.Linq.Expressions;

namespace Colsp.Api.Services
{
	public static class Query
	{
		public static IQueryable<DbModel> ChainPaginationFilter<DbModel>(IQueryable<DbModel> iq, String _order, int _start, int _max, bool? _reverse)
		{
			//Generate Lambda Expression for LINQ that will sort
			//It is equivalent to doing OrderBy(p => p.Name) which will order by Name,
			//but dynamically at runtime

			var type = typeof(DbModel);
			var property = type.GetProperty(_order);
			var parameter = Expression.Parameter(type, "p");
			var propertyAccess = Expression.MakeMemberAccess(parameter, property);
			Expression<Func<DbModel, string>> lambda = Expression.Lambda<Func<DbModel, string>>(propertyAccess, parameter);

			iq = iq.OrderBy<DbModel, String>(lambda);

			//Sorting and Limiting
			//If use want the result in reverse order
			if (_reverse != null && _reverse.Value == true)
			{
				iq = iq.OrderByDescending<DbModel, String>(lambda);
			}

			return iq.Skip(_start).Take(_max);
		}
	}
}