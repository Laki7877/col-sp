using Colsp.Api.Constants;
using Colsp.Api.Helpers;
using Colsp.Entity.Models;
using Colsp.Model.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Script.Serialization;

namespace Colsp.Api.Services
{
	public class ElasticService
	{
		public void UpdateByPassFieldElastic(ProductStageGroup group, string url, string method, string email, DateTime currentDt, ColspEntities db)
		{
			decimal priceRangeMin = group.ProductStages.Where(w => w.IsSell).Select(s => s.SalePrice).Min();
			decimal priceRangeMax = group.ProductStages.Where(w => w.IsSell).Select(s => s.SalePrice).Max();
			foreach (var stage in group.ProductStages)
			{
				ElasticByPassRequest request = new ElasticByPassRequest()
				{
					Pid = stage.Pid,
					EffectiveDatePromotion = stage.EffectiveDatePromotion.HasValue ? stage.EffectiveDatePromotion.Value.ToString("yyyy-MM-ddTHH:mm:ss.fffZ") : null,
					ExpireDatePromotion = stage.ExpireDatePromotion.HasValue ? stage.ExpireDatePromotion.Value.ToString("yyyy-MM-ddTHH:mm:ss.fffZ") : null,
					PromotionPrice = stage.PromotionPrice,
					SalePrice = stage.SalePrice,
					PriceRangeMax = priceRangeMax,
					PriceRangeMin = priceRangeMin
				};
				var tmpJson = new
				{
					Pid = request.Pid,
					data = request
				};
				var json = new JavaScriptSerializer().Serialize(tmpJson);
				SystemHelper.SendRequest(url, method, null, json, email, currentDt, "SP", "ELAS", db);
			}
		}

