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
                    shopId = this.User.ShopRequest().ShopId;
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
                                    cat.UrlKeyEn,
                                    cat.UrlKeyTh,
                                    cat.Visibility,
                                    cat.Status,
                                    cat.UpdatedDt,
                                    cat.CreatedDt,
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
                                                    Modified = p.UpdatedDt,
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
                int shopId = this.User.ShopRequest().ShopId;
                var cat = (from category in db.LocalCategories
                                 where category.CategoryId == categoryId && category.ShopId == shopId
                                 select new
                                 {
                                     category.CategoryId,
                                     category.NameEn,
                                     category.NameTh,
                                     CategoryBannerEn = category.LocalCatImages.Where(w => Constant.LANG_EN.Equals(w.EnTh)).OrderBy(o => o.Position).Select(si => new {
                                         url = si.ImageUrl,
                                         position = si.Position
                                     }),
                                     CategoryBannerTh = category.LocalCatImages.Where(w => Constant.LANG_TH.Equals(w.EnTh)).OrderBy(o => o.Position).Select(si => new {
                                         url = si.ImageUrl,
                                         position = si.Position
                                     }),
                                     FeatureProducts = category.LocalCatFeatureProducts
                                        .Select(s => new
                                        {
                                            s.ProductId,
                                            s.ProductStageGroup.ProductStages.Where(w => w.IsVariant == false).FirstOrDefault().Pid,
                                            s.ProductStageGroup.ProductStages.Where(w => w.IsVariant == false).FirstOrDefault().ProductNameEn
                                        }),
                                     category.DescriptionFullEn,
                                     category.DescriptionFullTh,
                                     category.DescriptionShortEn,
                                     category.DescriptionShortTh,
                                     category.FeatureTitle,
                                     category.UrlKeyEn,
                                     category.UrlKeyTh,
                                     category.Visibility,
                                     category.Status,
                                 }).SingleOrDefault();
                if(cat == null)
                {
                    throw new Exception(HttpErrorMessage.NotFound);
                }
                return Request.CreateResponse(HttpStatusCode.OK, cat);
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
                int shopId = this.User.ShopRequest().ShopId;
                category = new LocalCategory();
                category.ShopId = shopId;
                SetupCategory(category, request);

                #region Banner Image En
                if (request.CategoryBannerEn != null && request.CategoryBannerEn.Count > 0)
                {
                    int position = 0;
                    foreach (ImageRequest img in request.CategoryBannerEn)
                    {
                        category.LocalCatImages.Add(new LocalCatImage()
                        {
                            ImageUrl = img.url,
                            ShopId = shopId,
                            Position = position++,
                            EnTh = Constant.LANG_EN,
                            UpdatedBy = this.User.UserRequest().Email,
                            UpdatedDt = DateTime.Now
                        });
                    }
                }
                #endregion
                #region Banner Image Th
                if (request.CategoryBannerTh != null && request.CategoryBannerTh.Count > 0)
                {
                    int position = 0;
                    foreach (ImageRequest img in request.CategoryBannerTh)
                    {
                        category.LocalCatImages.Add(new LocalCatImage()
                        {
                            ImageUrl = img.url,
                            ShopId = shopId,
                            Position = position++,
                            EnTh = Constant.LANG_TH,
                            UpdatedBy = this.User.UserRequest().Email,
                            UpdatedDt = DateTime.Now
                        });
                    }
                }
                #endregion

                category.Visibility = request.Visibility;
                category.Status = Constant.STATUS_ACTIVE;
                category.CreatedBy = this.User.UserRequest().Email;
                category.CreatedDt = DateTime.Now;
                category.UpdatedBy = this.User.UserRequest().Email;
                category.UpdatedDt = DateTime.Now;
                
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
                category.CategoryId = db.LocalCategoryId().SingleOrDefault().Value;
                if (string.IsNullOrWhiteSpace(request.UrlKeyEn))
                {
                    category.UrlKeyEn = string.Concat(category.NameEn.Replace(" ", "-"), "-", category.CategoryId);
                }
                else
                {
                    category.UrlKeyEn = request.UrlKeyEn.Replace(" ", "-");
                }

                db.LocalCategories.Add(category);
                Util.DeadlockRetry(db.SaveChanges, "LocalCategory");
                return Request.CreateResponse(HttpStatusCode.OK, category);
            }
            catch (Exception e)
            {
                if (category != null && category.CategoryId != 0)
                {
                    db.LocalCategories.Remove(category);
                    Util.DeadlockRetry(db.SaveChanges, "LocalCategory");
                }
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }

        [Route("api/LocalCategories/{categoryId}")]
        [HttpPut]
        public HttpResponseMessage SaveChange(int categoryId, CategoryRequest request)
        {
            try
            {
                int shopId = this.User.ShopRequest().ShopId;
                var category = db.LocalCategories
                    .Where(w => w.CategoryId == categoryId && w.ShopId== shopId)
                    .Include(i=>i.LocalCatImages)
                    .Include(i=>i.LocalCatFeatureProducts).SingleOrDefault();
                if (category == null)
                {
                    throw new Exception("Cannot find selected category");
                }
                SetupCategory(category, request);
                if (string.IsNullOrWhiteSpace(request.UrlKeyEn))
                {
                    category.UrlKeyEn = string.Concat(category.NameEn.Replace(" ", "-"), "-", category.CategoryId);
                }
                else
                {
                    category.UrlKeyEn = request.UrlKeyEn.Replace(" ", "-");
                }

                #region Banner Image En
                var imageOldEn = category.LocalCatImages.Where(w => Constant.LANG_EN.Equals(w.EnTh)).ToList();
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
                                current.ImageUrl = img.url;
                                current.Position = position++;
                                current.UpdatedBy = this.User.UserRequest().Email;
                                current.UpdatedDt = DateTime.Now;
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
                                ImageUrl = img.url,
                                Position = position++,
                                EnTh = Constant.LANG_EN,
                                UpdatedBy = this.User.UserRequest().Email,
                                UpdatedDt = DateTime.Now
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
                var imageOldTh = category.LocalCatImages.Where(w => Constant.LANG_TH.Equals(w.EnTh)).ToList();
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
                                current.ImageUrl = img.url;
                                current.Position = position++;
                                current.UpdatedBy = this.User.UserRequest().Email;
                                current.UpdatedDt = DateTime.Now;
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
                                ImageUrl = img.url,
                                Position = position++,
                                EnTh = Constant.LANG_TH,
                                UpdatedBy = this.User.UserRequest().Email,
                                UpdatedDt = DateTime.Now
                            });
                        }
                    }
                }
                if (imageOldTh != null && imageOldTh.Count > 0)
                {
                    db.LocalCatImages.RemoveRange(imageOldTh);
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
                                    CreatedBy = this.User.UserRequest().Email,
                                    CreatedDt = DateTime.Now,
                                    UpdatedBy = this.User.UserRequest().Email,
                                    UpdatedDt = DateTime.Now
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


                category.Visibility = request.Visibility;
                category.Status = request.Status;
                category.UpdatedBy = this.User.UserRequest().Email;
                category.UpdatedDt = DateTime.Now;
                Util.DeadlockRetry(db.SaveChanges, "LocalCategory");
                return Request.CreateResponse(HttpStatusCode.OK, category);
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
                int shopId = this.User.ShopRequest().ShopId;
                if (shopId == 0)
                {
                    throw new Exception("Shop is invalid. Cannot find shop in session");
                }
                //var catEnList = (from cat in db.LocalCategories
                //                 join proStg in db.ProductStages on cat.CategoryId equals proStg.LocalCatId into j
                //                 from j2 in j.DefaultIfEmpty()
                //                 where cat.ShopId == shopIdRq
                //                 group j2 by cat into g
                //                 select new
                //                 {
                //                     Category = g,
                //                     ProductCount = g.Key.ProductStages.Count
                //                 }).ToList();
                var catEnList = db.LocalCategories.Where(w => w.ShopId == shopId).Include(i => i.ProductStageGroups).ToList();
                foreach (CategoryRequest catRq in request)
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

                    var catEn = catEnList.Where(w => w.CategoryId == catRq.CategoryId).SingleOrDefault();
                    if (catEn == null)
                    {
                        throw new Exception("Category " + catRq.NameEn + " is invalid. Cannot find Category key " + catRq.CategoryId + " in database");
                    }
                    catEn.Lft = catRq.Lft;
                    catEn.Rgt = catRq.Rgt;
                    catEn.UpdatedBy = this.User.UserRequest().Email;
                    catEn.UpdatedDt = DateTime.Now;
                    catEnList.Remove(catEn);
                }
                foreach (var cat in catEnList)
                {
                    if (cat.ProductStageGroups.Count != 0)
                    {
                        db.Dispose();
                        throw new Exception("Cannot delete category <strong>" + cat.NameEn + "</strong> with product associated");
                    }
                    db.LocalCategories.Remove(cat);
                }
                Util.DeadlockRetry(db.SaveChanges, "LocalCategory");
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
                int shopId = this.User.ShopRequest().ShopId;
                var catList = db.LocalCategories.Where(w=>w.ShopId==shopId).ToList();
                if (catList == null || catList.Count == 0)
                {
                    throw new Exception("No category found in this shop");
                }
                foreach (CategoryRequest catRq in request)
                {

                    var current = catList.Where(w => w.CategoryId == catRq.CategoryId).SingleOrDefault();
                    if (current == null)
                    {
                        throw new Exception("Cannot find category " + catRq.CategoryId);
                    }
                    var child = catList.Where(w => w.Lft >= current.Lft && w.Rgt <= current.Rgt);
                    child.ToList().ForEach(f => { f.Visibility = catRq.Visibility ; f.UpdatedBy = this.User.UserRequest().Email; f.UpdatedDt = DateTime.Now; });
                }
                Util.DeadlockRetry(db.SaveChanges, "LocalCategory");
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }

        [Route("api/LocalCategories")]
        [HttpDelete]
        public HttpResponseMessage DeteleCategory(List<CategoryRequest> request)
        {
            try
            {
                int shopId = this.User.ShopRequest().ShopId;
                foreach (CategoryRequest rq in request)
                {
                    var cat = db.LocalCategories.Where(w=>w.CategoryId==rq.CategoryId && w.ShopId==shopId).SingleOrDefault();
                    if (cat == null)
                    {
                        throw new Exception("Cannot find category");
                    }
                    int childSize = cat.Rgt - cat.Lft + 1;
                    //delete
                    db.LocalCategories.Where(w => w.Rgt > cat.Rgt && w.ShopId==shopId).ToList()
                        .ForEach(e => { e.Lft = e.Lft > cat.Rgt ? e.Lft - childSize : e.Lft; e.Rgt = e.Rgt - childSize; });
                    db.LocalCategories.RemoveRange(db.LocalCategories.Where(w => w.Lft >= cat.Lft && w.Rgt <= cat.Rgt && w.ShopId==shopId));
                    break;
                }
                Util.DeadlockRetry(db.SaveChanges, "LocalCategory");
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }


        [Route("api/LocalCategoryImages")]
        [HttpPost]
        public async Task<HttpResponseMessage> UploadFile()
        {
            try
            {
                FileUploadRespond fileUpload = await Util.SetupImage(Request, AppSettingKey.IMAGE_ROOT_PATH, AppSettingKey.LOCAL_CAT_FOLDER, 1500, 1500, 2000, 2000, 5, true);
                return Request.CreateResponse(HttpStatusCode.OK, fileUpload);
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, e);
            }
        }

        private void SetupCategory(LocalCategory category, CategoryRequest request)
        {
            category.NameEn = Validation.ValidateString(request.NameEn, "Category Name (English)", false, 200, true);
            category.NameTh = Validation.ValidateString(request.NameTh, "Category Name (Thai)", false, 200, true);
            category.UrlKeyEn = Validation.ValidateString(request.UrlKeyEn, "Url Key (English)", true, 100, true,string.Empty);
            category.UrlKeyTh = Validation.ValidateString(request.UrlKeyTh, "Url Key (Thai)", true, 100, true, string.Empty);
            category.DescriptionFullEn = Validation.ValidateString(request.DescriptionFullEn, "Category Description (English)", false, Int32.MaxValue, false, string.Empty);
            category.DescriptionFullTh = Validation.ValidateString(request.DescriptionFullTh, "Category Description (Thai)", false, Int32.MaxValue, false, string.Empty);
            category.DescriptionShortEn = Validation.ValidateString(request.DescriptionShortEn, "Category Short Description (English)", false, 500, false, string.Empty);
            category.DescriptionShortTh = Validation.ValidateString(request.DescriptionShortTh, "Category Short Description (Thai)", false, 500, false, string.Empty);
            category.FeatureTitle = Validation.ValidateString(request.FeatureTitle, "Feature Products Title", false, 100, false, string.Empty);
            category.TitleShowcase = request.TitleShowcase;
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