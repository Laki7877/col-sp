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
using Colsp.Api.Helpers;
using System.Threading.Tasks;

namespace Colsp.Api.Controllers
{
    public class AttributesController : ApiController
    {
        private ColspEntities db = new ColspEntities();

        [Route("api/Attributes/DefaultAttribute")]
        [HttpGet]
        public HttpResponseMessage GetDefaultAttribute()
        {
            try
            {
                var attribute = db.Attributes.Where(w => w.DefaultAttribute == true).Select(s => new
                {
                    s.AttributeId,
                    s.AttributeNameEn,
                    s.DataType,
                    s.Required,
                    s.Status,
                    s.VariantDataType,
                    s.VariantStatus,
                    s.DataValidation,
                    AttributeValueMaps = s.AttributeValueMaps.Select(sv =>
                    new {
                        sv.AttributeId,
                        sv.AttributeValueId,
                        AttributeValue = new { sv.AttributeValue.AttributeValueId, sv.AttributeValue.AttributeValueEn, sv.AttributeValue.AttributeValueTh }
                    })
                });
                return Request.CreateResponse(HttpStatusCode.OK, attribute);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }

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
                                   attr.DisplayNameEn,
                                   attr.DisplayNameTh,
                                   attr.VariantStatus,
                                   attr.DataType,
                                   attr.Status,
                                   attr.DefaultAttribute,
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
                    attrList = attrList.Where(a => a.AttributeNameEn.Contains(request.SearchText));
                }
                if (!string.IsNullOrEmpty(request._filter))
                {
                    if (string.Equals("Dropdown", request._filter, StringComparison.OrdinalIgnoreCase))
                    {
                        attrList = attrList.Where(a => a.DataType.Equals("LT"));
                    }
                    else if (string.Equals("FreeText", request._filter, StringComparison.OrdinalIgnoreCase))
                    {
                        attrList = attrList.Where(a => a.DataType.Equals("ST"));
                    }
                    else if (string.Equals("HasVariation", request._filter, StringComparison.OrdinalIgnoreCase))
                    {
                        attrList = attrList.Where(a => a.VariantStatus==true);
                    }
                    else if (string.Equals("NoVariation", request._filter, StringComparison.OrdinalIgnoreCase))
                    {
                        attrList = attrList.Where(a => a.VariantStatus == false);
                    }
                    else if (string.Equals("HTMLBox", request._filter, StringComparison.OrdinalIgnoreCase))
                    {
                        attrList = attrList.Where(a => a.DataType.Equals("HB"));
                    }
                    else if (string.Equals("DefaultAttribute", request._filter, StringComparison.OrdinalIgnoreCase))
                    {
                        attrList = attrList.Where(a => a.DefaultAttribute == true);
                    }
                    else if (string.Equals("NoDefaultAttribute", request._filter, StringComparison.OrdinalIgnoreCase))
                    {
                        attrList = attrList.Where(a => a.DefaultAttribute == false);
                    }
                }
                var total = attrList.Count();
                var pagedAttribute = attrList.Paginate(request);
                var response = PaginatedResponse.CreateResponse(pagedAttribute, request, total);
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
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
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }

        [Route("api/Attributes")]
        [HttpPost]
        public HttpResponseMessage AddAttribute(AttributeRequest request)
        {
            Entity.Models.Attribute attribute = null;
            try
            {
                attribute = new Entity.Models.Attribute();
                SetupAttribute(attribute, request);
                attribute.CreatedBy = User.UserRequest().Email;
                attribute.CreatedDt = DateTime.Now;
                attribute.UpdatedBy = User.UserRequest().Email;
                attribute.UpdatedDt = DateTime.Now;
                attribute.Status = Constant.STATUS_ACTIVE;
                #region AttributeValue
                if (request.AttributeValues != null && request.AttributeValues.Count  > 0)
                {
                    AttributeValue attributeValue = null;
                    foreach (AttributeValueRequest val in request.AttributeValues)
                    {
                        attributeValue = new AttributeValue();
                        attributeValue.AttributeValueEn = val.AttributeValueEn;
                        attributeValue.AttributeValueTh = val.AttributeValueTh;
                        if(val.Image != null)
                        {
                            attributeValue.ImageUrl = val.Image.url;
                        }
                        attributeValue.MapValue = string.Concat("((", attributeValue.AttributeValueId, "))");
                        attributeValue.Status = Constant.STATUS_ACTIVE;
                        attributeValue.CreatedBy = User.UserRequest().Email;
                        attributeValue.CreatedDt = DateTime.Now;
                        attributeValue.UpdatedBy = User.UserRequest().Email;
                        attributeValue.UpdatedDt = DateTime.Now;
                        attribute.AttributeValueMaps.Add(new AttributeValueMap()
                        {
                            Attribute = attribute,
                            AttributeValue = attributeValue,
                            CreatedBy = User.UserRequest().Email,
                            CreatedDt = DateTime.Now,
                            UpdatedBy = User.UserRequest().Email,
                            UpdatedDt = DateTime.Now,
                        });
                    }
                }
                #endregion
                db.Attributes.Add(attribute);
                Util.DeadlockRetry(db.SaveChanges, "Attribute");
                return GetAttribute(attribute.AttributeId);
            }
            catch (Exception e)
            {
                #region Rollback
                if (attribute != null && attribute.AttributeId != 0)
                {
                    db.Attributes.Remove(attribute);
                    Util.DeadlockRetry(db.SaveChanges, "Attribute");
                }
                #endregion
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }

        [Route("api/Attributes/{attributeId}")]
        [HttpPut]
        public HttpResponseMessage SaveChangeAttribute([FromUri] int attributeId, AttributeRequest request)
        {

            Entity.Models.Attribute attribute = null;
            try
            {
                attribute = db.Attributes.Where(w=>w.AttributeId.Equals(attributeId))
                    .Include(i=>i.AttributeValueMaps.Select(s=>s.AttributeValue)).SingleOrDefault();
                if(attribute == null)
                {
                    throw new Exception("Cannot find attribute " + attributeId);
                }
                SetupAttribute(attribute, request);
                attribute.UpdatedBy = User.UserRequest().Email;
                attribute.UpdatedDt = DateTime.Now;
                attribute.Status = Constant.STATUS_ACTIVE;

                var attributeVal = attribute.AttributeValueMaps.Select(s => s.AttributeValue).ToList();
                #region AttributeValue
                AttributeValue value = null;
                AttributeValue current = null;
                if (request.AttributeValues != null && request.AttributeValues.Count > 0)
                {
                    foreach (AttributeValueRequest valRq in request.AttributeValues)
                    {
                        bool addNew = false;
                        if (attributeVal == null || attributeVal.Count == 0)
                        {
                            addNew = true;
                        }
                        if (!addNew)
                        {
                            current = attributeVal.Where(w => w.AttributeValueId == valRq.AttributeValueId).SingleOrDefault();
                            if (current != null)
                            {
                                current.AttributeValueEn = valRq.AttributeValueEn;
                                current.AttributeValueTh = valRq.AttributeValueTh;
                                if(valRq.Image != null)
                                {
                                    current.ImageUrl = valRq.Image.url;
                                }
                                else
                                {
                                    current.ImageUrl = null;
                                }
                                current.UpdatedBy = User.UserRequest().Email;
                                current.UpdatedDt = DateTime.Now;
                                attributeVal.Remove(current);
                            }
                            else
                            {
                                addNew = true;
                            }
                        }
                        if (addNew)
                        {
                            value = new AttributeValue();
                            value.AttributeValueEn = valRq.AttributeValueEn;
                            value.AttributeValueTh = valRq.AttributeValueTh;
                            if (valRq.Image != null)
                            {
                                value.ImageUrl = valRq.Image.url;
                            }
                            value.MapValue = string.Concat("((", value.AttributeValueId, "))");
                            value.Status = Constant.STATUS_ACTIVE;
                            value.CreatedBy = User.UserRequest().Email;
                            value.CreatedDt = DateTime.Now;
                            value.UpdatedBy = User.UserRequest().Email;
                            value.UpdatedDt = DateTime.Now;
                            attribute.AttributeValueMaps.Add(new AttributeValueMap()
                            {
                                Attribute = attribute,
                                AttributeValue = value,
                                CreatedBy = User.UserRequest().Email,
                                CreatedDt = DateTime.Now,
                                UpdatedBy = User.UserRequest().Email,
                                UpdatedDt = DateTime.Now,
                            });
                        }
                    }
                }
                if (attributeVal != null && attributeVal.Count > 0)
                {
                    throw new Exception("Cannot delete attribute value");
                    //db.AttributeValues.RemoveRange(attributeVal);
                }
                #endregion
                Util.DeadlockRetry(db.SaveChanges, "Attribute");
                return GetAttribute(attribute.AttributeId);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
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
                    throw new Exception("Invalid request");
                }
                var setList = db.Attributes
                    .Include(i=>i.ProductStageAttributes)
                    .Include(i=>i.AttributeValueMaps).ToList();
                foreach (AttributeRequest setRq in request)
                {
                    var current = setList.Where(w => w.AttributeId.Equals(setRq.AttributeId)).SingleOrDefault();
                    if (current == null)
                    {
                        throw new Exception(HttpErrorMessage.NotFound);
                    }
                    if(current.ProductStageAttributes != null && current.ProductStageAttributes.Count > 0 )
                    {
                        throw new Exception("Attribute has product or variant associate");
                    }
                    if (current.AttributeValueMaps != null && current.AttributeValueMaps.Count > 0)
                    {

                    }
                    db.Attributes.Remove(current);
                }
                Util.DeadlockRetry(db.SaveChanges, "Attribute");
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
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
                    throw new Exception("Cannot find attribute with id " + attributeId);
                }
                else
                {
                    response.AttributeId = 0;
                    return AddAttribute(response);
                }
            }
            catch(Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable,e.Message);
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
                Util.DeadlockRetry(db.SaveChanges, "Attribute");
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch(Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable,e.Message);
            }
        }

        [Route("api/AttributeValueImages")]
        [HttpPost]
        public async Task<HttpResponseMessage> UploadFileImage()
        {
            try
            {
                FileUploadRespond fileUpload = await Util.SetupImage(Request, AppSettingKey.IMAGE_ROOT_PATH, AppSettingKey.ATTRIBUTE_VALUE_FOLDER, 100, 100, 100, 100, 5, true);
                return Request.CreateResponse(HttpStatusCode.OK, fileUpload);
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, e);
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
                attribute.Required = attr.Required;
                attribute.Filterable = attr.Filterable;
                attribute.Status = attr.Status;
                attribute.DefaultAttribute = attr.DefaultAttribute;
                attribute.VisibleTo = attr.VisibleTo;

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
                        val.Image = new ImageRequest()
                        {
                            url = map.AttributeValue.ImageUrl
                        };
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

        private void SetupAttribute(Entity.Models.Attribute attribute, AttributeRequest request)
        {
            attribute.AttributeNameEn = Validation.ValidateString(request.AttributeNameEn, "Attribute Name (English)", true, 100, true);
            attribute.DisplayNameEn = Validation.ValidateString(request.DisplayNameEn, "Display Name (English)", true, 100, true);
            attribute.DisplayNameTh = Validation.ValidateString(request.DisplayNameTh, "Display Name (Thai)", true, 100, true);
            attribute.DataType = Validation.ValidateString(request.DataType, "Attribute Input Type", false, 2, true,string.Empty);
            if (!string.IsNullOrEmpty(attribute.DataType))
            {
                if (request.AttributeValues == null || request.AttributeValues.Count == 0)
                {
                    if (Constant.DATA_TYPE_LIST.Equals(request.DataType))
                    {
                        throw new Exception("DataType Dropdown should have at least 1 value");
                    }
                    else if (Constant.DATA_TYPE_CHECKBOX.Equals(request.DataType))
                    {
                        throw new Exception("DataType Checkbox should have at least 1 value");
                    }
                }
            }
            attribute.DefaultAttribute = request.DefaultAttribute;
            attribute.VariantStatus = request.VariantStatus;
            if(attribute.DefaultAttribute  && attribute.VariantStatus)
            {
                throw new Exception("Default attribute cannot be variant");
            }
            attribute.VisibleTo = Validation.ValidateString(request.VisibleTo, "Visible To", false, 2, false, string.Empty, new List<string>() { Constant.ATTRIBUTE_VISIBLE_ADMIN, Constant.ATTRIBUTE_VISIBLE_ALL_USER, string.Empty });


            attribute.DataValidation = Validation.ValidateString(request.DataValidation, "Input Validation", false, 2, true, string.Empty);
            attribute.DefaultValue = Validation.ValidateString(request.DefaultValue, "If empty, value equals", false, 100, true, string.Empty);
            attribute.ShowAdminFlag = request.ShowAdminFlag;
            attribute.ShowGlobalFilterFlag = request.ShowGlobalFilterFlag;
            attribute.ShowGlobalSearchFlag = request.ShowGlobalSearchFlag;
            attribute.ShowLocalFilterFlag = request.ShowLocalFilterFlag;
            attribute.ShowLocalSearchFlag = request.ShowLocalSearchFlag;
            attribute.VariantDataType = Validation.ValidateString(request.VariantDataType, "Variant Display Type", false, 2, true, string.Empty);
            
            attribute.AllowHtmlFlag = request.AllowHtmlFlag;
            attribute.Filterable = request.Filterable;
            attribute.Required = request.Required;
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