		public void SendToElastic(ProductStageGroup productGroup, string url, string method
			, string email, DateTime currentDt, ColspEntities db)
		{
			List<ElasticRequest> requests = new List<ElasticRequest>();
			decimal priceRangeMin = productGroup.ProductStages.Where(w => w.IsSell).Select(s => s.SalePrice).Min();
			decimal priceRangeMax = productGroup.ProductStages.Where(w => w.IsSell).Select(s => s.SalePrice).Max();
			var masterVariant = productGroup.ProductStages.Where(w => w.IsVariant == false).First();
			var defaultProduct = productGroup.ProductStages.Where(w => w.DefaultVariant).FirstOrDefault();
			#region query
			#region shop
			var shop = db.Shops.Where(w => w.ShopId == productGroup.ShopId).Select(s => new
			{
				s.ShopNameEn,
				s.ShopNameTh,
				s.UrlKey,
				s.ShopGroup,
			}).First();
			#endregion
			#region Brand
			BrandRequest brand = null;
			if (productGroup.BrandId.HasValue)
			{
				brand = db.Brands.Where(w => w.BrandId == productGroup.BrandId).Select(s => new BrandRequest()
				{
					BrandNameEn = s.BrandNameEn,
					BrandNameTh = s.BrandNameTh
				}).First();
			}
			#endregion
			#region Global Category
			var globalCategory = (from node in db.GlobalCategories
								  from parent in db.GlobalCategories
								  where node.Lft >= parent.Lft && node.Lft <= parent.Rgt && node.CategoryId == productGroup.GlobalCatId
								  orderby parent.Lft
								  select new
								  {
									  parent.CategoryId,
									  parent.NameEn,
									  parent.NameTh
								  }).ToList();
			var globalCatRoot = globalCategory.First();
			var globalLeaf = globalCategory.Last();
			#endregion
			#region Local Category
			CategoryRequest localCatRoot = null;
			CategoryRequest localLeaf = null;
			if (productGroup.LocalCatId.HasValue)
			{
				IEnumerable<CategoryRequest> localCategory = (from node in db.LocalCategories
															  from parent in db.LocalCategories
															  where node.ShopId == productGroup.ShopId
																&& parent.ShopId == productGroup.ShopId
																&& node.Lft >= parent.Lft
																&& node.Lft <= parent.Rgt
																&& node.CategoryId == productGroup.LocalCatId.Value
															  orderby parent.Lft
															  select new CategoryRequest()
															  {
																  CategoryId = parent.CategoryId,
																  NameEn = parent.NameEn,
																  NameTh = parent.NameTh
															  }).ToList();
				localCatRoot = localCategory.First();
				localLeaf = localCategory.Last();
			}
			#endregion
			#region Attribute
			var attrIds = productGroup.ProductStages.SelectMany(s => s.ProductStageAttributes.Select(sv => sv.AttributeId));
			var attrValueIds = productGroup.ProductStages.SelectMany(s => s.ProductStageAttributes.Where(w => w.AttributeValueId.HasValue).Select(sv => sv.AttributeValueId.Value));
			var attribute = db.Attributes.Where(w => attrIds.Contains(w.AttributeId)).Select(s => new
			{
				s.AttributeId,
				AttributeNameEn = s.DisplayNameEn,
				AttributeNameTh = s.DisplayNameTh,
				s.ShowLocalSearchFlag,
				s.ShowGlobalSearchFlag,
				IsFilterable = s.Filterable,
				IsVariant = s.VariantStatus,
				s.DataType,
				AttributeValues = s.AttributeValueMaps.Where(w => attrValueIds.Contains(w.AttributeValueId)).Select(sv => new
				{
					sv.AttributeValueId,
					AttributeValueNameEn = sv.AttributeValue.AttributeValueEn,
					AttributeValueNameTh = sv.AttributeValue.AttributeValueTh,
					sv.AttributeValue.ImageUrl
				})
			}).ToList();
			#endregion
			#endregion
			foreach (var stage in productGroup.ProductStages)
			{
				List<ElasticAttributeRequest> responseAttribute = new List<ElasticAttributeRequest>();
				#region Attribute
				foreach (var proAttr in stage.ProductStageAttributes)
				{

					if (string.IsNullOrEmpty(proAttr.ValueEn) && !proAttr.AttributeValueId.HasValue)
					{
						continue;
					}
					if (proAttr.AttributeId == 1)
					{
						continue;
					}
					var att = attribute.Where(w => w.AttributeId == proAttr.AttributeId).Single();
					if (responseAttribute.Any(a => a.AttributeId == att.AttributeId))
					{
						var tmpAttribute = responseAttribute.Where(w => w.AttributeId == att.AttributeId).First();
						if (proAttr.AttributeValueId.HasValue)
						{
							foreach (var val in att.AttributeValues.Where(w => w.AttributeValueId == proAttr.AttributeValueId.Value))
							{
								tmpAttribute.AttributeValues.Add(new ElasticAttributeValueRequest()
								{
									AttributeValueId = val.AttributeValueId,
									AttributeValueNameEn = val.AttributeValueNameEn,
									AttributeValueNameTh = val.AttributeValueNameTh,
									ImageUrl = val.ImageUrl
								});
							}
						}
						continue;
					}
					responseAttribute.Add(new ElasticAttributeRequest()
					{
						AttributeId = att.AttributeId,
						AttributeNameEn = att.AttributeNameEn,
						AttributeNameTh = att.AttributeNameTh,
						IsFilterable = att.IsFilterable,
						IsVariant = att.IsVariant,
						ShowGlobalSearchFlag = att.ShowGlobalSearchFlag,
						ShowLocalSearchFlag = att.ShowLocalSearchFlag,
						AttributeValues = proAttr.AttributeValueId.HasValue ? att.AttributeValues.Where(w => w.AttributeValueId == proAttr.AttributeValueId.Value)
							.Select(sv => new ElasticAttributeValueRequest()
							{
								AttributeValueId = sv.AttributeValueId,
								AttributeValueNameEn = sv.AttributeValueNameEn,
								AttributeValueNameTh = sv.AttributeValueNameTh,
								ImageUrl = sv.ImageUrl
							}).ToList() : new List<ElasticAttributeValueRequest>(),
						FreeTextValueEn = Constant.DATA_TYPE_STRING.Equals(att.DataType) ? proAttr.ValueEn : Constant.DATA_TYPE_HTML.Equals(att.DataType) ? proAttr.HtmlBoxValue : null,
						FreeTextValueTh = Constant.DATA_TYPE_STRING.Equals(att.DataType) ? proAttr.ValueTh : Constant.DATA_TYPE_HTML.Equals(att.DataType) ? proAttr.HtmlBoxValue : null,
						IsAttributeValue = proAttr.IsAttributeValue,
					});
				}
				#endregion
				#region Elastic Object
				ElasticRequest request = new ElasticRequest()
				{
					Pid = stage.Pid,
					ProductId = stage.ProductId,
					ParentPId = stage.IsVariant ? masterVariant.Pid : null,
					Sku = stage.Sku,
					MasterPid = null,
					ShopId = stage.ShopId,
					ShopNameEn = shop.ShopNameEn,
					ShopNameTh = shop.ShopNameTh,
					ShopUrlKey = shop.UrlKey,
					ShopGroup = shop.ShopGroup,
					Bu = stage.Bu,
					GlobalCatId = productGroup.GlobalCatId,
					GlobalCatNameEn = globalLeaf.NameEn,
					GlobalCatNameTh = globalLeaf.NameTh,
					GlobalRootCatId = globalCatRoot.CategoryId,
					GlobalRootCatNameEn = globalCatRoot.NameEn,
					GlobalRootCatNameTh = globalCatRoot.NameTh,
					LocalCatId = productGroup.LocalCatId,
					LocalCatNameEn = localLeaf != null ? localLeaf.NameEn : null,
					LocalCatNameTh = localLeaf != null ? localLeaf.NameTh : null,
					LocalRootCatId = localCatRoot != null ? localLeaf.CategoryId : (int?)null,
					LocalRootCatNameEn = localCatRoot != null ? localLeaf.NameEn : null,
					LocalRootCatNameTh = localCatRoot != null ? localLeaf.NameTh : null,
					BrandId = productGroup.BrandId,
					BrandNameEn = brand != null ? brand.BrandNameEn : null,
					BrandNameTh = brand != null ? brand.BrandNameTh : null,
					ProductNameEn = stage.ProductNameEn,
					ProductNameTh = stage.ProductNameTh,
					Upc = stage.Upc,
					OriginalPrice = stage.OriginalPrice,
					SalePrice = stage.SalePrice,
					PromotionPrice = stage.PromotionPrice,
					PriceRangeMin = priceRangeMin,
					PriceRangeMax = priceRangeMax,
					//Discount = stage,
					//ProductRating = stage,
					DescriptionFullEn = stage.DescriptionFullEn,
					DescriptionFullTh = stage.DescriptionFullTh,
					ImageCount = stage.ImageCount,
					Installment = stage.Installment,
					EffectiveDate = productGroup.EffectiveDate.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
					ExpireDate = productGroup.ExpireDate.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
					EffectiveDatePromotion = stage.EffectiveDatePromotion.HasValue ? stage.EffectiveDatePromotion.Value.ToString("yyyy-MM-ddTHH:mm:ss.fffZ") : null,
					ExpireDatePromotion = stage.ExpireDatePromotion.HasValue ? stage.ExpireDatePromotion.Value.ToString("yyyy-MM-ddTHH:mm:ss.fffZ") : null,
					ExpressDelivery = stage.ExpressDelivery,
					IsNew = productGroup.IsNew,
					IsClearance = productGroup.IsClearance,
					IsBestSeller = productGroup.IsBestSeller,
					IsOnlineExclusive = productGroup.IsOnlineExclusive,
					IsOnlyAt = productGroup.IsOnlyAt,
					MetaKeyEn = stage.MetaKeyEn,
					MetaKeyTh = stage.MetaKeyTh,
					SeoEn = stage.SeoEn,
					SeoTh = stage.SeoTh,
					UrlKey = stage.UrlKey,
					BoostWeight = stage.BoostWeight,
					GlobalBoostWeight = stage.GlobalBoostWeight,
					IsMaster = stage.IsMaster,
					IsVariant = stage.IsVariant,
					IsSell = stage.IsSell,
					Visibility = stage.Visibility,
					Status = stage.Status,
					Display = stage.Display,
					CreateDate = stage.CreateOn.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
					UpdateDate = stage.UpdateOn.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
					NewArrivalDate = stage.NewArrivalDate.HasValue ? stage.NewArrivalDate.Value.ToString("yyyy-MM-ddTHH:mm:ss.fffZ") : null,
					IsSearch = stage.ProductStageAttributes.Any(a => a.AttributeValueId == 1 && a.CheckboxValue),
					IsLocalSearch = stage.ProductStageAttributes.Any(a => a.AttributeValueId == 2 && a.CheckboxValue),
					IsGlobalSearch = stage.ProductStageAttributes.Any(a => a.AttributeValueId == 3 && a.CheckboxValue),
					StockAvailable = stage.Inventory != null ?
						(
							stage.Inventory.Quantity
							- stage.Inventory.Reserve
							- stage.Inventory.OnHold
							- stage.Inventory.Defect
						) : 0,
					MaxQtyAllowInCart = stage.Inventory != null ? stage.Inventory.MaxQtyAllowInCart : 0,
					MinQtyAllowInCart = stage.Inventory != null ? stage.Inventory.MinQtyAllowInCart : 0,
					tag = productGroup.ProductStageTags.Select(s => s.Tag).ToList(),
					DefaultProduct = defaultProduct != null && stage.IsVariant ? new DefaultProductRequest()
					{
						Pid = defaultProduct.Pid,
						ShopUrlKey = shop.UrlKey
					} : null,
					Attributes = responseAttribute,
				};
				#endregion
				var tmpJson = new
				{
					Pid = request.Pid,
					data = request
				};
				var json = new JavaScriptSerializer().Serialize(tmpJson);
				SystemHelper.SendRequest(url, method, null, json, email, currentDt, "SP", "ELAS", db);
			}
		}

	}
}