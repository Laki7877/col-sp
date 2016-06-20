using Colsp.Api.Constants;
using Colsp.Api.Helpers;
using Colsp.Entity.Models;
using Colsp.Model.Requests;
using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;

namespace Colsp.Api.Services
{
	public class ProductStageService
	{

		public void SetupProductStageGroup(ProductStageGroup group, ProductStageRequest request, List<AttributeRequest> attributeList, List<Inventory> inventoryList, int shopId, bool isAdmin, bool isNew, string email, DateTime currentDt, ColspEntities db)
		{
			group.ShopId = shopId;
			#region setup category
			if (request.MainGlobalCategory != null && request.MainGlobalCategory.CategoryId != 0)
			{
				var globalCategory = db.GlobalCategories.Where(w => w.CategoryId == request.MainGlobalCategory.CategoryId && w.Rgt - w.Lft == 1)
					.Select(s => new
					{
						s.NameEn,
						s.Visibility,
						s.Status
					}).FirstOrDefault();
				if (globalCategory != null)
				{
					if (!globalCategory.Visibility || !Constant.STATUS_ACTIVE.Equals(globalCategory.Status))
					{
						throw new Exception(string.Concat("Global category ", globalCategory.NameEn, " is not active."));
					}
					group.GlobalCatId = request.MainGlobalCategory.CategoryId;
				}
				else
				{
					throw new Exception("Invalid global category id");
				}
			}
			else
			{
				throw new Exception("Invalid global category id");
			}
			group.LocalCatId = null;
			if (request.MainLocalCategory != null && request.MainLocalCategory.CategoryId != 0)
			{
				group.LocalCatId = request.MainLocalCategory.CategoryId;
			}
			if (request.GlobalCategories != null)
			{
				foreach (var category in request.GlobalCategories)
				{
					group.ProductStageGlobalCatMaps.Add(new ProductStageGlobalCatMap()
					{
						CategoryId = category.CategoryId,
						CreateBy = email,
						CreateOn = currentDt,
						UpdateBy = email,
						UpdateOn = currentDt,
					});
				}
			}
			if (request.LocalCategories != null)
			{
				foreach (var category in request.LocalCategories)
				{
					group.ProductStageLocalCatMaps.Add(new ProductStageLocalCatMap()
					{
						CategoryId = category.CategoryId,
						CreateBy = email,
						CreateOn = currentDt,
						UpdateBy = email,
						UpdateOn = currentDt,
					});
				}
			}
			#endregion
			#region setup other mapping
			group.AttributeSetId = null;
			if (request.AttributeSet != null && request.AttributeSet.AttributeSetId != 0)
			{
				group.AttributeSetId = request.AttributeSet.AttributeSetId;
			}
			group.BrandId = null;
			if (request.Brand != null && request.Brand.BrandId != 0)
			{
				group.BrandId = request.Brand.BrandId;
			}
			if (request.ShippingMethod != 0)
			{
				group.ShippingId = request.ShippingMethod;
			}
			else
			{
				group.ShippingId = Constant.DEFAULT_SHIPPING_ID;
			}
			if (request.Tags != null)
			{
				foreach (var tag in request.Tags)
				{
					var tmpTag = tag.Trim();
					if (tmpTag.Length > 30)
					{
						throw new Exception("Tag field must be no longer than 30 characters.");
					}
					if (group.ProductStageTags.Any(a => string.Equals(a.Tag, tmpTag, StringComparison.OrdinalIgnoreCase)))
					{
						continue;
					}
					if (group.ProductStageTags.Count >= 30)
					{
						throw new Exception("A product can have only 30 tags.");
					}
					group.ProductStageTags.Add(new ProductStageTag()
					{
						CreateBy = email,
						CreateOn = currentDt,
						Tag = tmpTag,
						UpdateBy = email,
						UpdateOn = currentDt
					});
				}
			}
			if (request.RelatedProducts != null && request.RelatedProducts.Count > 0)
			{
				foreach (var product in request.RelatedProducts)
				{
					group.ProductStageRelateds1.Add(new ProductStageRelated()
					{
						Child = product.ProductId,
						ShopId = group.ShopId,
						CreateBy = email,
						CreateOn = currentDt,
						UpdateBy = email,
						UpdateOn = currentDt
					});
				}
			}
			#endregion
			#region setup other field
			group.EffectiveDate = request.EffectiveDate.HasValue ? request.EffectiveDate.Value : currentDt;
			group.ExpireDate = request.ExpireDate.HasValue && request.ExpireDate.Value.CompareTo(group.EffectiveDate) >= 0 ? request.ExpireDate.Value : group.EffectiveDate.AddYears(Constant.DEFAULT_ADD_YEAR);
			group.TheOneCardEarn = request.TheOneCardEarn;
			group.GiftWrap = request.GiftWrap;
			group.Status = request.Status;
			group.Remark = request.Remark;
			if (request.ControlFlags != null)
			{
				group.IsNew = request.ControlFlags.IsNew;
				group.IsClearance = request.ControlFlags.IsClearance;
				group.IsBestSeller = request.ControlFlags.IsBestSeller;
				group.IsOnlineExclusive = request.ControlFlags.IsOnlineExclusive;
				group.IsOnlyAt = request.ControlFlags.IsOnlyAt;
			}
			if (isNew)
			{
				group.InformationTabStatus = Constant.PRODUCT_STATUS_WAIT_FOR_APPROVAL;
				group.ImageTabStatus = Constant.PRODUCT_STATUS_WAIT_FOR_APPROVAL;
				group.CategoryTabStatus = Constant.PRODUCT_STATUS_WAIT_FOR_APPROVAL;
				group.VariantTabStatus = Constant.PRODUCT_STATUS_WAIT_FOR_APPROVAL;
				group.MoreOptionTabStatus = Constant.PRODUCT_STATUS_WAIT_FOR_APPROVAL;
				group.RejectReason = string.Empty;
				group.FirstApproveBy = null;
				group.FirstApproveOn = null;
				group.ApproveBy = null;
				group.ApproveOn = null;
				group.RejecteBy = null;
				group.RejectOn = null;
				group.SubmitBy = null;
				group.SubmitOn = null;
				group.CreateBy = email;
				group.CreateOn = currentDt;
			}
			if (isAdmin)
			{
				group.InformationTabStatus = request.AdminApprove.Information;
				group.ImageTabStatus = request.AdminApprove.Image;
				group.CategoryTabStatus = request.AdminApprove.Category;
				group.VariantTabStatus = request.AdminApprove.Variation;
				group.MoreOptionTabStatus = request.AdminApprove.MoreOption;
				group.RejectReason = request.AdminApprove.RejectReason;
			}
			if (Constant.PRODUCT_STATUS_APPROVE.Equals(group.Status))
			{
				group.ApproveBy = email;
				group.ApproveOn = currentDt;
				if (group.FirstApproveBy == null)
				{
					group.FirstApproveBy = email;
					group.FirstApproveOn = currentDt;
				}
			}
			else if (Constant.PRODUCT_STATUS_WAIT_FOR_APPROVAL.Equals(group.Status))
			{
				group.SubmitBy = email;
				group.SubmitOn = currentDt;
			}
			else if (Constant.PRODUCT_STATUS_NOT_APPROVE.Equals(group.Status))
			{
				group.RejecteBy = email;
				group.RejectOn = currentDt;
			}
			group.NewArrivalDate = request.NewArrivalDate;
			if (group.NewArrivalDate == null)
			{
				group.NewArrivalDate = group.CreateOn;
			}
			group.UpdateBy = email;
			group.UpdateOn = currentDt;
			#endregion
			#region setup master
			if (request.MasterVariant == null)
			{
				throw new Exception("Invalid master variant");
			}

			ProductStage masterVariant = group.ProductStages.Where(w => w.Pid.Equals(request.MasterVariant.Pid)).SingleOrDefault();
			if (masterVariant == null)
			{
				masterVariant = new ProductStage()
				{
					IsVariant = false,
				};
				group.ProductStages.Add(masterVariant);
			}
			request.MasterVariant.Status = group.Status;
			SetupProductStage(masterVariant, request.MasterVariant, attributeList, inventoryList.Where(w => w.Pid.Equals(masterVariant.Pid)).SingleOrDefault(), shopId, group.ShippingId, isAdmin, isNew, email, currentDt, db);

			if (request.MasterAttribute != null)
			{
				SetupAttribute(masterVariant, request.MasterAttribute, attributeList, email, currentDt);
			}

			#endregion
			#region setup variant
			if (request.Variants != null)
			{
				var varaintList = group.ProductStages.Where(w => !Constant.STATUS_REMOVE.Equals(group.Status)).ToList();
				foreach (var variantRequest in request.Variants)
				{
					bool isNewProduct = false;
					ProductStage variant = null;
					if (varaintList == null || varaintList.Count == 0)
					{
						isNewProduct = true;
					}
					if (!isNewProduct)
					{
						var currentProduct = varaintList.Where(w => w.Pid != null && w.Pid.Equals(variantRequest.Pid)).SingleOrDefault();
						if (currentProduct != null)
						{
							variant = currentProduct;
							varaintList.Remove(currentProduct);
						}
						else
						{
							isNewProduct = true;
						}
					}

					if (isNewProduct)
					{
						variant = new ProductStage()
						{
							CreateBy = email,
							CreateOn = currentDt
						};
						group.ProductStages.Add(variant);
					}
					variantRequest.Status = group.Status;
					SetupProductStage(variant, variantRequest, attributeList, inventoryList.Where(w => w.Pid.Equals(variant.Pid)).SingleOrDefault(), shopId, group.ShippingId, isAdmin, isNew, email, currentDt, db);
					variant.IsVariant = true;
					variant.IsSell = true;
				}
				masterVariant.VariantCount = request.Variants.Where(w => w.Visibility == true).Count();
			}
			if (masterVariant.VariantCount == 0 && !masterVariant.IsMaster)
			{
				masterVariant.IsSell = true;
			}
			#endregion
			#region Check Flag
			if (!string.IsNullOrWhiteSpace(masterVariant.ProductNameEn)
				&& !string.IsNullOrWhiteSpace(masterVariant.ProductNameTh)
				&& !string.IsNullOrWhiteSpace(masterVariant.Sku)
				&& group.BrandId != null)
			{
				group.InfoFlag = true;
				var masterAttrIds = masterVariant.ProductStageAttributes.Where(w => !string.IsNullOrWhiteSpace(w.ValueEn)).Select(s => s.AttributeId);
				var defaultAttr = db.Attributes.Where(w => w.Required && !Constant.DATA_TYPE_CHECKBOX.Equals(w.DataType) && w.DefaultAttribute).Select(s => s.AttributeId);
				foreach (var id in defaultAttr)
				{
					if (!masterAttrIds.Contains(id))
					{
						group.InfoFlag = false;
						break;
					}
				}
				if (group.AttributeSetId.HasValue && group.InfoFlag)
				{
					var masterAttr = db.Attributes.Where(w => w.Required && !Constant.DATA_TYPE_CHECKBOX.Equals(w.DataType) && w.AttributeSetMaps.Any(a => a.AttributeSetId == group.AttributeSetId.Value)).Select(s => s.AttributeId);
					foreach (var id in masterAttr)
					{
						if (!masterAttrIds.Contains(id))
						{
							group.InfoFlag = false;
							break;
						}
					}
				}
			}
			else
			{
				group.InfoFlag = false;
			}
			if (!string.IsNullOrEmpty(masterVariant.FeatureImgUrl))
			{
				group.ImageFlag = true;
			}
			else
			{
				group.ImageFlag = false;
			}
			#endregion
			if (Constant.RETURN_STATUS_APPROVE.Equals(group.Status))
			{
				//setup approve product
				SetupApprovedProduct(group, db);
			}
			else if (!string.IsNullOrEmpty(group.ApproveBy))
			{
				//update by pass field
				UpdateByPassField(group, db);
			}
		}

