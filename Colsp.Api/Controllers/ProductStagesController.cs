using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Colsp.Entity.Models;
using Colsp.Model.Responses;
using Colsp.Api.Extensions;
using Colsp.Model.Requests;
using Colsp.Api.Constants;
using Colsp.Api.Helper;
using System.Data.Entity;
using Colsp.Api.Helpers;
using System.Web;
using System.Threading.Tasks;
using System.IO;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using CsvHelper;
using System.Web.Http.Cors;
using System.Data.Entity.Infrastructure;
using System.Security.Principal;
using System.Web.Script.Serialization;
using System.Data.Entity.Validation;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;
using Renci.SshNet;

namespace Colsp.Api.Controllers
{
	public class ProductStagesController : ApiController
	{
		private ColspEntities db = new ColspEntities();

		[Route("api/Products")]
		[HttpGet]
		public HttpResponseMessage GetProducts([FromUri] ProductRequest request)
		{
			try
			{
				var products = db.Products.Where(w => w.IsSell == true && w.IsMaster == false && !Constant.STATUS_REMOVE.Equals(w.Status))
					.Select(s => new
					{
						s.ProductNameEn,
						s.Pid,
						s.ProductId,
						s.Sku
					});
				if (request == null)
				{
					return Request.CreateResponse(HttpStatusCode.OK, products);
				}

				request.DefaultOnNull();
				if (!string.IsNullOrEmpty(request.SearchText))
				{
					products = products.Where(w => w.Pid.Contains(request.SearchText)
					|| w.ProductNameEn.Contains(request.SearchText)
					|| w.Sku.Contains(request.SearchText));
				}

				//count number of products
				var total = products.Count();
				//make paginate query from database
				var pagedProducts = products.Paginate(request);
				//create response
				var response = PaginatedResponse.CreateResponse(pagedProducts, request, total);
				//return response
				return Request.CreateResponse(HttpStatusCode.OK, response);
			}
			catch (Exception e)
			{
				return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
			}
		}

		[Route("api/ProductStages/AttributeSet")]
		[HttpPost]
		public HttpResponseMessage GetAttributeSetFromProduct(List<ProductStageRequest> request)
		{
			try
			{
				if (request == null)
				{
					throw new Exception("Invalid request");
				}
				var productIds = request.Select(s => s.ProductId).ToList();
				var attrSet = db.AttributeSets.Where(w => w.ProductStageGroups.Any(a => productIds.Contains(a.ProductId) && !Constant.STATUS_REMOVE.Equals(a.Status)))
					.Select(s => new
					{
						s.AttributeSetId,
						s.AttributeSetNameEn,
						ProductCount = s.ProductStageGroups.Where(w => productIds.Contains(w.ProductId))
					});
				return Request.CreateResponse(HttpStatusCode.OK, attrSet);
			}
			catch (Exception e)
			{
				return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
			}

		}

		[Route("api/ProductStages/Guidance/Export")]
		[HttpGet]
		public HttpResponseMessage GetAllGuidance()
		{
			try
			{
				var tmpGuidance = db.ImportHeaders.Where(w => true);
				if (User.ShopRequest() != null)
				{
					var groupName = User.ShopRequest().ShopGroup;
					tmpGuidance = tmpGuidance.Where(w => w.ShopGroups.Any(a => a.Abbr.Equals(groupName)));
				}
				var guidance = tmpGuidance.Where(w =>
						!w.MapName.Equals("ADI")
						&& !w.MapName.Equals("ADJ")
						&& !w.MapName.Equals("ADK"))
						.Select(s => new
						{
							s.GroupName,
							s.HeaderName,
							s.MapName,
							s.ImportHeaderId,
							s.AcceptedValue,
							s.Position,
						}).OrderBy(o => o.Position);
				return Request.CreateResponse(HttpStatusCode.OK, guidance);
			}
			catch (Exception e)
			{
				return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
			}
		}

		[Route("api/ProductStages/Visibility")]
		[HttpPut]
		public HttpResponseMessage SaveVisibilityProductStage(List<ProductStageRequest> request)
		{
			try
			{
				if (request == null || request.Count == 0)
				{
					throw new Exception("Invalid request");
				}
				IQueryable<ProductStage> productList = null;
				var ids = request.Select(s => s.ProductId).ToList();
				#region Permission
				if (User.ShopRequest() != null)
				{
					int shopId = User.ShopRequest().ShopId;
					productList = db.ProductStages.Where(w => w.ShopId == shopId && w.IsVariant == false);
				}
				else
				{
					productList = db.ProductStages.Where(w => w.IsVariant == false);
				}
				#endregion
				productList = productList.Where(w => ids.Contains(w.ProductId));
				var pids = productList.Select(s => s.Pid);
				var realProduct = db.Products.Where(w => pids.Contains(w.Pid)).Select(s => new
				{
					s.Pid
				});
				var email = User.UserRequest().Email;
				var currentDt = SystemHelper.GetCurrentDateTime();
				foreach (ProductStageRequest proRq in request)
				{
					var current = productList.Where(w => w.ProductId.Equals(proRq.ProductId)).SingleOrDefault();
					if (current == null)
					{
						throw new Exception("Cannot find product " + proRq.ProductId + " in shop " + proRq.ShopId);
					}
					current.Visibility = proRq.Visibility;
					current.UpdateBy = email;
					current.UpdateOn = currentDt;
					ProductStageGroup group = new ProductStageGroup()
					{
						ProductId = current.ProductId
					};
					var currentReal = realProduct.Where(w => w.Pid.Equals(current.Pid)).SingleOrDefault();
					if (currentReal != null)
					{
						Product product = new Product()
						{
							Pid = currentReal.Pid
						};
						db.Products.Attach(product);
						db.Entry(product).Property(p => p.Visibility).IsModified = true;
						db.Entry(product).Property(p => p.UpdateBy).IsModified = true;
						db.Entry(product).Property(p => p.UpdateOn).IsModified = true;
						product.Visibility = current.Visibility;
						product.UpdateBy = current.UpdateBy;
						product.UpdateOn = current.UpdateOn;
						db.ProductStageGroups.Attach(group);
						db.Entry(group).Property(p => p.OnlineFlag).IsModified = true;
						db.Entry(group).Property(p => p.UpdateBy).IsModified = true;
						db.Entry(group).Property(p => p.UpdateOn).IsModified = true;
						group.UpdateBy = current.UpdateBy;
						group.UpdateOn = current.UpdateOn;
						if (current.Visibility == false)
						{
							group.OnlineFlag = false;
						}
						else
						{
							group.OnlineFlag = true;
						}
					}
				}
				db.Configuration.ValidateOnSaveEnabled = false;
				Util.DeadlockRetry(db.SaveChanges, "ProductStage");
				return Request.CreateResponse(HttpStatusCode.OK);
			}
			catch (Exception e)
			{
				return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
			}
		}


		[Route("api/ProductStages/UnGroup")]
		[HttpGet]
		public HttpResponseMessage GetUnGroupProduct([FromUri] UnGroupProductRequest request)
		{
			try
			{
				if (request == null)
				{
					throw new Exception("Invalid Request");
				}
				var productTemp = db.ProductStages.Where(w => w.IsSell == true
					&& !Constant.STATUS_REMOVE.Equals(w.Status)
					&& !Constant.PRODUCT_STATUS_WAIT_FOR_APPROVAL.Equals(w.Status)
					&& w.IsVariant == false
					&& (w.ProductStageGroup.AttributeSetId == null
					|| w.ProductStageGroup.AttributeSetId == request.AttributeSetId))
					.Select(s => new
					{
						s.ShopId,
						s.Pid,
						s.ProductNameEn,
						s.Sku,
						s.ProductId
					});
				if (User.ShopRequest() != null)
				{
					int shopId = User.ShopRequest().ShopId;
					productTemp = productTemp.Where(w => w.ShopId == shopId);
				}
				else
				{
					productTemp = productTemp.Where(w => w.ShopId == request.ShopId);
				}
				request.DefaultOnNull();

				if (!string.IsNullOrWhiteSpace(request.SearchText))
				{
					productTemp = productTemp.Where(w => w.ProductNameEn.Contains(request.SearchText)
					|| w.Pid.Contains(request.SearchText)
					|| w.Sku.Contains(request.SearchText));
				}

				//count number of products
				var total = productTemp.Count();
				//make paginate query from database
				var pagedProducts = productTemp.Paginate(request);
				//create response
				var response = PaginatedResponse.CreateResponse(pagedProducts, request, total);
				//return response
				return Request.CreateResponse(HttpStatusCode.OK, response);
			}
			catch (Exception e)
			{
				return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
			}
		}

		[Route("api/ProductStages/PendingProduct")]
		[HttpPost]
		public HttpResponseMessage GroupPendingProduct(PendingProduct request)
		{
			try
			{
				#region Validation
				if (request == null)
				{
					throw new Exception("Invalid request.");
				}
				if (request.AttributeSet == null || request.AttributeSet.AttributeSetId == 0)
				{
					throw new Exception("Invalid attribute set.");
				}
				if(request.Variants.Count > 100)
				{
					throw new Exception("Cannot group more than 100 products.");
				}
				var pids = request.Variants.Select(s => s.Pid).ToList();
				if (pids == null || pids.Count == 0)
				{
					throw new Exception("No pid selected");
				}
				if (pids.Where(w => !string.IsNullOrWhiteSpace(w)).GroupBy(n => n).Any(c => c.Count() > 1))
				{
					throw new Exception("Please choose different products for each variant.");
				}
				#endregion
				#region Shop and User
				int shopId = -1;
				if (User.ShopRequest() != null)
				{
					shopId = User.ShopRequest().ShopId;
				}
				else
				{
					shopId = request.Shop.ShopId;
				}
				var email = User.UserRequest().Email;
				var currentDt = SystemHelper.GetCurrentDateTime();
				#endregion
				#region Default Variant
				var defaultVariantRq = request.Variants.Where(w => w.DefaultVariant == true).FirstOrDefault();
				if (defaultVariantRq == null)
				{
					throw new Exception("Cannot find default variant");
				}
				var productStage = db.ProductStages.Where(w => pids.Contains(w.Pid)).Include(i=>i.Inventory);
				var defaultVariant = productStage.Where(w => w.Pid.Equals(defaultVariantRq.Pid))
					.Include(i => i.ProductStageAttributes)
					.Include(i => i.ProductStageGroup)
					.Include(i => i.Inventory)
					.FirstOrDefault();
				if (defaultVariant == null)
				{
					throw new Exception("Cannot find default variant");
				}
				#endregion
				defaultVariant.ProductStageGroup.AttributeSetId = request.AttributeSet.AttributeSetId;
				defaultVariant.ProductStageGroup.GlobalCatId = request.Category.CategoryId;
				defaultVariant.ProductStageGroup.UpdateBy = email;
				defaultVariant.ProductStageGroup.UpdateOn = currentDt;
				defaultVariant.Status = Constant.PRODUCT_STATUS_DRAFT;
				if(defaultVariant.Inventory == null)
				{
					defaultVariant.Inventory = new Inventory()
					{
						CreateBy = email,
						CreateOn = currentDt,
						Defect = 0,
						MaxQtyAllowInCart = 0,
						MaxQtyPreOrder = 0,
						MinQtyAllowInCart = 0,
						OnHold = 0,
						Quantity = 0,
						Reserve = 0,
						SafetyStockAdmin = 0,
						SafetyStockSeller = 0,
						StockType = Constant.STOCK_TYPE[Constant.DEFAULT_STOCK_TYPE],
						UpdateBy = email,
						UpdateOn = currentDt,
						UseDecimal = false
					};
				}
				var parentVariant = new ProductStage()
				{
					ProductId = defaultVariant.ProductId,
					ShopId = defaultVariant.ShopId,
					ProductNameEn = defaultVariant.ProductNameEn,
					ProductNameTh = defaultVariant.ProductNameTh,
					ProdTDNameTh = defaultVariant.ProdTDNameTh,
					ProdTDNameEn = defaultVariant.ProdTDNameEn,
					JDADept = defaultVariant.JDADept,
					JDASubDept = defaultVariant.JDASubDept,
					SaleUnitTh = defaultVariant.SaleUnitTh,
					SaleUnitEn = defaultVariant.SaleUnitEn,
					Sku = defaultVariant.Sku,
					Upc = defaultVariant.Upc,
					OriginalPrice = defaultVariant.OriginalPrice,
					SalePrice = defaultVariant.SalePrice,
					DescriptionFullEn = defaultVariant.DescriptionFullEn,
					DescriptionShortEn = defaultVariant.DescriptionShortEn,
					DescriptionFullTh = defaultVariant.DescriptionFullTh,
					DescriptionShortTh = defaultVariant.DescriptionShortTh,
					MobileDescriptionEn = defaultVariant.MobileDescriptionEn,
					MobileDescriptionTh = defaultVariant.MobileDescriptionTh,
					ImageCount = defaultVariant.ImageCount,
					FeatureImgUrl = defaultVariant.FeatureImgUrl,
					PrepareDay = defaultVariant.PrepareDay,
					LimitIndividualDay = defaultVariant.LimitIndividualDay,
					PrepareMon = defaultVariant.PrepareMon,
					PrepareTue = defaultVariant.PrepareTue,
					PrepareWed = defaultVariant.PrepareWed,
					PrepareThu = defaultVariant.PrepareThu,
					PrepareFri = defaultVariant.PrepareFri,
					PrepareSat = defaultVariant.PrepareSat,
					PrepareSun = defaultVariant.PrepareSun,
					KillerPoint1En = defaultVariant.KillerPoint1En,
					KillerPoint2En = defaultVariant.KillerPoint2En,
					KillerPoint3En = defaultVariant.KillerPoint3En,
					KillerPoint1Th = defaultVariant.KillerPoint1Th,
					KillerPoint2Th = defaultVariant.KillerPoint2Th,
					KillerPoint3Th = defaultVariant.KillerPoint3Th,
					Installment = defaultVariant.Installment,
					Length = defaultVariant.Length,
					Height = defaultVariant.Height,
					Width = defaultVariant.Width,
					DimensionUnit = defaultVariant.DimensionUnit,
					Weight = defaultVariant.Weight,
					WeightUnit = defaultVariant.WeightUnit,
					MetaTitleEn = defaultVariant.MetaTitleEn,
					MetaTitleTh = defaultVariant.MetaTitleTh,
					MetaDescriptionEn = defaultVariant.MetaDescriptionEn,
					MetaDescriptionTh = defaultVariant.MetaDescriptionTh,
					MetaKeyEn = defaultVariant.MetaKeyEn,
					MetaKeyTh = defaultVariant.MetaKeyTh,
					SeoEn = defaultVariant.SeoEn,
					SeoTh = defaultVariant.SeoTh,
					IsHasExpiryDate = defaultVariant.IsHasExpiryDate,
					IsVat = defaultVariant.IsVat,
					BoostWeight = defaultVariant.BoostWeight,
					GlobalBoostWeight = defaultVariant.GlobalBoostWeight,
					ExpressDelivery = defaultVariant.ExpressDelivery,
					DeliveryFee = defaultVariant.DeliveryFee,
					PromotionPrice = defaultVariant.PromotionPrice,
					EffectiveDatePromotion = defaultVariant.EffectiveDatePromotion,
					ExpireDatePromotion = defaultVariant.ExpireDatePromotion,
					NewArrivalDate = defaultVariant.NewArrivalDate,
					DefaultVariant = false,
					Display = defaultVariant.Display,
					MiniQtyAllowed = defaultVariant.MiniQtyAllowed,
					MaxiQtyAllowed = defaultVariant.MaxiQtyAllowed,
					UnitPrice = defaultVariant.UnitPrice,
					PurchasePrice = defaultVariant.PurchasePrice,
					IsSell = false,
					IsVariant = false,
					IsMaster = false,
					VariantCount = 0,
					Visibility = defaultVariant.Visibility,
					OldPid = defaultVariant.OldPid,
					Bu = defaultVariant.Bu,
					Status = Constant.PRODUCT_STATUS_DRAFT,
					CreateBy = email,
					CreateOn = currentDt,
					UpdateBy = email,
					UpdateOn = currentDt,
					Inventory = new Inventory()
					{
						CreateBy = email,
						CreateOn = currentDt,
						Defect = defaultVariant.Inventory.Defect,
						MaxQtyAllowInCart = defaultVariant.Inventory.MaxQtyAllowInCart,
						MaxQtyPreOrder = defaultVariant.Inventory.MaxQtyPreOrder,
						MinQtyAllowInCart = defaultVariant.Inventory.MinQtyAllowInCart,
						OnHold = defaultVariant.Inventory.OnHold,
						Quantity = defaultVariant.Inventory.Quantity,
						Reserve = defaultVariant.Inventory.Reserve,
						SafetyStockAdmin = defaultVariant.Inventory.SafetyStockAdmin,
						SafetyStockSeller = defaultVariant.Inventory.SafetyStockSeller,
						StockType = defaultVariant.Inventory.StockType,
						UpdateBy = email,
						UpdateOn = currentDt,
						UseDecimal = defaultVariant.Inventory.UseDecimal
					}
				};
				foreach (var attribute in defaultVariant.ProductStageAttributes)
				{
					parentVariant.ProductStageAttributes.Add(new ProductStageAttribute()
					{
						AttributeId = attribute.AttributeId,
						AttributeValueId = attribute.AttributeValueId,
						CheckboxValue = attribute.CheckboxValue,
						CreateBy = email,
						CreateOn = currentDt,
						IsAttributeValue = attribute.IsAttributeValue,
						Position = attribute.Position,
						UpdateBy = email,
						UpdateOn = currentDt,
						ValueEn = attribute.ValueEn,
						ValueTh = attribute.ValueTh,
						HtmlBoxValue = attribute.HtmlBoxValue,
					});
				}
				parentVariant.VariantCount = request.Variants.Count;
				List<long> groupIds = new List<long>();
				foreach (var variantRq in request.Variants)
				{
					var variant = productStage.Where(w => w.Pid.Equals(variantRq.Pid)).FirstOrDefault();
					if (variant == null)
					{
						continue;
					}
					if (variant.ProductId != parentVariant.ProductId)
					{
						groupIds.Add(variant.ProductId);
					}
					if(variant.Inventory == null)
					{
						variant.Inventory = new Inventory()
						{
							CreateBy = email,
							CreateOn = currentDt,
							Defect = 0,
							MaxQtyAllowInCart = 0,
							MaxQtyPreOrder = 0,
							MinQtyAllowInCart = 0,
							OnHold = 0,
							Quantity = 0,
							Reserve = 0,
							SafetyStockAdmin = 0,
							SafetyStockSeller = 0,
							StockType = Constant.STOCK_TYPE[Constant.DEFAULT_STOCK_TYPE],
							UpdateBy = email,
							UpdateOn = currentDt,
							UseDecimal = false
						};
					}
					variant.DefaultVariant = variantRq.DefaultVariant;
					variant.ProductId = parentVariant.ProductId;
					variant.IsVariant = true;
					variant.IsSell = true;
					variant.IsMaster = false;
					variant.Status = Constant.PRODUCT_STATUS_DRAFT;
					db.ProductStageAttributes.RemoveRange(db.ProductStageAttributes.Where(w => w.Pid.Equals(variant.Pid)));
					if (variantRq.FirstAttribute != null && variantRq.FirstAttribute.AttributeId != 0)
					{
						variant.ProductStageAttributes.Add(new ProductStageAttribute()
						{
							AttributeId = variantRq.FirstAttribute.AttributeId,
							AttributeValueId = variantRq.FirstAttribute.AttributeValues[0].AttributeValueId,
							CheckboxValue = false,
							CreateBy = email,
							CreateOn = currentDt,
							IsAttributeValue = true,
							ValueEn = string.Concat("((", variantRq.FirstAttribute.AttributeValues[0].AttributeValueId, "))"),
							ValueTh = string.Concat("((", variantRq.FirstAttribute.AttributeValues[0].AttributeValueId, "))"),
							UpdateBy = email,
							UpdateOn = currentDt,
							Position = 1,
							HtmlBoxValue = string.Empty,
						});
					}
					if (variantRq.SecondAttribute != null && variantRq.SecondAttribute.AttributeId != 0)
					{
						variant.ProductStageAttributes.Add(new ProductStageAttribute()
						{
							AttributeId = variantRq.SecondAttribute.AttributeId,
							AttributeValueId = variantRq.SecondAttribute.AttributeValues[0].AttributeValueId,
							CheckboxValue = false,
							CreateBy = email,
							CreateOn = currentDt,
							IsAttributeValue = true,
							ValueEn = string.Concat("((", variantRq.SecondAttribute.AttributeValues[0].AttributeValueId, "))"),
							ValueTh = string.Concat("((", variantRq.SecondAttribute.AttributeValues[0].AttributeValueId, "))"),
							UpdateBy = email,
							UpdateOn = currentDt,
							Position = 2,
							HtmlBoxValue = string.Empty,
						});
					}
				}
				db.ProductStageRelateds.RemoveRange(db.ProductStageRelateds.Where(w => groupIds.Contains(w.Child)));
				db.ProductStageGroups.RemoveRange(db.ProductStageGroups.Where(w => groupIds.Contains(w.ProductId)));
				AutoGenerate.GeneratePid(db, new List<ProductStage>() { parentVariant });
				db.ProductStages.Add(parentVariant);
				Util.DeadlockRetry(db.SaveChanges, "ProductStage");
				return Request.CreateResponse(HttpStatusCode.OK);

			}
			catch (Exception e)
			{
				return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
			}
		}



		[Route("api/Products/Master/{productId}")]
		[HttpPut]
		public HttpResponseMessage SaveMasterProduct([FromUri]long productId, MasterProductRequest request)
		{
			try
			{
				if (request == null)
				{
					throw new Exception("Invalid request");
				}
				var masterProduct = db.Products.Where(w => w.ProductId == productId && w.IsMaster == true).SingleOrDefault();
				if (masterProduct == null)
				{
					throw new Exception("Cannot find master product");
				}
				var dbChild = db.Products.Where(w => w.MasterPid.Equals(masterProduct.Pid)).Select(s => s.Pid).ToList();
				List<string> newChildPids = new List<string>();
				foreach (var child in request.ChildProducts)
				{
					bool isNew = false;
					if (dbChild == null || dbChild.Count == 0)
					{
						isNew = true;
					}
					if (!isNew)
					{
						var current = dbChild.Where(w => w.Equals(child.Pid)).SingleOrDefault();
						if (current != null)
						{
							dbChild.Remove(current);
						}
						else
						{
							isNew = true;
						}
					}
					if (isNew)
					{
						Product product = new Product()
						{
							Pid = child.Pid
						};
						db.Products.Attach(product);
						db.Entry(product).Property(p => p.MasterPid).IsModified = true;
						product.MasterPid = masterProduct.Pid;
					}
				}
				if (dbChild != null && dbChild.Count > 0)
				{
					foreach (var id in dbChild)
					{
						Product product = new Product()
						{
							Pid = id
						};
						db.Products.Attach(product);
						db.Entry(product).Property(p => p.MasterPid).IsModified = true;
						product.MasterPid = null;
					}
				}
				db.Configuration.ValidateOnSaveEnabled = false;
				Util.DeadlockRetry(db.SaveChanges, "ProductStageMaster");
				return GetMasterProduct(productId);
			}
			catch (Exception e)
			{
				return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
			}
		}

		[Route("api/Products/Master")]
		[HttpPost]
		public HttpResponseMessage AddMasterProduct(MasterProductRequest request)

		{
			try
			{
				if (request == null)
				{
					throw new Exception("Invaide request");
				}

				var parentProduct = db.Products
					.Where(w => w.Pid.Equals(request.MasterProduct.Pid) && w.IsSell == true)
					.SingleOrDefault();

				if (parentProduct == null)
				{
					throw new Exception(string.Concat("Cannot find product ", request.MasterProduct.Pid));
				}

				var email = User.UserRequest().Email;
				var currentDt = SystemHelper.GetCurrentDateTime();

				var masterProduct = new Product()
				{
					ShopId = Constant.ADMIN_SHOP_ID,
					AttributeSetId = parentProduct.AttributeSetId,
					CreateBy = email,
					CreateOn = currentDt,
					EffectiveDate = parentProduct.EffectiveDate,
					ExpireDate = parentProduct.ExpireDate,
					GlobalCatId = parentProduct.GlobalCatId,
					IsBestSeller = parentProduct.IsBestSeller,
					IsClearance = parentProduct.IsClearance,
					IsNew = parentProduct.IsNew,
					IsOnlineExclusive = parentProduct.IsOnlineExclusive,
					IsOnlyAt = parentProduct.IsOnlyAt,
					LocalCatId = parentProduct.LocalCatId,
					Remark = parentProduct.Remark,
					Status = parentProduct.Status,
					UpdateBy = email,
					UpdateOn = currentDt,
					TheOneCardEarn = parentProduct.TheOneCardEarn,
					ShippingId = parentProduct.ShippingId,
					BrandId = parentProduct.BrandId,
					GiftWrap = parentProduct.GiftWrap,
					BoostWeight = parentProduct.BoostWeight,
					DefaultVariant = parentProduct.DefaultVariant,
					DeliveryFee = parentProduct.DeliveryFee,
					DescriptionFullEn = parentProduct.DescriptionFullEn,
					DescriptionFullTh = parentProduct.DescriptionFullTh,
					DescriptionShortEn = parentProduct.DescriptionShortEn,
					DescriptionShortTh = parentProduct.DescriptionShortTh,
					DimensionUnit = parentProduct.DimensionUnit,
					Display = parentProduct.Display,
					EffectiveDatePromotion = parentProduct.EffectiveDatePromotion,
					ExpireDatePromotion = parentProduct.ExpireDatePromotion,
					ExpressDelivery = parentProduct.ExpressDelivery,
					FeatureImgUrl = parentProduct.FeatureImgUrl,
					GlobalBoostWeight = parentProduct.GlobalBoostWeight,
					Height = parentProduct.Height,
					ImageCount = parentProduct.ImageCount,
					Installment = parentProduct.Installment,
					IsHasExpiryDate = parentProduct.IsHasExpiryDate,
					IsMaster = true,
					IsSell = false,
					IsVariant = false,
					IsVat = parentProduct.IsVat,
					JDADept = parentProduct.JDADept,
					JDASubDept = parentProduct.JDASubDept,
					KillerPoint1En = parentProduct.KillerPoint1En,
					KillerPoint1Th = parentProduct.KillerPoint1Th,
					KillerPoint2En = parentProduct.KillerPoint2En,
					KillerPoint2Th = parentProduct.KillerPoint2Th,
					KillerPoint3En = parentProduct.KillerPoint3En,
					KillerPoint3Th = parentProduct.KillerPoint3Th,
					Length = parentProduct.Length,
					LimitIndividualDay = parentProduct.LimitIndividualDay,
					MaxiQtyAllowed = parentProduct.MaxiQtyAllowed,
					MetaDescriptionEn = parentProduct.MetaDescriptionEn,
					MetaDescriptionTh = parentProduct.MetaDescriptionTh,
					MetaKeyEn = parentProduct.MetaKeyEn,
					MetaKeyTh = parentProduct.MetaKeyTh,
					MetaTitleEn = parentProduct.MetaTitleEn,
					MetaTitleTh = parentProduct.MetaTitleTh,
					MiniQtyAllowed = parentProduct.MiniQtyAllowed,
					MobileDescriptionEn = parentProduct.MobileDescriptionEn,
					MobileDescriptionTh = parentProduct.MobileDescriptionTh,
					NewArrivalDate = parentProduct.NewArrivalDate,
					OriginalPrice = parentProduct.OriginalPrice,
					PrepareDay = parentProduct.PrepareDay,
					PrepareFri = parentProduct.PrepareFri,
					PrepareMon = parentProduct.PrepareMon,
					PrepareSat = parentProduct.PrepareSat,
					PrepareSun = parentProduct.PrepareSun,
					PrepareThu = parentProduct.PrepareThu,
					PrepareTue = parentProduct.PrepareTue,
					PrepareWed = parentProduct.PrepareWed,
					ProdTDNameEn = parentProduct.ProdTDNameEn,
					ProdTDNameTh = parentProduct.ProdTDNameTh,
					ProductNameEn = parentProduct.ProductNameEn,
					ProductNameTh = parentProduct.ProductNameTh,
					PromotionPrice = parentProduct.PromotionPrice,
					PurchasePrice = parentProduct.PurchasePrice,
					SalePrice = parentProduct.SalePrice,
					SaleUnitEn = parentProduct.SaleUnitEn,
					SaleUnitTh = parentProduct.SaleUnitTh,
					UnitPrice = parentProduct.UnitPrice,
					SeoEn = parentProduct.SeoEn,
					SeoTh = parentProduct.SeoTh,
					Sku = parentProduct.Sku,
					Upc = parentProduct.Upc,
					Visibility = parentProduct.Visibility,
					VariantCount = parentProduct.VariantCount,
					Weight = parentProduct.Weight,
					WeightUnit = parentProduct.WeightUnit,
					Width = parentProduct.Width,
					MasterPid = null,
					ParentPid = null,
					Pid = parentProduct.Pid,
				};

				ProductStageGroup stageGroup = new ProductStageGroup()
				{
					ProductId = masterProduct.ProductId,
					ShopId = Constant.ADMIN_SHOP_ID,
					GlobalCatId = masterProduct.GlobalCatId,
					LocalCatId = masterProduct.LocalCatId,
					AttributeSetId = masterProduct.AttributeSetId,
					BrandId = masterProduct.BrandId,
					ShippingId = masterProduct.ShippingId,
					EffectiveDate = masterProduct.EffectiveDate,
					ExpireDate = masterProduct.ExpireDate,
					NewArrivalDate = masterProduct.NewArrivalDate,
					TheOneCardEarn = masterProduct.TheOneCardEarn,
					GiftWrap = masterProduct.GiftWrap,
					IsNew = masterProduct.IsNew,
					IsClearance = masterProduct.IsClearance,
					IsBestSeller = masterProduct.IsBestSeller,
					IsOnlineExclusive = masterProduct.IsOnlineExclusive,
					IsOnlyAt = masterProduct.IsOnlyAt,
					Remark = masterProduct.Remark,
					InfoFlag = false,
					ImageFlag = false,
					OnlineFlag = false,
					InformationTabStatus = Constant.PRODUCT_STATUS_WAIT_FOR_APPROVAL,
					ImageTabStatus = Constant.PRODUCT_STATUS_WAIT_FOR_APPROVAL,
					CategoryTabStatus = Constant.PRODUCT_STATUS_WAIT_FOR_APPROVAL,
					VariantTabStatus = Constant.PRODUCT_STATUS_WAIT_FOR_APPROVAL,
					MoreOptionTabStatus = Constant.PRODUCT_STATUS_WAIT_FOR_APPROVAL,
					RejectReason = string.Empty,
					Status = Constant.PRODUCT_STATUS_APPROVE,
					FirstApproveBy = null,
					FirstApproveOn = null,
					ApproveBy = null,
					ApproveOn = null,
					RejecteBy = null,
					RejectOn = null,
					SubmitBy = null,
					SubmitOn = null,
					CreateBy = email,
					CreateOn = currentDt,
					UpdateBy = email,
					UpdateOn = currentDt,
					ProductStages = new List<ProductStage>()
					{
						new ProductStage()
						{
							ProductId = masterProduct.ProductId,
							ShopId = Constant.ADMIN_SHOP_ID,
							ProductNameEn = masterProduct.ProductNameEn,
							ProductNameTh = masterProduct.ProductNameTh,
							ProdTDNameTh = masterProduct.ProdTDNameTh,
							ProdTDNameEn = masterProduct.ProdTDNameEn,
							JDADept = masterProduct.JDADept,
							JDASubDept = masterProduct.JDASubDept,
							SaleUnitTh = masterProduct.SaleUnitTh,
							SaleUnitEn = masterProduct.SaleUnitEn,
							Sku = masterProduct.Sku,
							Upc = masterProduct.Upc,
							OriginalPrice = masterProduct.OriginalPrice,
							SalePrice = masterProduct.SalePrice,
							DescriptionFullEn = masterProduct.DescriptionFullEn,
							DescriptionShortEn = masterProduct.DescriptionShortEn,
							DescriptionFullTh = masterProduct.DescriptionFullTh,
							DescriptionShortTh = masterProduct.DescriptionShortTh,
							MobileDescriptionEn = masterProduct.MobileDescriptionEn,
							MobileDescriptionTh = masterProduct.MobileDescriptionTh,
							ImageCount = masterProduct.ImageCount,
							FeatureImgUrl = masterProduct.FeatureImgUrl,
							PrepareDay = masterProduct.PrepareDay,
							LimitIndividualDay = masterProduct.LimitIndividualDay,
							PrepareMon = masterProduct.PrepareMon,
							PrepareTue = masterProduct.PrepareTue,
							PrepareWed = masterProduct.PrepareWed,
							PrepareThu = masterProduct.PrepareThu,
							PrepareFri = masterProduct.PrepareFri,
							PrepareSat = masterProduct.PrepareSat,
							PrepareSun = masterProduct.PrepareSun,
							KillerPoint1En = masterProduct.KillerPoint1En,
							KillerPoint2En = masterProduct.KillerPoint2En,
							KillerPoint3En = masterProduct.KillerPoint3En,
							KillerPoint1Th = masterProduct.KillerPoint1Th,
							KillerPoint2Th = masterProduct.KillerPoint2Th,
							KillerPoint3Th = masterProduct.KillerPoint3Th,
							Installment = masterProduct.Installment,
							Length = masterProduct.Length,
							Height = masterProduct.Height,
							Width = masterProduct.Width,
							DimensionUnit = masterProduct.DimensionUnit,
							Weight = masterProduct.Weight,
							WeightUnit = masterProduct.WeightUnit,
							MetaTitleEn = masterProduct.MetaTitleEn,
							MetaTitleTh = masterProduct.MetaTitleTh,
							MetaDescriptionEn = masterProduct.MetaDescriptionEn,
							MetaDescriptionTh = masterProduct.MetaDescriptionTh,
							MetaKeyEn = masterProduct.MetaKeyEn,
							MetaKeyTh = masterProduct.MetaKeyTh,
							SeoEn = masterProduct.SeoEn,
							SeoTh = masterProduct.SeoTh,
							IsHasExpiryDate = masterProduct.IsHasExpiryDate,
							IsVat = masterProduct.IsVat,
							UrlKey = masterProduct.UrlKey,
							BoostWeight = masterProduct.BoostWeight,
							GlobalBoostWeight = masterProduct.GlobalBoostWeight,
							ExpressDelivery = masterProduct.ExpressDelivery,
							DeliveryFee = masterProduct.DeliveryFee,
							PromotionPrice = masterProduct.PromotionPrice,
							EffectiveDatePromotion = masterProduct.EffectiveDatePromotion,
							ExpireDatePromotion = masterProduct.ExpireDatePromotion,
							NewArrivalDate = masterProduct.NewArrivalDate,
							DefaultVariant = masterProduct.DefaultVariant,
							Display = masterProduct.Display,
							MiniQtyAllowed = masterProduct.MiniQtyAllowed,
							MaxiQtyAllowed = masterProduct.MaxiQtyAllowed,
							UnitPrice = masterProduct.UnitPrice,
							PurchasePrice = masterProduct.PurchasePrice,
							IsSell = false,
							IsVariant = false,
							IsMaster = true,
							VariantCount = 0,
							Visibility = false,
							OldPid = null,
							Bu = null,
							Status = Constant.PRODUCT_STATUS_APPROVE,
							CreateBy = email,
							CreateOn = currentDt,
							UpdateBy = email,
							UpdateOn = currentDt,
						}
					}
				};

				AutoGenerate.GeneratePid(db, stageGroup.ProductStages);
				masterProduct.Pid = stageGroup.ProductStages.First().Pid;
				masterProduct.UrlKey = stageGroup.ProductStages.First().UrlKey;
				masterProduct.ShopId = Constant.ADMIN_SHOP_ID;
				stageGroup.ProductId = masterProduct.ProductId = db.GetNextProductStageGroupId().SingleOrDefault().Value;
				db.ProductStageGroups.Add(stageGroup);
				db.Products.Add(masterProduct);
				var childPids = request.ChildProducts.Select(s => s.Pid).ToList();
				childPids.Add(request.MasterProduct.Pid);
				var childList = db.Products.Where(w => childPids.Contains(w.Pid)).ToList();
				childList.ForEach(f => f.MasterPid = masterProduct.Pid);
				Util.DeadlockRetry(db.SaveChanges, "ProductStage");
				return GetMasterProduct(masterProduct.ProductId);
			}
			catch (Exception e)
			{
				return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
			}
		}

