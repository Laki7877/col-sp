using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Colsp.Entity.Models;
using Colsp.Api.Constants;
using Colsp.Model.Requests;
using Colsp.Api.Extensions;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Colsp.Api.Helpers;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Colsp.Api.Filters;

namespace Colsp.Api.Controllers
{
	public class GlobalCategoriesController : ApiController
	{
		private ColspEntities db = new ColspEntities();

		[Route("api/GlobalCategories")]
		[HttpGet]
		[ClaimsAuthorize(Permission = new string[] { "8", "12" , "2", "3", "35", "34" })]
		public HttpResponseMessage GetGlobalCategory()
		{
			try
			{
				var globalCat = (from cat in db.GlobalCategories
								 where cat.CategoryId != 0
								 orderby cat.Lft ascending
								 select new
								 {
									 cat.CategoryId,
									 cat.NameEn,
									 //cat.NameTh,
									 //cat.CategoryAbbreviation,
									 cat.Lft,
									 cat.Rgt,
									 cat.UrlKey,
									 //cat.UrlKeyTh,
									 cat.Visibility,
									 //cat.Status,
									 UpdatedDt = cat.UpdateOn,
									 CreatedDt = cat.CreateOn,
									 cat.Commission,
									 ProductCount = cat.ProductStageGroups.Count(c => !c.Status.Equals(Constant.STATUS_REMOVE)),
									 //+ cat.Products.Count(c => !c.Status.Equals(Constant.STATUS_REMOVE))
									 //+ cat.ProductHistories.Count(c => !c.Status.Equals(Constant.STATUS_REMOVE)),
									 AttributeSetCount = cat.GlobalCatAttributeSetMaps.Count()
									 //AttributeSets = cat.CategoryAttributeSetMaps.Select(s=> new { s.AttributeSetId, s.AttributeSet.AttributeSetNameEn, ProductCount = s.AttributeSet.ProductStages.Count + s.AttributeSet.Products.Count + s.AttributeSet.ProductHistories.Count })
								 });
				if (User.ShopRequest() != null)
				{
					globalCat = globalCat.Where(w => w.Visibility == true);
				}
				return Request.CreateResponse(HttpStatusCode.OK, globalCat);
			}
			catch (Exception e)
			{
				return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
			}
		}

		[Route("api/GlobalCategories/{categoryId}")]
		[HttpGet]
		[ClaimsAuthorize(Permission = new string[] { "8" })]
		public HttpResponseMessage GetGlobalCategory(int categoryId)
		{
			try
			{
				var globalCat = (from cat in db.GlobalCategories
								 where cat.CategoryId != 0 && cat.CategoryId == categoryId
								 select new
								 {
									 cat.CategoryId,
									 cat.NameEn,
									 cat.NameTh,
									 CategoryBannerEn = cat.GlobalCatImages.Where(w => Constant.LANG_EN.Equals(w.EnTh) && Constant.MEDIUM.Equals(w.Type)).OrderBy(o => o.Position).Select(si => new
									 {
										 ImageId = si.CategoryImageId,
										 Url = si.ImageUrl,
										 Position = si.Position,
										 Link = si.Link
									 }),
									 CategoryBannerTh = cat.GlobalCatImages.Where(w => Constant.LANG_TH.Equals(w.EnTh) && Constant.MEDIUM.Equals(w.Type)).OrderBy(o => o.Position).Select(si => new
									 {
										 ImageId = si.CategoryImageId,
										 Url = si.ImageUrl,
										 Position = si.Position,
										 Link = si.Link
									 }),
									 CategorySmallBannerEn = cat.GlobalCatImages.Where(w => Constant.LANG_EN.Equals(w.EnTh) && Constant.SMALL.Equals(w.Type)).OrderBy(o => o.Position).Select(si => new
									 {
										 ImageId = si.CategoryImageId,
										 Url = si.ImageUrl,
										 Position = si.Position,
										 Link = si.Link
									 }),
									 CategorySmallBannerTh = cat.GlobalCatImages.Where(w => Constant.LANG_TH.Equals(w.EnTh) && Constant.SMALL.Equals(w.Type)).OrderBy(o => o.Position).Select(si => new
									 {
										 ImageId = si.CategoryImageId,
										 Url = si.ImageUrl,
										 Position = si.Position,
										 Link = si.Link
									 }),
									 FeatureProducts = cat.GlobalCatFeatureProducts
										.Where(w => !Constant.STATUS_REMOVE.Equals(w.ProductStageGroup.Status) && w.ProductStageGroup.GlobalCatId == categoryId)
										.Select(s => new
										{
											s.ProductId,
											s.ProductStageGroup.ProductStages.Where(w => w.IsVariant == false).FirstOrDefault().Pid,
											s.ProductStageGroup.ProductStages.Where(w => w.IsVariant == false).FirstOrDefault().ProductNameEn
										}),
									 cat.DescriptionFullEn,
									 cat.DescriptionFullTh,
									 cat.DescriptionShortEn,
									 cat.DescriptionShortTh,
									 cat.DescriptionMobileEn,
									 cat.DescriptionMobileTh,
									 cat.FeatureTitle,
									 cat.TitleShowcase,
									 cat.FeatureProductStatus,
									 cat.BannerSmallStatusEn,
									 cat.BannerStatusEn,
									 cat.BannerSmallStatusTh,
									 cat.BannerStatusTh,
									 cat.Lft,
									 cat.Rgt,
									 cat.UrlKey,
									 cat.Visibility,
									 cat.Status,
									 UpdatedDt = cat.UpdateOn,
									 CreatedDt = cat.CreateOn,
									 cat.Commission,
									 SortBy = cat.SortBy == null ? null : new
									 {
										 cat.SortBy.SortById,
										 cat.SortBy.NameEn,
										 cat.SortBy.NameTh,
										 cat.SortBy.SortByName
									 },
									 cat.IsLandingPage,
									 AttributeSets = cat.GlobalCatAttributeSetMaps.Select(s => new
									 {
										 s.AttributeSetId,
										 s.AttributeSet.AttributeSetNameEn,
										 ProductCount = s.AttributeSet.ProductStageGroups.Count
									 })
								 }).SingleOrDefault();
				if (globalCat == null)
				{
					throw new Exception("Cannot find selected category");
				}
				return Request.CreateResponse(HttpStatusCode.OK, globalCat);
			}
			catch (Exception e)
			{
				return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
			}
		}