		private void SetupProductStage(ProductStage stage, VariantRequest request, List<AttributeRequest> attributeList, Inventory inventory, int shopId, int shippingId, bool isAdmin, bool isNew, string email, DateTime currentDt, ColspEntities db)
		{
			stage.Pid = request.Pid;
			stage.ShopId = shopId;
			stage.ProductNameEn = Validation.ValidateString(request.ProductNameEn, "Product Name (English)", true, 255, false);
			stage.ProductNameTh = Validation.ValidateString(request.ProductNameTh, "Product Name (Thai)", true, 255, false);
			stage.ProdTDNameTh = Validation.ValidateString(request.ProdTDNameTh, "Short Product Name (Thai)", true, 55, false, string.Empty);
			stage.ProdTDNameEn = Validation.ValidateString(request.ProdTDNameEn, "Short Product Name (English)", true, 55, false, string.Empty);
			stage.JDADept = string.Empty;
			stage.JDASubDept = string.Empty;
			stage.SaleUnitTh = Validation.ValidateString(request.SaleUnitTh, "Sale Unit (Thai)", true, 255, false, string.Empty);
			stage.SaleUnitEn = Validation.ValidateString(request.SaleUnitEn, "Sale Unit (English)", true, 255, false, string.Empty);
			stage.Sku = Validation.ValidateString(request.Sku, "SKU", true, 255, false,
					Constant.PRODUCT_STATUS_DRAFT.Equals(request.Status) || !request.Visibility ? string.Empty : null);
			#region UPC
			stage.Upc = Validation.ValidateString(request.Upc, "UPC", true, 13, false, string.Empty);
			if (!string.IsNullOrEmpty(stage.Upc) && !Regex.IsMatch(stage.Upc, @"^[0-9]*$"))
			{
				throw new Exception("Invalid UPC");
			}
			#endregion
			stage.OriginalPrice = request.OriginalPrice;
			if (!Constant.IGNORE_PRICE_SHIPPING.Contains(shippingId))
			{
				stage.SalePrice = request.SalePrice;
			}
			stage.DescriptionFullEn = Validation.ValidateString(request.DescriptionFullEn, "Description (English)", true, 50000, false, string.Empty);
			stage.DescriptionShortEn = Validation.ValidateString(request.DescriptionShortEn, "Short Description (English)", true, 500, false, string.Empty);
			stage.DescriptionFullTh = Validation.ValidateString(request.DescriptionFullTh, "Description (Thai)", true, 50000, false, string.Empty);
			stage.DescriptionShortTh = Validation.ValidateString(request.DescriptionShortTh, "Short Description (Thai)", true, 500, false, string.Empty);
			stage.MobileDescriptionEn = Validation.ValidateString(request.MobileDescriptionEn, "Mobile Description (English)", true, 50000, false, string.Empty);
			stage.MobileDescriptionTh = Validation.ValidateString(request.MobileDescriptionTh, "Mobile Description (Thai)", true, 50000, false, string.Empty);
			stage.ImageCount = 0;
			#region Images
			if (request.Images != null)
			{
				stage.ImageCount = request.Images.Count;
				int position = 1;
				foreach (var img in request.Images)
				{
					if (string.IsNullOrWhiteSpace(img.Url))
					{
						continue;
					}
					stage.ProductStageImages.Add(new ProductStageImage()
					{
						ImageId = img.ImageId,
						FeatureFlag = position == 1 ? true : false,
						Large = true,
						Normal = true,
						Thumbnail = true,
						Zoom = true,
						CreateBy = email,
						CreateOn = currentDt,
						SeqNo = position++,
						ShopId = shopId,
						Status = Constant.STATUS_ACTIVE,
						ImageName = img.Url.Split('/').Last(),
						UpdateBy = email,
						UpdateOn = currentDt
					});
				}
				stage.FeatureImgUrl = stage.ProductStageImages.Where(w => w.FeatureFlag == true).Select(s => s.ImageName).FirstOrDefault();
				if (stage.FeatureImgUrl == null)
				{
					stage.FeatureImgUrl = string.Empty;
				}
			}
			#endregion
			#region Video
			if (request.VideoLinks != null)
			{
				int position = 1;
				foreach (var video in request.VideoLinks)
				{
					if (string.IsNullOrWhiteSpace(video.Url))
					{
						continue;
					}
					stage.ProductStageVideos.Add(new ProductStageVideo()
					{
						CreateBy = email,
						CreateOn = currentDt,
						Position = position++,
						ShopId = shopId,
						Status = Constant.STATUS_ACTIVE,
						VideoUrlEn = video.Url,
						UpdateBy = email,
						UpdateOn = currentDt,
						VideoId = video.VideoId
					});
				}
			}
			#endregion
			stage.PrepareDay = request.PrepareDay;
			stage.LimitIndividualDay = request.LimitIndividualDay;
			stage.PrepareMon = request.PrepareMon;
			stage.PrepareTue = request.PrepareTue;
			stage.PrepareWed = request.PrepareWed;
			stage.PrepareThu = request.PrepareThu;
			stage.PrepareFri = request.PrepareFri;
			stage.PrepareSat = request.PrepareSat;
			stage.PrepareSun = request.PrepareSun;
			stage.KillerPoint1En = Validation.ValidateString(request.KillerPoint1En, "Killer Point 1 (English)", true, 50, false, string.Empty);
			stage.KillerPoint2En = Validation.ValidateString(request.KillerPoint2En, "Killer Point 2 (English)", true, 50, false, string.Empty);
			stage.KillerPoint3En = Validation.ValidateString(request.KillerPoint3En, "Killer Point 3 (English)", true, 50, false, string.Empty);
			stage.KillerPoint1Th = Validation.ValidateString(request.KillerPoint1Th, "Killer Point 1 (Thai)", true, 50, false, string.Empty);
			stage.KillerPoint2Th = Validation.ValidateString(request.KillerPoint2Th, "Killer Point 2 (Thai)", true, 50, false, string.Empty);
			stage.KillerPoint3Th = Validation.ValidateString(request.KillerPoint3Th, "Killer Point 3 (Thai)", true, 50, false, string.Empty);
			stage.Installment = Validation.ValidateString(request.Installment, "Installment", true, 1, false, Constant.STATUS_NO, new List<string>() { Constant.STATUS_NO, Constant.STATUS_YES });
			stage.Length = request.Length;
			stage.Height = request.Height;
			stage.Width = request.Width;
			stage.DimensionUnit = Validation.ValidateString(request.DimensionUnit, "Dimension Unit", true, 2, false, Constant.DIMENSTION_MM, new List<string>() { Constant.DIMENSTION_MM, Constant.DIMENSTION_CM, Constant.DIMENSTION_M });
			if (Constant.DIMENSTION_CM.Equals(stage.DimensionUnit))
			{
				stage.Length = stage.Length * 10;
				stage.Height = stage.Height * 10;
				stage.Width = stage.Width * 10;
			}
			else if (Constant.DIMENSTION_M.Equals(stage.DimensionUnit))
			{
				stage.Length = stage.Length * 1000;
				stage.Height = stage.Height * 1000;
				stage.Width = stage.Width * 1000;
			}
			stage.Weight = request.Weight;
			stage.WeightUnit = Validation.ValidateString(request.WeightUnit, "Weight Unit", true, 2, false, Constant.WEIGHT_MEASURE_G, new List<string>() { Constant.WEIGHT_MEASURE_G, Constant.WEIGHT_MEASURE_KG });
			if (Constant.WEIGHT_MEASURE_KG.Equals(request.WeightUnit))
			{
				stage.Weight = stage.Weight * 1000;
			}
			if (request.SEO != null)
			{
				stage.MetaTitleEn = Validation.ValidateString(request.SEO.MetaTitleEn, "Meta Title (English)", true, 60, false, string.Empty);
				stage.MetaTitleTh = Validation.ValidateString(request.SEO.MetaTitleTh, "Meta Title (Thai)", true, 60, false, string.Empty);
				stage.MetaDescriptionEn = Validation.ValidateString(request.SEO.MetaDescriptionEn, "Meta Description (English)", true, 150, false, string.Empty);
				stage.MetaDescriptionTh = Validation.ValidateString(request.SEO.MetaDescriptionTh, "Meta Description (Thai)", true, 150, false, string.Empty);
				stage.MetaKeyEn = Validation.ValidateString(request.SEO.MetaKeywordEn, "Meta Keywords (English)", true, 1000, false, string.Empty);
				stage.MetaKeyTh = Validation.ValidateString(request.SEO.MetaKeywordTh, "Meta Keywords (Thai)", true, 1000, false, string.Empty);
				stage.SeoEn = Validation.ValidateString(request.SEO.SeoEn, "SEO (English)", true, 1000, false, string.Empty);
				stage.SeoTh = Validation.ValidateString(request.SEO.SeoTh, "SEO (Thai)", true, 1000, false, string.Empty);
				stage.UrlKey = Validation.ValidateString(request.SEO.ProductUrlKeyEn, "Product URL Key", true, 100, false, string.Empty);
				stage.BoostWeight = request.SEO.ProductBoostingWeight;
				if (isAdmin)
				{
					stage.GlobalBoostWeight = request.SEO.GlobalProductBoostingWeight;
				}
			}
			stage.IsHasExpiryDate = Validation.ValidateString(request.IsHasExpiryDate, "Has Expiry Date", true, 1, false, Constant.STATUS_NO, new List<string>() { Constant.STATUS_NO, Constant.STATUS_YES });
			stage.IsVat = Validation.ValidateString(request.IsVat, "Is Vat", true, 1, false, Constant.STATUS_NO, new List<string>() { Constant.STATUS_NO, Constant.STATUS_YES });
			stage.ExpressDelivery = Validation.ValidateString(request.ExpressDelivery, "Express Delivery", true, 1, false, Constant.STATUS_NO, new List<string>() { Constant.STATUS_NO, Constant.STATUS_YES });
			stage.DeliveryFee = request.DeliveryFee;
			stage.PromotionPrice = request.PromotionPrice;
			stage.EffectiveDatePromotion = request.EffectiveDatePromotion;
			stage.ExpireDatePromotion = request.ExpireDatePromotion;
			stage.NewArrivalDate = request.NewArrivalDate.HasValue ? request.NewArrivalDate.Value : currentDt;
			stage.DefaultVariant = request.DefaultVariant;
			stage.Display = request.Display;
			stage.MiniQtyAllowed = request.MiniQtyAllowed;
			stage.MaxiQtyAllowed = request.MaxiQtyAllowed;
			stage.UnitPrice = request.UnitPrice;
			stage.PurchasePrice = request.PurchasePrice;
			stage.Status = request.Status;
			stage.IsSell = false;
			stage.IsVariant = false;
			stage.VariantCount = 0;
			stage.Visibility = request.Visibility;
			stage.OldPid = null;
			stage.Bu = null;
			if (isNew)
			{
				stage.CreateBy = email;
				stage.CreateOn = currentDt;
			}
			stage.UpdateBy = email;
			stage.UpdateOn = currentDt;
			if (request.FirstAttribute != null && request.FirstAttribute.AttributeId != 0)
			{
				SetupAttribute(stage, new List<AttributeRequest>() { request.FirstAttribute }, attributeList, email, currentDt);
			}
			if (request.SecondAttribute != null && request.SecondAttribute.AttributeId != 0)
			{
				SetupAttribute(stage, new List<AttributeRequest>() { request.SecondAttribute }, attributeList, email, currentDt);
			}
			int defaultStockType = Constant.STOCK_TYPE[Constant.DEFAULT_STOCK_TYPE];
			if (Constant.STOCK_TYPE.ContainsKey(request.StockType))
			{
				defaultStockType = Constant.STOCK_TYPE[request.StockType];
			}
			if (stage.Inventory == null)
			{
				stage.Inventory = new Inventory()
				{
					MaxQtyAllowInCart = request.MaxQtyAllowInCart,
					Defect = 0,
					MaxQtyPreOrder = request.MaxQtyPreOrder,
					MinQtyAllowInCart = request.MinQtyAllowInCart,
					OnHold = 0 + request.UpdateAmount,
					Quantity = 0 + request.Quantity,
					Reserve = 0 + request.Reserve,
					SafetyStockAdmin = request.SafetyStock,
					StockType = defaultStockType,
					SafetyStockSeller = request.SafetyStock,
					UseDecimal = request.UseDecimal,
				};
			}
			else
			{
				if (!Constant.IGNORE_INVENTORY_SHIPPING.Contains(shippingId))
				{
					inventory.Quantity = inventory.Quantity + request.UpdateAmount;
				}
				inventory.StockType = defaultStockType;
				inventory.SafetyStockSeller = request.SafetyStock;
				inventory.MaxQtyAllowInCart = request.MaxQtyAllowInCart;
				inventory.MaxQtyPreOrder = request.MaxQtyPreOrder;
				inventory.MinQtyAllowInCart = request.MinQtyAllowInCart;
				if (isAdmin)
				{
					inventory.OnHold = request.OnHold;
					inventory.Reserve = request.Reserve;
					inventory.Defect = request.Defect;
				}
				if (request.UpdateAmount != 0)
				{
					db.InventoryHistories.Add(new InventoryHistory()
					{
						CreateBy = email,
						CreateOn = currentDt,
						Defect = inventory.Defect,
						MaxQtyAllowInCart = inventory.MaxQtyAllowInCart,
						MaxQtyPreOrder = inventory.MaxQtyPreOrder,
						MinQtyAllowInCart = inventory.MinQtyAllowInCart,
						OnHold = inventory.OnHold,
						Pid = inventory.Pid,
						Quantity = inventory.Quantity,
						Reserve = inventory.Reserve,
						SafetyStockAdmin = inventory.SafetyStockAdmin,
						SafetyStockSeller = inventory.SafetyStockSeller,
						StockType = inventory.StockType,
						UpdateBy = email,
						UpdateOn = currentDt,
						UseDecimal = inventory.UseDecimal,
						Status = Constant.INVENTORY_STATUS_UPDATE,
					});
				}
			}

		}