		[Route("api/Products/Master/{productId}")]
		[HttpGet]
		public HttpResponseMessage GetMasterProduct(long productId)
		{
			try
			{
				var master = db.Products.Where(w => w.ProductId == productId && w.IsMaster == true)
					.Select(s => new
					{
						MasterProduct = new
						{
							s.ProductId,
							s.ProductNameEn,
							s.Pid,
						},
						ChildProducts = db.Products.Where(w => w.MasterPid.Equals(s.Pid)).Select(sc => new
						{
							sc.ProductId,
							sc.Pid,
							sc.ProductNameEn
						}),
						UpdatedDt = s.UpdateOn,
					}).FirstOrDefault();
				return Request.CreateResponse(HttpStatusCode.OK, master);
			}
			catch (Exception e)
			{
				return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
			}
		}

		[Route("api/Products/Master")]
		[HttpGet]
		public HttpResponseMessage GetMasterProduct([FromUri]ProductRequest request)
		{
			try
			{

				var master = db.Products.Where(w => w.IsMaster == true).Select(s => new
				{
					s.ProductId,
					s.ProductNameEn,
					s.Pid,
					ChildPids = db.Products.Where(w => w.MasterPid.Equals(s.Pid)).Select(sc => new
					{
						sc.Pid
					}),
					UpdatedDt = s.UpdateOn,
				});
				request.DefaultOnNull();

				if (!string.IsNullOrEmpty(request.SearchText))
				{
					master = master.Where(p => p.ProductId.ToString().Equals(request.SearchText)
					|| p.ProductNameEn.Contains(request.SearchText)
					|| p.Pid.Contains(request.SearchText)
					|| p.ChildPids.Any(a => a.Pid.Contains(request.SearchText)));
				}

				var total = master.Count();
				var pagedProducts = master.Paginate(request);
				var response = PaginatedResponse.CreateResponse(pagedProducts, request, total);
				return Request.CreateResponse(HttpStatusCode.OK, response);
			}
			catch (Exception e)
			{
				return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
			}
		}



		[Route("api/ProductStages/Template")]
		[HttpPost]
		public HttpResponseMessage ExportTemplate(CSVTemplateRequest request)
		{
			MemoryStream stream = null;
			StreamWriter writer = null;
			try
			{
				if (request == null)
				{
					throw new Exception("Invalid request");
				}
				var tmpGuidance = db.ImportHeaders.Where(w => true);
				if (User.ShopRequest() != null)
				{
					var groupName = User.ShopRequest().ShopGroup;
					tmpGuidance = tmpGuidance.Where(w => w.ShopGroups.Any(a => a.Abbr.Equals(groupName)));
				}
				var guidance = tmpGuidance.OrderBy(o => o.Position).ToList();
				//tmpGuidance.Where(w=>!"DAT".Equals(w.MapName)).OrderBy(o => o.ImportHeaderId).ToList();
				List<string> header = new List<string>();
				foreach (var g in guidance)
				{
					header.Add(g.HeaderName);
				}
				List<string> defaultAttribute = null;
				if (User.ShopRequest() != null)
				{
					defaultAttribute = db.Attributes
						.Where(w => w.DefaultAttribute && Constant.ATTRIBUTE_VISIBLE_ALL_USER.Equals(w.VisibleTo))
						.Select(s => s.AttributeNameEn)
						.ToList();
				}
				else
				{
					defaultAttribute = db.Attributes
						.Where(w => w.DefaultAttribute)
						.Select(s => s.AttributeNameEn)
						.ToList();
				}

				if (defaultAttribute != null && defaultAttribute.Count > 0)
				{
					header.AddRange(defaultAttribute);
				}
				if (request.GlobalCategories != null)
				{
					List<int> categoryIds = request.GlobalCategories.Select(s => s.CategoryId).ToList();
					var categories = db.GlobalCategories.Where(w => categoryIds.Contains(w.CategoryId)).Select(s =>
					new
					{
						s.NameEn,
						s.CategoryId,
						AttribuyeSet = s.GlobalCatAttributeSetMaps.Select(se =>
						new
						{
							se.AttributeSet.AttributeSetNameEn,
							Attribute = se.AttributeSet.AttributeSetMaps.Select(sa => sa.Attribute.AttributeNameEn)
						})
					}).ToList();
					if (categories != null && categories.Count > 0)
					{
						HashSet<string> attribute = new HashSet<string>();
						foreach (var cat in categories)
						{
							foreach (var attibutS in cat.AttribuyeSet)
							{
								foreach (var attr in attibutS.Attribute)
								{
									attribute.Add(attr);
								}
							}
						}
						if (attribute != null && attribute.Count > 0)
						{
							header.AddRange(attribute.ToList());
						}
					}
				}

				stream = new MemoryStream();
				writer = new StreamWriter(stream);
				var csv = new CsvWriter(writer);
				foreach (string h in header)
				{
					csv.WriteField(h);
				}
				csv.NextRecord();
				writer.Flush();
				stream.Position = 0;
				HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);
				result.Content = new StreamContent(stream);
				result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream")
				{
					CharSet = Encoding.UTF8.WebName
				};
				result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
				result.Content.Headers.ContentDisposition.FileName = "file.csv";
				return result;
			}
			catch (Exception e)
			{
				return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
			}
		}


		[Route("api/ProductStages/Guidance")]
		[HttpGet]
		public HttpResponseMessage GetGuidance(string SearchText, int _limit)
		{
			try
			{
				if (_limit < 0)
				{
					throw new Exception("Limit cannot be negative");
				}
				var tmpGuidance = db.ImportHeaders.Where(w => true);
				if (User.ShopRequest() != null)
				{
					var groupName = User.ShopRequest().ShopGroup;
					tmpGuidance = tmpGuidance.Where(w => w.ShopGroups.Any(a => a.Abbr.Equals(groupName)));
				}
				var guidance = tmpGuidance.Where(w => w.HeaderName.Contains(SearchText))
					.Select(s => new ImportHeaderRequest()
					{
						HeaderName = s.HeaderName,
						Description = s.Description,
						Example = s.Example,
						GroupName = s.GroupName,
						Note = s.Note,
						AcceptedValue = s.AcceptedValue,
						IsAttribute = false
					}).Take(_limit).ToList();
				if (guidance != null && guidance.Count < _limit)
				{
					var tmpAttribute = db.Attributes.Where(w => true);
					if (User.ShopRequest() != null)
					{
						tmpAttribute = tmpAttribute.Where(w => w.DefaultAttribute == false
						|| (w.DefaultAttribute == true && Constant.ATTRIBUTE_VISIBLE_ALL_USER.Equals(w.VisibleTo)));
					}
					var attribute = tmpAttribute.Where(w => w.AttributeNameEn.Contains(SearchText))
						.Select(s => new ImportHeaderRequest()
						{
							HeaderName = s.AttributeNameEn,
							GroupName = s.AttributeNameEn,
							IsVariant = s.VariantStatus,
							AttributeType = s.DataType,
							IsAttribute = true,
							Description = s.AttributeDescriptionEn,
							AttributeValue = s.AttributeValueMaps.Select(sv => new { sv.AttributeValue.AttributeValueEn, sv.AttributeValue.AttributeValueTh }).ToList()
						}).Take(_limit - guidance.Count).ToList();
					guidance.AddRange(attribute);
				}
				return Request.CreateResponse(HttpStatusCode.OK, guidance);
			}
			catch (Exception e)
			{
				return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
			}
		}

		[Route("api/ProductStages/All/Image")]
		[HttpPut]
		public HttpResponseMessage SaveChangeAllImage(List<VariantRequest> request)
		{
			try
			{
				if (request == null)
				{
					throw new Exception("Invalid request");
				}
				int shopId = User.ShopRequest().ShopId;
				var email = User.UserRequest().Email;
				var currentDt = SystemHelper.GetCurrentDateTime();
				var pids = request.Select(s => s.Pid).ToList();
				var products = db.ProductStages.Where(w => w.ShopId == shopId && pids.Contains(w.Pid));
				db.ProductStageImages.RemoveRange(db.ProductStageImages.Where(w => pids.Contains(w.Pid)));
				HashSet<long> productGroup = new HashSet<long>();
				foreach (VariantRequest varRq in request)
				{
					var pro = products.Where(w => w.Pid.Equals(varRq.Pid)).SingleOrDefault();
					if (pro == null)
					{
						throw new Exception(string.Concat("Cannot find product ", varRq.Pid));
					}

					if (pro.IsVariant)
					{
						if (varRq.VariantImg != null && varRq.VariantImg.Count > 0)
						{
							int position = 1;
							foreach (var img in varRq.VariantImg)
							{
								pro.ProductStageImages.Add(new ProductStageImage()
								{
									ImageId = img.ImageId,
									CreateBy = email,
									CreateOn = currentDt,
									FeatureFlag = position == 1 ? true : false,
									ImageName = img.Url.Split('/').Last(),
									SeqNo = position++,
									Large = true,
									Normal = true,
									Thumbnail = true,
									Zoom = true,
									Pid = pro.Pid,
									ShopId = shopId,
									Status = Constant.STATUS_ACTIVE,
									UpdateBy = email,
									UpdateOn = currentDt,
								});
							}
							pro.FeatureImgUrl = pro.ProductStageImages.ElementAt(0).ImageName;
							pro.Status = Constant.PRODUCT_STATUS_DRAFT;

							if (productGroup.Contains(pro.ProductId))
							{
								continue;
							}
							ProductStageGroup group = new ProductStageGroup()
							{
								ProductId = pro.ProductId,
							};
							db.ProductStageGroups.Attach(group);
							db.Entry(group).Property(p => p.ImageFlag).IsModified = true;
							db.Entry(group).Property(p => p.Status).IsModified = true;
							group.Status = Constant.PRODUCT_STATUS_DRAFT;
							group.ImageFlag = true;
							productGroup.Add(pro.ProductId);
						}
						else
						{
							pro.FeatureImgUrl = string.Empty;
						}
					}
					else
					{
						if (varRq.MasterImg != null && varRq.MasterImg.Count > 0)
						{

							int position = 1;
							foreach (var img in varRq.MasterImg)
							{
								pro.ProductStageImages.Add(new ProductStageImage()
								{
									ImageId = img.ImageId,
									CreateBy = email,
									CreateOn = currentDt,
									FeatureFlag = position == 1 ? true : false,
									ImageName = img.Url.Split('/').Last(),
									SeqNo = position++,
									Large = true,
									Normal = true,
									Thumbnail = true,
									Zoom = true,
									Pid = pro.Pid,
									ShopId = shopId,
									Status = Constant.STATUS_ACTIVE,
									UpdateBy = email,
									UpdateOn = currentDt,
								});
							}
							pro.FeatureImgUrl = pro.ProductStageImages.ElementAt(0).ImageName;
							pro.Status = Constant.PRODUCT_STATUS_DRAFT;
							if (productGroup.Contains(pro.ProductId))
							{
								continue;
							}
							ProductStageGroup group = new ProductStageGroup()
							{
								ProductId = pro.ProductId,
							};
							db.ProductStageGroups.Attach(group);
							db.Entry(group).Property(p => p.ImageFlag).IsModified = true;
							db.Entry(group).Property(p => p.Status).IsModified = true;
							group.Status = Constant.PRODUCT_STATUS_DRAFT;
							group.ImageFlag = true;
							productGroup.Add(pro.ProductId);
						}
						else
						{
							pro.FeatureImgUrl = string.Empty;
						}
					}
				}
				foreach (var stage in products)
				{
					SetupStageAfterSave(stage,email,currentDt,db, false);
				}
				db.Configuration.ValidateOnSaveEnabled = false;
				Util.DeadlockRetry(db.SaveChanges, "ProductStage");

				return Request.CreateResponse(HttpStatusCode.OK);
			}
			catch (Exception e)
			{
				return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
			}
		}

		[Route("api/ProductStages/All")]
		[HttpGet]
		public HttpResponseMessage GetProductAllStages([FromUri] ProductRequest request)
		{
			try
			{
				var products = db.ProductStages.Where(w => !Constant.STATUS_REMOVE.Equals(w.Status) && w.Visibility == true)
					.Select(productStage => new
					{
						productStage.ProductId,
						productStage.Sku,
						productStage.Upc,
						productStage.ProductNameEn,
						productStage.ProductNameTh,
						productStage.Pid,
						productStage.Status,
						MasterImg = productStage.ProductStageImages.Select(s => new
						{
							ImageId = s.ImageId,
							Url = Constant.PRODUCT_IMAGE_URL + s.ImageName,
							position = s.SeqNo
						}).OrderBy(o => o.position),
						VariantImg = productStage.ProductStageImages.Select(s => new
						{
							ImageId = s.ImageId,
							Url = Constant.PRODUCT_IMAGE_URL + s.ImageName,
							position = s.SeqNo
						}).OrderBy(o => o.position),
						productStage.IsVariant,
						productStage.VariantCount,
						productStage.ProductStageComments.FirstOrDefault().Comment,
						VariantAttribute = productStage.ProductStageAttributes.Select(s => new
						{
							s.Attribute.AttributeNameEn,
							Value = s.IsAttributeValue ? (from tt in db.AttributeValues where tt.MapValue.Equals(s.ValueEn) select tt.AttributeValueEn).FirstOrDefault()
								: s.ValueEn,
						}),
						CreatedDt = productStage.CreateOn,
						productStage.ShopId,
						Brand = productStage.ProductStageGroup.Brand != null ? new { productStage.ProductStageGroup.Brand.BrandId, productStage.ProductStageGroup.Brand.BrandNameEn } : null,
					});


				if (User.ShopRequest() != null)
				{
					int shopId = User.ShopRequest().ShopId;
					products = products.Where(w => w.ShopId == shopId);
					if (User.BrandRequest() != null)
					{
						var brands = User.BrandRequest().Select(s => s.BrandId).ToList();
						if (brands != null && brands.Count > 0)
						{
							products = products.Where(w => brands.Contains(w.Brand.BrandId));
						}
					}
				}
				if (request == null)
				{
					return Request.CreateResponse(HttpStatusCode.OK, products.ToList());
				}
				request.DefaultOnNull();
				if (!string.IsNullOrEmpty(request.SearchText))
				{
					products = products.Where(p => p.Sku.Contains(request.SearchText)
					|| p.ProductNameEn.Contains(request.SearchText)
					|| p.ProductNameTh.Contains(request.SearchText)
					|| p.Pid.Contains(request.SearchText)
					|| p.Upc.Contains(request.SearchText));
				}
				if (!string.IsNullOrEmpty(request.Pid))
				{
					products = products.Where(p => p.Pid.Equals(request.Pid));
				}
				if (!string.IsNullOrEmpty(request._filter))
				{

					if (string.Equals("ImageMissing", request._filter, StringComparison.OrdinalIgnoreCase))
					{
						products = products.Where(w => w.MasterImg.Count() == 0 && w.VariantImg.Count() == 0);
					}
					else if (string.Equals("Approved", request._filter, StringComparison.OrdinalIgnoreCase))
					{
						products = products.Where(p => p.Status.Equals(Constant.PRODUCT_STATUS_APPROVE));
					}
					else if (string.Equals("NotApproved", request._filter, StringComparison.OrdinalIgnoreCase))
					{
						products = products.Where(p => p.Status.Equals(Constant.PRODUCT_STATUS_NOT_APPROVE));
					}
					else if (string.Equals("WaitforApproval", request._filter, StringComparison.OrdinalIgnoreCase))
					{
						products = products.Where(p => p.Status.Equals(Constant.PRODUCT_STATUS_WAIT_FOR_APPROVAL));
					}
					else if (string.Equals("Draft", request._filter, StringComparison.OrdinalIgnoreCase))
					{
						products = products.Where(p => p.Status.Equals(Constant.PRODUCT_STATUS_DRAFT));
					}
				}
				var total = products.Count();
				var pagedProducts = products.Paginate(request);
				var response = PaginatedResponse.CreateResponse(pagedProducts, request, total);
				return Request.CreateResponse(HttpStatusCode.OK, response);
			}
			catch (Exception e)
			{
				return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
			}
		}

		/// <summary>
		/// This endpoint will take in advance search criteria from request
		/// and return list of product
		/// </summary>
		/// <param name="request"></param>
		/// <returns>List of product with advance criteria</returns>
		[Route("api/ProductStages/Search")]
		[HttpPost]
		public HttpResponseMessage GetProductStagesAdvance(ProductRequest request)
		{
			try
			{
				//Request cannot be null
				if (request == null)
				{
					throw new Exception("Invalid request");
				}
				//Prepare Query to query table ProductStage
				var products = (from p in db.ProductStageGroups
								where !Constant.STATUS_REMOVE.Equals(p.Status)
									&& p.ProductStages.Any(a => a.IsVariant == false)
								select new
								{
									p.ProductStages.FirstOrDefault().Sku,
									p.ProductStages.FirstOrDefault().Pid,
									p.ProductStages.FirstOrDefault().Upc,
									p.ProductStages.FirstOrDefault().ProductId,
									p.ProductStages.FirstOrDefault().ProductNameEn,
									p.ProductStages.FirstOrDefault().ProductNameTh,
									p.ProductStages.FirstOrDefault().SalePrice,
									p.ProductStages.FirstOrDefault().OriginalPrice,
									p.ProductStages.FirstOrDefault().IsMaster,
									p.Shop.ShopNameEn,
									p.Status,
									p.ImageFlag,
									p.InfoFlag,
									p.OnlineFlag,
									p.ProductStages.FirstOrDefault().Visibility,
									p.ProductStages.FirstOrDefault().VariantCount,
									ImageUrl = string.Empty.Equals(p.ProductStages.FirstOrDefault().FeatureImgUrl) ? string.Empty : string.Concat(Constant.PRODUCT_IMAGE_URL, p.ProductStages.FirstOrDefault().FeatureImgUrl),
									GlobalCategory = p.GlobalCategory != null ? new { p.GlobalCategory.CategoryId, p.GlobalCategory.NameEn, p.GlobalCategory.Lft, p.GlobalCategory.Rgt } : null,
									LocalCategory = p.LocalCategory != null ? new { p.LocalCategory.CategoryId, p.LocalCategory.NameEn, p.LocalCategory.Lft, p.LocalCategory.Rgt } : null,
									Brand = p.Brand != null ? new { p.Brand.BrandId, p.Brand.BrandNameEn } : null,
									Tags = p.ProductStageTags.Select(s => s.Tag),
									CreatedDt = p.CreateOn,
									UpdatedDt = p.UpdateOn,
									Shop = new { p.Shop.ShopId, p.Shop.ShopNameEn },
									p.InformationTabStatus,
									p.ImageTabStatus,
									p.MoreOptionTabStatus,
									p.VariantTabStatus,
									p.CategoryTabStatus,
								});
				//check if its seller permission
				bool isSeller = false;
				if (User.ShopRequest() != null)
				{
					//add shopid criteria for seller request
					int shopId = User.ShopRequest().ShopId;
					products = products.Where(w => w.Shop.ShopId == shopId);
					if (User.BrandRequest() != null)
					{
						var brands = User.BrandRequest().Select(s => s.BrandId).ToList();
						if (brands != null && brands.Count > 0)
						{
							products = products.Where(w => brands.Contains(w.Brand.BrandId));
						}
					}
					isSeller = true;
				}
				//set request default value
				request.DefaultOnNull();
				//add ProductName criteria
				if (request.ProductNames != null && request.ProductNames.Count > 0)
				{
					products = products.Where(w => request.ProductNames.Any(a => w.ProductNameEn.Contains(a))
					|| request.ProductNames.Any(a => w.ProductNameTh.Contains(a)));
				}
				//add Pid criteria
				if (request.Pids != null && request.Pids.Count > 0)
				{
					products = products.Where(w => request.Pids.Any(a => w.Pid.Contains(a)));
				}
				//add Sku criteria
				if (request.Skus != null && request.Skus.Count > 0)
				{
					products = products.Where(w => request.Skus.Any(a => w.Sku.Contains(a)));
				}
				//add Brand criteria
				if (request.Brands != null && request.Brands.Count > 0)
				{
					//if request send brand id, add brand id criteria
					List<int> brandIds = request.Brands.Where(w => w.BrandId != 0).Select(s => s.BrandId).ToList();
					if (brandIds != null && brandIds.Count > 0)
					{
						products = products.Where(w => brandIds.Contains(w.Brand.BrandId));
					}
					//if request send brand name, add brand name criteria
					List<string> brandNames = request.Brands.Where(w => w.BrandNameEn != null && !string.IsNullOrWhiteSpace(w.BrandNameEn)).Select(s => s.BrandNameEn).ToList();
					if (brandNames != null && brandNames.Count > 0)
					{
						products = products.Where(w => brandNames.Any(a => w.Brand.BrandNameEn.Contains(a)));
					}
				}
				if (request.Shops != null && request.Shops.Count > 0)
				{
					List<int> shopIds = request.Shops.Where(w => w.ShopId != 0).Select(s => s.ShopId).ToList();
					if (shopIds != null && shopIds.Count > 0)
					{
						products = products.Where(w => shopIds.Contains(w.Shop.ShopId));
					}
				}
				//add Global category criteria
				if (request.GlobalCategories != null && request.GlobalCategories.Count > 0)
				{
					//if request send category parent left and right, add category parent left and right criteria
					var lft = request.GlobalCategories.Where(w => w.Lft != 0).Select(s => s.Lft).ToList();
					var rgt = request.GlobalCategories.Where(w => w.Rgt != 0).Select(s => s.Rgt).ToList();
					if (lft != null && lft.Count > 0 && rgt != null && rgt.Count > 0)
					{
						products = products.Where(w => lft.Any(a => a <= w.GlobalCategory.Lft) && rgt.Any(a => a >= w.GlobalCategory.Rgt));
					}
					//if request send category name, add category category name criteria
					List<string> catNames = request.GlobalCategories.Where(w => w.NameEn != null && !string.IsNullOrWhiteSpace(w.NameEn)).Select(s => s.NameEn).ToList();
					if (catNames != null && catNames.Count > 0)
					{
						products = products.Where(w => catNames.Any(a => w.GlobalCategory.NameEn.Contains(a)));
					}
				}
				//add Local category criteria
				if (request.LocalCategories != null && request.LocalCategories.Count > 0)
				{
					//if request send category parent left and right, add category parent left and right criteria
					var lft = request.LocalCategories.Where(w => w.Lft != 0).Select(s => s.Lft).ToList();
					var rgt = request.LocalCategories.Where(w => w.Rgt != 0).Select(s => s.Rgt).ToList();

					if (lft != null && lft.Count > 0 && rgt != null && rgt.Count > 0)
					{
						products = products.Where(w => lft.Any(a => a <= w.LocalCategory.Lft) && rgt.Any(a => a >= w.LocalCategory.Rgt));
					}
					//if request send category name, add category category name criteria
					List<string> catNames = request.LocalCategories.Where(w => w.NameEn != null && !string.IsNullOrWhiteSpace(w.NameEn)).Select(s => s.NameEn).ToList();
					if (catNames != null && catNames.Count > 0)
					{
						products = products.Where(w => catNames.Any(a => w.LocalCategory.NameEn.Contains(a)));
					}
				}
				//add Tag criteria
				if (request.Tags != null && request.Tags.Count > 0)
				{
					products = products.Where(w => request.Tags.Any(a => w.Tags.Contains(a)));
				}
				//add sale price(from) criteria
				if (request.PriceFrom != 0)
				{
					products = products.Where(w => w.SalePrice >= request.PriceFrom);
				}
				//add sale price(to) criteria
				if (request.PriceTo != 0)
				{
					products = products.Where(w => w.SalePrice <= request.PriceTo);
				}
				//add create date(from) criteria
				if (request.CreatedDtFrom != null)
				{
					DateTime from = Convert.ToDateTime(request.CreatedDtFrom);
					products = products.Where(w => w.CreatedDt >= from);
				}
				//add create date(to) criteria
				if (request.CreatedDtTo != null)
				{
					DateTime to = Convert.ToDateTime(request.CreatedDtTo);
					products = products.Where(w => w.CreatedDt <= to);
				}
				//add modify date(from) criteria
				if (request.ModifyDtFrom != null)
				{
					DateTime from = Convert.ToDateTime(request.ModifyDtFrom);
					products = products.Where(w => w.UpdatedDt >= from);
				}
				//add modify date(to) criteria
				if (request.ModifyDtTo != null)
				{
					DateTime to = Convert.ToDateTime(request.ModifyDtTo);
					products = products.Where(w => w.UpdatedDt <= to);
				}
				if (!string.IsNullOrEmpty(request.SearchText))
				{
					if (isSeller)
					{
						products = products.Where(p => p.Sku.Contains(request.SearchText)
							|| p.ProductNameEn.Contains(request.SearchText)
							|| p.ProductNameTh.Contains(request.SearchText)
							|| p.Pid.Contains(request.SearchText)
							|| p.Upc.Contains(request.SearchText)
							|| p.Tags.Any(a => a.Contains(request.SearchText)));
					}
					else
					{
						products = products.Where(p => p.Sku.Contains(request.SearchText)
							|| p.ProductNameEn.Contains(request.SearchText)
							|| p.ProductNameTh.Contains(request.SearchText)
							|| p.Pid.Contains(request.SearchText)
							|| p.Upc.Contains(request.SearchText));
					}
				}
				//add filter criteria
				if (!string.IsNullOrEmpty(request._filter))
				{
					if (string.Equals("Draft", request._filter, StringComparison.OrdinalIgnoreCase))
					{
						products = products.Where(p => p.Status.Equals(Constant.PRODUCT_STATUS_DRAFT));
					}
					else if (string.Equals("Approved", request._filter, StringComparison.OrdinalIgnoreCase))
					{
						products = products.Where(p => p.Status.Equals(Constant.PRODUCT_STATUS_APPROVE));
					}
					else if (string.Equals("NotApproved", request._filter, StringComparison.OrdinalIgnoreCase))
					{
						products = products.Where(p => p.Status.Equals(Constant.PRODUCT_STATUS_NOT_APPROVE));
					}
					else if (string.Equals("WaitforApproval", request._filter, StringComparison.OrdinalIgnoreCase))
					{
						products = products.Where(p => p.Status.Equals(Constant.PRODUCT_STATUS_WAIT_FOR_APPROVAL));
					}
					else if (string.Equals("ProductMaster", request._filter, StringComparison.OrdinalIgnoreCase))
					{
						products = products.Where(p => p.IsMaster == true);
					}
					else if (string.Equals("Single", request._filter, StringComparison.OrdinalIgnoreCase))
					{
						products = products.Where(p => p.VariantCount == 0);
					}
					else if (string.Equals("Variant", request._filter, StringComparison.OrdinalIgnoreCase))
					{
						products = products.Where(p => p.VariantCount > 0);
					}
				}
				if (!string.IsNullOrEmpty(request._filter2))
				{
					if (string.Equals("Information", request._filter2, StringComparison.OrdinalIgnoreCase))
					{
						products = products.Where(p => p.InformationTabStatus.Equals(Constant.PRODUCT_STATUS_WAIT_FOR_APPROVAL));
					}
					else if (string.Equals("Image", request._filter2, StringComparison.OrdinalIgnoreCase))
					{
						products = products.Where(p => p.ImageTabStatus.Equals(Constant.PRODUCT_STATUS_WAIT_FOR_APPROVAL));
					}
					else if (string.Equals("Category", request._filter2, StringComparison.OrdinalIgnoreCase))
					{
						products = products.Where(p => p.CategoryTabStatus.Equals(Constant.PRODUCT_STATUS_WAIT_FOR_APPROVAL));
					}
					else if (string.Equals("Variation", request._filter2, StringComparison.OrdinalIgnoreCase))
					{
						products = products.Where(p => p.VariantTabStatus.Equals(Constant.PRODUCT_STATUS_WAIT_FOR_APPROVAL));
					}
					else if (string.Equals("More", request._filter2, StringComparison.OrdinalIgnoreCase))
					{
						products = products.Where(p => p.MoreOptionTabStatus.Equals(Constant.PRODUCT_STATUS_WAIT_FOR_APPROVAL));
					}
					else if (string.Equals("ReadyForAction", request._filter2, StringComparison.OrdinalIgnoreCase))
					{
						products = products.Where(p =>
						   p.InformationTabStatus.Equals(Constant.PRODUCT_STATUS_APPROVE)
						&& p.ImageTabStatus.Equals(Constant.PRODUCT_STATUS_APPROVE)
						&& p.VariantTabStatus.Equals(Constant.PRODUCT_STATUS_APPROVE)
						&& p.MoreOptionTabStatus.Equals(Constant.PRODUCT_STATUS_APPROVE)
						&& p.CategoryTabStatus.Equals(Constant.PRODUCT_STATUS_APPROVE));
					}
				}
				//count number of products
				var total = products.Count();
				//make paginate query from database
				var pagedProducts = products.Paginate(request);
				//create response
				var response = PaginatedResponse.CreateResponse(pagedProducts, request, total);
				//return response
				return Request.CreateResponse(HttpStatusCode.OK, response);
			}
			//if anything wrong happen return error
			catch (Exception e)
			{
				return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
			}
		}

