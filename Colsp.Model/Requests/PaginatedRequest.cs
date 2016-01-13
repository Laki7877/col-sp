using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colsp.Model.Requests
{
	public class PaginatedRequest : Request
	{
		public int? _limit { get; set; }
		public int? _offset { get; set; }
		public string _order { get; set; }
		public string _direction { get; set; }
        public string _filter { get; set; }

		public override void DefaultOnNull()
		{
			_limit = GetValueOrDefault(_limit, 10);
			_offset = GetValueOrDefault(_offset, 0);
			_order = GetValueOrDefault(_order, "");
			_direction = GetValueOrDefault(_direction, "asc");
            _filter = GetValueOrDefault(_filter, "");

            base.DefaultOnNull();
		}
	}
}