		[Route("api/GlobalCategories")]
		[HttpPost]
		[ClaimsAuthorize(Permission = new string[] { "8" })]
		public HttpResponseMessage AddGlobalCategory(CategoryRequest request)
		{
			GlobalCategory category = null;
			try
			{
				var email = User.UserRequest().Email;
				var currentDt = SystemHelper.GetCurrentDateTime();
				category = new GlobalCategory();
				SetupCategory(category, request, email, currentDt, true);
				#region Lft-Rgt
				int max = db.GlobalCategories.Select(s => s.Rgt).DefaultIfEmpty(0).Max();
				if (max == 0)
				{
					category.Lft = 1;
					category.Rgt = 2;
				}
				else
				{
					category.Lft = max + 1;
					category.Rgt = max + 2;
				}
				#endregion
				category.CategoryId = db.GetNextGlobalCategoryId().SingleOrDefault().Value;
				#region Url Key
				Regex rgAlphaNumeric = new Regex(@"[^a-zA-Z0-9_-]");
				if (string.IsNullOrWhiteSpace(request.UrlKey))
				{
					request.UrlKey = category.NameEn
						.ToLower()
						.Replace(" ", "-").Replace("_", "-");
					request.UrlKey = rgAlphaNumeric.Replace(request.UrlKey, "");
					if (request.UrlKey.Length > (99 - string.Concat(category.CategoryId).Length))
					{
						request.UrlKey = request.UrlKey.Substring(0, 99 - string.Concat(category.CategoryId).Length);
					}
					category.UrlKey = string.Concat(request.UrlKey, "-", category.CategoryId);
				}
				else
				{
					request.UrlKey = request.UrlKey
						.ToLower()
						.Replace(" ", "-").Replace("_", "-");
					request.UrlKey = rgAlphaNumeric.Replace(request.UrlKey, "");
					if (request.UrlKey.Length > 100)
					{
						request.UrlKey = request.UrlKey.Substring(0, 100);
					}
					category.UrlKey = request.UrlKey;
				}
				#endregion
				db.GlobalCategories.Add(category);
				Util.DeadlockRetry(db.SaveChanges, "GlobalCategory");
				return Request.CreateResponse(HttpStatusCode.OK, category);
			}
			catch (Exception e)
			{
				return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
			}
		}

