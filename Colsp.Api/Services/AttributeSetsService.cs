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
	public class AttributeSetsService
	{
		public AttributeSetRequest SetupResponse(AttributeSet attributeSet)
		{
			AttributeSetRequest response = new AttributeSetRequest();
			response.AttributeSetId = attributeSet.AttributeSetId;
			response.AttributeSetNameEn = attributeSet.AttributeSetNameEn;
			response.AttributeSetDescriptionEn = attributeSet.AttributeSetDescriptionEn;
			response.Visibility = attributeSet.Visibility;
			response.Status = attributeSet.Status;
			if (attributeSet.AttributeSetMaps != null)
			{
				response.Attributes = new List<AttributeRequest>();
				foreach (var map in attributeSet.AttributeSetMaps)
				{
					if (map.Attribute == null)
					{
						continue;
					}
					AttributeRequest attr = new AttributeRequest();
					attr.AttributeId = map.AttributeId;
					attr.AttributeNameEn = map.Attribute.AttributeNameEn;
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
					attr.ProductCount = map.Attribute.ProductStageAttributes.Count;
					attr.Status = map.Attribute.Status;
					if (map.Attribute.AttributeValueMaps != null)
					{
						foreach (var val in map.Attribute.AttributeValueMaps)
						{
							attr.AttributeValues.Add(new AttributeValueRequest()
							{
								AttributeValueEn = val.AttributeValue.AttributeValueEn,
								AttributeValueId = val.AttributeValue.AttributeValueId,
								AttributeValueTh = val.AttributeValue.AttributeValueTh,
							});
						}
					}
					response.Attributes.Add(attr);
				}
			}
			if (attributeSet.GlobalCatAttributeSetMaps != null)
			{
				response.Category = new List<CategoryRequest>();
				foreach (var map in attributeSet.GlobalCatAttributeSetMaps)
				{
					CategoryRequest cat = new CategoryRequest();
					cat.CategoryId = map.GlobalCategory.CategoryId;
					cat.NameEn = map.GlobalCategory.NameEn;
					response.Category.Add(cat);
				}
			}
			if (attributeSet.AttributeSetTags != null)
			{
				response.Tags = new List<TagRequest>();
				foreach (var map in attributeSet.AttributeSetTags)
				{
					TagRequest tag = new TagRequest();
					tag.TagName = map.Tag;
					response.Tags.Add(tag);
				}
			}
			return response;
		}

		public AttributeSetRequest GetAttributeSetResponse(ColspEntities db, int attributeSetId)
		{
			var attrSet = db.AttributeSets.Where(w => w.AttributeSetId == attributeSetId).Select(s => new
			{
				s.AttributeSetId,
				s.AttributeSetNameEn,
				s.AttributeSetDescriptionEn,
				s.Visibility,
				s.Status,
				GlobalCatAttributeSetMaps = s.GlobalCatAttributeSetMaps.Select(sc => new
				{
					GlobalCategory = sc.GlobalCategory == null ? null : new
					{
						sc.GlobalCategory.CategoryId,
						sc.GlobalCategory.NameEn,
					}
				}),
				AttributeSetTags = s.AttributeSetTags.Select(st => new
				{
					st.Tag
				}),
				AttributeSetMaps = s.AttributeSetMaps.Select(sm => new
				{
					sm.AttributeId,
					sm.Position,
					Attribute = sm.Attribute == null ? null : new
					{
						sm.Attribute.AttributeNameEn,
						sm.Attribute.DataType,
						sm.Attribute.DataValidation,
						sm.Attribute.DefaultValue,
						sm.Attribute.DisplayNameEn,
						sm.Attribute.DisplayNameTh,
						sm.Attribute.ShowAdminFlag,
						sm.Attribute.ShowGlobalFilterFlag,
						sm.Attribute.ShowGlobalSearchFlag,
						sm.Attribute.ShowLocalFilterFlag,
						sm.Attribute.ShowLocalSearchFlag,
						sm.Attribute.VariantDataType,
						sm.Attribute.VariantStatus,
						sm.Attribute.AllowHtmlFlag,
						sm.Attribute.Status,
						sm.Attribute.Required,
						ProductCount = sm.Attribute.ProductStageAttributes.Count,
						AttributeValueMaps = sm.Attribute.AttributeValueMaps.Select(sv => new
						{
							sv.AttributeValueId,
							sv.AttributeValue.AttributeValueEn,
							sv.AttributeValue.AttributeValueTh,
						})
					}
				}).OrderBy(o => o.Position),
			}).SingleOrDefault();

			if (attrSet == null)
			{
				throw new Exception("Cannot find attribute set id " + attributeSetId);
			}
			AttributeSetRequest response = new AttributeSetRequest();
			response.AttributeSetId = attrSet.AttributeSetId;
			response.AttributeSetNameEn = attrSet.AttributeSetNameEn;
			response.AttributeSetDescriptionEn = attrSet.AttributeSetDescriptionEn;
			response.Visibility = attrSet.Visibility;
			response.Status = attrSet.Status;
			if (attrSet.AttributeSetMaps != null)
			{
				response.Attributes = new List<AttributeRequest>();
				foreach (var map in attrSet.AttributeSetMaps)
				{
					if (map.Attribute == null)
					{
						continue;
					}
					AttributeRequest attr = new AttributeRequest();
					attr.AttributeId = map.AttributeId;
					attr.AttributeNameEn = map.Attribute.AttributeNameEn;
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
					attr.ProductCount = map.Attribute.ProductCount;
					attr.Required = map.Attribute.Required;
					attr.Status = map.Attribute.Status;
					if (map.Attribute.AttributeValueMaps != null)
					{
						foreach (var val in map.Attribute.AttributeValueMaps)
						{
							attr.AttributeValues.Add(new AttributeValueRequest()
							{
								AttributeValueEn = val.AttributeValueEn,
								AttributeValueId = val.AttributeValueId,
								AttributeValueTh = val.AttributeValueTh,
							});
						}
					}
					response.Attributes.Add(attr);
				}
			}
			if (attrSet.GlobalCatAttributeSetMaps != null)
			{
				response.Category = new List<CategoryRequest>();
				foreach (var map in attrSet.GlobalCatAttributeSetMaps)
				{
					CategoryRequest cat = new CategoryRequest();
					cat.CategoryId = map.GlobalCategory.CategoryId;
					cat.NameEn = map.GlobalCategory.NameEn;
					response.Category.Add(cat);
				}
			}
			if (attrSet.AttributeSetTags != null)
			{
				response.Tags = new List<TagRequest>();
				foreach (var map in attrSet.AttributeSetTags)
				{
					TagRequest tag = new TagRequest();
					tag.TagName = map.Tag;
					response.Tags.Add(tag);
				}
			}
			return response;
		}

		public void SetupAttributeSet(AttributeSet set, AttributeSetRequest request, string email, DateTime currentDt, bool isNew, ColspEntities db)
		{
			set.AttributeSetNameEn = Validation.ValidateString(request.AttributeSetNameEn, "Attribute Set Name (English)", true, 100, true);
			set.AttributeSetDescriptionEn = Validation.ValidateString(request.AttributeSetDescriptionEn, "Attribute Set Description (English)", false, 500, false, string.Empty);
			set.Visibility = request.Visibility;
			set.Status = Constant.STATUS_ACTIVE;
			set.UpdateBy = email;
			set.UpdateOn = currentDt;
			if (isNew)
			{
				set.CreateBy = email;
				set.CreateOn = currentDt;
			}
			#region Attribute Map
			var attributeIds = request.Attributes.Select(s => s.AttributeId).ToList();
			List<AttributeSetMap> mapList = set.AttributeSetMaps.ToList();
			if (request.Attributes != null)
			{
				var attribute = db.Attributes.Where(w => attributeIds.Any(a => a == w.AttributeId && w.DefaultAttribute == true)).Count();
				if (attribute != 0)
				{
					throw new Exception("Cannot map attribute that is default attribute to attribute set");
				}
				int position = 1;
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
							current.Position = position++;
							mapList.Remove(current);
						}
						else
						{
							addNew = true;
						}
					}
					if (addNew)
					{
						set.AttributeSetMaps.Add(new AttributeSetMap()
						{
							AttributeId = attrRq.AttributeId,
							Position = position++,
							CreateBy = email,
							CreateOn = currentDt,
							UpdateBy = email,
							UpdateOn = currentDt
						});
					}
				}
			}
			if (mapList != null && mapList.Count > 0)
			{
				foreach (AttributeSetMap map in mapList)
				{
					if (map.Attribute.ProductStageAttributes != null
						&& map.Attribute.ProductStageAttributes.Count > 0
						&& map.Attribute.ProductStageAttributes.Any(a => a.ProductStage.ProductStageGroup.AttributeSetId == set.AttributeSetId))
					{
						throw new Exception("Cannot delete attribute maping " + map.Attribute.AttributeNameEn + " in attribute set " + set.AttributeSetNameEn + " with product associated");
					}
				}
				db.AttributeSetMaps.RemoveRange(mapList);
			}
			#endregion
			#region Tag Map
			if (set.AttributeSetTags != null && set.AttributeSetTags.Count > 0)
			{
				db.AttributeSetTags.RemoveRange(set.AttributeSetTags);
				set.AttributeSetTags.Clear();
			}
			if (request.Tags != null)
			{
				foreach (TagRequest tagRq in request.Tags)
				{
					if (string.IsNullOrWhiteSpace(tagRq.TagName)
							|| set.AttributeSetTags.Any(a => string.Equals(a.Tag, tagRq.TagName.Trim(), StringComparison.OrdinalIgnoreCase)))
					{
						continue;
					}
					set.AttributeSetTags.Add(new AttributeSetTag()
					{
						Tag = tagRq.TagName,
						CreateBy = email,
						CreateOn = currentDt,
						UpdateBy = email,
						UpdateOn = currentDt,
					});
				}
			}
			#endregion
		}


		public object GetAttributeSets(AttributeSetRequest request, ShopRequest shopUser, ColspEntities db)
		{
			var attrSet = from atrS in db.AttributeSets
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
									  new
									  {
										  sv.AttributeId,
										  sv.AttributeValueId,
										  AttributeValue = new { sv.AttributeValue.AttributeValueId, sv.AttributeValue.AttributeValueEn, sv.AttributeValue.AttributeValueTh }
									  })
								  }
							  }),
							  AttributeSetTagMaps = atrS.AttributeSetTags.Select(s => new { s.AttributeSetId, Tag = new { TagName = s.Tag } }),
							  AttributeCount = atrS.AttributeSetMaps.Count(),
							  CategoryCount = atrS.GlobalCatAttributeSetMaps.Count(),
							  ProductCount = atrS.ProductStageGroups.Count(),
							  Shops = atrS.ProductStageGroups.Where(w => !Constant.STATUS_REMOVE.Equals(w.Status)).Select(s => s.ShopId).Distinct(),
						  };
			//export page
			if (shopUser != null && request.ByShop)
			{
				var shopId = shopUser.ShopId;
				attrSet = attrSet.Where(w => w.Shops.Contains(shopId));
			}

			if (request == null)
			{
				attrSet = attrSet.Where(w => w.Visibility == true);
				return attrSet;
			}
			request.DefaultOnNull();
			if (!string.IsNullOrEmpty(request.SearchText))
			{
				attrSet = attrSet.Where(a => a.AttributeSetNameEn.Contains(request.SearchText));
			}
			if (!string.IsNullOrEmpty(request._filter))
			{
				//All VisibleNot Visible
				if (string.Equals("Visible", request._filter, StringComparison.OrdinalIgnoreCase))
				{
					attrSet = attrSet.Where(a => a.Visibility == true);
				}
				else if (string.Equals("NotVisible", request._filter, StringComparison.OrdinalIgnoreCase))
				{
					attrSet = attrSet.Where(a => a.Visibility == false);
				}
			}
			var total = attrSet.Count();
			var pagedAttributeSet = attrSet.Paginate(request);
			var response = PaginatedResponse.CreateResponse(pagedAttributeSet, request, total);
			return response;
		}

		public void DeleteAttributeSet(List<AttributeSetRequest> request,ColspEntities db)
		{
			if (request == null || request.Count == 0)
			{
				throw new Exception("Invalid request");
			}
			var ids = request.Select(s => s.AttributeSetId);
			var productMap = db.ProductStageGroups.Where(w => ids.Contains(w.AttributeSetId.HasValue ? w.AttributeSetId.Value : 0)).Select(s => s.AttributeSet.AttributeSetNameEn);
			if (productMap != null && productMap.Count() > 0)
			{
				throw new Exception(string.Concat("Cannot delete arrtibute set ", string.Join(",", productMap), " with product associated"));
			}
			var globalCatMap = db.GlobalCatAttributeSetMaps.Where(w => ids.Contains(w.AttributeSetId)).Select(s => s.AttributeSet.AttributeSetNameEn);
			if (globalCatMap != null && globalCatMap.Count() > 0)
			{
				throw new Exception(string.Concat("Cannot delete arrtibute set ", string.Join(",", globalCatMap), " with global category associated"));
			}
			var attributeMap = db.AttributeSetMaps.Where(w => ids.Contains(w.AttributeSetId)).Select(s => s.AttributeSet.AttributeSetNameEn);
			if (attributeMap != null && attributeMap.Count() > 0)
			{
				throw new Exception(string.Concat("Cannot delete arrtibute set ", string.Join(",", attributeMap), " with attribute associated"));
			}
			var setList = db.AttributeSets.Where(w => ids.Contains(w.AttributeSetId));
			foreach (AttributeSetRequest setRq in request)
			{
				var current = setList.Where(w => w.AttributeSetId.Equals(setRq.AttributeSetId)).SingleOrDefault();
				if (current == null)
				{
					throw new Exception("Cannot find arrtibute set " + setRq.AttributeSetNameEn);
				}
				db.AttributeSets.Remove(current);
			}
		}

		public void VisibilityAttributeSet(List<AttributeSetRequest> request, ColspEntities db)
		{
			if (request == null || request.Count == 0)
			{
				throw new Exception("Invalid request");
			}
			var setList = db.AttributeSets.ToList();
			foreach (AttributeSetRequest setRq in request)
			{
				var current = setList.Where(w => w.AttributeSetId.Equals(setRq.AttributeSetId)).SingleOrDefault();
				if (current == null)
				{
					throw new Exception(HttpErrorMessage.NotFound);
				}
				current.Visibility = setRq.Visibility;
			}
		}
	}
}