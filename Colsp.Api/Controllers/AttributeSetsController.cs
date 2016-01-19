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
                              where !atrS.Status.Equals(Constant.STATUS_REMOVE)
                              select new
                              {
                                  atrS.AttributeSetId,
                                  atrS.AttributeSetNameEn,
                                  atrS.AttributeSetNameTh,
                                  atrS.Status,
                                  atrS.UpdatedDt,
                                  atrS.CreatedDt,
                                  AttributeCount = atrS.AttributeSetMaps.AsEnumerable().Count()
                              };
                if (request == null)
                {
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
                set.AttributeSetNameEn = request.AttributeSetNameEn;
                set.AttributeSetNameTh = request.AttributeSetNameTh;
                set.AttributeSetDescriptionEn = request.AttributeSetDescriptionEn;
                set.AttributeSetDescriptionTh = request.AttributeSetDescriptionTh;
                #region Validation
                if (string.IsNullOrEmpty(set.AttributeSetNameEn))
                {
                    return Request.CreateResponse(HttpStatusCode.NotAcceptable, "AttributeSetNameEn is required");
                }
                #endregion
                set.Status = Constant.STATUS_VISIBLE;
                set.CreatedBy = this.User.Email();
                set.CreatedDt = DateTime.Now;
                set.UpdatedBy = this.User.Email();
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
                        map.CreatedBy = this.User.Email();
                        map.CreatedDt = DateTime.Now;
                        map.UpdatedBy = this.User.Email();
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
                        tag.CreatedBy = this.User.Email();
                        tag.CreatedDt = DateTime.Now;
                        tag.UpdatedBy = this.User.Email();
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
                        map.CreatedBy = this.User.Email();
                        map.CreatedDt = DateTime.Now;
                        map.UpdatedBy = this.User.Email();
                        map.UpdatedDt = DateTime.Now;
                        db.AttributeSetTagMaps.Add(map);
                    }
                }
                db.SaveChanges();
                return GetAttributeSets(set.AttributeSetId);
            }
            catch
            {
                db.Dispose();
                db = new ColspEntities();
                #region Rollback
                if (set != null)
                {
                    if (tagList != null && tagList.Count > 0)
                    {
                        db.Tags.RemoveRange(tagList);
                    }
                    db.AttributeSets.Remove(set);
                    db.SaveChanges();
                }
                #endregion
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, HttpErrorMessage.InternalServerError);
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
                    .Include(i => i.AttributeSetMaps.Select(s => s.Attribute))
                    .Include(i => i.AttributeSetTagMaps.Select(s => s.Tag))
                    .SingleOrDefault();
                if(attrSet != null)
                {
                    attrSet.AttributeSetNameEn = request.AttributeSetNameEn;
                    attrSet.AttributeSetNameTh = request.AttributeSetNameTh;
                    attrSet.AttributeSetDescriptionEn = request.AttributeSetDescriptionEn;
                    attrSet.AttributeSetDescriptionTh = request.AttributeSetDescriptionTh;
                    #region Validation
                    if (string.IsNullOrEmpty(attrSet.AttributeSetNameEn))
                    {
                        return Request.CreateResponse(HttpStatusCode.NotAcceptable, "AttributeSetNameEn is required");
                    }
                    #endregion
                    attrSet.Status = request.Status;
                    attrSet.UpdatedBy = this.User.Email();
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
                                    current.UpdatedBy = this.User.Email();
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
                                map.CreatedBy = this.User.Email();
                                map.CreatedDt = DateTime.Now;
                                map.UpdatedBy = this.User.Email();
                                map.UpdatedDt = DateTime.Now;
                                db.AttributeSetMaps.Add(map);
                            }
                        }
                        
                    }
                    if(mapList != null && mapList.Count > 0)
                    {
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
                            tag.CreatedBy = this.User.Email();
                            tag.CreatedDt = DateTime.Now;
                            tag.UpdatedBy = this.User.Email();
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
                            map.CreatedBy = this.User.Email();
                            map.CreatedDt = DateTime.Now;
                            map.UpdatedBy = this.User.Email();
                            map.UpdatedDt = DateTime.Now;
                            db.AttributeSetTagMaps.Add(map);
                        }
                    }
                    db.SaveChanges();
                    return GetAttributeSets(attrSet.AttributeSetId);
                }
                else
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, HttpErrorMessage.NotFound);
                }
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, HttpErrorMessage.InternalServerError);
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
                    if(!Constant.STATUS_VISIBLE.Equals(setRq.Status) 
                        && !Constant.STATUS_NOT_VISIBLE.Equals(setRq.Status))
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable,"Invalid status");
                    }
                    var current = setList.Where(w => w.AttributeSetId.Equals(setRq.AttributeSetId)).SingleOrDefault();
                    if (current == null)
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.NotFound, HttpErrorMessage.NotFound);
                    }
                    current.Status = setRq.Status;
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
                var setList = db.AttributeSets.ToList();
                foreach (AttributeSetRequest setRq in request)
                {
                    var current = setList.Where(w => w.AttributeSetId.Equals(setRq.AttributeSetId)).SingleOrDefault();
                    if (current == null)
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.NotFound, HttpErrorMessage.NotFound);
                    }
                    current.Status = Constant.STATUS_REMOVE;
                }
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, HttpErrorMessage.InternalServerError);
            }
        }

        private AttributeSetRequest GetAttributeSetResponse(ColspEntities db, int attributeSetId)
        {
            var attrSet = db.AttributeSets.Where(w => w.AttributeSetId.Equals(attributeSetId))
                     .Include(i => i.AttributeSetMaps.Select(s => s.Attribute))
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