		[Route("api/GlobalCategories/{categoryId}")]
		[HttpPut]
		[ClaimsAuthorize(Permission = new string[] { "8" })]
		public HttpResponseMessage SaveChange(int categoryId, CategoryRequest request)
		{
			try
			{
				var category = db.GlobalCategories
						.Include(i => i.GlobalCatAttributeSetMaps
						.Select(s => s.AttributeSet))
						.Include(i => i.GlobalCatImages)
						.Include(i => i.GlobalCatFeatureProducts)
					.Where(w => w.CategoryId != 0 && w.CategoryId == categoryId)
					.SingleOrDefault();
				if (category == null)
				{
					throw new Exception("Cannot find selected category");
				}
				var email = User.UserRequest().Email;
				var currentDt = SystemHelper.GetCurrentDateTime();
				SetupCategory(category, request, email, currentDt, false);
				#region Url Key
				Regex rgAlphaNumeric = new Regex(@"[^a-zA-Z0-9_-]");
				if (string.IsNullOrWhiteSpace(request.UrlKey))
				{
					request.UrlKey = category.NameEn
						.ToLower()
						.Replace(" ", "-").Replace("_", "-");
					request.UrlKey = rgAlphaNumeric.Replace(request.UrlKey, "");
					if (request.UrlKey.Length > (99 - string.Concat(category.CategoryId).Length))
					{
						request.UrlKey = request.UrlKey.Substring(0, 99 - string.Concat(category.CategoryId).Length);
					}
					category.UrlKey = string.Concat(request.UrlKey, "-", category.CategoryId);
				}
				else
				{
					request.UrlKey = request.UrlKey
						.ToLower()
						.Replace(" ", "-").Replace("_", "-");
					request.UrlKey = rgAlphaNumeric.Replace(request.UrlKey, "");
					if (request.UrlKey.Length > 100)
					{
						request.UrlKey = request.UrlKey.Substring(0, 100);
					}
					category.UrlKey = request.UrlKey;
				}
				#endregion
				#region Global Category Feature Product
				var pidList = category.GlobalCatFeatureProducts.Distinct().ToList();
				if (request.FeatureProducts != null && request.FeatureProducts.Count > 0)
				{
					var proStageList = db.ProductStageGroups
							.Where(w => w.GlobalCatId == category.CategoryId)
							.Select(s => s.ProductId).ToList();
					foreach (var pro in request.FeatureProducts)
					{
						bool isNew = false;
						if (pidList == null || pidList.Count == 0)
						{
							isNew = true;
						}
						if (!isNew)
						{
							var current = pidList.Where(w => w.ProductId == pro.ProductId).SingleOrDefault();
							if (current != null)
							{
								pidList.Remove(current);
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
								category.GlobalCatFeatureProducts.Add(new GlobalCatFeatureProduct()
								{
									ProductId = pro.ProductId,
									CreateBy = email,
									CreateOn = currentDt,
									UpdateBy = email,
									UpdateOn = currentDt
								});
							}
							else
							{
								throw new Exception("Pid " + pro.Pid + " is not in this global category.");
							}
						}
					}
				}
				if (pidList != null && pidList.Count > 0)
				{
					db.GlobalCatFeatureProducts.RemoveRange(pidList);
				}
				#endregion
				Util.DeadlockRetry(db.SaveChanges, "GlobalCategory");
				return GetGlobalCategory(category.CategoryId);
			}
			catch (Exception e)
			{
				return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
			}
		}

		[Route("api/GlobalCategories/Shift")]
		[HttpPut]
		public HttpResponseMessage ShiftChange(CategoryShiftRequest request)
		{
			try
			{
				var querList = new List<int?> { request.Child, request.Parent, request.Sibling };
				var catList = db.GlobalCategories.Where(w => w.CategoryId != 0 && querList.Contains(w.CategoryId)).ToList();
				if (catList == null || catList.Count == 0)
				{
					throw new Exception("Invalid request");
				}
				var parent = catList.Where(w => w.CategoryId == request.Parent).SingleOrDefault();
				var sibling = catList.Where(w => w.CategoryId == request.Sibling).SingleOrDefault();
				var child = catList.Where(w => w.CategoryId == request.Child).SingleOrDefault();

				if (child == null)
				{
					throw new Exception("Invalid request");
				}
				int childSize = child.Rgt - child.Lft + 1;
				//delete 
				db.GlobalCategories.Where(w => w.Rgt <= child.Rgt && w.Lft >= child.Lft).ToList()
				.ForEach(e => { e.Status = "TM"; });
				db.GlobalCategories.Where(w => w.Rgt > child.Rgt).ToList()
				.ForEach(e => { e.Lft = e.Lft > child.Rgt ? e.Lft - childSize : e.Lft; e.Rgt = e.Rgt - childSize; });

				Util.DeadlockRetry(db.SaveChanges, "GlobalCategory");
				int x = 0;

				if (sibling != null)
				{
					x = sibling.Rgt;
				}
				else if (parent != null)
				{
					x = parent.Lft;
				}

				int offset = x - child.Lft + 1;
				int size = child.Rgt - child.Lft + 1;

				db.GlobalCategories.Where(w => w.Lft >= child.Lft && w.Rgt <= child.Rgt && w.Status == "TM").ToList()
				.ForEach(e => { e.Lft = e.Lft + offset; e.Rgt = e.Rgt + offset; });

				db.GlobalCategories.Where(w => w.Rgt > x && w.Status != "TM").ToList()
				.ForEach(e => { e.Lft = e.Lft > x ? e.Lft + size : e.Lft; e.Rgt = e.Rgt + size; });

				db.GlobalCategories.Where(w => w.Status == "TM").ToList()
				.ForEach(e => { e.Status = "AT"; });
				Util.DeadlockRetry(db.SaveChanges, "GlobalCategory");
				return Request.CreateResponse(HttpStatusCode.OK);
			}
			catch (Exception e)
			{
				return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
			}
		}