		[Route("api/ProductStages")]
		[HttpGet]
		//[ClaimsAuthorize(Permission = new string[] { "View Product", "View All Products" })]
		public HttpResponseMessage GetProductStages([FromUri] ProductRequest request)
		{
			try
			{
				if (request == null)
				{
					throw new Exception("Invalid request");
				}

				var products = db.ProductStages
					.Where(w => w.IsVariant == false && !Constant.STATUS_REMOVE.Equals(w.Status))
					.Select(s => new
				{
					s.Sku,
					s.Pid,
					s.Upc,
					s.ProductId,
					s.ProductNameEn,
					s.ProductNameTh,
					s.OriginalPrice,
					s.SalePrice,
					s.ProductStageGroup.Status,
					s.ProductStageGroup.ImageFlag,
					s.ProductStageGroup.InfoFlag,
					s.ProductStageGroup.OnlineFlag,
					s.Visibility,
					s.VariantCount,
					s.IsMaster,
					ImageUrl = string.Empty.Equals(s.FeatureImgUrl) ? string.Empty : string.Concat(Constant.PRODUCT_IMAGE_URL, s.FeatureImgUrl),
					s.ProductStageGroup.GlobalCatId,
					s.ProductStageGroup.LocalCatId,
					s.ProductStageGroup.AttributeSetId,
					s.ProductStageAttributes,
					UpdatedDt = s.ProductStageGroup.UpdateOn,
					CreatedDt = s.ProductStageGroup.CreateOn,
					s.ShopId,
					s.ProductStageGroup.InformationTabStatus,
					s.ProductStageGroup.ImageTabStatus,
					s.ProductStageGroup.CategoryTabStatus,
					s.ProductStageGroup.VariantTabStatus,
					s.ProductStageGroup.MoreOptionTabStatus,
					Tags = s.ProductStageGroup.ProductStageTags.Select(t => t.Tag),
					Shop = new { s.Shop.ShopId, s.Shop.ShopNameEn },
					Brand = s.ProductStageGroup.Brand != null ? new { s.ProductStageGroup.Brand.BrandId, s.ProductStageGroup.Brand.BrandNameEn } : null,
				});
				bool isSeller = false;
				if (User.ShopRequest() != null)
				{
					int shopId = User.ShopRequest().ShopId;
					products = products.Where(w => w.ShopId == shopId);
					if (User.BrandRequest() != null)
					{
						var brands = User.BrandRequest().Select(s => s.BrandId).ToList();
						if (brands != null && brands.Count > 0)
						{
							products = products.Where(w => brands.Contains(w.Brand.BrandId));
						}
					}
					isSeller = true;
				}
				request.DefaultOnNull();
				if (request.GlobalCatId != 0)
				{
					products = products.Where(p => p.GlobalCatId == request.GlobalCatId);
				}
				if (request.LocalCatId != 0)
				{
					products = products.Where(p => p.LocalCatId == request.LocalCatId);
				}
				if (request.AttributeSetId != 0)
				{
					products = products.Where(p => p.LocalCatId == request.LocalCatId);
				}
				if (request.AttributeId != 0)
				{
					products = products.Where(p => p.ProductStageAttributes.All(a => a.AttributeId == request.AttributeId));
				}
				if (!string.IsNullOrEmpty(request.SearchText))
				{
					if (isSeller)
					{
						products = products.Where(p => p.Sku.Contains(request.SearchText)
							|| p.ProductNameEn.Contains(request.SearchText)
							|| p.ProductNameTh.Contains(request.SearchText)
							|| p.Pid.Contains(request.SearchText)
							|| p.Upc.Contains(request.SearchText)
							|| p.Tags.Any(a => a.Contains(request.SearchText)));
					}
					else
					{
						products = products.Where(p => p.Sku.Contains(request.SearchText)
							|| p.ProductNameEn.Contains(request.SearchText)
							|| p.ProductNameTh.Contains(request.SearchText)
							|| p.Pid.Contains(request.SearchText)
							|| p.Upc.Contains(request.SearchText));
					}
					
				}
				if (!string.IsNullOrEmpty(request.Pid))
				{
					products = products.Where(p => p.Pid.Equals(request.Pid));
				}
				if (!string.IsNullOrEmpty(request._filter))
				{

					if (string.Equals("Draft", request._filter, StringComparison.OrdinalIgnoreCase))
					{
						products = products.Where(p => p.Status.Equals(Constant.PRODUCT_STATUS_DRAFT));
					}
					else if (string.Equals("Approved", request._filter, StringComparison.OrdinalIgnoreCase))
					{
						products = products.Where(p => p.Status.Equals(Constant.PRODUCT_STATUS_APPROVE));
					}
					else if (string.Equals("NotApproved", request._filter, StringComparison.OrdinalIgnoreCase))
					{
						products = products.Where(p => p.Status.Equals(Constant.PRODUCT_STATUS_NOT_APPROVE));
					}
					else if (string.Equals("WaitforApproval", request._filter, StringComparison.OrdinalIgnoreCase))
					{
						products = products.Where(p => p.Status.Equals(Constant.PRODUCT_STATUS_WAIT_FOR_APPROVAL));
					}
					else if (string.Equals("ProductMaster", request._filter, StringComparison.OrdinalIgnoreCase))
					{
						products = products.Where(p => p.IsMaster == true);
					}
					else if (string.Equals("Single", request._filter, StringComparison.OrdinalIgnoreCase))
					{
						products = products.Where(p => p.VariantCount == 0);
					}
					else if (string.Equals("Variant", request._filter, StringComparison.OrdinalIgnoreCase))
					{
						products = products.Where(p => p.VariantCount > 0);
					}
				}
				if (!string.IsNullOrEmpty(request._filter2))
				{
					if (string.Equals("Information", request._filter2, StringComparison.OrdinalIgnoreCase))
					{
						products = products.Where(p => p.InformationTabStatus.Equals(Constant.PRODUCT_STATUS_WAIT_FOR_APPROVAL));
					}
					else if (string.Equals("Image", request._filter2, StringComparison.OrdinalIgnoreCase))
					{
						products = products.Where(p => p.ImageTabStatus.Equals(Constant.PRODUCT_STATUS_WAIT_FOR_APPROVAL));
					}
					else if (string.Equals("Category", request._filter2, StringComparison.OrdinalIgnoreCase))
					{
						products = products.Where(p => p.CategoryTabStatus.Equals(Constant.PRODUCT_STATUS_WAIT_FOR_APPROVAL));
					}
					else if (string.Equals("Variation", request._filter2, StringComparison.OrdinalIgnoreCase))
					{
						products = products.Where(p => p.VariantTabStatus.Equals(Constant.PRODUCT_STATUS_WAIT_FOR_APPROVAL));
					}
					else if (string.Equals("More", request._filter2, StringComparison.OrdinalIgnoreCase))
					{
						products = products.Where(p => p.MoreOptionTabStatus.Equals(Constant.PRODUCT_STATUS_WAIT_FOR_APPROVAL));
					}
					else if (string.Equals("ReadyForAction", request._filter2, StringComparison.OrdinalIgnoreCase))
					{
						products = products.Where(p =>
						   p.InformationTabStatus.Equals(Constant.PRODUCT_STATUS_APPROVE)
						&& p.ImageTabStatus.Equals(Constant.PRODUCT_STATUS_APPROVE)
						&& p.VariantTabStatus.Equals(Constant.PRODUCT_STATUS_APPROVE)
						&& p.MoreOptionTabStatus.Equals(Constant.PRODUCT_STATUS_APPROVE));
					}
				}
				var total = products.Count();
				var pagedProducts = products.Paginate(request);
				var response = PaginatedResponse.CreateResponse(pagedProducts, request, total);
				return Request.CreateResponse(HttpStatusCode.OK, response);
			}
			catch (Exception e)
			{
				return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
			}
		}


		[Route("api/ProductStages")]
		[HttpPost]
		public HttpResponseMessage AddProduct(ProductStageRequest request)
		{
			try
			{
				#region request validation
				var shop = User.ShopRequest();
				if (request == null || shop == null)
				{
					throw new Exception("Invalid request");
				}
				#endregion
				#region attribute
				List<int> attributeids = new List<int>();
				attributeids.AddRange(request.MasterAttribute.Where(w => w.AttributeId != 0).Select(s => s.AttributeId));
				attributeids.AddRange(request.Variants.Where(w => w.FirstAttribute.AttributeId != 0).Select(s => s.FirstAttribute.AttributeId));
				attributeids.AddRange(request.Variants.Where(w => w.SecondAttribute.AttributeId != 0).Select(s => s.SecondAttribute.AttributeId));

				var attributeList = db.Attributes.Where(w => attributeids.Contains(w.AttributeId)).Select(s => new AttributeRequest()
				{
					AttributeId = s.AttributeId,
					DataType = s.DataType,
					AttributeValues = s.AttributeValueMaps.Select(sv => new AttributeValueRequest()
					{
						AttributeValueId = sv.AttributeValueId,
					}).ToList(),
				}).ToList();
				#endregion
				#region inventory
				var inventoryList = new List<Inventory>();
				#endregion
				ProductStageGroup group = new ProductStageGroup();
				var email = User.UserRequest().Email;
				var currentDt = SystemHelper.GetCurrentDateTime();
				SetupProductStageGroup(group, request, attributeList, inventoryList, shop.ShopId, false, true, email, currentDt, db);
				#region validate url key
				var urls = group.ProductStages.Select(s => s.UrlKey);
				if(db.ProductStages.Where(w=> urls.Contains(w.UrlKey)).Count() > 0)
				{
					throw new Exception("Product URL Key has already been used.");
				}
				#endregion
				AutoGenerate.GeneratePid(db, group.ProductStages);
				group.ProductId = db.GetNextProductStageGroupId().Single().Value;
				db.ProductStageGroups.Add(group);
				SetupGroupAfterSave(group,email,currentDt, db, true);
				Util.DeadlockRetry(db.SaveChanges, "ProductStage");
				#region send to cmos
				Task.Run(() =>
				{
					Thread.Sleep(1000);
					using (ColspEntities db = new ColspEntities())
					{
						SendToCmos(group, Apis.CmosCreateProduct, "POST", email, currentDt, db);
						db.SaveChanges();
					}
				});
				#endregion
				return GetProductStage(group.ProductId);
			}
			catch (Exception e)
			{
				return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
			}
		}

		[Route("api/ProductStages/{productId}")]
		[HttpPut]
		public HttpResponseMessage SaveChangeProduct([FromUri]long productId, ProductStageRequest request)
		{
			try
			{
				#region Validation
				var tmpGroup = db.ProductStageGroups.Where(w => true);
				bool isAdmin = true;
				if (User.ShopRequest() != null)
				{
					var shopId = User.ShopRequest().ShopId;
					tmpGroup = tmpGroup.Where(w => w.ShopId == shopId);
					isAdmin = false;
				}
				var group = tmpGroup.Where(w => w.ProductId == productId).Include(i => i.ProductStages).SingleOrDefault();
				if (group == null)
				{
					throw new Exception(string.Concat("Cannot find product with id ", productId));
				}
				#endregion
				#region Remove stuff
				db.ProductStageGlobalCatMaps.RemoveRange(db.ProductStageGlobalCatMaps.Where(w => w.ProductId == productId));
				db.ProductStageLocalCatMaps.RemoveRange(db.ProductStageLocalCatMaps.Where(w => w.ProductId == productId));
				db.ProductStageTags.RemoveRange(db.ProductStageTags.Where(w => w.ProductId == productId));
				db.ProductStageRelateds.RemoveRange(db.ProductStageRelateds.Where(w => w.Parent == productId));
				List<string> pids = new List<string>();
				pids.Add(request.MasterVariant.Pid);
				pids.AddRange(request.Variants.Select(s => s.Pid));
				db.ProductStageAttributes.RemoveRange(db.ProductStageAttributes.Where(w => pids.Contains(w.Pid)));
				db.ProductStageImages.RemoveRange(db.ProductStageImages.Where(w => pids.Contains(w.Pid)));
				db.ProductStageVideos.RemoveRange(db.ProductStageVideos.Where(w => pids.Contains(w.Pid)));
				#endregion
				#region attribute
				List<int> attributeids = new List<int>();
				attributeids.AddRange(request.MasterAttribute.Where(w => w.AttributeId != 0).Select(s => s.AttributeId));
				attributeids.AddRange(request.Variants.Where(w => w.FirstAttribute.AttributeId != 0).Select(s => s.FirstAttribute.AttributeId));
				attributeids.AddRange(request.Variants.Where(w => w.SecondAttribute.AttributeId != 0).Select(s => s.SecondAttribute.AttributeId));
				var attributeList = db.Attributes.Where(w => attributeids.Contains(w.AttributeId)).Select(s => new AttributeRequest()
				{
					AttributeId = s.AttributeId,
					DataType = s.DataType,
					AttributeValues = s.AttributeValueMaps.Select(sv => new AttributeValueRequest()
					{
						AttributeValueId = sv.AttributeValueId,
					}).ToList(),
				}).ToList();
				#endregion
				#region Setup
				var inventoryList = db.Inventories.Where(w => pids.Contains(w.Pid)).ToList();
				var email = User.UserRequest().Email;
				var currentDt = SystemHelper.GetCurrentDateTime();
				SetupProductStageGroup(group, request, attributeList, inventoryList, group.ShopId, isAdmin, false, email, currentDt, db);
				#region validate url key
				foreach (var stage in group.ProductStages)
				{
					if (!string.IsNullOrEmpty(stage.UrlKey))
					{
						if (!string.IsNullOrEmpty(stage.Pid))
						{
							if (db.ProductStages.Where(w => w.UrlKey.Equals(stage.UrlKey) 
								&& !w.Pid.Equals(stage.Pid)).Count() > 0)
							{
								throw new Exception("Product URL Key has already been used.");
							}
						}
						else
						{
							if (db.ProductStages.Where(w => w.UrlKey.Equals(stage.UrlKey)).Count() > 0)
							{
								throw new Exception("Product URL Key has already been used.");
							}
						}
					}
				}
				#endregion
				AutoGenerate.GeneratePid(db, group.ProductStages);
				SetupGroupAfterSave(group,email,currentDt, db, false);
				if (Constant.RETURN_STATUS_APPROVE.Equals(group.Status))
				{
					//setup approve product
					SetupApprovedProduct(group, db);
					//send to elastic search
					#region send to cmos
					//send to cmos
					Task.Run(() =>
					{
						Thread.Sleep(1000);
						using (ColspEntities db = new ColspEntities())
						{
							SendToElastic(group, Apis.ElasticCreateProduct, "POST", email, currentDt, db);
							db.SaveChanges();
						}
					});
					#endregion
				}
				//save change database
				Util.DeadlockRetry(db.SaveChanges, "ProductStage");
				#endregion
				#region send to cmos
				//send to cmos
				Task.Run(() =>
				{
					Thread.Sleep(1000);
					using (ColspEntities db = new ColspEntities())
					{
						SendToCmos(group, Apis.CmosUpdateProduct, "PUT", email, currentDt, db);
						db.SaveChanges();
					}
				});
				#endregion
				return GetProductStage(group.ProductId);
			}
			catch (Exception e)
			{
				return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
			}
		}

		[Route("api/ProductStages/{productId}")]
		[HttpGet]
		public HttpResponseMessage GetProductStage(long productId)
		{
			try
			{
				ProductStageRequest response = GetProductStageRequestFromId(db, productId);
				return Request.CreateResponse(HttpStatusCode.OK, response);
			}
			catch (Exception e)
			{
				return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
			}
		}

