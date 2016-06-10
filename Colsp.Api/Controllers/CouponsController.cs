using System;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Colsp.Entity.Models;
using Colsp.Model.Requests;
using Colsp.Api.Extensions;
using Colsp.Api.Constants;
using Colsp.Model.Responses;
using System.Data.Entity.SqlServer;
using System.Data.Entity;
using Colsp.Api.Helpers;
using System.Collections.Generic;
using System.Web.Script.Serialization;

namespace Colsp.Api.Controllers
{
	public class CouponsController : ApiController
	{
		private ColspEntities db = new ColspEntities();

		[Route("api/Coupons")]
		[HttpGet]
		public HttpResponseMessage GetAllCoupon([FromUri] CouponRequest request)
		{
			try
			{
				var coupon = from c in db.Coupons
							 select new
							 {
								 c.CouponId,
								 c.CouponCode,
								 c.CouponName,
								 Remaining = c.CouponQuantity - c.CouponUsed,
								 c.StartDate,
								 c.ExpireDate,
								 c.Status,
								 c.ShopId,
								 c.MaximumUser,
								 Shop = new { c.Shop.ShopNameEn, c.Shop.Status },
								 c.CouponType
							 };
				if (User.ShopRequest() != null)
				{
					var shopId = User.ShopRequest().ShopId;
					coupon = coupon.Where(w => w.ShopId == shopId && !Constant.STATUS_REMOVE.Equals(w.Shop.Status) && Constant.USER_TYPE_SELLER.Equals(w.CouponType));
				}
				else
				{
					if (request.IsGlobalCoupon)
					{
						coupon = coupon.Where(w => w.CouponType.Equals(Constant.USER_TYPE_ADMIN));
					}
					else
					{
						coupon = coupon.Where(w => w.CouponType.Equals(Constant.USER_TYPE_SELLER) && !Constant.STATUS_REMOVE.Equals(w.Shop.Status));
					}
				}
				if (request == null)
				{
					return Request.CreateResponse(HttpStatusCode.OK, coupon);
				}
				request.DefaultOnNull();
				if (!string.IsNullOrEmpty(request.SearchText))
				{
					coupon = coupon.Where(w => w.CouponName.Contains(request.SearchText)
					|| w.CouponCode.Contains(request.SearchText));
				}
				var total = coupon.Count();
				var pagedAttribute = coupon.Paginate(request);
				var response = PaginatedResponse.CreateResponse(pagedAttribute, request, total);
				return Request.CreateResponse(HttpStatusCode.OK, response);
			}
			catch (Exception e)
			{
				return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.GetBaseException().Message);
			}
		}

