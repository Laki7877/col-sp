using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Colsp.Entity.Models;
using Colsp.Api.Constants;
using Colsp.Model.Requests;
using Colsp.Api.Extensions;
using Colsp.Model.Responses;
using System.Data.SqlClient;
using Colsp.Api.Helpers;
using System.Threading.Tasks;
using System.IO;
using System.Text;

namespace Colsp.Api.Controllers
{
    public class LocalCategoriesController : ApiController
    {
        private ColspEntities db = new ColspEntities();

        [Route("api/LocalCategories")]
        [HttpGet]
        public HttpResponseMessage GetCategoryFromShop([FromUri] ShopRequest request)
        {
            try
            {
                int shopId = 0;
                if (request == null)
                {
                    shopId = User.ShopRequest().ShopId;
                }
                else
                {
                    shopId = request.ShopId;
                }
                var localCat = (from cat in db.LocalCategories
                                where cat.ShopId == shopId
                                select new
                                {
                                    cat.CategoryId,
                                    cat.NameEn,
                                    cat.NameTh,
                                    cat.Lft,
                                    cat.Rgt,
                                    cat.UrlKey,
                                    //cat.UrlKeyTh,
                                    cat.Visibility,
                                    cat.Status,
                                    UpdatedDt = cat.UpdateOn,
                                    CreatedDt = cat.CreateOn,
                                    ProductCount = cat.ProductStageGroups.Count,
                                });
                return Request.CreateResponse(HttpStatusCode.OK, localCat);
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }

        [Route("api/LocalCategories/{categoryId}/ProductStages")]
        [HttpGet]
        public HttpResponseMessage GetProductStage([FromUri] ProductRequest request)
        {
            try
            {
                if(request == null)
                {
                    throw new Exception("Invalid request");
                }
                request.DefaultOnNull();
                var products = db.ProductStages.Where(w => true);
                //var products = db.ProductStageGroups.Where(p => true);
                if (request.SearchText != null)
                {
                    products = products.Where(p => p.Sku.Contains(request.SearchText)
                    || p.ProductNameEn.Contains(request.SearchText)
                    || p.ProductNameTh.Contains(request.SearchText));
                }
                if (request.CategoryId != 0)
                {
                    products = products.Where(p => p.ProductStageGroup.LocalCatId == request.CategoryId);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.NotAcceptable, "Category is invalid");
                }
                var total = products.Count();
                var pagedProducts = products.GroupJoin(db.ProductStageImages,
                                                p => p.Pid,
                                                m => m.Pid,
                                                (p, m) => new
                                                {
                                                    p.Sku,
                                                    p.ProductId,
                                                    p.ProductNameEn,
                                                    p.ProductNameTh,
                                                    p.OriginalPrice,
                                                    p.SalePrice,
                                                    p.Status,
                                                    p.ProductStageGroup.ImageFlag,
                                                    p.ProductStageGroup.InfoFlag,
                                                    Modified = p.UpdateOn,
                                                    ImageUrl = m.Where(w=>w.FeatureFlag == true).FirstOrDefault().ImageUrlEn,
                                                }
                                            )
                                            .Paginate(request);
                var response = PaginatedResponse.CreateResponse(pagedProducts, request, total);
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch(Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }

        [Route("api/LocalCategories/{categoryId}")]
        [HttpGet]
        public HttpResponseMessage GetLocalCategory([FromUri] int categoryId)
        {
            try
            {
                int shopId = User.ShopRequest().ShopId;
                var localCat = (from cat in db.LocalCategories
                                 where cat.CategoryId == categoryId && cat.ShopId == shopId
                                 select new
                                 {
                                     cat.CategoryId,
                                     cat.NameEn,
                                     cat.NameTh,
                                     CategoryBannerEn = cat.LocalCatImages.Where(w => Constant.LANG_EN.Equals(w.EnTh) && Constant.MEDIUM.Equals(w.Type)).OrderBy(o => o.Position).Select(si => new {
                                         ImageId = si.CategoryImageId,
                                         Url = si.ImageUrl,
                                         Position = si.Position,
                                         Link = si.Link
                                     }),
                                     CategoryBannerTh = cat.LocalCatImages.Where(w => Constant.LANG_TH.Equals(w.EnTh) && Constant.MEDIUM.Equals(w.Type)).OrderBy(o => o.Position).Select(si => new {
                                         ImageId = si.CategoryImageId,
                                         Url = si.ImageUrl,
                                         Position = si.Position,
                                         Link = si.Link
                                     }),
                                     CategorySmallBannerEn = cat.LocalCatImages.Where(w => Constant.LANG_EN.Equals(w.EnTh) && Constant.SMALL.Equals(w.Type)).OrderBy(o => o.Position).Select(si => new {
                                         ImageId = si.CategoryImageId,
                                         Url = si.ImageUrl,
                                         Position = si.Position,
                                         Link = si.Link
                                     }),
                                     CategorySmallBannerTh = cat.LocalCatImages.Where(w => Constant.LANG_TH.Equals(w.EnTh) && Constant.SMALL.Equals(w.Type)).OrderBy(o => o.Position).Select(si => new {
                                         ImageId = si.CategoryImageId,
                                         Url = si.ImageUrl,
                                         Position = si.Position,
                                         Link = si.Link
                                     }),
                                     FeatureProducts = cat.LocalCatFeatureProducts
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
                                     //cat.CategoryAbbreviation,
                                     cat.Lft,
                                     cat.Rgt,
                                     cat.UrlKey,
                                     SortBy = cat.SortBy == null ? null : new
                                     {
                                         cat.SortBy.SortById,
                                         cat.SortBy.NameEn,
                                         cat.SortBy.NameTh,
                                         cat.SortBy.SortByName
                                     },
                                     cat.Visibility,
                                     cat.Status,
                                     UpdatedDt = cat.UpdateOn,
                                     CreatedDt = cat.CreateOn,
                                     
                                 }).SingleOrDefault();
                if(localCat == null)
                {
                    throw new Exception(HttpErrorMessage.NotFound);
                }
                return Request.CreateResponse(HttpStatusCode.OK, localCat);
            }
            catch
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, HttpErrorMessage.InternalServerError);
            }
        }