		private void SetupProductStageGroup(ProductStageGroup group, ProductStageRequest request, List<AttributeRequest> attributeList, List<Inventory> inventoryList, int shopId, bool isAdmin, bool isNew, string email, DateTime currentDt, ColspEntities db)
		{
			group.ShopId = shopId;
			#region setup category
			if (request.MainGlobalCategory != null && request.MainGlobalCategory.CategoryId != 0)
			{
				group.GlobalCatId = request.MainGlobalCategory.CategoryId;
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
					if(tmpTag.Length > 30)
					{
						throw new Exception("Tag field must be no longer than 30 characters.");
					}
					if(group.ProductStageTags.Any(a=>string.Equals(a.Tag, tmpTag, StringComparison.OrdinalIgnoreCase)))
					{
						continue;
					}
					if(group.ProductStageTags.Count >= 30)
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
			group.NewArrivalDate = request.NewArrivalDate;
			if(group.NewArrivalDate == null)
			{
				group.NewArrivalDate = currentDt;
			}
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
				if (masterVariant.VariantCount == 0)
				{
					masterVariant.IsSell = true;
				}
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
			stage.Upc = Validation.ValidateString(request.Upc, "UPC", true, 13, false,string.Empty);
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

		private ProductStageRequest GetProductStageRequestFromId(ColspEntities db, long productId)
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

		private void SetupApprovedProduct(ProductStageGroup group, ColspEntities db)
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

					//bool isNewGlobalCat = false;
					//if (globalCatList == null || globalCatList.Count == 0)
					//{
					//    isNewGlobalCat = true;
					//}
					//if (!isNewGlobalCat)
					//{
					//    var currentGlobalCat = globalCatList.Where(w => w.CategoryId == category.CategoryId).SingleOrDefault();
					//    if (currentGlobalCat != null)
					//    {
					//        globalCatList.Remove(currentGlobalCat);
					//    }
					//    else
					//    {
					//        isNewGlobalCat = true;
					//    }
					//}
					//if (isNewGlobalCat)
					//{
					//    product.ProductGlobalCatMaps.Add(new ProductGlobalCatMap()
					//    {
					//        CategoryId = category.CategoryId,
					//        CreateBy = category.CreateBy,
					//        CreateOn = category.CreateOn,
					//        UpdateBy = category.UpdateBy,
					//        UpdateOn = category.UpdateOn
					//    });
					//}
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

					//bool isNewLocalCat = false;
					//if (localCatList == null || localCatList.Count == 0)
					//{
					//    isNewLocalCat = true;
					//}
					//if (!isNewLocalCat)
					//{
					//    var currentLocalCat = localCatList.Where(w => w.CategoryId == category.CategoryId).SingleOrDefault();
					//    if (currentLocalCat != null)
					//    {
					//        localCatList.Remove(currentLocalCat);
					//    }
					//    else
					//    {
					//        isNewLocalCat = true;
					//    }
					//}
					//if (isNewLocalCat)
					//{
					//    product.ProductLocalCatMaps.Add(new ProductLocalCatMap()
					//    {
					//        CategoryId = category.CategoryId,
					//        CreateBy = category.CreateBy,
					//        CreateOn = category.CreateOn,
					//        UpdateBy = category.UpdateBy,
					//        UpdateOn = category.UpdateOn
					//    });
					//}
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

					//bool isNewVideo = false;
					//if (videoList == null || videoList.Count == 0)
					//{
					//    isNewVideo = true;
					//}
					//if (!isNewVideo)
					//{
					//    var currentVideo = videoList.Where(w => w.VideoUrlEn.Equals(video.VideoUrlEn)).SingleOrDefault();
					//    if (currentVideo != null)
					//    {
					//        videoList.Remove(currentVideo);
					//    }
					//    else
					//    {
					//        isNewVideo = true;
					//    }
					//}
					//if (isNewVideo)
					//{
					//    product.ProductVideos.Add(new ProductVideo()
					//    {
					//        VideoId = 0,
					//        VideoUrlEn = video.VideoUrlEn,
					//        Position = video.Position,
					//        Status = video.Status,
					//        CreateBy = video.CreateBy,
					//        CreateOn = video.CreateOn,
					//        UpdateBy = video.UpdateBy,
					//        UpdateOn = video.UpdateOn
					//    });
					//}
					//history.ProductHistoryVideos.Add(new ProductHistoryVideo()
					//{
					//    VideoUrlEn = video.VideoUrlEn,
					//    Position = video.Position,
					//    Status = video.Status,
					//    CreateBy = video.CreateBy,
					//    CreateOn = video.CreateOn,
					//    UpdateBy = video.UpdateBy,
					//    UpdateOn = video.UpdateOn
					//});
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

					//bool isNewTag = false;
					//if (tagList == null || tagList.Count == 0)
					//{
					//    isNewTag = true;
					//}
					//if (!isNewTag)
					//{
					//    var currentTag = tagList.Where(w => w.Tag.Equals(tag.Tag)).SingleOrDefault();
					//    if (currentTag != null)
					//    {
					//        tagList.Remove(currentTag);
					//    }
					//    else
					//    {
					//        isNewTag = true;
					//    }
					//}
					//if (isNewTag)
					//{
					//    product.ProductTags.Add(new ProductTag()
					//    {
					//        Tag = tag.Tag,
					//        CreateBy = tag.CreateBy,
					//        CreateOn = tag.CreateOn,
					//        UpdateBy = tag.UpdateBy,
					//        UpdateOn = tag.UpdateOn
					//    });
					//}
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

		private void SetupGroupAfterSave(ProductStageGroup groupProduct,string email,DateTime currentDt, ColspEntities db, bool isNew = false)
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
						}else
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
				SetupStageAfterSave(stage,email,currentDt, db, isNew);
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

		private void SetupStageAfterSave(ProductStage stage,string email,DateTime currentDt, ColspEntities db = null, bool isNew = false)
		{
			#region Image
			using (SftpClient sft = new SftpClient("27.254.48.250", "mkp", "Mkp@123!"))
			{
				sft.Connect();
				string productRootFolder = Path.Combine(
						AppSettingKey.IMAGE_ROOT_PATH,
						AppSettingKey.PRODUCT_FOLDER);
				foreach (var image in stage.ProductStageImages)
				{
					string lastPart = image.ImageName;
					string newFileName = string.Concat(stage.Pid, "_", image.SeqNo, Path.GetExtension(lastPart));
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

		public static Image ScaleImage(Image image, int maxWidth, int maxHeight)
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

		private void SendToCmos(ProductStageGroup group, string url, string method
			, string email, DateTime currentDt, ColspEntities db)
		{
			List<CmosRequest> requests = new List<CmosRequest>();
			foreach (var stage in group.ProductStages.Where(w=>w.IsSell))
			{
				CmosRequest cms = new CmosRequest()
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
				requests.Add(cms);
			}
			Dictionary<string, string> headers = new Dictionary<string, string>();
			headers.Add(Apis.CmosKeyAppIdKey, Apis.CmosKeyAppIdValue);
			headers.Add(Apis.CmosKeyAppSecretKey, Apis.CmosKeyAppSecretValue);
			foreach (var req in requests)
			{
				var json = new JavaScriptSerializer().Serialize(req);
				SystemHelper.SendRequest(url, method, headers, json, email, currentDt, "SP", "CMOS", db);
			}
		}


		private void SendToElastic(ProductStageGroup productGroup, string url, string method
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
				s.UrlKey
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

				ElasticRequest cms = new ElasticRequest()
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
					//StockAvailable = stage,
					tag = productGroup.ProductStageTags.Select(s => s.Tag).ToList(),
					DefaultProduct = defaultProduct != null && stage.IsVariant ? new DefaultProductRequest()
					{
						Pid = defaultProduct.Pid,
						ShopUrlKey = shop.UrlKey
					} : null,
					Attributes = responseAttribute
				};
				requests.Add(cms);
			}
			foreach (var req in requests)
			{
				var tmpJson = new
				{
					Pid = req.Pid,
					data = req
				};
				var json = new JavaScriptSerializer().Serialize(tmpJson);
				SystemHelper.SendRequest(url, method, null, json, email, currentDt, "SP", "ELAS", db);
			}
		}


		[Route("api/ProductStages/Approve")]
		[HttpPut]
		public HttpResponseMessage ApproveProduct(List<ProductStageRequest> request)
		{
			try
			{
				if (request == null || request.Count == 0)
				{
					throw new Exception("Invalid request");
				}
				var productIds = request.Select(s => s.ProductId).ToList();
				if (productIds.Count > Constant.BULK_APPROVE_LIMIT)
				{
					throw new Exception(string.Concat("Cannot bulk appove more than ", Constant.BULK_APPROVE_LIMIT, " products"));
				}
				var groupList = db.ProductStageGroups.Where(w => productIds.Contains(w.ProductId))
					.Include(i => i.ProductStages.Select(s => s.ProductStageAttributes))
					.Include(i => i.ProductStages.Select(s => s.ProductStageVideos))
					.Include(i => i.ProductStageTags)
					.Include(i => i.ProductStageGlobalCatMaps)
					.Include(i => i.ProductStageLocalCatMaps);
				var email = User.UserRequest().Email;
				var currentDt = SystemHelper.GetCurrentDateTime();
				foreach (ProductStageRequest proRq in request)
				{
					var pro = groupList.Where(w => w.ProductId == proRq.ProductId).SingleOrDefault();
					if (pro == null)
					{
						throw new Exception("Cannot find deleted product");
					}
					pro.Status = Constant.PRODUCT_STATUS_APPROVE;
					pro.ProductStages.ToList().ForEach(e => e.Status = Constant.PRODUCT_STATUS_APPROVE);
					pro.OnlineFlag = true;
					pro.ApproveBy = email;
					pro.ApproveOn = currentDt;
					if (string.IsNullOrEmpty(pro.FirstApproveBy))
					{
						pro.FirstApproveBy = email;
						pro.FirstApproveOn = currentDt;
					}
					SetupApprovedProduct(pro, db);
				}
				Util.DeadlockRetry(db.SaveChanges, "ProductStage");
				return Request.CreateResponse(HttpStatusCode.OK);
			}
			catch (Exception e)
			{
				return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
			}
		}


		[Route("api/ProductStages/Reject")]
		[HttpPut]
		public HttpResponseMessage RejectProduct(List<ProductStageRequest> request)
		{
			try
			{
				if (request == null || request.Count == 0)
				{
					throw new Exception("Invalid request");
				}
				var productIds = request.Select(s => s.ProductId).ToList();
				if (productIds.Count > Constant.BULK_APPROVE_LIMIT)
				{
					throw new Exception(string.Concat("Cannot bulk appove more than ", Constant.BULK_APPROVE_LIMIT, " products"));
				}
				var groupList = db.ProductStageGroups.Where(w => productIds.Contains(w.ProductId))
					.Include(i => i.ProductStages);
				var email = User.UserRequest().Email;
				var currentDt = SystemHelper.GetCurrentDateTime();
				foreach (ProductStageRequest proRq in request)
				{
					var pro = groupList.Where(w => w.ProductId == proRq.ProductId).SingleOrDefault();
					if (pro == null)
					{
						throw new Exception("Cannot find deleted product");
					}
					pro.Status = Constant.PRODUCT_STATUS_NOT_APPROVE;
					pro.RejecteBy = email;
					pro.RejectOn = currentDt;
					pro.ProductStages.ToList().ForEach(e => e.Status = Constant.PRODUCT_STATUS_NOT_APPROVE);
				}
				Util.DeadlockRetry(db.SaveChanges, "ProductStage");
				return Request.CreateResponse(HttpStatusCode.OK);
			}
			catch (Exception e)
			{
				return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
			}
		}


		//duplicate
		[Route("api/ProductStages/{productId}")]
		[HttpPost]
		public HttpResponseMessage DuplicateProductStage(long productId)
		{
			try
			{
				if (User.ShopRequest() == null)
				{
					throw new Exception("Shop is required");
				}
				var response = GetProductStageRequestFromId(db, productId);
				if (response == null)
				{
					throw new Exception("Cannot find product with id " + productId);
				}
				if (response.MasterVariant != null)
				{
					response.MasterVariant.SEO.ProductUrlKeyEn = null;
					response.MasterVariant.Pid = null;
					response.MasterVariant.ProductNameEn = string.Concat("Copy of ", response.MasterVariant.ProductNameEn);
					if (response.MasterVariant.Images != null)
					{
						response.MasterVariant.Images.Clear();
					}
					response.Status = Constant.PRODUCT_STATUS_DRAFT;
				}
				if (response.Variants != null)
				{
					response.Variants.Where(w => w.SEO != null).ToList().ForEach(f => { f.SEO.ProductUrlKeyEn = null; f.Pid = null; f.Images.Clear(); f.Status = Constant.PRODUCT_STATUS_DRAFT; });
				}
				return AddProduct(response);
			}
			catch (Exception e)
			{
				return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
			}
		}

		[Route("api/ProductStages")]
		[HttpDelete]
		public HttpResponseMessage DeleteProduct(List<ProductStageRequest> request)
		{
			try
			{
				int shopId = User.ShopRequest().ShopId;
				var email = User.UserRequest().Email;
				var currentDt = SystemHelper.GetCurrentDateTime();
				var ids = request.Select(s => s.ProductId).ToList();
				var producGrouptList = db.ProductStageGroups.Where(w => w.ShopId == shopId && ids.Contains(w.ProductId)).Select(s => new
				{
					s.ProductId,
					ProductStages = s.ProductStages.Select(st => new
					{
						st.Pid,
						st.Inventory,
					}),
				});
				var stagePids = producGrouptList.SelectMany(s => s.ProductStages.Select(st => st.Pid));
				var productProd = db.Products.Where(w => stagePids.Contains(w.Pid)).Select(s => s.Pid);
				//var producGrouptList = db.ProductStageGroups.Where(w => w.ShopId == shopId && ids.Contains(w.ProductId)).Include(i=>i.ProductStages.Select(s=>s.Inventory));
				foreach (ProductStageRequest proRq in request)
				{
					var productGroup = producGrouptList.Where(w => w.ProductId == proRq.ProductId).SingleOrDefault();
					if (productGroup == null)
					{
						throw new Exception("Cannot find deleted product");
					}
					var pids = productGroup.ProductStages.Select(s => s.Pid).ToList();
					var inventories = productGroup.ProductStages.Select(s => s.Inventory).ToList();
					foreach (var inventory in inventories)
					{
						ProductStage stage = new ProductStage()
						{
							Pid = inventory.Pid
						};
						db.ProductStages.Attach(stage);
						db.Entry(stage).Property(p => p.Status).IsModified = true;
						db.Entry(stage).Property(p => p.UrlKey).IsModified = true;
						db.Entry(stage).Property(p => p.UpdateBy).IsModified = true;
						db.Entry(stage).Property(p => p.UpdateOn).IsModified = true;
						stage.Status = Constant.STATUS_REMOVE;
						stage.UrlKey = string.Concat(stage.Pid, "_DELETE");
						stage.UpdateBy = email;
						stage.UpdateOn = currentDt;

						if (productProd.Contains(inventory.Pid))
						{
							Product product = new Product()
							{
								Pid = inventory.Pid
							};
							db.Products.Attach(product);
							db.Entry(product).Property(p => p.Status).IsModified = true;
							db.Entry(product).Property(p => p.UrlKey).IsModified = true;
							db.Entry(product).Property(p => p.UpdateBy).IsModified = true;
							db.Entry(product).Property(p => p.UpdateOn).IsModified = true;
							product.Status = Constant.STATUS_REMOVE;
							product.UrlKey = string.Concat(product.Pid, "_DELETE");
							product.UpdateBy = email;
							product.UpdateOn = currentDt;
						}

						db.InventoryHistories.Add(new InventoryHistory()
						{
							Pid = inventory.Pid,
							StockType = inventory.StockType,
							MaxQtyAllowInCart = inventory.MaxQtyAllowInCart,
							MinQtyAllowInCart = inventory.MinQtyAllowInCart,
							UseDecimal = inventory.UseDecimal,
							MaxQtyPreOrder = inventory.MaxQtyPreOrder,
							Defect = inventory.Defect,
							OnHold = inventory.OnHold,
							Quantity = inventory.Quantity,
							Reserve = inventory.Reserve,
							SafetyStockAdmin = inventory.SafetyStockAdmin,
							SafetyStockSeller = inventory.SafetyStockSeller,
							Status = Constant.INVENTORY_STATUS_DELETE,
							CreateBy = email,
							CreateOn = currentDt,
							UpdateBy = email,
							UpdateOn = currentDt,
						});
					}
					ProductStageGroup group = new ProductStageGroup()
					{
						ProductId = productGroup.ProductId
					};
					db.ProductStageGroups.Attach(group);
					db.Entry(group).Property(p => p.Status).IsModified = true;
					db.Entry(group).Property(p => p.UpdateBy).IsModified = true;
					db.Entry(group).Property(p => p.UpdateOn).IsModified = true;
					group.Status = Constant.STATUS_REMOVE;
					group.UpdateBy = email;
					group.UpdateOn = currentDt;
				}
				db.Configuration.ValidateOnSaveEnabled = false;
				Util.DeadlockRetry(db.SaveChanges, "ProductStage");
				return Request.CreateResponse(HttpStatusCode.OK);
			}
			catch (Exception e)
			{
				return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
			}
		}

		[Route("api/ProductStages/Tags")]
		[HttpPut]
		public HttpResponseMessage AppendTage(TagRequest request)
		{
			try
			{
				if (request == null)
				{
					throw new Exception("Invalid request");
				}
				var productIds = request.Products.Select(s => s.ProductId).ToList();
				var productList = db.ProductStageGroups.Where(w => true).Include(i => i.ProductStageTags);
				productList = productList.Where(w => productIds.Any(a => a == w.ProductId));
				if (User.ShopRequest() != null)
				{
					var shopId = User.ShopRequest().ShopId;
					productList = productList.Where(w => w.ShopId == shopId);
				}
				var tagList = request.Tags;
				var email = User.UserRequest().Email;
				var currentDt = SystemHelper.GetCurrentDateTime();
				foreach (var product in productList)
				{
					if (Constant.PRODUCT_STATUS_WAIT_FOR_APPROVAL.Equals(product.Status))
					{
						throw new Exception("Cannot add tag to Wait for Approval products");
					}
					foreach (var tag in tagList)
					{
						if (string.IsNullOrWhiteSpace(tag)
							|| product.ProductStageTags.Any(a => string.Equals(a.Tag, tag.Trim(), StringComparison.OrdinalIgnoreCase)))
						{
							continue;
						}
						product.ProductStageTags.Add(new ProductStageTag()
						{
							Tag = tag.Trim(),
							CreateBy = email,
							CreateOn = currentDt,
							UpdateBy = email,
							UpdateOn = currentDt,
						});
					}
					product.Status = Constant.PRODUCT_STATUS_DRAFT;
				}
				Util.DeadlockRetry(db.SaveChanges, "ProductStage");
				return Request.CreateResponse(HttpStatusCode.OK);
			}
			catch (Exception e)
			{
				return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
			}
		}

		[Route("api/ProductImages")]
		[HttpPost]
		public async Task<HttpResponseMessage> UploadFile()
		{
			try
			{
				var fileUpload = await Util.SetupImage(Request
					, AppSettingKey.IMAGE_ROOT_PATH
					, Path.Combine(AppSettingKey.PRODUCT_FOLDER, AppSettingKey.IMAGE_TMP_FOLDER)
					, 1500, 1500, 2000, 2000, 5, true);
				return Request.CreateResponse(HttpStatusCode.OK, fileUpload);
			}
			catch (Exception e)
			{
				return Request.CreateResponse(HttpStatusCode.NotAcceptable, e.Message);
			}
		}

		[Route("api/ProductStages/IgnoreApprove")]
		[HttpGet]
		public HttpResponseMessage GetIgnoreApprove()
		{
			var ignoreList = new List<string>()
			{
				"MinimumAllowedInCart",
				"MaximumAllowedInCart",
				"SalePrice",
				"Quantity",
				"PromotionPrice",
				"PromotionEffectiveDate",
				"PromotionExpireDate",
				"UnitPrice",
				"PurchasePrice",
				"SaleUnitTh",
				"SaleUnitEn",
				"SafetyStock",

			};
			return Request.CreateResponse(HttpStatusCode.OK, ignoreList);
		}


		private static volatile Dictionary<int, double> userExportProducts = new Dictionary<int, double>();

		private void Export(ExportRequest request, IPrincipal user)
		{
			var userId = user.UserRequest().UserId;
			StreamWriter writer = null;
			try
			{
				#region validation
				if (request == null)
				{
					throw new Exception("Invalid request");
				}
				#endregion
				using (ColspEntities db = new ColspEntities())
				{
					#region Setup Header
					Dictionary<string, Tuple<string, int>> headDicTmp = new Dictionary<string, Tuple<string, int>>();
					var tmpGuidance = db.ImportHeaders.Where(w => true);
					if (user.ShopRequest() != null)
					{
						var groupName = user.ShopRequest().ShopGroup;
						tmpGuidance = tmpGuidance.Where(w => w.ShopGroups.Any(a => a.Abbr.Equals(groupName)));
					}
					var guidance = tmpGuidance.Where(w => !w.MapName.Equals("ADM")).OrderBy(o => o.Position);
					int i = 0;
					foreach (var current in guidance)
					{
						var op = request.Options.Where(w => w.Equals(current.MapName)).SingleOrDefault();
						if (op == null)
						{
							continue;
						}
						if (!headDicTmp.ContainsKey(current.HeaderName))
						{
							headDicTmp.Add(current.MapName, new Tuple<string, int>(current.HeaderName, i++));
						}
					}
					//default attribute
					if (request.Options.Contains("ADM"))
					{
						List<string> defAttri = null;
						if (user.ShopRequest() != null)
						{
							defAttri = db.Attributes.Where(w => w.DefaultAttribute && Constant.ATTRIBUTE_VISIBLE_ALL_USER.Equals(w.VisibleTo)).Select(s => s.AttributeNameEn).ToList();
						}
						else
						{
							defAttri = db.Attributes.Where(w => w.DefaultAttribute).Select(s => s.AttributeNameEn).ToList();
						}
						if (defAttri != null && defAttri.Count > 0)
						{
							foreach (var attr in defAttri)
							{
								headDicTmp.Add(attr, new Tuple<string, int>(attr, i++));
							}
						}
					}
					#endregion
					#region Query
					var query = db.ProductStages.Where(w => !Constant.STATUS_REMOVE.Equals(w.Status))
						.Select(s => new
						{
							ProductStageGroup = s.ProductStageGroup == null ? null : new
							{
								Brand = s.ProductStageGroup.BrandId == null ? null : new
								{
									s.ProductStageGroup.Brand.BrandNameEn,
									s.ProductStageGroup.Brand.BrandId,
								},
								Tags = s.ProductStageGroup.ProductStageTags.Select(st => st.Tag),
								s.ProductStageGroup.GlobalCatId,
								s.ProductStageGroup.LocalCatId,
								ProductStageGlobalCatMaps = s.ProductStageGroup.ProductStageGlobalCatMaps.Select(sc => sc.CategoryId),
								ProductStageLocalCatMaps = s.ProductStageGroup.ProductStageLocalCatMaps.Select(sc => sc.CategoryId),
								ProductStageRelateds1 = s.ProductStageGroup.ProductStageRelateds1.Select(sp => sp.ProductStageGroup1.ProductStages.Where(w => w.IsVariant == false).Select(sv => sv.Pid)),
								s.ProductStageGroup.EffectiveDate,
								s.ProductStageGroup.ExpireDate,
								s.ProductStageGroup.Shipping.ShippingMethodEn,
								s.ExpressDelivery,
								s.DeliveryFee,
								s.NewArrivalDate,
								s.ProductStageGroup.IsBestSeller,
								s.ProductStageGroup.IsClearance,
								s.ProductStageGroup.IsNew,
								s.ProductStageGroup.IsOnlineExclusive,
								s.ProductStageGroup.IsOnlyAt,
								s.ProductStageGroup.Remark,
								s.ProductStageGroup.GiftWrap,
								AttributeSet = s.ProductStageGroup.AttributeSet == null ? null : new
								{
									s.ProductStageGroup.AttributeSet.AttributeSetId,
									s.ProductStageGroup.AttributeSet.AttributeSetNameEn,
									AttributeSetMaps = s.ProductStageGroup.AttributeSet.AttributeSetMaps.Select(sm => new
									{
										Attribute = sm.Attribute == null ? null : new
										{
											sm.Attribute.AttributeNameEn
										},
									}),
								},
							},
							s.Status,
							s.ProductId,
							s.DefaultVariant,
							s.Pid,
							s.ProductNameEn,
							s.ProductNameTh,
							s.ProdTDNameEn,
							s.ProdTDNameTh,
							s.Sku,
							s.Upc,
							s.OriginalPrice,
							s.PromotionPrice,
							s.EffectiveDatePromotion,
							s.ExpireDatePromotion,
							s.SalePrice,
							s.UnitPrice,
							s.PurchasePrice,
							s.SaleUnitEn,
							s.SaleUnitTh,
							s.IsVat,
							s.Installment,
							s.DescriptionFullEn,
							s.DescriptionFullTh,
							s.MobileDescriptionEn,
							s.MobileDescriptionTh,
							s.DescriptionShortEn,
							s.DescriptionShortTh,
							s.KillerPoint1En,
							s.KillerPoint1Th,
							s.KillerPoint2En,
							s.KillerPoint2Th,
							s.KillerPoint3En,
							s.KillerPoint3Th,
							s.IsHasExpiryDate,
							Inventory = s.Inventory == null ? null : new
							{
								s.Inventory.Quantity,
								s.Inventory.SafetyStockSeller,
								s.Inventory.StockType,
								s.Inventory.MinQtyAllowInCart,
								s.Inventory.MaxQtyAllowInCart,
								s.Inventory.MaxQtyPreOrder
							},
							s.PrepareDay,
							s.PrepareMon,
							s.PrepareTue,
							s.PrepareWed,
							s.PrepareThu,
							s.PrepareFri,
							s.PrepareSat,
							s.PrepareSun,
							s.Length,
							s.Width,
							s.Height,
							s.Weight,
							s.MetaDescriptionEn,
							s.MetaDescriptionTh,
							s.MetaKeyEn,
							s.MetaKeyTh,
							s.MetaTitleEn,
							s.MetaTitleTh,
							s.SeoEn,
							s.SeoTh,
							s.UrlKey,
							s.BoostWeight,
							s.GlobalBoostWeight,

							ProductStageAttributes = s.ProductStageAttributes.Select(ss => new
							{
								ss.IsAttributeValue,
								ss.CheckboxValue,
								ss.ValueEn,
								ss.ValueTh,
								ss.HtmlBoxValue,
								Attribute = ss.Attribute == null ? null : new
								{
									ss.Attribute.AttributeId,
									ss.Attribute.AttributeNameEn,
									DataType = ss.Attribute.DataType,
									DefaultAttribute = ss.Attribute.DefaultAttribute,
									AttributeValueMaps =
									ss.Attribute.AttributeValueMaps.Select(sv => new
									{
										AttributeValue = sv.AttributeValue == null ? null : new
										{
											sv.AttributeValue.AttributeValueEn,
											sv.AttributeValue.AttributeValueId,
											sv.AttributeValue.MapValue,
										}
									})
								}
							}),
							s.ShopId,
							s.IsVariant,
							s.VariantCount,
							s.Visibility,
						});
					var productIds = request.ProductList.Select(s => s.ProductId).ToList();
					if (productIds != null && productIds.Count > 0)
					{
						if (productIds.Count > 2000)
						{
							throw new Exception("Too many product selected");
						}
						query = query.Where(w => productIds.Contains(w.ProductId));
					}
					if (user.ShopRequest() != null)
					{
						var shopId = user.ShopRequest().ShopId;
						query = query.Where(w => w.ShopId == shopId);

						if (user.BrandRequest() != null)
						{
							var brands = user.BrandRequest().Select(s => s.BrandId).ToList();
							if (brands != null && brands.Count > 0)
							{
								query = query.Where(w => w.ProductStageGroup.Brand != null && brands.Contains(w.ProductStageGroup.Brand.BrandId));
							}
						}
					}
					#endregion
					#region initiate
					int take = 1000;
					int multiply = 0;
					int len = query.Count();
					List<List<string>> rs = new List<List<string>>();
					List<string> bodyList = null;
					if (request.AttributeSets != null && request.AttributeSets.Count > 0)
					{
						headDicTmp.Add("ADI", new Tuple<string, int>("Attribute Set", i++));
						headDicTmp.Add("ADJ", new Tuple<string, int>("Variation Option 1", i++));
						headDicTmp.Add("ADK", new Tuple<string, int>("Variation Option 2", i++));
					}
					List<ProductStageAttribute> masterAttribute = null;
					List<ProductStageAttribute> defaultAttribute = null;
					int iter = 0;
					#endregion
					while (true)
					{
						var productList = query
							.OrderBy(o => o.ProductId)
							.ThenBy(o => o.IsVariant)
							.Skip(take * multiply).Take(take);
						if (productList.Count() == 0)
						{
							break;
						}
						multiply++;
						foreach (var p in productList)
						{
							iter++;
							#region Setup Attribute
							if (p.IsVariant == false)
							{
								masterAttribute = new List<ProductStageAttribute>();
								foreach (var stageAttr in p.ProductStageAttributes.Where(w => !w.Attribute.DefaultAttribute))
								{
									var tmpAttribute = new ProductStageAttribute();
									tmpAttribute.ValueEn = stageAttr.ValueEn;
									tmpAttribute.ValueTh = stageAttr.ValueTh;
									tmpAttribute.CheckboxValue = stageAttr.CheckboxValue;
									tmpAttribute.HtmlBoxValue = stageAttr.HtmlBoxValue;
									if (stageAttr.Attribute != null)
									{
										tmpAttribute.Attribute = new Entity.Models.Attribute()
										{
											AttributeId = stageAttr.Attribute.AttributeId,
											AttributeNameEn = stageAttr.Attribute.AttributeNameEn,
											DataType = stageAttr.Attribute.DataType,
											DefaultAttribute = stageAttr.Attribute.DefaultAttribute,
										};
										if (stageAttr.Attribute.AttributeValueMaps != null)
										{
											foreach (var stageVal in stageAttr.Attribute.AttributeValueMaps)
											{
												tmpAttribute.Attribute.AttributeValueMaps.Add(new AttributeValueMap()
												{
													AttributeId = tmpAttribute.AttributeId,
													AttributeValueId = stageVal.AttributeValue.AttributeValueId,
													AttributeValue = new AttributeValue()
													{
														AttributeValueId = stageVal.AttributeValue.AttributeValueId,
														AttributeValueEn = stageVal.AttributeValue.AttributeValueEn,
														MapValue = stageVal.AttributeValue.MapValue,
													}
												});
											}
										}
									}
									masterAttribute.Add(tmpAttribute);
								}

								defaultAttribute = new List<ProductStageAttribute>();
								foreach (var stageAttr in p.ProductStageAttributes.Where(w => w.Attribute.DefaultAttribute))
								{
									var tmpAttribute = new ProductStageAttribute();
									tmpAttribute.ValueEn = stageAttr.ValueEn;
									tmpAttribute.ValueTh = stageAttr.ValueTh;
									tmpAttribute.CheckboxValue = stageAttr.CheckboxValue;
									tmpAttribute.HtmlBoxValue = stageAttr.HtmlBoxValue;
									if (stageAttr.Attribute != null)
									{
										tmpAttribute.Attribute = new Entity.Models.Attribute()
										{
											AttributeId = stageAttr.Attribute.AttributeId,
											AttributeNameEn = stageAttr.Attribute.AttributeNameEn,
											DataType = stageAttr.Attribute.DataType,
											DefaultAttribute = stageAttr.Attribute.DefaultAttribute,
										};
										if (stageAttr.Attribute.AttributeValueMaps != null)
										{
											foreach (var stageVal in stageAttr.Attribute.AttributeValueMaps)
											{
												tmpAttribute.Attribute.AttributeValueMaps.Add(new AttributeValueMap()
												{
													AttributeId = tmpAttribute.AttributeId,
													AttributeValueId = stageVal.AttributeValue.AttributeValueId,
													AttributeValue = new AttributeValue()
													{
														AttributeValueId = stageVal.AttributeValue.AttributeValueId,
														AttributeValueEn = stageVal.AttributeValue.AttributeValueEn,
														MapValue = stageVal.AttributeValue.MapValue,
													}
												});
											}
										}
									}
									defaultAttribute.Add(tmpAttribute);
								}
								if (p.VariantCount > 0)
								{
									continue;
								}
								//masterAttribute = p.ProductStageAttributes.Where(w=>!w.Attribute.DefaultAttribute).ToList();
								//defaultAttribute = p.ProductStageAttributes.Where(w => w.Attribute.DefaultAttribute).ToList();
							}
							#endregion
							bodyList = new List<string>(new string[headDicTmp.Count]);
							#region System Information
							if (headDicTmp.ContainsKey("AAA"))
							{
								if (Constant.PRODUCT_STATUS_DRAFT.Equals(p.Status))
								{
									bodyList[headDicTmp["AAA"].Item2] = "Draft";
								}
								else if (Constant.PRODUCT_STATUS_WAIT_FOR_APPROVAL.Equals(p.Status))
								{
									bodyList[headDicTmp["AAA"].Item2] = "Wait for Approval";
								}
								else if (Constant.PRODUCT_STATUS_APPROVE.Equals(p.Status))
								{
									bodyList[headDicTmp["AAA"].Item2] = "Approve";
								}
								else if (Constant.PRODUCT_STATUS_NOT_APPROVE.Equals(p.Status))
								{
									bodyList[headDicTmp["AAA"].Item2] = "Not Approve";
								}
							}
							if (headDicTmp.ContainsKey("AAB"))
							{
								bodyList[headDicTmp["AAB"].Item2] = string.Concat(p.ProductId);
							}
							if (headDicTmp.ContainsKey("AAC"))
							{
								bodyList[headDicTmp["AAC"].Item2] = p.DefaultVariant == true ? "Yes" : "No";
							}
							if (headDicTmp.ContainsKey("AAD"))
							{
								bodyList[headDicTmp["AAD"].Item2] = p.Pid;
							}
							#endregion
							#region Vital Infromation
							if (headDicTmp.ContainsKey("ADL"))
							{
								bodyList[headDicTmp["ADL"].Item2] = p.Visibility == true ? "Yes" : "No";
							}
							if (headDicTmp.ContainsKey("AAE"))
							{
								bodyList[headDicTmp["AAE"].Item2] = p.ProductNameEn;
							}
							if (headDicTmp.ContainsKey("AAF"))
							{
								bodyList[headDicTmp["AAF"].Item2] = p.ProductNameTh;
							}

							if (headDicTmp.ContainsKey("AAG"))
							{
								bodyList[headDicTmp["AAG"].Item2] = p.ProdTDNameEn;
							}
							if (headDicTmp.ContainsKey("AAH"))
							{
								bodyList[headDicTmp["AAH"].Item2] = p.ProdTDNameTh;
							}
							if (headDicTmp.ContainsKey("AAI"))
							{
								bodyList[headDicTmp["AAI"].Item2] = p.Sku;
							}

							if (headDicTmp.ContainsKey("AAJ"))
							{
								bodyList[headDicTmp["AAJ"].Item2] = p.Upc;
							}
							if (headDicTmp.ContainsKey("AAK"))
							{
								if (p.ProductStageGroup.Brand != null && !string.IsNullOrEmpty(p.ProductStageGroup.Brand.BrandNameEn))
								{
									bodyList[headDicTmp["AAK"].Item2] = p.ProductStageGroup.Brand.BrandNameEn;
								}
							}
							#endregion
							#region Price
							if (headDicTmp.ContainsKey("AAL"))
							{
								bodyList[headDicTmp["AAL"].Item2] = string.Concat(p.SalePrice);
							}
							if (headDicTmp.ContainsKey("AAM"))
							{
								bodyList[headDicTmp["AAM"].Item2] = string.Concat(p.OriginalPrice);
							}
							if (headDicTmp.ContainsKey("AAN"))
							{
								bodyList[headDicTmp["AAN"].Item2] = Constant.STATUS_YES.Equals(p.Installment) ? "Yes" : "No";
							}
							if (headDicTmp.ContainsKey("AAO"))
							{
								bodyList[headDicTmp["AAO"].Item2] = string.Concat(p.PromotionPrice);
							}
							if (headDicTmp.ContainsKey("AAP"))
							{
								if (p.EffectiveDatePromotion != null)
								{
									bodyList[headDicTmp["AAP"].Item2] = p.EffectiveDatePromotion.Value.ToString(Constant.DATETIME_FORMAT);
								}
							}
							if (headDicTmp.ContainsKey("AAQ"))
							{
								if (p.ExpireDatePromotion != null)
								{
									bodyList[headDicTmp["AAQ"].Item2] = p.ExpireDatePromotion.Value.ToString(Constant.DATETIME_FORMAT);
								}
							}
							if (headDicTmp.ContainsKey("AAR"))
							{
								bodyList[headDicTmp["AAR"].Item2] = string.Concat(p.UnitPrice);
							}
							if (headDicTmp.ContainsKey("AAS"))
							{
								bodyList[headDicTmp["AAS"].Item2] = string.Concat(p.PurchasePrice);
							}
							if (headDicTmp.ContainsKey("AAT"))
							{
								bodyList[headDicTmp["AAT"].Item2] = p.SaleUnitEn;
							}
							if (headDicTmp.ContainsKey("AAU"))
							{
								bodyList[headDicTmp["AAU"].Item2] = p.SaleUnitTh;
							}
							if (headDicTmp.ContainsKey("AAV"))
							{
								bodyList[headDicTmp["AAV"].Item2] = Constant.STATUS_YES.Equals(p.IsVat) ? "Yes" : "No";
							}

							#endregion
							#region Description
							if (headDicTmp.ContainsKey("AAW"))
							{
								bodyList[headDicTmp["AAW"].Item2] = p.DescriptionFullEn;
							}
							if (headDicTmp.ContainsKey("AAX"))
							{
								bodyList[headDicTmp["AAX"].Item2] = p.DescriptionFullTh;
							}
							if (headDicTmp.ContainsKey("AAY"))
							{
								bodyList[headDicTmp["AAY"].Item2] = p.MobileDescriptionEn;
							}
							if (headDicTmp.ContainsKey("AAZ"))
							{
								bodyList[headDicTmp["AAZ"].Item2] = p.MobileDescriptionTh;
							}
							if (headDicTmp.ContainsKey("ABA"))
							{
								bodyList[headDicTmp["ABA"].Item2] = p.DescriptionShortEn;
							}
							if (headDicTmp.ContainsKey("ABB"))
							{
								bodyList[headDicTmp["ABB"].Item2] = p.DescriptionShortTh;
							}
							if (headDicTmp.ContainsKey("ABC"))
							{
								bodyList[headDicTmp["ABC"].Item2] = p.KillerPoint1En;
							}
							if (headDicTmp.ContainsKey("ABD"))
							{
								bodyList[headDicTmp["ABD"].Item2] = p.KillerPoint1Th;
							}
							if (headDicTmp.ContainsKey("ABE"))
							{
								bodyList[headDicTmp["ABE"].Item2] = p.KillerPoint2En;
							}
							if (headDicTmp.ContainsKey("ABF"))
							{
								bodyList[headDicTmp["ABF"].Item2] = p.KillerPoint2Th;
							}
							if (headDicTmp.ContainsKey("ABG"))
							{
								bodyList[headDicTmp["ABG"].Item2] = p.KillerPoint3En;
							}
							if (headDicTmp.ContainsKey("ABH"))
							{
								bodyList[headDicTmp["ABH"].Item2] = p.KillerPoint3Th;
							}
							#endregion
							#region Search Tags
							if (headDicTmp.ContainsKey("ABI"))
							{
								if (p.ProductStageGroup.Tags != null && p.ProductStageGroup.Tags.ToList().Count > 0)
								{
									bodyList[headDicTmp["ABI"].Item2] = string.Join(",", p.ProductStageGroup.Tags);
								}
							}
							#endregion
							#region Inventory
							if (headDicTmp.ContainsKey("ABJ"))
							{
								if (p.Inventory != null)
								{
									bodyList[headDicTmp["ABJ"].Item2] = string.Concat(p.Inventory.Quantity);
								}
							}
							if (headDicTmp.ContainsKey("ABL"))
							{
								if (p.Inventory != null)
								{
									bodyList[headDicTmp["ABL"].Item2] = string.Concat(p.Inventory.SafetyStockSeller);
								}
							}
							if (headDicTmp.ContainsKey("ABM"))
							{
								if (p.Inventory != null)
								{
									bodyList[headDicTmp["ABM"].Item2] = string.Concat(p.Inventory.MinQtyAllowInCart);
								}
							}
							if (headDicTmp.ContainsKey("ABN"))
							{
								if (p.Inventory != null)
								{
									bodyList[headDicTmp["ABN"].Item2] = string.Concat(p.Inventory.MaxQtyAllowInCart);
								}
							}
							if (headDicTmp.ContainsKey("ABO"))
							{
								if (p.Inventory != null)
								{
									bodyList[headDicTmp["ABO"].Item2] = Constant.STOCK_TYPE.Where(w => w.Value.Equals(p.Inventory.StockType)).SingleOrDefault().Key;
								}
							}
							if (headDicTmp.ContainsKey("ABP"))
							{
								if (p.Inventory != null)
								{
									bodyList[headDicTmp["ABP"].Item2] = string.Concat(p.Inventory.MaxQtyPreOrder);
								}
							}
							if (headDicTmp.ContainsKey("ABQ"))
							{
								bodyList[headDicTmp["ABQ"].Item2] = Constant.STATUS_YES.Equals(p.IsHasExpiryDate) ? "Yes" : "No";
							}
							#endregion
							#region Shipping Detail
							if (headDicTmp.ContainsKey("ABR"))
							{
								bodyList[headDicTmp["ABR"].Item2] = p.ProductStageGroup.ShippingMethodEn;
							}
							if (headDicTmp.ContainsKey("ABS"))
							{
								bodyList[headDicTmp["ABS"].Item2] = Constant.STATUS_YES.Equals(p.ProductStageGroup.ExpressDelivery) ? "Yes" : "No";
							}
							if (headDicTmp.ContainsKey("ABT"))
							{
								bodyList[headDicTmp["ABT"].Item2] = string.Concat(p.ProductStageGroup.DeliveryFee);
							}

							if (headDicTmp.ContainsKey("ABU"))
							{
								bodyList[headDicTmp["ABU"].Item2] = string.Concat(p.PrepareDay);
							}
							if (headDicTmp.ContainsKey("ABV"))
							{
								bodyList[headDicTmp["ABV"].Item2] = string.Concat(p.PrepareMon);
							}
							if (headDicTmp.ContainsKey("ABW"))
							{
								bodyList[headDicTmp["ABW"].Item2] = string.Concat(p.PrepareTue);
							}
							if (headDicTmp.ContainsKey("ABX"))
							{
								bodyList[headDicTmp["ABX"].Item2] = string.Concat(p.PrepareWed);
							}
							if (headDicTmp.ContainsKey("ABY"))
							{
								bodyList[headDicTmp["ABY"].Item2] = string.Concat(p.PrepareThu);
							}
							if (headDicTmp.ContainsKey("ABZ"))
							{
								bodyList[headDicTmp["ABZ"].Item2] = string.Concat(p.PrepareFri);
							}
							if (headDicTmp.ContainsKey("ACA"))
							{
								bodyList[headDicTmp["ACA"].Item2] = string.Concat(p.PrepareSat);
							}
							if (headDicTmp.ContainsKey("ACB"))
							{
								bodyList[headDicTmp["ACB"].Item2] = string.Concat(p.PrepareSun);
							}
							if (headDicTmp.ContainsKey("ACC"))
							{
								bodyList[headDicTmp["ACC"].Item2] = string.Concat(p.Length);
							}
							if (headDicTmp.ContainsKey("ACD"))
							{
								bodyList[headDicTmp["ACD"].Item2] = string.Concat(p.Height);
							}
							if (headDicTmp.ContainsKey("ACE"))
							{
								bodyList[headDicTmp["ACE"].Item2] = string.Concat(p.Width);
							}
							if (headDicTmp.ContainsKey("ACF"))
							{
								bodyList[headDicTmp["ACF"].Item2] = string.Concat(p.Weight);
							}
							#endregion
							#region Category
							if (headDicTmp.ContainsKey("ACG"))
							{
								bodyList[headDicTmp["ACG"].Item2] = string.Concat(p.ProductStageGroup.GlobalCatId);
							}
							if (headDicTmp.ContainsKey("ACH"))
							{
								if (p.ProductStageGroup.ProductStageGlobalCatMaps != null && p.ProductStageGroup.ProductStageGlobalCatMaps.ToList().Count > 0)
								{
									bodyList[headDicTmp["ACH"].Item2] = string.Concat(p.ProductStageGroup.ProductStageGlobalCatMaps.ToList()[0]);
								}
							}
							if (headDicTmp.ContainsKey("ACI"))
							{
								if (p.ProductStageGroup.ProductStageGlobalCatMaps != null && p.ProductStageGroup.ProductStageGlobalCatMaps.ToList().Count > 1)
								{
									bodyList[headDicTmp["ACI"].Item2] = string.Concat(p.ProductStageGroup.ProductStageGlobalCatMaps.ToList()[1]);
								}
							}
							if (headDicTmp.ContainsKey("ACJ"))
							{
								bodyList[headDicTmp["ACJ"].Item2] = string.Concat(p.ProductStageGroup.LocalCatId);
							}
							if (headDicTmp.ContainsKey("ACK"))
							{
								if (p.ProductStageGroup.ProductStageLocalCatMaps != null && p.ProductStageGroup.ProductStageLocalCatMaps.ToList().Count > 0)
								{
									bodyList[headDicTmp["ACK"].Item2] = string.Concat(p.ProductStageGroup.ProductStageLocalCatMaps.ToList()[0]);
								}
							}
							if (headDicTmp.ContainsKey("ACL"))
							{
								if (p.ProductStageGroup.ProductStageLocalCatMaps != null && p.ProductStageGroup.ProductStageLocalCatMaps.ToList().Count > 1)
								{
									bodyList[headDicTmp["ACL"].Item2] = string.Concat(p.ProductStageGroup.ProductStageLocalCatMaps.ToList()[1]);
								}
							}
							#endregion
							#region Relationship
							if (headDicTmp.ContainsKey("ACM"))
							{
								if (p.ProductStageGroup.ProductStageRelateds1 != null && p.ProductStageGroup.ProductStageRelateds1.ToList().Count > 0)
								{
									var pids = p.ProductStageGroup.ProductStageRelateds1.SelectMany(s => s);
									bodyList[headDicTmp["ACM"].Item2] = string.Join(",", pids);
								}
							}
							#endregion
							#region SEO
							if (headDicTmp.ContainsKey("ACN"))
							{
								bodyList[headDicTmp["ACN"].Item2] = p.SeoEn;
							}
							if (headDicTmp.ContainsKey("ACO"))
							{
								bodyList[headDicTmp["ACO"].Item2] = p.SeoTh;
							}

							if (headDicTmp.ContainsKey("ACP"))
							{
								bodyList[headDicTmp["ACP"].Item2] = p.MetaTitleEn;
							}
							if (headDicTmp.ContainsKey("ACQ"))
							{
								bodyList[headDicTmp["ACQ"].Item2] = p.MetaTitleTh;
							}
							if (headDicTmp.ContainsKey("ACR"))
							{
								bodyList[headDicTmp["ACR"].Item2] = p.MetaDescriptionEn;
							}
							if (headDicTmp.ContainsKey("ACS"))
							{
								bodyList[headDicTmp["ACS"].Item2] = p.MetaDescriptionTh;
							}
							if (headDicTmp.ContainsKey("ACT"))
							{
								bodyList[headDicTmp["ACT"].Item2] = p.MetaKeyEn;
							}
							if (headDicTmp.ContainsKey("ACU"))
							{
								bodyList[headDicTmp["ACU"].Item2] = p.MetaKeyTh;
							}
							if (headDicTmp.ContainsKey("ACV"))
							{
								bodyList[headDicTmp["ACV"].Item2] = p.UrlKey;
							}
							if (headDicTmp.ContainsKey("ACW"))
							{
								bodyList[headDicTmp["ACW"].Item2] = string.Concat(p.BoostWeight);
							}
							if (headDicTmp.ContainsKey("ACX"))
							{
								bodyList[headDicTmp["ACX"].Item2] = string.Concat(p.GlobalBoostWeight);
							}
							#endregion
							#region More Detail
							if (headDicTmp.ContainsKey("ACY"))
							{
								if (p.ProductStageGroup.EffectiveDate != null)
								{
									bodyList[headDicTmp["ACY"].Item2] = p.ProductStageGroup.EffectiveDate.ToString(Constant.DATETIME_FORMAT);
								}
							}
							if (headDicTmp.ContainsKey("ACZ"))
							{
								if (p.ProductStageGroup.ExpireDate != null)
								{
									bodyList[headDicTmp["ACZ"].Item2] = p.ProductStageGroup.ExpireDate.ToString(Constant.DATETIME_FORMAT);
								}
							}
							if (headDicTmp.ContainsKey("ADA"))
							{
								bodyList[headDicTmp["ADA"].Item2] = Constant.STATUS_YES.Equals(p.ProductStageGroup.GiftWrap) ? "Yes" : "No";
							}
							if (headDicTmp.ContainsKey("ADB"))
							{
								if (p.ProductStageGroup.NewArrivalDate != null)
								{
									bodyList[headDicTmp["ADB"].Item2] = p.ProductStageGroup.NewArrivalDate.Value.ToString(Constant.DATETIME_FORMAT);
								}
							}
							if (headDicTmp.ContainsKey("ADC"))
							{
								bodyList[headDicTmp["ADC"].Item2] = p.ProductStageGroup.IsNew == true ? "Yes" : "No";
							}

							if (headDicTmp.ContainsKey("ADD"))
							{
								bodyList[headDicTmp["ADD"].Item2] = p.ProductStageGroup.IsClearance == true ? "Yes" : "No";
							}
							if (headDicTmp.ContainsKey("ADE"))
							{
								bodyList[headDicTmp["ADE"].Item2] = p.ProductStageGroup.IsBestSeller == true ? "Yes" : "No";
							}

							if (headDicTmp.ContainsKey("ADF"))
							{
								bodyList[headDicTmp["ADF"].Item2] = p.ProductStageGroup.IsOnlineExclusive == true ? "Yes" : "No";
							}
							if (headDicTmp.ContainsKey("ADG"))
							{
								bodyList[headDicTmp["ADG"].Item2] = p.ProductStageGroup.IsOnlyAt == true ? "Yes" : "No";
							}
							if (headDicTmp.ContainsKey("ADH"))
							{
								bodyList[headDicTmp["ADH"].Item2] = p.ProductStageGroup.Remark;
							}
							#endregion
							#region Attibute Section
							if (request.AttributeSets != null && request.AttributeSets.Count > 0)
							{
								if (p.ProductStageGroup.AttributeSet != null)
								{
									var set = request.AttributeSets.Where(w => w.AttributeSetId == p.ProductStageGroup.AttributeSet.AttributeSetId).SingleOrDefault();
									if (set != null)
									{
										//make header for attribute
										foreach (var attr in p.ProductStageGroup.AttributeSet.AttributeSetMaps.Select(s => s.Attribute))
										{
											if (!headDicTmp.ContainsKey(attr.AttributeNameEn))
											{
												headDicTmp.Add(attr.AttributeNameEn, new Tuple<string, int>(attr.AttributeNameEn, i++));
												bodyList.Add(string.Empty);
											}
										}

										bodyList[headDicTmp["ADI"].Item2] = p.ProductStageGroup.AttributeSet.AttributeSetNameEn;
										//make vaiant option 1 value
										if (p.IsVariant && p.ProductStageAttributes != null && p.ProductStageAttributes.ToList().Count > 0)
										{
											bodyList[headDicTmp["ADJ"].Item2] = p.ProductStageAttributes.ToList()[0].Attribute.AttributeNameEn;
										}
										//make vaiant option 2 value
										if (p.IsVariant && p.ProductStageAttributes != null && p.ProductStageAttributes.ToList().Count > 1)
										{
											bodyList[headDicTmp["ADK"].Item2] = p.ProductStageAttributes.ToList()[1].Attribute.AttributeNameEn;
										}
										//make master attribute value
										if (!p.IsVariant && masterAttribute != null && masterAttribute.ToList().Count > 0)
										{
											foreach (var masterValue in masterAttribute)
											{

												if (headDicTmp.ContainsKey(masterValue.Attribute.AttributeNameEn))
												{
													int desColumn = headDicTmp[masterValue.Attribute.AttributeNameEn].Item2;
													for (int j = bodyList.Count; j <= desColumn; j++)
													{
														bodyList.Add(string.Empty);
													}

													if (Constant.DATA_TYPE_LIST.Equals(masterValue.Attribute.DataType))
													{
														var tmpValue = masterValue.Attribute.AttributeValueMaps.Where(w => w.AttributeValue.MapValue == masterValue.ValueEn).Select(s => s.AttributeValue.AttributeValueEn).SingleOrDefault();
														bodyList[desColumn] = tmpValue;
													}
													else if (Constant.DATA_TYPE_CHECKBOX.Equals(masterValue.Attribute.DataType))
													{
														var tmpValue = masterValue.Attribute.AttributeValueMaps.Where(w => w.AttributeValue.MapValue == masterValue.ValueEn).Select(s => s.AttributeValue.AttributeValueEn).SingleOrDefault();
														if (masterValue.CheckboxValue)
														{
															if (string.IsNullOrWhiteSpace(bodyList[desColumn]))
															{
																bodyList[desColumn] = tmpValue;
															}
															else
															{
																bodyList[desColumn] = string.Concat(bodyList[desColumn], ",", tmpValue);
															}
														}
													}
													else if (!string.IsNullOrWhiteSpace(masterValue.ValueEn))
													{
														bodyList[desColumn] = string.Concat(masterValue.ValueEn, ";", masterValue.ValueTh);
														if (Constant.DATA_TYPE_HTML.Equals(masterValue.Attribute.DataType))
														{
															bodyList[desColumn] = masterValue.HtmlBoxValue;
														}
													}
												}
											}
										}
										if (p.IsVariant && p.ProductStageAttributes != null && p.ProductStageAttributes.ToList().Count > 0)
										{
											foreach (var variantValue in p.ProductStageAttributes)
											{
												if (headDicTmp.ContainsKey(variantValue.Attribute.AttributeNameEn))
												{
													int desColumn = headDicTmp[variantValue.Attribute.AttributeNameEn].Item2;
													for (int j = bodyList.Count; j <= desColumn; j++)
													{
														bodyList.Add(string.Empty);
													}

													if (Constant.DATA_TYPE_LIST.Equals(variantValue.Attribute.DataType))
													{
														var tmpValue = variantValue.Attribute.AttributeValueMaps.Where(w => w.AttributeValue.MapValue == variantValue.ValueEn).Select(s => s.AttributeValue.AttributeValueEn).SingleOrDefault();
														bodyList[desColumn] = tmpValue;
													}
													else if (Constant.DATA_TYPE_CHECKBOX.Equals(variantValue.Attribute.DataType))
													{
														var tmpValue = variantValue.Attribute.AttributeValueMaps.Where(w => w.AttributeValue.MapValue == variantValue.ValueEn).Select(s => s.AttributeValue.AttributeValueEn).SingleOrDefault();
														if (variantValue.CheckboxValue)
														{
															if (string.IsNullOrWhiteSpace(bodyList[desColumn]))
															{
																bodyList[desColumn] = tmpValue;
															}
															else
															{
																bodyList[desColumn] = string.Concat(bodyList[desColumn], ",", tmpValue);
															}
														}
													}
													else if (!string.IsNullOrWhiteSpace(variantValue.ValueEn))
													{
														bodyList[desColumn] = string.Concat(variantValue.ValueEn, ";", variantValue.ValueTh);
														if (Constant.DATA_TYPE_HTML.Equals(variantValue.Attribute.DataType))
														{
															bodyList[desColumn] = variantValue.HtmlBoxValue;
														}
													}
												}
											}
										}
									}
								}
							}
							#endregion
							#region Default Attribute
							//make default attribute value
							if (defaultAttribute != null && defaultAttribute.ToList().Count > 0)
							{
								foreach (var defaultValue in defaultAttribute)
								{
									//avoid admin attribute
									if (!headDicTmp.ContainsKey(defaultValue.Attribute.AttributeNameEn))
									{
										continue;
									}
									int desColumn = headDicTmp[defaultValue.Attribute.AttributeNameEn].Item2;
									for (int j = bodyList.Count; j <= desColumn; j++)
									{
										bodyList.Add(string.Empty);
									}
									if (Constant.DATA_TYPE_LIST.Equals(defaultValue.Attribute.DataType))
									{
										var tmpValue = defaultValue.Attribute.AttributeValueMaps.Where(w => w.AttributeValue.MapValue == defaultValue.ValueEn).Select(s => s.AttributeValue.AttributeValueEn).SingleOrDefault();
										bodyList[desColumn] = tmpValue;
									}
									else if (Constant.DATA_TYPE_CHECKBOX.Equals(defaultValue.Attribute.DataType))
									{
										var tmpValue = defaultValue.Attribute.AttributeValueMaps.Where(w => w.AttributeValue.MapValue == defaultValue.ValueEn).Select(s => s.AttributeValue.AttributeValueEn).SingleOrDefault();
										if (defaultValue.CheckboxValue)
										{
											if (string.IsNullOrWhiteSpace(bodyList[desColumn]))
											{
												bodyList[desColumn] = tmpValue;
											}
											else
											{
												bodyList[desColumn] = string.Concat(bodyList[desColumn], ",", tmpValue);
											}
										}
									}
									else if (!string.IsNullOrWhiteSpace(defaultValue.ValueEn))
									{
										bodyList[desColumn] = string.Concat(defaultValue.ValueEn, ";", defaultValue.ValueTh);
										if (Constant.DATA_TYPE_HTML.Equals(defaultValue.Attribute.DataType))
										{
											bodyList[desColumn] = defaultValue.HtmlBoxValue;
										}
									}
								}
							}
							#endregion
							#region Update progress
							if (userExportProducts.ContainsKey(userId))
							{
								userExportProducts[userId] = (iter * 100.0) / len;
							}
							else
							{
								return;
							}
							#endregion
							rs.Add(bodyList);
						}
						Thread.Sleep(500);
					}
					#region Write
					string filePath = Path.Combine(AppSettingKey.EXPORT_ROOT_PATH, string.Concat(userId));
					if (File.Exists(filePath))
					{
						File.Delete(filePath);
					}
					using (writer = new StreamWriter(filePath, false, Encoding.UTF8))
					{
						var csv = new CsvWriter(writer);
						string headers = string.Empty;
						foreach (KeyValuePair<string, Tuple<string, int>> entry in headDicTmp)
						{
							csv.WriteField(entry.Value.Item1);
						}
						csv.NextRecord();
						#region Write body
						foreach (List<string> r in rs)
						{
							foreach (string field in r)
							{
								csv.WriteField(field);
							}
							csv.NextRecord();
						}
						#endregion
						writer.Flush();
					}
					#endregion
				}
			}
			catch (Exception e)
			{
				if (userExportProducts.ContainsKey(userId))
				{
					userExportProducts[userId] = -1.0;
				}
			}
			finally
			{
				#region close writer
				if (writer != null)
				{
					writer.Close();
					writer.Dispose();
				}
				#endregion
			}
		}

		[Route("api/ProductStages/Export")]
		[HttpPost]
		public HttpResponseMessage ExportProductProducts(ExportRequest request)
		{
			var userId = User.UserRequest().UserId;
			//Put progress to global dict
			if (!userExportProducts.ContainsKey(userId))
			{
				userExportProducts.Add(userId, 0.0);
			}
			else
			{
				userExportProducts[userId] = 0.0;
			}
			Task.Run(() =>
			{
				Export(request, User);
			});
			return Request.CreateResponse(HttpStatusCode.OK);
		}

		[Route("api/ProductStages/Export/Abort")]
		[HttpPost]
		public HttpResponseMessage ExportProductAbort()
		{
			//Check if userId exist in dictionary
			if (userExportProducts.ContainsKey(User.UserRequest().UserId))
			{
				userExportProducts.Remove(User.UserRequest().UserId);
				return Request.CreateResponse(HttpStatusCode.OK);
			}
			else
			{
				return Request.CreateResponse(HttpStatusCode.NotAcceptable);
			}
		}

		[Route("api/ProductStages/Export/Progress")]
		[HttpGet]
		public HttpResponseMessage ExportProductProgress()
		{
			//Check if userId exist in dictionary
			if (userExportProducts.ContainsKey(User.UserRequest().UserId))
			{
				return Request.CreateResponse(HttpStatusCode.OK, userExportProducts[User.UserRequest().UserId]);
			}
			else
			{
				return Request.CreateResponse(HttpStatusCode.NotAcceptable);
			}
		}

		[Route("api/ProductStages/Export")]
		[HttpGet]
		public HttpResponseMessage ExportProductGet()
		{
			try
			{
				HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);
				var currentDt = SystemHelper.GetCurrentDateTime();
				int userId = User.UserRequest().UserId;
				var stream = new FileStream(Path.Combine(AppSettingKey.EXPORT_ROOT_PATH, string.Concat(userId)), FileMode.Open);
				result.Content = new StreamContent(stream);
				result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream")
				{
					CharSet = Encoding.UTF8.WebName
				};
				result.Headers.Add("Cache-Control", "no-cache");
				result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
				result.Content.Headers.ContentDisposition.FileName = string.Concat("ProductExport",currentDt.ToString("yyyy-MM-dd"),".csv");
				return result;
			}
			catch (Exception e)
			{
				return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.GetBaseException());
			}

		}



