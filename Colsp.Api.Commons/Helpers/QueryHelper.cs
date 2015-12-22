using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;

namespace Colsp.Api.Commons
{
	public static class QueryHelper
	{
		public static IQueryable<Model> ChainPaginatedQuery<Model>(IQueryable<Model> iq, String _order, int _start, int _max, bool? _reverse)
		{
			//Generate Lambda Expression for LINQ that will sort
			//It is equivalent to doing OrderBy(p => p.Name) which will order by Name,
			//but dynamically at runtime

			var type = typeof(Model);
			var property = type.GetProperty(_order);
			var parameter = Expression.Parameter(type, "p");
			var propertyAccess = Expression.MakeMemberAccess(parameter, property);
			Expression<Func<Model, string>> lambda = Expression.Lambda<Func<Model, string>>(propertyAccess, parameter);

			iq = iq.OrderBy<Model, String>(lambda);

			//Sorting and Limiting
			//If use want the result in reverse order
			if (_reverse != null && _reverse.Value == true)
			{
				iq = iq.OrderByDescending<Model, String>(lambda);
			}

			return iq.Skip(_start).Take(_max);
		}
	}
}