        [Route("api/LocalCategories")]
        [HttpPost]
        public HttpResponseMessage AddGlobalCategory(CategoryRequest request)
        {
            LocalCategory category = null;
            try
            {
                int shopId = User.ShopRequest().ShopId;
                string sql = "SELECT COUNT(Category.CategoryId) FROM (SELECT Node.CategoryId FROM LocalCategory AS Node, LocalCategory AS Parent WHERE Node.Lft BETWEEN Parent.Lft AND Parent.Rgt AND Node.ShopId = @shopid AND Parent.ShopId = @shopid GROUP BY Node.CategoryId, node.lft HAVING (COUNT(parent.CategoryId) - 1) = 0) AS Category";
                var shopCount = db.Database.SqlQuery<int>(sql,new SqlParameter("shopid",shopId)).FirstOrDefault();
                var maxLocalCategory = db.Shops.Where(w => w.ShopId == shopId).Select(s => s.MaxLocalCategory).SingleOrDefault();
                if (shopCount >= maxLocalCategory)
                {
                    throw new Exception("This shop has reached the maximum local category");
                }
                category = new LocalCategory();
                category.ShopId = shopId;
                string email = User.UserRequest().Email;
                DateTime currentDt = DateTime.Now;
                SetupCategory(category, request, shopId, email, currentDt,true);
                int max = db.LocalCategories.Where(w=>w.ShopId==shopId).Select(s=>s.Rgt).DefaultIfEmpty(0).Max();
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
                category.CategoryId = db.GetNextLocalCategoryId().SingleOrDefault().Value;
                if (string.IsNullOrWhiteSpace(request.UrlKey))
                {
                    category.UrlKey = string.Concat(category.NameEn.Replace(" ", "-"), "-", category.CategoryId);
                }
                else
                {
                    category.UrlKey = request.UrlKey.Replace(" ", "-");
                }

                db.LocalCategories.Add(category);
                Util.DeadlockRetry(db.SaveChanges, "LocalCategory");
                return Request.CreateResponse(HttpStatusCode.OK, category);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }

        [Route("api/LocalCategories/{categoryId}")]
        [HttpPut]
        public HttpResponseMessage SaveChange(int categoryId, CategoryRequest request)
        {
            try
            {
                int shopId = User.ShopRequest().ShopId;
                var category = db.LocalCategories
                    .Where(w => w.CategoryId == categoryId && w.ShopId== shopId)
                    .Include(i=>i.LocalCatImages)
                    .Include(i=>i.LocalCatFeatureProducts).SingleOrDefault();
                if (category == null)
                {
                    throw new Exception("Cannot find selected category");
                }
                var email = User.UserRequest().Email;
                var currentDt = DateTime.Now;
                SetupCategory(category, request, shopId, email, currentDt, false);
                #region Url Key
                if (string.IsNullOrWhiteSpace(request.UrlKey))
                {
                    category.UrlKey = string.Concat(category.NameEn.Replace(" ", "-"), "-", category.CategoryId);
                }
                else
                {
                    category.UrlKey = request.UrlKey.Replace(" ", "-");
                }
                #endregion
                #region Local Category Feature Product
                var pidList = category.LocalCatFeatureProducts.ToList();
                if (request.FeatureProducts != null && request.FeatureProducts.Count > 0)
                {
                    var proStageList = db.ProductStages
                            .Where(w => w.ProductStageGroup.LocalCatId == category.CategoryId)
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
                                category.LocalCatFeatureProducts.Add(new LocalCatFeatureProduct()
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
                                throw new Exception("Pid " + pro.Pid + " is not in this local category.");
                            }
                        }
                    }
                }
                if (pidList != null && pidList.Count > 0)
                {
                    db.LocalCatFeatureProducts.RemoveRange(pidList);
                }
                #endregion
                Util.DeadlockRetry(db.SaveChanges, "LocalCategory");
                return GetLocalCategory(category.CategoryId); ;
            }
            catch (DbUpdateException e)
            {
                if (e != null && e.InnerException != null && e.InnerException.InnerException != null)
                {
                    int sqlError = ((SqlException)e.InnerException.InnerException).Number;
                    if (sqlError == 2627)
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable
                           , "URL Key has already been used");
                    }
                }
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, HttpErrorMessage.InternalServerError);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }

        [Route("api/LocalCategories")]
        [HttpPut]
        public HttpResponseMessage SaveChangeLocalCategory(List<CategoryRequest> request)
        {
            try
            {
                if (request == null || User.ShopRequest() == null)
                {
                    throw new Exception("Invalid request");
                }
                var shopId = User.ShopRequest().ShopId;
                string email = User.UserRequest().Email;
                DateTime currentDt = DateTime.Now;
                StringBuilder sb = new StringBuilder();
                string update = "UPDATE LocalCategory SET Lft = @1 , Rgt = @2 , UpdateBy = '@4' , UpdateOn = '@5' WHERE CategoryId = @3 AND ShopId = @6 ;";
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
                        .Replace("@5", currentDt.ToString("yyyy-MM-dd HH:mm:ss"))
                        .Replace("@6", string.Concat(shopId)));
                }
                var reqCatIds = request.Where(w => w.CategoryId != 0).Select(s => s.CategoryId);
                var allIds = db.LocalCategories.Where(w=>w.ShopId==shopId).Select(s => s.CategoryId).ToList();
                var deleteIds = allIds.Where(w => !reqCatIds.Any(a => a == w));
                if (deleteIds != null && deleteIds.Count() > 0)
                {
                    var productMap = db.ProductStageGroups.Where(w => w.ShopId == shopId && deleteIds.Contains(w.LocalCatId.HasValue ? w.LocalCatId.Value : 0)).Select(s => s.LocalCategory.NameEn);
                    if (productMap != null && productMap.Count() > 0)
                    {
                        throw new Exception(string.Concat("Cannot delete local category ", string.Join(",", productMap.Distinct()), " with product associated"));
                    }
                    sb.Append(string.Concat("DELETE LocalCategory WHERE CategoryId IN (", string.Join(",", deleteIds), ");"));
                }

                db.Database.ExecuteSqlCommand(sb.ToString());
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }

        }

        [Route("api/LocalCategories/Visibility")]
        [HttpPut]
        public HttpResponseMessage VisibilityCategory(List<CategoryRequest> request)
        {
            try
            {
                if (request == null || request.Count == 0)
                {
                    throw new Exception("Invalid request");
                }
                int shopId = User.ShopRequest().ShopId;

                var ids = request.Select(s => s.CategoryId);
                var catList = db.LocalCategories.Where(w => w.ShopId == shopId && ids.Contains(w.CategoryId));
                var email = User.UserRequest().Email;
                var currentDt = DateTime.Now;
                foreach (CategoryRequest catRq in request)
                {
                    var current = catList.Where(w => w.CategoryId == catRq.CategoryId).SingleOrDefault();
                    if (current == null)
                    {
                        throw new Exception("Cannot find category " + catRq.CategoryId);
                    }
                    var child = catList.Where(w => w.Lft >= current.Lft && w.Rgt <= current.Rgt);
                    child.ToList().ForEach(f => { f.Visibility = catRq.Visibility ; f.UpdateBy = email; f.UpdateOn = currentDt; });
                }
                Util.DeadlockRetry(db.SaveChanges, "LocalCategory");
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }

        //[Route("api/LocalCategories")]
        //[HttpDelete]
        //public HttpResponseMessage DeteleCategory(List<CategoryRequest> request)
        //{
        //    try
        //    {
        //        int shopId = User.ShopRequest().ShopId;
        //        var ids = request.Select(s => s.CategoryId);

        //        var catList = db.LocalCategories.Where(w => ids.Contains(w.CategoryId));

        //        foreach (CategoryRequest rq in request)
        //        {
        //            var cat = catList.Where(w=>w.CategoryId==rq.CategoryId && w.ShopId==shopId).SingleOrDefault();
        //            if (cat == null)
        //            {
        //                throw new Exception("Cannot find category");
        //            }
        //            int childSize = cat.Rgt - cat.Lft + 1;
        //            //delete
        //            db.LocalCategories.Where(w => w.Rgt > cat.Rgt && w.ShopId==shopId).ToList()
        //                .ForEach(e => { e.Lft = e.Lft > cat.Rgt ? e.Lft - childSize : e.Lft; e.Rgt = e.Rgt - childSize; });
        //            db.LocalCategories.RemoveRange(db.LocalCategories.Where(w => w.Lft >= cat.Lft && w.Rgt <= cat.Rgt && w.ShopId==shopId));
        //            break;
        //        }
        //        Util.DeadlockRetry(db.SaveChanges, "LocalCategory");
        //        return Request.CreateResponse(HttpStatusCode.OK);
        //    }
        //    catch (Exception e)
        //    {
        //        return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
        //    }
        //}

        [Route("api/LocalCategoryImages")]
        [HttpPost]
        public async Task<HttpResponseMessage> UploadFile()
        {
            try
            {
                if(!Request.Content.IsMimeMultipartContent())
                {
                    throw new Exception("In valid content multi-media");
                }
                var streamProvider = new MultipartFormDataStreamProvider(Path.Combine(AppSettingKey.IMAGE_ROOT_PATH, AppSettingKey.LOCAL_CAT_FOLDER));
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
                            AppSettingKey.LOCAL_CAT_FOLDER, 500, 500, 1000, 1000, 5, true);
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
                            AppSettingKey.LOCAL_CAT_FOLDER, 1920, 1080, 1920, 1080, 5, false);
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
                            AppSettingKey.LOCAL_CAT_FOLDER, 1600, 900, 1600, 900, 5, false);
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
                            AppSettingKey.LOCAL_CAT_FOLDER, Constant.ImageRatio.IMAGE_RATIO_16_9);
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

        private void SetupCategory(LocalCategory category, CategoryRequest request, int shopId, string email, DateTime currentDt, bool isNewAdd)
        {
            category.NameEn = Validation.ValidateString(request.NameEn, "Category Name (English)", false, 200, true);
            category.NameTh = Validation.ValidateString(request.NameTh, "Category Name (Thai)", false, 200, true);
            category.UrlKey = Validation.ValidateString(request.UrlKey, "Url Key (English)", true, 100, true,string.Empty);
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
            category.Status = Constant.STATUS_ACTIVE;
            if (request.SortBy.SortById != 0)
            {
                category.SortById = request.SortBy.SortById;
            }
            #region Banner Image En
            var imageOldEn = category.LocalCatImages.Where(w => Constant.LANG_EN.Equals(w.EnTh) && Constant.MEDIUM.Equals(w.Type)).ToList();
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
                            current.ShopId = shopId;
                            current.Type = Constant.LANG_EN;
                            current.Type = Constant.MEDIUM;
                            current.Position = position++;
                            current.UpdateBy = User.UserRequest().Email;
                            current.UpdateOn = DateTime.Now;
                            imageOldEn.Remove(current);
                        }
                        else
                        {
                            isNew = true;
                        }
                    }
                    if (isNew)
                    {
                        category.LocalCatImages.Add(new LocalCatImage()
                        {
                            ImageUrl = img.Url,
                            Link = img.Link,
                            ShopId = shopId,
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
                db.LocalCatImages.RemoveRange(imageOldEn);
            }
            #endregion
            #region Banner Image Th
            var imageOldTh = category.LocalCatImages.Where(w => Constant.LANG_TH.Equals(w.EnTh) && Constant.MEDIUM.Equals(w.Type)).ToList();
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
                            current.ShopId = shopId;
                            current.Type = Constant.LANG_TH;
                            current.Type = Constant.MEDIUM;
                            current.Position = position++;
                            current.UpdateBy = User.UserRequest().Email;
                            current.UpdateOn = DateTime.Now;
                            imageOldTh.Remove(current);
                        }
                        else
                        {
                            isNew = true;
                        }
                    }
                    if (isNew)
                    {
                        category.LocalCatImages.Add(new LocalCatImage()
                        {
                            ImageUrl = img.Url,
                            Link = img.Link,
                            ShopId = shopId,
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
                db.LocalCatImages.RemoveRange(imageOldTh);
            }
            #endregion
            #region Banner Image Small En
            var imageOldSmallEn = category.LocalCatImages.Where(w => Constant.LANG_EN.Equals(w.EnTh) && Constant.SMALL.Equals(w.Type)).ToList();
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
                            current.ShopId = shopId;
                            current.Type = Constant.LANG_EN;
                            current.Type = Constant.SMALL;
                            current.Position = position++;
                            current.UpdateBy = User.UserRequest().Email;
                            current.UpdateOn = DateTime.Now;
                            imageOldEn.Remove(current);
                        }
                        else
                        {
                            isNew = true;
                        }
                    }
                    if (isNew)
                    {
                        category.LocalCatImages.Add(new LocalCatImage()
                        {
                            ImageUrl = img.Url,
                            Link = img.Link,
                            ShopId = shopId,
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
                db.LocalCatImages.RemoveRange(imageOldSmallEn);
            }
            #endregion
            #region Banner Image Th
            var imageOldSmallTh = category.LocalCatImages.Where(w => Constant.LANG_TH.Equals(w.EnTh) && Constant.SMALL.Equals(w.Type)).ToList();
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
                            current.ShopId = shopId;
                            current.Type = Constant.LANG_TH;
                            current.Type = Constant.SMALL;
                            current.Position = position++;
                            current.UpdateBy = User.UserRequest().Email;
                            current.UpdateOn = DateTime.Now;
                            imageOldSmallTh.Remove(current);
                        }
                        else
                        {
                            isNew = true;
                        }
                    }
                    if (isNew)
                    {
                        category.LocalCatImages.Add(new LocalCatImage()
                        {
                            ImageUrl = img.Url,
                            Link = img.Link,
                            ShopId = shopId,
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
                db.LocalCatImages.RemoveRange(imageOldSmallTh);
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

        private bool LocalCategoryExists(int id)
        {
            return db.LocalCategories.Count(e => e.CategoryId == id) > 0;
        }
    }
}