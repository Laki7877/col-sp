using Colsp.Api.Constants;
using Colsp.Api.Helpers;
using Colsp.Entity.Models;
using Colsp.Model.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;

namespace Colsp.Api.Services
{
	public class CmosService
	{
		public void SendToCmos(ProductStageGroup group, string url, string method
			, string email, DateTime currentDt, ColspEntities db)
		{
			List<CmosProductRequest> requests = new List<CmosProductRequest>();
			Dictionary<string, string> headers = new Dictionary<string, string>();
			headers.Add(Apis.CmosKeyAppIdKey, Apis.CmosKeyAppIdValue);
			headers.Add(Apis.CmosKeyAppSecretKey, Apis.CmosKeyAppSecretValue);
			foreach (var stage in group.ProductStages.Where(w => w.IsSell))
			{
				CmosProductRequest request = new CmosProductRequest()
				{
					BrandId = group.BrandId.HasValue ? group.BrandId.Value : 0,
					BrandNameEng = string.Empty,
					BrandNameThai = string.Empty,
					DeliverFee = stage.DeliveryFee,
					DescriptionFullEng = stage.DescriptionFullEn,
					DescriptionFullThai = stage.DescriptionFullTh,
					DescriptionShortThai = stage.DescriptionShortTh,
					DescriptionShotEng = stage.DescriptionShortEn,
					DocumentNameEng = stage.ProdTDNameEn,
					DocumentNameThai = stage.ProdTDNameTh,
					Height = stage.Height,
					IsBestDeal = group.IsBestSeller,
					IsHasExpiryDate = Constant.STATUS_YES.Equals(stage.IsHasExpiryDate),
					IsVat = Constant.STATUS_YES.Equals(stage.IsVat),
					JDADept = stage.JDADept,
					JDASubDept = stage.JDASubDept,
					Length = stage.Length,
					NameEng = stage.ProductNameEn,
					NameThai = stage.ProductNameTh,
					OriginalPrice = stage.OriginalPrice,
					PrepareDay = stage.PrepareDay,
					ProductID = stage.Pid,
					ProductStatus = stage.Status,
					StockType = stage.Inventory == null ? string.Concat(Constant.STOCK_TYPE[Constant.DEFAULT_STOCK_TYPE]) : string.Concat(stage.Inventory.StockType),
					Remark = group.Remark,
					SalePrice = stage.SalePrice,
					SaleUnitEng = stage.SaleUnitEn,
					SaleUnitThai = stage.SaleUnitTh,
					ShopId = stage.ShopId,
					Sku = stage.Sku,
					Upc = stage.Upc,
					Weight = stage.Weight,
					Width = stage.Width
				};
				var json = new JavaScriptSerializer().Serialize(request);
				SystemHelper.SendRequest(url, method, headers, json, email, currentDt, "SP", "CMOS", db);
			}
		}
	}
}