		[Route("api/Coupons/{couponId}")]
		[HttpGet]
		public HttpResponseMessage GetCoupon(int couponId)
		{
			try
			{
				var couponList = db.Coupons
					.Where(w => w.CouponId == couponId && !Constant.STATUS_REMOVE.Equals(w.Status))
					.Select(s => new
					{
						s.CouponId,
						s.CouponName,
						s.CouponCode,
						s.Status,
						StartDate = SqlFunctions.DateName("month", s.StartDate) + " " + SqlFunctions.DateName("day", s.StartDate) + ", " + SqlFunctions.DateName("year", s.StartDate) + " " + SqlFunctions.DateName("hour", s.StartDate) + ":" + SqlFunctions.DateName("minute", s.StartDate),
						ExpireDate = SqlFunctions.DateName("month", s.ExpireDate) + " " + SqlFunctions.DateName("day", s.ExpireDate) + ", " + SqlFunctions.DateName("year", s.ExpireDate) + " " + SqlFunctions.DateName("hour", s.ExpireDate) + ":" + SqlFunctions.DateName("minute", s.ExpireDate),
						s.ShopId,
						Action = new { Type = s.Action, s.DiscountAmount, s.MaximumAmount },
						s.UsagePerCustomer,
						s.MaximumUser,
						ShopStatus = s.Shop.Status,
						Conditions = new
						{
							Order = s.CouponOrders.Select(se => new { Type = se.Criteria, Value = se.CriteriaPrice }),
							FilterBy = new
							{
								Type = s.FilterBy,
								Brands = s.CouponBrandMaps.Select(se => new { se.Brand.BrandId, se.Brand.BrandNameEn }),
								Emails = s.CouponCustomerMaps.Select(se => se.Email),
								GlobalCategories = s.CouponGlobalCatMaps.Select(se => new { se.GlobalCategory.CategoryId, se.GlobalCategory.NameEn }),
								LocalCategories = s.CouponLocalCatMaps.Select(se => new
                                {
                                    se.LocalCategory.CategoryId,
                                    se.LocalCategory.NameEn,
                                    Exclude = s.CouponLocalCatPidMaps.Where(w => w.Filter.Equals(Constant.COUPON_FILTER_EXCLUDE) && w.CouponId == s.CouponId && w.CategoryId == se.LocalCategory.CategoryId).Select(i => new {
                                        Pid = i.Pid,
                                        ProductNameEn = db.ProductStages.Where(w => w.Pid == i.Pid).Select(p => p.ProductNameEn).FirstOrDefault(),
                                        ProductNameTh = db.ProductStages.Where(w => w.Pid == i.Pid).Select(p => p.ProductNameTh).FirstOrDefault(),
                                        Sku = db.ProductStages.Where(w => w.Pid == i.Pid).Select(p => p.Sku).FirstOrDefault(),
                                    })
                                }),
								Shops = s.CouponShopMaps.Select(se => new { se.Shop.ShopId, se.Shop.ShopNameEn })
							},
							//Include = s.CouponPidMaps.Where(w=>w.Filter.Equals(Constant.COUPON_FILTER_INCLUDE)).Select(se=>se.Pid),
							//Exclude = s.CouponPidMaps.Where(w=>w.Filter.Equals(Constant.COUPON_FILTER_EXCLUDE)).Select(se=>se.Pid)
							Include = s.CouponPidMaps.Where(w => w.Filter.Equals(Constant.COUPON_FILTER_INCLUDE) && w.CouponId == s.CouponId).Select(i => new
							{
								Pid = i.Pid,
								ProductNameEn = db.ProductStages.Where(w => w.Pid == i.Pid).Select(p => p.ProductNameEn).FirstOrDefault(),
								ProductNameTh = db.ProductStages.Where(w => w.Pid == i.Pid).Select(p => p.ProductNameTh).FirstOrDefault(),
								Sku = db.ProductStages.Where(w => w.Pid == i.Pid).Select(p => p.Sku).FirstOrDefault(),
							}),
							Exclude = s.CouponPidMaps.Where(w => w.Filter.Equals(Constant.COUPON_FILTER_EXCLUDE) && w.CouponId == s.CouponId).Select(i => new {
								Pid = i.Pid,
								ProductNameEn = db.ProductStages.Where(w => w.Pid == i.Pid).Select(p => p.ProductNameEn).FirstOrDefault(),
								ProductNameTh = db.ProductStages.Where(w => w.Pid == i.Pid).Select(p => p.ProductNameTh).FirstOrDefault(),
								Sku = db.ProductStages.Where(w => w.Pid == i.Pid).Select(p => p.Sku).FirstOrDefault(),
							})
						}

					});
				if (User.ShopRequest() != null)
				{
					var shopId = User.ShopRequest().ShopId;
					couponList = couponList.Where(w => w.ShopId == shopId);
				}
				var coupon = couponList.SingleOrDefault();

				if (coupon == null || Constant.STATUS_REMOVE.Equals(coupon.ShopStatus))
				{
					throw new Exception("Cannot find coupon");
				}
				return Request.CreateResponse(HttpStatusCode.OK, coupon);
			}
			catch (Exception e)
			{
				return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.GetBaseException().Message);
			}
		}

		[Route("api/Coupons")]
		[HttpPost]
		public HttpResponseMessage AddCoupon(CouponRequest request)
		{
			try
			{
				if (request == null)
				{
					throw new Exception("Invalid request");
				}
				Coupon coupon  = new Coupon();
				if (User.ShopRequest() != null)
				{
					var shopId = User.ShopRequest().ShopId;
					coupon.ShopId = shopId;
					coupon.CouponType = Constant.USER_TYPE_SELLER;
				}
				else
				{
					coupon.CouponType = Constant.USER_TYPE_ADMIN;
				}
				var email = User.UserRequest().Email;
				var currentDt = SystemHelper.GetCurrentDateTime();
				SetupCoupon(coupon, request, email, currentDt, db);
				coupon.CreateBy = email;
				coupon.CreateOn = currentDt;
				SendToCore(coupon,Apis.EVoucherCreate,"POST",email,currentDt,db);
				coupon.CouponId = db.GetNextCouponId().SingleOrDefault().Value;
				db.Coupons.Add(coupon);
				Util.DeadlockRetry(db.SaveChanges, "Coupon");
				return GetCoupon(coupon.CouponId);
			}
			catch (Exception e)
			{
				return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.GetBaseException().Message);
			}
		}

		private void SendToCore(Coupon coupon, string url, string method
			, string email, DateTime currentDt, ColspEntities db)
		{
			var criteria = coupon.CouponOrders.FirstOrDefault();
			EVoucherRequest request = new EVoucherRequest()
			{
				CouponCode = coupon.CouponCode,
				CouponName = coupon.CouponName,
				ExpiredDate = coupon.ExpireDate.Value,
				StartDate = coupon.StartDate.Value,
				Status = Constant.STATUS_ACTIVE.Equals(coupon.Status),
				DiscountType = Constant.COUPON_ACTION_AMOUNT.Equals(coupon.Action) ? 0 : 1,
				DiscountBaht = Constant.COUPON_ACTION_AMOUNT.Equals(coupon.Action) ? coupon.DiscountAmount : (decimal?)null,
				DiscountPercent = Constant.COUPON_ACTION_PERCENT.Equals(coupon.Action) ? coupon.DiscountAmount : (decimal?)null,
				MaximumDiscount = Constant.COUPON_ACTION_PERCENT.Equals(coupon.Action) ? coupon.MaximumAmount : (decimal?)null,
				MaximumUses = coupon.MaximumUser,
				UsesPerCustomer = coupon.UsagePerCustomer,
				CartCriteria = criteria != null ? 0 : 0,
				CriteriaPrice = criteria != null ? criteria.CriteriaPrice : 0,
				IncludeProductCriteria = "LocalCategory".Equals(coupon.FilterBy) ? 3 : 
										 "GlobalCategory".Equals(coupon.FilterBy) ? 1 : 
										 "Shop".Equals(coupon.FilterBy) ? 2 :
										 (int?)null,
				IncludeGlobalCategories = coupon.CouponGlobalCatMaps.Select(s=>s.CategoryId).ToList(),
				IncludeLocalCategories = coupon.CouponLocalCatMaps.Select(s=>s.CategoryId).ToList(),
				IncludeShopIds = coupon.CouponShopMaps.Select(s=>s.ShopId).ToList(),
				IncludeProductIds = coupon.CouponPidMaps
										.Where(w=>Constant.COUPON_FILTER_INCLUDE.Equals(w.Filter))
										.Select(s=>s.Pid).ToList(),
				ExcludeProductCriteria = "LocalCategory".Equals(coupon.FilterBy) ? 3 :
										 "GlobalCategory".Equals(coupon.FilterBy) ? 1 :
										 "Shop".Equals(coupon.FilterBy) ? 2 :
										 (int?)null,
				ExcludeProductIds = coupon.CouponPidMaps
										.Where(w => Constant.COUPON_FILTER_EXCLUDE.Equals(w.Filter))
										.Select(s => s.Pid).ToList(),
			};
			Dictionary<string, string> headers = new Dictionary<string, string>();
			headers.Add(Apis.EVoucherKeyAppIdKey, Apis.EVoucherKeyAppIdValue);
			headers.Add(Apis.EVoucherKeyAppSecretKey, Apis.EVoucherKeyAppSecretValue);
			headers.Add(Apis.EVoucherKeyVersionKey, Apis.EVoucherKeyVersionValue);
			var json = new JavaScriptSerializer().Serialize(request);
			string  responseJson = SystemHelper.SendRequest(url, method, headers,json, email, currentDt, "SP", "EVOUCHER", db);
			EVoucherResponse response =  new JavaScriptSerializer().Deserialize<EVoucherResponse>(responseJson);
			if (!"00".Equals(response.returncode))
			{
				throw new Exception(string.Join("<br>", response.message.Select(s=>s.Value)));
			}
			coupon.EVoucherId = response.evoucher.id;
		}



		[Route("api/Coupons/{couponid}")]
		[HttpPut]
		public HttpResponseMessage SaveChangeCoupon(int couponId, CouponRequest request)
		{
			Coupon coupon = null;
			try
			{
				if (User.ShopRequest() != null)
				{
					int shopId = User.ShopRequest().ShopId;
					coupon = db.Coupons.Where(w => w.CouponId == couponId && w.ShopId == shopId)
						.Include(i => i.CouponBrandMaps)
						.Include(i => i.CouponCustomerMaps)
						.Include(i => i.CouponGlobalCatMaps)
						.Include(i => i.CouponLocalCatMaps)
						.Include(i => i.CouponPidMaps)
						.Include(i => i.CouponShopMaps)
						.Include(i => i.CouponOrders).SingleOrDefault();
				}
				else
				{
					coupon = db.Coupons.Where(w => w.CouponId == couponId)
						.Include(i => i.CouponBrandMaps)
						.Include(i => i.CouponCustomerMaps)
						.Include(i => i.CouponGlobalCatMaps)
						.Include(i => i.CouponLocalCatMaps)
						.Include(i => i.CouponPidMaps)
						.Include(i => i.CouponShopMaps)
						.Include(i => i.CouponOrders).SingleOrDefault();
				}
				if (coupon == null)
				{
					throw new Exception("Cannot find coupon");
				}
				var email = User.UserRequest().Email;
				var currentDt = SystemHelper.GetCurrentDateTime();
				SetupCoupon(coupon, request, email, currentDt, db);
				SendToCore(coupon, string.Concat(Apis.EVoucherUpdate, coupon.EVoucherId), "PUT", email, currentDt, db);
				Util.DeadlockRetry(db.SaveChanges, "Coupon");
				return GetCoupon(coupon.CouponId);
			}
			catch (Exception e)
			{
				return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.GetBaseException().Message);
			}
		}


		private void SetupCoupon(Coupon coupon, CouponRequest request, string email, DateTime currentDt, ColspEntities db)
		{
			coupon.CouponName = Validation.ValidateString(request.CouponName, "Coupon Name", true, 300, false);
			coupon.CouponCode = Validation.ValidateString(request.CouponCode, "Coupon Code", true, 50, true); ;
			coupon.Status = Validation.ValidateString(request.Status, "Status", true, 2, true);
			coupon.ExpireDate = Validation.ValidateDateTime(request.ExpireDate, "Expire Date", false);
			coupon.StartDate = Validation.ValidateDateTime(request.StartDate, "Start Date", false);
			if(coupon.StartDate.HasValue 
				&& coupon.ExpireDate.HasValue
				&& coupon.StartDate.Value.CompareTo(coupon.ExpireDate.Value) > 0)
			{
				throw new Exception("Start Date cannot be more than Expire Date");
			}
			coupon.Action = request.Action.Type;
			coupon.DiscountAmount = request.Action.DiscountAmount;
			if (Constant.COUPON_ACTION_PERCENT.Equals(coupon.Action)
							&& coupon.DiscountAmount >= 100)
			{
				throw new Exception("Discount Percent cannot more than equal 100%.");
			}
			coupon.MaximumAmount = request.Action.MaximumAmount;
			if(coupon.MaximumAmount <= 0)
			{
				coupon.MaximumAmount = 9999999;
			}
			coupon.UsagePerCustomer = request.UsagePerCustomer;
			if (coupon.UsagePerCustomer <= 0)
			{
				coupon.UsagePerCustomer = 1;
			}
			coupon.MaximumUser = request.MaximumUser;
			
			coupon.UpdateBy = email;
			coupon.UpdateOn = currentDt;
			var orderList = coupon.CouponOrders.ToList();
			var brandList = coupon.CouponBrandMaps.ToList();
			var customerList = coupon.CouponCustomerMaps.ToList();
			var globalCatList = coupon.CouponGlobalCatMaps.ToList();
			var localCatList = coupon.CouponLocalCatMaps.ToList();
			var shopList = coupon.CouponShopMaps.ToList();
			var includeList = coupon.CouponPidMaps.Where(w => w.Filter.Equals(Constant.COUPON_FILTER_INCLUDE)).ToList();
			var excludeList = coupon.CouponPidMaps.Where(w => w.Filter.Equals(Constant.COUPON_FILTER_EXCLUDE)).ToList();
			var localCatIncludeList = coupon.CouponLocalCatPidMaps.Where(w => w.Filter.Equals(Constant.COUPON_FILTER_INCLUDE)).ToList();
			var localCatExcludeList = coupon.CouponLocalCatPidMaps.Where(w => w.Filter.Equals(Constant.COUPON_FILTER_EXCLUDE)).ToList();
			if (request.Conditions != null)
			{
				if (request.Conditions.Order != null && request.Conditions.Order.Count > 0)
				{

					foreach (OrderRequest o in request.Conditions.Order)
					{
						if(Constant.COUPON_ACTION_AMOUNT.Equals(coupon.Action) 
							&& o.Value >= coupon.DiscountAmount)
						{
							throw new Exception("Criteria Price cannot be more than equal to Discount Amount.");
						}
						bool addNew = false;
						if (orderList == null || orderList.Count == 0)
						{
							addNew = true;
						}
						if (!addNew)
						{
							CouponOrder current = orderList.Where(w => w.Criteria == o.Type).SingleOrDefault();
							if (current != null)
							{
								if (current.CriteriaPrice != o.Value)
								{
									current.CriteriaPrice = o.Value;
									current.UpdateBy = email;
									current.UpdateOn = currentDt;
								}
								orderList.Remove(current);
							}
							else
							{
								addNew = true;
							}
						}
						if (addNew)
						{
							coupon.CouponOrders.Add(new CouponOrder()
							{
								Criteria = o.Type,
								CriteriaPrice = o.Value,
								CreateBy = email,
								CreateOn = currentDt,
								UpdateBy = email,
								UpdateOn = currentDt
							});
						}
					}
				}
				else
				{
					throw new Exception("Order Condition is required.");
				}
				if (request.Conditions.FilterBy != null)
				{
					coupon.FilterBy = request.Conditions.FilterBy.Type;
					if ("Brand".Equals(request.Conditions.FilterBy.Type))
					{
						if (request.Conditions.FilterBy.Brands != null && request.Conditions.FilterBy.Brands.Count > 0)
						{

							foreach (BrandRequest b in request.Conditions.FilterBy.Brands)
							{
								bool addNew = false;
								if (brandList == null || brandList.Count == 0)
								{
									addNew = true;
								}
								if (!addNew)
								{
									CouponBrandMap current = brandList.Where(w => w.BrandId == b.BrandId).SingleOrDefault();
									if (current != null)
									{
										brandList.Remove(current);
									}
									else
									{
										addNew = true;
									}
								}
								if (addNew)
								{
									coupon.CouponBrandMaps.Add(new CouponBrandMap()
									{
										BrandId = b.BrandId,
										CreateBy = email,
										CreateOn = currentDt,
										UpdateBy = email,
										UpdateOn = currentDt,
									});
								}
							}
						}
					}
					else if ("Email".Equals(request.Conditions.FilterBy.Type))
					{
						if (request.Conditions.FilterBy.Emails != null && request.Conditions.FilterBy.Emails.Count > 0)
						{

							foreach (String e in request.Conditions.FilterBy.Emails)
							{
								bool addNew = false;
								if (customerList == null || customerList.Count == 0)
								{
									addNew = true;
								}
								if (!addNew)
								{
									CouponCustomerMap current = customerList.Where(w => w.Email == e).SingleOrDefault();
									if (current != null)
									{
										customerList.Remove(current);
									}
									else
									{
										addNew = true;
									}
								}
								if (addNew)
								{
									coupon.CouponCustomerMaps.Add(new CouponCustomerMap()
									{
										Email = e,
										CreateBy = email,
										CreateOn = currentDt,
										UpdateBy = email,
										UpdateOn = currentDt,
									});
								}
							}
						}
					}
					else if ("GlobalCategory".Equals(request.Conditions.FilterBy.Type))
					{

						if (request.Conditions.FilterBy.GlobalCategories != null && request.Conditions.FilterBy.GlobalCategories.Count > 0)
						{

							foreach (CategoryRequest c in request.Conditions.FilterBy.GlobalCategories)
							{
								bool addNew = false;
								if (globalCatList == null || globalCatList.Count == 0)
								{
									addNew = true;
								}
								if (!addNew)
								{
									CouponGlobalCatMap current = globalCatList.Where(w => w.CategoryId == c.CategoryId).SingleOrDefault();
									if (current != null)
									{
										globalCatList.Remove(current);
									}
									else
									{
										addNew = true;
									}
								}
								if (addNew)
								{
									coupon.CouponGlobalCatMaps.Add(new CouponGlobalCatMap()
									{
										Filter = Constant.COUPON_FILTER_INCLUDE,
										CategoryId = c.CategoryId,
										CreateBy = email,
										CreateOn = currentDt,
										UpdateBy = email,
										UpdateOn = currentDt,
									});
								}
							}
						}
					}
					else if ("LocalCategory".Equals(request.Conditions.FilterBy.Type))
					{
						if (request.Conditions.FilterBy.LocalCategories != null && request.Conditions.FilterBy.LocalCategories.Count > 0)
						{

							foreach (CategoryRequest c in request.Conditions.FilterBy.LocalCategories)
							{
								bool addNew = false;
								if (localCatList == null || localCatList.Count == 0)
								{
									addNew = true;
								}
								if (!addNew)
								{
									CouponLocalCatMap current = localCatList.Where(w => w.CategoryId == c.CategoryId).SingleOrDefault();
									if (current != null)
									{
										localCatList.Remove(current);
									}
									else
									{
										addNew = true;
									}
								}
								if (addNew)
								{
									coupon.CouponLocalCatMaps.Add(new CouponLocalCatMap()
									{
										Filter = Constant.COUPON_FILTER_INCLUDE,
										CouponId = coupon.CouponId,
										CategoryId = c.CategoryId,
										CreateBy = email,
										CreateOn = currentDt,
										UpdateBy = email,
										UpdateOn = currentDt,
									});
								}

								// Exclude Product in Local Categories
								if (c.Exclude != null && c.Exclude.Count > 0)
								{

									foreach (var p in c.Exclude)
									{
										bool _addNew = false;
										if (localCatExcludeList == null || localCatExcludeList.Count == 0)
										{
											_addNew = true;
										}
										if (!_addNew)
										{
											CouponLocalCatPidMap current = localCatExcludeList.Where(w => w.Pid == p.Pid).SingleOrDefault();
											if (current != null)
											{
												localCatExcludeList.Remove(current);
											}
											else
											{
												_addNew = true;
											}
										}
										if (_addNew)
										{
											coupon.CouponLocalCatPidMaps.Add(new CouponLocalCatPidMap()
											{
												Pid = p.Pid,
												CategoryId = c.CategoryId,
												Filter = Constant.COUPON_FILTER_EXCLUDE,
												CreateBy = email,
												CreateOn = currentDt,
												UpdateBy = email,
												UpdateOn = currentDt,
											});
										}
									}
								}
							}
						}
					}
					else if ("Shop".Equals(request.Conditions.FilterBy.Type))
					{

						if (request.Conditions.FilterBy.Shops != null && request.Conditions.FilterBy.Shops.Count > 0)
						{

							foreach (ShopRequest s in request.Conditions.FilterBy.Shops)
							{
								bool addNew = false;
								if (shopList == null || shopList.Count == 0)
								{
									addNew = true;
								}
								if (!addNew)
								{
									CouponShopMap current = shopList.Where(w => w.ShopId == s.ShopId).SingleOrDefault();
									if (current != null)
									{
										shopList.Remove(current);
									}
									else
									{
										addNew = true;
									}
								}
								if (addNew)
								{
									coupon.CouponShopMaps.Add(new CouponShopMap()
									{
										ShopId = s.ShopId,
										CreateBy = email,
										CreateOn = currentDt,
										UpdateBy = email,
										UpdateOn = currentDt,
									});
								}
							}
						}
					}
					if (request.Conditions.Include != null && request.Conditions.Include.Count > 0)
					{

						foreach (var p in request.Conditions.Include)
						{
							bool addNew = false;
							if (includeList == null || includeList.Count == 0)
							{
								addNew = true;
							}
							if (!addNew)
							{
								CouponPidMap current = includeList.Where(w => w.Pid == p.Pid).SingleOrDefault();
								if (current != null)
								{
									includeList.Remove(current);
								}
								else
								{
									addNew = true;
								}
							}
							if (addNew)
							{
								coupon.CouponPidMaps.Add(new CouponPidMap()
								{
									Pid = p.Pid,
									Filter = Constant.COUPON_FILTER_INCLUDE,
									CreateBy = email,
									CreateOn = currentDt,
									UpdateBy = email,
									UpdateOn = currentDt,
								});
							}
						}
					}
					if (request.Conditions.Exclude != null && request.Conditions.Exclude.Count > 0)
					{

						foreach (var p in request.Conditions.Exclude)
						{
							bool addNew = false;
							if (excludeList == null || excludeList.Count == 0)
							{
								addNew = true;
							}
							if (!addNew)
							{
								CouponPidMap current = excludeList.Where(w => w.Pid == p.Pid).SingleOrDefault();
								if (current != null)
								{
									excludeList.Remove(current);
								}
								else
								{
									addNew = true;
								}
							}
							if (addNew)
							{

								coupon.CouponPidMaps.Add(new CouponPidMap()
								{
									Pid = p.Pid,
									Filter = Constant.COUPON_FILTER_EXCLUDE,
									CreateBy = email,
									CreateOn = currentDt,
									UpdateBy = email,
									UpdateOn = currentDt,
								});
							}
						}
					}
				}
				else
				{
					coupon.FilterBy = string.Empty;
				}
			}

			if (orderList != null && orderList.Count > 0)
			{
				db.CouponOrders.RemoveRange(orderList);
			}
			if (brandList != null && brandList.Count > 0)
			{
				db.CouponBrandMaps.RemoveRange(brandList);
			}
			if (customerList != null && customerList.Count > 0)
			{
				db.CouponCustomerMaps.RemoveRange(customerList);
			}
			if (globalCatList != null && globalCatList.Count > 0)
			{
				db.CouponGlobalCatMaps.RemoveRange(globalCatList);
			}
			if (localCatList != null && localCatList.Count > 0)
			{
				db.CouponLocalCatMaps.RemoveRange(localCatList);
			}
			if (shopList != null && shopList.Count > 0)
			{
				db.CouponShopMaps.RemoveRange(shopList);
			}
			if (includeList != null && includeList.Count > 0)
			{
				db.CouponPidMaps.RemoveRange(includeList);
			}
			if (excludeList != null && excludeList.Count > 0)
			{
				db.CouponPidMaps.RemoveRange(excludeList);
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

		private bool CouponExists(int id)
		{
			return db.Coupons.Count(e => e.CouponId == id) > 0;
		}
	}
}