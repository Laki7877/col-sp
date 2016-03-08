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
using Colsp.Api.Helper;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.IO;
using Colsp.Model.Responses;
using Colsp.Api.Helpers;

namespace Colsp.Api.Controllers
{
    public class GlobalCategoriesController : ApiController
    {
        private ColspEntities db = new ColspEntities();

        [Route("api/GlobalCategories")]
        [HttpGet]
        public HttpResponseMessage GetGlobalCategory()
        {
            try
            {
                var globalCat = (from cat in db.GlobalCategories
                                 orderby cat.Lft ascending
                                 select new
                                {
                                    cat.CategoryId,
                                    cat.NameEn,
                                    //cat.NameTh,
                                    cat.CategoryAbbreviation,
                                    cat.Lft,
                                    cat.Rgt,
                                    //cat.UrlKeyEn,
                                    //cat.UrlKeyTh,
                                    cat.Visibility,
                                    //cat.Status,
                                    //cat.UpdatedDt,
                                    //cat.CreatedDt,
                                    //cat.Commission,
                                    ProductCount = cat.ProductStages.Count(c => !c.Status.Equals(Constant.STATUS_REMOVE)) 
                                                    + cat.Products.Count(c => !c.Status.Equals(Constant.STATUS_REMOVE))
                                                    + cat.ProductHistories.Count(c => !c.Status.Equals(Constant.STATUS_REMOVE)),
                                     AttributeSetCount = cat.CategoryAttributeSetMaps.Count()
                                     //AttributeSets = cat.CategoryAttributeSetMaps.Select(s=> new { s.AttributeSetId, s.AttributeSet.AttributeSetNameEn, ProductCount = s.AttributeSet.ProductStages.Count + s.AttributeSet.Products.Count + s.AttributeSet.ProductHistories.Count })
                                 });
                if (this.User.HasPermission("Manage Global Category"))
                {

                }
                if (this.User.HasPermission("Add Product"))
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
        public HttpResponseMessage GetGlobalCategory(int categoryId)
        {
            try
            {
                var globalCat = (from cat in db.GlobalCategories
                                 where cat.CategoryId==categoryId
                                 select new
                                 {
                                     cat.CategoryId,
                                     cat.NameEn,
                                     cat.NameTh,
                                     BannerImageEn = cat.GlobalCatImages.Where(w=>Constant.LANG_EN.Equals(w.EnTh)).OrderBy(o=>o.Position).Select(si=>new {
                                         url = si.ImageUrl,
                                         position = si.Position
                                     }),
                                     BannerImageTh = cat.GlobalCatImages.Where(w => Constant.LANG_TH.Equals(w.EnTh)).OrderBy(o => o.Position).Select(si => new {
                                         url = si.ImageUrl,
                                         position = si.Position
                                     }),
                                     FeatureProducts = cat.GlobalCatFeatureProducts.Select(s=>new { s.ProductId,s.ProductStage.Pid, s.ProductStage.ProductNameEn }),
                                     //cat.CategoryAbbreviation,
                                     //cat.Lft,
                                     //cat.Rgt,
                                     cat.UrlKeyEn,
                                     cat.UrlKeyTh,
                                     cat.Visibility,
                                     cat.Status,
                                     //cat.UpdatedDt,
                                     //cat.CreatedDt,
                                     cat.Commission,

                                     //ProductCount = cat.ProductStages.Count(c => !c.Status.Equals(Constant.STATUS_REMOVE))
                                     //               + cat.Products.Count(c => !c.Status.Equals(Constant.STATUS_REMOVE))
                                     //               + cat.ProductHistories.Count(c => !c.Status.Equals(Constant.STATUS_REMOVE)),
                                     AttributeSets = cat.CategoryAttributeSetMaps.Select(s => new { s.AttributeSetId, s.AttributeSet.AttributeSetNameEn, ProductCount = s.AttributeSet.ProductStages.Count + s.AttributeSet.Products.Count + s.AttributeSet.ProductHistories.Count })
                                 }).SingleOrDefault();
                if(globalCat == null)
                {
                    throw new Exception("Cannot find selected category");
                }
                return Request.CreateResponse(HttpStatusCode.OK, globalCat);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable,e.Message);
            }
        }

        [Route("api/GlobalCategories")]
        [HttpPost]
        public HttpResponseMessage AddGlobalCategory(CategoryRequest request)
        {
            GlobalCategory category = null;
            try
            {
                category = new GlobalCategory();
                string abbr = AutoGenerate.NextCatAbbre(db);
                category.CategoryAbbreviation = abbr;
                SetupCategory(category, request);

                category.Visibility = request.Visibility;
                category.Status = Constant.STATUS_ACTIVE;
                category.CreatedBy = this.User.UserRequest().Email;
                category.CreatedDt = DateTime.Now;
                category.UpdatedBy = this.User.UserRequest().Email;
                category.UpdatedDt = DateTime.Now;
                
                int max = db.GlobalCategories.Select(s=>s.Rgt).DefaultIfEmpty(0).Max();
                if(max == 0)
                {
                    category.Lft = 1;
                    category.Rgt = 2;
                }
                else
                {
                    category.Lft = max + 1;
                    category.Rgt = max + 2;
                }
                category.GlobalCategoryPID = new GlobalCategoryPID()
                {
                    CategoryId = category.CategoryId,
                    CategoryAbbreviation = AutoGenerate.NextCatAbbre(db),
                    CurrentKey = "11111"
                };
                #region Banner Image En
                if (request.CategoryBannerEn != null && request.CategoryBannerEn.Count > 0)
                {
                    int position = 0;
                    foreach (ImageRequest img in request.CategoryBannerEn)
                    {
                        category.GlobalCatImages.Add(new GlobalCatImage()
                        {
                            ImageUrl = img.url,
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
                        category.GlobalCatImages.Add(new GlobalCatImage()
                        {
                            ImageUrl = img.url,
                            Position = position++,
                            EnTh = Constant.LANG_TH,
                            UpdatedBy = this.User.UserRequest().Email,
                            UpdatedDt = DateTime.Now
                        });
                    }
                }
                #endregion
                if (string.IsNullOrWhiteSpace(request.UrlKeyEn))
                {
                    category.UrlKeyEn = string.Concat(category.NameEn.Replace(" ", "-"),"-",category.CategoryId);
                }
                else
                {
                    category.UrlKeyEn = request.UrlKeyEn.Trim().Replace(" ", "-");
                }

                if (request.AttributeSets != null && request.AttributeSets.Count > 0)
                {
                   
                    foreach (AttributeSetRequest mapRq in request.AttributeSets)
                    {
                        category.CategoryAttributeSetMaps.Add(new CategoryAttributeSetMap()
                        {
                            AttributeSetId = mapRq.AttributeSetId.Value,
                            CreatedBy = this.User.UserRequest().Email,
                            CreatedDt = DateTime.Now,
                            UpdatedBy = this.User.UserRequest().Email,
                            UpdatedDt = DateTime.Now,
                        });
                    }
                }
                db.GlobalCategories.Add(category);
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK, category);
            }
            catch (DbUpdateException e)
            {
                if (category != null && category.CategoryId != 0)
                {
                    db.GlobalCategories.Remove(category);
                    db.SaveChanges();
                }
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
                if (category != null && category.CategoryId != 0)
                {
                    db.GlobalCategories.Remove(category);
                    db.SaveChanges();
                }
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }

        [Route("api/GlobalCategories/{categoryId}")]
        [HttpPut]
        public HttpResponseMessage SaveChange(int categoryId, CategoryRequest request)
        {
            try
            {
                var category = db.GlobalCategories
                    .Include(i => i.CategoryAttributeSetMaps
                    .Select(s => s.AttributeSet))
                    .Include(i=>i.GlobalCatImages)
                    .Include(i=>i.GlobalCatFeatureProducts)
                    .Where(w => w.CategoryId == categoryId).SingleOrDefault();
                if (category == null)
                {
                    throw new Exception("Cannot find selected category");
                }
                SetupCategory(category, request);
                if (string.IsNullOrWhiteSpace(request.UrlKeyEn))
                {
                    category.UrlKeyEn = string.Concat(category.NameEn.Replace(" ", "-"),"-", category.CategoryId);
                }
                else
                {
                    category.UrlKeyEn = request.UrlKeyEn.Replace(" ", "-");
                }
                category.Visibility = request.Visibility;
                category.Status = Constant.STATUS_ACTIVE;
                category.UpdatedBy = this.User.UserRequest().Email;
                category.UpdatedDt = DateTime.Now;
                #region Banner Image En
                var imageOldEn = category.GlobalCatImages.Where(w => Constant.LANG_EN.Equals(w.EnTh)).ToList();
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
                            category.GlobalCatImages.Add(new GlobalCatImage()
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
                    db.GlobalCatImages.RemoveRange(imageOldEn);
                }
                #endregion
                #region Banner Image Th
                var imageOldTh = category.GlobalCatImages.Where(w => Constant.LANG_TH.Equals(w.EnTh)).ToList();
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
                            category.GlobalCatImages.Add(new GlobalCatImage()
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
                    db.GlobalCatImages.RemoveRange(imageOldTh);
                }
                #endregion
                #region Global Category Feature Product
                var pidList = category.GlobalCatFeatureProducts.ToList();
                if (request.FeatureProducts != null && request.FeatureProducts.Count > 0)
                {
                    var proStageList = db.ProductStages
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
                                    CreatedBy = this.User.UserRequest().Email,
                                    CreatedDt = DateTime.Now,
                                    UpdatedBy = this.User.UserRequest().Email,
                                    UpdatedDt = DateTime.Now
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
                #region Attribute Set
                var setList = db.CategoryAttributeSetMaps.Where(w => w.CategoryId == categoryId).ToList();
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
                                current.UpdatedBy = this.User.UserRequest().Email;
                                current.UpdatedDt = DateTime.Now;
                                setList.Remove(current);
                            }
                            else
                            {
                                addNew = true;
                            }
                        }
                        if (addNew)
                        {
                            CategoryAttributeSetMap map = new CategoryAttributeSetMap();
                            map.AttributeSetId = mapRq.AttributeSetId.Value;
                            map.CategoryId = category.CategoryId;
                            map.CreatedBy = this.User.UserRequest().Email;
                            map.CreatedDt = DateTime.Now;
                            map.UpdatedBy = this.User.UserRequest().Email;
                            map.UpdatedDt = DateTime.Now;
                            db.CategoryAttributeSetMaps.Add(map);
                        }
                    }
                }
                if (setList != null && setList.Count > 0)
                {
                    db.CategoryAttributeSetMaps.RemoveRange(setList);
                }
                #endregion

                db.SaveChanges();
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

        [Route("api/GlobalCategories/Shift")]
        [HttpPut]
        public HttpResponseMessage ShiftChange(CategoryShiftRequest request)
        {
            try
            {
                var querList = new List<int?> { request.Child, request.Parent, request.Sibling };
                var catList = db.GlobalCategories.Where(w => querList.Contains(w.CategoryId)).ToList();
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

                db.SaveChanges();
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
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }

        [Route("api/GlobalCategories/Visibility")]
        [HttpPut]
        public HttpResponseMessage VisibilityCategory(List<CategoryRequest> request)
        {
            try
            {
                if (request == null || request.Count == 0)
                {
                    throw new Exception("Invalid request");
                }
                var catList = db.GlobalCategories.ToList();
                if (catList == null || catList.Count == 0)
                {
                    throw new Exception("No category found in this shop");
                }
                foreach (CategoryRequest catRq in request)
                {

                    var current = catList.Where(w => w.CategoryId==catRq.CategoryId).SingleOrDefault();
                    if (current == null)
                    {
                        throw new Exception("Cannot find category " + catRq.CategoryId);
                    }
                    var child = catList.Where(w => w.Lft >= current.Lft && w.Rgt <= current.Rgt);
                    child.ToList().ForEach(f => { f.Visibility = catRq.Visibility; f.UpdatedBy = this.User.UserRequest().Email; f.UpdatedDt = DateTime.Now; });
                }
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }

        [Route("api/GlobalCategories")]
        [HttpDelete]
        public HttpResponseMessage DeteleCategory(List<CategoryRequest> request)
        {
            try
            {
                foreach(CategoryRequest rq in request)
                {
                    var cat = db.GlobalCategories.Find(rq.CategoryId);
                    if (cat == null)
                    {
                        throw new Exception("Cannot find category");
                    }
                    int childSize = cat.Rgt - cat.Lft + 1;
                    //delete
                    db.GlobalCategories.Where(w => w.Rgt > cat.Rgt).ToList()
                        .ForEach(e => { e.Lft = e.Lft > cat.Rgt ? e.Lft - childSize : e.Lft; e.Rgt = e.Rgt - childSize; });
                    db.GlobalCategories.RemoveRange(db.GlobalCategories.Where(w => w.Lft >= cat.Lft && w.Rgt <= cat.Rgt));
                    break;
                }
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }

        [Route("api/GlobalCategories/{catId}/Attributes")]
        [HttpGet]
        public HttpResponseMessage GetVarientAttribute(int catId)
        {
            try
            {

                var attribute = (from cat in db.GlobalCategories
                                 join catMap in db.CategoryAttributeSetMaps on cat.CategoryId equals catMap.CategoryId
                                 join attrSet in db.AttributeSets on catMap.AttributeSetId equals attrSet.AttributeSetId
                                 where cat.CategoryId.Equals(catId)
                                         && cat.Status.Equals(Constant.STATUS_ACTIVE)
                                         && attrSet.Status.Equals(Constant.STATUS_ACTIVE)
                                 select new
                                 {
                                     attrSet,
                                     attrSetMap = from a in db.AttributeSetMaps
                                                  where a.Status.Equals(Constant.STATUS_ACTIVE)
                                                  select a,
                                     attr = from a in db.Attributes
                                            where a.Status.Equals(Constant.STATUS_ACTIVE) && a.VariantStatus.Equals(true)
                                            select a
                                 }).AsEnumerable().Select(t => t.attr).ToList();
                if (attribute != null && attribute.Count > 0)
                {
                    return Request.CreateResponse(attribute);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, HttpErrorMessage.NotFound);
                }
            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, HttpErrorMessage.InternalServerError);
            }
        }

        [Route("api/GlobalCategories/{catId}/AttributeSets")]
        [HttpGet]
        public HttpResponseMessage GetAttributeSetFromCat(int catId)
        {
            try
            {

                var attributeSet = db.AttributeSets
                    .Include(i => i.CategoryAttributeSetMaps.Select(s=>s.GlobalCategory))
                    .Include(i=>i.AttributeSetMaps.Select(s=>s.Attribute.AttributeValueMaps.Select(s1=>s1.AttributeValue)))
                    .Include(i=>i.AttributeSetTagMaps.Select(s=>s.Tag))
                    .Where(w=>w.CategoryAttributeSetMaps.Select(s=>s.CategoryId).Contains(catId)).ToList();
                if (attributeSet != null && attributeSet.Count > 0)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, attributeSet);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new List<AttributeSet>(0));
                }

            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, HttpErrorMessage.InternalServerError);
            }
        }