		private void SetupAttribute(ProductStage variant, List<AttributeRequest> requestList, List<AttributeRequest> attributeList, string email, DateTime currentDt)
		{
			int position = 1;
			foreach (var attributeRequest in requestList)
			{
				#region validation
				if (attributeRequest == null || attributeRequest.AttributeId == 0)
				{
					continue;
				}
				var attribute = attributeList.Where(w => w.AttributeId == attributeRequest.AttributeId).SingleOrDefault();
				if (attribute == null)
				{
					throw new Exception(string.Concat("No attribute found ", attributeRequest.AttributeId));
				}
				if (variant.IsVariant && !attribute.VariantStatus)
				{
					throw new Exception(string.Concat("Attribute ", attribute.AttributeNameEn, " is not variant type"));
				}
				#endregion
				#region Instantiate
				string valueEn = attributeRequest.ValueEn;
				string valueTh = attributeRequest.ValueTh;
				string hmtlValue = string.Empty;
				bool isAttributeValue = false;
				bool checkboxValue = false;
				int? attributeValueId = null;
				#endregion
				#region List
				if (Constant.DATA_TYPE_LIST.Equals(attribute.DataType))
				{
					if (attributeRequest.AttributeValues == null || attributeRequest.AttributeValues.Count == 0)
					{
						continue;
					}
					if (attribute.AttributeValues.Any(a => attributeRequest.AttributeValues
						 .Select(s => s.AttributeValueId)
						 .Contains(a.AttributeValueId)))
					{
						valueEn = string.Concat(
							Constant.ATTRIBUTE_VALUE_MAP_PREFIX,
							attributeRequest.AttributeValues.FirstOrDefault().AttributeValueId,
							Constant.ATTRIBUTE_VALUE_MAP_SURFIX);
						valueTh = string.Concat(
							Constant.ATTRIBUTE_VALUE_MAP_PREFIX,
							attributeRequest.AttributeValues.FirstOrDefault().AttributeValueId,
							Constant.ATTRIBUTE_VALUE_MAP_SURFIX);
						isAttributeValue = true;
						attributeValueId = attributeRequest.AttributeValues.FirstOrDefault().AttributeValueId;
					}
					else
					{
						throw new Exception(string.Concat("Attribute ", attribute.AttributeNameEn, " id ", attribute.AttributeId, " has no ", "Attribute value ",
							string.Join(",", attributeRequest.AttributeValues.Select(s => s.AttributeValueEn))));
					}
				}
				#endregion
				#region Check box
				else if (Constant.DATA_TYPE_CHECKBOX.Equals(attribute.DataType))
				{
					if (attributeRequest.AttributeValues == null)
					{
						throw new Exception(string.Concat("Attribute ", attribute.AttributeNameEn, " should have variant"));
					}
					if (attribute.AttributeValues.Any(a => attributeRequest.AttributeValues
						 .Select(s => s.AttributeValueId)
						 .Contains(a.AttributeValueId)))
					{
						foreach (var attributeValue in attributeRequest.AttributeValues)
						{
							valueEn = string.Concat(
								Constant.ATTRIBUTE_VALUE_MAP_PREFIX,
								attributeValue.AttributeValueId,
								Constant.ATTRIBUTE_VALUE_MAP_SURFIX);
							valueTh = string.Concat(
								Constant.ATTRIBUTE_VALUE_MAP_PREFIX,
								attributeValue.AttributeValueId,
								Constant.ATTRIBUTE_VALUE_MAP_SURFIX);
							isAttributeValue = true;
							attributeValueId = attributeValue.AttributeValueId;
							checkboxValue = attributeValue.CheckboxValue;

							variant.ProductStageAttributes.Add(new ProductStageAttribute()
							{
								AttributeId = attribute.AttributeId,
								CheckboxValue = checkboxValue,
								HtmlBoxValue = hmtlValue,
								ValueEn = valueEn,
								ValueTh = valueTh,
								Position = position++,
								IsAttributeValue = isAttributeValue,
								AttributeValueId = attributeValueId,
								CreateBy = email,
								CreateOn = currentDt,
								UpdateBy = email,
								UpdateOn = currentDt,
							});
						}
						continue;
					}
				}
				#endregion
				#region HTML Box
				else if (Constant.DATA_TYPE_HTML.Equals(attribute.DataType))
				{
					valueEn = string.Concat(
							Constant.ATTRIBUTE_VALUE_MAP_PREFIX,
							attribute.AttributeId,
							Constant.ATTRIBUTE_VALUE_MAP_SURFIX);
					valueTh = string.Concat(
						Constant.ATTRIBUTE_VALUE_MAP_PREFIX,
						attribute.AttributeId,
						Constant.ATTRIBUTE_VALUE_MAP_SURFIX);
					isAttributeValue = false;
					hmtlValue = attributeRequest.ValueEn;
				}
				#endregion
				#region Free text
				else
				{
					Regex rg = new Regex(@"/(\(\()\d+(\)\))/");
					if (rg.IsMatch(valueEn) || rg.IsMatch(valueTh))
					{
						throw new Exception(string.Concat("Attribute value cannot contain prefix "
							, Constant.ATTRIBUTE_VALUE_MAP_PREFIX
							, " and surfix "
							, Constant.ATTRIBUTE_VALUE_MAP_SURFIX));
					}
				}
				#endregion
				#region Add attribute
				variant.ProductStageAttributes.Add(new ProductStageAttribute()
				{
					AttributeId = attribute.AttributeId,
					CheckboxValue = checkboxValue,
					ValueEn = valueEn,
					ValueTh = valueTh,
					HtmlBoxValue = hmtlValue,
					Position = position++,
					IsAttributeValue = isAttributeValue,
					AttributeValueId = attributeValueId,
					CreateBy = email,
					CreateOn = currentDt,
					UpdateBy = email,
					UpdateOn = currentDt,
				});
				#endregion
			}

		}

