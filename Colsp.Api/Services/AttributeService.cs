using Colsp.Api.Constants;
using Colsp.Api.Extensions;
using Colsp.Api.Helpers;
using Colsp.Entity.Models;
using Colsp.Model.Requests;
using Colsp.Model.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Colsp.Api.Services
{
	public class AttributeService
	{
		public void SetupAttribute(Entity.Models.Attribute attribute, AttributeRequest request, string email, DateTime currentDt,bool isNew, ColspEntities db)
		{
			attribute.AttributeNameEn = Validation.ValidateUniqueName(request.AttributeNameEn, "Attribute Name (English)", 255);
			#region Validate Attribute Name
			if (db.Attributes.Where(w => w.AttributeId != attribute.AttributeId && w.AttributeNameEn.Equals(attribute.AttributeNameEn)).Count() != 0)
			{
				throw new Exception(string.Concat(attribute.AttributeNameEn, " has already been used."));
			}
			#endregion
			attribute.DisplayNameEn = Validation.ValidateString(request.DisplayNameEn, "Display Name (English)", true, 255, false);
			attribute.DisplayNameTh = Validation.ValidateString(request.DisplayNameTh, "Display Name (Thai)", true, 255, false);
			attribute.AttributeDescriptionEn = Validation.ValidateString(request.AttributeDescriptionEn, "Attribute Description (English)", true, 1000, false, string.Empty);
			attribute.DataType = Validation.ValidateString(request.DataType, "Attribute Input Type", true, 2, true, Constant.DATA_TYPE_STRING, new List<string>() { Constant.DATA_TYPE_STRING, Constant.DATA_TYPE_LIST, Constant.DATA_TYPE_HTML, Constant.DATA_TYPE_CHECKBOX });
			attribute.DefaultAttribute = request.DefaultAttribute;
			attribute.VariantStatus = request.VariantStatus;
			#region Validate Data Type
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
			#endregion
			attribute.VisibleTo = Validation.ValidateString(request.VisibleTo, "Visible To", true, 2, false, Constant.ATTRIBUTE_VISIBLE_ALL_USER, new List<string>() { Constant.ATTRIBUTE_VISIBLE_ADMIN, Constant.ATTRIBUTE_VISIBLE_ALL_USER });
			attribute.DataValidation = Validation.ValidateString(request.DataValidation, "Input Validation", false, 2, true, string.Empty);
			attribute.DefaultValue = Validation.ValidateString(request.DefaultValue, "If empty, value equals", false, 255, true, string.Empty);
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
			if (isNew)
			{
				attribute.CreateBy = email;
				attribute.CreateOn = currentDt;
			}
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

		public AttributeRequest SetupResponse(Entity.Models.Attribute attribute)
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

		public AttributeRequest GetAttibuteResponse(ColspEntities db, int attributeId)
		{
			var attr = db.Attributes
				.Where(w => w.AttributeId == attributeId && !w.Status.Equals(Constant.STATUS_REMOVE))
				.Select(s => new AttributeRequest
				{
					AttributeId = s.AttributeId,
					AttributeNameEn = s.AttributeNameEn,
					AttributeDescriptionEn = s.AttributeDescriptionEn,
					DataType = s.DataType,
					DataValidation = s.DataValidation,
					DefaultValue = s.DefaultValue,
					DisplayNameEn = s.DisplayNameEn,
					DisplayNameTh = s.DisplayNameTh,
					ShowAdminFlag = s.ShowAdminFlag,
					ShowGlobalFilterFlag = s.ShowGlobalFilterFlag,
					ShowGlobalSearchFlag = s.ShowGlobalSearchFlag,
					ShowLocalFilterFlag = s.ShowLocalFilterFlag,
					ShowLocalSearchFlag = s.ShowLocalSearchFlag,
					VariantDataType = s.VariantDataType,
					VariantStatus = s.VariantStatus,
					AllowHtmlFlag = s.AllowHtmlFlag,
					Required = s.Required,
					Filterable = s.Filterable,
					Status = s.Status,
					DefaultAttribute = s.DefaultAttribute,
					VisibleTo = s.VisibleTo,
					AttributeValueMaps = s.AttributeValueMaps.Select(sv => new AttributeValueMapRequest
					{
						AttributeValue = sv.AttributeValue == null ? null : new AttributeValueRequest
						{
							AttributeValueId = sv.AttributeValue.AttributeValueId,
							AttributeValueEn = sv.AttributeValue.AttributeValueEn,
							AttributeValueTh = sv.AttributeValue.AttributeValueTh,
							ImageUrl = sv.AttributeValue.ImageUrl,
							Position = sv.AttributeValue.Position
						}
					}).OrderBy(o => o.AttributeValue.Position),
				}).SingleOrDefault();
			if (attr == null)
			{
				throw new Exception(string.Concat("Cannot find attribute with id ", attributeId));
			}
			else
			{
				return attr;
			}
		}

		public List<AttributeRequest> GetDefaultAttribute(ColspEntities db)
		{
			#region Query
			var defaultAttributes = db.Attributes.Where(w => w.DefaultAttribute == true
									&& !w.Status.Equals(Constant.STATUS_REMOVE))
									.Select(s => new AttributeRequest
									{
										AttributeId = s.AttributeId,
										AttributeNameEn = s.AttributeNameEn,
										DisplayNameEn = s.DisplayNameEn,
										DataType = s.DataType,
										Required = s.Required,
										Status = s.Status,
										VariantDataType = s.VariantDataType,
										VariantStatus = s.VariantStatus,
										DataValidation = s.DataValidation,
										VisibleTo = s.VisibleTo,
										AttributeValueMaps = s.AttributeValueMaps.Select(sv => new AttributeValueMapRequest
										{
											AttributeId = sv.AttributeId,
											AttributeValueId = sv.AttributeValueId,
											AttributeValue = sv.AttributeValue == null ? null : new AttributeValueRequest
											{
												AttributeValueId = sv.AttributeValue.AttributeValueId,
												AttributeValueEn = sv.AttributeValue.AttributeValueEn,
												AttributeValueTh = sv.AttributeValue.AttributeValueTh
											}
										})
									});
			#endregion
			return defaultAttributes.ToList();
		}

		public PaginatedResponse GetAttributes(AttributeRequest request, ColspEntities db)
		{
			if (request == null)
			{
				throw new Exception("Invalid request");
			}
			var attrList = from attr in db.Attributes
						   where !attr.Status.Equals(Constant.STATUS_REMOVE)
						   select new AttributeRequest
						   {
							   AttributeId = attr.AttributeId,
							   AttributeNameEn = attr.AttributeNameEn,
							   DisplayNameEn = attr.DisplayNameEn,
							   DisplayNameTh = attr.DisplayNameTh,
							   VariantStatus = attr.VariantStatus,
							   DataType = attr.DataType,
							   Status = attr.Status,
							   DefaultAttribute = attr.DefaultAttribute,
							   UpdatedDt = attr.UpdateOn,
							   AttributeSetCount = attr.AttributeSetMaps.Count()
						   };
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
			return response;
		}

		public void DeleteAttribute(List<AttributeRequest> request, ColspEntities db)
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
		}

		public void VisibilityAttribute(List<AttributeRequest> request, ColspEntities db)
		{
			if (request == null || request.Count == 0)
			{
				throw new Exception("Invalid request");
			}
			var ids = request.Select(s => s.AttributeId).ToList();
			var setList = db.Attributes
				.Where(w => ids.Contains(w.AttributeId) && !w.Status.Equals(Constant.STATUS_REMOVE));
			foreach (AttributeRequest setRq in request)
			{
				var current = setList.Where(w => w.AttributeId.Equals(setRq.AttributeId)).SingleOrDefault();
				if (current == null)
				{
					throw new Exception(string.Concat("Cannot find attribute with id ", setRq.AttributeId));
				}
				current.Status = setRq.Status;
			}
		}
	}
}