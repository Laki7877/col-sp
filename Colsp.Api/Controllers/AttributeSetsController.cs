using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Colsp.Entity.Models;
using Colsp.Model.Requests;
using Colsp.Api.Extensions;
using System;
using System.Data.Entity;
using System.Collections.Generic;
using Colsp.Api.Helpers;
using System.Linq.Dynamic;
using Colsp.Api.Filters;
using Colsp.Api.Services;

namespace Colsp.Api.Controllers
{
	public class AttributeSetsController : ApiController
	{
		private ColspEntities db = new ColspEntities();
		private AttributeSetsService attributeSetService = new AttributeSetsService();

		[Route("api/AttributeSets")]
		[HttpGet]
		[ClaimsAuthorize(Permission = new string[] { "7","8", "2", "3", "35", "34" })]
		public HttpResponseMessage GetAttributeSets([FromUri] AttributeSetRequest request)
		{
			try
			{
				var response = attributeSetService.GetAttributeSets(request,User.ShopRequest(), db);
				return Request.CreateResponse(HttpStatusCode.OK, response);
			}
			catch (Exception e)
			{
				return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.GetBaseException().Message);
			}
		}

		[Route("api/AttributeSets/{attributeSetId}")]
		[HttpGet]
		[ClaimsAuthorize(Permission = new string[] { "7", "2", "3", "35", "34" })]
		public HttpResponseMessage GetAttributeSets([FromUri]int attributeSetId)
		{
			try
			{
				AttributeSetRequest response = attributeSetService.GetAttributeSetResponse(db, attributeSetId);
				return Request.CreateResponse(HttpStatusCode.OK, response);
			}
			catch (Exception e)
			{
				return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.GetBaseException().Message);
			}
		}

		[Route("api/AttributeSets")]
		[HttpPost]
		[ClaimsAuthorize(Permission = new string[] { "7" })]
		public HttpResponseMessage AddAttributeSet(AttributeSetRequest request)
		{
			try
			{
				if (request == null)
				{
					throw new Exception("Invalid request");
				}
				AttributeSet attributeSet = attributeSet = new AttributeSet();
				var email = User.UserRequest().Email;
				var currentDt = SystemHelper.GetCurrentDateTime();
				attributeSetService.SetupAttributeSet(attributeSet, request, email, currentDt,true, db);
				attributeSet.AttributeSetId = db.GetNextAttributeSetId().SingleOrDefault().Value;
				db.AttributeSets.Add(attributeSet);
				Util.DeadlockRetry(db.SaveChanges, "AttributeSet");
				return GetAttributeSets(attributeSet.AttributeSetId);
			}
			catch (Exception e)
			{
				return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.GetBaseException().Message);
			}
		}

		[Route("api/AttributeSets/{attributeSetId}")]
		[HttpPut]
		[ClaimsAuthorize(Permission = new string[] { "7" })]
		public HttpResponseMessage SaveAttributeSet([FromUri]int attributeSetId, AttributeSetRequest request)
		{
			try
			{
				if (request == null)
				{
					throw new Exception("Invalid request");
				}
				var attrSet = db.AttributeSets.Where(w => w.AttributeSetId.Equals(attributeSetId))
					.Include(i => i.AttributeSetMaps
						.Select(s => s.Attribute.ProductStageAttributes
							.Select(sa => sa.ProductStage.ProductStageGroup)))
					.Include(i => i.AttributeSetTags)
					.Include(i => i.GlobalCatAttributeSetMaps.Select(s => s.GlobalCategory))
					.SingleOrDefault();
				if (attrSet == null)
				{
					throw new Exception("Cannot find attribute set " + attributeSetId);
				}
				var email = User.UserRequest().Email;
				var currentDt = SystemHelper.GetCurrentDateTime();
				attributeSetService.SetupAttributeSet(attrSet, request, email, currentDt,false, db);
				Util.DeadlockRetry(db.SaveChanges, "AttributeSet");
				return Request.CreateResponse(HttpStatusCode.OK, attributeSetService.SetupResponse(attrSet));
			}
			catch (Exception e)
			{
				return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.GetBaseException().Message);
			}
		}

		//duplicate
		[Route("api/AttributeSets/{attributeSetId}")]
		[HttpPost]
		[ClaimsAuthorize(Permission = new string[] { "7" })]
		public HttpResponseMessage DuplicateAttributeSet([FromUri]int attributeSetId)
		{
			try
			{
				AttributeSetRequest response = attributeSetService.GetAttributeSetResponse(db, attributeSetId);
				response.AttributeSetId = 0;
				return AddAttributeSet(response);
			}
			catch (Exception e)
			{
				return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.GetBaseException().Message);
			}
		}

		[Route("api/AttributeSets/Visibility")]
		[HttpPut]
		[ClaimsAuthorize(Permission = new string[] { "7" })]
		public HttpResponseMessage VisibilityAttributeSet(List<AttributeSetRequest> request)
		{
			try
			{
				attributeSetService.VisibilityAttributeSet(request, db);
				Util.DeadlockRetry(db.SaveChanges, "AttributeSet");
				return Request.CreateResponse(HttpStatusCode.OK);
			}
			catch (Exception e)
			{
				return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.GetBaseException().Message);
			}
		}

		[Route("api/AttributeSets")]
		[HttpDelete]
		[ClaimsAuthorize(Permission = new string[] { "7" })]
		public HttpResponseMessage DeleteAttributeSet(List<AttributeSetRequest> request)
		{
			try
			{
				attributeSetService.DeleteAttributeSet(request, db);
				Util.DeadlockRetry(db.SaveChanges, "AttributeSet");
				return Request.CreateResponse(HttpStatusCode.OK);
			}
			catch (Exception e)
			{
				return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.GetBaseException().Message);
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

	}
}