        [Route("api/GlobalCategories")]
        [HttpPut]
        public HttpResponseMessage SaveChangeGlobalCategory(List<CategoryRequest> request)
        {
            try
            {
                var catEnList = db.GlobalCategories.Include(i=>i.ProductStages).ToList();
                foreach (CategoryRequest catRq in request)
                {
                    if (catRq.Lft == null || catRq.Rgt == null || catRq.Lft >= catRq.Rgt)
                    {
                        throw new Exception("Category " + catRq.NameEn + " is invalid. Node is not properly formated");
                    }
                    var validate = request.Where(w => w.Lft == catRq.Lft || w.Rgt == catRq.Rgt).ToList();
                    if (validate != null && validate.Count > 1)
                    {
                        throw new Exception("Category " + catRq.NameEn + " is invalid. Node child has duplicated left or right key");
                    }
                    if(catRq.CategoryId == 0)
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
                    if (cat.ProductStages.Count != 0)
                    {
                        db.Dispose();
                        throw new Exception("Cannot delete category <strong>" + cat.NameEn + "</strong> with product associated");
                    }
                    db.GlobalCategories.Remove(cat);
                }
                db.SaveChanges();
                
                return Request.CreateResponse(HttpStatusCode.OK);
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
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }

        [Route("api/GlobalCategoryImages")]
        [HttpPost]
        public async Task<HttpResponseMessage> UploadFile()
        {
            try
            {
                if (!Request.Content.IsMimeMultipartContent())
                {
                    return Request.CreateResponse(HttpStatusCode.NotAcceptable, "Content Multimedia");
                }
                string tmpFolder = Path.Combine(AppSettingKey.IMAGE_ROOT_PATH, AppSettingKey.GLOBAL_CAT_FOLDER);
                var streamProvider = new MultipartFormDataStreamProvider(tmpFolder);
                await Request.Content.ReadAsMultipartAsync(streamProvider);

                FileUploadRespond fileUpload = new FileUploadRespond();
                string fileName = string.Empty;
                string ext = string.Empty;
                foreach (MultipartFileData fileData in streamProvider.FileData)
                {
                    fileName = fileData.LocalFileName;
                    string tmp = fileData.Headers.ContentDisposition.FileName;
                    if (tmp.StartsWith("\"") && tmp.EndsWith("\""))
                    {
                        tmp = tmp.Trim('"');
                    }
                    ext = Path.GetExtension(tmp);
                    break;
                }

                string newName = string.Concat(fileName, ext);
                File.Move(fileName, newName);
                fileUpload.tmpPath = newName;

                var name = Path.GetFileName(newName);
                var schema = Request.GetRequestContext().Url.Request.RequestUri.Scheme;
                var imageUrl = Request.GetRequestContext().Url.Request.RequestUri.Authority;
                fileUpload.url = string.Concat(schema, "://", imageUrl, "/", AppSettingKey.IMAGE_ROOT_FOLDER, "/", AppSettingKey.GLOBAL_CAT_FOLDER, "/", name);

                return Request.CreateResponse(HttpStatusCode.OK, fileUpload);
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, e);
            }
        }

        private void SetupCategory(GlobalCategory category, CategoryRequest request)
        {
            category.NameEn = Validation.ValidateString(request.NameEn, "Category Name (English)", false, 200, true, string.Empty);
            category.NameTh = Validation.ValidateString(request.NameTh, "Category Name (Thai)", false, 200, true, string.Empty);
            category.Commission = Validation.ValidateDecimal(request.Commission, "Commission (%)", true,20,2,true);
            category.DescriptionFullEn = Validation.ValidateString(request.DescriptionFullEn, "Category Description (English)", false, Int32.MaxValue, false, string.Empty);
            category.DescriptionFullTh = Validation.ValidateString(request.DescriptionFullTh, "Category Description (Thai)", false, Int32.MaxValue, false, string.Empty);
            category.DescriptionShortEn = Validation.ValidateString(request.DescriptionFullEn, "Category Description (English)", false, 500, false, string.Empty);
            category.DescriptionShortTh = Validation.ValidateString(request.DescriptionFullTh, "Category Description (Thai)", false, 500, false, string.Empty);
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

        private bool GlobalCategoryExists(int id)
        {
            return db.GlobalCategories.Count(e => e.CategoryId == id) > 0;
        }
    }
}