﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Colsp.Entity.Models;
using Colsp.Api.Constants;
using Colsp.Model.Requests;
using Colsp.Api.Extensions;
using Colsp.Model.Responses;
using Colsp.Api.Helpers;
using System.Threading.Tasks;
using Cenergy.Dazzle.Admin.Security.Cryptography;
using System.Web.Script.Serialization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Colsp.Api.Filters;

namespace Colsp.Api.Controllers
{
    public class ShopsController : ApiController
    {
        private ColspEntities db = new ColspEntities();
        private SaltedSha256PasswordHasher salt = new SaltedSha256PasswordHasher();


        [Route("api/Shops/{shopId}/LocalCategories")]
        [HttpGet]
		[ClaimsAuthorize(Permission = new string[] { "2", "3" })]
		public HttpResponseMessage GetLocalCategories(int shopId)
        {
            try
            {
                var localCat = db.LocalCategories
                    .Where(w => w.ShopId == shopId && (w.Shop.Status.Equals(Constant.STATUS_ACTIVE) || w.Shop.Status.Equals(Constant.STATUS_NOT_ACTIVE))).Select(s => new
                {
                    s.CategoryId,
                    s.NameEn,
                    s.NameTh,
                    s.Lft,
                    s.Rgt,
                    s.UrlKey,
                    s.Visibility
                });
                return Request.CreateResponse(HttpStatusCode.OK, localCat);
            }
            catch(Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }

        [Route("api/ThemeImages")]
        [HttpPost]
        public async Task<HttpResponseMessage> UploadBanner()
        {
            try
            {
                if (!Request.Content.IsMimeMultipartContent())
                {
                    throw new Exception("In valid content multi-media");
                }
                var streamProvider = new MultipartFormDataStreamProvider(Path.Combine(AppSettingKey.IMAGE_ROOT_PATH, AppSettingKey.GLOBAL_CAT_FOLDER));
                try
                {
                    await Request.Content.ReadAsMultipartAsync(streamProvider);
                }
                catch (Exception)
                {
                    throw new Exception("Image size exceeded " + 5 + " mb");
                }
                #region Validate Image
                string type = streamProvider.FormData["Type"];
                ImageRequest fileUpload = null;
                foreach (MultipartFileData fileData in streamProvider.FileData)
                {
                    fileUpload = Util.SetupImage(Request,
                        fileData,
                        AppSettingKey.IMAGE_ROOT_FOLDER,
                        AppSettingKey.GLOBAL_CAT_FOLDER, 0, 0, int.MaxValue, int.MaxValue, int.MaxValue, false);
                    break;
                }
                #endregion
                return Request.CreateResponse(HttpStatusCode.OK, fileUpload);

            }
            catch(Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }

        [Route("api/ShopImages")]
        [HttpPost]
        public async Task<HttpResponseMessage> UploadFile()
        {
            try
            {

                if (!Request.Content.IsMimeMultipartContent())
                {
                    throw new Exception("In valid content multi-media");
                }
                var streamProvider = new MultipartFormDataStreamProvider(Path.Combine(AppSettingKey.IMAGE_ROOT_PATH, AppSettingKey.SHOP_FOLDER));
                try
                {
                    await Request.Content.ReadAsMultipartAsync(streamProvider);
                }
                catch (Exception)
                {
                    throw new Exception("Image size exceeded " + 5 + " mb");
                }
                #region Validate Image
                string type = streamProvider.FormData["Type"];
                ImageRequest fileUpload = null;
                if ("Logo".Equals(type))
                {
                    foreach (MultipartFileData fileData in streamProvider.FileData)
                    {
                        fileUpload = Util.SetupImage(Request,
                            fileData,
                            AppSettingKey.IMAGE_ROOT_FOLDER,
                            AppSettingKey.SHOP_FOLDER, 500, 500, 1000, 1000, 5, true);
                        break;
                    }

                }
                else 
                {
                    foreach (MultipartFileData fileData in streamProvider.FileData)
                    {
                        fileUpload = Util.SetupImage(Request,
                            fileData,
                            AppSettingKey.IMAGE_ROOT_FOLDER,
                            AppSettingKey.SHOP_FOLDER, 0, 0, int.MaxValue, int.MaxValue, 5, false);
                        break;
                    }
                }
                
                #endregion
                return Request.CreateResponse(HttpStatusCode.OK, fileUpload);
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }

        [Route("api/Shops")]
        [HttpGet]
		[ClaimsAuthorize(Permission = new string[] { "10", "21", "2", "3", "35", "34", "12" })]
		public HttpResponseMessage GetShop([FromUri] ShopRequest request)
        {
            try
            {
                var shopList = db.Shops.Where(w => w.Status.Equals(Constant.STATUS_ACTIVE) 
					|| w.Status.Equals(Constant.STATUS_NOT_ACTIVE))
                    .Select(s => new
                    {
                        s.ShopId,
                        s.VendorId,
                        s.ShopNameEn,
                        ShopType = new { s.ShopTypeId, s.ShopType.ShopTypeNameEn },
                        s.Status,
                        s.ShopGroup,
                        UpdatedDt = s.UpdateOn,
                        s.BankAccountName,
                        s.BankAccountNumber
                    });
                if (request == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, shopList);
                }
                request.DefaultOnNull();
                if (!string.IsNullOrEmpty(request.SearchText))
                {
                    shopList = shopList.Where(a => a.ShopNameEn.Contains(request.SearchText) 
                    || a.ShopId.ToString().Equals(request.SearchText)
                    || a.ShopType.ShopTypeNameEn.Contains(request.SearchText));
                }
                var total = shopList.Count();
                var pagedUsers = shopList.Paginate(request);
                var response = PaginatedResponse.CreateResponse(pagedUsers, request, total);
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.GetBaseException().Message);
            }
        }

        [Route("api/Shops/{shopId}/ShopTypes")]
        [HttpGet]
		[ClaimsAuthorize(Permission = new string[] { "10" })]
		public HttpResponseMessage GetShopType(int shopId)
        {
            try
            {
                var shopType = db.Shops.Where(w => w.ShopId == shopId && (w.Status.Equals(Constant.STATUS_ACTIVE) || w.Status.Equals(Constant.STATUS_NOT_ACTIVE)))
                    .Select(s => s.ShopType).SingleOrDefault();
                return Request.CreateResponse(HttpStatusCode.OK, shopType);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.GetBaseException().Message);
            }
        }