		[Route("api/ProductStages/Import")]
		[HttpPost]
		public async Task<HttpResponseMessage> ImportProduct()
		{
			string fileName = string.Empty;
			HashSet<string> errorMessage = new HashSet<string>();
			int row = 2;
			try
			{
				#region Validate Request
				if (!Request.Content.IsMimeMultipartContent())
				{
					throw new Exception("Content Multimedia");
				}
				string root = HttpContext.Current.Server.MapPath("~/Import");
				var streamProvider = new MultipartFormDataStreamProvider(root);
				await Request.Content.ReadAsMultipartAsync(streamProvider);

				if (streamProvider.FileData == null || streamProvider.FileData.Count == 0)
				{
					throw new Exception("No file uploaded");
				}
				fileName = streamProvider.FileData[0].LocalFileName;
				#endregion
				var email = User.UserRequest().Email;
				var currentDt = SystemHelper.GetCurrentDateTime();
				Dictionary<string, ProductStageGroup> groupList = SetupImport(fileName, errorMessage, row,email,currentDt, db);
				#region Validate Error Message
				var allUrl = groupList.SelectMany(s => s.Value.ProductStages.Select(su => su.UrlKey));
				var urlCount = db.ProductStages.Where(w => allUrl.Contains(w.UrlKey)).Select(s => s.UrlKey).Distinct();
				if (urlCount.Count() != 0)
				{
					errorMessage.Add(string.Concat("Url already in used ", string.Join(",", urlCount)));
				}
				if (errorMessage.Count > 0)
				{
					return Request.CreateResponse(HttpStatusCode.NotAcceptable, errorMessage.ToList());
				}
				#endregion
				#region Setup Product for database
				var requireDefaultAttr = db.Attributes.Where(w => w.Required && !Constant.DATA_TYPE_CHECKBOX.Equals(w.DataType) && w.DefaultAttribute).Select(s => s.AttributeId);
				foreach (var product in groupList)
				{
					
					var masterVariant = product.Value.ProductStages.Where(w => w.IsVariant == false).SingleOrDefault();
					#region Validate flag
					if (!string.IsNullOrWhiteSpace(masterVariant.ProductNameEn)
					   && !string.IsNullOrWhiteSpace(masterVariant.ProductNameTh)
					   && product.Value.BrandId != null)
					{
						product.Value.InfoFlag = true;
						var attrIds = masterVariant.ProductStageAttributes.Where(w => !string.IsNullOrWhiteSpace(w.ValueEn)).Select(s => s.AttributeId);
						foreach (var id in requireDefaultAttr)
						{
							if (!attrIds.Contains(id))
							{
								product.Value.InfoFlag = false;
								break;
							}
						}
						if (product.Value.AttributeSetId.HasValue && product.Value.InfoFlag)
						{
							var masterAttr = db.Attributes
								.Where(w => w.Required 
									&& !Constant.DATA_TYPE_CHECKBOX.Equals(w.DataType) 
									&& w.AttributeSetMaps.Any(a => a.AttributeSetId == product.Value.AttributeSetId.Value))
								.Select(s => s.AttributeId);
							foreach (var id in masterAttr)
							{
								if (!masterAttr.Contains(id))
								{
									product.Value.InfoFlag = false;
									break;
								}
							}
						}
					}
					else
					{
						product.Value.InfoFlag = false;
					}
					#endregion
					masterVariant.VariantCount = product.Value.ProductStages.Where(w => w.IsVariant == true).Count();
					if (masterVariant.VariantCount == 0)
					{
						masterVariant.IsSell = true;
					}
					else
					{
						masterVariant.UrlKey = string.Empty;
					}
					AutoGenerate.GeneratePid(db, product.Value.ProductStages);
					product.Value.ProductId = db.GetNextProductStageGroupId().SingleOrDefault().Value;
					db.ProductStageGroups.Add(product.Value);
				}
				#endregion
				Util.DeadlockRetry(db.SaveChanges, "ProductStage");
				return Request.CreateResponse(HttpStatusCode.OK, "Total " + groupList.Count + " products added");
			}
			catch (Exception e)
			{
				return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
			}
			finally
			{
				if (File.Exists(fileName))
				{
					File.Delete(fileName);
				}
			}
		}

