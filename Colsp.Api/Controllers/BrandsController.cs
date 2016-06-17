using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Colsp.Entity.Models;
using Colsp.Model.Requests;
using Colsp.Api.Constants;
using Colsp.Model.Responses;
using Colsp.Api.Extensions;
using System.Threading.Tasks;
using System.Data;
using System.Data.Entity;
using System.Collections.Generic;
using Colsp.Api.Helpers;
using System.IO;
using Colsp.Api.Filters;
using Colsp.Api.Services;

namespace Colsp.Api.Controllers
{
	public class BrandsController : ApiController
	{
		private ColspEntities db = new ColspEntities();
		private BrandService brandService = new BrandService();

		[Route("api/BrandImages")]
		[HttpPost]
		[ClaimsAuthorize(Permission = new string[] { "6" })]
		public async Task<HttpResponseMessage> UploadFile()
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
				#region Validate Image
				string type = streamProvider.FormData["Type"];
				ImageRequest fileUpload = null;
				if ("Logo".Equals(type))
				{
					foreach (MultipartFileData fileData in streamProvider.FileData)
					{
						fileUpload = Util.SetupImage(Request,
							fileData,
							AppSettingKey.IMAGE_ROOT_FOLDER,
							AppSettingKey.BRAND_FOLDER, 500, 500, 1000, 1000, 5, true);
						break;
					}

				}
				else if ("Banner".Equals(type))
				{
					foreach (MultipartFileData fileData in streamProvider.FileData)
					{
						fileUpload = Util.SetupImage(Request,
							fileData,
							AppSettingKey.IMAGE_ROOT_FOLDER,
							AppSettingKey.BRAND_FOLDER, 1920, 1080, 1920, 1080, 5, false);
						break;
					}
				}
				else if ("SmallBanner".Equals(type))
				{
					foreach (MultipartFileData fileData in streamProvider.FileData)
					{
						fileUpload = Util.SetupImage(Request,
							fileData,
							AppSettingKey.IMAGE_ROOT_FOLDER,
							AppSettingKey.BRAND_FOLDER, 1600, 900, 1600, 900, 5, false);
						break;
					}
				}
				else
				{
					foreach (MultipartFileData fileData in streamProvider.FileData)
					{
						fileUpload = Util.SetupImage(Request,
							fileData,
							AppSettingKey.IMAGE_ROOT_FOLDER,
							AppSettingKey.BRAND_FOLDER, Constant.ImageRatio.IMAGE_RATIO_16_9);
						break;
					}
				}
				#endregion
				return Request.CreateResponse(HttpStatusCode.OK, fileUpload);
			}
			catch (Exception e)
			{
				return Request.CreateResponse(HttpStatusCode.NotAcceptable, e.GetBaseException().Message);
			}
		}

		[Route("api/Brands")]
		[HttpGet]
		[ClaimsAuthorize(Permission = new string[] { "6", "2", "3", "35", "34" })]
		public HttpResponseMessage GetBrand([FromUri] BrandRequest request)
		{
			try
			{
				var response = brandService.GetBrand(request,User.ShopRequest(),User.BrandRequest(), db);
				return Request.CreateResponse(HttpStatusCode.OK, response);
			}
			catch (Exception e)
			{
				return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.GetBaseException().Message);
			}
		}

		[Route("api/Brands/{brandId}")]
		[HttpGet]
		[ClaimsAuthorize(Permission = new string[] { "6" })]
		public HttpResponseMessage GetBrand([FromUri]int brandId)
		{
			try
			{
				var response = brandService.GetBrand(brandId,db);
				return Request.CreateResponse(HttpStatusCode.OK, response);
			}
			catch (Exception e)
			{
				return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.GetBaseException().Message);
			}
		}

		[Route("api/Brands")]
		[HttpDelete]
		[ClaimsAuthorize(Permission = new string[] { "6" })]
		public HttpResponseMessage DeleteBrand(List<BrandRequest> request)
		{
			try
			{
				brandService.DeleteBrand(request, db);
				Util.DeadlockRetry(db.SaveChanges, "Brand");
				return Request.CreateResponse(HttpStatusCode.OK);
			}
			catch (Exception e)
			{
				return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.GetBaseException().Message);
			}
		}

		[Route("api/Brands")]
		[HttpPost]
		[ClaimsAuthorize(Permission = new string[] { "6" })]
		public HttpResponseMessage AddBrand(BrandRequest request)
		{
			try
			{
				Brand brand = new Brand();
				var email = User.UserRequest().Email;
				var currentDt = SystemHelper.GetCurrentDateTime();
				brandService.SetupBrand(brand, request, email, currentDt,true, db);
				if(db.Brands.Where(w=>w.UrlKey.Equals(brand.UrlKey)).Count() > 0)
				{
					throw new Exception(string.Concat(brand.UrlKey, " has already been used."));
				}
				brand.BrandId = db.GetNextBrandId().SingleOrDefault().Value;
				db.Brands.Add(brand);
				Util.DeadlockRetry(db.SaveChanges, "Brand");
				return GetBrand(brand.BrandId);
			}
			catch (Exception e)
			{
				return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.GetBaseException().Message);
			}
		}

		[Route("api/Brands/{brandId}")]
		[HttpPut]
		[ClaimsAuthorize(Permission = new string[] { "6" })]
		public HttpResponseMessage SaveChangeBrand([FromUri]int brandId, BrandRequest request)
		{
			try
			{
				if (request == null)
				{
					throw new Exception("Invalid request");
				}
				var brand = db.Brands.Where(w => w.BrandId == brandId)
					.Include(i => i.BrandImages)
					.Include(i => i.BrandFeatureProducts)
					.SingleOrDefault();
				if (brand == null)
				{
					throw new Exception(string.Concat("Cannot find brand id ", brandId));
				}
				var email = User.UserRequest().Email;
				var currentDt = SystemHelper.GetCurrentDateTime();
				brandService.SetupBrand(brand, request, email, currentDt, false, db);
				Util.DeadlockRetry(db.SaveChanges, "Brand");
				return GetBrand(brand.BrandId);
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