		[Route("api/GlobalCategories/Visibility")]
		[HttpPut]
		[ClaimsAuthorize(Permission = new string[] { "8" })]
		public HttpResponseMessage VisibilityCategory(List<CategoryRequest> request)
		{
			try
			{
				if (request == null || request.Count == 0)
				{
					throw new Exception("Invalid request");
				}
				var ids = request.Select(s => s.CategoryId);
				var catList = db.GlobalCategories.Where(w => ids.Contains(w.CategoryId));
				var email = User.UserRequest().Email;
				var currentDt = SystemHelper.GetCurrentDateTime();
				foreach (CategoryRequest catRq in request)
				{
					var current = catList.Where(w => w.CategoryId == catRq.CategoryId).SingleOrDefault();
					if (current == null)
					{
						throw new Exception("Cannot find category " + catRq.CategoryId);
					}
					var child = catList.Where(w => w.Lft >= current.Lft && w.Rgt <= current.Rgt);
					child.ToList().ForEach(f => { f.Visibility = catRq.Visibility; f.UpdateBy = email; f.UpdateOn = currentDt; });
				}
				Util.DeadlockRetry(db.SaveChanges, "GlobalCategory");
				return Request.CreateResponse(HttpStatusCode.OK);
			}
			catch (Exception e)
			{
				return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
			}
		}

		[Route("api/GlobalCategories/{catId}/Attributes")]
		[HttpGet]
		[ClaimsAuthorize(Permission = new string[] { "2", "3", "35", "34" })]
		public HttpResponseMessage GetVarientAttribute(int catId)
		{
			try
			{

				var attribute =
					(from cat in db.GlobalCategories
					 join catMap in db.GlobalCatAttributeSetMaps on cat.CategoryId equals catMap.CategoryId
					 join attrSet in db.AttributeSets on catMap.AttributeSetId equals attrSet.AttributeSetId
					 join attrSetMap in db.AttributeSetMaps on attrSet.AttributeSetId equals attrSetMap.AttributeSetId
					 join attr in db.Attributes on attrSetMap.AttributeId equals attr.AttributeId
					 where cat.CategoryId != 0 && cat.CategoryId == catId && attr.VariantStatus == true
					 select attr);
				var response = new List<IQueryable<Entity.Models.Attribute>>() { attribute };


				if (response == null || response.Count() == 0)
				{
					throw new Exception(HttpErrorMessage.NotFound);
				}
				return Request.CreateResponse(response);
			}
			catch (Exception e)
			{
				return Request.CreateResponse(HttpStatusCode.NotAcceptable, e.Message);
			}
		}

