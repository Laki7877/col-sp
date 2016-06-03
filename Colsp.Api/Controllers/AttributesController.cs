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
using System.IO;

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
				var attribute = db.Attributes
					.Where(w => w.DefaultAttribute == true
						&& !w.Status.Equals(Constant.STATUS_REMOVE))
					.Select(s => new
					{
						s.AttributeId,
						s.AttributeNameEn,
						s.DisplayNameEn,
						s.DataType,
						s.Required,
						s.Status,
						s.VariantDataType,
						s.VariantStatus,
						s.DataValidation,
						AttributeValueMaps = s.AttributeValueMaps
					.Select(sv => new
					{
						sv.AttributeId,
						sv.AttributeValueId,
						AttributeValue = sv.AttributeValue == null ? null : new
						{
							sv.AttributeValue.AttributeValueId,
							sv.AttributeValue.AttributeValueEn,
							sv.AttributeValue.AttributeValueTh
						}
					}),
						s.VisibleTo
					});
				return Request.CreateResponse(HttpStatusCode.OK, attribute);
			}
			catch (Exception e)
			{
				return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.GetBaseException().Message);
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
								   UpdatedDt = attr.UpdateOn,
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
						attrList = attrList.Where(a => a.DataType.Equals(Constant.DATA_TYPE_LIST));
					}
					else if (string.Equals("FreeText", request._filter, StringComparison.OrdinalIgnoreCase))
					{
						attrList = attrList.Where(a => a.DataType.Equals(Constant.DATA_TYPE_STRING));
					}
					else if (string.Equals("HasVariation", request._filter, StringComparison.OrdinalIgnoreCase))
					{
						attrList = attrList.Where(a => a.VariantStatus == true);
					}
					else if (string.Equals("NoVariation", request._filter, StringComparison.OrdinalIgnoreCase))
					{
						attrList = attrList.Where(a => a.VariantStatus == false);
					}
					else if (string.Equals("HTMLBox", request._filter, StringComparison.OrdinalIgnoreCase))
					{
						attrList = attrList.Where(a => a.DataType.Equals(Constant.DATA_TYPE_HTML));
					}
					else if (string.Equals("CheckBox", request._filter, StringComparison.OrdinalIgnoreCase))
					{
						attrList = attrList.Where(a => a.DataType.Equals(Constant.DATA_TYPE_CHECKBOX));
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
				return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.GetBaseException().Message);
			}
		}

		[Route("api/Attributes/{attributeId}")]
		[HttpGet]
		public HttpResponseMessage GetAttribute(int attributeId)
		{
			try
			{
				AttributeRequest attribute = GetAttibuteResponse(db, attributeId);
				if (attribute == null)
				{
					return Request.CreateResponse(HttpStatusCode.NotFound, HttpStatusCode.NotFound);
				}
				return Request.CreateResponse(HttpStatusCode.OK, attribute);
			}
			catch (Exception e)
			{
				return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.GetBaseException().Message);
			}
		}

		[Route("api/Attributes")]
		[HttpPost]
		public HttpResponseMessage AddAttribute(AttributeRequest request)
		{
			Entity.Models.Attribute attribute = null;
			try
			{
				if (request == null)
				{
					throw new Exception("Invalid request");
				}
				attribute = new Entity.Models.Attribute();
				string email = User.UserRequest().Email;
				DateTime cuurentDt = DateTime.Now;
				SetupAttribute(attribute, request, email, cuurentDt);
				attribute.CreateBy = email;
				attribute.CreateOn = cuurentDt;
				attribute.AttributeId = db.GetNextAttributeId().SingleOrDefault().Value;
				attribute = db.Attributes.Add(attribute);
				Util.DeadlockRetry(db.SaveChanges, "Attribute");
				return Request.CreateResponse(HttpStatusCode.OK, SetupResponse(attribute));
			}
			catch (Exception e)
			{
				return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.GetBaseException().Message);
			}
		}

		[Route("api/Attributes/{attributeId}")]
		[HttpPut]
		public HttpResponseMessage SaveChangeAttribute([FromUri] int attributeId, AttributeRequest request)
		{
			Entity.Models.Attribute attribute = null;
			try
			{
				if (request == null)
				{
					throw new Exception("Invalid request");
				}
				attribute = db.Attributes
					.Where(w => w.AttributeId.Equals(attributeId) && !w.Status.Equals(Constant.STATUS_REMOVE))
					.Include(i => i.AttributeValueMaps
					.Select(s => s.AttributeValue))
					.SingleOrDefault();
				if (attribute == null)
				{
					throw new Exception("Cannot find attribute " + attributeId);
				}
				string email = User.UserRequest().Email;
				DateTime cuurentDt = DateTime.Now;
				SetupAttribute(attribute, request, email, cuurentDt);
				Util.DeadlockRetry(db.SaveChanges, "Attribute");
				return Request.CreateResponse(HttpStatusCode.OK, SetupResponse(attribute));
				//return GetAttribute(attribute.AttributeId);
			}
			catch (Exception e)
			{
				return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.GetBaseException().Message);
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
				var ids = request.Select(s => s.AttributeId).ToList();
				var productAttributeCount = db.ProductStageAttributes.Where(w => ids.Contains(w.AttributeId)).Count();
				if (productAttributeCount != 0)
				{
					throw new Exception("Cannot delete attribute because it has been associated with products.");
				}
				var attributeSetCount = db.AttributeSetMaps.Where(w => ids.Contains(w.AttributeId)).Count();
				if (attributeSetCount != 0)
				{
					throw new Exception("Cannot delete attribute because it has been associated with attribute set.");
				}

				var setList = db.Attributes.Where(w => ids.Contains(w.AttributeId) && !w.Status.Equals(Constant.STATUS_REMOVE));
				foreach (AttributeRequest setRq in request)
				{
					var current = setList.Where(w => w.AttributeId.Equals(setRq.AttributeId)).SingleOrDefault();
					if (current == null)
					{
						throw new Exception(HttpErrorMessage.NotFound);
					}
					current.Status = Constant.STATUS_REMOVE;
					db.Attributes.Remove(current);
				}
				Util.DeadlockRetry(db.SaveChanges, "Attribute");
				return Request.CreateResponse(HttpStatusCode.OK);
			}
			catch (Exception e)
			{
				return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.GetBaseException().Message);
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
				return AddAttribute(response);
			}
			catch (Exception e)
			{
				return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.GetBaseException().Message);
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
				var ids = request.Select(s => s.AttributeId).ToList();
				var setList = db.Attributes
					.Where(w => ids.Contains(w.AttributeId) && !w.Status.Equals(Constant.STATUS_REMOVE));
				foreach (AttributeRequest setRq in request)
				{
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
			catch (Exception e)
			{
				return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.GetBaseException().Message);
			}
		}

		[Route("api/AttributeValueImages")]
		[HttpPost]
		public async Task<HttpResponseMessage> UploadFileImage()
		{
			try
			{
				if (!Request.Content.IsMimeMultipartContent())
				{
					throw new Exception("In valid content multi-media");
				}
				var streamProvider = new MultipartFormDataStreamProvider(Path.Combine(AppSettingKey.IMAGE_ROOT_PATH, AppSettingKey.BRAND_FOLDER));
				try
				{
					await Request.Content.ReadAsMultipartAsync(streamProvider);
				}
				catch (Exception)
				{
					throw new Exception("Image size exceeded " + 5 + " mb");
				}
				//var fileUpload = await Util.SetupImage(Request, AppSettingKey.IMAGE_ROOT_PATH, AppSettingKey.ATTRIBUTE_VALUE_FOLDER, 100, 100, 100, 100, 5, true);
				#region Validate Image
				ImageRequest fileUpload = null;
				foreach (MultipartFileData fileData in streamProvider.FileData)
				{
					fileUpload = Util.SetupImage(Request,
						fileData,
						AppSettingKey.IMAGE_ROOT_FOLDER,
						AppSettingKey.BRAND_FOLDER, 100, 100, 100, 100, 5, true);
					break;
				}
				#endregion




				return Request.CreateResponse(HttpStatusCode.OK, fileUpload);
			}
			catch (Exception e)
			{
				return Request.CreateResponse(HttpStatusCode.NotAcceptable, e.GetBaseException().Message);
			}
		}

		private AttributeRequest GetAttibuteResponse(ColspEntities db, int attributeId)
		{
			var attr = db.Attributes
				.Where(w => w.AttributeId == attributeId && !w.Status.Equals(Constant.STATUS_REMOVE))
				.Select(s => new
				{
					s.AttributeId,
					s.AttributeNameEn,
					s.AttributeDescriptionEn,
					s.DataType,
					s.DataValidation,
					s.DefaultValue,
					s.DisplayNameEn,
					s.DisplayNameTh,
					s.ShowAdminFlag,
					s.ShowGlobalFilterFlag,
					s.ShowGlobalSearchFlag,
					s.ShowLocalFilterFlag,
					s.ShowLocalSearchFlag,
					s.VariantDataType,
					s.VariantStatus,
					s.AllowHtmlFlag,
					s.Required,
					s.Filterable,
					s.Status,
					s.DefaultAttribute,
					s.VisibleTo,
					AttributeValueMaps = s.AttributeValueMaps.Select(sv => new
					{
						AttributeValue = sv.AttributeValue == null ? null : new
						{
							sv.AttributeValue.AttributeValueId,
							sv.AttributeValue.AttributeValueEn,
							sv.AttributeValue.AttributeValueTh,
							sv.AttributeValue.ImageUrl,
							sv.AttributeValue.Position
						}
					}),
				}).SingleOrDefault();
			if (attr == null)
			{
				return null;
			}
			AttributeRequest attribute = new AttributeRequest();
			attribute.AttributeId = attr.AttributeId;
			attribute.AttributeNameEn = attr.AttributeNameEn;
			attribute.AttributeDescriptionEn = attr.AttributeDescriptionEn;
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
			if (attr.AttributeValueMaps != null)
			{
				attribute.AttributeValues = new List<AttributeValueRequest>();
				foreach (var map in attr.AttributeValueMaps.OrderBy(o => o.AttributeValue.Position))
				{
					AttributeValueRequest val = new AttributeValueRequest();
					val.AttributeValueId = map.AttributeValue.AttributeValueId;
					val.AttributeValueEn = map.AttributeValue.AttributeValueEn;
					val.AttributeValueTh = map.AttributeValue.AttributeValueTh;
					val.Position = map.AttributeValue.Position;
					val.Image = new ImageRequest()
					{
						Url = map.AttributeValue.ImageUrl
					};
					attribute.AttributeValues.Add(val);
				}
			}
			return attribute;
		}

		private void SetupAttribute(Entity.Models.Attribute attribute, AttributeRequest request, string email, DateTime currentDt)
		{

			attribute.AttributeNameEn = Validation.ValidateUniqueName(request.AttributeNameEn, "Attribute Name (English)");
			attribute.AttributeDescriptionEn = Validation.ValidateString(request.AttributeDescriptionEn, "Attribute Description (English)", true, 1000, false, string.Empty);
			attribute.DisplayNameEn = Validation.ValidateString(request.DisplayNameEn, "Display Name (English)", true, 100, true);
			attribute.DisplayNameTh = Validation.ValidateString(request.DisplayNameTh, "Display Name (Thai)", true, 100, true);
			attribute.DataType = Validation.ValidateString(request.DataType, "Attribute Input Type", false, 2, true, string.Empty);
			attribute.DefaultAttribute = request.DefaultAttribute;
			attribute.VariantStatus = request.VariantStatus;
			if (!string.IsNullOrEmpty(attribute.DataType))
			{
				if (request.AttributeValues == null || request.AttributeValues.Count == 0)
				{
					if (Constant.DATA_TYPE_LIST.Equals(request.DataType))
					{
						throw new Exception("Data Type Dropdown should have at least 1 value");
					}
					else if (Constant.DATA_TYPE_CHECKBOX.Equals(request.DataType))
					{
						throw new Exception("Data Type Checkbox should have at least 1 value");
					}
				}
			}
			if ((Constant.DATA_TYPE_STRING.Equals(attribute.DataType)
				|| Constant.DATA_TYPE_HTML.Equals(attribute.DataType))
				&& attribute.VariantStatus)
			{
				throw new Exception("Data Type Free Text and HTML Box cannot be variant");
			}
			if (attribute.DefaultAttribute && attribute.VariantStatus)
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
			attribute.Status = Constant.STATUS_ACTIVE;
			attribute.UpdateBy = email;
			attribute.UpdateOn = currentDt;

			#region AttributeValue
			var attributeVal = attribute.AttributeValueMaps.Select(s => s.AttributeValue).ToList();
			AttributeValue value = null;
			AttributeValue current = null;
			if (request.AttributeValues != null)
			{
				foreach (var valRq in request.AttributeValues)
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
							if (!current.AttributeValueEn.Equals(valRq.AttributeValueEn)
								|| !current.AttributeValueTh.Equals(valRq.AttributeValueTh)
								|| !current.ImageUrl.Equals(valRq.Image.Url)
								|| current.Position != valRq.Position)
							{
								current.AttributeValueEn = Validation.ValidateString(valRq.AttributeValueEn, "Attribute Value (English)", true, 100, true);
								current.AttributeValueTh = Validation.ValidateString(valRq.AttributeValueTh, "Attribute Value (Thai)", true, 100, true);
								current.Position = valRq.Position;
								current.ImageUrl = Validation.ValidateString(valRq.Image.Url, "Attribute Value Url", true, 2000, true, string.Empty);
								current.UpdateBy = email;
								current.UpdateOn = currentDt;
							}
							attributeVal.Remove(current);
						}
						else
						{
							addNew = true;
						}
					}
					if (addNew)
					{
						value = new AttributeValue()
						{
							AttributeValueEn = Validation.ValidateString(valRq.AttributeValueEn, "Attribute Value (English)", true, 100, true),
							AttributeValueTh = Validation.ValidateString(valRq.AttributeValueTh, "Attribute Value (Thai)", true, 100, true),
							Position = valRq.Position,
							ImageUrl = Validation.ValidateString(valRq.Image.Url, "Attribute Value Url", true, 2000, true, string.Empty),
							Status = Constant.STATUS_ACTIVE,
							CreateBy = email,
							CreateOn = currentDt,
							UpdateBy = email,
							UpdateOn = currentDt,
						};
						value.AttributeValueId = db.GetNextAttributeValueId().SingleOrDefault().Value;
						value.MapValue = string.Concat(
							Constant.ATTRIBUTE_VALUE_MAP_PREFIX,
							value.AttributeValueId,
							Constant.ATTRIBUTE_VALUE_MAP_SURFIX);
						attribute.AttributeValueMaps.Add(new AttributeValueMap()
						{
							Attribute = attribute,
							AttributeValue = value,
							CreateBy = email,
							CreateOn = currentDt,
							UpdateBy = email,
							UpdateOn = currentDt,
						});
					}
				}
			}
			if (attributeVal != null && attributeVal.Count > 0)
			{
				throw new Exception("Cannot delete attribute value");
			}
			#endregion
		}

		private AttributeRequest SetupResponse(Entity.Models.Attribute attribute)
		{
			AttributeRequest response = new AttributeRequest();
			response.AttributeId = attribute.AttributeId;
			response.AttributeDescriptionEn = attribute.AttributeDescriptionEn;
			response.AttributeNameEn = attribute.AttributeNameEn;
			response.DataType = attribute.DataType;
			response.DataValidation = attribute.DataValidation;
			response.DefaultValue = attribute.DefaultValue;
			response.DisplayNameEn = attribute.DisplayNameEn;
			response.DisplayNameTh = attribute.DisplayNameTh;
			response.ShowAdminFlag = attribute.ShowAdminFlag;
			response.ShowGlobalFilterFlag = attribute.ShowGlobalFilterFlag;
			response.ShowGlobalSearchFlag = attribute.ShowGlobalSearchFlag;
			response.ShowLocalFilterFlag = attribute.ShowLocalFilterFlag;
			response.ShowLocalSearchFlag = attribute.ShowLocalSearchFlag;
			response.VariantDataType = attribute.VariantDataType;
			response.VariantStatus = attribute.VariantStatus;
			response.AllowHtmlFlag = attribute.AllowHtmlFlag;
			response.Required = attribute.Required;
			response.Filterable = attribute.Filterable;
			response.Status = attribute.Status;
			response.DefaultAttribute = attribute.DefaultAttribute;
			response.VisibleTo = attribute.VisibleTo;
			if (attribute.AttributeValueMaps != null)
			{
				response.AttributeValues = new List<AttributeValueRequest>();
				foreach (var map in attribute.AttributeValueMaps)
				{
					AttributeValueRequest val = new AttributeValueRequest();
					val.AttributeValueId = map.AttributeValue.AttributeValueId;
					val.AttributeValueEn = map.AttributeValue.AttributeValueEn;
					val.AttributeValueTh = map.AttributeValue.AttributeValueTh;
					val.Position = map.AttributeValue.Position;
					val.Image = new ImageRequest()
					{
						Url = map.AttributeValue.ImageUrl
					};
					response.AttributeValues.Add(val);
				}
			}
			return response;
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