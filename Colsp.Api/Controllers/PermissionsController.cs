using System;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Colsp.Entity.Models;
using Colsp.Model.Requests;
using Colsp.Api.Constants;
using Colsp.Api.Extensions;
using Colsp.Model.Responses;
using Colsp.Api.Filters;

namespace Colsp.Api.Controllers
{
	public class PermissionsController : ApiController
	{
		private ColspEntities db = new ColspEntities();

		[Route("api/Permissions/Shop")]
		[HttpGet]
		[ClaimsAuthorize(Permission = new string[] { "10" })]
		public HttpResponseMessage GetShopPermission([FromUri] PermissionRequest request)
		{
			try
			{
				var permissionList = db.Permissions.Where(w => w.Type.Equals(Constant.SHOP_TYPE))
					.Select(s => new
					{
						s.PermissionName,
						s.PermissionGroup,
						s.PermissionId,
						s.Position,
						s.Parent
					});
				if (request == null)
				{
					return Request.CreateResponse(HttpStatusCode.OK, permissionList);
				}
				request.DefaultOnNull();
				var total = permissionList.Count();
				var pagedUsers = permissionList.Paginate(request);
				var response = PaginatedResponse.CreateResponse(pagedUsers, request, total);
				return Request.CreateResponse(HttpStatusCode.OK, response);
			}
			catch (Exception e)
			{
				return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
			}
		}

		[Route("api/Permissions/Admin")]
		[HttpGet]
		[ClaimsAuthorize(Permission = new string[] { "11" })]
		public HttpResponseMessage GetUserPermissions([FromUri] PermissionRequest request)
		{
			try
			{
				var permissionList = db.Permissions.Where(w => w.Type.Equals(Constant.USER_TYPE_ADMIN))
					.Select(s => new
					{
						s.PermissionName,
						s.PermissionGroup,
						s.PermissionId,
						s.Position,
						s.Parent
					});
				if (request == null)
				{
					return Request.CreateResponse(HttpStatusCode.OK, permissionList);
				}
				request.DefaultOnNull();

				var total = permissionList.Count();
				var pagedUsers = permissionList.Paginate(request);
				var response = PaginatedResponse.CreateResponse(pagedUsers, request, total);
				return Request.CreateResponse(HttpStatusCode.OK, response);
			}
			catch (Exception e)
			{
				return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
			}
		}

		[Route("api/Permissions/Seller")]
		[HttpGet]
		[ClaimsAuthorize(Permission = new string[] { "57" })]
		public HttpResponseMessage GetSellerPermissions([FromUri] PermissionRequest request)
		{
			try
			{
				var permissionList = db.Permissions.Where(w => w.Type.Equals(Constant.USER_TYPE_SELLER))
					.Select(s => new
					{
						s.PermissionName,
						s.PermissionGroup,
						s.PermissionId,
						s.Position,
						s.Parent
					});
				if (request == null)
				{
					return Request.CreateResponse(HttpStatusCode.OK, permissionList);
				}
				request.DefaultOnNull();

				var total = permissionList.Count();
				var pagedUsers = permissionList.Paginate(request);
				var response = PaginatedResponse.CreateResponse(pagedUsers, request, total);
				return Request.CreateResponse(HttpStatusCode.OK, response);
			}
			catch (Exception e)
			{
				return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
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

		private bool PermissionExists(int id)
		{
			return db.Permissions.Count(e => e.PermissionId == id) > 0;
		}
	}
}