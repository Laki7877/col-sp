﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colsp.Model.Requests
{
	public class ProductRequest : PaginatedRequest
	{
		public int? SellerId;
		public string Sku; 
		public override void DefaultOnNull()
		{
			SellerId = GetValueOrDefault(SellerId, null);
			Sku = GetValueOrDefault(Sku, null);
			_order = GetValueOrDefault(_order, "ProductId");
			base.DefaultOnNull();
		}
	}
}