		[Route("api/ProductStages/Import")]
		[HttpPut]
		public async Task<HttpResponseMessage> ImportSaveProduct()
		{
			string fileName = string.Empty;
			HashSet<string> errorMessage = new HashSet<string>();
			int row = 2;
			try
			{
				if (User.ShopRequest() == null)
				{
					throw new Exception("Invalid request");
				}

				#region Validate Request
				if (!Request.Content.IsMimeMultipartContent())
				{
					throw new Exception("Content Multimedia");
				}
				string root = HttpContext.Current.Server.MapPath("~/Import");
				var streamProvider = new MultipartFormDataStreamProvider(root);
				await Request.Content.ReadAsMultipartAsync(streamProvider);

				if (streamProvider.FileData == null || streamProvider.FileData.Count == 0)
				{
					throw new Exception("No file uploaded");
				}
				fileName = streamProvider.FileData[0].LocalFileName;
				#endregion
				HashSet<string> header = new HashSet<string>();
				var email = User.UserRequest().Email;
				var currentDt = SystemHelper.GetCurrentDateTime();
				Dictionary<string, ProductStageGroup> groupList = SetupImport(fileName, errorMessage, row,email,currentDt, db, true, header);
				#region Validate Error Message
				if (errorMessage.Count > 0)
				{
					return Request.CreateResponse(HttpStatusCode.NotAcceptable, errorMessage.ToList());
				}
				#endregion
				#region Query
				var shopId = User.ShopRequest().ShopId;
				int take = 100;
				int multiply = 0;
				while (true)
				{
					var tmpGroupList = groupList.Values.Skip(take * multiply).Take(take);
					var productIds = tmpGroupList.Select(s => s.ProductId);
					if (productIds.Count() == 0)
					{
						break;
					}
					var gropEnList = db.ProductStageGroups
					   .Where(w => w.ShopId == shopId && productIds.Contains(w.ProductId))
					   .Include(i => i.ProductStages.Select(s => s.ProductStageAttributes))
					   .Include(i => i.ProductStageTags)
					   .Include(i => i.ProductStageGlobalCatMaps)
					   .Include(i => i.ProductStageLocalCatMaps)
					   .Include(i => i.ProductStages.Select(s => s.Inventory))
					   .Include(i => i.ProductStageRelateds)
					   .Include(i => i.ProductStages.Select(s => s.ProductStageImages))
					   .Include(i => i.ProductStages.Select(s => s.ProductStageVideos));
					foreach (var g in tmpGroupList)
					{
						#region Validation
						var groupEn = gropEnList.Where(w => w.ProductId == g.ProductId).SingleOrDefault();
						if (groupEn == null)
						{
							errorMessage.Add("Cannot find 'empty' group id in seller portal. If you are trying to add new products, please use 'Import - Add New Products' feature.");
							continue;
						}
						if (Constant.PRODUCT_STATUS_WAIT_FOR_APPROVAL.Equals(groupEn.Status))
						{
							errorMessage.Add("Cannot edit product " + groupEn.ProductId + " with status Wait for Approval");
							continue;
						}
						#endregion
						groupEn.Status = Constant.PRODUCT_STATUS_DRAFT;
						#region Brand
						if (header.Contains("AAK"))
						{
							groupEn.BrandId = g.BrandId;
						}
						#endregion
						#region Category
						if (header.Contains("ACG"))
						{
							groupEn.GlobalCatId = g.GlobalCatId;
						}
						if (header.Contains("ACJ"))
						{
							groupEn.LocalCatId = g.LocalCatId;
						}
						if (header.Contains("ACH")
							|| header.Contains("ACI"))
						{
							var globalCat = groupEn.ProductStageGlobalCatMaps.ToList();
							if (g.ProductStageGlobalCatMaps != null)
							{
								foreach (var cat in g.ProductStageGlobalCatMaps)
								{
									bool isNewCat = false;
									if (globalCat == null || globalCat.Count == 0)
									{
										isNewCat = true;
									}
									if (!isNewCat)
									{
										var currentCat = globalCat.Where(w => w.CategoryId == cat.CategoryId).SingleOrDefault();
										if (currentCat != null)
										{
											globalCat.Remove(currentCat);
										}
										else
										{
											isNewCat = true;
										}
									}
									if (isNewCat)
									{
										groupEn.ProductStageGlobalCatMaps.Add(new ProductStageGlobalCatMap()
										{
											CategoryId = cat.CategoryId,
											CreateBy = email,
											CreateOn = currentDt,
											UpdateBy = email,
											UpdateOn = currentDt,
										});
									}
								}
							}
							if (globalCat != null && globalCat.Count > 0)
							{
								db.ProductStageGlobalCatMaps.RemoveRange(globalCat);
							}
						}
						if (header.Contains("ACK")
							|| header.Contains("ACL"))
						{
							var localCat = groupEn.ProductStageLocalCatMaps.ToList();
							if (g.ProductStageLocalCatMaps != null)
							{
								foreach (var cat in g.ProductStageLocalCatMaps)
								{
									bool isNewCat = false;
									if (localCat == null || localCat.Count == 0)
									{
										isNewCat = true;
									}
									if (!isNewCat)
									{
										var currentCat = localCat.Where(w => w.CategoryId == cat.CategoryId).SingleOrDefault();
										if (currentCat != null)
										{
											localCat.Remove(currentCat);
										}
										else
										{
											isNewCat = true;
										}
									}
									if (isNewCat)
									{
										groupEn.ProductStageLocalCatMaps.Add(new ProductStageLocalCatMap()
										{
											CategoryId = cat.CategoryId,
											CreateBy = email,
											CreateOn = currentDt,
											UpdateBy = email,
											UpdateOn = currentDt,
										});
									}
								}
							}
							if (localCat != null && localCat.Count > 0)
							{
								db.ProductStageLocalCatMaps.RemoveRange(localCat);
							}
						}
						#endregion
						#region Tag
						if (header.Contains("ABI"))
						{
							var tag = groupEn.ProductStageTags.ToList();
							if (g.ProductStageTags != null && g.ProductStageTags.ToList().Count > 0)
							{
								foreach (var t in g.ProductStageTags)
								{
									bool isNew = false;
									if (tag == null || tag.Count == 0)
									{
										isNew = true;
									}
									if (!isNew)
									{
										var current = tag.Where(w => w.Tag.Equals(t.Tag)).SingleOrDefault();
										if (current != null)
										{
											tag.Remove(current);
										}
										else
										{
											isNew = true;
										}
									}
									if (isNew)
									{
										groupEn.ProductStageTags.Add(new ProductStageTag()
										{
											Tag = t.Tag,
											CreateBy = email,
											CreateOn = currentDt,
											UpdateBy = email,
											UpdateOn = currentDt,
										});
									}
								}
							}
							if (tag != null && tag.Count > 0)
							{
								db.ProductStageTags.RemoveRange(tag);
							}
						}
						#endregion
						#region Relate Product
						if (header.Contains("ACM"))
						{
							db.ProductStageRelateds.RemoveRange(db.ProductStageRelateds.Where(w => w.Parent == groupEn.ProductId));
							groupEn.ProductStageRelateds1.Clear();
							foreach (var child in g.ProductStageRelateds1)
							{
								child.Parent = groupEn.ProductId;
								groupEn.ProductStageRelateds1.Add(child);
							}
						}
						#endregion
						#region Setup Group
						if (header.Contains("ACY"))
						{
							groupEn.EffectiveDate = g.EffectiveDate;
						}
						if (header.Contains("ACZ"))
						{
							groupEn.ExpireDate = g.ExpireDate;
						}
						if (header.Contains("ADA"))
						{
							groupEn.GiftWrap = g.GiftWrap;
						}

						if (header.Contains("ADB"))
						{
							groupEn.NewArrivalDate = g.NewArrivalDate;
						}
						if (header.Contains("ADC"))
						{
							groupEn.IsNew = g.IsNew;
						}
						if (header.Contains("ADD"))
						{
							groupEn.IsClearance = g.IsClearance;
						}
						if (header.Contains("ADE"))
						{
							groupEn.IsBestSeller = g.IsBestSeller;
						}
						if (header.Contains("ADF"))
						{
							groupEn.IsOnlineExclusive = g.IsOnlineExclusive;
						}
						if (header.Contains("ADG"))
						{
							groupEn.IsOnlyAt = g.IsOnlyAt;
						}
						if (header.Contains("ADH"))
						{
							groupEn.Remark = g.Remark;
						}
						if (header.Contains("ADI"))
						{
							groupEn.AttributeSetId = g.AttributeSetId;
						}
						if (header.Contains("ABR"))
						{
							groupEn.ShippingId = g.ShippingId;
						}
						#endregion
						#region Master Variant
						var masterVariantEn = groupEn.ProductStages.Where(w => w.IsVariant == false).SingleOrDefault();
						var importVariantEn = g.ProductStages.Where(w => w.IsVariant == false).SingleOrDefault();
						if (importVariantEn != null && masterVariantEn != null)
						{
							#region Setup Variant
							if (header.Contains("AAC"))
							{
								masterVariantEn.DefaultVariant = importVariantEn.DefaultVariant;
							}
							if (header.Contains("AAE"))
							{
								masterVariantEn.ProductNameEn = importVariantEn.ProductNameEn;
							}
							if (header.Contains("AAF"))
							{
								masterVariantEn.ProductNameTh = importVariantEn.ProductNameTh;
							}
							if (header.Contains("AAG"))
							{
								masterVariantEn.ProdTDNameEn = importVariantEn.ProdTDNameEn;
							}
							if (header.Contains("AAH"))
							{
								masterVariantEn.ProdTDNameTh = importVariantEn.ProdTDNameTh;
							}
							if (header.Contains("AAI"))
							{
								masterVariantEn.Sku = importVariantEn.Sku;
							}
							if (header.Contains("AAJ"))
							{
								masterVariantEn.Upc = importVariantEn.Upc;
							}
							if (header.Contains("AAM"))
							{
								masterVariantEn.OriginalPrice = importVariantEn.OriginalPrice;
							}
							if (header.Contains("AAL") && !Constant.IGNORE_PRICE_SHIPPING.Contains(groupEn.ShippingId))
							{
								masterVariantEn.SalePrice = importVariantEn.SalePrice;
							}
							if (header.Contains("AAN"))
							{
								masterVariantEn.Installment = importVariantEn.Installment;
							}
							if (header.Contains("AAO"))
							{
								masterVariantEn.PromotionPrice = importVariantEn.PromotionPrice;
							}
							if (header.Contains("AAP"))
							{
								masterVariantEn.EffectiveDatePromotion = importVariantEn.EffectiveDatePromotion;
							}
							if (header.Contains("AAQ"))
							{
								masterVariantEn.ExpireDatePromotion = importVariantEn.ExpireDatePromotion;
							}
							if (header.Contains("AAR"))
							{
								masterVariantEn.UnitPrice = importVariantEn.UnitPrice;
							}
							if (header.Contains("AAS"))
							{
								masterVariantEn.PurchasePrice = importVariantEn.PurchasePrice;
							}
							if (header.Contains("AAT"))
							{
								masterVariantEn.SaleUnitEn = importVariantEn.SaleUnitEn;
							}
							if (header.Contains("AAU"))
							{
								masterVariantEn.SaleUnitTh = importVariantEn.SaleUnitTh;
							}
							if (header.Contains("AAV"))
							{
								masterVariantEn.IsVat = importVariantEn.IsVat;
							}
							if (header.Contains("AAW"))
							{
								masterVariantEn.DescriptionFullEn = importVariantEn.DescriptionFullEn;
							}
							if (header.Contains("AAX"))
							{
								masterVariantEn.DescriptionFullTh = importVariantEn.DescriptionFullTh;
							}
							if (header.Contains("AAY"))
							{
								masterVariantEn.MobileDescriptionEn = importVariantEn.MobileDescriptionEn;
							}
							if (header.Contains("AAZ"))
							{
								masterVariantEn.MobileDescriptionTh = importVariantEn.MobileDescriptionTh;
							}

							if (header.Contains("ABA"))
							{
								masterVariantEn.DescriptionShortEn = importVariantEn.DescriptionShortEn;
							}
							if (header.Contains("ABB"))
							{
								masterVariantEn.DescriptionShortTh = importVariantEn.DescriptionShortTh;
							}
							if (header.Contains("ABC"))
							{
								masterVariantEn.KillerPoint1En = importVariantEn.KillerPoint1En;
							}
							if (header.Contains("ABD"))
							{
								masterVariantEn.KillerPoint1Th = importVariantEn.KillerPoint1Th;
							}
							if (header.Contains("ABE"))
							{
								masterVariantEn.KillerPoint2En = importVariantEn.KillerPoint2En;
							}
							if (header.Contains("ABF"))
							{
								masterVariantEn.KillerPoint2Th = importVariantEn.KillerPoint2Th;
							}
							if (header.Contains("ABG"))
							{
								masterVariantEn.KillerPoint3En = importVariantEn.KillerPoint3En;
							}
							if (header.Contains("ABH"))
							{
								masterVariantEn.KillerPoint3Th = importVariantEn.KillerPoint3Th;
							}
							if (header.Contains("ABK") && !Constant.IGNORE_INVENTORY_SHIPPING.Contains(groupEn.ShippingId))
							{
								if (masterVariantEn.Inventory != null)
								{
									masterVariantEn.Inventory.Quantity = masterVariantEn.Inventory.Quantity + importVariantEn.Inventory.Quantity;
								}
								else
								{
									masterVariantEn.Inventory = importVariantEn.Inventory;
								}
							}
							if (header.Contains("ABL"))
							{
								if (masterVariantEn.Inventory != null)
								{
									masterVariantEn.Inventory.SafetyStockSeller = importVariantEn.Inventory.SafetyStockSeller;
								}
								else
								{
									masterVariantEn.Inventory = importVariantEn.Inventory;
								}
							}
							if (header.Contains("ABM"))
							{
								if (masterVariantEn.Inventory != null)
								{
									masterVariantEn.Inventory.MinQtyAllowInCart = importVariantEn.Inventory.MinQtyAllowInCart;
								}
								else
								{
									masterVariantEn.Inventory = importVariantEn.Inventory;
								}
							}
							if (header.Contains("ABN"))
							{
								if (masterVariantEn.Inventory != null)
								{
									masterVariantEn.Inventory.MaxQtyAllowInCart = importVariantEn.Inventory.MaxQtyAllowInCart;
								}
								else
								{
									masterVariantEn.Inventory = importVariantEn.Inventory;
								}
							}
							if (header.Contains("ABP"))
							{
								if (masterVariantEn.Inventory != null)
								{
									masterVariantEn.Inventory.MaxQtyPreOrder = importVariantEn.Inventory.MaxQtyPreOrder;
								}
								else
								{
									masterVariantEn.Inventory = importVariantEn.Inventory;
								}
							}
							if (header.Contains("ABO"))
							{
								if (masterVariantEn.Inventory != null)
								{
									masterVariantEn.Inventory.StockType = importVariantEn.Inventory.StockType;
								}
								else
								{
									masterVariantEn.Inventory = importVariantEn.Inventory;
								}
							}
							if (header.Contains("ABQ"))
							{
								masterVariantEn.IsHasExpiryDate = importVariantEn.IsHasExpiryDate;
							}
							if (header.Contains("ABS"))
							{
								masterVariantEn.ExpressDelivery = importVariantEn.ExpressDelivery;
							}
							if (header.Contains("ABT"))
							{
								masterVariantEn.DeliveryFee = importVariantEn.DeliveryFee;
							}

							if (header.Contains("ABU"))
							{
								masterVariantEn.PrepareDay = importVariantEn.PrepareDay;
							}
							if (header.Contains("ABV"))
							{
								masterVariantEn.PrepareMon = importVariantEn.PrepareMon;
							}
							if (header.Contains("ABW"))
							{
								masterVariantEn.PrepareTue = importVariantEn.PrepareTue;
							}
							if (header.Contains("ABX"))
							{
								masterVariantEn.PrepareWed = importVariantEn.PrepareWed;
							}
							if (header.Contains("ABY"))
							{
								masterVariantEn.PrepareThu = importVariantEn.PrepareThu;
							}
							if (header.Contains("ABZ"))
							{
								masterVariantEn.PrepareFri = importVariantEn.PrepareFri;
							}
							if (header.Contains("ACA"))
							{
								masterVariantEn.PrepareSat = importVariantEn.PrepareSat;
							}
							if (header.Contains("ACB"))
							{
								masterVariantEn.PrepareSun = importVariantEn.PrepareSun;
							}
							if (header.Contains("ACC"))
							{
								masterVariantEn.Length = importVariantEn.Length;
							}
							if (header.Contains("ACD"))
							{
								masterVariantEn.Height = importVariantEn.Height;
							}
							if (header.Contains("ACE"))
							{
								masterVariantEn.Width = importVariantEn.Width;
							}
							if (header.Contains("ACF"))
							{
								masterVariantEn.Weight = importVariantEn.Weight;
							}

							if (header.Contains("ACN"))
							{
								masterVariantEn.SeoEn = importVariantEn.SeoEn;
							}
							if (header.Contains("ACO"))
							{
								masterVariantEn.SeoTh = importVariantEn.SeoTh;
							}

							if (header.Contains("ACP"))
							{
								masterVariantEn.MetaTitleEn = importVariantEn.MetaTitleEn;
							}
							if (header.Contains("ACQ"))
							{
								masterVariantEn.MetaTitleTh = importVariantEn.MetaTitleTh;
							}
							if (header.Contains("ACR"))
							{
								masterVariantEn.MetaDescriptionEn = importVariantEn.MetaDescriptionEn;
							}
							if (header.Contains("ACS"))
							{
								masterVariantEn.MetaDescriptionTh = importVariantEn.MetaDescriptionTh;
							}
							if (header.Contains("ACT"))
							{
								masterVariantEn.MetaKeyEn = importVariantEn.MetaKeyEn;
							}
							if (header.Contains("ACU"))
							{
								masterVariantEn.MetaKeyTh = importVariantEn.MetaKeyTh;
							}
							if (header.Contains("ACW"))
							{
								masterVariantEn.BoostWeight = importVariantEn.BoostWeight;
							}
							if (header.Contains("ACX"))
							{
								masterVariantEn.GlobalBoostWeight = importVariantEn.GlobalBoostWeight;
							}
							if (header.Contains("ADL"))
							{
								masterVariantEn.Visibility = importVariantEn.Visibility;
							}
							if (header.Contains("ACV") && g.ProductStages.Where(w => w.IsVariant == true).Count() == 0)
							{
								masterVariantEn.UrlKey = importVariantEn.UrlKey;
							}
							#endregion
						}
						#endregion
						#region Default Attribute
						var defaultAttribute = masterVariantEn.ProductStageAttributes;
						List<ProductStageAttribute> deleteList = new List<ProductStageAttribute>();
						if (importVariantEn.ProductStageAttributes != null)
						{
							var attrIds = importVariantEn.ProductStageAttributes.Select(s => s.AttributeId);
							deleteList.AddRange(defaultAttribute.Where(w => attrIds.Contains(w.AttributeId)));


							foreach (var tmpAttri in importVariantEn.ProductStageAttributes)
							{
								masterVariantEn.ProductStageAttributes.Add(new ProductStageAttribute()
								{
									AttributeId = tmpAttri.AttributeId,
									ValueEn = tmpAttri.ValueEn,
									ValueTh = tmpAttri.ValueTh,
									CheckboxValue = tmpAttri.CheckboxValue,
									CreateBy = tmpAttri.CreateBy,
									CreateOn = tmpAttri.CreateOn,
									IsAttributeValue = tmpAttri.IsAttributeValue,
									Position = tmpAttri.Position,
									UpdateBy = tmpAttri.UpdateBy,
									UpdateOn = tmpAttri.UpdateOn,
									AttributeValueId = tmpAttri.AttributeValueId,
									HtmlBoxValue = tmpAttri.HtmlBoxValue,
								});
							}
						}
						if (deleteList != null && deleteList.Count > 0)
						{
							db.ProductStageAttributes.RemoveRange(deleteList);
						}
						#endregion
						#region Variants
						var stageList = groupEn.ProductStages.Where(w => w.IsVariant == true).ToList();
						if (g.ProductStages != null)
						{
							foreach (var staging in g.ProductStages.Where(w => w.IsVariant == true))
							{
								bool isNewStage = false;
								if (stageList == null || stageList.Count == 0)
								{
									isNewStage = true;
								}
								if (!isNewStage)
								{
									var currentStage = stageList.Where(w => w.Pid.Equals(staging.Pid)).SingleOrDefault();
									if (currentStage != null)
									{
										stageList.Remove(currentStage);
										currentStage.Status = Constant.PRODUCT_STATUS_DRAFT;
										#region Setup Variant
										if (header.Contains("AAC"))
										{
											currentStage.DefaultVariant = staging.DefaultVariant;
										}
										if (header.Contains("AAE"))
										{
											currentStage.ProductNameEn = staging.ProductNameEn;
										}
										if (header.Contains("AAF"))
										{
											currentStage.ProductNameTh = staging.ProductNameTh;
										}
										if (header.Contains("AAG"))
										{
											currentStage.ProdTDNameEn = staging.ProdTDNameEn;
										}
										if (header.Contains("AAH"))
										{
											currentStage.ProdTDNameTh = staging.ProdTDNameTh;
										}
										if (header.Contains("AAI"))
										{
											currentStage.Sku = staging.Sku;
										}
										if (header.Contains("AAJ"))
										{
											currentStage.Upc = staging.Upc;
										}
										if (header.Contains("AAM"))
										{
											currentStage.OriginalPrice = staging.OriginalPrice;
										}
										if (header.Contains("AAL"))
										{
											currentStage.SalePrice = staging.SalePrice;
										}
										if (header.Contains("AAN"))
										{
											currentStage.Installment = staging.Installment;
										}
										if (header.Contains("AAO"))
										{
											currentStage.PromotionPrice = staging.PromotionPrice;
										}
										if (header.Contains("AAP"))
										{
											currentStage.EffectiveDatePromotion = staging.EffectiveDatePromotion;
										}
										if (header.Contains("AAQ"))
										{
											currentStage.ExpireDatePromotion = staging.ExpireDatePromotion;
										}
										if (header.Contains("AAR"))
										{
											currentStage.UnitPrice = staging.UnitPrice;
										}
										if (header.Contains("AAS"))
										{
											currentStage.PurchasePrice = staging.PurchasePrice;
										}
										if (header.Contains("AAT"))
										{
											currentStage.SaleUnitEn = staging.SaleUnitEn;
										}
										if (header.Contains("AAU"))
										{
											currentStage.SaleUnitTh = staging.SaleUnitTh;
										}
										if (header.Contains("AAV"))
										{
											currentStage.IsVat = staging.IsVat;
										}

										if (header.Contains("AAW"))
										{
											currentStage.DescriptionFullEn = staging.DescriptionFullEn;
										}
										if (header.Contains("AAX"))
										{
											currentStage.DescriptionFullTh = staging.DescriptionFullTh;
										}
										if (header.Contains("AAY"))
										{
											currentStage.MobileDescriptionEn = staging.MobileDescriptionEn;
										}
										if (header.Contains("AAZ"))
										{
											currentStage.MobileDescriptionTh = staging.MobileDescriptionTh;
										}

										if (header.Contains("ABA"))
										{
											currentStage.DescriptionShortEn = staging.DescriptionShortEn;
										}
										if (header.Contains("ABB"))
										{
											currentStage.DescriptionShortTh = staging.DescriptionShortTh;
										}
										if (header.Contains("ABC"))
										{
											currentStage.KillerPoint1En = staging.KillerPoint1En;
										}
										if (header.Contains("ABD"))
										{
											currentStage.KillerPoint1Th = staging.KillerPoint1Th;
										}
										if (header.Contains("ABE"))
										{
											currentStage.KillerPoint2En = staging.KillerPoint2En;
										}
										if (header.Contains("ABF"))
										{
											currentStage.KillerPoint2Th = staging.KillerPoint2Th;
										}
										if (header.Contains("ABG"))
										{
											currentStage.KillerPoint3En = staging.KillerPoint3En;
										}
										if (header.Contains("ABH"))
										{
											currentStage.KillerPoint3Th = staging.KillerPoint3Th;
										}
										if (header.Contains("ABJ"))
										{
											if (currentStage.Inventory != null)
											{
												currentStage.Inventory.Quantity = staging.Inventory.Quantity;
											}
											else
											{
												currentStage.Inventory = staging.Inventory;
											}
										}
										if (header.Contains("ABL"))
										{
											if (currentStage.Inventory != null)
											{
												currentStage.Inventory.SafetyStockSeller = staging.Inventory.SafetyStockSeller;
											}
											else
											{
												currentStage.Inventory = staging.Inventory;
											}
										}
										if (header.Contains("ABM"))
										{
											if (currentStage.Inventory != null)
											{
												currentStage.Inventory.MinQtyAllowInCart = staging.Inventory.MinQtyAllowInCart;
											}
											else
											{
												currentStage.Inventory = staging.Inventory;
											}
										}
										if (header.Contains("ABN"))
										{
											if (currentStage.Inventory != null)
											{
												currentStage.Inventory.MaxQtyAllowInCart = staging.Inventory.MaxQtyAllowInCart;
											}
											else
											{
												currentStage.Inventory = staging.Inventory;
											}
										}
										if (header.Contains("ABP"))
										{
											if (currentStage.Inventory != null)
											{
												currentStage.Inventory.MaxQtyPreOrder = staging.Inventory.MaxQtyPreOrder;
											}
											else
											{
												currentStage.Inventory = staging.Inventory;
											}
										}
										if (header.Contains("ABO"))
										{
											if (currentStage.Inventory != null)
											{
												currentStage.Inventory.StockType = staging.Inventory.StockType;
											}
											else
											{
												currentStage.Inventory = staging.Inventory;
											}
										}
										if (header.Contains("ABQ"))
										{
											currentStage.IsHasExpiryDate = staging.IsHasExpiryDate;
										}
										if (header.Contains("ABS"))
										{
											currentStage.ExpressDelivery = staging.ExpressDelivery;
										}
										if (header.Contains("ABT"))
										{
											currentStage.DeliveryFee = staging.DeliveryFee;
										}

										if (header.Contains("ABU"))
										{
											currentStage.PrepareDay = staging.PrepareDay;
										}
										if (header.Contains("ABV"))
										{
											currentStage.PrepareMon = staging.PrepareMon;
										}
										if (header.Contains("ABW"))
										{
											currentStage.PrepareTue = staging.PrepareTue;
										}
										if (header.Contains("ABX"))
										{
											currentStage.PrepareWed = staging.PrepareWed;
										}
										if (header.Contains("ABY"))
										{
											currentStage.PrepareThu = staging.PrepareThu;
										}
										if (header.Contains("ABZ"))
										{
											currentStage.PrepareFri = staging.PrepareFri;
										}
										if (header.Contains("ACA"))
										{
											currentStage.PrepareSat = staging.PrepareSat;
										}
										if (header.Contains("ACB"))
										{
											currentStage.PrepareSun = staging.PrepareSun;
										}
										if (header.Contains("ACC"))
										{
											currentStage.Length = staging.Length;
										}
										if (header.Contains("ACD"))
										{
											currentStage.Height = staging.Height;
										}
										if (header.Contains("ACE"))
										{
											currentStage.Width = staging.Width;
										}
										if (header.Contains("ACF"))
										{
											currentStage.Weight = staging.Weight;
										}
										if (header.Contains("ACN"))
										{
											currentStage.SeoEn = staging.SeoEn;
										}
										if (header.Contains("ACO"))
										{
											currentStage.SeoTh = staging.SeoTh;
										}
										if (header.Contains("ACP"))
										{
											currentStage.MetaTitleEn = staging.MetaTitleEn;
										}
										if (header.Contains("ACQ"))
										{
											currentStage.MetaTitleTh = staging.MetaTitleTh;
										}
										if (header.Contains("ACR"))
										{
											currentStage.MetaDescriptionEn = staging.MetaDescriptionEn;
										}
										if (header.Contains("ACS"))
										{
											currentStage.MetaDescriptionTh = staging.MetaDescriptionTh;
										}
										if (header.Contains("ACT"))
										{
											currentStage.MetaKeyEn = staging.MetaKeyEn;
										}
										if (header.Contains("ACU"))
										{
											currentStage.MetaKeyTh = staging.MetaKeyTh;
										}
										if (header.Contains("ACW"))
										{
											currentStage.BoostWeight = staging.BoostWeight;
										}
										if (header.Contains("ACX"))
										{
											currentStage.GlobalBoostWeight = staging.GlobalBoostWeight;
										}
										if (header.Contains("ADL"))
										{
											currentStage.Visibility = staging.Visibility;
										}
										if (header.Contains("ACV"))
										{
											currentStage.UrlKey = staging.UrlKey;
										}
										#endregion
										#region Setup Attribute
										var attributeTmp = currentStage.ProductStageAttributes.ToList();
										if (staging.ProductStageAttributes != null && staging.ProductStageAttributes.Count > 0)
										{
											foreach (var tmpAttri in staging.ProductStageAttributes)
											{
												bool isNew = false;
												if (attributeTmp == null || attributeTmp.Count == 0)
												{
													isNew = true;
												}
												if (!isNew)
												{
													var current = attributeTmp.Where(w => w.AttributeId == tmpAttri.AttributeId && w.ValueEn.Equals(tmpAttri.ValueEn)).SingleOrDefault();
													if (current != null)
													{
														attributeTmp.Remove(current);
													}
													else
													{
														isNew = true;
													}
												}
												if (isNew)
												{
													currentStage.ProductStageAttributes.Add(new ProductStageAttribute()
													{
														AttributeId = tmpAttri.AttributeId,
														ValueEn = tmpAttri.ValueEn,
														ValueTh = tmpAttri.ValueTh,
														CheckboxValue = tmpAttri.CheckboxValue,
														CreateBy = tmpAttri.CreateBy,
														CreateOn = tmpAttri.CreateOn,
														IsAttributeValue = tmpAttri.IsAttributeValue,
														Position = tmpAttri.Position,
														UpdateBy = tmpAttri.UpdateBy,
														UpdateOn = tmpAttri.UpdateOn,
														HtmlBoxValue = tmpAttri.HtmlBoxValue,
													});
												}
											}
										}
										if (attributeTmp != null && attributeTmp.Count > 0)
										{
											db.ProductStageAttributes.RemoveRange(attributeTmp);
										}
										#endregion
									}
									else
									{
										isNewStage = true;
									}
								}
								if (isNewStage)
								{
									if (!string.IsNullOrEmpty(staging.Pid))
									{
										errorMessage.Add("New variant cannot have Pid " + staging.Pid);
										continue;
									}
									staging.Status = Constant.PRODUCT_STATUS_DRAFT;
									groupEn.ProductStages.Add(staging);
								}
							}
						}
						//if (stageList != null && stageList.Count > 0)
						//{
						//    db.ProductStages.RemoveRange(stageList);
						//}
						#endregion
						#region Flag
						var masterVariant = groupEn.ProductStages.Where(w => w.IsVariant == false).SingleOrDefault();
						if (!string.IsNullOrWhiteSpace(masterVariant.ProductNameEn)
							&& !string.IsNullOrWhiteSpace(masterVariant.ProductNameTh)
							&& !string.IsNullOrWhiteSpace(masterVariant.Sku)
							&& groupEn.BrandId != null
							&& !string.IsNullOrWhiteSpace(masterVariant.DescriptionFullEn)
							&& !string.IsNullOrWhiteSpace(masterVariant.DescriptionFullTh)
							&& !string.IsNullOrWhiteSpace(masterVariant.MobileDescriptionEn)
							&& !string.IsNullOrWhiteSpace(masterVariant.MobileDescriptionTh))
						{
							groupEn.InfoFlag = true;
						}
						else
						{
							groupEn.InfoFlag = false;
						}
						#endregion
						masterVariant.VariantCount = groupEn.ProductStages.Where(w => w.IsVariant == true).ToList().Count;
						groupEn.UpdateOn = currentDt;
						groupEn.UpdateBy = email;
					}
					multiply++;
				}
				#endregion
				#region Validate Error Message
				if (errorMessage.Count > 0)
				{
					return Request.CreateResponse(HttpStatusCode.NotAcceptable, errorMessage.ToList());
				}
				#endregion
				#region Setup Product for database
				var requireDefaultAttr = db.Attributes.Where(w => w.Required && !Constant.DATA_TYPE_CHECKBOX.Equals(w.DataType) && w.DefaultAttribute).Select(s => s.AttributeId);
				foreach (var product in groupList)
				{
					var masterProduct = product.Value.ProductStages.Where(w => w.IsVariant == false).SingleOrDefault();
					#region Validate Flag
					if (!string.IsNullOrWhiteSpace(masterProduct.ProductNameEn)
					   && !string.IsNullOrWhiteSpace(masterProduct.ProductNameTh)
					   && product.Value.BrandId != null)
					{
						product.Value.InfoFlag = true;
						var attrIds = masterProduct.ProductStageAttributes.Where(w => !string.IsNullOrWhiteSpace(w.ValueEn)).Select(s => s.AttributeId);
						foreach (var id in requireDefaultAttr)
						{
							if (!attrIds.Contains(id))
							{
								product.Value.InfoFlag = false;
								break;
							}
						}
						if (product.Value.AttributeSetId.HasValue && product.Value.InfoFlag)
						{
							var masterAttr = db.Attributes
								.Where(w => w.Required
									&& !Constant.DATA_TYPE_CHECKBOX.Equals(w.DataType)
									&& w.AttributeSetMaps.Any(a => a.AttributeSetId == product.Value.AttributeSetId.Value))
								.Select(s => s.AttributeId);
							foreach (var id in masterAttr)
							{
								if (!masterAttr.Contains(id))
								{
									product.Value.InfoFlag = false;
									break;
								}
							}
						}
					}
					else
					{
						product.Value.InfoFlag = false;
					}
					#endregion
					masterProduct.VariantCount = product.Value.ProductStages.Where(w => w.IsVariant == true).Count();
					if (masterProduct.VariantCount == 0)
					{
						masterProduct.IsSell = true;
					}
					AutoGenerate.GeneratePid(db, product.Value.ProductStages);
				}
				#endregion
				Util.DeadlockRetry(db.SaveChanges, "ProductStage");
				return Request.CreateResponse(HttpStatusCode.OK, "Total " + groupList.Count + " products updated");
			}
			catch (Exception e)
			{
				return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
			}
			finally
			{
				if (File.Exists(fileName))
				{
					File.Delete(fileName);
				}
			}
		}


		private List<List<string>> ReadExcel(CsvReader csvResult, string[] header, List<string> firstRow)
		{
			List<List<string>> listRow = new List<List<string>>() { firstRow };
			List<string> listColumn = null;
			while (csvResult.Read())
			{
				listColumn = new List<string>();
				foreach (string h in header)
				{
					listColumn.Add(csvResult.GetField<string>(h));
				}
				listRow.Add(listColumn);
			}
			return listRow;
		}

