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
using Colsp.Api.Helpers;
using System.Threading.Tasks;
using System.IO;
using Colsp.Api.Filters;
using Colsp.Api.Services;

namespace Colsp.Api.Controllers
{
	public class AttributesController : ApiController
	{
		private ColspEntities db = new ColspEntities();
		private AttributeService attributeService = new AttributeService();

		[Route("api/Attributes/DefaultAttribute")]
		[HttpGet]
		[ClaimsAuthorize(Permission = new string[] { "2", "3", "35", "34" })]
		public HttpResponseMessage GetDefaultAttribute()
		{
			try
			{
				var defaultAttributes = attributeService.GetDefaultAttribute(db);
				return Request.CreateResponse(HttpStatusCode.OK, defaultAttributes);
			}
			catch (Exception e)
			{
				return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.GetBaseException().Message);
			}
		}

		[Route("api/Attributes")]
		[HttpGet]
		[ClaimsAuthorize(Permission = new string[] { "7" })]
		public HttpResponseMessage GetAttributes([FromUri] AttributeRequest request)
		{
			try
			{
				var response = attributeService.GetAttributes(request, db);
				return Request.CreateResponse(HttpStatusCode.OK, response);
			}
			catch (Exception e)
			{
				return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.GetBaseException().Message);
			}
		}

		[Route("api/Attributes/{attributeId}")]
		[HttpGet]
		[ClaimsAuthorize(Permission = new string[] { "7" })]
		public HttpResponseMessage GetAttribute(int attributeId)
		{
			try
			{
				AttributeRequest attribute = attributeService.GetAttibuteResponse(db, attributeId);
				return Request.CreateResponse(HttpStatusCode.OK, attribute);
			}
			catch (Exception e)
			{
				return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.GetBaseException().Message);
			}
		}

		[Route("api/Attributes")]
		[HttpPost]
		[ClaimsAuthorize(Permission = new string[] { "7" })]
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
				var email = User.UserRequest().Email;
				var currentDt = SystemHelper.GetCurrentDateTime();
				attributeService.SetupAttribute(attribute, request, email, currentDt,true, db);
				attribute.AttributeId = db.GetNextAttributeId().SingleOrDefault().Value;
				attribute = db.Attributes.Add(attribute);
				Util.DeadlockRetry(db.SaveChanges, "Attribute");
				return Request.CreateResponse(HttpStatusCode.OK, attributeService.SetupResponse(attribute));
			}
			catch (Exception e)
			{
				return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.GetBaseException().Message);
			}
		}

		[Route("api/Attributes/{attributeId}")]
		[HttpPut]
		[ClaimsAuthorize(Permission = new string[] { "7" })]
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
				var email = User.UserRequest().Email;
				var currentDt = SystemHelper.GetCurrentDateTime();
				attributeService.SetupAttribute(attribute, request, email, currentDt,false, db);
				Util.DeadlockRetry(db.SaveChanges, "Attribute");
				return Request.CreateResponse(HttpStatusCode.OK, attributeService.SetupResponse(attribute));
			}
			catch (Exception e)
			{
				return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.GetBaseException().Message);
			}
		}

		[Route("api/Attributes")]
		[HttpDelete]
		[ClaimsAuthorize(Permission = new string[] { "7" })]
		public HttpResponseMessage DeleteAttribute(List<AttributeRequest> request)
		{
			try
			{
				attributeService.DeleteAttribute(request,db);
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
		[ClaimsAuthorize(Permission = new string[] { "7" })]
		public HttpResponseMessage DuplicateAttribute(int attributeId)
		{
			try
			{
				AttributeRequest response = attributeService.GetAttibuteResponse(db, attributeId);
				return AddAttribute(response);
			}
			catch (Exception e)
			{
				return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.GetBaseException().Message);
			}
		}

		[Route("api/Attributes/Visibility")]
		[HttpPut]
		[ClaimsAuthorize(Permission = new string[] { "7" })]
		public HttpResponseMessage VisibilityAttribute(List<AttributeRequest> request)
		{
			try
			{
				attributeService.VisibilityAttribute(request,db);
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
		[ClaimsAuthorize(Permission = new string[] { "7" })]
		public async Task<HttpResponseMessage> UploadFileImage()
		{
			try
			{
				#region Validation
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
				#endregion
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