		public void SetupApprovedProduct(ProductStageGroup group, ColspEntities db)
		{
			if (group == null)
			{
				throw new Exception("Product group cannot be null");
			}
			group.OnlineFlag = true;
			if (!group.ProductStages.Where(w => w.IsVariant == false).FirstOrDefault().Visibility)
			{
				group.OnlineFlag = false;
			}
			#region History Group
			ProductHistoryGroup historyGroup = new ProductHistoryGroup()
			{

				HistoryDt = SystemHelper.GetCurrentDateTime(),
				ProductId = group.ProductId,
				ShopId = group.ShopId,
				GlobalCatId = group.GlobalCatId,
				LocalCatId = group.LocalCatId,
				AttributeSetId = group.AttributeSetId,
				BrandId = group.BrandId,
				ShippingId = group.ShippingId,
				EffectiveDate = group.EffectiveDate,
				ExpireDate = group.ExpireDate,
				TheOneCardEarn = group.TheOneCardEarn,
				GiftWrap = group.GiftWrap,
				IsNew = group.IsNew,
				IsClearance = group.IsClearance,
				IsBestSeller = group.IsBestSeller,
				IsOnlineExclusive = group.IsOnlineExclusive,
				IsOnlyAt = group.IsOnlyAt,
				Remark = group.Remark,
				InfoFlag = group.InfoFlag,
				ImageFlag = group.ImageFlag,
				OnlineFlag = group.OnlineFlag,
				InformationTabStatus = group.InformationTabStatus,
				ImageTabStatus = group.ImageTabStatus,
				CategoryTabStatus = group.CategoryTabStatus,
				VariantTabStatus = group.VariantTabStatus,
				MoreOptionTabStatus = group.MoreOptionTabStatus,
				RejectReason = group.RejectReason,
				Status = group.Status,
				FirstApproveBy = group.FirstApproveBy,
				FirstApproveOn = group.FirstApproveOn,
				ApproveBy = group.ApproveBy,
				ApproveOn = group.ApproveOn,
				RejecteBy = group.RejecteBy,
				RejectOn = group.RejectOn,
				SubmitBy = group.SubmitBy,
				SubmitOn = group.SubmitOn,
				CreateBy = group.CreateBy,
				CreateOn = group.CreateOn,
				UpdateBy = group.UpdateBy,
				UpdateOn = group.UpdateOn
			};
			#endregion
			#region History Global Category
			foreach (var category in group.ProductStageGlobalCatMaps)
			{
				historyGroup.ProductHistoryGlobalCatMaps.Add(new ProductHistoryGlobalCatMap()
				{
					CategoryId = category.CategoryId,
					CreateBy = category.CreateBy,
					CreateOn = category.CreateOn,
					UpdateBy = category.UpdateBy,
					UpdateOn = category.UpdateOn,
				});
			}
			#endregion
			#region History Local Category
			foreach (var category in group.ProductStageLocalCatMaps)
			{
				historyGroup.ProductHistoryLocalCatMaps.Add(new ProductHistoryLocalCatMap()
				{
					CategoryId = category.CategoryId,
					CreateBy = category.CreateBy,
					CreateOn = category.CreateOn,
					UpdateBy = category.UpdateBy,
					UpdateOn = category.UpdateOn
				});
			}
			#endregion
			#region History Tag
			foreach (var tag in group.ProductStageTags)
			{
				historyGroup.ProductHistoryTags.Add(new ProductHistoryTag()
				{
					Tag = tag.Tag,
					CreateBy = tag.CreateBy,
					CreateOn = tag.CreateOn,
					UpdateBy = tag.UpdateBy,
					UpdateOn = tag.UpdateOn,
				});
			}
			#endregion
			var parent = group.ProductStages.Where(w => w.IsVariant == false).FirstOrDefault();
			if (parent == null)
			{
				throw new Exception("Cannot get parent product");
			}
			var pids = group.ProductStages.Select(s => s.Pid).ToList();
			var productList = db.Products.Where(w => pids.Contains(w.Pid)).ToList();

			db.ProductGlobalCatMaps.RemoveRange(db.ProductGlobalCatMaps.Where(w => pids.Contains(w.Pid)));
			db.ProductLocalCatMaps.RemoveRange(db.ProductLocalCatMaps.Where(w => pids.Contains(w.Pid)));
			db.ProductTags.RemoveRange(db.ProductTags.Where(w => pids.Contains(w.Pid)));
			db.ProductVideos.RemoveRange(db.ProductVideos.Where(w => pids.Contains(w.Pid)));
			db.ProductAttributes.RemoveRange(db.ProductAttributes.Where(w => pids.Contains(w.Pid)));
			db.ProductImages.RemoveRange(db.ProductImages.Where(w => pids.Contains(w.Pid)));
			db.ProductRelateds.RemoveRange(db.ProductRelateds.Where(w => w.Parent == group.ProductId));
			#region Related Product
			foreach (var related in group.ProductStageRelateds1)
			{
				db.ProductRelateds.Add(new ProductRelated()
				{
					Parent = related.Parent,
					Child = related.Child,
					CreateBy = related.CreateBy,
					CreateOn = related.CreateOn,
					ShopId = related.ShopId,
					UpdateBy = related.UpdateBy,
					UpdateOn = related.UpdateOn,
				});
				historyGroup.ProductHistoryRelateds.Add(new ProductHistoryRelated()
				{
					Parent = related.Parent,
					Child = related.Child,
					CreateBy = related.CreateBy,
					CreateOn = related.CreateOn,
					ShopId = related.ShopId,
					UpdateBy = related.UpdateBy,
					UpdateOn = related.UpdateOn,
				});
			}
			#endregion
			foreach (var stage in group.ProductStages)
			{
				bool isNewProduct = false;
				Product product = null;
				if (productList == null || productList.Count == 0)
				{
					isNewProduct = true;
				}
				if (!isNewProduct)
				{
					var currentProduct = productList.Where(w => w.Pid.Equals(stage.Pid)).SingleOrDefault();
					if (currentProduct != null)
					{
						product = currentProduct;
						productList.Remove(currentProduct);
					}
					else
					{
						isNewProduct = true;
					}
				}

				if (isNewProduct)
				{
					product = new Product();
				}
				#region History
				ProductHistory history = new ProductHistory()
				{
					ProductId = group.ProductId,
					Pid = stage.Pid,
					Sku = stage.Sku,
					ProductNameEn = stage.ProductNameEn,
					ProductNameTh = stage.ProductNameTh,
					DescriptionFullEn = stage.DescriptionFullEn,
					DescriptionFullTh = stage.DescriptionFullTh,
					DescriptionShortEn = stage.DescriptionShortEn,
					DescriptionShortTh = stage.DescriptionShortTh,
					Length = stage.Length,
					Height = stage.Height,
					Weight = stage.Weight,
					DimensionUnit = stage.DimensionUnit,
					Width = stage.Width,
					WeightUnit = stage.WeightUnit,
					BoostWeight = stage.BoostWeight,
					DefaultVariant = stage.DefaultVariant,
					Display = stage.Display,
					FeatureImgUrl = stage.FeatureImgUrl,
					GlobalBoostWeight = stage.GlobalBoostWeight,
					ImageCount = stage.ImageCount,
					Installment = stage.Installment,
					IsMaster = stage.IsMaster,
					IsVariant = stage.IsMaster,
					KillerPoint1En = stage.KillerPoint1En,
					KillerPoint1Th = stage.KillerPoint1Th,
					KillerPoint2En = stage.KillerPoint2En,
					KillerPoint2Th = stage.KillerPoint2Th,
					KillerPoint3En = stage.KillerPoint3En,
					KillerPoint3Th = stage.KillerPoint3Th,
					LimitIndividualDay = stage.LimitIndividualDay,
					MaxiQtyAllowed = stage.MaxiQtyAllowed,
					MetaDescriptionEn = stage.MetaDescriptionEn,
					MetaDescriptionTh = stage.MetaDescriptionTh,
					MetaKeyEn = stage.MetaKeyEn,
					MetaKeyTh = stage.MetaKeyTh,
					MetaTitleEn = stage.MetaTitleEn,
					MetaTitleTh = stage.MetaTitleTh,
					SeoEn = stage.SeoEn,
					SeoTh = stage.SeoTh,
					MiniQtyAllowed = stage.MiniQtyAllowed,
					OriginalPrice = stage.OriginalPrice,
					PrepareDay = stage.PrepareDay,
					PrepareFri = stage.PrepareFri,
					PrepareMon = stage.PrepareMon,
					PrepareSat = stage.PrepareSat,
					PrepareSun = stage.PrepareSun,
					PrepareThu = stage.PrepareThu,
					PrepareTue = stage.PrepareTue,
					PrepareWed = stage.PrepareWed,
					PurchasePrice = stage.PurchasePrice,
					SalePrice = stage.SalePrice,
					ShopId = stage.ShopId,
					UnitPrice = stage.UnitPrice,
					Upc = stage.Upc,
					DeliveryFee = stage.DeliveryFee,
					EffectiveDatePromotion = stage.EffectiveDatePromotion,
					ExpireDatePromotion = stage.ExpireDatePromotion,
					ExpressDelivery = stage.ExpressDelivery,
					IsHasExpiryDate = stage.IsHasExpiryDate,
					IsSell = stage.IsSell,
					IsVat = stage.IsVat,
					JDADept = stage.JDADept,
					JDASubDept = stage.JDASubDept,
					MobileDescriptionEn = stage.MobileDescriptionEn,
					MobileDescriptionTh = stage.MobileDescriptionTh,
					NewArrivalDate = stage.NewArrivalDate,
					ProdTDNameEn = stage.ProdTDNameEn,
					ProdTDNameTh = stage.ProdTDNameTh,
					PromotionPrice = stage.PromotionPrice,
					SaleUnitEn = stage.SaleUnitEn,
					SaleUnitTh = stage.SaleUnitTh,
					UrlKey = stage.UrlKey,
					VariantCount = stage.VariantCount,
					Visibility = stage.Visibility,
					Status = stage.Status,
					CreateBy = stage.CreateBy,
					CreateOn = stage.CreateOn,
					UpdateBy = stage.UpdateBy,
					UpdateOn = stage.UpdateOn,
					Bu = stage.Bu,
					OldPid = stage.OldPid,
				};
				#endregion
				#region Setup Product
				product.Pid = stage.Pid;
				if (stage.IsVariant)
				{
					product.ParentPid = parent.Pid;
				}
				else
				{
					product.ParentPid = null;
				}
				product.AttributeSetId = group.AttributeSetId;
				product.BoostWeight = stage.BoostWeight;
				product.BrandId = group.BrandId;
				product.Bu = stage.Bu;
				product.CreateBy = stage.CreateBy;
				product.CreateOn = stage.CreateOn;
				product.DefaultVariant = stage.DefaultVariant;
				product.DeliveryFee = stage.DeliveryFee;
				product.DescriptionFullEn = stage.DescriptionFullEn;
				product.DescriptionFullTh = stage.DescriptionFullTh;
				product.DescriptionShortEn = stage.DescriptionShortEn;
				product.DescriptionShortTh = stage.DescriptionShortTh;
				product.DimensionUnit = stage.DimensionUnit;
				product.Display = stage.Display;
				product.EffectiveDate = group.EffectiveDate;
				product.EffectiveDatePromotion = stage.EffectiveDatePromotion;
				product.EstimateGoodsReceiveDate = stage.EstimateGoodsReceiveDate;
				product.ExpireDate = group.ExpireDate;
				product.ExpireDatePromotion = stage.ExpireDatePromotion;
				product.ExpressDelivery = stage.ExpressDelivery;
				product.FeatureImgUrl = stage.FeatureImgUrl;
				product.GiftWrap = group.GiftWrap;
				product.GlobalBoostWeight = stage.GlobalBoostWeight;
				product.GlobalCatId = group.GlobalCatId;
				product.Height = stage.Height;
				product.ImageCount = stage.ImageCount;
				product.Installment = stage.Installment;
				product.IsBestSeller = group.IsBestSeller;
				product.IsClearance = group.IsClearance;
				product.IsHasExpiryDate = stage.IsHasExpiryDate;
				//product.IsMaster = stage.IsMaster;
				product.IsNew = group.IsNew;
				product.IsOnlineExclusive = group.IsOnlineExclusive;
				product.IsOnlyAt = group.IsOnlyAt;
				product.IsSell = stage.IsSell;
				product.IsVariant = stage.IsVariant;
				product.IsVat = stage.IsVat;
				product.JDADept = stage.JDADept;
				product.JDASubDept = stage.JDASubDept;
				product.KillerPoint1En = stage.KillerPoint1En;
				product.KillerPoint1Th = stage.KillerPoint1Th;
				product.KillerPoint2En = stage.KillerPoint2En;
				product.KillerPoint2Th = stage.KillerPoint2Th;
				product.KillerPoint3En = stage.KillerPoint3En;
				product.KillerPoint3Th = stage.KillerPoint3Th;
				product.Length = stage.Length;
				product.LimitIndividualDay = stage.LimitIndividualDay;
				product.LocalCatId = group.LocalCatId;
				//product.MasterPid = null;
				product.MaxiQtyAllowed = stage.MaxiQtyAllowed;
				product.MetaDescriptionEn = stage.MetaDescriptionEn;
				product.MetaDescriptionTh = stage.MetaDescriptionTh;
				product.MetaKeyEn = stage.MetaKeyEn;
				product.MetaKeyTh = stage.MetaKeyTh;
				product.MetaTitleEn = stage.MetaTitleEn;
				product.MetaTitleTh = stage.MetaTitleTh;
				product.MiniQtyAllowed = stage.MiniQtyAllowed;
				product.MobileDescriptionEn = stage.MobileDescriptionEn;
				product.MobileDescriptionTh = stage.MobileDescriptionTh;
				product.NewArrivalDate = stage.NewArrivalDate;
				product.OldPid = stage.OldPid;
				product.OriginalPrice = stage.OriginalPrice;
				product.Pid = stage.Pid;
				product.PrepareDay = stage.PrepareDay;
				product.PrepareFri = stage.PrepareFri;
				product.PrepareMon = stage.PrepareMon;
				product.PrepareSat = stage.PrepareSat;
				product.PrepareSun = stage.PrepareSun;
				product.PrepareThu = stage.PrepareThu;
				product.PrepareTue = stage.PrepareTue;
				product.PrepareWed = stage.PrepareWed;
				product.ProdTDNameEn = stage.ProdTDNameEn;
				product.ProdTDNameTh = stage.ProdTDNameTh;
				product.ProductId = stage.ProductId;
				product.ProductNameEn = stage.ProductNameEn;
				product.ProductNameTh = stage.ProductNameTh;
				//product.ProductRating = group.ProductRating;
				product.PromotionPrice = stage.PromotionPrice;
				product.PurchasePrice = stage.PurchasePrice;
				product.Remark = group.Remark;
				product.SalePrice = stage.SalePrice;
				product.SaleUnitEn = stage.SaleUnitEn;
				product.SaleUnitTh = stage.SaleUnitTh;
				product.SeoEn = stage.SeoEn;
				product.SeoTh = stage.SeoTh;
				product.ShippingId = group.ShippingId;
				product.ShopId = stage.ShopId;
				product.Sku = stage.Sku;
				product.Status = stage.Status;
				product.TheOneCardEarn = group.TheOneCardEarn;
				product.UnitPrice = stage.UnitPrice;
				product.Upc = stage.Upc;
				product.UpdateBy = stage.UpdateBy;
				product.UpdateOn = stage.UpdateOn;
				product.UrlKey = stage.UrlKey;
				product.VariantCount = stage.VariantCount;
				product.Visibility = stage.Visibility;
				product.Weight = stage.Weight;
				product.WeightUnit = stage.WeightUnit;
				product.Width = stage.Width;
				#endregion
				#region Attribute
				foreach (var attribute in stage.ProductStageAttributes)
				{
					product.ProductAttributes.Add(new ProductAttribute()
					{
						AttributeId = attribute.AttributeId,
						CheckboxValue = attribute.CheckboxValue,
						HtmlBoxValue = attribute.HtmlBoxValue,
						IsAttributeValue = attribute.IsAttributeValue,
						Position = attribute.Position,
						ValueEn = attribute.ValueEn,
						ValueTh = attribute.ValueTh,
						AttributeValueId = attribute.AttributeValueId,
						CreateBy = attribute.CreateBy,
						CreateOn = attribute.CreateOn,
						UpdateBy = attribute.UpdateBy,
						UpdateOn = attribute.UpdateOn,
					});
					history.ProductHistoryAttributes.Add(new ProductHistoryAttribute()
					{
						AttributeId = attribute.AttributeId,
						CheckboxValue = attribute.CheckboxValue,
						HtmlBoxValue = attribute.HtmlBoxValue,
						IsAttributeValue = attribute.IsAttributeValue,
						AttributeValueId = attribute.AttributeValueId,
						Position = attribute.Position,
						ValueEn = attribute.ValueEn,
						ValueTh = attribute.ValueTh,
						CreateBy = attribute.CreateBy,
						CreateOn = attribute.CreateOn,
						UpdateBy = attribute.UpdateBy,
						UpdateOn = attribute.UpdateOn,
					});

				}
				//if (attribteList != null && attribteList.Count > 0)
				//{
				//    db.ProductAttributes.RemoveRange(attribteList);
				//}
				#endregion
				#region Related Global Category
				var globalCatList = product.ProductGlobalCatMaps.ToList();
				foreach (var category in group.ProductStageGlobalCatMaps)
				{
					product.ProductGlobalCatMaps.Add(new ProductGlobalCatMap()
					{
						CategoryId = category.CategoryId,
						CreateBy = category.CreateBy,
						CreateOn = category.CreateOn,
						UpdateBy = category.UpdateBy,
						UpdateOn = category.UpdateOn
					});
				}
				if (globalCatList != null && globalCatList.Count > 0)
				{
					db.ProductGlobalCatMaps.RemoveRange(globalCatList);
				}

				#endregion
				#region Related Local Category
				var localCatList = product.ProductLocalCatMaps.ToList();
				foreach (var category in group.ProductStageLocalCatMaps)
				{

					product.ProductLocalCatMaps.Add(new ProductLocalCatMap()
					{
						CategoryId = category.CategoryId,
						CreateBy = category.CreateBy,
						CreateOn = category.CreateOn,
						UpdateBy = category.UpdateBy,
						UpdateOn = category.UpdateOn
					});
				}
				if (localCatList != null && localCatList.Count > 0)
				{
					db.ProductLocalCatMaps.RemoveRange(localCatList);
				}
				#endregion
				#region Video
				var videoList = product.ProductVideos.ToList();
				foreach (var video in stage.ProductStageVideos)
				{
					product.ProductVideos.Add(new ProductVideo()
					{
						VideoId = video.VideoId,
						VideoUrlEn = video.VideoUrlEn,
						Position = video.Position,
						Status = video.Status,
						CreateBy = video.CreateBy,
						CreateOn = video.CreateOn,
						UpdateBy = video.UpdateBy,
						UpdateOn = video.UpdateOn
					});

					history.ProductHistoryVideos.Add(new ProductHistoryVideo()
					{
						VideoId = video.VideoId,
						VideoUrlEn = video.VideoUrlEn,
						Position = video.Position,
						Status = video.Status,
						CreateBy = video.CreateBy,
						CreateOn = video.CreateOn,
						UpdateBy = video.UpdateBy,
						UpdateOn = video.UpdateOn
					});
				}
				if (videoList != null && videoList.Count > 0)
				{
					db.ProductVideos.RemoveRange(videoList);
				}
				#endregion
				#region Tag
				var tagList = product.ProductTags.ToList();
				foreach (var tag in group.ProductStageTags)
				{
					product.ProductTags.Add(new ProductTag()
					{
						Tag = tag.Tag,
						CreateBy = tag.CreateBy,
						CreateOn = tag.CreateOn,
						UpdateBy = tag.UpdateBy,
						UpdateOn = tag.UpdateOn
					});
				}
				if (tagList != null && tagList.Count > 0)
				{
					db.ProductTags.RemoveRange(tagList);
				}
				#endregion
				#region Image
				foreach (var image in stage.ProductStageImages)
				{
					product.ProductImages.Add(new ProductImage()
					{
						ImageId = image.ImageId,
						FeatureFlag = image.FeatureFlag,
						ImageName = image.ImageName,
						Large = image.Large,
						Normal = image.Normal,
						Thumbnail = image.Thumbnail,
						Zoom = image.Zoom,
						Pid = image.Pid,
						SeqNo = image.SeqNo,
						ShopId = image.ShopId,
						Status = image.Status,
						CreateBy = image.CreateBy,
						CreateOn = image.CreateOn,
						UpdateBy = image.UpdateBy,
						UpdateOn = image.UpdateOn,
					});
					history.ProductHistoryImages.Add(new ProductHistoryImage()
					{
						ImageId = image.ImageId,
						FeatureFlag = image.FeatureFlag,
						ImageName = image.ImageName,
						Large = image.Large,
						Normal = image.Normal,
						Thumbnail = image.Thumbnail,
						Zoom = image.Zoom,
						Pid = image.Pid,
						SeqNo = image.SeqNo,
						ShopId = image.ShopId,
						Status = image.Status,
						CreateBy = image.CreateBy,
						CreateOn = image.CreateOn,
						UpdateBy = image.UpdateBy,
						UpdateOn = image.UpdateOn,
					});
				}
				#endregion
				historyGroup.ProductHistories.Add(history);
				if (isNewProduct)
				{
					db.Products.Add(product);
				}
			}
			if (productList != null && productList.Count > 0)
			{
				productList.ForEach(e => e.Status = Constant.STATUS_REMOVE);
			}
			historyGroup.HistoryId = db.GetNextProductHistoryId().SingleOrDefault().Value;
			db.ProductHistoryGroups.Add(historyGroup);
		}

