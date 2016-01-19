using System;
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



namespace Colsp.Api.Controllers
{
    public class AttributesController : ApiController
    {
        private ColspEntities db = new ColspEntities();

        [Route("api/Attributes")]
        [HttpGet]
        public HttpResponseMessage GetAttributes([FromUri] AttributeRequest request)
        {
            try
            {

                var attrList = from attr in db.Attributes
                               where !attr.Status.Equals(Constant.STATUS_REMOVE)
                               select new
                               {
                                   attr.AttributeId,
                                   attr.AttributeNameEn,
                                   attr.AttributeNameTh,
                                   attr.DisplayNameEn,
                                   attr.DisplayNameTh,
                                   attr.VariantStatus,
                                   attr.DataType,
                                   attr.Status,
                                   attr.UpdatedDt,
                                   AttributeSetCount = attr.AttributeSetMaps.Count()
                               };

                if (request == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, attrList);
                }
                request.DefaultOnNull();
                
                if (!string.IsNullOrEmpty(request.SearchText))
                {
                    attrList = attrList.Where(a => a.AttributeNameEn.Contains(request.SearchText)
                    || a.AttributeNameTh.Contains(request.SearchText));
                }
                if (!string.IsNullOrEmpty(request._filter))
                {
                    if ("Dropdown".Equals(request._filter))
                    {
                        attrList = attrList.Where(a => a.DataType.Equals("LT"));
                    }
                    else if ("Text".Equals(request._filter))
                    {
                        attrList = attrList.Where(a => a.DataType.Equals("ST"));
                    }
                    else if ("Variation".Equals(request._filter))
                    {
                        attrList = attrList.Where(a => a.VariantStatus==true);
                    }
                    else if ("No Variation".Equals(request._filter))
                    {
                        attrList = attrList.Where(a => a.VariantStatus == false);
                    }
                }
                var total = attrList.Count();
                var pagedAttribute = attrList.Paginate(request);
                var response = PaginatedResponse.CreateResponse(pagedAttribute, request, total);
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, HttpErrorMessage.InternalServerError);
            }
        }

        [Route("api/Attributes/{attributeId}")]
        [HttpGet]
        public HttpResponseMessage GetAttribute(int attributeId)
        {
            try
            {
                AttributeRequest attribute = GetAttibuteResponse(db, attributeId);
                if(attribute == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, HttpStatusCode.NotFound);
                }
                return Request.CreateResponse(HttpStatusCode.OK, attribute);
            }
            catch(Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, e);
            }
        }

        [Route("api/Attributes")]
        [HttpPost]
        public HttpResponseMessage AddAttribute(AttributeRequest request)
        {
            Entity.Models.Attribute attribute = null;
            List<AttributeValue> newList = null;
            try
            {
                attribute = new Entity.Models.Attribute();
                attribute.AttributeNameEn = request.AttributeNameEn;
                attribute.AttributeNameTh = request.AttributeNameTh;
                attribute.AttributeUnitEn = request.AttributeUnitEn;
                attribute.AttributeUnitTh = request.AttributeUnitTh;
                attribute.DataType = request.DataType;
                attribute.DataValidation = request.DataValidation;
                attribute.DefaultValue = request.DefaultValue;
                attribute.DisplayNameEn = request.DisplayNameEn;
                attribute.DisplayNameTh = request.DisplayNameTh;
                attribute.ShowAdminFlag = request.ShowAdminFlag;
                attribute.ShowGlobalFilterFlag = request.ShowGlobalFilterFlag;
                attribute.ShowGlobalSearchFlag = request.ShowGlobalSearchFlag;
                attribute.ShowLocalFilterFlag = request.ShowLocalFilterFlag;
                attribute.ShowLocalSearchFlag = request.ShowLocalSearchFlag;
                attribute.VariantDataType = request.VariantDataType;
                attribute.VariantStatus = request.VariantStatus;
                attribute.AllowHtmlFlag = request.AllowHtmlFlag;
                attribute.CreatedBy = this.User.Email();
                attribute.CreatedDt = DateTime.Now;
                attribute.UpdatedBy = this.User.Email();
                attribute.UpdatedDt = DateTime.Now;
                attribute.Status = Constant.STATUS_ACTIVE;
                #region Validation
                if (string.IsNullOrEmpty(attribute.AttributeNameEn))
                { 
                    return Request.CreateResponse(HttpStatusCode.NotAcceptable, "AttributeNameEn is required");
                }
                if (string.IsNullOrEmpty(attribute.AttributeNameTh))
                {
                    return Request.CreateResponse(HttpStatusCode.NotAcceptable, "AttributeNameTh is required");
                }
                #endregion

                attribute = db.Attributes.Add(attribute);
                db.SaveChanges();
                if (request.AttributeValues != null && request.AttributeValues.Count  > 0)
                {
                    newList = new List<AttributeValue>();
                    foreach(AttributeValueRequest val in request.AttributeValues)
                    {
                        AttributeValue value = new AttributeValue();
                        value.AttributeValueEn = val.AttributeValueEn;
                        value.AttributeValueTh = val.AttributeValueTh;
                        value.Status = Constant.STATUS_ACTIVE;
                        value.CreatedBy = this.User.Email();
                        value.CreatedDt = DateTime.Now;
                        value.UpdatedBy = this.User.Email();
                        value.UpdatedDt = DateTime.Now;
                        newList.Add(db.AttributeValues.Add(value));
                    }
                    db.SaveChanges();
                    if(newList != null && newList.Count > 0)
                    {
                        foreach(AttributeValue val in newList)
                        {
                            AttributeValueMap map = new AttributeValueMap();
                            map.AttributeId = attribute.AttributeId;
                            map.AttributeValueId = val.AttributeValueId;
                            map.CreatedBy = this.User.Email();
                            map.CreatedDt = DateTime.Now;
                            map.UpdatedBy = this.User.Email();
                            map.UpdatedDt = DateTime.Now;
                            db.AttributeValueMaps.Add(map);
                        }
                        db.SaveChanges();
                    }
                }
                return GetAttribute(attribute.AttributeId);
            }
            catch
            {
                if(attribute != null)
                {
                    db.Dispose();
                    db = new ColspEntities();
                    if (newList != null && newList.Count > 0)
                    {
                        db.AttributeValues.RemoveRange(newList);
                    }
                    db.Attributes.Remove(attribute);
                    db.SaveChanges();
                }
                
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, HttpErrorMessage.InternalServerError);
            }
        }

        [Route("api/Attributes/{attributeId}")]
        [HttpPut]
        public HttpResponseMessage SaveChangeAttribute([FromUri] int attributeId, AttributeRequest request)
        {

            Entity.Models.Attribute attribute = null;
            List<AttributeValue> newList = null;
            try
            {

                #region Validation
                if (request.AttributeId.Equals(0))
                {
                    return Request.CreateResponse(HttpStatusCode.NotAcceptable, "Attribute is invalid");
                }
                if (string.IsNullOrEmpty(request.AttributeNameEn))
                {
                    return Request.CreateResponse(HttpStatusCode.NotAcceptable, "AttributeNameEn is required");
                }
                if (string.IsNullOrEmpty(request.AttributeNameTh))
                {
                    return Request.CreateResponse(HttpStatusCode.NotAcceptable, "AttributeNameTh is required");
                }

                attribute = db.Attributes.Where(w=>w.AttributeId.Equals(attributeId)).Include("AttributeValueMaps.AttributeValue").SingleOrDefault();
                if(attribute == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, HttpErrorMessage.NotFound);
                }
                #endregion

                attribute.AttributeNameEn = request.AttributeNameEn;
                attribute.AttributeNameTh = request.AttributeNameTh;
                attribute.AttributeUnitEn = request.AttributeUnitEn;
                attribute.AttributeUnitTh = request.AttributeUnitTh;
                attribute.DataType = request.DataType;
                attribute.DataValidation = request.DataValidation;
                attribute.DefaultValue = request.DefaultValue;
                attribute.DisplayNameEn = request.DisplayNameEn;
                attribute.DisplayNameTh = request.DisplayNameTh;
                attribute.ShowAdminFlag = request.ShowAdminFlag;
                attribute.ShowGlobalFilterFlag = request.ShowGlobalFilterFlag;
                attribute.ShowGlobalSearchFlag = request.ShowGlobalSearchFlag;
                attribute.ShowLocalFilterFlag = request.ShowLocalFilterFlag;
                attribute.ShowLocalSearchFlag = request.ShowLocalSearchFlag;
                attribute.VariantDataType = request.VariantDataType;
                attribute.VariantStatus = request.VariantStatus;
                attribute.AllowHtmlFlag = request.AllowHtmlFlag;
                attribute.UpdatedBy = this.User.Email();
                attribute.UpdatedDt = DateTime.Now;
                attribute.Status = Constant.STATUS_ACTIVE;
               
                if (request.AttributeValues != null && request.AttributeValues.Count > 0)
                {
                    newList = new List<AttributeValue>();
                    foreach (AttributeValueRequest valRq in request.AttributeValues)
                    {
                        if(valRq.AttributeValueId != null)
                        {
                            db.AttributeValueMaps.RemoveRange(
                                db.AttributeValueMaps.Where(w=>w.AttributeId==attribute.AttributeId
                                &&w.AttributeValueId==valRq.AttributeValueId));
                        }

                        AttributeValue value = new AttributeValue();
                        value.AttributeValueEn = valRq.AttributeValueEn;
                        value.AttributeValueTh = valRq.AttributeValueTh;
                        value.Status = Constant.STATUS_ACTIVE;
                        value.CreatedBy = this.User.Email();
                        value.CreatedDt = DateTime.Now;
                        value.UpdatedBy = this.User.Email();
                        value.UpdatedDt = DateTime.Now;
                        newList.Add(db.AttributeValues.Add(value));
                    }
                }
                db.SaveChanges();
                if(newList != null && newList.Count > 0)
                {
                    foreach (AttributeValue val in newList)
                    {
                        AttributeValueMap map = new AttributeValueMap();
                        map.AttributeId = attribute.AttributeId;
                        map.AttributeValueId = val.AttributeValueId;
                        map.CreatedBy = this.User.Email();
                        map.CreatedDt = DateTime.Now;
                        map.UpdatedBy = this.User.Email();
                        map.UpdatedDt = DateTime.Now;
                        db.AttributeValueMaps.Add(map);
                    }
                    db.SaveChanges();
                }
                return GetAttribute(attribute.AttributeId);
            }
            catch
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, HttpErrorMessage.InternalServerError);
            }
        }

        [Route("api/Attributes")]
        [HttpDelete]
        public HttpResponseMessage DeleteAttribute(List<AttributeRequest> request)
        {
            try
            {
                if (request == null || request.Count == 0)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, "Invalid request");
                }
                var setList = db.Attributes.Include(i=>i.ProductStageAttributes)
                    .Include(i=>i.ProductStageVariants)
                    .Include(i=>i.ProductStageVariants1).ToList();
                foreach (AttributeRequest setRq in request)
                {
                    var current = setList.Where(w => w.AttributeId.Equals(setRq.AttributeId)).SingleOrDefault();
                    if (current == null)
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.NotFound, HttpErrorMessage.NotFound);
                    }
                    if((current.ProductStageAttributes != null && current.ProductStageAttributes.Count > 0 )
                        || (current.ProductStageVariants != null && current.ProductStageVariants.Count > 0)
                        || (current.ProductStageVariants1!= null && current.ProductStageVariants1.Count > 0))
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable,"Attribute has product or variant associate");
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

        //duplicate
        [Route("api/Attributes/{attributeId}")]
        [HttpPost]
        public HttpResponseMessage DuplicateAttribute(int attributeId)
        {
            try
            {
                AttributeRequest response = GetAttibuteResponse(db, attributeId);
                if (response == null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, HttpErrorMessage.NotFound);
                }
                else
                {
                    response.AttributeId = null;
                    return AddAttribute(response);
                }
            }
            catch
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, HttpErrorMessage.InternalServerError);
            }
        }

        [Route("api/Attributes/Visibility")]
        [HttpPut]
        public HttpResponseMessage VisibilityAttribute(List<AttributeRequest> request)
        {
            try
            {
                if (request == null || request.Count == 0)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, "Invalid request");
                }
                var setList = db.Attributes.ToList();
                foreach (AttributeRequest setRq in request)
                {
                    if (!Constant.STATUS_VISIBLE.Equals(setRq.Status)
                        && !Constant.STATUS_NOT_VISIBLE.Equals(setRq.Status))
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, "Invalid status");
                    }
                    var current = setList.Where(w => w.AttributeId.Equals(setRq.AttributeId)).SingleOrDefault();
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

        private AttributeRequest GetAttibuteResponse(ColspEntities db,int attributeId)
        {
            var attr = db.Attributes.Where(w => w.AttributeId.Equals(attributeId)).Include(i => i.AttributeValueMaps.Select(s => s.AttributeValue)).SingleOrDefault();
            if (attr != null)
            {
                AttributeRequest attribute = new AttributeRequest();
                attribute.AttributeId = attr.AttributeId;
                attribute.AttributeNameEn = attr.AttributeNameEn;
                attribute.AttributeNameTh = attr.AttributeNameTh;
                attribute.AttributeUnitEn = attr.AttributeUnitEn;
                attribute.AttributeUnitTh = attr.AttributeUnitTh;
                attribute.DataType = attr.DataType;
                attribute.DataValidation = attr.DataValidation;
                attribute.DefaultValue = attr.DefaultValue;
                attribute.DisplayNameEn = attr.DisplayNameEn;
                attribute.DisplayNameTh = attr.DisplayNameTh;
                attribute.ShowAdminFlag = attr.ShowAdminFlag;
                attribute.ShowGlobalFilterFlag = attr.ShowGlobalFilterFlag;
                attribute.ShowGlobalSearchFlag = attr.ShowGlobalSearchFlag;
                attribute.ShowLocalFilterFlag = attr.ShowLocalFilterFlag;
                attribute.ShowLocalSearchFlag = attr.ShowLocalSearchFlag;
                attribute.VariantDataType = attr.VariantDataType;
                attribute.VariantStatus = attr.VariantStatus;
                attribute.AllowHtmlFlag = attr.AllowHtmlFlag;
                attribute.Status = attr.Status;

                if (attr.AttributeValueMaps != null && attr.AttributeValueMaps.Count > 0)
                {
                    attribute.AttributeValues = new List<AttributeValueRequest>();
                    foreach (AttributeValueMap map in attr.AttributeValueMaps)
                    {
                        AttributeValueRequest val = new AttributeValueRequest();
                        val.AttributeValueId = map.AttributeValue.AttributeValueId;
                        val.AttributeValueEn = map.AttributeValue.AttributeValueEn;
                        val.AttributeValueId = map.AttributeValue.AttributeValueId;
                        val.AttributeValueTh = map.AttributeValue.AttributeValueTh;
                        attribute.AttributeValues.Add(val);
                    }
                }

                return attribute;
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

        private bool AttributeExists(int id)
        {
            return db.Attributes.Count(e => e.AttributeId == id) > 0;
        }
    }
}