		[Route("api/GlobalCategories/{catId}/AttributeSets")]
		[HttpGet]
		[ClaimsAuthorize(Permission = new string[] { "2", "3", "35", "34" })]
		public HttpResponseMessage GetAttributeSetFromCat(int catId)
		{
			try
			{
				var attributeSet = (from atrS in db.AttributeSets
									where atrS.GlobalCatAttributeSetMaps.Select(s => s.CategoryId).Contains(catId)
									select new
									{
										atrS.AttributeSetId,
										atrS.AttributeSetNameEn,
										atrS.Visibility,
										atrS.Status,
										UpdatedDt = atrS.UpdateOn,
										CreatedDt = atrS.CreateOn,
										AttributeSetMaps = atrS.AttributeSetMaps.Select(s =>
										new
										{
											s.AttributeId,
											s.AttributeSetId,
											Attribute = new
											{
												s.Attribute.AttributeId,
												s.Attribute.AttributeNameEn,
												s.Attribute.DataType,
												s.Attribute.Required,
												s.Attribute.Status,
												s.Attribute.VariantDataType,
												s.Attribute.VariantStatus,
												s.Attribute.DataValidation,
												s.Attribute.DisplayNameEn,
												s.Attribute.DisplayNameTh,
												AttributeValueMaps = s.Attribute.AttributeValueMaps.Select(sv =>
												new
												{
													sv.AttributeId,
													sv.AttributeValueId,
													AttributeValue = new { sv.AttributeValue.AttributeValueId, sv.AttributeValue.AttributeValueEn, sv.AttributeValue.AttributeValueTh }
												})
											}
										}),
										AttributeSetTagMaps = atrS.AttributeSetTags.Select(s => new { s.AttributeSetId, Tag = new { TagName = s.Tag } }),
										AttributeCount = atrS.AttributeSetMaps.Count(),
										CategoryCount = atrS.GlobalCatAttributeSetMaps.Count(),
										ProductCount = atrS.ProductStageGroups.Count()
									});
				return Request.CreateResponse(HttpStatusCode.OK, attributeSet);

			}
			catch (Exception e)
			{
				return Request.CreateResponse(HttpStatusCode.NotAcceptable, e.Message);
			}
		}

		[Route("api/GlobalCategories")]
		[HttpPut]
		[ClaimsAuthorize(Permission = new string[] { "8" })]
		public HttpResponseMessage SaveChangeGlobalCategory(List<CategoryRequest> request)
		{
			try
			{

				if (request == null)
				{
					throw new Exception("Invalid request");
				}
				var email = User.UserRequest().Email;
				var currentDt = SystemHelper.GetCurrentDateTime();
				StringBuilder sb = new StringBuilder();
				string update = "UPDATE GlobalCategory SET Lft = @1 , Rgt = @2 , UpdateBy = '@4' , UpdateOn = '@5' WHERE CategoryId = @3 ;";
				foreach (var catRq in request)
				{
					if (catRq.Lft >= catRq.Rgt)
					{
						throw new Exception("Category " + catRq.NameEn + " is invalid. Node is not properly formated");
					}
					var validate = request.Where(w => w.Lft == catRq.Lft || w.Rgt == catRq.Rgt).ToList();
					if (validate != null && validate.Count > 1)
					{
						throw new Exception("Category " + catRq.NameEn + " is invalid. Node child has duplicated left or right key");
					}
					if (catRq.CategoryId == 0)
					{
						throw new Exception("Category " + catRq.NameEn + " is invalid.");
					}
					sb.Append(update
						.Replace("@1", string.Concat(catRq.Lft))
						.Replace("@2", string.Concat(catRq.Rgt))
						.Replace("@3", string.Concat(catRq.CategoryId))
						.Replace("@4", email)
						.Replace("@5", currentDt.ToString("yyyy-MM-dd HH:mm:ss")));
				}
				var reqCatIds = request.Where(w => w.CategoryId != 0).Select(s => s.CategoryId);
				var allIds = db.GlobalCategories.Select(s => s.CategoryId).ToList();
				var deleteIds = allIds.Where(w => !reqCatIds.Any(a => a == w));
				if (deleteIds != null && deleteIds.Count() > 0)
				{
					var productMap = db.ProductStageGroups.Where(w => deleteIds.Contains(w.GlobalCatId)).Select(s => s.GlobalCategory.NameEn);
					if (productMap != null && productMap.Count() > 0)
					{
						throw new Exception(string.Concat("Cannot delete global category ", string.Join(",", productMap.Distinct()), " with product associated"));
					}
					var attributesetMap = db.GlobalCatAttributeSetMaps.Where(w => deleteIds.Contains(w.CategoryId)).Select(s => s.GlobalCategory.NameEn);
					if (attributesetMap != null && attributesetMap.Count() > 0)
					{
						throw new Exception(string.Concat("Cannot delete global category ", string.Join(",", attributesetMap.Distinct()), " with attribute set associated"));
					}
					sb.Append(string.Concat("DELETE GlobalCategory WHERE CategoryId IN (", string.Join(",", deleteIds), ");"));
				}

				db.Database.ExecuteSqlCommand(sb.ToString());
				return Request.CreateResponse(HttpStatusCode.OK);

			}
			catch (Exception e)
			{
				return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
			}
		}