		private Dictionary<string, ProductStageGroup> SetupImport(string fileName, HashSet<string> errorMessage, int row,string email,DateTime currentDt, ColspEntities db, bool isUpdate = false, HashSet<string> updateHeader = null)
		{
			using (var fileReader = File.OpenText(fileName))
			{
				Dictionary<string, ProductStageGroup> groupList = null;
				using (var csvResult = new CsvReader(fileReader))
				{
					if (!csvResult.Read())
					{
						throw new Exception("File is not in a proper format");
					}
					#region Header
					var tmpGuidance = db.ImportHeaders.Where(w => true);
					if (User.ShopRequest() != null && User.UserRequest().IsAdmin == false)
					{
						var groupName = User.ShopRequest().ShopGroup;
						tmpGuidance = tmpGuidance.Where(w => w.ShopGroups.Any(a => a.Abbr.Equals(groupName)));
					}
					var guidance = tmpGuidance.Select(s => new ImportHeaderRequest()
					{
						HeaderName = s.HeaderName,
						MapName = s.MapName,
					}).ToList();
					Dictionary<string, int> headDic = new Dictionary<string, int>();
					IEnumerable<IEnumerable<string>> csvRows = null;
					int i = 0;
					string[] headers = csvResult.FieldHeaders;
					List<string> firstRow = new List<string>();
					foreach (string head in headers)
					{
						if (headDic.ContainsKey(head))
						{
							throw new Exception(head + " is duplicate header");
						}
						var headerGuidance = guidance.Where(w => w.HeaderName.Equals(head.Trim())).Select(s => s.MapName).FirstOrDefault();
						if (string.IsNullOrEmpty(headerGuidance))
						{
							headDic.Add(head, i++);
						}
						else
						{
							headDic.Add(headerGuidance, i++);
						}
						firstRow.Add(csvResult.GetField<string>(head));
						if (isUpdate)
						{
							updateHeader.Add(headerGuidance);
						}
					}
					csvRows = ReadExcel(csvResult, headers, firstRow);
					foreach(var ignore in Constant.IMPORT_REQUIRE_FIELD)
					{
						if (!headDic.ContainsKey(ignore))
						{
							throw new Exception("The uploaded file has invalid format");
						}
					}
					#endregion
					List<ProductStage> products = new List<ProductStage>();
					#region Default Query
					int shopId = User.ShopRequest().ShopId;
					var brands = db.Brands.Where(w => w.Status.Equals(Constant.STATUS_ACTIVE)).Select(s => new { BrandNameEn = s.BrandNameEn, BrandId = s.BrandId });
					var globalCatId = db.GlobalCategories.Where(w => w.Rgt - w.Lft == 1).Select(s => new { CategoryId = s.CategoryId });
					var localCatId = db.LocalCategories.Where(w => w.Rgt - w.Lft == 1 && w.ShopId == shopId).Select(s => new { CategoryId = s.CategoryId });
					var attributeSet = db.AttributeSets
						.Where(w => w.Status.Equals(Constant.STATUS_ACTIVE))
						.Select(s => new
						{
							s.AttributeSetId,
							s.AttributeSetNameEn,
							Attribute = s.AttributeSetMaps.Select(se => new
							{
								se.Attribute.AttributeId,
								se.Attribute.AttributeNameEn,
								se.Attribute.VariantStatus,
								se.Attribute.DataType,
								AttributeValue = se.Attribute.AttributeValueMaps.Select(sv => new { sv.AttributeValue.AttributeValueId, sv.AttributeValue.AttributeValueEn })
							})
						});

					var tmpDefaultAttribute = db.Attributes.Where(w => w.DefaultAttribute == true);
					if (User.ShopRequest() != null)
					{
						tmpDefaultAttribute = tmpDefaultAttribute.Where(w => Constant.ATTRIBUTE_VISIBLE_ALL_USER.Equals(w.VisibleTo));
					}
					var defaultAttribute = tmpDefaultAttribute
						.Select(se => new
						{
							se.AttributeId,
							se.AttributeNameEn,
							se.VariantStatus,
							se.DataType,
							AttributeValue = se.AttributeValueMaps.Select(sv => new { sv.AttributeValue.AttributeValueId, sv.AttributeValue.AttributeValueEn })
						});
					var relatedProduct = db.ProductStages.Where(w => w.ShopId == shopId && w.IsVariant == false).Select(s => new { s.Pid, s.ProductId });
					var shipping = db.Shippings.ToList();
					#endregion
					#region Initialize
					Dictionary<Tuple<string, int>, Inventory> inventoryList = new Dictionary<Tuple<string, int>, Inventory>();
					groupList = new Dictionary<string, ProductStageGroup>();
					int tmpGroupId = 0;
					Regex rg = new Regex(@"/(\(\()\d+(\)\))/");
					Regex rgNumOnly = new Regex(@"^[0-9]*$");
					Regex rgAlphaNumeric = new Regex(@"^[a-zA-Z0-9-]*$");
					List<string> body = null;
					string groupId = null;
					bool isNew = true;
					ProductStageGroup group = null;
					ProductStage variant = null;
					//ProductStageVariant variant = null;
					#endregion
					foreach (var b in csvRows)
					{
						body = b.ToList();
						if (body.All(a => string.IsNullOrWhiteSpace(a)))
						{
							continue;
						}
						#region Group
						isNew = true;
						groupId = string.Empty;
						group = null;
						if (headDic.ContainsKey("AAB"))
						{
							//Get column 'Group Id'.
							groupId = body[headDic["AAB"]];
							if (rg.IsMatch(groupId))
							{
								errorMessage.Add("Invalid Group ID at row" + row);
								continue;
							}
							if (groupList.ContainsKey(groupId))
							{
								group = groupList[groupId];
								isNew = false;
							}
						}
						if (group == null)
						{
							long productId = 0;
							if (string.IsNullOrEmpty(groupId))
							{
								groupId = string.Concat("((", tmpGroupId++, "))");
							}
							else if (isUpdate)
							{
								try
								{
									productId = Convert.ToInt32(groupId);
								}
								catch (Exception)
								{
									productId = 0;
								}
							}
							group = new ProductStageGroup()
							{
								ProductId = productId,
								ShopId = shopId,
								Status = Constant.PRODUCT_STATUS_DRAFT,
								CategoryTabStatus = Constant.PRODUCT_STATUS_WAIT_FOR_APPROVAL,
								ImageTabStatus = Constant.PRODUCT_STATUS_WAIT_FOR_APPROVAL,
								InformationTabStatus = Constant.PRODUCT_STATUS_WAIT_FOR_APPROVAL,
								MoreOptionTabStatus = Constant.PRODUCT_STATUS_WAIT_FOR_APPROVAL,
								VariantTabStatus = Constant.PRODUCT_STATUS_WAIT_FOR_APPROVAL,
								ImageFlag = false,
								InfoFlag = false,
								OnlineFlag = false,
								RejectReason = string.Empty,
								TheOneCardEarn = Constant.DEFAULT_THE_ONE_CARD,
								CreateBy = email,
								CreateOn = currentDt,
								UpdateBy = email,
								UpdateOn = currentDt
							};
						}
						#endregion
						#region Variant Detail
						//Initialise product stage variant
						variant = new ProductStage()
						{
							ShopId = shopId,
							Status = Constant.PRODUCT_STATUS_DRAFT,
							CreateBy = email,
							CreateOn = currentDt,
							UpdateBy = email,
							UpdateOn = currentDt,
							Visibility = headDic.ContainsKey("ADL") && string.Equals(body[headDic["ADL"]], "no", StringComparison.OrdinalIgnoreCase) ? false : true,
							IsVariant = true,
							IsMaster = false,
							FeatureImgUrl = string.Empty,
							ImageCount = 0,
							MaxiQtyAllowed = 0,
							MiniQtyAllowed = 0,
							UnitPrice = 0,
							DimensionUnit = Constant.DIMENSTION_MM,
							WeightUnit = Constant.WEIGHT_MEASURE_G,
							GlobalBoostWeight = Validation.ValidateCSVIntegerColumn(headDic, body, "ACX", guidance, false, 10000, errorMessage, row, Constant.DEFAULT_GLOBAL_BOOSTWEIGHT),
							Display = Constant.VARIANT_DISPLAY_GROUP,
							VariantCount = 0,
							PurchasePrice = 0,
							SalePrice = 0,
							OriginalPrice = 0,
							DefaultVariant = false,
							Bu = null,
							DeliveryFee = 0,
							EffectiveDatePromotion = Validation.ValidateCSVDatetimeColumn(headDic, body, "AAP", guidance, errorMessage, row),
							ExpireDatePromotion = Validation.ValidateCSVDatetimeColumn(headDic, body, "AAQ", guidance, errorMessage, row),
							IsSell = false,
							JDADept = string.Empty,
							JDASubDept = string.Empty,
							NewArrivalDate = Validation.ValidateCSVDatetimeColumn(headDic, body, "ADB", guidance, errorMessage, row),
							PromotionPrice = 0,
							OldPid = null,
							ProductNameEn = Validation.ValidateCSVStringColumn(headDic, body, "AAE", guidance, !isUpdate, 255, errorMessage, row),
							ProductNameTh = Validation.ValidateCSVStringColumn(headDic, body, "AAF", guidance, !isUpdate, 255, errorMessage, row),
							ProdTDNameEn = Validation.ValidateCSVStringColumn(headDic, body, "AAG", guidance, false, 55, errorMessage, row, string.Empty),
							ProdTDNameTh = Validation.ValidateCSVStringColumn(headDic, body, "AAH", guidance, false, 55, errorMessage, row, string.Empty),
							Sku = Validation.ValidateCSVStringColumn(headDic, body, "AAI", guidance, !isUpdate, 255, errorMessage, row),
							Upc = Validation.ValidateCSVStringColumn(headDic, body, "AAJ", guidance, false, 13, errorMessage, row, string.Empty, rgNumOnly),
							SaleUnitEn = Validation.ValidateCSVStringColumn(headDic, body, "AAT", guidance, false, 255, errorMessage, row, string.Empty),
							SaleUnitTh = Validation.ValidateCSVStringColumn(headDic, body, "AAU", guidance, false, 255, errorMessage, row, string.Empty),
							IsVat = headDic.ContainsKey("AAV") && string.Equals(body[headDic["AAV"]], "yes", StringComparison.OrdinalIgnoreCase) ? Constant.STATUS_YES : Constant.STATUS_NO,
							DescriptionFullEn = Validation.ValidateCSVStringColumn(headDic, body, "AAW", guidance, false, 50000, errorMessage, row, string.Empty),
							DescriptionFullTh = Validation.ValidateCSVStringColumn(headDic, body, "AAX", guidance, false, 50000, errorMessage, row, string.Empty),
							MobileDescriptionEn = Validation.ValidateCSVStringColumn(headDic, body, "AAY", guidance, false, 50000, errorMessage, row, string.Empty),
							MobileDescriptionTh = Validation.ValidateCSVStringColumn(headDic, body, "AAZ", guidance, false, 50000, errorMessage, row, string.Empty),
							DescriptionShortEn = Validation.ValidateCSVStringColumn(headDic, body, "ABA", guidance, false, 500, errorMessage, row, string.Empty),
							DescriptionShortTh = Validation.ValidateCSVStringColumn(headDic, body, "ABB", guidance, false, 500, errorMessage, row, string.Empty),
							KillerPoint1En = Validation.ValidateCSVStringColumn(headDic, body, "ABC", guidance, false, 50, errorMessage, row, string.Empty),
							KillerPoint1Th = Validation.ValidateCSVStringColumn(headDic, body, "ABD", guidance, false, 50, errorMessage, row, string.Empty),
							KillerPoint2En = Validation.ValidateCSVStringColumn(headDic, body, "ABE", guidance, false, 50, errorMessage, row, string.Empty),
							KillerPoint2Th = Validation.ValidateCSVStringColumn(headDic, body, "ABF", guidance, false, 50, errorMessage, row, string.Empty),
							KillerPoint3En = Validation.ValidateCSVStringColumn(headDic, body, "ABG", guidance, false, 50, errorMessage, row, string.Empty),
							KillerPoint3Th = Validation.ValidateCSVStringColumn(headDic, body, "ABH", guidance, false, 50, errorMessage, row, string.Empty),
							IsHasExpiryDate = headDic.ContainsKey("ABQ") && string.Equals(body[headDic["ABQ"]], "yes", StringComparison.OrdinalIgnoreCase) ? Constant.STATUS_YES : Constant.STATUS_NO,
							ExpressDelivery = headDic.ContainsKey("ABS") && string.Equals(body[headDic["ABS"]], "yes", StringComparison.OrdinalIgnoreCase) ? Constant.STATUS_YES : Constant.STATUS_NO,
							SeoEn = Validation.ValidateCSVStringColumn(headDic, body, "ACN", guidance, false, 1000, errorMessage, row, string.Empty),
							SeoTh = Validation.ValidateCSVStringColumn(headDic, body, "ACO", guidance, false, 1000, errorMessage, row, string.Empty),
							MetaTitleEn = Validation.ValidateCSVStringColumn(headDic, body, "ACP", guidance, false, 60, errorMessage, row, string.Empty),
							MetaTitleTh = Validation.ValidateCSVStringColumn(headDic, body, "ACQ", guidance, false, 60, errorMessage, row, string.Empty),
							MetaDescriptionEn = Validation.ValidateCSVStringColumn(headDic, body, "ACR", guidance, false, 150, errorMessage, row, string.Empty),
							MetaDescriptionTh = Validation.ValidateCSVStringColumn(headDic, body, "ACS", guidance, false, 150, errorMessage, row, string.Empty),
							MetaKeyEn = Validation.ValidateCSVStringColumn(headDic, body, "ACT", guidance, false, 1000, errorMessage, row, string.Empty),
							MetaKeyTh = Validation.ValidateCSVStringColumn(headDic, body, "ACU", guidance, false, 1000, errorMessage, row, string.Empty),
							UrlKey = Validation.ValidateCSVStringColumn(headDic, body, "ACV", guidance, false, 100, errorMessage, row, string.Empty, rgAlphaNumeric),
							Installment = headDic.ContainsKey("AAN") && string.Equals(body[headDic["AAN"]], "yes", StringComparison.OrdinalIgnoreCase) ? Constant.STATUS_YES : Constant.STATUS_NO,
							PrepareDay = Validation.ValidateCSVIntegerColumn(headDic, body, "ABU", guidance, false, 999, errorMessage, row, 0),
							PrepareMon = Validation.ValidateCSVIntegerColumn(headDic, body, "ABV", guidance, false, 999, errorMessage, row, 0),
							PrepareTue = Validation.ValidateCSVIntegerColumn(headDic, body, "ABW", guidance, false, 999, errorMessage, row, 0),
							PrepareWed = Validation.ValidateCSVIntegerColumn(headDic, body, "ABX", guidance, false, 999, errorMessage, row, 0),
							PrepareThu = Validation.ValidateCSVIntegerColumn(headDic, body, "ABY", guidance, false, 999, errorMessage, row, 0),
							PrepareFri = Validation.ValidateCSVIntegerColumn(headDic, body, "ABZ", guidance, false, 999, errorMessage, row, 0),
							PrepareSat = Validation.ValidateCSVIntegerColumn(headDic, body, "ACA", guidance, false, 999, errorMessage, row, 0),
							PrepareSun = Validation.ValidateCSVIntegerColumn(headDic, body, "ACB", guidance, false, 999, errorMessage, row, 0),
							LimitIndividualDay = true,
							Length = Validation.ValidateCSVIntegerColumn(headDic, body, "ACC", guidance, false, int.MaxValue, errorMessage, row, 0),
							Height = Validation.ValidateCSVIntegerColumn(headDic, body, "ACD", guidance, false, int.MaxValue, errorMessage, row, 0),
							Width = Validation.ValidateCSVIntegerColumn(headDic, body, "ACE", guidance, false, int.MaxValue, errorMessage, row, 0),
							Weight = Validation.ValidateCSVIntegerColumn(headDic, body, "ACF", guidance, false, int.MaxValue, errorMessage, row, 0),
							BoostWeight = Validation.ValidateCSVIntegerColumn(headDic, body, "ACW", guidance, false, 10000, errorMessage, row, 0),
						};
						if (headDic.ContainsKey("AAC"))
						{
							variant.DefaultVariant = string.Equals(body[headDic["AAC"]], "yes", StringComparison.OrdinalIgnoreCase);
						}
						#region Variant Validation
						if (!string.IsNullOrWhiteSpace(variant.UrlKey))
						{
							variant.UrlKey = variant.UrlKey.ToLower();
						}
						if (!variant.NewArrivalDate.HasValue)
						{
							variant.NewArrivalDate = currentDt;
						}
						#endregion
						#region Price
						if (headDic.ContainsKey("AAO"))
						{
							try
							{
								var priceSt = body[headDic["AAO"]];
								if (!string.IsNullOrWhiteSpace(priceSt))
								{
									decimal price = decimal.Parse(priceSt);
									variant.PromotionPrice = price;
								}
							}
							catch
							{
								errorMessage.Add("Invalid Promotion Price at row " + row);
							}
						}
						if (headDic.ContainsKey("AAR"))
						{
							try
							{
								var unitPriceSt = body[headDic["AAR"]];
								if (!string.IsNullOrWhiteSpace(unitPriceSt))
								{
									decimal unitPrice = decimal.Parse(unitPriceSt);
									variant.UnitPrice = unitPrice;
								}
							}
							catch
							{
								errorMessage.Add("Invalid Unit Price at row " + row);
							}
						}
						if (headDic.ContainsKey("AAS"))
						{
							try
							{
								var purchasePriceSt = body[headDic["AAS"]];
								if (!string.IsNullOrWhiteSpace(purchasePriceSt))
								{
									decimal purchasePrice = decimal.Parse(purchasePriceSt);
									variant.PurchasePrice = purchasePrice;
								}
							}
							catch
							{
								errorMessage.Add("Invalid Purchase Price at row " + row);
							}
						}
						#endregion
						#region Pid
						if (isUpdate)
						{
							if (headDic.ContainsKey("AAD"))
							{
								variant.Pid = Validation.ValidateCSVStringColumn(headDic, body, "AAD", guidance, false, 7, errorMessage, row, string.Empty);
							}
							else
							{
								throw new Exception("No PID column found");
							}
						}
						#endregion
						#region Validate Promotion
						if (variant.EffectiveDatePromotion.HasValue
							&& variant.ExpireDatePromotion.HasValue)
						{
							if (variant.EffectiveDatePromotion.Value.CompareTo(variant.ExpireDatePromotion.Value)
								> 0)
							{
								errorMessage.Add(string.Concat("Promotion Effective Date must be earlier than Promotion Expire Date at row ", row));
							}
						}
						#endregion
						#endregion
						#region Delivery Fee
						if (headDic.ContainsKey("ABT"))
						{
							try
							{
								var feeSt = body[headDic["ABT"]];
								if (!string.IsNullOrWhiteSpace(feeSt))
								{
									decimal fee = decimal.Parse(feeSt);
									variant.DeliveryFee = fee;
								}
							}
							catch
							{
								errorMessage.Add("Invalid Delivery Fee at row " + row);
							}
						}
						#endregion
						#region Original Price
						if (headDic.ContainsKey("AAM"))
						{
							try
							{
								var originalPriceSt = body[headDic["AAM"]];
								if (!string.IsNullOrWhiteSpace(originalPriceSt))
								{
									decimal originalPrice = decimal.Parse(originalPriceSt);
									variant.OriginalPrice = originalPrice;
								}
							}
							catch
							{
								errorMessage.Add("Invalid Original Price at row " + row);
							}
						}
						#endregion
						#region Sale Price
						if (headDic.ContainsKey("AAL"))
						{
							try
							{
								var salePriceSt = body[headDic["AAL"]];
								decimal salePrice = decimal.Parse(salePriceSt);
								variant.SalePrice = salePrice;
							}
							catch
							{
								if (!isUpdate)
								{
									errorMessage.Add("Invalid Sale Price at row " + row);
								}
							}
						}
						else if (!isUpdate)
						{
							errorMessage.Add("Sale Price field is required");
						}
						#endregion
						#region Validate Sale Price
						if (variant.OriginalPrice < variant.SalePrice)
						{
							variant.OriginalPrice = variant.SalePrice;
						}
						#endregion
						#region Inventory Amount
						if (headDic.ContainsKey("ABJ"))
						{
							try
							{
								string val = body[headDic["ABJ"]];
								if (!string.IsNullOrWhiteSpace(val))
								{
									if (variant.Inventory == null)
									{
										variant.Inventory = new Inventory()
										{
											CreateBy = email,
											CreateOn = currentDt,
											UpdateBy = email,
											UpdateOn = currentDt,
										};
										variant.Inventory.StockType = Constant.STOCK_TYPE[Constant.DEFAULT_STOCK_TYPE];
										if (headDic.ContainsKey("ABO"))
										{
											if (Constant.STOCK_TYPE.ContainsKey(body[headDic["ABO"]]))
											{
												variant.Inventory.StockType = Constant.STOCK_TYPE[body[headDic["ABO"]]];
											}
										}
									}
									if (!isUpdate)
									{
										variant.Inventory.Quantity = variant.Inventory.Quantity + int.Parse(val);
									}
								}
							}
							catch
							{
								errorMessage.Add("Invalid Inventory Amount at row " + row);
							}
						}
						if (headDic.ContainsKey("ABK"))
						{
							try
							{
								string val = body[headDic["ABK"]];
								if (!string.IsNullOrWhiteSpace(val))
								{
									if (variant.Inventory == null)
									{
										variant.Inventory = new Inventory()
										{
											CreateBy = email,
											CreateOn = currentDt,
											UpdateBy = email,
											UpdateOn = currentDt,
										};
										variant.Inventory.StockType = Constant.STOCK_TYPE[Constant.DEFAULT_STOCK_TYPE];
										if (headDic.ContainsKey("ABO"))
										{
											if (Constant.STOCK_TYPE.ContainsKey(body[headDic["ABO"]]))
											{
												variant.Inventory.StockType = Constant.STOCK_TYPE[body[headDic["ABO"]]];
											}
										}
									}
									variant.Inventory.Quantity = variant.Inventory.Quantity + int.Parse(val);
								}
							}
							catch
							{
								errorMessage.Add("Invalid Inventory Amount at row " + row);
							}
						}
						#endregion
						#region Safety Stock Amount
						if (headDic.ContainsKey("ABL"))
						{
							try
							{
								string val = body[headDic["ABL"]];
								if (!string.IsNullOrWhiteSpace(val))
								{
									if (variant.Inventory == null)
									{
										variant.Inventory = new Inventory()
										{
											CreateBy = email,
											CreateOn = currentDt,
											UpdateBy = email,
											UpdateOn = currentDt,
										};
										variant.Inventory.StockType = Constant.STOCK_TYPE[Constant.DEFAULT_STOCK_TYPE];
										if (headDic.ContainsKey("ABO"))
										{
											if (Constant.STOCK_TYPE.ContainsKey(body[headDic["ABO"]]))
											{
												variant.Inventory.StockType = Constant.STOCK_TYPE[body[headDic["ABO"]]];
											}
										}
									}
									variant.Inventory.SafetyStockSeller = int.Parse(val);
								}
							}
							catch
							{
								errorMessage.Add("Invalid Safety Stock Amount at row " + row);
							}
						}
						#endregion
						#region Minimum Quantity Allowed in Cart
						if (headDic.ContainsKey("ABM"))
						{
							try
							{
								string val = body[headDic["ABM"]];
								if (!string.IsNullOrWhiteSpace(val))
								{
									if (variant.Inventory == null)
									{
										variant.Inventory = new Inventory()
										{
											CreateBy = email,
											CreateOn = currentDt,
											UpdateBy = email,
											UpdateOn = currentDt
										};
										variant.Inventory.StockType = Constant.STOCK_TYPE[Constant.DEFAULT_STOCK_TYPE];
										if (headDic.ContainsKey("ABO"))
										{
											if (Constant.STOCK_TYPE.ContainsKey(body[headDic["ABO"]]))
											{
												variant.Inventory.StockType = Constant.STOCK_TYPE[body[headDic["ABO"]]];
											}
										}

									}
									variant.Inventory.MinQtyAllowInCart = int.Parse(val);
								}
							}
							catch
							{
								errorMessage.Add("Invalid Minimum Quantity Allowed in Cart at row " + row);
							}
						}
						#endregion
						#region Maximum Quantity Allowed in Cart
						if (headDic.ContainsKey("ABN"))
						{
							try
							{
								string val = body[headDic["ABN"]];
								if (!string.IsNullOrWhiteSpace(val))
								{
									if (variant.Inventory == null)
									{
										variant.Inventory = new Inventory()
										{
											CreateBy = email,
											CreateOn = currentDt,
											UpdateBy = email,
											UpdateOn = currentDt
										};
										variant.Inventory.StockType = Constant.STOCK_TYPE[Constant.DEFAULT_STOCK_TYPE];
										if (headDic.ContainsKey("ABO"))
										{
											if (Constant.STOCK_TYPE.ContainsKey(body[headDic["ABO"]]))
											{
												variant.Inventory.StockType = Constant.STOCK_TYPE[body[headDic["ABO"]]];
											}
										}

									}
									variant.Inventory.MaxQtyAllowInCart = int.Parse(val);
								}
							}
							catch
							{
								errorMessage.Add("Invalid Maximum Quantity Allowed in Cart at row " + row);
							}
						}
						#endregion
						#region Maximum Quantity Allow for Preorder
						if (headDic.ContainsKey("ABP"))
						{
							try
							{
								string val = body[headDic["ABP"]];
								if (!string.IsNullOrWhiteSpace(val))
								{
									if (variant.Inventory == null)
									{
										variant.Inventory = new Inventory()
										{
											CreateBy = email,
											CreateOn = currentDt,
											UpdateBy = email,
											UpdateOn = currentDt
										};
										variant.Inventory.StockType = Constant.STOCK_TYPE[Constant.DEFAULT_STOCK_TYPE];
										if (headDic.ContainsKey("ABO"))
										{
											if (Constant.STOCK_TYPE.ContainsKey(body[headDic["ABO"]]))
											{
												variant.Inventory.StockType = Constant.STOCK_TYPE[body[headDic["ABO"]]];
											}
										}

									}
									variant.Inventory.MaxQtyPreOrder = int.Parse(val);
								}
							}
							catch
							{
								errorMessage.Add("Invalid Maximum Quantity Allow for Preorder at row " + row);
							}
						}
						#endregion
						#region  On Hold
						if (headDic.ContainsKey("ADN"))
						{
							try
							{
								string val = body[headDic["ADN"]];
								if (!string.IsNullOrWhiteSpace(val))
								{
									if (variant.Inventory == null)
									{
										variant.Inventory = new Inventory()
										{
											CreateBy = email,
											CreateOn = currentDt,
											UpdateBy = email,
											UpdateOn = currentDt
										};
										variant.Inventory.StockType = Constant.STOCK_TYPE[Constant.DEFAULT_STOCK_TYPE];
										if (headDic.ContainsKey("ABO"))
										{
											if (Constant.STOCK_TYPE.ContainsKey(body[headDic["ABO"]]))
											{
												variant.Inventory.StockType = Constant.STOCK_TYPE[body[headDic["ABO"]]];
											}
										}

									}
									variant.Inventory.OnHold = int.Parse(val);
								}
							}
							catch
							{
								errorMessage.Add("Invalid Maximum Quantity Allow for Preorder at row " + row);
							}
						}
						#endregion
						#region  Reserve
						if (headDic.ContainsKey("ADO"))
						{
							try
							{
								string val = body[headDic["ADO"]];
								if (!string.IsNullOrWhiteSpace(val))
								{
									if (variant.Inventory == null)
									{
										variant.Inventory = new Inventory()
										{
											CreateBy = email,
											CreateOn = currentDt,
											UpdateBy = email,
											UpdateOn = currentDt
										};
										variant.Inventory.StockType = Constant.STOCK_TYPE[Constant.DEFAULT_STOCK_TYPE];
										if (headDic.ContainsKey("ABO"))
										{
											if (Constant.STOCK_TYPE.ContainsKey(body[headDic["ABO"]]))
											{
												variant.Inventory.StockType = Constant.STOCK_TYPE[body[headDic["ABO"]]];
											}
										}

									}
									variant.Inventory.Reserve = int.Parse(val);
								}
							}
							catch
							{
								errorMessage.Add("Invalid Maximum Quantity Allow for Preorder at row " + row);
							}
						}
						#endregion
						#region  Defect
						if (headDic.ContainsKey("ADP"))
						{
							try
							{
								string val = body[headDic["ADP"]];
								if (!string.IsNullOrWhiteSpace(val))
								{
									if (variant.Inventory == null)
									{
										variant.Inventory = new Inventory()
										{
											CreateBy = email,
											CreateOn = currentDt,
											UpdateBy = email,
											UpdateOn = currentDt
										};
										variant.Inventory.StockType = Constant.STOCK_TYPE[Constant.DEFAULT_STOCK_TYPE];
										if (headDic.ContainsKey("ABO"))
										{
											if (Constant.STOCK_TYPE.ContainsKey(body[headDic["ABO"]]))
											{
												variant.Inventory.StockType = Constant.STOCK_TYPE[body[headDic["ABO"]]];
											}
										}

									}
									variant.Inventory.Defect = int.Parse(val);
								}
							}
							catch
							{
								errorMessage.Add("Invalid Maximum Quantity Allow for Preorder at row " + row);
							}
						}
						#endregion
						#region Stock Type
						if (headDic.ContainsKey("ABO"))
						{
							if (Constant.STOCK_TYPE.ContainsKey(body[headDic["ABO"]]))
							{
								variant.Inventory.StockType = Constant.STOCK_TYPE[body[headDic["ABO"]]];
							}
						}
						#endregion
						#region inititae inventory
						if (!isUpdate && variant.Inventory == null)
						{
							variant.Inventory = new Inventory()
							{
								StockType = Constant.STOCK_TYPE[Constant.DEFAULT_STOCK_TYPE],
								CreateBy = email,
								CreateOn = currentDt,
								UpdateBy = email,
								UpdateOn = currentDt
							};
						}
						#endregion
						if (variant.DefaultVariant || isNew)
						{
							group.ProductStages.ToList().ForEach(f => f.DefaultVariant = false);
							variant.DefaultVariant = true;
							#region Relate Product
							if (headDic.ContainsKey("ACM"))
							{
								var tmpRelateProduct = body[headDic["ACM"]].Split(',');
								foreach (var pro in tmpRelateProduct)
								{
									if (string.IsNullOrWhiteSpace(pro))
									{
										continue;
									}
									var child = relatedProduct.Where(w => w.Pid.Equals(pro.Trim())).Select(s => s.ProductId).SingleOrDefault();
									if (child != 0)
									{
										if (!group.ProductStageRelateds1.Any(a => a.Child == child))
										{
											group.ProductStageRelateds1.Add(new ProductStageRelated()
											{
												Child = child,
												CreateBy = email,
												CreateOn = currentDt,
												UpdateBy = email,
												UpdateOn = currentDt,
												ShopId = shopId,
											});
										}
									}
									else
									{
										errorMessage.Add("Cannot find related product " + pro + " at row " + row);
									}
								}
							}
							#endregion
							#region Brand 
							if (headDic.ContainsKey("AAK"))
							{
								var name = body[headDic["AAK"]].Trim().ToLower().Replace(' ', '_');
								var brandId = brands.Where(w => w.BrandNameEn.Equals(name)).Select(s => s.BrandId).FirstOrDefault();
								if (brandId != 0)
								{
									group.BrandId = brandId;
								}
								else if(!isUpdate)
								{
									errorMessage.Add("Invalid Brand Name at row " + row);
								}
							}
							else if (!isUpdate)
							{
								errorMessage.Add("Brand Name column is required");
							}
							#endregion
							#region Global category
							if (headDic.ContainsKey("ACG"))
							{
								try
								{
									var catIdSt = body[headDic["ACG"]];
									if (!string.IsNullOrWhiteSpace(catIdSt))
									{
										int catId = int.Parse(catIdSt);
										var cat = globalCatId.Where(w => w.CategoryId == catId).Select(s => s.CategoryId).FirstOrDefault();
										if (cat != 0)
										{
											group.GlobalCatId = cat;
										}
										else
										{
											throw new Exception();
										}
									}
									else if (!isUpdate)
									{
										errorMessage.Add(string.Concat(guidance.Where(w => w.MapName.Equals("ACG")).Select(s => s.HeaderName).Single(), " is required at row " + row));
									}
								}
								catch (Exception)
								{
									errorMessage.Add(string.Concat("Invalid ",guidance.Where(w => w.MapName.Equals("ACG")).Select(s => s.HeaderName).Single(), " at row " + row));
								}
							}
							else if (!isUpdate)
							{
								errorMessage.Add(string.Concat(guidance.Where(w => w.MapName.Equals("ACG")).Select(s => s.HeaderName).Single(), " column is required"));
							}
							if (headDic.ContainsKey("ACH"))
							{
								try
								{
									var catIdSt = body[headDic["ACH"]];
									if (!string.IsNullOrWhiteSpace(catIdSt))
									{
										int catId = int.Parse(catIdSt);
										var cat = globalCatId.Where(w => w.CategoryId == catId).Select(s => s.CategoryId).FirstOrDefault();
										if (cat != 0)
										{
											if (!group.ProductStageGlobalCatMaps.Any(a => a.CategoryId == cat))
											{
												group.ProductStageGlobalCatMaps.Add(new ProductStageGlobalCatMap()
												{
													CategoryId = cat,
													CreateBy = email,
													CreateOn = currentDt,
													UpdateBy = email,
													UpdateOn = currentDt,
												});
											}
										}
										else
										{
											throw new Exception();
										}
									}
								}
								catch (Exception)
								{
									errorMessage.Add("Invalid 1st Alternative Global Category at row " + row);
								}
							}
							if (headDic.ContainsKey("ACI"))
							{
								try
								{
									var catIdSt = body[headDic["ACI"]];
									if (!string.IsNullOrWhiteSpace(catIdSt))
									{
										int catId = int.Parse(catIdSt);
										var cat = globalCatId.Where(w => w.CategoryId == catId).Select(s => s.CategoryId).FirstOrDefault();
										if (cat != 0)
										{
											if (!group.ProductStageGlobalCatMaps.Any(a => a.CategoryId == cat))
											{
												group.ProductStageGlobalCatMaps.Add(new ProductStageGlobalCatMap()
												{
													CategoryId = cat,
													CreateBy = email,
													CreateOn = currentDt,
													UpdateBy = email,
													UpdateOn = currentDt,
												});
											}
										}
										else
										{
											throw new Exception();
										}
									}
								}
								catch (Exception)
								{
									errorMessage.Add("Invalid 2nd Alternative Global Category at row " + row);
								}
							}

							#endregion
							#region Local Category
							if (headDic.ContainsKey("ACJ"))
							{
								try
								{
									var catIdSt = body[headDic["ACJ"]];
									if (!string.IsNullOrWhiteSpace(catIdSt))
									{
										int catId = int.Parse(catIdSt);
										var cat = localCatId.Where(w => w.CategoryId == catId).Select(s => s.CategoryId).FirstOrDefault();
										if (cat != 0)
										{
											group.LocalCatId = cat;
										}
										else
										{
											throw new Exception();
										}
									}
								}
								catch
								{
									errorMessage.Add("Invalid Local Category ID at row " + row);
								}
							}
							if (headDic.ContainsKey("ACK"))
							{
								try
								{
									var catIdSt = body[headDic["ACK"]];
									if (!string.IsNullOrWhiteSpace(catIdSt))
									{
										int catId = int.Parse(catIdSt);
										var cat = localCatId.Where(w => w.CategoryId == catId).Select(s => s.CategoryId).FirstOrDefault();
										if (cat != 0)
										{
											if (!group.ProductStageLocalCatMaps.Any(a => a.CategoryId == cat))
											{
												group.ProductStageLocalCatMaps.Add(new ProductStageLocalCatMap()
												{
													CategoryId = cat,
													CreateBy = email,
													CreateOn = currentDt,
													UpdateBy = email,
													UpdateOn = currentDt,
												});
											}
										}
										else
										{
											throw new Exception();
										}
									}
								}
								catch (Exception)
								{
									errorMessage.Add("Invalid 1st Alternative Local Category at row " + row);
								}
							}
							if (headDic.ContainsKey("ACL"))
							{
								try
								{
									var catIdSt = body[headDic["ACL"]];
									if (!string.IsNullOrWhiteSpace(catIdSt))
									{
										int catId = int.Parse(catIdSt);
										var cat = localCatId.Where(w => w.CategoryId == catId).Select(s => s.CategoryId).FirstOrDefault();
										if (cat != 0)
										{
											if (!group.ProductStageLocalCatMaps.Any(a => a.CategoryId == cat))
											{
												group.ProductStageLocalCatMaps.Add(new ProductStageLocalCatMap()
												{
													CategoryId = cat,
													CreateBy = email,
													CreateOn = currentDt,
													UpdateBy = email,
													UpdateOn = currentDt,
												});
											}
										}
										else
										{
											throw new Exception();
										}
									}
								}
								catch (Exception)
								{
									errorMessage.Add("Invalid 2nd Alternative Local Category at row " + row);
								}
							}
							#endregion
							#region Master Variant

							ProductStage masterVariant = group.ProductStages.Where(w => w.IsVariant == false).SingleOrDefault();
							if (masterVariant == null)
							{
								masterVariant = new ProductStage();
								group.ProductStages.Add(masterVariant);
							}
							masterVariant.ProductStageAttributes.Clear();
							masterVariant.ShopId = variant.ShopId;
							masterVariant.Status = variant.Status;
							masterVariant.CreateBy = variant.CreateBy;
							masterVariant.CreateOn = variant.CreateOn;
							masterVariant.UpdateBy = variant.UpdateBy;
							masterVariant.UpdateOn = variant.UpdateOn;
							masterVariant.Visibility = variant.Visibility;
							masterVariant.IsVariant = false;
							masterVariant.FeatureImgUrl = variant.FeatureImgUrl;
							masterVariant.ImageCount = variant.ImageCount;
							masterVariant.MaxiQtyAllowed = variant.MaxiQtyAllowed;
							masterVariant.MiniQtyAllowed = variant.MiniQtyAllowed;
							masterVariant.UnitPrice = variant.UnitPrice;
							masterVariant.DimensionUnit = variant.DimensionUnit;
							masterVariant.WeightUnit = variant.WeightUnit;
							masterVariant.GlobalBoostWeight = variant.GlobalBoostWeight;
							masterVariant.Display = variant.Display;
							masterVariant.VariantCount = variant.VariantCount;
							masterVariant.PurchasePrice = variant.PurchasePrice;
							masterVariant.SalePrice = variant.SalePrice;
							masterVariant.OriginalPrice = variant.OriginalPrice;
							masterVariant.DefaultVariant = false;
							masterVariant.Bu = variant.Bu;
							masterVariant.DeliveryFee = variant.DeliveryFee;
							masterVariant.EffectiveDatePromotion = variant.EffectiveDatePromotion;
							masterVariant.ExpireDatePromotion = variant.ExpireDatePromotion;
							masterVariant.IsSell = variant.IsSell;
							masterVariant.JDADept = variant.JDADept;
							masterVariant.JDASubDept = variant.JDASubDept;
							masterVariant.NewArrivalDate = variant.NewArrivalDate;
							masterVariant.PromotionPrice = variant.PromotionPrice;
							masterVariant.OldPid = variant.OldPid;
							masterVariant.ProductNameEn = variant.ProductNameEn;
							masterVariant.ProductNameTh = variant.ProductNameTh;
							masterVariant.ProdTDNameEn = variant.ProdTDNameEn;
							masterVariant.ProdTDNameTh = variant.ProdTDNameTh;
							masterVariant.Sku = variant.Sku;
							masterVariant.Upc = variant.Upc;
							masterVariant.SaleUnitEn = variant.SaleUnitEn;
							masterVariant.SaleUnitTh = variant.SaleUnitTh;
							masterVariant.IsVat = variant.IsVat;
							masterVariant.DescriptionFullEn = variant.DescriptionFullEn;
							masterVariant.DescriptionFullTh = variant.DescriptionFullTh;
							masterVariant.MobileDescriptionEn = variant.MobileDescriptionEn;
							masterVariant.MobileDescriptionTh = variant.MobileDescriptionTh;
							masterVariant.DescriptionShortEn = variant.DescriptionShortEn;
							masterVariant.DescriptionShortTh = variant.DescriptionShortTh;
							masterVariant.KillerPoint1En = variant.KillerPoint1En;
							masterVariant.KillerPoint1Th = variant.KillerPoint1Th;
							masterVariant.KillerPoint2En = variant.KillerPoint2En;
							masterVariant.KillerPoint2Th = variant.KillerPoint2Th;
							masterVariant.KillerPoint3En = variant.KillerPoint3En;
							masterVariant.KillerPoint3Th = variant.KillerPoint3Th;
							masterVariant.IsHasExpiryDate = variant.IsHasExpiryDate;
							masterVariant.ExpressDelivery = variant.ExpressDelivery;
							masterVariant.SeoEn = variant.SeoEn;
							masterVariant.SeoTh = variant.SeoTh;
							masterVariant.MetaTitleEn = variant.MetaTitleEn;
							masterVariant.MetaTitleTh = variant.MetaTitleTh;
							masterVariant.MetaDescriptionEn = variant.MetaDescriptionEn;
							masterVariant.MetaDescriptionTh = variant.MetaDescriptionTh;
							masterVariant.MetaKeyEn = variant.MetaKeyEn;
							masterVariant.MetaKeyTh = variant.MetaKeyTh;
							masterVariant.UrlKey = variant.UrlKey;
							masterVariant.Installment = variant.Installment;
							masterVariant.PrepareDay = variant.PrepareDay;
							masterVariant.PrepareMon = variant.PrepareMon;
							masterVariant.PrepareTue = variant.PrepareTue;
							masterVariant.PrepareWed = variant.PrepareWed;
							masterVariant.PrepareThu = variant.PrepareThu;
							masterVariant.PrepareFri = variant.PrepareFri;
							masterVariant.PrepareSat = variant.PrepareSat;
							masterVariant.PrepareSun = variant.PrepareSun;
							masterVariant.LimitIndividualDay = variant.LimitIndividualDay;
							masterVariant.Length = variant.Length;
							masterVariant.Height = variant.Height;
							masterVariant.Width = variant.Width;
							masterVariant.Weight = variant.Weight;
							masterVariant.BoostWeight = variant.BoostWeight;

							if (variant.Inventory != null)
							{
								if (masterVariant.Inventory == null)
								{
									masterVariant.Inventory = new Inventory()
									{
										CreateBy = variant.Inventory.CreateBy,
										CreateOn = variant.Inventory.CreateOn,
										Defect = variant.Inventory.Defect,
										Quantity = variant.Inventory.Quantity,
										Reserve = variant.Inventory.Reserve,
										OnHold = variant.Inventory.OnHold,
										SafetyStockAdmin = variant.Inventory.SafetyStockAdmin,
										SafetyStockSeller = variant.Inventory.SafetyStockSeller,
										UseDecimal = variant.Inventory.UseDecimal,
										UpdateBy = variant.Inventory.UpdateBy,
										UpdateOn = variant.Inventory.UpdateOn,
										MaxQtyAllowInCart = variant.Inventory.MaxQtyAllowInCart,
										MaxQtyPreOrder = variant.Inventory.MaxQtyPreOrder,
										MinQtyAllowInCart = variant.Inventory.MinQtyAllowInCart,
										StockType = variant.Inventory.StockType
									};
								}
								else
								{
									masterVariant.Inventory.CreateBy = variant.Inventory.CreateBy;
									masterVariant.Inventory.CreateOn = variant.Inventory.CreateOn;
									masterVariant.Inventory.Defect = variant.Inventory.Defect;
									masterVariant.Inventory.Quantity = variant.Inventory.Quantity;
									masterVariant.Inventory.Reserve = variant.Inventory.Reserve;
									masterVariant.Inventory.OnHold = variant.Inventory.OnHold;
									masterVariant.Inventory.SafetyStockAdmin = variant.Inventory.SafetyStockAdmin;
									masterVariant.Inventory.SafetyStockSeller = variant.Inventory.SafetyStockSeller;
									masterVariant.Inventory.UseDecimal = variant.Inventory.UseDecimal;
									masterVariant.Inventory.UpdateBy = variant.Inventory.UpdateBy;
									masterVariant.Inventory.UpdateOn = variant.Inventory.UpdateOn;
									masterVariant.Inventory.MaxQtyAllowInCart = variant.Inventory.MaxQtyAllowInCart;
									masterVariant.Inventory.MaxQtyPreOrder = variant.Inventory.MaxQtyPreOrder;
									masterVariant.Inventory.MinQtyAllowInCart = variant.Inventory.MinQtyAllowInCart;
									masterVariant.Inventory.StockType = variant.Inventory.StockType;
								}
							}

							#endregion
							#region More Detail
							if (headDic.ContainsKey("ABI"))
							{
								var tmpTag = body[headDic["ABI"]].Split(',');
								foreach (var tag in tmpTag)
								{
									if (string.IsNullOrWhiteSpace(tag))
									{
										continue;
									}
									var insertTag = tag.Trim();
									if(insertTag.Length > 30)
									{
										errorMessage.Add(string.Concat("Each tag cannot be more than 30 characters"));
										break;
									}
									if (!group.ProductStageTags.Any(a => a.Tag.Equals(insertTag)))
									{
										if (group.ProductStageTags.Count() >= 30)
										{
											errorMessage.Add(string.Concat("Can have only 30 tags for each product"));
											break;
										}
										group.ProductStageTags.Add(new ProductStageTag()
										{
											Tag = insertTag,
											CreateBy = email,
											CreateOn = currentDt,
											UpdateBy = email,
											UpdateOn = currentDt,
										});
									}
								}
							}
							if (headDic.ContainsKey("ADB"))
							{
								group.NewArrivalDate = variant.NewArrivalDate;
							}
							if(group.NewArrivalDate == null)
							{
								group.NewArrivalDate = currentDt;
							}
							//group.EffectiveDate = request.EffectiveDate.HasValue ? request.EffectiveDate.Value : currentDt;
							//group.ExpireDate = request.ExpireDate.HasValue && request.ExpireDate.Value.CompareTo(group.EffectiveDate) >= 0 ? request.ExpireDate.Value : group.EffectiveDate.AddYears(Constant.DEFAULT_ADD_YEAR);
							var tmpDate = Validation.ValidateCSVDatetimeColumn(headDic, body, "ACY", guidance, errorMessage, row);
							group.EffectiveDate = tmpDate.HasValue ? tmpDate.Value : currentDt;
							tmpDate = Validation.ValidateCSVDatetimeColumn(headDic, body, "ACZ", guidance, errorMessage, row);
							group.ExpireDate = tmpDate.HasValue ? tmpDate.Value : group.EffectiveDate.AddYears(Constant.DEFAULT_ADD_YEAR);
							if (group.EffectiveDate.CompareTo(group.ExpireDate) > 0)
							{
								errorMessage.Add(string.Concat("Effective Date must be earlier than Expire Date at row ", row));
							}
							group.Remark = Validation.ValidateCSVStringColumn(headDic, body, "ADH", guidance, false, 5000, errorMessage, row, string.Empty);
							if (headDic.ContainsKey("ADC"))
							{
								group.IsNew = string.Equals(body[headDic["ADC"]], "yes", StringComparison.OrdinalIgnoreCase) ? true : false;
							}
							if (headDic.ContainsKey("ADD"))
							{
								group.IsClearance = string.Equals(body[headDic["ADD"]], "yes", StringComparison.OrdinalIgnoreCase) ? true : false;
							}
							if (headDic.ContainsKey("ADE"))
							{
								group.IsBestSeller = string.Equals(body[headDic["ADE"]], "yes", StringComparison.OrdinalIgnoreCase) ? true : false;
							}
							if (headDic.ContainsKey("ADF"))
							{
								group.IsOnlineExclusive = string.Equals(body[headDic["ADF"]], "yes", StringComparison.OrdinalIgnoreCase) ? true : false;
							}
							if (headDic.ContainsKey("ADG"))
							{
								group.IsOnlyAt = string.Equals(body[headDic["ADG"]], "yes", StringComparison.OrdinalIgnoreCase) ? true : false;
							}
							group.GiftWrap = headDic.ContainsKey("ADG") && string.Equals(body[headDic["AAV"]], "yes", StringComparison.OrdinalIgnoreCase) ? Constant.STATUS_YES : Constant.STATUS_NO;

							#endregion
							#region Default Attribute
							foreach (var attr in defaultAttribute)
							{
								if (headDic.ContainsKey(attr.AttributeNameEn))
								{
									var valueEn = Validation.ValidateCSVStringColumn(headDic, body, attr.AttributeNameEn, guidance, false
												, Constant.DATA_TYPE_HTML.Equals(attr.DataType) ? 50000 : 255, errorMessage, row);
									if (string.IsNullOrWhiteSpace(valueEn))
									{
										continue;
									}
									var spit = valueEn.Split(';');
									var valueTh = valueEn;
									if (spit.Count() > 0)
									{
										valueEn = spit[0].Trim();
									}
									if (spit.Count() > 1)
									{
										valueTh = spit[1].Trim();
									}
									int? valueId = null;
									string htmlValue = string.Empty;
									bool isValue = false;
									#region List
									if (Constant.DATA_TYPE_LIST.Equals(attr.DataType))
									{
										valueId = attr.AttributeValue.Where(w => w.AttributeValueEn.Equals(valueEn)).Select(s => s.AttributeValueId).FirstOrDefault();
										if (valueId == 0)
										{
											throw new Exception("Invalid attribute value " + valueEn + " in attribute " + attr.AttributeNameEn);
										}
										valueEn = string.Concat("((", valueId, "))");
										valueTh = valueEn;
										isValue = true;
									}
									#endregion
									#region CheckBox
									if (Constant.DATA_TYPE_CHECKBOX.Equals(attr.DataType))
									{
										var tmpValue = valueEn.Split(',');
										tmpValue = tmpValue.Distinct().ToArray();
										foreach (var v in tmpValue)
										{
											var tmpDefValue = v.Trim();
											valueId = attr.AttributeValue.Where(w => w.AttributeValueEn.Equals(tmpDefValue)).Select(s => s.AttributeValueId).FirstOrDefault();
											if (valueId == 0)
											{
												throw new Exception("Invalid attribute value " + valueEn + " in attribute " + attr.AttributeNameEn);
											}
											var checkValue = string.Concat("((", valueId, "))");
											var tmpVariant = group.ProductStages
													.Where(w => w.IsVariant == false
													 && !w.ProductStageAttributes.Any(a => a.AttributeId == attr.AttributeId && a.ValueEn.Equals(checkValue))
													 )
													.SingleOrDefault();
											if (tmpVariant != null)
											{
												tmpVariant.ProductStageAttributes.Add(new ProductStageAttribute()
												{
													AttributeId = attr.AttributeId,
													ValueEn = checkValue,
													ValueTh = checkValue,
													CheckboxValue = true,
													AttributeValueId = valueId,
													IsAttributeValue = true,
													CreateBy = email,
													CreateOn = currentDt,
													UpdateBy = email,
													UpdateOn = currentDt,
													HtmlBoxValue = htmlValue,
												});
											}
										}
										continue;
									}
									#endregion
									#region HtmlBox
									if (Constant.DATA_TYPE_LIST.Equals(attr.DataType))
									{
										htmlValue = valueEn;
										valueEn = string.Concat(
											Constant.ATTRIBUTE_VALUE_MAP_PREFIX,
											attr.AttributeId,
											Constant.ATTRIBUTE_VALUE_MAP_SURFIX);
										valueTh = valueEn;
										isValue = false;
									}
									#endregion
									var tmpMasterVariant = group.ProductStages
													.Where(w => w.IsVariant == false
														&& !w.ProductStageAttributes.Any(a => a.AttributeId == attr.AttributeId && a.ValueEn.Equals(valueEn)))
													.SingleOrDefault();
									if (tmpMasterVariant != null)
									{
										tmpMasterVariant.ProductStageAttributes.Add(new ProductStageAttribute()
										{
											AttributeId = attr.AttributeId,
											HtmlBoxValue = htmlValue,
											ValueEn = valueEn,
											ValueTh = valueTh,
											CheckboxValue = false,
											IsAttributeValue = isValue,
											AttributeValueId = valueId,
											CreateBy = email,
											CreateOn = currentDt,
											UpdateBy = email,
											UpdateOn = currentDt,
										});
									}
								}
							}
							#endregion
							#region Shipping 
							if (headDic.ContainsKey("ABR"))
							{
								var shippingId = shipping.Where(w => w.ShippingMethodEn.Equals(body[headDic["ABR"]])).Select(s => s.ShippingId).FirstOrDefault();
								if (shippingId != 0)
								{
									group.ShippingId = shippingId;
								}
							}
							if(group.ShippingId == 0)
							{
								group.ShippingId = Constant.DEFAULT_SHIPPING_ID;
							}
							#endregion
						}
						#region Attribute Set
						if (headDic.ContainsKey("ADI"))
						{
							try
							{
								string val = body[headDic["ADI"]];
								if (!string.IsNullOrWhiteSpace(val))
								{
									var name = val.Trim().ToLower().Replace(' ', '_');
									var attrSet = attributeSet.Where(w => w.AttributeSetNameEn.Equals(name)).SingleOrDefault();
									if (attrSet == null)
									{
										throw new Exception("Attribute set " + val + " not found in database at row " + row);
									}
									group.AttributeSetId = attrSet.AttributeSetId;
									var variant1 = Validation.ValidateCSVStringColumn(headDic, body, "ADJ", guidance, false, 255, errorMessage, row);
									var variant2 = Validation.ValidateCSVStringColumn(headDic, body, "ADK", guidance, false, 255, errorMessage, row);
									bool isInvalid = false;
									if (!string.IsNullOrWhiteSpace(variant1) && !attrSet.Attribute.Any(a => a.AttributeNameEn.Equals(variant1)))
									{
										errorMessage.Add("Cannot find attribute " + variant1 + " in attribute set " + attrSet.AttributeSetNameEn + " at row " + row);
										isInvalid = true;
									}
									if (!string.IsNullOrWhiteSpace(variant2) && !attrSet.Attribute.Any(a => a.AttributeNameEn.Equals(variant2)))
									{
										errorMessage.Add("Cannot find attribute " + variant2 + " in attribute set " + attrSet.AttributeSetNameEn + " at row " + row);
										isInvalid = true;
									}
									if (!string.IsNullOrEmpty(variant1) && variant1.Equals(variant2))
									{
										errorMessage.Add("Attribute variant 1 " + variant1 + " cannot be same with attribute variant 2 " + variant2 + " at row " + row);
										isInvalid = true;
									}
									if (isInvalid)
									{
										continue;
									}
									foreach (var attr in attrSet.Attribute)
									{
										if (headDic.ContainsKey(attr.AttributeNameEn))
										{
											var valueEn = Validation.ValidateCSVStringColumn(headDic, body, attr.AttributeNameEn, guidance, false
												, Constant.DATA_TYPE_HTML.Equals(attr.DataType) ? 50000 : 255, errorMessage, row);
											if (string.IsNullOrWhiteSpace(valueEn))
											{
												continue;
											}
											var valueTh = valueEn;
											if (!Constant.DATA_TYPE_HTML.Equals(attr.DataType))
											{
												var spit = valueEn.Split(';');

												if (spit.Count() > 0)
												{
													valueEn = spit[0].Trim();
												}
												if (spit.Count() > 1)
												{
													valueTh = spit[1].Trim();
												}
											}
											string htmlValue = string.Empty;
											int? valueId = null;
											bool isValue = false;
											#region List
											if (Constant.DATA_TYPE_LIST.Equals(attr.DataType))
											{
												valueId = attr.AttributeValue.Where(w => w.AttributeValueEn.Equals(valueEn)).Select(s => s.AttributeValueId).FirstOrDefault();
												if (valueId == 0)
												{
													throw new Exception("Invalid attribute value " + valueEn + " in attribute " + attr.AttributeNameEn);
												}
												valueEn = string.Concat("((", valueId, "))");
												valueTh = valueEn;
												isValue = true;
											}
											#endregion
											#region CheckBox
											if (Constant.DATA_TYPE_CHECKBOX.Equals(attr.DataType))
											{
												var tmpValue = valueEn.Split(',');
												tmpValue = tmpValue.Distinct().ToArray();
												foreach (var v in tmpValue)
												{
													var tmpDefValue = v.Trim();
													valueId = attr.AttributeValue.Where(w => w.AttributeValueEn.Equals(tmpDefValue)).Select(s => s.AttributeValueId).FirstOrDefault();
													if (valueId == 0)
													{
														throw new Exception("Invalid attribute value " + valueEn + " in attribute " + attr.AttributeNameEn);
													}
													if (string.Equals(attr.AttributeNameEn, variant1, StringComparison.OrdinalIgnoreCase)
														|| string.Equals(attr.AttributeNameEn, variant2, StringComparison.OrdinalIgnoreCase))
													{
														errorMessage.Add("Checkbox cannot be variant");
														continue;
													}
													else
													{

														var checkValue = string.Concat("((", valueId, "))");
														var tmpMasterVariant = group.ProductStages
																.Where(w => w.IsVariant == false
																	&& !w.ProductStageAttributes.Any(a => a.AttributeId == attr.AttributeId && a.ValueEn.Equals(checkValue))
																 )
																.SingleOrDefault();
														if (tmpMasterVariant != null)
														{
															tmpMasterVariant.ProductStageAttributes.Add(new ProductStageAttribute()
															{
																AttributeId = attr.AttributeId,
																ValueEn = checkValue,
																ValueTh = checkValue,
																CheckboxValue = true,
																IsAttributeValue = true,
																AttributeValueId = valueId,
																CreateBy = email,
																CreateOn = currentDt,
																UpdateBy = email,
																UpdateOn = currentDt,
																HtmlBoxValue = htmlValue,
															});
														}
													}
												}
												continue;
											}
											#endregion
											#region HtmlBox
											if (Constant.DATA_TYPE_HTML.Equals(attr.DataType))
											{
												htmlValue = valueEn;
												valueEn = string.Concat("((", attr.AttributeId, "))");
												valueTh = valueEn;
												isValue = false;
											}
											#endregion
											if (string.Equals(attr.AttributeNameEn, variant1, StringComparison.OrdinalIgnoreCase))
											{
												if (!attr.VariantStatus && Constant.DATA_TYPE_STRING.Equals(attr.AttributeNameEn))
												{
													errorMessage.Add("Attribute " + attr.AttributeNameEn + " is not variant type at row " + row);
													continue;
												}
												if (!variant.ProductStageAttributes.Any(a => a.AttributeId == attr.AttributeId))
												{
													variant.ProductStageAttributes.Add(new ProductStageAttribute()
													{
														AttributeId = attr.AttributeId,
														ValueEn = valueEn,
														ValueTh = valueTh,
														CheckboxValue = false,
														AttributeValueId = valueId,
														IsAttributeValue = isValue,
														CreateBy = email,
														CreateOn = currentDt,
														UpdateBy = email,
														UpdateOn = currentDt,
														HtmlBoxValue = htmlValue,
													});
												}
											}
											else if (string.Equals(attr.AttributeNameEn, variant2, StringComparison.OrdinalIgnoreCase))
											{
												if (!attr.VariantStatus && Constant.DATA_TYPE_STRING.Equals(attr.AttributeNameEn))
												{
													errorMessage.Add("Attribute " + attr.AttributeNameEn + " is not variant type at row " + row);
													continue;
												}
												if (!variant.ProductStageAttributes.Any(a => a.AttributeId == attr.AttributeId))
												{
													variant.ProductStageAttributes.Add(new ProductStageAttribute()
													{
														AttributeId = attr.AttributeId,
														ValueEn = valueEn,
														ValueTh = valueTh,
														CheckboxValue = false,
														AttributeValueId = valueId,
														IsAttributeValue = isValue,
														CreateBy = email,
														CreateOn = currentDt,
														UpdateBy = email,
														UpdateOn = currentDt,
														HtmlBoxValue = htmlValue,
													});
												}
											}
											else
											{
												if (variant.DefaultVariant || isNew)
												{
													var tmpMasterVariant = group.ProductStages
															.Where(w => w.IsVariant == false
															&& !w.ProductStageAttributes.Any(a => a.AttributeId == attr.AttributeId && a.ValueEn.Equals(valueEn)))
															.SingleOrDefault();
													if (tmpMasterVariant != null)
													{
														tmpMasterVariant.ProductStageAttributes.Add(new ProductStageAttribute()
														{
															AttributeId = attr.AttributeId,
															ValueEn = valueEn,
															ValueTh = valueTh,
															AttributeValueId = valueId,
															CheckboxValue = false,
															IsAttributeValue = isValue,
															CreateBy = email,
															CreateOn = currentDt,
															UpdateBy = email,
															UpdateOn = currentDt,
															HtmlBoxValue = htmlValue,
														});
													}
												}
												
											}
										}
									}
								}
							}
							catch (Exception e)
							{
								errorMessage.Add(e.Message + " at row " + row);
							}
						}
						#endregion
						#region Validate Attribute
						if (variant.ProductStageAttributes != null && variant.ProductStageAttributes.Count > 0)
						{
							var productAttribute = group.ProductStages.Select(s => s.ProductStageAttributes);
							foreach (var v in productAttribute)
							{
								if (variant.ProductStageAttributes
									.All(a => v.Any(aa => a.AttributeId == aa.AttributeId && a.ValueEn.Equals(aa.ValueEn) && a.ValueTh.Equals(aa.ValueTh))))
								{
									errorMessage.Add(string.Concat("Duplicate variant value at row ", row));
								}
							}
							group.ProductStages.Add(variant);
						}
						if (!groupList.ContainsKey(groupId))
						{
							groupList.Add(groupId, group);
						}
						#endregion
						row++;
					}
				}
				return groupList;
			}

		}