		public void SetupGroupAfterSave(ProductStageGroup groupProduct, string email, DateTime currentDt, ColspEntities db, bool isNew = false)
		{
			#region Image
			HashSet<string> tmpImageHash = new HashSet<string>();
			foreach (var stage in groupProduct.ProductStages)
			{
				int index = 0;
				foreach (var image in stage.ProductStageImages)
				{
					if (tmpImageHash.Contains(image.ImageName))
					{
						string lastPart = image.ImageName;
						string oldFileTmp = Path.Combine(
							AppSettingKey.IMAGE_ROOT_PATH,
							AppSettingKey.PRODUCT_FOLDER,
							AppSettingKey.IMAGE_TMP_FOLDER,
							lastPart);
						string oldFileOriginal = Path.Combine(
							AppSettingKey.IMAGE_ROOT_PATH,
							AppSettingKey.PRODUCT_FOLDER,
							AppSettingKey.ORIGINAL_FOLDER,
							lastPart);
						if (File.Exists(oldFileTmp))
						{
							string newFileName = string.Concat(stage.Pid, "-", index, Path.GetExtension(lastPart));
							image.ImageName = newFileName;
							File.Copy(oldFileTmp, Path.Combine(
								AppSettingKey.IMAGE_ROOT_PATH,
								AppSettingKey.PRODUCT_FOLDER,
								AppSettingKey.IMAGE_TMP_FOLDER,
								newFileName));
							image.ImageId = 0;
							++index;
						}
						else if (File.Exists(oldFileOriginal))
						{
							string newFileName = string.Concat(stage.Pid, "-", index, Path.GetExtension(lastPart));
							image.ImageName = newFileName;
							File.Copy(oldFileOriginal, Path.Combine(
								AppSettingKey.IMAGE_ROOT_PATH,
								AppSettingKey.PRODUCT_FOLDER,
								AppSettingKey.IMAGE_TMP_FOLDER,
								newFileName));
							image.ImageId = 0;
							++index;
						}
						else
						{

							var myStringWebResource = string.Concat(Constant.PRODUCT_IMAGE_URL, lastPart);
							string newFileName = string.Concat(stage.Pid, "-", index, Path.GetExtension(lastPart));
							string newFile = Path.Combine(
									AppSettingKey.IMAGE_ROOT_PATH,
									AppSettingKey.PRODUCT_FOLDER,
									AppSettingKey.IMAGE_TMP_FOLDER,
									newFileName);
							using (WebClient myWebClient = new WebClient())
							{
								try
								{
									myWebClient.DownloadFile(myStringWebResource, newFile);
								}
								catch (Exception e)
								{
									throw e.GetBaseException();
								}

							}
							image.ImageName = newFileName;
							image.ImageId = 0;
							++index;
						}
					}
					else
					{
						tmpImageHash.Add(image.ImageName);
					}
				}
			}
			foreach (var stage in groupProduct.ProductStages)
			{
				SetupStageAfterSave(stage, email, currentDt, db, isNew);
			}
			#endregion
			#region Video
			foreach (var video in groupProduct.ProductStages.SelectMany(s => s.ProductStageVideos))
			{
				if (video.VideoId == 0)
				{
					video.VideoId = db.GetNextProductStageVideoId().SingleOrDefault().Value;
				}
			}
			#endregion
		}