        [Route("api/Shops/{shopId}")]
        [HttpGet]
		[ClaimsAuthorize(Permission = new string[] { "10" })]
		public HttpResponseMessage GetShop(int shopId)
        {
            try
            {
                var shop = db.Shops.Include(i=>i.User.UserGroupMaps.Select(ug => ug.UserGroup))
                    .Where(w => w.ShopId == shopId && (w.Status.Equals(Constant.STATUS_ACTIVE) || w.Status.Equals(Constant.STATUS_NOT_ACTIVE)))
                    .Select(s=>new
                    {
                        s.ShopId,
                        s.ShopNameEn,
                        ShopType = new { s.ShopTypeId, s.ShopType.ShopTypeNameEn },
                        s.Status,
                        s.Commission,
                        s.ShopGroup,
                        s.MaxLocalCategory,
                        BankName = s.BankDetail,
                        s.BankAccountName,
                        s.BankAccountNumber,
                        s.TaxPayerId,
                        s.TermPayment,
                        s.VendorTaxRate,
                        s.WithholdingTax,
                        s.Payment,
                        s.DomainName,
                        s.ShopAddress,
                        s.VendorAddressLine1,
                        s.VendorAddressLine2,
                        s.VendorAddressLine3,
                        s.PhoneNumber,
                        s.FaxNumber,
                        s.Telex,
                        s.ContactPersonFirstName,
                        s.ContactPersonLastName,
                        s.Email,
                        s.OverseasVendorIndicator,
                        Commissions = s.ShopCommissions.Select(sc => new { sc.CategoryId, sc.Commission }),
                        ShopOwner = new
                        {
                            s.User.UserId,
                            s.User.NameEn,
                            s.User.Email,
                            s.User.Phone,
                            s.User.Status,
                            s.User.Position,
                            UserGroup = s.User.UserGroupMaps.Select(ug => ug.UserGroup.GroupNameEn)
                        },
                        ShopImage = new ImageRequest { Url = s.ShopImageUrl },
                        s.ShopDescriptionEn,
                        s.ShopDescriptionTh,
                        s.FloatMessageEn,
                        s.FloatMessageTh,
                        s.Facebook,
                        s.YouTube,
                        s.Twitter,
                        s.Instagram,
                        s.Pinterest,
                        s.GiftWrap,
                        s.TaxInvoice,
                        s.StockAlert,
                        s.RemittanceFaxNumber,
                        City = s.City == null ? null : new
                        {
                            s.City.CityId,
                            s.City.CityName,
                        },
                        Province = s.Province == null ? null : new
                        {
                            s.Province.ProvinceId,
                            s.Province.ProvinceName
                        },
                        District = s.District == null ? null : new
                        {
                            s.District.DistrictId,
                            s.District.DistrictName
                        },
                        Country = s.Country == null ? null : new
                        {
                            s.Country.CountryCode,
                            s.Country.CountryName
                        },
                        PostalCode = s.PostCode == null ? null : new
                        {
                            s.PostCodeId,
                            PostCode = s.PostCode.PostCode1,
                        },
                        s.UrlKey,
                        Users = s.UserShopMaps.Select(u=> u.User.Status.Equals(Constant.STATUS_REMOVE) ? null :
                        new
                        {
                            u.User.UserId,
                            u.User.NameEn,
                            u.User.Email,
                            u.User.Status,
                            u.User.Position,
                            UserGroup = u.User.UserGroupMaps.Select(ug => ug.UserGroup.GroupNameEn)
                        }),
                    }).SingleOrDefault();
                if(shop == null )
                {
                    throw new Exception("Cannot find shop");
                }
                return Request.CreateResponse(HttpStatusCode.OK, shop);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.GetBaseException().Message);
            }
        }