		[Route("api/ProductStages/Publish")]
		[HttpPost]
		public HttpResponseMessage PublishProduct(List<ProductStageRequest> request)
		{
			try
			{
				if (request == null || request.Count == 0)
				{
					throw new Exception("Invalid request");
				}
				int take = 100;
				int multiply = 0;
				var email = User.UserRequest().Email;
				var currentDt = SystemHelper.GetCurrentDateTime();
				var defaultAttr = db.Attributes.Where(w => w.Required && w.DefaultAttribute && !Constant.DATA_TYPE_CHECKBOX.Equals(w.DataType)).Select(s => s.AttributeId);
				while (true)
				{
					var ids = request.Where(w => w.ProductId != 0).Select(s => s.ProductId).Skip(take * multiply).Take(take);
					if (ids.Count() == 0)
					{
						break;
					}
					var productList = db.ProductStageGroups.Where(w => true);
					if (User.ShopRequest() != null)
					{
						int shopId = User.ShopRequest().ShopId;
						productList = productList.Where(w => w.ShopId == shopId);
					}
					//productList = productList.Where(w => ids.Any(a => a == w.ProductId)).Include(i => i.ProductStages.Select(s => s.ProductStageAttributes));
					productList = productList.Where(w => ids.Any(a => a == w.ProductId)).Include(i => i.ProductStages);
					foreach (ProductStageRequest rq in request)
					{
						var current = productList.Where(w => w.ProductId.Equals(rq.ProductId)).SingleOrDefault();
						if (current == null)
						{
							throw new Exception(string.Concat("Cannot find product ", rq.ProductId));
						}
						if (!current.Status.Equals(Constant.PRODUCT_STATUS_DRAFT))
						{
							throw new Exception(string.Concat("Only product with draft status and complete Info & Image tab can be published"));
						}
						if (current.InfoFlag == false || current.ImageFlag == false)
						{
							throw new Exception(string.Concat("Product ID ", rq.ProductId, " is not ready for publishing"));
						}
						//if (current.ProductStages.Any(a => string.IsNullOrWhiteSpace(a.ProductNameEn))
						//    || current.ProductStages.Any(a => string.IsNullOrWhiteSpace(a.ProductNameTh))
						//    || current.ProductStages.Any(a => string.IsNullOrWhiteSpace(a.Sku))
						//    || current.BrandId == null)
						//{
						//    var masterVariant = current.ProductStages.Where(w => w.IsVariant == false).FirstOrDefault();
						//    if (defaultAttr != null && defaultAttr.Count() > 0)
						//    {
						//        var defaultIds = masterVariant.ProductStageAttributes.Where(w => !string.IsNullOrWhiteSpace(w.ValueEn)).Select(s => s.AttributeId);
						//        if (!defaultAttr.Any(a => defaultIds.Contains(a)))
						//        {
						//            throw new Exception(string.Concat("Product ID ", rq.ProductId, " is not ready for publishing"));
						//        }
						//    }
						//}
						current.Status = Constant.PRODUCT_STATUS_WAIT_FOR_APPROVAL;
						current.UpdateBy = email;
						current.UpdateOn = currentDt;
						current.ProductStages.ToList().ForEach(e =>
						{
							e.Status = Constant.PRODUCT_STATUS_WAIT_FOR_APPROVAL;
							e.UpdateBy = email;
							e.UpdateOn = currentDt;
						});
					}
					multiply++;
				}
				Util.DeadlockRetry(db.SaveChanges, "ProductStage");
				return Request.CreateResponse(HttpStatusCode.OK, "Published success");
			}
			catch (Exception e)
			{
				return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
			}
		}


		[Route("api/ProductStages/JdaProduct")]
		[HttpPost]
		[OverrideAuthentication, OverrideAuthorization]
		public HttpResponseMessage AddProductJda(List<JdaRequest> request)
		{
			try
			{
				if (request == null)
				{
					throw new Exception("Invalid request");
				}
				var email = "jda@col.co.th";
				var currentDt = SystemHelper.GetCurrentDateTime();

				int take = 1000;
				int multiply = 0;
				List<ProductStageGroup> groupList = new List<ProductStageGroup>();
				var response = new List<JdaRequest>();
				if (request.GroupBy(n => n.Sku).Any(c => c.Count() > 1))
				{
					throw new Exception("Request Sku cannot be duplicate");
				}
				while (true)
				{
					var tmpRequest = request.Skip(take * multiply).Take(take);
					var req = tmpRequest.Select(s => new { s.Sku, s.ShopId });
					#region validation
					if (tmpRequest.Count() == 0)
					{
						break;
					}
					#endregion
					var rqSkus = req.Select(s => s.Sku);
					var shopIds = req.Select(s => s.ShopId);
					var products = db.ProductStages
						.Where(w => shopIds.Contains(w.ShopId) && rqSkus.Contains(w.Sku))
						.Select(s => new
					{
						s.Sku,
						s.ShopId,
						s.Pid
					});
					foreach (var jdaProduct in tmpRequest)
					{
						#region validation
						if (jdaProduct.ShopId == 0
							|| string.IsNullOrWhiteSpace(jdaProduct.Sku))
						{
							throw new Exception("Shop id and sku are required");
						}
						if (jdaProduct.Sku.Trim().Length > 255)
						{
							throw new Exception("Sku cannot be more than 255 characters.");
						}
						if (jdaProduct.JDADept.Trim().Length > 3)
						{
							throw new Exception("JDADept cannot be more than 3 characters.");
						}
						if (jdaProduct.JDASubDept.Trim().Length > 3)
						{
							throw new Exception("JDASubDept cannot be more than 3 characters.");
						}
						var tmpProduct = products.Where(w => w.ShopId == jdaProduct.ShopId && w.Sku.Equals(jdaProduct.Sku)).Select(s => s.Pid).FirstOrDefault();
						if (!string.IsNullOrWhiteSpace(tmpProduct))
						{
							ProductStage stage = new ProductStage()
							{
								Pid = tmpProduct,
							};
							UpdateJda(stage,email,currentDt, jdaProduct, db);
							jdaProduct.Pid = tmpProduct;
							response.Add(jdaProduct);
							continue;
						}
						#endregion
						#region setup group
						ProductStageGroup group = new ProductStageGroup()
						{
							ApproveBy = null,
							ApproveOn = null,
							AttributeSetId = null,
							BrandId = null,
							CategoryTabStatus = Constant.PRODUCT_STATUS_WAIT_FOR_APPROVAL,
							CreateBy = email,
							CreateOn = currentDt,
							EffectiveDate = currentDt,
							ExpireDate = currentDt.AddYears(Constant.DEFAULT_ADD_YEAR),
							FirstApproveBy = null,
							FirstApproveOn = null,
							GiftWrap = Constant.STATUS_NO,
							GlobalCatId = Constant.DEFAULT_GLOBAL_CATEGORY,
							ImageFlag = false,
							ImageTabStatus = Constant.PRODUCT_STATUS_WAIT_FOR_APPROVAL,
							InfoFlag = false,
							InformationTabStatus = Constant.PRODUCT_STATUS_WAIT_FOR_APPROVAL,
							IsBestSeller = false,
							IsClearance = false,
							IsNew = false,
							IsOnlineExclusive = false,
							IsOnlyAt = false,
							LocalCatId = null,
							MoreOptionTabStatus = Constant.PRODUCT_STATUS_WAIT_FOR_APPROVAL,
							NewArrivalDate = null,
							OldGroupId = null,
							OnlineFlag = false,
							RejecteBy = null,
							RejectOn = null,
							RejectReason = string.Empty,
							Remark = string.Empty,
							ShippingId = Constant.DEFAULT_SHIPPING_ID,
							ShopId = jdaProduct.ShopId,
							Status = Constant.PRODUCT_STATUS_DRAFT,
							SubmitBy = null,
							SubmitOn = null,
							TheOneCardEarn = Constant.DEFAULT_THE_ONE_CARD,
							UpdateBy = email,
							UpdateOn = currentDt,
							VariantTabStatus = Constant.PRODUCT_STATUS_WAIT_FOR_APPROVAL,
						};
						#endregion
						#region setup master variant
						group.ProductStages.Add(new ProductStage()
						{
							BoostWeight = Constant.DEFAULT_BOOSTWEIGHT,
							Bu = null,
							CreateBy = email,
							CreateOn = currentDt,
							DefaultVariant = false,
							DeliveryFee = 0,
							DescriptionFullEn = string.Empty,
							DescriptionFullTh = string.Empty,
							DescriptionShortEn = jdaProduct.ShortDescriptionEn,
							DescriptionShortTh = jdaProduct.ShortDescriptionTh,
							DimensionUnit = Constant.DIMENSTION_MM,
							Display = string.Empty,
							EffectiveDatePromotion = jdaProduct.EffectiveDatePromotion,
							ExpireDatePromotion = jdaProduct.ExpireDatePromotion,
							EstimateGoodsReceiveDate = null,
							ExpressDelivery = Constant.STATUS_NO,
							FeatureImgUrl = string.Empty,
							GlobalBoostWeight = Constant.DEFAULT_BOOSTWEIGHT,
							Height = 0,
							ImageCount = 0,
							Installment = Constant.STATUS_NO,
							IsHasExpiryDate = Constant.STATUS_NO,
							IsMaster = false,
							IsSell = true,
							IsVariant = false,
							IsVat = Constant.STATUS_NO,
							Inventory = new Inventory()
							{
								CreateBy = email,
								CreateOn = currentDt,
								Defect = 0,
								MaxQtyAllowInCart = int.MaxValue,
								MaxQtyPreOrder = int.MaxValue,
								MinQtyAllowInCart = 0,
								OnHold = 0,
								Quantity = jdaProduct.Quantity,
								Reserve = 0,
								SafetyStockAdmin = 0,
								SafetyStockSeller = 0,
								StockType = Constant.STOCK_TYPE[Constant.DEFAULT_STOCK_TYPE],
								UpdateBy = email,
								UpdateOn = currentDt,
								UseDecimal = false
							},
							JDADept = jdaProduct.JDADept,
							JDASubDept = jdaProduct.JDASubDept,
							KillerPoint1En = string.Empty,
							KillerPoint1Th = string.Empty,
							KillerPoint2En = string.Empty,
							KillerPoint2Th = string.Empty,
							KillerPoint3En = string.Empty,
							KillerPoint3Th = string.Empty,
							Length = 0,
							LimitIndividualDay = false,
							MaxiQtyAllowed = 0,
							MetaDescriptionEn = string.Empty,
							MetaDescriptionTh = string.Empty,
							MetaKeyEn = string.Empty,
							MetaKeyTh = string.Empty,
							MetaTitleEn = string.Empty,
							MetaTitleTh = string.Empty,
							MiniQtyAllowed = 0,
							MobileDescriptionEn = string.Empty,
							MobileDescriptionTh = string.Empty,
							NewArrivalDate = null,
							OldPid = null,
							OriginalPrice = jdaProduct.Price.HasValue ? jdaProduct.Price.Value : 0,
							PrepareDay = 0,
							PrepareFri = 0,
							PrepareMon = 0,
							PrepareSat = 0,
							PrepareSun = 0,
							PrepareThu = 0,
							PrepareTue = 0,
							PrepareWed = 0,
							ProdTDNameEn = string.Empty,
							ProdTDNameTh = string.Empty,
							ProductNameEn = !string.IsNullOrEmpty(jdaProduct.ShortDescriptionEn) ?
											jdaProduct.ShortDescriptionEn.Trim() :
											!string.IsNullOrEmpty(jdaProduct.ShortDescriptionTh) ?
											jdaProduct.ShortDescriptionTh.Trim() : 
											jdaProduct.Sku,
							ProductNameTh = !string.IsNullOrEmpty(jdaProduct.ShortDescriptionEn) ?
											jdaProduct.ShortDescriptionEn.Trim() :
											!string.IsNullOrEmpty(jdaProduct.ShortDescriptionTh) ?
											jdaProduct.ShortDescriptionTh.Trim() :
											jdaProduct.Sku,
							PromotionPrice = jdaProduct.PromotionPrice.HasValue ? jdaProduct.PromotionPrice.Value : 0,
							PurchasePrice = 0,
							SalePrice = jdaProduct.Price.HasValue ? jdaProduct.PromotionPrice.Value : 0,
							SaleUnitEn = string.Empty,
							SaleUnitTh = string.Empty,
							SeoEn = string.Empty,
							SeoTh = string.Empty,
							ShopId = jdaProduct.ShopId,
							Sku = jdaProduct.Sku,
							Status = Constant.PRODUCT_STATUS_DRAFT,
							UnitPrice = 0,
							Upc = string.IsNullOrEmpty(jdaProduct.Barcode) ? string.Empty : jdaProduct.Barcode,
							UpdateBy = email,
							UpdateOn = currentDt,
							UrlKey = string.Empty,
							VariantCount = 0,
							Visibility = true,
							Weight = 0,
							WeightUnit = Constant.WEIGHT_MEASURE_G,
							Width = 0,
						});
						#endregion
						groupList.Add(group);
					}
					multiply++;
				}
				#region generate id
				foreach (var group in groupList)
				{
					AutoGenerate.GeneratePid(db, group.ProductStages);
					group.ProductId = db.GetNextProductStageGroupId().SingleOrDefault().Value;
					var stage = group.ProductStages.FirstOrDefault();
					if (stage != null)
					{
						response.Add(new JdaRequest()
						{
							Sku = stage.Sku,
							Pid = stage.Pid,
							ShopId = stage.ShopId
						});
					}
					SendToCmos(group, Apis.CmosCreateProduct, "POST", email, currentDt, db);
				}
				#endregion
				db.Configuration.ValidateOnSaveEnabled = false;
				db.ProductStageGroups.AddRange(groupList);
				Util.DeadlockRetry(db.SaveChanges, "ProductStage");
				return Request.CreateResponse(HttpStatusCode.OK, response);
			}
			catch (Exception e)
			{
				return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
			}
		}

		[Route("api/ProductStages/JdaProduct")]
		[HttpPut]
		[OverrideAuthentication, OverrideAuthorization]
		public HttpResponseMessage SaveProductJda(List<JdaRequest> request)
		{
			try
			{
				if (request == null)
				{
					throw new Exception("Invalid request");
				}
				if (request.GroupBy(n => n.Pid).Any(c => c.Count() > 1))
				{
					throw new Exception("Request Pid cannot be duplicate");
				}
				int take = 1000;
				int multiply = 0;
				var email = "jda@col.co.th";
				var currentDt = SystemHelper.GetCurrentDateTime();
				while (true)
				{
					var tmpRequest = request.Skip(take * multiply).Take(take);
					var rqPids = tmpRequest.Select(s => s.Pid);
					var products = db.ProductStages.Where(w => rqPids.Contains(w.Pid)).Select(s => new
					{
						s.Pid,
						s.ShopId
					});
					#region validation
					if (tmpRequest.Count() == 0)
					{
						break;
					}
					#endregion
					foreach (var jdaProduct in tmpRequest)
					{
						if (string.IsNullOrWhiteSpace(jdaProduct.Pid))
						{
							throw new Exception("Pid cannot be empty");
						}
						var tmpProduct = products.Where(w => w.ShopId == jdaProduct.ShopId && w.Pid.Equals(jdaProduct.Pid)).SingleOrDefault();
						if (tmpProduct == null)
						{
							throw new Exception(string.Concat("Cannot find pid ", jdaProduct.Pid, " in shop id ", jdaProduct.ShopId));
						}
						ProductStage stage = new ProductStage()
						{
							Pid = jdaProduct.Pid
						};
						UpdateJda(stage,email,currentDt, jdaProduct, db);
					}
					multiply++;
				}
				db.Configuration.ValidateOnSaveEnabled = false;
				db.SaveChanges();
				return Request.CreateResponse(HttpStatusCode.OK, request);
			}
			catch (Exception e)
			{
				return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.GetBaseException());
			}
		}

		private void UpdateJda(ProductStage stage,string email,DateTime currentDt, JdaRequest jdaProduct, ColspEntities db)
		{
			db.ProductStages.Attach(stage);
			db.Entry(stage).Property(p => p.UpdateBy).IsModified = true;
			db.Entry(stage).Property(p => p.UpdateOn).IsModified = true;
			stage.UpdateBy = email;
			stage.UpdateOn = currentDt;
			if (jdaProduct.Price.HasValue)
			{
				db.Entry(stage).Property(p => p.SalePrice).IsModified = true;
				stage.SalePrice = jdaProduct.Price.Value;
			}
			if (jdaProduct.PromotionPrice.HasValue)
			{
				db.Entry(stage).Property(p => p.PromotionPrice).IsModified = true;
				stage.PromotionPrice = jdaProduct.PromotionPrice.Value;
			}
			if (jdaProduct.EffectiveDatePromotion.HasValue)
			{
				db.Entry(stage).Property(p => p.EffectiveDatePromotion).IsModified = true;
				stage.EffectiveDatePromotion = jdaProduct.EffectiveDatePromotion;
			}
			if (jdaProduct.ExpireDatePromotion.HasValue)
			{
				db.Entry(stage).Property(p => p.ExpireDatePromotion).IsModified = true;
				stage.ExpireDatePromotion = jdaProduct.ExpireDatePromotion;
			}

		}


		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				db.Dispose();
			}
			base.Dispose(disposing);
		}

	}

}