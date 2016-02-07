using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Colsp.Entity.Models;
using Colsp.Api.Constants;
using Colsp.Model.Requests;
using Colsp.Api.Extensions;
using Colsp.Model.Responses;
using System;
using System.Data.Entity;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data.Entity.Infrastructure;
using Colsp.Api.Helpers;

namespace Colsp.Api.Controllers
{
    public class AttributeSetsController : ApiController
    {
        private ColspEntities db = new ColspEntities();

        [Route("api/AttributeSets/{attributSetId}/Tags")]
        [HttpGet]
        public HttpResponseMessage GetTag([FromUri]int attributSetId)
        {
            try
            {
                var tag = (from attrSetTagMap in db.AttributeSetTagMaps
                          join tags in db.Tags on attrSetTagMap.TagId equals tags.TagId
                          where attrSetTagMap.AttributeSetId.Equals(attributSetId) && tags.Status.Equals(Constant.STATUS_ACTIVE) && attrSetTagMap.Status.Equals(Constant.STATUS_ACTIVE)
                          select tags).ToList();
                if(tag != null && tag.Count > 0)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, tag);
                }
                else
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, HttpErrorMessage.NotFound);
                }
                
            }
            catch
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, HttpErrorMessage.InternalServerError);
            }
        }

        [Route("api/AttributeSets")]
        [HttpGet]
        public HttpResponseMessage GetAttributeSets([FromUri] AttributeSetRequest request)
        {
            try
            {
                var attrSet = from atrS in db.AttributeSets
                              select new
                              {
                                  atrS.AttributeSetId,
                                  atrS.AttributeSetNameEn,
                                  atrS.AttributeSetNameTh,
                                  atrS.Visibility,
                                  atrS.Status,
                                  atrS.UpdatedDt,
                                  atrS.CreatedDt,
                                  AttributeCount = atrS.AttributeSetMaps.AsEnumerable().Count()
                              };
                if (request == null)
                {
                    attrSet = attrSet.Where(w => w.Visibility == true);
                    return Request.CreateResponse(HttpStatusCode.OK, attrSet);
                }
                request.DefaultOnNull();        
                if (!string.IsNullOrEmpty(request.SearchText))
                {
                    attrSet = attrSet.Where(a => a.AttributeSetNameEn.Contains(request.SearchText)
                    || a.AttributeSetNameTh.Contains(request.SearchText));
                }
                if (!string.IsNullOrEmpty(request._filter))
                {
                    //All VisibleNot Visible
                    if (string.Equals("Visible", request._filter, StringComparison.OrdinalIgnoreCase))
                    {
                        attrSet = attrSet.Where(a => a.Status.Equals(Constant.STATUS_VISIBLE));
                    }
                    else if(string.Equals("NotVisible", request._filter, StringComparison.OrdinalIgnoreCase))
                    {
                        attrSet = attrSet.Where(a => a.Status.Equals(Constant.STATUS_NOT_VISIBLE));
                    }
                }
                var total = attrSet.Count();
                var response = PaginatedResponse.CreateResponse(attrSet.Paginate(request), request, total);
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, HttpErrorMessage.InternalServerError);
            }
        }

        [Route("api/AttributeSets/{attributeSetId}")]
        [HttpGet]
        public HttpResponseMessage GetAttributeSets([FromUri]int attributeSetId)
        {
            try
            {
                AttributeSetRequest response = GetAttributeSetResponse(db, attributeSetId);
                if(response == null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, HttpErrorMessage.NotFound);
                }
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, HttpErrorMessage.InternalServerError);
            }
        }

        [Route("api/AttributeSets")]
        [HttpPost]
        public HttpResponseMessage AddAttributeSet(AttributeSetRequest request)
        {
            AttributeSet set = null;
            List<Tag> tagList = null;
            try
            {
                set = new AttributeSet();
                SetupAttributeSet(set, request);
                set.Status = Constant.STATUS_ACTIVE;
                set.CreatedBy = this.User.UserRequest().Email;
                set.CreatedDt = DateTime.Now;
                set.UpdatedBy = this.User.UserRequest().Email;
                set.UpdatedDt = DateTime.Now;
               
                set = db.AttributeSets.Add(set);
                db.SaveChanges();
                if(request.Attributes != null && request.Attributes.Count > 0)
                {
                    foreach (AttributeRequest attr in request.Attributes)
                    {
                        AttributeSetMap map = new AttributeSetMap();
                        map.AttributeId = attr.AttributeId.Value;
                        map.AttributeSetId = set.AttributeSetId;
                        map.Required = attr.Required;
                        map.Filterable = attr.Filterable;
                        map.Status = Constant.STATUS_ACTIVE;
                        map.CreatedBy = this.User.UserRequest().Email;
                        map.CreatedDt = DateTime.Now;
                        map.UpdatedBy = this.User.UserRequest().Email;
                        map.UpdatedDt = DateTime.Now;
                        db.AttributeSetMaps.Add(map);
                    }
                }
                if(request.Tags != null && request.Tags.Count > 0)
                {
                    tagList = new List<Tag>();
                    foreach (TagRequest tagRq in request.Tags)
                    {
                        Tag tag = new Tag();
                        tag.TagName = tagRq.TagName;
                        tag.CreatedBy = this.User.UserRequest().Email;
                        tag.CreatedDt = DateTime.Now;
                        tag.UpdatedBy = this.User.UserRequest().Email;
                        tag.UpdatedDt = DateTime.Now;
                        db.Tags.Add(tag);
                        tagList.Add(tag);
                    }
                }
                db.SaveChanges();
                if(tagList != null && tagList.Count > 0){
                    foreach (Tag t in tagList)
                    {
                        AttributeSetTagMap map = new AttributeSetTagMap();
                        map.TagId = t.TagId;
                        map.AttributeSetId = set.AttributeSetId;
                        map.CreatedBy = this.User.UserRequest().Email;
                        map.CreatedDt = DateTime.Now;
                        map.UpdatedBy = this.User.UserRequest().Email;
                        map.UpdatedDt = DateTime.Now;
                        db.AttributeSetTagMaps.Add(map);
                    }
                }
                db.SaveChanges();
                return GetAttributeSets(set.AttributeSetId);
            }
            catch(DbUpdateException e)
            {
                if(e != null && e.InnerException != null && e.InnerException.InnerException != null)
                {
                    int sqlError = ((SqlException)e.InnerException.InnerException).Number;
                    if(sqlError == 2627)
                    {

                        return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable
                           , "This attribute set name has already been used");
                    }
                }
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, HttpErrorMessage.InternalServerError);
            }
            catch (Exception e)
            {
                #region Rollback
                if (set != null && set.AttributeSetId != 0)
                {
                    if (tagList != null && tagList.Count > 0)
                    {
                        db.Tags.RemoveRange(tagList);
                    }
                    db.AttributeSets.Remove(set);
                    db.SaveChanges();
                }
                #endregion
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }

        [Route("api/AttributeSets/{attributeSetId}")]
        [HttpPut]
        public HttpResponseMessage SaveAttributeSet([FromUri]int attributeSetId,AttributeSetRequest request)
        {
            List<Tag> tagList = null;
            try
            {
                if(request == null || attributeSetId == 0)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, "Invalid request");
                }
                var attrSet = db.AttributeSets.Where(w => w.AttributeSetId.Equals(attributeSetId))
                    .Include(i => i.AttributeSetMaps.Select(s => s.Attribute.ProductStageAttributes))
                    .Include(i => i.AttributeSetMaps.Select(s => s.Attribute.ProductStageVariants ))
                    .Include(i => i.AttributeSetMaps.Select(s => s.Attribute.ProductStageVariants1 ))
                    .Include(i => i.AttributeSetTagMaps.Select(s => s.Tag))
                    .SingleOrDefault();
                if(attrSet != null)
                {
                    SetupAttributeSet(attrSet, request);
                    attrSet.Status = Constant.STATUS_ACTIVE;
                    attrSet.UpdatedBy = this.User.UserRequest().Email;
                    attrSet.UpdatedDt = DateTime.Now;
                    List<AttributeSetMap> mapList =  attrSet.AttributeSetMaps.ToList();
                    if(request.Attributes != null && request.Attributes.Count > 0)
                    {
                        foreach (AttributeRequest attrRq in request.Attributes)
                        {

                            bool addNew = false;
                            if (mapList == null || mapList.Count == 0)
                            {
                                addNew = true;
                            }
                            if (!addNew)
                            {
                                AttributeSetMap current = mapList.Where(w => w.AttributeId == attrRq.AttributeId).SingleOrDefault();
                                if (current != null)
                                {
                                    current.Required = attrRq.Required;
                                    current.Filterable = attrRq.Filterable;
                                    current.UpdatedBy = this.User.UserRequest().Email;
                                    current.UpdatedDt = DateTime.Now;
                                    mapList.Remove(current);
                                }
                                else
                                {
                                    addNew = true;
                                }
                            }
                            if (addNew)
                            {
                                AttributeSetMap map = new AttributeSetMap();
                                map.AttributeId = attrRq.AttributeId.Value;
                                map.Required = attrRq.Required;
                                map.Filterable = attrRq.Filterable;
                                map.AttributeSetId = attrSet.AttributeSetId;
                                map.CreatedBy = this.User.UserRequest().Email;
                                map.CreatedDt = DateTime.Now;
                                map.UpdatedBy = this.User.UserRequest().Email;
                                map.UpdatedDt = DateTime.Now;
                                db.AttributeSetMaps.Add(map);
                            }
                        }
                        
                    }
                    if(mapList != null && mapList.Count > 0)
                    {
                        foreach(AttributeSetMap map in mapList)
                        {
                            if((map.Attribute.ProductStageAttributes != null && map.Attribute.ProductStageAttributes.Count > 0)
                                || (map.Attribute.ProductStageVariants != null && map.Attribute.ProductStageVariants.Count > 0)
                                || (map.Attribute.ProductStageVariants1 != null && map.Attribute.ProductStageVariants1.Count > 0))
                            {
                                throw new Exception("Cannot delete attribute maping " + map.Attribute.AttributeNameEn + " in attribute set " + attrSet.AttributeSetNameEn + " with product associated");
                            }
                        }
                        db.AttributeSetMaps.RemoveRange(mapList);
                    }
                    if(attrSet.AttributeSetTagMaps != null)
                    {
                        db.AttributeSetTagMaps.RemoveRange(attrSet.AttributeSetTagMaps);
                    }
                    if (request.Tags != null && request.Tags.Count > 0)
                    {
                        tagList = new List<Tag>();
                        foreach (TagRequest tagRq in request.Tags)
                        {
                            Tag tag = new Tag();
                            tag.TagName = tagRq.TagName;
                            tag.CreatedBy = this.User.UserRequest().Email;
                            tag.CreatedDt = DateTime.Now;
                            tag.UpdatedBy = this.User.UserRequest().Email;
                            tag.UpdatedDt = DateTime.Now;
                            db.Tags.Add(tag);
                            tagList.Add(tag);
                        }
                    }
                    db.SaveChanges();
                    if (tagList != null && tagList.Count > 0)
                    {
                        foreach (Tag t in tagList)
                        {
                            AttributeSetTagMap map = new AttributeSetTagMap();
                            map.TagId = t.TagId;
                            map.AttributeSetId = attrSet.AttributeSetId;
                            map.CreatedBy = this.User.UserRequest().Email;
                            map.CreatedDt = DateTime.Now;
                            map.UpdatedBy = this.User.UserRequest().Email;
                            map.UpdatedDt = DateTime.Now;
                            db.AttributeSetTagMaps.Add(map);
                        }
                    }
                    db.SaveChanges();
                    return GetAttributeSets(attrSet.AttributeSetId);
                }
                else
                {
                    throw new Exception("Cannot find attribute set " + attributeSetId);
                }
            }
            catch (DbUpdateException e)
            {
                if (e != null && e.InnerException != null && e.InnerException.InnerException != null)
                {
                    int sqlError = ((SqlException)e.InnerException.InnerException).Number;
                    if (sqlError == 2627)
                    {

                        return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable
                            , "This attribute set name has already been used");
                    }
                }
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, HttpErrorMessage.InternalServerError);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }

        //duplicate
        [Route("api/AttributeSets/{attributeSetId}")]
        [HttpPost]
        public HttpResponseMessage DuplicateAttributeSet(int attributeSetId)
        {
            try
            {
                AttributeSetRequest response = GetAttributeSetResponse(db, attributeSetId);
                if(response == null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, HttpErrorMessage.NotFound);
                }
                else
                {
                    response.AttributeSetId = null;
                    return AddAttributeSet(response);
                }
            }
            catch
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, HttpErrorMessage.InternalServerError);
            }
        }

        [Route("api/AttributeSets/Visibility")]
        [HttpPut]
        public HttpResponseMessage VisibilityAttributeSet(List<AttributeSetRequest> request)
        {
            try
            {
                if (request == null || request.Count == 0)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, "Invalid request");
                }
                var setList = db.AttributeSets.ToList();
                foreach (AttributeSetRequest setRq in request)
                {
                    if(setRq.Visibility == null)
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, "Invalid visibility");
                    }
                    var current = setList.Where(w => w.AttributeSetId.Equals(setRq.AttributeSetId)).SingleOrDefault();
                    if (current == null)
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.NotFound, HttpErrorMessage.NotFound);
                    }
                    current.Visibility = setRq.Visibility;
                }
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, HttpErrorMessage.InternalServerError);
            }
        }

        [Route("api/AttributeSets")]
        [HttpDelete]
        public HttpResponseMessage DeleteAttributeSet(List<AttributeSetRequest> request)
        {
            try
            {
                if (request == null || request.Count == 0)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, "Invalid request");
                }
                var setList = db.AttributeSets
                    .Include(i=>i.ProductStages)
                    .Include(i=>i.Products)
                    .Include(i=>i.ProductHistories)
                    .Include(i=>i.AttributeSetMaps)
                    .Include(i=>i.AttributeSetTagMaps)
                    .Include(i=>i.CategoryAttributeSetMaps)
                    .ToList();
                foreach (AttributeSetRequest setRq in request)
                {
                    var current = setList.Where(w => w.AttributeSetId.Equals(setRq.AttributeSetId)).SingleOrDefault();
                    if (current == null)
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.NotFound,"Cannot find arrtibute set " + setRq.AttributeSetNameEn);
                    }
                    if((current.ProductHistories != null && current.ProductHistories.Count > 0 )
                        || (current.ProductStages != null && current.ProductStages.Count > 0)
                        || (current.Products != null && current.Products.Count > 0))
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, "Cannot delete arrtibute set " + setRq.AttributeSetNameEn + "  with product associated");
                    }
                    if(current.AttributeSetMaps != null && current.AttributeSetMaps.Count > 0)
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, "Cannot delete arrtibute set " + setRq.AttributeSetNameEn + "  with attribute associated");
                    }
                    if(current.AttributeSetTagMaps != null && current.AttributeSetTagMaps.Count > 0)
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, "Cannot delete arrtibute set " + setRq.AttributeSetNameEn + "  with tag associated");
                    }
                    if(current.CategoryAttributeSetMaps !=  null && current.CategoryAttributeSetMaps.Count > 0)
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, "Cannot delete arrtibute set " + setRq.AttributeSetNameEn + "  with global category associated");
                    }
                    db.AttributeSets.Remove(current);
                }
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, HttpErrorMessage.InternalServerError);
            }
        }

        private void SetupAttributeSet(AttributeSet set, AttributeSetRequest request)
        {
            set.AttributeSetNameEn = Validation.ValidateString(request.AttributeSetNameEn, "Attribute Set Name (English)", true, 100, true);
            set.AttributeSetNameTh = Validation.ValidateString(request.AttributeSetNameTh, "Attribute Set Name (Thai)", false, 100, true);
            set.AttributeSetDescriptionEn = request.AttributeSetDescriptionEn;
            set.AttributeSetDescriptionTh = request.AttributeSetDescriptionTh;
            set.Visibility = request.Visibility;
        }

        private AttributeSetRequest GetAttributeSetResponse(ColspEntities db, int attributeSetId)
        {
            var attrSet = db.AttributeSets.Where(w => w.AttributeSetId.Equals(attributeSetId))
                     .Include(i => i.AttributeSetMaps.Select(s => s.Attribute.ProductStageAttributes))
                     .Include(i => i.AttributeSetMaps.Select(s => s.Attribute.ProductStageVariants))
                     .Include(i => i.AttributeSetMaps.Select(s => s.Attribute.ProductStageVariants1))
                     .Include(i => i.AttributeSetTagMaps.Select(s => s.Tag))
                     .SingleOrDefault();
            if (attrSet != null)
            {
                AttributeSetRequest response = new AttributeSetRequest();
                response.AttributeSetId = attrSet.AttributeSetId;
                response.AttributeSetNameEn = attrSet.AttributeSetNameEn;
                response.AttributeSetNameTh = attrSet.AttributeSetNameTh;
                response.AttributeSetDescriptionEn = attrSet.AttributeSetDescriptionEn;
                response.AttributeSetDescriptionTh = attrSet.AttributeSetDescriptionTh;
                response.Visibility = attrSet.Visibility;
                response.Status = attrSet.Status;
                if (attrSet.AttributeSetMaps != null && attrSet.AttributeSetMaps.Count > 0)
                {
                    response.Attributes = new List<AttributeRequest>();
                    foreach (AttributeSetMap map in attrSet.AttributeSetMaps)
                    {
                        AttributeRequest attr = new AttributeRequest();
                        attr.AttributeId = map.AttributeId;
                        attr.AttributeNameEn = map.Attribute.AttributeNameEn;
                        attr.AttributeNameTh = map.Attribute.AttributeNameTh;
                        attr.AttributeUnitEn = map.Attribute.AttributeUnitEn;
                        attr.AttributeUnitTh = map.Attribute.AttributeUnitTh;
                        attr.DataType = map.Attribute.DataType;
                        attr.DataValidation = map.Attribute.DataValidation;
                        attr.DefaultValue = map.Attribute.DefaultValue;
                        attr.DisplayNameEn = map.Attribute.DisplayNameEn;
                        attr.DisplayNameTh = map.Attribute.DisplayNameTh;
                        attr.ShowAdminFlag = map.Attribute.ShowAdminFlag;
                        attr.ShowGlobalFilterFlag = map.Attribute.ShowGlobalFilterFlag;
                        attr.ShowGlobalSearchFlag = map.Attribute.ShowGlobalSearchFlag;
                        attr.ShowLocalFilterFlag = map.Attribute.ShowLocalFilterFlag;
                        attr.ShowLocalSearchFlag = map.Attribute.ShowLocalSearchFlag;
                        attr.VariantDataType = map.Attribute.VariantDataType;
                        attr.VariantStatus = map.Attribute.VariantStatus;
                        attr.AllowHtmlFlag = map.Attribute.AllowHtmlFlag;
                        attr.Required = map.Required;
                        attr.Filterable = map.Filterable;
                        attr.ProductCount = (map.Attribute.ProductStageAttributes != null ? map.Attribute.ProductStageAttributes.Count : 0)
                                            + (map.Attribute.ProductStageVariants != null ? map.Attribute.ProductStageVariants.Count : 0)
                                            + (map.Attribute.ProductStageVariants1 != null ? map.Attribute.ProductStageVariants1.Count : 0);
                        attr.Status = map.Attribute.Status;
                        response.Attributes.Add(attr);
                    }
                }
                if (attrSet.AttributeSetTagMaps != null && attrSet.AttributeSetTagMaps.Count > 0)
                {
                    response.Tags = new List<TagRequest>();
                    foreach (AttributeSetTagMap map in attrSet.AttributeSetTagMaps)
                    {
                        TagRequest tag = new TagRequest();
                        tag.TagId = map.TagId;
                        tag.TagName = map.Tag.TagName;
                        response.Tags.Add(tag);
                    }
                }
                return response;
            }
            else
            {
                return null;
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

        private bool AttributeSetExists(int id)
        {
            return db.AttributeSets.Count(e => e.AttributeSetId == id) > 0;
        }
    }
}