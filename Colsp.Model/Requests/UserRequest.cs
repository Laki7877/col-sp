﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colsp.Model.Requests
{
	public class UserRequest : PaginatedRequest
	{
		public string Name { get; set; }
		public override void DefaultOnNull()
		{
			Name = GetValueOrDefault(Name, "");
			_order = GetValueOrDefault(_order, "UserId");
			base.DefaultOnNull();
		}
	}
}