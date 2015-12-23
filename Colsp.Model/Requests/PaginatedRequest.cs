using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colsp.Model.Requests
{
	public class PaginatedRequest
	{
		[DefaultValue(20)]
		public int _limit;
		[DefaultValue(0)]
		public int _offset;
		public String _order;
		public String _direction;
	}
}
