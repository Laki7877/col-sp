using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
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
                               select new
                               {
                                   attr.AttributeId,
                                   attr.AttributeNameEn,
                                   attr.AttributeNameTh,
                                   attr.DisplayNameEn,
                                   attr.DisplayNameTh,
                                   attr.DataType,
                                   attr.Status,
                                   attr.CreatedDt,
                                   attr.UpdatedDt,
                                   AttributeSetCount = attr.AttributeSetMaps.AsEnumerable().Count()
                               };

                if (request == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, attrList);
                }
                request.DefaultOnNull();

                
                if (request.SearchText != null)
                {
                    attrList = attrList.Where(a => a.AttributeNameEn.Contains(request.SearchText)
                    || a.AttributeNameTh.Contains(request.SearchText));
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
                var attr = db.Attributes.Where(w => w.AttributeId.Equals(attributeId)).Include("AttributeValueMaps.AttributeValue").SingleOrDefault();
                if(attr != null)
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

                    if(attr.AttributeValueMaps != null && attr.AttributeValueMaps.Count > 0)
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

                    return Request.CreateResponse(HttpStatusCode.OK, attribute);
                }
                else
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, HttpErrorMessage.NotFound);
                }
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
                            db.AttributeValueMaps.Add(map);
                        }
                        db.SaveChanges();
                    }
                }
                return GetAttribute(attribute.AttributeId);
            }
            catch
            {
                db.Dispose();
                db = new ColspEntities();
                if(newList != null && newList.Count > 0)
                {
                    db.AttributeValues.RemoveRange(newList);
                }
                if(attribute != null)
                {
                    db.Attributes.Remove(attribute);
                }
                db.SaveChanges();
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, HttpErrorMessage.InternalServerError);
            }
        }

        [Route("api/Attributes/{attributeId}")]
        [HttpPut]
        public HttpResponseMessage EditAttribute([FromUri] int attributeId, AttributeRequest request)
        {
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

                var attr = db.Attributes.Where(w=>w.AttributeId.Equals(attributeId)).Include("AttributeValueMaps.AttributeValue").SingleOrDefault();
                if(attr == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, HttpErrorMessage.NotFound);
                }
                #endregion

                attr.AttributeNameEn = request.AttributeNameEn;
                attr.AttributeNameTh = request.AttributeNameTh;
                attr.AttributeUnitEn = request.AttributeUnitEn;
                attr.AttributeUnitTh = request.AttributeUnitTh;
                attr.DataType = request.DataType;
                attr.DataValidation = request.DataValidation;
                attr.DefaultValue = request.DefaultValue;
                attr.DisplayNameEn = request.DisplayNameEn;
                attr.DisplayNameTh = request.DisplayNameTh;
                attr.ShowAdminFlag = request.ShowAdminFlag;
                attr.ShowGlobalFilterFlag = request.ShowGlobalFilterFlag;
                attr.ShowGlobalSearchFlag = request.ShowGlobalSearchFlag;
                attr.ShowLocalFilterFlag = request.ShowLocalFilterFlag;
                attr.ShowLocalSearchFlag = request.ShowLocalSearchFlag;
                attr.VariantDataType = request.VariantDataType;
                attr.VariantStatus = request.VariantStatus;
                attr.AllowHtmlFlag = request.AllowHtmlFlag;
                attr.CreatedBy = this.User.Email();
                attr.CreatedDt = DateTime.Now;
                attr.Status = Constant.STATUS_ACTIVE;
                List<AttributeValueMap> mapList = attr.AttributeValueMaps.ToList();
                if (request.AttributeValues != null && request.AttributeValues.Count > 0)
                {
                    foreach(AttributeValueRequest valRq in request.AttributeValues)
                    {
                        
                        bool isNew = false;
                        if (mapList == null || mapList.Count == 0)
                        {
                            isNew = true;
                        }
                        if (!isNew)
                        {
                            AttributeValueMap current = mapList.Where(w => w.AttributeValueId.Equals(valRq.AttributeValueId)).SingleOrDefault();
                            if(current != null)
                            {
                                current.UpdatedBy = this.User.Email();
                                current.UpdatedDt = DateTime.Now;
                                mapList.Remove(current);
                            }
                            else
                            {
                                isNew = true;
                            }
                        }
                        if (isNew)
                        {
                            AttributeValueMap map = new AttributeValueMap();
                            map.AttributeId = attr.AttributeId;
                            map.AttributeValueId = valRq.AttributeValueId.Value;
                            map.CreatedBy = this.User.Email();
                            map.CreatedDt = DateTime.Now;
                            db.AttributeValueMaps.Add(map);
                        }
                    }
                }
                if(mapList != null && mapList.Count > 0)
                {
                    db.AttributeValueMaps.RemoveRange(mapList);
                }
                db.SaveChanges();
                return GetAttribute(attr.AttributeId);
            }
            catch
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, HttpErrorMessage.InternalServerError);
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