        [Route("api/Shops/Profile")]
        [HttpGet]
		[ClaimsAuthorize(Permission = new string[] { "55" })]
		public HttpResponseMessage GetShopProfile()
        {
            try
            {
                int shopId = User.ShopRequest().ShopId;
                var shop = db.Shops
                    .Where(w => w.ShopId == shopId && (w.Status.Equals(Constant.STATUS_ACTIVE) || w.Status.Equals(Constant.STATUS_NOT_ACTIVE)))
                    .Select(s => new
                    {
                        s.ShopId,
                        s.ShopNameEn,
                        ShopType = new { s.ShopTypeId, s.ShopType.ShopTypeNameEn },
                        s.Status,
                        s.Commission,
                        s.ShopGroup,
                        s.MaxLocalCategory,
                        BankName = s.BankDetail,
                        s.BankAccountName,
                        s.BankAccountNumber,
                        s.TaxPayerId,
                        s.TermPayment,
                        s.VendorTaxRate,
                        s.WithholdingTax,
                        s.Payment,
                        Commissions = s.ShopCommissions.Select(sc => new { sc.CategoryId, sc.Commission, sc.GlobalCategory.NameEn }),
                        ShopOwner = new
                        {
                            s.User.UserId,
                            s.User.NameEn,
                            s.User.Email,
                            s.User.Phone,
                            s.User.Status,
                            s.User.Position,
                            UserGroup = s.User.UserGroupMaps.Select(ug => ug.UserGroup.GroupNameEn)
                        },
                        ShopImage = new ImageRequest { Url = s.ShopImageUrl },
                        s.ShopDescriptionEn,
                        s.ShopDescriptionTh,
                        s.FloatMessageEn,
                        s.FloatMessageTh,
                        s.ShopAddress,
                        s.Facebook,
                        s.YouTube,
                        s.Twitter,
                        s.Instagram,
                        s.Pinterest,
                        s.GiftWrap,
                        s.TaxInvoice,
                        s.StockAlert,
						City = s.City == null ? null : new
						{
							s.City.CityId,
							s.City.CityName,
						},
						Province = s.Province == null ? null : new
						{
							s.Province.ProvinceId,
							s.Province.ProvinceName
						},
						District = s.District == null ? null : new
						{
							s.District.DistrictId,
							s.District.DistrictName
						},
						Country = s.Country == null ? null : new
						{
							s.Country.CountryCode,
							s.Country.CountryName
						},
						PostalCode = s.PostCode == null ? null : new
						{
							s.PostCodeId,
							PostCode = s.PostCode.PostCode1,
						},
						s.UrlKey,
                        s.DomainName,
                        Users = s.UserShopMaps.Select(u => u.User.Status.Equals(Constant.STATUS_REMOVE) ? null :
                        new
                        {
                            u.User.UserId,
                            u.User.NameEn,
                            u.User.Email,
                            u.User.Status,
                            u.User.Position,
                            UserGroup = u.User.UserGroupMaps.Select(ug => ug.UserGroup.GroupNameEn)
                        }),
                    }).SingleOrDefault();
                if (shop == null)
                {
                    throw new Exception("Cannot find shop");
                }
                return Request.CreateResponse(HttpStatusCode.OK,shop);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.GetBaseException().Message);
            }
        }

        [Route("api/Shops/Profile")]
        [HttpPut]
		[ClaimsAuthorize(Permission = new string[] { "55" })]
		public HttpResponseMessage SaveShopProfile(ShopRequest request)
        {
            try
            {
                int shopId = User.ShopRequest().ShopId;
                var shop = db.Shops.Where(w => w.ShopId == shopId && (w.Status.Equals(Constant.STATUS_ACTIVE) || w.Status.Equals(Constant.STATUS_NOT_ACTIVE))).SingleOrDefault();
                if (shop == null || shop.Status.Equals(Constant.STATUS_REMOVE))
                {
                    throw new Exception("Cannot find shop");
                }
                SetupShopProfile(shop, request);
				var email = User.UserRequest().Email;
				var currentDt = SystemHelper.GetCurrentDateTime();
				shop.UpdateBy = email;
                shop.UpdateOn = currentDt;
				SendToCmos(shop, Apis.ShopUpdate, "PUT", email, currentDt, db);
				Util.DeadlockRetry(db.SaveChanges, "Shop");
                return GetShopProfile();
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.GetBaseException().Message);
            }
        }

        [Route("api/Shops")]
        [HttpPost]
		[ClaimsAuthorize(Permission = new string[] { "10" })]

		public HttpResponseMessage AddShop(ShopRequest request)
        {
            Shop shop = null;
            try
            {
				var email = User.UserRequest().Email;
				var currentDt = SystemHelper.GetCurrentDateTime();
				shop = new Shop();
                SetupShopAdmin(shop, request,email, currentDt);
                shop.CreateBy = email;
                shop.CreateOn = currentDt;
                #region Owner
                User owner = new User();
                SetupUser(owner, request.ShopOwner);
                #region Password
                owner.Password = salt.HashPassword(Validation.ValidateString(request.ShopOwner.Password, "Password", true, 100, false));
                owner.PasswordLastChg = string.Empty;
                #endregion
                owner.Status = Constant.STATUS_ACTIVE;
                owner.Type = Constant.USER_TYPE_SELLER;
                owner.CreateBy = email;
                owner.CreateOn = currentDt;
                owner.UpdateBy = email;
                owner.UpdateOn = currentDt;
                owner.UserGroupMaps.Add(new UserGroupMap()
                {
                    GroupId = Constant.SHOP_OWNER_GROUP_ID,
                    CreateBy = email,
                    CreateOn = currentDt,
                    UpdateBy = email,
                    UpdateOn = currentDt,
                });
                shop.UserShopMaps.Add(new UserShopMap()
                {
                    User = owner,
                    CreateBy = email,
                    CreateOn = currentDt,
                    UpdateBy = email,
                    UpdateOn = currentDt,
                });
                shop.User = shop.UserShopMaps.ElementAt(0).User;
				#endregion
				#region Clone Category
				if (request.CloneGlobalCategory)
                {
                    var globalCategory = db.GlobalCategories.Where(w => w.Visibility && Constant.STATUS_ACTIVE.Equals(w.Status));
                    foreach (var cat in globalCategory)
                    {
                        shop.LocalCategories.Add(new LocalCategory()
                        {
                            BannerSmallStatusEn = false,
                            BannerSmallStatusTh = false,
                            BannerStatusEn = cat.BannerStatusEn,
                            BannerStatusTh = cat.BannerStatusTh,
                            CategoryId = db.GetNextLocalCategoryId().SingleOrDefault().Value,
                            DescriptionFullEn = cat.DescriptionFullEn,
                            DescriptionFullTh = cat.DescriptionFullTh,
                            DescriptionMobileEn = cat.DescriptionMobileEn,
                            DescriptionMobileTh = cat.DescriptionMobileTh,
                            DescriptionShortEn = cat.DescriptionShortEn,
                            DescriptionShortTh = cat.DescriptionShortTh,
                            FeatureProductStatus = false,
                            Lft = cat.Lft,
                            Rgt = cat.Rgt,
                            FeatureTitle = string.Empty,
                            NameEn = cat.NameEn,
                            NameTh = cat.NameTh,
                            SortById = cat.SortById,
                            Status = cat.Status,
                            UrlKey = cat.UrlKey,
                            Visibility = cat.Visibility,
                            TitleShowcase = false,
                            CreateBy = email,
                            CreateOn = currentDt,
                            UpdateBy = email,
                            UpdateOn = currentDt,
                        });
                    }
                }
				#endregion
				if(db.Shops.Where(w=>w.UrlKey.Equals(shop.UrlKey)).Count() > 0)
				{
					throw new Exception(string.Concat(shop.UrlKey, " has already been used."));
				}
				shop.User.UserId = db.GetNextUserId().SingleOrDefault().Value;
                shop.ShopId = db.GetNextShopId().SingleOrDefault().Value;
                shop.VendorId = string.Concat(shop.ShopId);
                shop.VendorId = shop.VendorId.PadLeft(5, '0');
                string.Concat("M", shop.VendorId);
                db.Shops.Add(shop);
				SendToCmos(shop, Apis.ShopCreate, "POST", email, currentDt, db);
				Util.DeadlockRetry(db.SaveChanges, "Shop");
				return GetShop(shop.ShopId);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.GetBaseException().Message);
            }
        }

		private void SendToCmos(Shop shop, string url, string method
			, string email, DateTime currentDt, ColspEntities db)
		{
			CmosShopRequest request = new CmosShopRequest()
			{
				BankNumber = shop.BankNumber,
				BankAccountName = shop.BankAccountName,
				BankAccountNumber = shop.BankAccountNumber,
				City = shop.CityId.HasValue && shop.CityId.Value != 0 ? db.Cities.Find(shop.CityId).CityNameEn : string.Empty,
				Commission = shop.Commission,
				ContactPersonFirstName = shop.ContactPersonFirstName,
				ContactPersonLastName = shop.ContactPersonLastName,
				CountryCode = shop.CountryCode,
				CreateBy = shop.CreateBy,
				CreateDt = shop.CreateOn,
				DistrictId = shop.DistrictId,
				DomainName = shop.DomainName,
				Email = shop.Email,
				Facebook = shop.Facebook,
				FaxNumber = shop.FaxNumber,
				FloatMessageEn = shop.FloatMessageEn,
				FloatMessageTh = shop.FloatMessageTh,
				GiftWrap = shop.GiftWrap,
				Instagram = shop.Instagram,
				MaxLocalCategory = shop.MaxLocalCategory,
				OverseasVendorIndicator = shop.OverseasVendorIndicator,
				Payment = shop.Payment,
				PhoneNumber =shop.PhoneNumber,
				Pinterest = shop.Pinterest,
				RemittanceFaxNumber = shop.RemittanceFaxNumber,
				ShopAddress = shop.ShopAddress,
				ShopAppearance = shop.ShopAppearance,
				ShopDescriptionEn = shop.ShopDescriptionEn,
				ShopDescriptionTh = shop.ShopDescriptionTh,
				ShopGroup =shop.ShopGroup,
				ShopId = shop.ShopId,
				ShopImageUrl = shop.ShopImageUrl,
				ShopNameEn = shop.ShopNameEn,
				ShopNameTh = shop.ShopNameTh,
				ShopOwner = shop.ShopOwner,
				ShopTypeId = shop.ShopTypeId,
				State = string.Empty,
				Status = shop.Status,
				StockAlert = shop.StockAlert,
				TaxInvoice = shop.TaxInvoice,
				TaxPayerId = shop.TaxPayerId,
				Telex = shop.Telex,
				TermPaymentCode = shop.TermPaymentCode,
				ThemeId = shop.ThemeId.HasValue ? shop.ThemeId.Value : 0,
				Twitter = shop.Twitter,
				UpdateBy = shop.UpdateBy,
				UpdateDt = shop.UpdateOn,
				UrlKey = shop.UrlKey,
				VendorAddressLine1 = shop.VendorAddressLine1,
				VendorAddressLine2 = shop.VendorAddressLine2,
				VendorAddressLine3 = shop.VendorAddressLine3,
				VendorId = shop.VendorId,
				VendorTaxRate = shop.VendorTaxRateCode,
				WithholdingTaxCode = shop.WithholdingTaxCode,
				YouTube = shop.YouTube,
				ZipCode = shop.PostCodeId.HasValue ? db.PostCodes.Find(shop.PostCodeId).PostCode1 : string.Empty,
			};
			var json = new JavaScriptSerializer().Serialize(request);
			Dictionary<string, string> headers = new Dictionary<string, string>();
			headers.Add(Apis.CmosKeyAppIdKey, Apis.CmosKeyAppIdValue);
			headers.Add(Apis.CmosKeyAppSecretKey, Apis.CmosKeyAppSecretValue);
			SystemHelper.SendRequest(url, method, headers, json, email, currentDt, "SP", "CMOS", db);
		}


		[Route("api/Shops/{shopId}")]
        [HttpPut]
		[ClaimsAuthorize(Permission = new string[] { "10" })]
		public HttpResponseMessage SaveChangeShop([FromUri]int shopId, ShopRequest request)
        {
            Shop shop = null;
            try
            {
                #region Query
                shop = db.Shops
                    .Include(i=>i.User.UserGroupMaps)
                    .Include(i=>i.UserShopMaps)
                    .Include(i=>i.ShopCommissions)
                    .Where(w => w.ShopId == shopId && (w.Status.Equals(Constant.STATUS_ACTIVE) || w.Status.Equals(Constant.STATUS_NOT_ACTIVE))).SingleOrDefault();
                if (shop == null )
                {
                    throw new Exception("Shop not found");
                }
				#endregion
				var email = User.UserRequest().Email;
				var currentDt = SystemHelper.GetCurrentDateTime();
				SetupShopAdmin(shop, request, email, currentDt);
                #region Shop Owner
                if (shop.User != null)
                {
                    if (shop.User.Email != null && shop.User.Email.Equals(request.ShopOwner.Email))
                    {
                        if (!string.IsNullOrEmpty(request.ShopOwner.Password))
                        {
                            shop.User.Password = request.ShopOwner.Password;
                        }
                        shop.User.NameEn = request.ShopOwner.NameEn;
                        shop.User.NameTh = request.ShopOwner.NameTh;
                        shop.User.Division = request.ShopOwner.Division;
                        shop.User.Phone = request.ShopOwner.Phone;
                        shop.User.Position = request.ShopOwner.Position;
                        shop.User.EmployeeId = request.ShopOwner.EmployeeId;
                        shop.User.Status = Constant.STATUS_ACTIVE;
                        shop.User.Type = Constant.USER_TYPE_SELLER;
                    }
                    else
                    {
                        shop.User.Status = Constant.STATUS_REMOVE;
                        db.UserGroupMaps.RemoveRange(shop.User.UserGroupMaps);
                        db.UserShopMaps.RemoveRange(shop.UserShopMaps);

                        shop.UserShopMaps.Add(new UserShopMap()
                        {
                            User = new Entity.Models.User()
                            {
                                Email = Validation.ValidateString(request.ShopOwner.Email, "Email", true, 100, false),
                                Password = request.ShopOwner.Password,
                                NameEn = request.ShopOwner.NameEn,
                                NameTh = request.ShopOwner.NameTh,
                                Division = request.ShopOwner.Division,
                                Phone = request.ShopOwner.Phone,
                                Position = request.ShopOwner.Position,
                                EmployeeId = request.ShopOwner.EmployeeId,
                                Status = Constant.STATUS_ACTIVE,
                                Type = Constant.USER_TYPE_SELLER,
                                CreateBy = email,
                                CreateOn = currentDt,
                                UpdateBy = email,
                                UpdateOn = currentDt,
                                UserGroupMaps = new List<UserGroupMap>() { new UserGroupMap()
								{
									GroupId = Constant.SHOP_OWNER_GROUP_ID,
									CreateBy = email,
									CreateOn = currentDt,
									UpdateBy = email,
									UpdateOn = currentDt,
								}}
                            },
							CreateBy = email,
							CreateOn = currentDt,
							UpdateBy = email,
							UpdateOn = currentDt,
						});
                        shop.User = shop.UserShopMaps.ElementAt(0).User;
                        
                    }
                }
				#endregion
				SendToCmos(shop, Apis.ShopUpdate, "PUT", email, currentDt, db);
				Util.DeadlockRetry(db.SaveChanges, "Shop");
                return GetShop(shop.ShopId);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.GetBaseException().Message);
            }
        }

        [Route("api/Shops")]
        [HttpDelete]
		[ClaimsAuthorize(Permission = new string[] { "10" })]
		public HttpResponseMessage DeleteShop(List<ShopRequest> request)
        {
            try
            {
                if(request == null)
                {
                    throw new Exception("Invalid request");
                }
                var ids = request.Select(s => s.ShopId);
                StringBuilder sb = new StringBuilder();
                string updateShop = string.Concat(
                    "UPDATE [Shop] SET "
                    , "[ShopNameEn] = concat([ShopNameEn],'_DELETE_',[ShopId]), "
                    , "[ShopOwner] = null, "
                    , "[UrlKey] = concat([UrlKey],'_DELETE_',[ShopId]), "
                    , "[Status] = '" , Constant.STATUS_REMOVE, "' " 
                    , "WHERE [ShopId] = @1");
                string updateGroup = string.Concat(
                    "UPDATE [ProductStageGroup] SET "
                    , "[Status] = '", Constant.STATUS_REMOVE, "' "
                    , "WHERE [ShopId] = @1");
                string updateStage = string.Concat(
                    "UPDATE [ProductStage] SET "
                    , "[UrlKey] = concat([Pid],'_DELETE'), "
                    , "[Status] = '", Constant.STATUS_REMOVE, "' "
                    , "WHERE [ShopId] = @1");
                string deleleShopOwner = string.Concat(
                    "DELETE [UserShopMap] "
                    , "WHERE [ShopId] = @1");
                foreach (var id in ids)
                {
                    sb.Append(updateShop.Replace("@1",string.Concat(id)));
                    sb.Append(updateGroup.Replace("@1", string.Concat(id)));
                    sb.Append(updateStage.Replace("@1", string.Concat(id)));
                    sb.Append(deleleShopOwner.Replace("@1", string.Concat(id)));
                }
                db.Database.ExecuteSqlCommand(sb.ToString());
				return Request.CreateResponse(HttpStatusCode.OK, "Delete successful");
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.GetBaseException().Message);
            }
        }

        [Route("api/Shops/Launch")]
        [HttpGet]
        public HttpResponseMessage LaunchShop(string status)
        {
            try
            {
                var shopId = User.ShopRequest().ShopId;
                var shop = db.Shops.Where(w => w.ShopId == shopId && (w.Status.Equals(Constant.STATUS_ACTIVE) || w.Status.Equals(Constant.STATUS_NOT_ACTIVE))).SingleOrDefault();
                if(shop == null)
                {
                    throw new Exception("Shop not found");
                }
                shop.Status = status;
                Util.DeadlockRetry(db.SaveChanges, "Shop");
                User.ShopRequest().Status = status;
                //Cache.Delete(Request.Headers.Authorization.Parameter);
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.GetBaseException().Message);
            }
        }

        [Route("api/Shops/ShopAppearance")]
        [HttpGet]
		[ClaimsAuthorize(Permission = new string[] { "56" })]
		public HttpResponseMessage GetShopAppearance()
        {
            try
            {
                if(User.ShopRequest() == null)
                {
                    throw new Exception("Invalid request");
                }
                var shop = User.ShopRequest();
                var shopId = shop.ShopId;
                int shopTypeId = 0;
                if(shop.ShopType != null)
                {
                    shopTypeId = shop.ShopType.ShopTypeId;
                }
                var shopAppearace = db.Shops
                    .Where(w => w.ShopId == shopId 
                    && (w.Status.Equals(Constant.STATUS_ACTIVE) || w.Status.Equals(Constant.STATUS_NOT_ACTIVE))
                    && (w.ShopType != null && w.ShopType.ShopTypeThemeMaps.Any(a=>a.ShopTypeId == shopTypeId)))
                    .Select(s => new
                {
                    s.ShopId,
                    s.ThemeId,
                    Data = s.ShopAppearance,
                }).SingleOrDefault();
                if(shopAppearace == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new
                    {
                        ShopId = shopId,
                        ThemeId = 0,
                        Data = string.Empty
                    });
                }
                return Request.CreateResponse(HttpStatusCode.OK, shopAppearace);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.GetBaseException().Message);
            }
        }

        [Route("api/Shops/ShopAppearance")]
        [HttpPut]
		[ClaimsAuthorize(Permission = new string[] { "56" })]
		public HttpResponseMessage SaveShopAppearance(ShopAppearanceRequest request)
        {
            try
            {
                if (request == null || User.ShopRequest() == null)
                {
                    throw new Exception("Invalid request");
                }
                var shopId = User.ShopRequest().ShopId;
				var email = User.UserRequest().Email;
				var currentDt = SystemHelper.GetCurrentDateTime();
				Shop shop = db.Shops.Find(shopId);
                //Shop shop = new Shop()
                //{
                //    ShopId = shopId,
                //};
				//db.Shops.Attach(shop);
                //db.Entry(shop).Property(p => p.ThemeId).IsModified = true;
                //db.Entry(shop).Property(p => p.ShopAppearance).IsModified = true;
                //db.Entry(shop).Property(p => p.UpdateBy).IsModified = true;
                //db.Entry(shop).Property(p => p.UpdateOn).IsModified = true;
                shop.ThemeId = request.ThemeId;
                shop.ShopAppearance = request.Data;
                shop.UpdateBy = email;
				shop.UpdateOn = currentDt;
				SendToCmos(shop, Apis.ShopUpdate, "PUT", email, currentDt, db);
                Util.DeadlockRetry(db.SaveChanges, "Shop");
                return GetShopAppearance();
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.GetBaseException().Message);
            }
        }

        [Route("api/TermPayments")]
        [HttpGet]
		[ClaimsAuthorize(Permission = new string[] { "10", "55" })]
		public HttpResponseMessage GetTermPayment()
        {
            try
            {
                var response = db.TermPayments;
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.GetBaseException().Message);
            }
        }

        [Route("api/VendorTaxRates")]
        [HttpGet]
		[ClaimsAuthorize(Permission = new string[] { "10", "55" })]
		public HttpResponseMessage GetVendorTaxRate()
        {
            try
            {
                var response = db.VendorTaxRates.Select(s=>new
                {
                    s.Description,
                    s.VendorTaxRateCode,
                    s.Position
                }).OrderBy(o=>o.Position);
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.GetBaseException().Message);
            }
        }

        [Route("api/WithholdingTaxes")]
        [HttpGet]
		[ClaimsAuthorize(Permission = new string[] { "10", "55" })]
		public HttpResponseMessage GetWithholdingTax()
        {
            try
            {
                var response = db.WithholdingTaxes.Select(s=>new
                {
                    s.Description,
                    s.WithholdingTaxCode
                });
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.GetBaseException().Message);
            }
        }

        [Route("api/BankNames")]
        [HttpGet]
		[ClaimsAuthorize(Permission = new string[] { "10", "55" })]
		public HttpResponseMessage GetBankName()
        {
            try
            {
                var response = db.BankDetails.Select(s=>new
                {
                    s.BankName,
                    s.BankNumber
                });
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.GetBaseException().Message);
            }
        }


        [Route("api/Overseas")]
        [HttpGet]
		[ClaimsAuthorize(Permission = new string[] { "10", "55" })]
		public HttpResponseMessage GetOverseas()
        {
            try
            {
                var overseasList = new List<object>()
                {
                    new
                    {
                        Key = "N",
                        Value = "No"
                    }
                };
                return Request.CreateResponse(HttpStatusCode.OK, overseasList);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.GetBaseException().Message);
            }
        }


        [Route("api/Countries")]
        [HttpGet]
		[ClaimsAuthorize(Permission = new string[] { "10", "55" })]
		public HttpResponseMessage GetCountries()
        {
            try
            {
                var countryList = db.Countries.Select(s => new
                {
                    s.CountryCode,
                    s.CountryName,
                });
                return Request.CreateResponse(HttpStatusCode.OK, countryList);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.GetBaseException().Message);
            }
        }


        [Route("api/Provinces")]
        [HttpGet]
		[ClaimsAuthorize(Permission = new string[] { "10", "55" })]
		public HttpResponseMessage GetProvinces()
        {
            try
            {
                var response = db.Provinces.Select(s => new
                {
                    s.ProvinceId,
                    s.ProvinceName,
                    s.ProvinceNameEn
                });
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.GetBaseException().Message);
            }
        }

        [Route("api/Cities/{provinceId}")]
		[ClaimsAuthorize(Permission = new string[] { "10", "55" })]
		public HttpResponseMessage GetCities([FromUri] int provinceId)
        {
            try
            {
                var response = db.Cities.Where(w=>w.ProvinceId==provinceId).Select(s=> new
                {
                    s.CityId,
                    s.CityName,
                    s.CityNameEn
                });
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.GetBaseException().Message);
            }
        }


        [Route("api/Districts/{cityId}")]
        [HttpGet]
		[ClaimsAuthorize(Permission = new string[] { "10", "55" })]
		public HttpResponseMessage GetDistricts([FromUri] int cityId)
        {
            try
            {
                var response = db.Districts.Where(w => w.CityId == cityId).Select(s => new
                {
                    s.DistrictId,
                    s.DistrictName,
                    s.DistrictNameEn
                });
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.GetBaseException().Message);
            }
        }

        [Route("api/PostCodes/{districtId}")]
        [HttpGet]
		[ClaimsAuthorize(Permission = new string[] { "10", "55" })]
		public HttpResponseMessage GetPostCodes([FromUri] int districtId)
        {
            try
            {
                var response = db.PostCodes.Where(w => w.DistrictId == districtId).Select(s => new
                {
                    s.PostCodeId,
                    PostCode = s.PostCode1,
                });
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.GetBaseException().Message);
            }
        }


        private void SetupShopAdmin(Shop shop, ShopRequest request,string email, DateTime currentDt)
        {
            SetupShopProfile(shop, request);
            shop.ShopGroup = Validation.ValidateString(request.ShopGroup, "Shop Group", true, 2, false, Constant.SHOP_GROUP_MERCHANT, new List<string>() { Constant.SHOP_GROUP_BU, Constant.SHOP_GROUP_INDY,Constant.SHOP_GROUP_MERCHANT });
            #region Shoptype
            if (request.ShopType == null)
            {
                throw new Exception("Shop Type is requeired");
            }
            var typeId = db.ShopTypes.Where(w => w.ShopTypeId == request.ShopType.ShopTypeId).Select(s => s.ShopTypeId).SingleOrDefault();
            if (typeId == 0)
            {
                throw new Exception("Cannot find Shop Type");
            }
            shop.ShopTypeId = typeId;
            #endregion
            shop.Status = Validation.ValidateString(request.Status, "Status", true, 2, true, Constant.STATUS_NOT_ACTIVE, new List<string>() { Constant.STATUS_ACTIVE, Constant.STATUS_NOT_ACTIVE });
            shop.MaxLocalCategory = request.MaxLocalCategory;
            if (shop.MaxLocalCategory == 0)
            {
                shop.MaxLocalCategory = Constant.MAX_LOCAL_CATEGORY;
            }
            shop.Commission = request.Commission;
            #region Shop Commission
            var commissions = shop.ShopCommissions.ToList();
            if (request.Commissions != null && request.Commissions.Count > 0)
            {
                var catIds = request.Commissions.Where(w => w.CategoryId != 0).Select(s => s.CategoryId).ToList();
                var globalCategory = db.GlobalCategories.Where(w => catIds.Contains(w.CategoryId)).ToList();
                foreach (var catMap in request.Commissions)
                {
                    if (catMap.CategoryId == 0 && catMap.Commission == 0)
                    {
                        throw new Exception("Category or commission cannot be null");
                    }
                    var category = globalCategory.Where(w => w.CategoryId == catMap.CategoryId).SingleOrDefault();
                    if (category == null)
                    {
                        throw new Exception("Cannot find category " + catMap.CategoryId);
                    }

                    bool isNew = false;
                    if (commissions == null || commissions.Count == 0)
                    {
                        isNew = true;
                    }
                    if (!isNew)
                    {
                        var current = commissions.Where(w => w.CategoryId == catMap.CategoryId).SingleOrDefault();
                        if (current != null)
                        {
                            current.Commission = catMap.Commission;
                            current.UpdateBy = email;
                            current.UpdateOn = currentDt;
                            commissions.Remove(current);
                        }
                        else
                        {
                            isNew = true;
                        }
                    }
                    if (isNew)
                    {
                        shop.ShopCommissions.Add(new Entity.Models.ShopCommission()
                        {
                            CategoryId = catMap.CategoryId,
                            Commission = catMap.Commission,
                            CreateBy = email,
                            CreateOn = currentDt,
                            UpdateBy = email,
                            UpdateOn = currentDt,
                        });
                    }
                }
            }
            if (commissions != null && commissions.Count > 0)
            {
                db.ShopCommissions.RemoveRange(commissions);
            }
            #endregion
            shop.TaxPayerId = Validation.ValidateString(request.TaxPayerId, "Tax Payer Id", true, 35, false, string.Empty);
            if(request.TermPayment != null && !string.IsNullOrEmpty(request.TermPayment.TermPaymentCode))
            {
                var termCode = db.TermPayments
                    .Where(w => w.TermPaymentCode.Equals(request.TermPayment.TermPaymentCode))
                    .Select(s => s.TermPaymentCode)
                    .SingleOrDefault();
                if (string.IsNullOrEmpty(termCode))
                {
                    throw new Exception("Invalid Term Payment Code");
                }
                shop.TermPaymentCode = termCode;
            }
            shop.Payment = Validation.ValidateString(request.Payment, "Payment", true, 1, false);
            if (request.VendorTaxRate != null && !string.IsNullOrEmpty(request.VendorTaxRate.VendorTaxRateCode))
            {
                var code = db.VendorTaxRates
                    .Where(w => w.VendorTaxRateCode.Equals(request.VendorTaxRate.VendorTaxRateCode))
                    .Select(s => s.VendorTaxRateCode)
                    .SingleOrDefault();
                if (string.IsNullOrEmpty(code))
                {
                    throw new Exception("Invalid Vendor Tax Rate Code");
                }
                shop.VendorTaxRateCode = code;
            }
            if (request.WithholdingTax != null && !string.IsNullOrEmpty(request.WithholdingTax.WithholdingTaxCode))
            {
                var code = db.WithholdingTaxes
                    .Where(w => w.WithholdingTaxCode.Equals(request.WithholdingTax.WithholdingTaxCode))
                    .Select(s => s.WithholdingTaxCode)
                    .SingleOrDefault();
                if (string.IsNullOrEmpty(code))
                {
                    throw new Exception("Invalid Withholding Tax Code");
                }
                shop.WithholdingTaxCode = code;
            }
            if (request.BankName != null && !string.IsNullOrEmpty(request.BankName.BankNumber))
            {
                var code = db.BankDetails
                    .Where(w => w.BankNumber.Equals(request.BankName.BankNumber))
                    .Select(s => s.BankNumber)
                    .SingleOrDefault();
                if (string.IsNullOrEmpty(code))
                {
                    throw new Exception("Invalid Bank Name");
                }
                shop.BankNumber = code;
            }
            shop.BankAccountNumber = Validation.ValidateString(request.BankAccountNumber, "Bank Account Number", true, 100, false);
            shop.BankAccountName = Validation.ValidateString(request.BankAccountName, "Bank Account Name", true, 100, false);
            shop.UpdateBy = email;
            shop.UpdateOn = currentDt;
        }

        private void SetupShopProfile(Shop shop, ShopRequest request)
        {
            shop.ShopNameEn = Validation.ValidateString(request.ShopNameEn, "Shop Name", true, 100, true);
            shop.ShopNameTh = Validation.ValidateString(request.ShopNameTh, "Shop Name (Thai)", false, 100, false, string.Empty);
            shop.ShopImageUrl = string.Empty;
            if (request.ShopImage != null)
            {
                shop.ShopImageUrl = Validation.ValidateString(request.ShopImage.Url, "Image", false, 1000, false, string.Empty);
            }
            shop.ShopDescriptionEn = Validation.ValidateString(request.ShopDescriptionEn, "Shop Description (English)", false, 500, false, string.Empty);
            shop.ShopDescriptionTh = Validation.ValidateString(request.ShopDescriptionTh, "Shop Description (Thai)", false, 500, false, string.Empty);
            shop.FloatMessageEn = Validation.ValidateString(request.FloatMessageEn, "Float Message (English)", false, 100, false, string.Empty);
            shop.FloatMessageTh = Validation.ValidateString(request.FloatMessageTh, "Float Message (Thai)", false, 100, false, string.Empty);
            shop.ShopAddress = Validation.ValidateString(request.ShopAddress, "Shop Address", false, 500, false, string.Empty);
            shop.Facebook = Validation.ValidateString(request.Facebook, "Facebook", false, 500, false, string.Empty);
            shop.YouTube = Validation.ValidateString(request.YouTube, "YouTube", false, 500, false, string.Empty);
            shop.Instagram = Validation.ValidateString(request.Instagram, "Instagram", false, 500, false, string.Empty);
            shop.Pinterest = Validation.ValidateString(request.Pinterest, "Pinterest", false, 500, false, string.Empty);
            shop.Twitter = Validation.ValidateString(request.Twitter, "Twitter", false, 500, false, string.Empty);
            shop.GiftWrap = Validation.ValidateString(request.GiftWrap, "Gift Wrap", true, 1, true, Constant.STATUS_NO, new List<string>() { Constant.STATUS_YES, Constant.STATUS_NO });
            shop.TaxInvoice = Validation.ValidateString(request.TaxInvoice, "Tax Invoice", true, 1, true, Constant.STATUS_NO, new List<string>() { Constant.STATUS_YES, Constant.STATUS_NO });
            shop.StockAlert = Validation.ValidationInteger(request.StockAlert, "Stock Alert", true, int.MaxValue, 0).Value;
			if(shop.StockAlert == 0)
			{
				shop.StockAlert = 3;
			}
            shop.Status = Validation.ValidateString(request.Status, "Status", true, 2, true, Constant.STATUS_NOT_ACTIVE, new List<string>() { Constant.STATUS_NOT_ACTIVE, Constant.STATUS_ACTIVE});
            shop.VendorAddressLine1 = Validation.ValidateString(request.VendorAddressLine1, "Vendor Address Line1", true, 35, false, string.Empty);
            shop.VendorAddressLine2 = Validation.ValidateString(request.VendorAddressLine2, "Vendor Address Line2", true, 35, false, string.Empty);
            shop.VendorAddressLine3 = Validation.ValidateString(request.VendorAddressLine3, "Vendor Address Line3", true, 35, false, string.Empty);
            shop.OverseasVendorIndicator = Validation.ValidateString(request.OverseasVendorIndicator, "Overseas Vendor Indicator", true, 1, true, Constant.STATUS_NO, new List<string>() { Constant.STATUS_NO, Constant.STATUS_YES });
            shop.DomainName = Validation.ValidateString(request.DomainName, "Domain Name", true, 500, false, string.Empty);
            shop.Email = Validation.ValidateString(request.Email, "Email", true, 100, false, string.Empty);
            shop.ContactPersonFirstName = Validation.ValidateString(request.ContactPersonFirstName, "Contact Person First Name", true, 100, false, string.Empty);
            shop.ContactPersonLastName = Validation.ValidateString(request.ContactPersonLastName, "Contact Person Last Name", true, 100, false, string.Empty);
            if (request.Country != null && !string.IsNullOrEmpty(request.Country.CountryCode))
            {
                var id = db.Countries.Where(w => w.CountryCode.Equals(request.Country.CountryCode)).Select(s => s.CountryCode).SingleOrDefault();
                if (string.IsNullOrEmpty(id))
                {
                    throw new Exception("Cannot find city");
                }
                shop.CountryCode = id;
            }
            if (request.Province != null && request.Province.ProvinceId != 0)
            {
                var provinceId = db.Provinces.Where(w => w.ProvinceId == request.Province.ProvinceId).Select(s => s.ProvinceId).SingleOrDefault();
                if (provinceId == 0)
                {
                    throw new Exception("Cannot find Province");
                }
                shop.ProvinceId = provinceId;

                if (request.City != null && request.City.CityId != 0)
                {
                    var cityId = db.Cities.Where(w => w.ProvinceId == shop.ProvinceId && w.CityId == request.City.CityId).Select(s => s.CityId).SingleOrDefault();
                    if (cityId == 0)
                    {
                        throw new Exception("Cannot find City");
                    }
                    shop.CityId = cityId;

                    if (request.District != null && request.District.DistrictId != 0)
                    {
                        var districtId = db.Districts.Where(w => w.CityId == shop.CityId && w.DistrictId == request.District.DistrictId).Select(s => s.DistrictId).SingleOrDefault();
                        if (districtId == 0)
                        {
                            throw new Exception("Cannot find District");
                        }
                        shop.DistrictId = districtId;
                        if (request.PostalCode != null && request.PostalCode.PostCodeId != 0)
                        {
                            var postCodeId = db.PostCodes.Where(w => w.PostCodeId == request.PostalCode.PostCodeId).Select(s => s.PostCodeId).SingleOrDefault();
                            if(postCodeId == 0)
                            {
                                throw new Exception("Cannot find Postal Code");
                            }
                            shop.PostCodeId = postCodeId;
                        }
                    }
                }
            }
			#region Url Key
			Regex rgAlphaNumeric = new Regex(@"[^a-zA-Z0-9_-]");
			if (string.IsNullOrWhiteSpace(request.UrlKey))
			{
				request.UrlKey = shop.ShopNameEn
					.Trim()
					.ToLower()
					.Replace(" ", "-").Replace("_", "-");
				request.UrlKey = rgAlphaNumeric.Replace(request.UrlKey, "");
			}
			else
			{
				request.UrlKey = request.UrlKey
					.Trim()
					.ToLower()
					.Replace(" ", "-").Replace("_", "-");
				request.UrlKey = rgAlphaNumeric.Replace(request.UrlKey, "");
			}
			if (request.UrlKey.Length > 100)
			{
				request.UrlKey = request.UrlKey.Substring(0, 100);
			}
			shop.UrlKey = request.UrlKey;
			#endregion
			shop.PhoneNumber = Validation.ValidateString(request.PhoneNumber, "Phone Number", true, 15, false, string.Empty);
            shop.FaxNumber = Validation.ValidateString(request.FaxNumber, "Fax Number", true, 15, false, string.Empty);
            shop.Telex = Validation.ValidateString(request.Telex, "Telex", true, 15, false, string.Empty);
            shop.OverseasVendorIndicator = Validation.ValidateString(request.OverseasVendorIndicator, "Overseas Vendor Indicator", true, 1, true, Constant.STATUS_NO, new List<string>() { Constant.STATUS_NO, Constant.STATUS_YES });
            shop.RemittanceFaxNumber = Validation.ValidateString(request.Telex, "Remittance Fax Number", true, 18, false, string.Empty);
        }

        private void SetupUser(User user, UserRequest request)
        {
            user.Email = Validation.ValidateString(request.Email, "Email", true, 100, false);
            user.NameEn = Validation.ValidateString(request.NameEn, "Name", true, 100, false);
            user.NameTh = Validation.ValidateString(request.NameTh, "Name", false, 100, false, string.Empty);
            user.Division = Validation.ValidateString(request.Division, "Division", false, 100, false, string.Empty);
            user.Phone = Validation.ValidateString(request.Phone, "Phone", true, 100, false);
            user.Position = Validation.ValidateString(request.Position, "Position", false, 100, false, string.Empty);
            user.EmployeeId = Validation.ValidateString(request.EmployeeId, "Employee Id", false, 100, false, string.Empty);
            user.Mobile = Validation.ValidateString(request.Mobile, "Mobile", false, 20, false, string.Empty);
            user.Fax = Validation.ValidateString(request.Fax, "Fax", false, 20, false, string.Empty);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool ShopExists(int id)
        {
            return db.Shops.Count(e => e.ShopId == id) > 0;
        }
    }
}