		[Route("api/GlobalCategoryImages")]
		[HttpPost]
		[ClaimsAuthorize(Permission = new string[] { "8" })]
		public async Task<HttpResponseMessage> UploadFile()
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
				if ("Logo".Equals(type))
				{
					foreach (MultipartFileData fileData in streamProvider.FileData)
					{
						fileUpload = Util.SetupImage(Request,
							fileData,
							AppSettingKey.IMAGE_ROOT_FOLDER,
							AppSettingKey.GLOBAL_CAT_FOLDER, 500, 500, 1000, 1000, 5, true);
						break;
					}

				}
				else if ("Banner".Equals(type))
				{
					foreach (MultipartFileData fileData in streamProvider.FileData)
					{
						fileUpload = Util.SetupImage(Request,
							fileData,
							AppSettingKey.IMAGE_ROOT_FOLDER,
							AppSettingKey.GLOBAL_CAT_FOLDER, 1920, 1080, 1920, 1080, 5, false);
						break;
					}
				}
				else if ("SmallBanner".Equals(type))
				{
					foreach (MultipartFileData fileData in streamProvider.FileData)
					{
						fileUpload = Util.SetupImage(Request,
							fileData,
							AppSettingKey.IMAGE_ROOT_FOLDER,
							AppSettingKey.GLOBAL_CAT_FOLDER, 1600, 900, 1600, 900, 5, false);
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
							AppSettingKey.GLOBAL_CAT_FOLDER, Constant.ImageRatio.IMAGE_RATIO_16_9);
						break;
					}
				}
				#endregion
				return Request.CreateResponse(HttpStatusCode.OK, fileUpload);

			}
			catch (Exception e)
			{
				return Request.CreateResponse(HttpStatusCode.InternalServerError, e);
			}
		}

		private void SetupCategory(GlobalCategory category, CategoryRequest request, string email, DateTime currentDt, bool isNewAdd)
		{
			category.NameEn = Validation.ValidateString(request.NameEn, "Category Name (English)", false, 200, true, string.Empty);
			category.NameTh = Validation.ValidateString(request.NameTh, "Category Name (Thai)", false, 200, true, string.Empty);
			category.Commission = Validation.ValidateDecimal(request.Commission, "Commission (%)", true, 20, 2, true).Value;
			category.DescriptionFullEn = Validation.ValidateString(request.DescriptionFullEn, "Category Description (English)", false, int.MaxValue, false, string.Empty);
			category.DescriptionFullTh = Validation.ValidateString(request.DescriptionFullTh, "Category Description (Thai)", false, int.MaxValue, false, string.Empty);
			category.DescriptionShortEn = Validation.ValidateString(request.DescriptionShortEn, "Category Short Description (English)", false, 500, false, string.Empty);
			category.DescriptionShortTh = Validation.ValidateString(request.DescriptionShortTh, "Category Short Description (Thai)", false, 500, false, string.Empty);
			category.DescriptionMobileEn = Validation.ValidateString(request.DescriptionMobileEn, "Category Mobile Description (English)", false, int.MaxValue, false, string.Empty);
			category.DescriptionMobileTh = Validation.ValidateString(request.DescriptionMobileTh, "Category Mobile Description (Thai)", false, int.MaxValue, false, string.Empty);
			category.BannerSmallStatusEn = request.BannerSmallStatusEn;
			category.BannerStatusEn = request.BannerStatusEn;
			category.BannerSmallStatusTh = request.BannerSmallStatusTh;
			category.BannerStatusTh = request.BannerStatusTh;
			category.FeatureProductStatus = request.FeatureProductStatus;
			category.FeatureTitle = Validation.ValidateString(request.FeatureTitle, "Feature Products Title", false, 100, false, string.Empty);
			category.TitleShowcase = request.TitleShowcase;
			category.Visibility = request.Visibility;
			category.IsLandingPage = request.IsLandingPage;
			category.Status = Constant.STATUS_ACTIVE;
			if (request.SortBy.SortById != 0)
			{
				category.SortById = request.SortBy.SortById;
			}
			#region Banner Image En
			var imageOldEn = category.GlobalCatImages.Where(w => Constant.LANG_EN.Equals(w.EnTh) && Constant.MEDIUM.Equals(w.Type)).ToList();
			if (request.CategoryBannerEn != null && request.CategoryBannerEn.Count > 0)
			{
				int position = 0;
				foreach (ImageRequest img in request.CategoryBannerEn)
				{
					bool isNew = false;
					if (imageOldEn == null || imageOldEn.Count == 0)
					{
						isNew = true;
					}
					if (!isNew)
					{
						var current = imageOldEn.Where(w => w.CategoryImageId == img.ImageId).SingleOrDefault();
						if (current != null)
						{
							current.ImageUrl = img.Url;
							current.Link = img.Link;
							current.EnTh = Constant.LANG_EN;
							current.Type = Constant.MEDIUM;
							current.Position = position++;
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
						category.GlobalCatImages.Add(new GlobalCatImage()
						{
							ImageUrl = img.Url,
							Link = img.Link,
							Position = position++,
							EnTh = Constant.LANG_EN,
							Type = Constant.MEDIUM,
							CreateBy = email,
							CreateOn = currentDt,
							UpdateBy = email,
							UpdateOn = currentDt
						});
					}
				}
			}
			if (imageOldEn != null && imageOldEn.Count > 0)
			{
				db.GlobalCatImages.RemoveRange(imageOldEn);
			}
			#endregion
			#region Banner Image Th
			var imageOldTh = category.GlobalCatImages.Where(w => Constant.LANG_TH.Equals(w.EnTh) && Constant.MEDIUM.Equals(w.Type)).ToList();
			if (request.CategoryBannerTh != null && request.CategoryBannerTh.Count > 0)
			{
				int position = 0;
				foreach (ImageRequest img in request.CategoryBannerTh)
				{
					bool isNew = false;
					if (imageOldTh == null || imageOldTh.Count == 0)
					{
						isNew = true;
					}
					if (!isNew)
					{
						var current = imageOldTh.Where(w => w.CategoryImageId == img.ImageId).SingleOrDefault();
						if (current != null)
						{
							current.ImageUrl = img.Url;
							current.Link = img.Link;
							current.EnTh = Constant.LANG_TH;
							current.Type = Constant.MEDIUM;
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
						category.GlobalCatImages.Add(new GlobalCatImage()
						{
							ImageUrl = img.Url,
							Link = img.Link,
							Position = position++,
							EnTh = Constant.LANG_TH,
							Type = Constant.MEDIUM,
							CreateBy = email,
							CreateOn = currentDt,
							UpdateBy = email,
							UpdateOn = currentDt
						});
					}
				}
			}
			if (imageOldTh != null && imageOldTh.Count > 0)
			{
				db.GlobalCatImages.RemoveRange(imageOldTh);
			}
			#endregion
			#region Banner Image Small En
			var imageOldSmallEn = category.GlobalCatImages.Where(w => Constant.LANG_EN.Equals(w.EnTh) && Constant.SMALL.Equals(w.Type)).ToList();
			if (request.CategorySmallBannerEn != null && request.CategorySmallBannerEn.Count > 0)
			{
				int position = 0;
				foreach (ImageRequest img in request.CategorySmallBannerEn)
				{
					bool isNew = false;
					if (imageOldSmallEn == null || imageOldSmallEn.Count == 0)
					{
						isNew = true;
					}
					if (!isNew)
					{
						var current = imageOldSmallEn.Where(w => w.CategoryImageId == img.ImageId).SingleOrDefault();
						if (current != null)
						{
							current.ImageUrl = img.Url;
							current.Link = img.Link;
							current.EnTh = Constant.LANG_EN;
							current.Type = Constant.SMALL;
							current.Position = position++;
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
						category.GlobalCatImages.Add(new GlobalCatImage()
						{
							ImageUrl = img.Url,
							Link = img.Link,
							Position = position++,
							EnTh = Constant.LANG_EN,
							Type = Constant.SMALL,
							CreateBy = email,
							CreateOn = currentDt,
							UpdateBy = email,
							UpdateOn = currentDt
						});
					}
				}
			}
			if (imageOldSmallEn != null && imageOldSmallEn.Count > 0)
			{
				db.GlobalCatImages.RemoveRange(imageOldSmallEn);
			}
			#endregion
			#region Banner Image Th
			var imageOldSmallTh = category.GlobalCatImages.Where(w => Constant.LANG_TH.Equals(w.EnTh) && Constant.SMALL.Equals(w.Type)).ToList();
			if (request.CategorySmallBannerTh != null && request.CategorySmallBannerTh.Count > 0)
			{
				int position = 0;
				foreach (ImageRequest img in request.CategorySmallBannerTh)
				{
					bool isNew = false;
					if (imageOldSmallTh == null || imageOldSmallTh.Count == 0)
					{
						isNew = true;
					}
					if (!isNew)
					{
						var current = imageOldSmallTh.Where(w => w.CategoryImageId == img.ImageId).SingleOrDefault();
						if (current != null)
						{
							current.ImageUrl = img.Url;
							current.Link = img.Link;
							current.EnTh = Constant.LANG_TH;
							current.Type = Constant.SMALL;
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
						category.GlobalCatImages.Add(new GlobalCatImage()
						{
							ImageUrl = img.Url,
							Link = img.Link,
							Position = position++,
							EnTh = Constant.LANG_TH,
							Type = Constant.SMALL,
							CreateBy = email,
							CreateOn = currentDt,
							UpdateBy = email,
							UpdateOn = currentDt
						});
					}
				}
			}
			if (imageOldSmallTh != null && imageOldSmallTh.Count > 0)
			{
				db.GlobalCatImages.RemoveRange(imageOldSmallTh);
			}
			#endregion
			#region Attribute Set
			var setList = db.GlobalCatAttributeSetMaps.Where(w => w.CategoryId == request.CategoryId).ToList();
			if (request.AttributeSets != null && request.AttributeSets.Count > 0)
			{
				foreach (AttributeSetRequest mapRq in request.AttributeSets)
				{
					bool addNew = false;
					if (setList == null || setList.Count == 0)
					{
						addNew = true;
					}
					if (!addNew)
					{
						var current = setList.Where(w => w.AttributeSetId == mapRq.AttributeSetId).SingleOrDefault();
						if (current != null)
						{
							setList.Remove(current);
						}
						else
						{
							addNew = true;
						}
					}
					if (addNew)
					{
						category.GlobalCatAttributeSetMaps.Add(new GlobalCatAttributeSetMap()
						{
							AttributeSetId = mapRq.AttributeSetId,
							CategoryId = category.CategoryId,
							CreateBy = email,
							CreateOn = currentDt,
							UpdateBy = email,
							UpdateOn = currentDt
						});
					}
				}
			}
			if (setList != null && setList.Count > 0)
			{
				db.GlobalCatAttributeSetMaps.RemoveRange(setList);
			}
			#endregion
			#region Create update
			if (isNewAdd)
			{
				category.CreateBy = email;
				category.CreateOn = currentDt;
			}
			category.UpdateBy = email;
			category.UpdateOn = currentDt;
			#endregion
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				db.Dispose();
			}
			base.Dispose(disposing);
		}

		private bool GlobalCategoryExists(int id)
		{
			return db.GlobalCategories.Count(e => e.CategoryId == id) > 0;
		}


		//[Route("api/GlobalCategories")]
		//[HttpDelete]
		//public HttpResponseMessage DeteleCategory(List<CategoryRequest> request)
		//{
		//    try
		//    {
		//        var ids = request.Select(s => s.CategoryId);

		//        var productMap = db.ProductStageGroups.Where(w => ids.Contains(w.GlobalCatId)).Select(s => s.GlobalCategory.NameEn);
		//        if (productMap != null && productMap.Count() > 0)
		//        {
		//            throw new Exception(string.Concat("Cannot delete global category ", string.Join(",", productMap), " with product associated"));
		//        }
		//        var attributesetMap = db.GlobalCatAttributeSetMaps.Where(w => ids.Contains(w.CategoryId)).Select(s => s.GlobalCategory.NameEn);
		//        if (attributesetMap != null && attributesetMap.Count() > 0)
		//        {
		//            throw new Exception(string.Concat("Cannot delete global category ", string.Join(",", attributesetMap), " with attribute set associated"));
		//        }

		//        var catList = db.GlobalCategories.Where(w => w.CategoryId != 0 && ids.Contains(w.CategoryId));

		//        foreach (CategoryRequest catRq in request)
		//        {
		//            var current = catList.Where(w => w.CategoryId == catRq.CategoryId).SingleOrDefault();
		//            if (current == null)
		//            {
		//                throw new Exception("Cannot find category " + catRq.CategoryId);
		//            }

		//            int childSize = current.Rgt - current.Lft + 1;
		//            //delete
		//            db.GlobalCategories.Where(w => w.Rgt > current.Rgt).ToList()
		//                .ForEach(e => { e.Lft = e.Lft > current.Rgt ? e.Lft - childSize : e.Lft; e.Rgt = e.Rgt - childSize; });
		//            db.GlobalCategories.RemoveRange(db.GlobalCategories.Where(w => w.Lft >= current.Lft && w.Rgt <= current.Rgt));
		//            break;
		//        }
		//        Util.DeadlockRetry(db.SaveChanges, "GlobalCategory");
		//        return Request.CreateResponse(HttpStatusCode.OK);
		//    }
		//    catch (Exception e)
		//    {
		//        return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
		//    }
		//}
	}
}