		public void SetupStageAfterSave(ProductStage stage, string email, DateTime currentDt, ColspEntities db = null, bool isNew = false)
		{
			#region Image
			using (SftpClient sft = new SftpClient(AppSettingKey.SFTP_HOST, AppSettingKey.SFTP_USERNAME, AppSettingKey.SFTP_PASSWORD))
			{
				sft.Connect();
				string productRootFolder = Path.Combine(
						AppSettingKey.IMAGE_ROOT_PATH,
						AppSettingKey.PRODUCT_FOLDER);
				foreach (var image in stage.ProductStageImages)
				{
					string lastPart = image.ImageName;
					string newFileName = string.Concat(stage.Pid,"_", image.SeqNo, Path.GetExtension(lastPart));
					string newFile = Path.Combine(
						productRootFolder,
						AppSettingKey.ORIGINAL_FOLDER,
						newFileName);
					string oldFile = Path.Combine(
						productRootFolder,
						AppSettingKey.IMAGE_TMP_FOLDER,
						lastPart);
					if (File.Exists(oldFile))
					{
						if (File.Exists(newFile))
						{
							File.Delete(newFile);
							#region delete old file
							string tmpFile = Path.Combine(productRootFolder, AppSettingKey.ZOOM_FOLDER, newFileName);
							if (File.Exists(tmpFile))
							{
								File.Delete(tmpFile);
							}
							tmpFile = Path.Combine(productRootFolder, AppSettingKey.LARGE_FOLDER, newFileName);
							if (File.Exists(tmpFile))
							{
								File.Delete(tmpFile);
							}
							tmpFile = Path.Combine(productRootFolder, AppSettingKey.NORMAL_FOLDER, newFileName);
							if (File.Exists(tmpFile))
							{
								File.Delete(tmpFile);
							}
							tmpFile = Path.Combine(productRootFolder, AppSettingKey.THUMBNAIL_FOLDER, newFileName);
							if (File.Exists(tmpFile))
							{
								File.Delete(tmpFile);
							}
							#endregion
						}
						File.Move(oldFile, newFile);
						image.ImageName = Path.GetFileName(newFile);
						image.ImageId = db.GetNextProductStageImageId().SingleOrDefault().Value;
						if (image.FeatureFlag)
						{
							stage.FeatureImgUrl = image.ImageName;
						}
						#region resize file
						using (var originalImage = Image.FromFile(newFile))
						{
							using (var resizeImage = ScaleImage(originalImage, 1500, 1500))
							{
								using (var stream = new System.IO.MemoryStream())
								{
									resizeImage.Save(stream, ImageFormat.Jpeg);
									stream.Position = 0;
									sft.UploadFile(stream, string.Concat("/var/www/html/productimages/", AppSettingKey.ZOOM_FOLDER, "/", newFileName), true);
								}
							}
							using (var resizeImage = ScaleImage(originalImage, 600, 600))
							{
								using (var stream = new System.IO.MemoryStream())
								{
									resizeImage.Save(stream, ImageFormat.Jpeg);
									stream.Position = 0;
									sft.UploadFile(stream, string.Concat("/var/www/html/productimages/", AppSettingKey.LARGE_FOLDER, "/", newFileName), true);
								}
							}
							using (var resizeImage = ScaleImage(originalImage, 300, 300))
							{
								using (var stream = new System.IO.MemoryStream())
								{
									resizeImage.Save(stream, ImageFormat.Jpeg);
									stream.Position = 0;
									sft.UploadFile(stream, string.Concat("/var/www/html/productimages/", AppSettingKey.NORMAL_FOLDER, "/", newFileName), true);
								}
							}
							using (var resizeImage = ScaleImage(originalImage, 100, 100))
							{
								using (var stream = new System.IO.MemoryStream())
								{
									resizeImage.Save(stream, ImageFormat.Jpeg);
									stream.Position = 0;
									sft.UploadFile(stream, string.Concat("/var/www/html/productimages/", AppSettingKey.THUMBNAIL_FOLDER, "/", newFileName), true);
								}
							}
						}
						File.Delete(newFile);
						#endregion
					}
				}
			}
			#endregion
			#region Inventory History
			if (isNew)
			{
				InventoryHistory history = new InventoryHistory()
				{
					Pid = stage.Pid,
					MaxQtyAllowInCart = stage.Inventory.MaxQtyAllowInCart,
					MaxQtyPreOrder = stage.Inventory.MaxQtyPreOrder,
					MinQtyAllowInCart = stage.Inventory.MinQtyAllowInCart,
					StockType = stage.Inventory.StockType,
					UseDecimal = stage.Inventory.UseDecimal,
					Defect = stage.Inventory.Defect,
					OnHold = stage.Inventory.OnHold,
					Quantity = stage.Inventory.Quantity,
					Reserve = stage.Inventory.Reserve,
					SafetyStockAdmin = stage.Inventory.SafetyStockAdmin,
					SafetyStockSeller = stage.Inventory.SafetyStockSeller,
					Status = Constant.INVENTORY_STATUS_ADD,
					CreateBy = email,
					CreateOn = currentDt,
					UpdateBy = email,
					UpdateOn = currentDt,
				};
				db.InventoryHistories.Add(history);
			}
			#endregion
		}

		private static Image ScaleImage(Image image, int maxWidth, int maxHeight)
		{
			var ratioX = (double)maxWidth / image.Width;
			var ratioY = (double)maxHeight / image.Height;
			var ratio = Math.Min(ratioX, ratioY);

			var newWidth = (int)(image.Width * ratio);
			var newHeight = (int)(image.Height * ratio);

			var newImage = new Bitmap(newWidth, newHeight);

			using (var graphics = Graphics.FromImage(newImage))
			{
				graphics.DrawImage(image, 0, 0, newWidth, newHeight);
			}

			return newImage;
		}

		private void UpdateByPassField(ProductStageGroup group, ColspEntities db)
		{
			foreach (var stage in group.ProductStages)
			{
				Product product = new Product()
				{
					Pid = stage.Pid
				};
				db.Products.Attach(product);
				db.Entry(product).Property(p => p.SalePrice).IsModified = true;
				db.Entry(product).Property(p => p.PromotionPrice).IsModified = true;
				db.Entry(product).Property(p => p.EffectiveDatePromotion).IsModified = true;
				db.Entry(product).Property(p => p.ExpireDatePromotion).IsModified = true;
				db.Entry(product).Property(p => p.UnitPrice).IsModified = true;
				db.Entry(product).Property(p => p.PurchasePrice).IsModified = true;
				db.Entry(product).Property(p => p.SaleUnitEn).IsModified = true;
				db.Entry(product).Property(p => p.SaleUnitTh).IsModified = true;
				product.SalePrice = stage.SalePrice;
				product.PromotionPrice = stage.PromotionPrice;
				product.EffectiveDatePromotion = stage.EffectiveDatePromotion;
				product.ExpireDatePromotion = stage.ExpireDatePromotion;
				product.UnitPrice = stage.UnitPrice;
				product.PurchasePrice = stage.PurchasePrice;
				product.SaleUnitEn = stage.SaleUnitEn;
				product.SaleUnitTh = stage.SaleUnitTh;
			}
		}

