using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colsp.Model.Requests
{
	public class UserRequest : PaginatedRequest
	{
		public string Name { get; set; }
		protected override void _DefaultOnNull()
		{
			Name = GetValueOrDefault(Name, "");
			base._DefaultOnNull();
		}
	}
}
