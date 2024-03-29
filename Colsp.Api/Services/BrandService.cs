﻿using Colsp.Api.Constants;
using Colsp.Api.Extensions;
using Colsp.Api.Helpers;
using Colsp.Entity.Models;
using Colsp.Model.Requests;
using Colsp.Model.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace Colsp.Api.Services
{
	public class BrandService
	{
		public void SetupBrand(Brand brand, BrandRequest request, string email, DateTime currentDt, bool isNewAdd, ColspEntities db)
		{
			brand.BrandNameEn = Validation.ValidateString(request.BrandNameEn, "Brand Name (English)", true, 100, true);
			brand.BrandNameTh = Validation.ValidateString(request.BrandNameTh, "Brand Name (Thai)", true, 100, false, string.Empty);
			brand.DisplayNameEn = Validation.ValidateString(request.DisplayNameEn, "Brand Display Name (Thai)", true, 300, false);
			brand.DisplayNameTh = Validation.ValidateString(request.DisplayNameTh, "Brand Display Name (Thai)", true, 300, false);
			brand.DescriptionFullEn = Validation.ValidateString(request.DescriptionFullEn, "Brand Description (English)", false, int.MaxValue, false, string.Empty);
			brand.DescriptionFullTh = Validation.ValidateString(request.DescriptionFullTh, "Brand Description (Thai)", false, int.MaxValue, false, string.Empty);
			brand.DescriptionShortEn = Validation.ValidateString(request.DescriptionShortEn, "Brand Short Description (English)", false, 500, false, string.Empty);
			brand.DescriptionShortTh = Validation.ValidateString(request.DescriptionShortTh, "Brand Short Description (Thai)", false, 500, false, string.Empty);
			brand.DescriptionMobileEn = Validation.ValidateString(request.DescriptionMobileEn, "Brand Mobile Description (English)", false, int.MaxValue, false, string.Empty);
			brand.DescriptionMobileTh = Validation.ValidateString(request.DescriptionMobileTh, "Brand Mobile Description (Thai)", false, int.MaxValue, false, string.Empty);
			brand.FeatureTitle = Validation.ValidateString(request.FeatureTitle, "Feature Products Title", false, 100, false, string.Empty);
			brand.TitleShowcase = request.TitleShowcase;
			brand.BannerSmallStatusEn = request.BannerSmallStatusEn;
			brand.BannerStatusEn = request.BannerStatusEn;
			brand.BannerSmallStatusTh = request.BannerSmallStatusTh;
			brand.BannerStatusTh = request.BannerStatusTh;
			brand.FeatureProductStatus = request.FeatureProductStatus;
			brand.IsLandingPage = request.IsLandingPage;
			if (request.SortBy.SortById != 0)
			{
				brand.SortById = request.SortBy.SortById;
			}
			if (request.SEO != null)
			{
				brand.MetaDescriptionEn = Validation.ValidateString(request.SEO.MetaDescriptionEn, "Meta Description (English)", false, 500, false, string.Empty);
				brand.MetaDescriptionTh = Validation.ValidateString(request.SEO.MetaDescriptionTh, "Meta Description (Thai)", false, 500, false, string.Empty);
				brand.MetaKeyEn = Validation.ValidateString(request.SEO.MetaKeywordEn, "Meta Keywords (English)", false, 500, false, string.Empty);
				brand.MetaKeyTh = Validation.ValidateString(request.SEO.MetaKeywordTh, "Meta Keywords (Thai)", false, 500, false, string.Empty);
				brand.MetaTitleEn = Validation.ValidateString(request.SEO.MetaTitleEn, "Meta Title (English)", false, 100, false, string.Empty);
				brand.MetaTitleTh = Validation.ValidateString(request.SEO.MetaTitleTh, "Meta Title (Thai)", false, 100, false, string.Empty);
				brand.SeoEn = Validation.ValidateString(request.SEO.SeoEn, "SEO (English)", false, 100, false, string.Empty);
				brand.SeoTh = Validation.ValidateString(request.SEO.SeoTh, "SEO (Thai)", false, 100, false, string.Empty);
			}
			brand.Status = request.Status;
			brand.PicUrl = Validation.ValidateString(request.BrandImage.Url, "Logo", false, 500, false, string.Empty);
			#region Url Key
			Regex rgAlphaNumeric = new Regex(@"[^a-zA-Z0-9_-]");
			if (request.SEO == null || string.IsNullOrWhiteSpace(request.SEO.ProductUrlKeyEn))
			{
				request.SEO.ProductUrlKeyEn = brand.BrandNameEn
					.Trim()
					.ToLower()
					.Replace(" ", "-").Replace("_", "-");
				request.SEO.ProductUrlKeyEn = rgAlphaNumeric.Replace(request.SEO.ProductUrlKeyEn, "");
			}
			else
			{
				request.SEO.ProductUrlKeyEn = request.SEO.ProductUrlKeyEn
					.Trim()
					.ToLower()
					.Replace(" ", "-").Replace("_", "-");
				request.SEO.ProductUrlKeyEn = rgAlphaNumeric.Replace(request.SEO.ProductUrlKeyEn, "");
			}
			if (request.SEO.ProductUrlKeyEn.Length > 100)
			{
				request.SEO.ProductUrlKeyEn = request.SEO.ProductUrlKeyEn.Substring(0, 100);
			}
			brand.UrlKey = request.SEO.ProductUrlKeyEn;
			#endregion
			#region BranImage En
			var imageOldEn = brand.BrandImages.Where(w => Constant.LANG_EN.Equals(w.EnTh) && Constant.MEDIUM.Equals(w.Type)).ToList();
			if (request.BrandBannerEn != null && request.BrandBannerEn.Count > 0)
			{
				int position = 0;
				foreach (ImageRequest img in request.BrandBannerEn)
				{
					bool isNew = false;
					if (imageOldEn == null || imageOldEn.Count == 0)
					{
						isNew = true;
					}
					if (!isNew)
					{
						var current = imageOldEn.Where(w => w.BrandImageId == img.ImageId).SingleOrDefault();
						if (current != null)
						{
							current.ImageUrl = img.Url;
							current.Position = position++;
							current.Type = Constant.MEDIUM;
							current.EnTh = Constant.LANG_EN;
							current.Link = img.Link;
							current.UpdateBy = email;
							current.UpdateOn = currentDt;
							imageOldEn.Remove(current);
						}
						else
						{
							isNew = true;
						}
					}
					if (isNew)
					{
						brand.BrandImages.Add(new BrandImage()
						{
							ImageUrl = img.Url,
							Link = img.Link,
							Position = position++,
							EnTh = Constant.LANG_EN,
							Type = Constant.MEDIUM,
							UpdateBy = email,
							UpdateOn = currentDt,
							CreateBy = email,
							CreateOn = currentDt
						});
					}
				}
			}
			if (imageOldEn != null && imageOldEn.Count > 0)
			{
				db.BrandImages.RemoveRange(imageOldEn);
			}
			#endregion
			#region BranImage Th
			var imageOldTh = brand.BrandImages.Where(w => Constant.LANG_TH.Equals(w.EnTh) && Constant.MEDIUM.Equals(w.Type)).ToList();
			if (request.BrandBannerTh != null && request.BrandBannerTh.Count > 0)
			{
				int position = 0;
				foreach (ImageRequest img in request.BrandBannerTh)
				{
					bool isNew = false;
					if (imageOldTh == null || imageOldTh.Count == 0)
					{
						isNew = true;
					}
					if (!isNew)
					{
						var current = imageOldTh.Where(w => w.BrandImageId == img.ImageId).SingleOrDefault();
						if (current != null)
						{
							current.ImageUrl = img.Url;
							current.Link = img.Link;
							current.Type = Constant.MEDIUM;
							current.EnTh = Constant.LANG_TH;
							current.Position = position++;
							current.UpdateBy = email;
							current.UpdateOn = currentDt;
							imageOldTh.Remove(current);
						}
						else
						{
							isNew = true;
						}
					}
					if (isNew)
					{
						brand.BrandImages.Add(new BrandImage()
						{
							ImageUrl = img.Url,
							Link = img.Link,
							Position = position++,
							EnTh = Constant.LANG_TH,
							Type = Constant.MEDIUM,
							UpdateBy = email,
							UpdateOn = currentDt,
							CreateBy = email,
							CreateOn = currentDt
						});
					}
				}
			}
			if (imageOldTh != null && imageOldTh.Count > 0)
			{
				db.BrandImages.RemoveRange(imageOldTh);
			}
			#endregion
			#region Small BranImage En
			var imageOldSmallEn = brand.BrandImages.Where(w => Constant.LANG_EN.Equals(w.EnTh) && Constant.SMALL.Equals(w.Type)).ToList();
			if (request.BrandSmallBannerEn != null && request.BrandSmallBannerEn.Count > 0)
			{
				int position = 0;
				foreach (ImageRequest img in request.BrandSmallBannerEn)
				{
					bool isNew = false;
					if (imageOldSmallEn == null || imageOldSmallEn.Count == 0)
					{
						isNew = true;
					}
					if (!isNew)
					{
						var current = imageOldSmallEn.Where(w => w.BrandImageId == img.ImageId).SingleOrDefault();
						if (current != null)
						{
							current.ImageUrl = img.Url;
							current.Link = img.Link;
							current.Type = Constant.SMALL;
							current.EnTh = Constant.LANG_EN;
							current.Position = position++;
							current.UpdateBy = email;
							current.UpdateOn = currentDt;
							imageOldSmallEn.Remove(current);
						}
						else
						{
							isNew = true;
						}
					}
					if (isNew)
					{
						brand.BrandImages.Add(new BrandImage()
						{
							ImageUrl = img.Url,
							Link = img.Link,
							Position = position++,
							EnTh = Constant.LANG_EN,
							Type = Constant.SMALL,
							UpdateBy = email,
							UpdateOn = currentDt,
							CreateBy = email,
							CreateOn = currentDt
						});
					}
				}
			}
			if (imageOldSmallEn != null && imageOldSmallEn.Count > 0)
			{
				db.BrandImages.RemoveRange(imageOldSmallEn);
			}
			#endregion
			#region BranImage Th
			var imageOldSmallTh = brand.BrandImages.Where(w => Constant.LANG_TH.Equals(w.EnTh) && Constant.SMALL.Equals(w.Type)).ToList();
			if (request.BrandSmallBannerTh != null && request.BrandSmallBannerTh.Count > 0)
			{
				int position = 0;
				foreach (ImageRequest img in request.BrandSmallBannerTh)
				{
					bool isNew = false;
					if (imageOldSmallTh == null || imageOldSmallTh.Count == 0)
					{
						isNew = true;
					}
					if (!isNew)
					{
						var current = imageOldSmallTh.Where(w => w.BrandImageId == img.ImageId).SingleOrDefault();
						if (current != null)
						{
							current.ImageUrl = img.Url;
							current.Link = img.Link;
							current.Type = Constant.SMALL;
							current.EnTh = Constant.LANG_TH;
							current.Position = position++;
							current.UpdateBy = email;
							current.UpdateOn = currentDt;
							imageOldSmallTh.Remove(current);
						}
						else
						{
							isNew = true;
						}
					}
					if (isNew)
					{
						brand.BrandImages.Add(new BrandImage()
						{
							ImageUrl = img.Url,
							Link = img.Link,
							Position = position++,
							EnTh = Constant.LANG_TH,
							Type = Constant.SMALL,
							UpdateBy = email,
							UpdateOn = currentDt,
							CreateBy = email,
							CreateOn = currentDt
						});
					}
				}
			}
			if (imageOldSmallTh != null && imageOldSmallTh.Count > 0)
			{
				db.BrandImages.RemoveRange(imageOldSmallTh);
			}
			#endregion
			#region Brand Feature Product
			var brandProList = brand.BrandFeatureProducts.Distinct().ToList();
			if (request.FeatureProducts != null && request.FeatureProducts.Count > 0)
			{
				int brandIdTmp = brand.BrandId;
				var proStageList = db.ProductStageGroups
					.Where(w => w.BrandId == brandIdTmp)
					.Select(s => s.ProductId).ToList();
				foreach (var pro in request.FeatureProducts)
				{
					bool isNew = false;
					if (brandProList == null || brandProList.Count == 0)
					{
						isNew = true;
					}
					if (!isNew)
					{
						var current = brandProList.Where(w => w.ProductId == pro.ProductId).SingleOrDefault();
						if (current != null)
						{
							brandProList.Remove(current);
						}
						else
						{
							isNew = true;
						}
					}
					if (isNew)
					{
						var isPid = proStageList.Where(w => w == pro.ProductId).ToList();
						if (isPid != null && isPid.Count > 0)
						{
							brand.BrandFeatureProducts.Add(new BrandFeatureProduct()
							{
								BrandId = brand.BrandId,
								ProductId = pro.ProductId,
								CreateBy = email,
								CreateOn = currentDt,
								UpdateBy = email,
								UpdateOn = currentDt,
							});
						}
						else
						{
							throw new Exception(string.Concat("Pid ", pro.Pid, " is not in this brand."));
						}
					}
				}
			}
			if (brandProList != null && brandProList.Count > 0)
			{
				db.BrandFeatureProducts.RemoveRange(brandProList);
			}
			#endregion
			#region Create Update
			if (isNewAdd)
			{
				brand.CreateBy = email;
				brand.CreateOn = currentDt;
			}
			brand.UpdateBy = email;
			brand.UpdateOn = currentDt;
			#endregion
		}

		public BrandRequest GetBrand(int brandId, ColspEntities db)
		{
			var brand = db.Brands.Where(w => w.BrandId == brandId).Select(s => new
			{
				s.BrandId,
				s.BrandNameEn,
				s.BrandNameTh,
				s.DisplayNameEn,
				s.DisplayNameTh,
				s.DescriptionFullEn,
				s.DescriptionFullTh,
				s.DescriptionShortEn,
				s.DescriptionShortTh,
				s.DescriptionMobileEn,
				s.DescriptionMobileTh,
				s.FeatureProductStatus,
				s.MetaDescriptionEn,
				s.MetaDescriptionTh,
				s.MetaKeyEn,
				s.MetaKeyTh,
				s.MetaTitleEn,
				s.MetaTitleTh,
				UrlEn = s.UrlKey,
				s.FeatureTitle,
				s.TitleShowcase,
				s.PicUrl,
				s.SeoEn,
				s.SeoTh,
				s.SortBy,
				s.BannerStatusEn,
				s.BannerSmallStatusEn,
				s.BannerStatusTh,
				s.BannerSmallStatusTh,
				s.Status,
				s.IsLandingPage,
				BrandImages = s.BrandImages.Select(si => new
				{
					si.EnTh,
					si.Link,
					si.Type,
					si.Position,
					si.BrandImageId,
					si.ImageUrl,
				}),
				BrandFeatureProducts = s.BrandFeatureProducts
						.Where(w => !Constant.STATUS_REMOVE.Equals(w.ProductStageGroup.Status) && w.ProductStageGroup.BrandId.HasValue && w.ProductStageGroup.BrandId == brandId)
						.Select(sp => new
						{
							ProductStageGroup = sp.ProductStageGroup == null ? null : new
							{
								sp.ProductStageGroup.ProductId,
								ProductStages = sp.ProductStageGroup.ProductStages.Select(st => new
								{
									st.IsVariant,
									st.Pid,
									st.ProductNameEn
								})
							},
						}),
			}).SingleOrDefault();
			if (brand == null)
			{
				throw new Exception(string.Concat("Cannot find brand id ", brandId));
			}
			BrandRequest response = new BrandRequest();
			response.BrandId = brand.BrandId;
			response.BrandNameEn = brand.BrandNameEn;
			response.BrandNameTh = brand.BrandNameTh;
			response.DisplayNameEn = brand.DisplayNameEn;
			response.DisplayNameTh = brand.DisplayNameTh;
			response.DescriptionFullEn = brand.DescriptionFullEn;
			response.DescriptionFullTh = brand.DescriptionFullTh;
			response.DescriptionShortEn = brand.DescriptionShortEn;
			response.DescriptionShortTh = brand.DescriptionShortTh;
			response.DescriptionMobileEn = brand.DescriptionMobileEn;
			response.DescriptionMobileTh = brand.DescriptionMobileTh;
			response.BannerStatusEn = brand.BannerStatusEn;
			response.BannerSmallStatusEn = brand.BannerSmallStatusEn;
			response.BannerStatusTh = brand.BannerStatusTh;
			response.BannerSmallStatusTh = brand.BannerSmallStatusTh;
			response.IsLandingPage = brand.IsLandingPage;
			if (brand.SortBy != null)
			{
				response.SortBy = new SortByRequest()
				{
					NameEn = brand.SortBy.NameEn,
					NameTh = brand.SortBy.NameTh,
					SortByName = brand.SortBy.SortByName
				};
			}
			response.SEO = new SEORequest();
			response.SEO.MetaDescriptionEn = brand.MetaDescriptionEn;
			response.SEO.MetaDescriptionTh = brand.MetaDescriptionTh;
			response.SEO.MetaKeywordEn = brand.MetaKeyEn;
			response.SEO.MetaKeywordTh = brand.MetaKeyTh;
			response.SEO.MetaTitleEn = brand.MetaTitleEn;
			response.SEO.MetaTitleTh = brand.MetaTitleTh;
			response.SEO.ProductUrlKeyEn = brand.UrlEn;
			response.SEO.SeoEn = brand.SeoEn;
			response.SEO.SeoTh = brand.SeoTh;
			response.FeatureTitle = brand.FeatureTitle;
			response.TitleShowcase = brand.TitleShowcase;
			response.Status = brand.Status;
			if (brand.BrandImages != null)
			{
				var productImgEn = brand.BrandImages.Where(w => Constant.LANG_EN.Equals(w.EnTh) && Constant.MEDIUM.Equals(w.Type)).OrderBy(o => o.Position).ToList();
				foreach (var img in productImgEn)
				{
					response.BrandBannerEn.Add(new ImageRequest()
					{
						ImageId = img.BrandImageId,
						Url = img.ImageUrl,
						Link = img.Link,
						Position = img.Position,
					});
				}
				var productImgTh = brand.BrandImages.Where(w => Constant.LANG_TH.Equals(w.EnTh) && Constant.MEDIUM.Equals(w.Type)).OrderBy(o => o.Position).ToList();
				foreach (var img in productImgTh)
				{
					response.BrandBannerTh.Add(new ImageRequest()
					{
						ImageId = img.BrandImageId,
						Url = img.ImageUrl,
						Link = img.Link,
						Position = img.Position,
					});
				}

				var productImgSmallEn = brand.BrandImages.Where(w => Constant.LANG_EN.Equals(w.EnTh) && Constant.SMALL.Equals(w.Type)).OrderBy(o => o.Position).ToList();
				foreach (var img in productImgSmallEn)
				{
					response.BrandSmallBannerEn.Add(new ImageRequest()
					{
						ImageId = img.BrandImageId,
						Url = img.ImageUrl,
						Link = img.Link,
						Position = img.Position,
					});
				}
				var productImgSmallTh = brand.BrandImages.Where(w => Constant.LANG_TH.Equals(w.EnTh) && Constant.SMALL.Equals(w.Type)).OrderBy(o => o.Position).ToList();
				foreach (var img in productImgSmallTh)
				{
					response.BrandSmallBannerTh.Add(new ImageRequest()
					{
						ImageId = img.BrandImageId,
						Url = img.ImageUrl,
						Link = img.Link,
						Position = img.Position,
					});
				}


			}
			if (brand.BrandFeatureProducts != null)
			{
				foreach (var pro in brand.BrandFeatureProducts)
				{
					response.FeatureProducts.Add(new ProductRequest()
					{
						ProductId = pro.ProductStageGroup.ProductId,
						Pid = pro.ProductStageGroup.ProductStages.Where(w => w.IsVariant == false).SingleOrDefault().Pid,
						ProductNameEn = pro.ProductStageGroup.ProductStages.Where(w => w.IsVariant == false).SingleOrDefault().ProductNameEn
					});
				}
			}
			if (!string.IsNullOrEmpty(brand.PicUrl))
			{
				response.BrandImage = new ImageRequest();
				response.BrandImage.Url = brand.PicUrl;
			}
			return response;
		}

		public void DeleteBrand(List<BrandRequest> request, ColspEntities db)
		{
			if (request == null)
			{
				throw new Exception("Invalid request");
			}
			var ids = request.Select(s => s.BrandId).ToList();

			var productMap = db.ProductStageGroups.Where(w => ids.Contains(w.BrandId.HasValue ? w.BrandId.Value : 0)).Select(s => s.Brand.BrandNameEn);
			if (productMap != null && productMap.Count() > 0)
			{
				throw new Exception("Cannot delete brand imaginary_brand because it has been associated with products.");
			}
			var brandList = db.Brands.Where(w => ids.Contains(w.BrandId));
			foreach (BrandRequest brandRq in request)
			{
				var current = brandList.Where(w => w.BrandId == brandRq.BrandId).SingleOrDefault();
				if (current == null)
				{
					throw new Exception(string.Concat("Cannot find brand id ", brandRq.BrandId));
				}
				db.Brands.Remove(current);
			}
		}

		public object GetBrand(BrandRequest request,ShopRequest shopUser,List<BrandRequest> brandUser, ColspEntities db)
		{
			var brands = db.Brands.Select(s => new
			{
				s.BrandId,
				s.BrandNameEn,
				s.BrandNameTh,
				s.DisplayNameEn,
				UpdatedDt = s.UpdateOn,
				s.Status
			});
			if (request == null)
			{
				return brands;
			}
			request.DefaultOnNull();
			if (request.SearchText != null)
			{
				brands = brands.Where(b => b.BrandNameEn.Contains(request.SearchText)
				|| b.BrandNameTh.Contains(request.SearchText)
				|| b.DisplayNameEn.Contains(request.SearchText)
				|| b.BrandId.ToString().Contains(request.SearchText));
			}
			if (request.BrandId != 0)
			{
				brands = brands.Where(p => p.BrandId.Equals(request.BrandId));
			}
			if (shopUser != null)
			{
				brands = brands.Where(w => w.Status.Equals(Constant.STATUS_ACTIVE));
				if (brandUser != null)
				{
					var ids = brandUser.Select(s => s.BrandId);
					brands = brands.Where(w => ids.Contains(w.BrandId));
				}
			}
			var total = brands.Count();
			var pagedbrand = brands.Paginate(request);
			var response = PaginatedResponse.CreateResponse(pagedbrand, request, total);
			return response;
		}
	}
}