		public ProductStageRequest GetProductStageRequestFromId(ColspEntities db, long productId)
		{
			#region Query
			var tmpPro = db.ProductStageGroups.Where(w => !Constant.STATUS_REMOVE.Equals(w.Status) && w.ProductId == productId)
				.Select(s => new
				{
					s.ProductId,
					Shop = new
					{
						s.ShopId,
						s.Shop.ShopNameEn
					},
					MainGlobalCategory = new
					{
						s.GlobalCatId,
						s.GlobalCategory.NameEn,
					},
					MainLocaCategory = s.LocalCatId == null ? null : new
					{
						s.LocalCatId,
						s.LocalCategory.NameEn,
					},
					s.AttributeSetId,
					s.BrandId,
					s.ShippingId,
					s.EffectiveDate,
					s.ExpireDate,
					s.NewArrivalDate,
					s.TheOneCardEarn,
					s.GiftWrap,
					s.IsNew,
					s.IsClearance,
					s.IsBestSeller,
					s.IsOnlineExclusive,
					s.IsOnlyAt,
					s.Remark,
					s.InfoFlag,
					s.ImageFlag,
					s.OnlineFlag,
					s.InformationTabStatus,
					s.ImageTabStatus,
					s.CategoryTabStatus,
					s.VariantTabStatus,
					s.MoreOptionTabStatus,
					s.RejectReason,
					s.Status,
					s.FirstApproveBy,
					s.FirstApproveOn,
					s.ApproveBy,
					s.ApproveOn,
					s.RejecteBy,
					s.RejectOn,
					s.SubmitBy,
					s.SubmitOn,
					s.CreateBy,
					s.CreateOn,
					s.UpdateBy,
					s.UpdateOn,
					ProductRelated = s.ProductStageRelateds1.Select(r => new
					{
						Child = r.ProductStageGroup.ProductStages.Where(w => w.IsVariant == false && !Constant.STATUS_REMOVE.Equals(w.Status)).Select(sm => new
						{
							sm.ProductId,
							sm.Pid,
							sm.ProductNameEn
						})
					}),
					AttributeSet = s.AttributeSet == null ? null : new
					{
						s.AttributeSet.AttributeSetId,
						s.AttributeSet.AttributeSetNameEn,
						AttributeSetMap = s.AttributeSet.AttributeSetMaps.Select(sm => new
						{
							sm.Attribute.DataType,
							sm.AttributeId,
							AttributeValueMap = sm.Attribute.AttributeValueMaps.Select(sv => new
							{
								sv.AttributeValue.AttributeValueId,
								sv.AttributeValue.AttributeValueEn
							})
						})
					},
					Brand = s.Brand == null ? null : new
					{
						s.Brand.BrandId,
						s.Brand.BrandNameEn,
						s.Brand.DisplayNameEn,
					},
					GlobalCategories = s.ProductStageGlobalCatMaps.Select(sg => new
					{
						sg.CategoryId,
						sg.GlobalCategory.NameEn
					}),
					LocalCategories = s.ProductStageLocalCatMaps.Select(sl => new
					{
						sl.CategoryId,
						sl.LocalCategory.NameEn
					}),
					Tags = s.ProductStageTags.Select(st => st.Tag),
					ProductStages = s.ProductStages.Select(st => new
					{
						st.Pid,
						st.ProductId,
						st.ShopId,
						st.ProductNameEn,
						st.ProductNameTh,
						st.ProdTDNameTh,
						st.ProdTDNameEn,
						st.JDADept,
						st.JDASubDept,
						st.SaleUnitTh,
						st.SaleUnitEn,
						st.Sku,
						st.Upc,
						st.OriginalPrice,
						st.SalePrice,
						st.DescriptionFullEn,
						st.DescriptionShortEn,
						st.DescriptionFullTh,
						st.DescriptionShortTh,
						st.MobileDescriptionEn,
						st.MobileDescriptionTh,
						st.ImageCount,
						st.FeatureImgUrl,
						st.PrepareDay,
						st.LimitIndividualDay,
						st.PrepareMon,
						st.PrepareTue,
						st.PrepareWed,
						st.PrepareThu,
						st.PrepareFri,
						st.PrepareSat,
						st.PrepareSun,
						st.KillerPoint1En,
						st.KillerPoint2En,
						st.KillerPoint3En,
						st.KillerPoint1Th,
						st.KillerPoint2Th,
						st.KillerPoint3Th,
						st.Installment,
						st.Length,
						st.Height,
						st.Width,
						st.DimensionUnit,
						st.Weight,
						st.WeightUnit,
						st.MetaTitleEn,
						st.MetaTitleTh,
						st.MetaDescriptionEn,
						st.MetaDescriptionTh,
						st.MetaKeyEn,
						st.MetaKeyTh,
						st.SeoEn,
						st.SeoTh,
						st.IsHasExpiryDate,
						st.IsVat,
						st.UrlKey,
						st.BoostWeight,
						st.GlobalBoostWeight,
						st.ExpressDelivery,
						st.DeliveryFee,
						st.PromotionPrice,
						st.EffectiveDatePromotion,
						st.ExpireDatePromotion,
						st.NewArrivalDate,
						st.DefaultVariant,
						st.Display,
						st.MiniQtyAllowed,
						st.MaxiQtyAllowed,
						st.UnitPrice,
						st.PurchasePrice,
						st.IsSell,
						st.IsVariant,
						st.IsMaster,
						st.VariantCount,
						st.Visibility,
						ProductStageAttributes = st.ProductStageAttributes.Select(sa => new
						{
							sa.Pid,
							sa.AttributeId,
							sa.ValueEn,
							sa.ValueTh,
							sa.AttributeValueId,
							sa.CheckboxValue,
							sa.HtmlBoxValue,
							sa.Position,
							sa.IsAttributeValue,
							Attribute = new
							{
								sa.Attribute.AttributeId,
								sa.Attribute.AttributeNameEn,
								sa.Attribute.DataType,
								sa.Attribute.VariantStatus,
							},
							AttributeValue = sa.AttributeValue == null ? null : new
							{
								sa.AttributeValue.AttributeValueId,
								sa.AttributeValue.AttributeValueEn,
								sa.AttributeValue.AttributeValueTh,
							}
						}),
						Images = st.ProductStageImages.OrderBy(o => o.SeqNo).Select(si => new
						{
							si.ImageId,
							si.ImageName
						}),
						Videos = st.ProductStageVideos.OrderBy(o => o.Position).Select(sv => new
						{
							sv.VideoUrlEn,
							sv.VideoId
						}),
						Inventory = st.Inventory == null ? null : new
						{
							st.Inventory.OnHold,
							st.Inventory.Reserve,
							st.Inventory.Quantity,
							st.Inventory.SafetyStockSeller,
							st.Inventory.StockType,
							st.Inventory.MaxQtyAllowInCart,
							st.Inventory.MaxQtyPreOrder,
							st.Inventory.MinQtyAllowInCart,
							st.Inventory.Defect
						}
					}),

				}).SingleOrDefault();
			#endregion
			#region group
			ProductStageRequest response = new ProductStageRequest()
			{
				AdminApprove = new AdminApproveRequest()
				{
					Category = tmpPro.CategoryTabStatus,
					Image = tmpPro.ImageTabStatus,
					Information = tmpPro.InformationTabStatus,
					MoreOption = tmpPro.MoreOptionTabStatus,
					RejectReason = tmpPro.RejectReason,
					Variation = tmpPro.VariantTabStatus
				},
				AttributeSet = tmpPro.AttributeSet == null ? new AttributeSetRequest() : new AttributeSetRequest()
				{
					AttributeSetId = tmpPro.AttributeSet.AttributeSetId,
					AttributeSetNameEn = tmpPro.AttributeSet.AttributeSetNameEn,
					Attributes = tmpPro.AttributeSet.AttributeSetMap.Select(s => new AttributeRequest()
					{
						AttributeId = s.AttributeId,
						DataType = s.DataType,
						AttributeValues = s.AttributeValueMap.Select(sv => new AttributeValueRequest()
						{
							AttributeValueEn = sv.AttributeValueEn,
							AttributeValueId = sv.AttributeValueId,
						}).ToList(),
					}).ToList(),
				},
				Brand = tmpPro.Brand == null ? new BrandRequest() : new BrandRequest()
				{
					BrandId = tmpPro.Brand.BrandId,
					BrandNameEn = tmpPro.Brand.BrandNameEn,
					DisplayNameEn = tmpPro.Brand.DisplayNameEn,
				},
				ControlFlags = new ControlFlagRequest()
				{
					IsBestSeller = tmpPro.IsBestSeller,
					IsClearance = tmpPro.IsClearance,
					IsNew = tmpPro.IsNew,
					IsOnlineExclusive = tmpPro.IsOnlineExclusive,
					IsOnlyAt = tmpPro.IsOnlyAt
				},
				EffectiveDate = tmpPro.EffectiveDate,
				ExpireDate = tmpPro.ExpireDate,
				NewArrivalDate = tmpPro.NewArrivalDate,
				GiftWrap = tmpPro.GiftWrap,
				GlobalCategories = tmpPro.GlobalCategories.Select(s => new CategoryRequest()
				{
					CategoryId = s.CategoryId,
					NameEn = s.NameEn,
				}).ToList(),
				LocalCategories = tmpPro.LocalCategories.Select(s => new CategoryRequest()
				{
					CategoryId = s.CategoryId,
					NameEn = s.NameEn
				}).ToList(),
				ImageFlag = tmpPro.ImageFlag,
				InfoFlag = tmpPro.InfoFlag,
				ProductId = tmpPro.ProductId,
				MainGlobalCategory = new CategoryRequest()
				{
					CategoryId = tmpPro.MainGlobalCategory.GlobalCatId,
					NameEn = tmpPro.MainGlobalCategory.NameEn
				},
				MainLocalCategory = new CategoryRequest()
				{
					CategoryId = tmpPro.MainLocaCategory != null ? tmpPro.MainLocaCategory.LocalCatId.Value : 0,
					NameEn = tmpPro.MainLocaCategory != null ? tmpPro.MainLocaCategory.NameEn : string.Empty,
				},
				OnlineFlag = tmpPro.OnlineFlag,
				Remark = tmpPro.Remark,
				ShippingMethod = tmpPro.ShippingId,
				ShopId = tmpPro.Shop.ShopId,
				ShopName = tmpPro.Shop.ShopNameEn,
				Status = tmpPro.Status,
				Tags = tmpPro.Tags.ToList()
			};

			#endregion
			#region related product
			foreach (var related in tmpPro.ProductRelated)
			{
				foreach (var child in related.Child)
				{
					response.RelatedProducts.Add(new VariantRequest()
					{
						ProductNameEn = child.ProductNameEn,
						ProductId = child.ProductId,
						Pid = child.Pid
					});
				}
			}
			#endregion
			#region master
			var masterVariant = tmpPro.ProductStages.Where(w => w.IsVariant == false).SingleOrDefault();
			response.MasterVariant = new VariantRequest()
			{
				Pid = masterVariant.Pid,
				ProductId = masterVariant.ProductId,
				ShopId = masterVariant.ShopId,
				ProductNameEn = masterVariant.ProductNameEn,
				ProductNameTh = masterVariant.ProductNameTh,
				ProdTDNameTh = masterVariant.ProdTDNameTh,
				ProdTDNameEn = masterVariant.ProdTDNameEn,
				SaleUnitTh = masterVariant.SaleUnitTh,
				SaleUnitEn = masterVariant.SaleUnitEn,
				Sku = masterVariant.Sku,
				Upc = masterVariant.Upc,
				OriginalPrice = masterVariant.OriginalPrice,
				SalePrice = masterVariant.SalePrice,
				DescriptionFullEn = masterVariant.DescriptionFullEn,
				DescriptionShortEn = masterVariant.DescriptionShortEn,
				DescriptionFullTh = masterVariant.DescriptionFullTh,
				DescriptionShortTh = masterVariant.DescriptionShortTh,
				MobileDescriptionEn = masterVariant.MobileDescriptionEn,
				MobileDescriptionTh = masterVariant.MobileDescriptionTh,
				PrepareDay = masterVariant.PrepareDay,
				LimitIndividualDay = masterVariant.LimitIndividualDay,
				PrepareMon = masterVariant.PrepareMon,
				PrepareTue = masterVariant.PrepareTue,
				PrepareWed = masterVariant.PrepareWed,
				PrepareThu = masterVariant.PrepareThu,
				PrepareFri = masterVariant.PrepareFri,
				PrepareSat = masterVariant.PrepareSat,
				PrepareSun = masterVariant.PrepareSun,
				KillerPoint1En = masterVariant.KillerPoint1En,
				KillerPoint2En = masterVariant.KillerPoint2En,
				KillerPoint3En = masterVariant.KillerPoint3En,
				KillerPoint1Th = masterVariant.KillerPoint1Th,
				KillerPoint2Th = masterVariant.KillerPoint2Th,
				KillerPoint3Th = masterVariant.KillerPoint3Th,
				Installment = masterVariant.Installment,
				Length = Constant.DIMENSTION_CM.Equals(masterVariant.DimensionUnit) ? (masterVariant.Length / 10) : Constant.DIMENSTION_M.Equals(masterVariant.DimensionUnit) ? (masterVariant.Length / 1000) : masterVariant.Length,
				Height = Constant.DIMENSTION_CM.Equals(masterVariant.DimensionUnit) ? (masterVariant.Height / 10) : Constant.DIMENSTION_M.Equals(masterVariant.DimensionUnit) ? (masterVariant.Height / 1000) : masterVariant.Height,
				Width = Constant.DIMENSTION_CM.Equals(masterVariant.DimensionUnit) ? (masterVariant.Width / 10) : Constant.DIMENSTION_M.Equals(masterVariant.DimensionUnit) ? (masterVariant.Width / 1000) : masterVariant.Width,
				DimensionUnit = masterVariant.DimensionUnit,
				Weight = Constant.WEIGHT_MEASURE_KG.Equals(masterVariant.WeightUnit) ? (masterVariant.Weight / 1000) : masterVariant.Weight,
				WeightUnit = masterVariant.WeightUnit,
				SEO = new SEORequest()
				{
					GlobalProductBoostingWeight = masterVariant.GlobalBoostWeight,
					MetaDescriptionEn = masterVariant.MetaDescriptionEn,
					MetaDescriptionTh = masterVariant.MetaDescriptionTh,
					MetaKeywordEn = masterVariant.MetaKeyEn,
					MetaKeywordTh = masterVariant.MetaKeyTh,
					MetaTitleEn = masterVariant.MetaTitleEn,
					MetaTitleTh = masterVariant.MetaTitleTh,
					ProductBoostingWeight = masterVariant.BoostWeight,
					ProductUrlKeyEn = masterVariant.UrlKey,
					SeoEn = masterVariant.SeoEn,
					SeoTh = masterVariant.SeoTh,
				},
				IsHasExpiryDate = masterVariant.IsHasExpiryDate,
				IsVat = masterVariant.IsVat,
				ExpressDelivery = masterVariant.ExpressDelivery,
				DeliveryFee = masterVariant.DeliveryFee,
				PromotionPrice = masterVariant.PromotionPrice,
				EffectiveDatePromotion = masterVariant.EffectiveDatePromotion,
				ExpireDatePromotion = masterVariant.ExpireDatePromotion,
				NewArrivalDate = masterVariant.NewArrivalDate,
				DefaultVariant = masterVariant.DefaultVariant,
				Display = masterVariant.Display,
				MiniQtyAllowed = masterVariant.MiniQtyAllowed,
				MaxiQtyAllowed = masterVariant.MaxiQtyAllowed,
				UnitPrice = masterVariant.UnitPrice,
				PurchasePrice = masterVariant.PurchasePrice,
				IsVariant = masterVariant.IsVariant,
				Visibility = masterVariant.Visibility,
				OnHold = masterVariant.Inventory == null ? 0 : masterVariant.Inventory.OnHold,
				Quantity = masterVariant.Inventory == null ? 0 : masterVariant.Inventory.Quantity,
				Defect = masterVariant.Inventory == null ? 0 : masterVariant.Inventory.Defect,
				Reserve = masterVariant.Inventory == null ? 0 : masterVariant.Inventory.Reserve,
				SafetyStock = masterVariant.Inventory == null ? 0 : masterVariant.Inventory.SafetyStockSeller,
				StockType = masterVariant.Inventory == null ? Constant.DEFAULT_STOCK_TYPE : masterVariant.Inventory.StockType == 1 ? "Stock" : "Pre-Order",
				MaxQtyAllowInCart = masterVariant.Inventory == null ? 0 : masterVariant.Inventory.MaxQtyAllowInCart,
				MinQtyAllowInCart = masterVariant.Inventory == null ? 0 : masterVariant.Inventory.MinQtyAllowInCart,
				MaxQtyPreOrder = masterVariant.Inventory == null ? 0 : masterVariant.Inventory.MaxQtyPreOrder,
			};
			if (masterVariant.ProductStageAttributes != null)
			{
				foreach (var attribute in masterVariant.ProductStageAttributes)
				{

					var attr = new AttributeRequest()
					{
						AttributeId = attribute.Attribute.AttributeId,
						DataType = attribute.Attribute.DataType,
						AttributeNameEn = attribute.Attribute.AttributeNameEn,
						ValueEn = Constant.DATA_TYPE_HTML.Equals(attribute.Attribute.DataType) ? attribute.HtmlBoxValue : attribute.ValueEn,
						ValueTh = attribute.ValueTh,
					};
					if (attribute.AttributeValue != null)
					{
						attr.ValueEn = string.Empty;
						attr.ValueTh = string.Empty;
						attr.AttributeValues.Add(new AttributeValueRequest()
						{
							AttributeValueId = attribute.AttributeValueId.HasValue ? attribute.AttributeValueId.Value : 0,
							AttributeValueEn = attribute.AttributeValue.AttributeValueEn,
							CheckboxValue = attribute.CheckboxValue,
							AttributeValueTh = attribute.AttributeValue.AttributeValueTh,
							Position = attribute.Position,
						});
					}
					response.MasterAttribute.Add(attr);
				}
			}
			if (masterVariant.Images != null)
			{
				foreach (var image in masterVariant.Images)
				{
					response.MasterVariant.Images.Add(new ImageRequest()
					{
						Url = string.Concat(Constant.PRODUCT_IMAGE_URL, image.ImageName),
						ImageId = image.ImageId,
					});
				}
			}
			if (masterVariant.Videos != null)
			{
				foreach (var video in masterVariant.Videos)
				{
					response.MasterVariant.VideoLinks.Add(new VideoLinkRequest()
					{
						VideoId = video.VideoId,
						Url = video.VideoUrlEn,
					});
				}
			}
			response.Visibility = masterVariant.Visibility;
			#endregion
			#region variant
			var variants = tmpPro.ProductStages.Where(w => w.IsVariant == true && w.IsMaster == false);
			foreach (var variant in variants)
			{
				var tmpVariant = new VariantRequest()
				{
					Pid = variant.Pid,
					ProductId = variant.ProductId,
					ShopId = variant.ShopId,
					ProductNameEn = variant.ProductNameEn,
					ProductNameTh = variant.ProductNameTh,
					ProdTDNameTh = variant.ProdTDNameTh,
					ProdTDNameEn = variant.ProdTDNameEn,
					SaleUnitTh = variant.SaleUnitTh,
					SaleUnitEn = variant.SaleUnitEn,
					Sku = variant.Sku,
					Upc = variant.Upc,
					OriginalPrice = variant.OriginalPrice,
					SalePrice = variant.SalePrice,
					DescriptionFullEn = variant.DescriptionFullEn,
					DescriptionShortEn = variant.DescriptionShortEn,
					DescriptionFullTh = variant.DescriptionFullTh,
					DescriptionShortTh = variant.DescriptionShortTh,
					MobileDescriptionEn = variant.MobileDescriptionEn,
					MobileDescriptionTh = variant.MobileDescriptionTh,
					PrepareDay = variant.PrepareDay,
					LimitIndividualDay = variant.LimitIndividualDay,
					PrepareMon = variant.PrepareMon,
					PrepareTue = variant.PrepareTue,
					PrepareWed = variant.PrepareWed,
					PrepareThu = variant.PrepareThu,
					PrepareFri = variant.PrepareFri,
					PrepareSat = variant.PrepareSat,
					PrepareSun = variant.PrepareSun,
					KillerPoint1En = variant.KillerPoint1En,
					KillerPoint2En = variant.KillerPoint2En,
					KillerPoint3En = variant.KillerPoint3En,
					KillerPoint1Th = variant.KillerPoint1Th,
					KillerPoint2Th = variant.KillerPoint2Th,
					KillerPoint3Th = variant.KillerPoint3Th,
					Installment = variant.Installment,
					Length = Constant.DIMENSTION_CM.Equals(variant.DimensionUnit) ? (variant.Length / 10) : Constant.DIMENSTION_M.Equals(variant.DimensionUnit) ? (variant.Length / 1000) : variant.Length,
					Height = Constant.DIMENSTION_CM.Equals(variant.DimensionUnit) ? (variant.Height / 10) : Constant.DIMENSTION_M.Equals(variant.DimensionUnit) ? (variant.Height / 1000) : variant.Height,
					Width = Constant.DIMENSTION_CM.Equals(variant.DimensionUnit) ? (variant.Width / 10) : Constant.DIMENSTION_M.Equals(variant.DimensionUnit) ? (variant.Width / 1000) : variant.Width,
					DimensionUnit = variant.DimensionUnit,
					Weight = Constant.WEIGHT_MEASURE_KG.Equals(variant.WeightUnit) ? (variant.Weight / 1000) : variant.Weight,
					WeightUnit = variant.WeightUnit,
					SEO = new SEORequest()
					{
						GlobalProductBoostingWeight = variant.GlobalBoostWeight,
						MetaDescriptionEn = variant.MetaDescriptionEn,
						MetaDescriptionTh = variant.MetaDescriptionTh,
						MetaKeywordEn = variant.MetaKeyEn,
						MetaKeywordTh = variant.MetaKeyTh,
						MetaTitleEn = variant.MetaTitleEn,
						MetaTitleTh = variant.MetaTitleTh,
						ProductBoostingWeight = variant.BoostWeight,
						ProductUrlKeyEn = variant.UrlKey,
						SeoEn = variant.SeoEn,
						SeoTh = variant.SeoTh,
					},
					IsHasExpiryDate = variant.IsHasExpiryDate,
					IsVat = variant.IsVat,
					ExpressDelivery = variant.ExpressDelivery,
					DeliveryFee = variant.DeliveryFee,
					PromotionPrice = variant.PromotionPrice,
					EffectiveDatePromotion = variant.EffectiveDatePromotion,
					ExpireDatePromotion = variant.ExpireDatePromotion,
					NewArrivalDate = variant.NewArrivalDate,
					DefaultVariant = variant.DefaultVariant,
					Display = variant.Display,
					MiniQtyAllowed = variant.MiniQtyAllowed,
					MaxiQtyAllowed = variant.MaxiQtyAllowed,
					UnitPrice = variant.UnitPrice,
					PurchasePrice = variant.PurchasePrice,
					IsVariant = variant.IsVariant,
					Visibility = variant.Visibility,
					OnHold = variant.Inventory == null ? 0 : variant.Inventory.OnHold,
					Quantity = variant.Inventory == null ? 0 : variant.Inventory.Quantity,
					Reserve = variant.Inventory == null ? 0 : variant.Inventory.Reserve,
					Defect = variant.Inventory == null ? 0 : variant.Inventory.Defect,
					SafetyStock = variant.Inventory == null ? 0 : variant.Inventory.SafetyStockSeller,
					StockType = variant.Inventory == null ? Constant.DEFAULT_STOCK_TYPE : variant.Inventory.StockType == 1 ? "Stock" : "Pre-Order",
					MaxQtyAllowInCart = variant.Inventory == null ? 0 : variant.Inventory.MaxQtyAllowInCart,
					MinQtyAllowInCart = variant.Inventory == null ? 0 : variant.Inventory.MinQtyAllowInCart,
					MaxQtyPreOrder = variant.Inventory == null ? 0 : variant.Inventory.MaxQtyPreOrder,
				};

				if (variant.ProductStageAttributes != null)
				{
					foreach (var attribute in variant.ProductStageAttributes)
					{

						var attr = new AttributeRequest()
						{
							AttributeId = attribute.Attribute.AttributeId,
							DataType = attribute.Attribute.DataType,
							AttributeNameEn = attribute.Attribute.AttributeNameEn,
							ValueEn = Constant.DATA_TYPE_HTML.Equals(attribute.Attribute.DataType) ? attribute.HtmlBoxValue : attribute.ValueEn,
							ValueTh = attribute.ValueTh,
						};
						if (attribute.AttributeValue != null)
						{
							attr.ValueEn = string.Empty;
							attr.ValueTh = string.Empty;

							attr.AttributeValues.Add(new AttributeValueRequest()
							{
								AttributeValueId = attribute.AttributeValueId.HasValue ? attribute.AttributeValueId.Value : 0,
								AttributeValueEn = attribute.AttributeValue.AttributeValueEn,
								CheckboxValue = attribute.CheckboxValue,
								AttributeValueTh = attribute.AttributeValue.AttributeValueTh,
								Position = attribute.Position,
							});
						}
						if (tmpVariant.FirstAttribute == null || tmpVariant.FirstAttribute.AttributeId == 0)
						{
							tmpVariant.FirstAttribute = attr;
						}
						else
						{
							tmpVariant.SecondAttribute = attr;
						}
					}
				}
				if (variant.Images != null)
				{
					foreach (var image in variant.Images)
					{
						tmpVariant.Images.Add(new ImageRequest()
						{
							Url = string.Concat(Constant.PRODUCT_IMAGE_URL, image.ImageName),
							ImageId = image.ImageId,
						});
					}
				}
				if (variant.Videos != null)
				{
					foreach (var video in variant.Videos)
					{
						tmpVariant.VideoLinks.Add(new VideoLinkRequest()
						{
							VideoId = video.VideoId,
							Url = video.VideoUrlEn,
						});
					}
				}
				response.Variants.Add(tmpVariant);
			}
			#endregion
			#region history
			var historyList = db.ProductHistoryGroups
				.Where(w => w.ProductId == response.ProductId)
				.OrderByDescending(o => o.HistoryDt)
				.Select(s => new ProductHistoryRequest()
				{
					ApproveOn = s.ApproveOn,
					SubmitBy = s.SubmitBy,
					HistoryId = s.HistoryId,
					SubmitOn = s.SubmitOn
				}).Take(Constant.HISTORY_REVISION);
			foreach (var history in historyList)
			{
				response.Revisions.Add(new ProductHistoryRequest()
				{
					ApproveOn = history.ApproveOn,
					HistoryId = history.HistoryId,
					SubmitBy = history.SubmitBy,
					SubmitOn = history.SubmitOn
				});
			}
			#endregion
			return response;
		}

	}
}