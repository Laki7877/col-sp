using System;
using System.Linq;
using System.Linq.Expressions;
using System.Linq.Dynamic;
using Colsp.Model.Requests;

namespace Colsp.Api.Extensions
{
	public static class IQueryableExtension
	{
		public static IQueryable<T> Paginate<T>(this IQueryable<T> iq, PaginatedRequest pagination)
		{
			//Generate Lambda Expression for LINQ that will sort
			//It is equivalent to doing OrderBy(p => p.Name) which will order by Name,
			//but dynamically at runtime
			var _order = pagination._order;
            string _dir = pagination._direction.ToUpper().Equals("DESC") ? "DESC" : "ASC";
                
			try {
				iq = iq.OrderBy(_order + " " + _dir);
			} catch (Exception)
			{
               
                pagination._order = null;
                pagination.DefaultOnNull();
                if (string.IsNullOrWhiteSpace(pagination._order))
                {
                    return iq;
                }
                iq = iq.OrderBy(pagination._order + " " + _dir);
            }
			return iq.Skip((int)pagination._offset).Take((int)pagination._limit);
		}
	}
}