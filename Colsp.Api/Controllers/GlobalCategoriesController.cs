﻿using System;
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
                                    cat.NameTh,
                                    cat.CategoryAbbreviation,
                                    cat.Lft,
                                    cat.Rgt,
                                    cat.UrlKeyEn,
                                    cat.UrlKeyTh,
                                    cat.Visibility,
                                    cat.Status,
                                    cat.UpdatedDt,
                                    cat.CreatedDt,
                                    cat.Commission,
                                    ProductCount = cat.ProductStages.Count(c => !c.Status.Equals(Constant.STATUS_REMOVE)) 
                                                    + cat.Products.Count(c => !c.Status.Equals(Constant.STATUS_REMOVE))
                                                    + cat.ProductHistories.Count(c => !c.Status.Equals(Constant.STATUS_REMOVE)),
                                    AttributeSets = cat.CategoryAttributeSetMaps.Select(s=> new { s.AttributeSetId, s.AttributeSet.AttributeSetNameEn, ProductCount = s.AttributeSet.ProductStages.Count + s.AttributeSet.Products.Count + s.AttributeSet.ProductHistories.Count })
                                }).ToList();

                if (globalCat != null && globalCat.Count > 0)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, globalCat);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "Cannot find any global category");
                }
            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, HttpErrorMessage.InternalServerError);
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
                var catEnList = db.GlobalCategories
                    .Include(i => i.CategoryAttributeSetMaps.Select(s => s.AttributeSet.ProductStages))
                    .Include(i => i.ProductStages).ToList();

                List<GlobalCategory> newCat = new List<GlobalCategory>();
                List<Tuple<GlobalCategory, int>> insetMap = new List<Tuple<GlobalCategory, int>>();
                foreach (CategoryRequest catRq in request)
                {
                    if(string.IsNullOrEmpty(catRq.NameEn))
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, "Category require field are not met the requirement");
                    }
                    if (catRq.Lft == null || catRq.Rgt == null || catRq.Lft >= catRq.Rgt)
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, "Category " + catRq.NameEn + " is invalid. Node is not properly formated");
                    }
                    var validate = request.Where(w => w.Lft == catRq.Lft || w.Rgt == catRq.Rgt).ToList();
                    if (validate != null && validate.Count > 1)
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, "Category " + catRq.NameEn + " is invalid. Node child has duplicated left or right key");
                    }
                    if (catRq.CategoryId != 0)
                    {
                        var catEn = catEnList.Where(w => w.CategoryId == catRq.CategoryId).SingleOrDefault();
                        if (catEn == null)
                        {
                            return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, "Category " + catRq.NameEn + " is invalid. Cannot find Category key " + catRq.CategoryId + " in database");
                        }
                        else
                        {
                            catEn.Lft = catRq.Lft;
                            catEn.Rgt = catRq.Rgt;
                            catEn.NameEn = catRq.NameEn;
                            catEn.NameTh = catRq.NameTh;
                            catEn.UrlKeyEn = catRq.UrlKeyEn;
                            catEn.Commission = catRq.Commission;
                            catEn.Visibility = catRq.Visibility;
                            catEn.Status = catRq.Status;
                            catEn.UpdatedBy = this.User.UserRequest().Email;
                            catEn.UpdatedDt = DateTime.Now;
                            catEnList.Remove(catEn);

                            var tmpList = catEn.CategoryAttributeSetMaps.ToList();
                            if (catRq.AttributeSets != null && catRq.AttributeSets.Count > 0)
                            {
                                foreach (AttributeSetRequest attrMap in catRq.AttributeSets)
                                {
                                    bool addNew = false;
                                    if (tmpList == null || tmpList.Count == 0)
                                    {
                                        addNew = true;
                                    }
                                    if (!addNew)
                                    {
                                        CategoryAttributeSetMap current = tmpList.Where(w => w.AttributeSetId == attrMap.AttributeSetId).SingleOrDefault();
                                        if (current != null)
                                        {
                                            current.UpdatedBy = this.User.UserRequest().Email;
                                            current.UpdatedDt = DateTime.Now;
                                            tmpList.Remove(current);
                                        }
                                        else
                                        {
                                            addNew = true;
                                        }
                                    }
                                    if (addNew)
                                    {
                                        CategoryAttributeSetMap map = new CategoryAttributeSetMap();
                                        map.AttributeSetId = attrMap.AttributeSetId.Value;
                                        map.CategoryId = catEn.CategoryId;
                                        map.CreatedBy = this.User.UserRequest().Email;
                                        map.CreatedDt = DateTime.Now;
                                        map.UpdatedBy = this.User.UserRequest().Email;
                                        map.UpdatedDt = DateTime.Now;
                                        db.CategoryAttributeSetMaps.Add(map);
                                    }
                                }
                            }
                            if (tmpList != null && tmpList.Count > 0)
                            {

                                foreach (CategoryAttributeSetMap map in tmpList)
                                {
                                    if ((map.AttributeSet.ProductStages != null && map.AttributeSet.ProductStages.Count > 0)
                                        || (map.AttributeSet.Products != null && map.AttributeSet.Products.Count > 0)
                                        || (map.AttributeSet.ProductHistories != null && map.AttributeSet.ProductHistories.Count > 0))
                                    {
                                        db.Dispose();
                                        return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, "Cannot delete attribute set maping " + map.AttributeSet.AttributeSetNameEn + " in category " + catRq.NameEn + " with product associated");
                                    }
                                }
                                db.CategoryAttributeSetMaps.RemoveRange(tmpList);
                            }
                        }
                    }
                    else
                    {
                        GlobalCategory catEn = new GlobalCategory();
                        string abbr = AutoGenerate.NextCatAbbre(db);
                        catEn.CategoryAbbreviation = abbr;
                        catEn.Lft = catRq.Lft;
                        catEn.Rgt = catRq.Rgt;
                        catEn.NameEn = catRq.NameEn;
                        catEn.NameTh = catRq.NameTh;
                        catEn.Commission = catRq.Commission;
                        catEn.UrlKeyEn = catRq.UrlKeyEn;
                        catEn.Visibility = catRq.Visibility;
                        catEn.Status = catRq.Status;
                        catEn.CreatedBy = this.User.UserRequest().Email;
                        catEn.CreatedDt = DateTime.Now;
                        catEn.UpdatedBy = this.User.UserRequest().Email;
                        catEn.UpdatedDt = DateTime.Now;
                        if (catRq.AttributeSets != null && catRq.AttributeSets.Count > 0)
                        {
                            foreach (AttributeSetRequest mapRq in catRq.AttributeSets)
                            {
                                Tuple<GlobalCategory, int> insert = new Tuple<GlobalCategory, int>(catEn, mapRq.AttributeSetId.Value);
                                insetMap.Add(insert);
                            }
                        }
                        
                        db.GlobalCategories.Add(catEn);
                        newCat.Add(catEn);
                    }
                }
                foreach (var cat in catEnList)
                {
                    if (cat.ProductStages.Count != 0)
                    {
                        db.Dispose();
                        return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, "Cannot delete category " + cat.NameEn + " with product associated");
                    }
                    db.GlobalCategories.Remove(cat);
                    db.GlobalCategoryPIDs.Remove(db.GlobalCategoryPIDs.Find(cat.CategoryId));
                }
                db.SaveChanges();
                if(newCat != null)
                {
                    foreach (GlobalCategory cat in newCat)
                    {
                        GlobalCategoryPID catPid = new GlobalCategoryPID();
                        catPid.CategoryId = cat.CategoryId;
                        catPid.CategoryAbbreviation = cat.CategoryAbbreviation;
                        catPid.CurrentKey = "11111";
                        db.GlobalCategoryPIDs.Add(catPid);
                    }
                    foreach(Tuple<GlobalCategory, int> i in insetMap)
                    {
                        CategoryAttributeSetMap map = new CategoryAttributeSetMap();
                        map.AttributeSetId = i.Item2;
                        map.CategoryId = i.Item1.CategoryId;
                        map.CreatedBy = this.User.UserRequest().Email;
                        map.CreatedDt = DateTime.Now;
                        db.CategoryAttributeSetMaps.Add(map);
                    }
                    db.SaveChanges();
                }
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
                           , "Category field is already exits");
                    }
                }
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, HttpErrorMessage.InternalServerError);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
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

        private bool GlobalCategoryExists(int id)
        {
            return db.GlobalCategories.Count(e => e.CategoryId == id) > 0;
        }
    }
}