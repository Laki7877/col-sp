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
                if (User.ShopRequest() != null )
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
                                     CategoryBannerEn = cat.GlobalCatImages.Where(w=>Constant.LANG_EN.Equals(w.EnTh)).OrderBy(o=>o.Position).Select(si=>new {
                                         url = si.ImageUrl,
                                         position = si.Position
                                     }),
                                     CategoryBannerTh = cat.GlobalCatImages.Where(w => Constant.LANG_TH.Equals(w.EnTh)).OrderBy(o => o.Position).Select(si => new {
                                         url = si.ImageUrl,
                                         position = si.Position
                                     }),
                                     FeatureProducts = cat.GlobalCatFeatureProducts
                                        .Select(s=> new
                                        {
                                            s.ProductId,
                                            s.ProductStageGroup.ProductStages.Where(w=>w.IsVariant==false).FirstOrDefault().Pid,
                                            s.ProductStageGroup.ProductStages.Where(w => w.IsVariant == false).FirstOrDefault().ProductNameEn
                                        }),
                                     cat.DescriptionFullEn,
                                     cat.DescriptionFullTh,
                                     cat.DescriptionShortEn,
                                     cat.DescriptionShortTh,
                                     cat.FeatureTitle,
                                     cat.TitleShowcase,
                                     //cat.CategoryAbbreviation,
                                     cat.Lft,
                                     cat.Rgt,
                                     cat.UrlKey,
                                     cat.Visibility,
                                     cat.Status,
                                     UpdatedDt = cat.UpdateOn,
                                     CreatedDt = cat.CreateOn,
                                     cat.Commission,

                                     //ProductCount = cat.ProductStages.Count(c => !c.Status.Equals(Constant.STATUS_REMOVE))
                                     //               + cat.Products.Count(c => !c.Status.Equals(Constant.STATUS_REMOVE))
                                     //               + cat.ProductHistories.Count(c => !c.Status.Equals(Constant.STATUS_REMOVE)),
                                     AttributeSets = cat.GlobalCatAttributeSetMaps.Select(s => new { s.AttributeSetId, s.AttributeSet.AttributeSetNameEn, ProductCount = s.AttributeSet.ProductStageGroups.Count })
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
                var email = User.UserRequest().Email;
                var currentDt = DateTime.Now;
                category = new GlobalCategory();
                SetupCategory(category, request);
                #region Create-Update
                category.CreateBy = email;
                category.CreateOn = currentDt;
                category.UpdateBy = email;
                category.UpdateOn = currentDt;
                #endregion
                #region lft-rgt
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
                #endregion
                #region Banner Image En
                if (request.CategoryBannerEn != null && request.CategoryBannerEn.Count > 0)
                {
                    int position = 0;
                    foreach (ImageRequest img in request.CategoryBannerEn)
                    {
                        category.GlobalCatImages.Add(new GlobalCatImage()
                        {
                            ImageUrl = img.Url,
                            Position = position++,
                            EnTh = Constant.LANG_EN,
                            CreateBy = email,
                            CreateOn = currentDt,
                            UpdateBy = email,
                            UpdateOn = currentDt
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
                            ImageUrl = img.Url,
                            Position = position++,
                            EnTh = Constant.LANG_TH,
                            CreateBy = email,
                            CreateOn = currentDt,
                            UpdateBy = email,
                            UpdateOn = currentDt
                        });
                    }
                }
                #endregion
                #region Attribute Set
                if (request.AttributeSets != null && request.AttributeSets.Count > 0)
                {
                   
                    foreach (AttributeSetRequest mapRq in request.AttributeSets)
                    {
                        category.GlobalCatAttributeSetMaps.Add(new GlobalCatAttributeSetMap()
                        {
                            AttributeSetId = mapRq.AttributeSetId,
                            CreateBy = email,
                            CreateOn = currentDt,
                            UpdateBy = email,
                            UpdateOn = currentDt
                        });
                    }
                }
                #endregion
                #region url
                category.CategoryId = db.GetNextGlobalCategoryId().SingleOrDefault().Value;
                if (string.IsNullOrWhiteSpace(request.UrlKey))
                {
                    category.UrlKey = string.Concat(category.NameEn.Replace(" ", "-"), "-", category.CategoryId);
                }
                else
                {
                    category.UrlKey = request.UrlKey.Trim().Replace(" ", "-");
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
        public HttpResponseMessage SaveChange(int categoryId, CategoryRequest request)
        {
            try
            {
                var category = db.GlobalCategories
                        .Include(i => i.GlobalCatAttributeSetMaps
                        .Select(s => s.AttributeSet))
                        .Include(i=>i.GlobalCatImages)
                        .Include(i=>i.GlobalCatFeatureProducts)
                    .Where(w => w.CategoryId == categoryId)
                    .SingleOrDefault();
                if (category == null)
                {
                    throw new Exception("Cannot find selected category");
                }
                var email = User.UserRequest().Email;
                var currentDt = DateTime.Now;
                SetupCategory(category, request);
                category.UpdateBy = email;
                category.UpdateOn = currentDt;
                category.Status = request.Status;
                #region url
                if (string.IsNullOrWhiteSpace(request.UrlKey))
                {
                    category.UrlKey = string.Concat(category.NameEn.Replace(" ", "-"),"-", category.CategoryId);
                }
                else
                {
                    category.UrlKey = request.UrlKey.Replace(" ", "-");
                }
                #endregion
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
                                current.ImageUrl = img.Url;
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
                            category.GlobalCatImages.Add(new GlobalCatImage()
                            {
                                ImageUrl = img.Url,
                                Position = position++,
                                EnTh = Constant.LANG_EN,
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
                                current.ImageUrl = img.Url;
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
                            category.GlobalCatImages.Add(new GlobalCatImage()
                            {
                                ImageUrl = img.Url,
                                Position = position++,
                                EnTh = Constant.LANG_TH,
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
                #region Global Category Feature Product
                var pidList = category.GlobalCatFeatureProducts.ToList();
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
                #region Attribute Set
                var setList = db.GlobalCatAttributeSetMaps.Where(w => w.CategoryId == categoryId).ToList();
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
        public HttpResponseMessage VisibilityCategory(List<CategoryRequest> request)
        {
            try
            {
                if (request == null || request.Count == 0)
                {
                    throw new Exception("Invalid request");
                }
                var ids = request.Select(s => s.CategoryId);
                var catList = db.GlobalCategories.Where(w=> ids.Contains(w.CategoryId));
                var email = User.UserRequest().Email;
                var currentDt = DateTime.Now;
                foreach (CategoryRequest catRq in request)
                {
                    var current = catList.Where(w => w.CategoryId==catRq.CategoryId).SingleOrDefault();
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

        [Route("api/GlobalCategories")]
        [HttpDelete]
        public HttpResponseMessage DeteleCategory(List<CategoryRequest> request)
        {
            try
            {
                var ids = request.Select(s => s.CategoryId);

                var productMap = db.ProductStageGroups.Where(w => ids.Contains(w.GlobalCatId)).Select(s => s.GlobalCategory.NameEn);
                if (productMap != null && productMap.Count() > 0)
                {
                    throw new Exception(string.Concat("Cannot delete global category ", string.Join(",", productMap), " with product associated"));
                }
                var attributesetMap = db.GlobalCatAttributeSetMaps.Where(w => ids.Contains(w.CategoryId)).Select(s => s.GlobalCategory.NameEn);
                if (attributesetMap != null && attributesetMap.Count() > 0)
                {
                    throw new Exception(string.Concat("Cannot delete global category ", string.Join(",", attributesetMap), " with attribute set associated"));
                }

                var catList = db.GlobalCategories.Where(w => ids.Contains(w.CategoryId));

                foreach (CategoryRequest catRq in request)
                {
                    var current = catList.Where(w => w.CategoryId == catRq.CategoryId).SingleOrDefault();
                    if (current == null)
                    {
                        throw new Exception("Cannot find category " + catRq.CategoryId);
                    }

                    int childSize = current.Rgt - current.Lft + 1;
                    //delete
                    db.GlobalCategories.Where(w => w.Rgt > current.Rgt).ToList()
                        .ForEach(e => { e.Lft = e.Lft > current.Rgt ? e.Lft - childSize : e.Lft; e.Rgt = e.Rgt - childSize; });
                    db.GlobalCategories.RemoveRange(db.GlobalCategories.Where(w => w.Lft >= current.Lft && w.Rgt <= current.Rgt));
                    break;
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
                          where cat.CategoryId == catId && attr.VariantStatus==true
                    select attr);
                var response = new List<IQueryable<Entity.Models.Attribute>>() { attribute };


                if(response == null || response.Count() == 0)
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
                                          new {
                                              sv.AttributeId,
                                              sv.AttributeValueId,
                                              AttributeValue = new { sv.AttributeValue.AttributeValueId, sv.AttributeValue.AttributeValueEn, sv.AttributeValue.AttributeValueTh }
                                          })
                                      }
                                  }),
                                  AttributeSetTagMaps = atrS.AttributeSetTags.Select(s => new { s.AttributeSetId, Tag =  new { TagName = s.Tag } }),
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
        public HttpResponseMessage SaveChangeGlobalCategory(List<CategoryRequest> request)
        {
            try
            {
                var catEnList = db.GlobalCategories;
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
                    catEn.UpdateBy = User.UserRequest().Email;
                    catEn.UpdateOn = DateTime.Now;
                    catEnList.Remove(catEn);
                }
                var ids = catEnList.Select(s => s.CategoryId);
                var productMap = db.ProductStageGroups.Where(w=> ids.Contains(w.GlobalCatId)).Select(s=>s.GlobalCategory.NameEn);
                if (productMap != null && productMap.Count() > 0)
                {
                    throw new Exception(string.Concat("Cannot delete global category ", string.Join(",", productMap), " with product associated"));
                }
                var attributesetMap = db.GlobalCatAttributeSetMaps.Where(w => ids.Contains(w.CategoryId)).Select(s => s.GlobalCategory.NameEn);
                if (attributesetMap != null && attributesetMap.Count() > 0)
                {
                    throw new Exception(string.Concat("Cannot delete global category ", string.Join(",", attributesetMap), " with attribute set associated"));
                }
                if(catEnList != null && catEnList.Count() > 0)
                {
                    db.GlobalCategories.RemoveRange(catEnList);
                }
                
                Util.DeadlockRetry(db.SaveChanges, "GlobalCategory");

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
                var fileUpload = await Util.SetupImage(Request, AppSettingKey.IMAGE_ROOT_PATH, AppSettingKey.GLOBAL_CAT_FOLDER, 1500, 1500, 2000, 2000, 5, true);
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
            category.Commission = Validation.ValidateDecimal(request.Commission, "Commission (%)", true,20,2,true).Value;
            category.DescriptionFullEn = Validation.ValidateString(request.DescriptionFullEn, "Category Description (English)", false, int.MaxValue, false, string.Empty);
            category.DescriptionFullTh = Validation.ValidateString(request.DescriptionFullTh, "Category Description (Thai)", false, int.MaxValue, false, string.Empty);
            category.DescriptionShortEn = Validation.ValidateString(request.DescriptionShortEn, "Category Short Description (English)", false, 500, false, string.Empty);
            category.DescriptionShortTh = Validation.ValidateString(request.DescriptionShortTh, "Category Short Description (Thai)", false, 500, false, string.Empty);
            category.FeatureTitle = Validation.ValidateString(request.FeatureTitle, "Feature Products Title", false, 100, false, string.Empty);
            category.TitleShowcase = request.TitleShowcase;
            category.Visibility = request.Visibility;
            category.Status = Constant.STATUS_ACTIVE;
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