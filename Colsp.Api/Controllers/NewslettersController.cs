﻿using System;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Colsp.Entity.Models;
using Colsp.Model.Requests;
using System.Threading.Tasks;
using Colsp.Model.Responses;
using Colsp.Api.Constants;
using Colsp.Api.Helpers;
using Colsp.Api.Extensions;
using System.Collections.Generic;
using System.Data.Entity;
using Colsp.Api.Filters;

namespace Colsp.Api.Controllers
{
    public class NewslettersController : ApiController
    {
        private ColspEntities db = new ColspEntities();

        [Route("api/Newsletters")]
        [HttpGet]
        public HttpResponseMessage GetNewsletters([FromUri] NewsletterRequest request)
        {
            try
            {
                var newsLetter = db.Newsletters.Include(i=>i.NewsletterShopMaps)
                    .Where(w => true);
                if(User.ShopRequest() != null)
                {
                    var shopGroup = User.ShopRequest().ShopGroup;
                    var shopId = User.ShopRequest().ShopId;
                    newsLetter = newsLetter.Where(w => (w.PublishedDt == null || DateTime.Now >= w.PublishedDt) && (w.ExpiredDt == null || DateTime.Now <= w.ExpiredDt));
                    newsLetter = newsLetter.Where(w => (Constant.NEWSLETTER_VISIBLE_TO_ALL.Equals(w.VisibleShopGroup) || w.VisibleShopGroup.Equals(shopGroup)) && !w.NewsletterShopMaps.Any(a=>a.ShopId==shopId && Constant.NEWSLETTER_FILTER_EXCLUDE.Equals(a.Filter)));
                    newsLetter = newsLetter.Union(db.Newsletters.Where(w=> !(Constant.NEWSLETTER_VISIBLE_TO_ALL.Equals(w.VisibleShopGroup) || w.VisibleShopGroup.Equals(shopGroup)) && w.NewsletterShopMaps.Any(a=>a.ShopId==shopId && Constant.NEWSLETTER_FILTER_INCLUDE.Equals(a.Filter))));

                }
                if(request == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, newsLetter);
                }
                request.DefaultOnNull();
                var total = newsLetter.Count();
                var response = PaginatedResponse.CreateResponse(newsLetter.Paginate(request), request, total);
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }

        [Route("api/NewsletterImages")]
        [HttpPost]
        public async Task<HttpResponseMessage> UploadFile()
        {
            try
            {
                var fileUpload = await Util.SetupImage(Request, AppSettingKey.IMAGE_ROOT_PATH, AppSettingKey.NEWSLETTER_FOLDER, 1, 1, int.MaxValue, int.MaxValue, int.MaxValue, false);
                return Request.CreateResponse(HttpStatusCode.OK, fileUpload);
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }

        [Route("api/Newsletters/{newsletterId}")]
        [HttpGet]
        public HttpResponseMessage GetNewsletter([FromUri]int newsletterId)
        {
            try
            {
                var newsletter = db.Newsletters.Where(w => w.NewsletterId == newsletterId).Select(s=> new 
                {
                   s.Description,
                   s.Subject,
                   s.VisibleShopGroup,
                   s.PublishedDt,
                   s.ExpiredDt,
                   Image = new ImageRequest() { Url = s.ImageUrl},
                   IncludeShop = s.NewsletterShopMaps.Where(w=>w.Filter.Equals(Constant.NEWSLETTER_FILTER_INCLUDE)).Select(si=>new 
                   {
                       ShopId = si.ShopId,
                       ShopNameEn = si.Shop.ShopNameEn
                   }),
                   ExcludeShop = s.NewsletterShopMaps.Where(w => w.Filter.Equals(Constant.NEWSLETTER_FILTER_EXCLUDE)).Select(si => new
                   {
                        ShopId = si.ShopId,
                        ShopNameEn = si.Shop.ShopNameEn
                    }),
                }).SingleOrDefault();
                if(newsletter == null)
                {
                    throw new Exception("Cannot find newsletter");
                }
                return Request.CreateResponse(HttpStatusCode.OK, newsletter);
            }
            catch(Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }

        [Route("api/Newsletters/{newsletterId}")]
        [HttpPut]
		[ClaimsAuthorize(Permission = new string[] { "21" })]
		public HttpResponseMessage SaveChangeNewsletter([FromUri]int newsletterId, NewsletterRequest request)
        {
            try
            {

                if (newsletterId == 0 || request == null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, "Invalid request");
                }
                var newsLetter = db.Newsletters.Where(w => w.NewsletterId == newsletterId).Include(i=>i.NewsletterShopMaps).SingleOrDefault();
                if(newsLetter == null)
                {
                    throw new Exception("Cannot find Newsletter");
                }
				string email = User.UserRequest().Email;
				var currentDt = SystemHelper.GetCurrentDateTime();
				SetupnewsLetter(newsLetter,request,email,currentDt);
                Util.DeadlockRetry(db.SaveChanges, "Newsletter");
                return GetNewsletter(newsLetter.NewsletterId);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }

        [Route("api/Newsletters")]
        [HttpPost]
		[ClaimsAuthorize(Permission = new string[] { "21" })]
		public HttpResponseMessage AddNewsletter(NewsletterRequest request)
        {
            try
            {
                Newsletter newsLetter = new Newsletter();
                string email = User.UserRequest().Email;
				var currentDt = SystemHelper.GetCurrentDateTime();
                SetupnewsLetter(newsLetter, request,email, currentDt);
                newsLetter.CreateBy = email;
                newsLetter.CreateOn = currentDt;
                newsLetter.NewsletterId = db.GetNextNewsletterId().SingleOrDefault().Value;
                db.Newsletters.Add(newsLetter);
                Util.DeadlockRetry(db.SaveChanges, "Newsletter");
                return GetNewsletter(newsLetter.NewsletterId); 
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }

        [Route("api/Newsletters")]
        [HttpDelete]
		[ClaimsAuthorize(Permission = new string[] { "21" })]
		public HttpResponseMessage DeleteNewsletter(List<NewsletterRequest> request)
        {
            try
            {
                if(request == null)
                {
                    throw new Exception("Invalid request");
                }
                var ids = request.Select(s => s.NewsletterId);
                db.Newsletters.RemoveRange(db.Newsletters.Where(w => ids.Contains(w.NewsletterId)));
                Util.DeadlockRetry(db.SaveChanges, "Newsletter");
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }

        }

        private void SetupnewsLetter(Newsletter newsLetter, NewsletterRequest request, string email,DateTime currentDt)
        {
            newsLetter.Subject = request.Subject;
            newsLetter.Description = request.Description;
            newsLetter.VisibleShopGroup = request.VisibleShopGroup;
            newsLetter.PublishedDt = request.PublishedDt;
            newsLetter.ExpiredDt = request.ExpiredDt;
            if (request.Image != null)
            {
                newsLetter.ImageUrl = request.Image.Url;
            }
            else
            {
                newsLetter.ImageUrl = string.Empty;
            }
            newsLetter.Status = Constant.STATUS_ACTIVE;
            newsLetter.UpdateBy = email;
            newsLetter.UpdateOn = currentDt;
            var shopMap = newsLetter.NewsletterShopMaps.ToList();
            #region Include shop
            var includeShop = shopMap.Where(w => w.Filter.Equals(Constant.NEWSLETTER_FILTER_INCLUDE)).ToList();
            if (request.IncludeShop != null && request.IncludeShop.Count > 0)
            {
                foreach (var shop in request.IncludeShop)
                {
                    if (shop.ShopId == 0) { continue; }
                    bool isNew = false;
                    if(includeShop == null || includeShop.Count == 0)
                    {
                        isNew = true;
                    }
                    if (!isNew)
                    {
                        var current = includeShop.Where(w => w.ShopId == shop.ShopId).SingleOrDefault();
                        if(current != null)
                        {
                            includeShop.Remove(current);
                        }
                        else
                        {
                            isNew = true;
                        }
                    }
                    if (isNew)
                    {
                        newsLetter.NewsletterShopMaps.Add(new NewsletterShopMap()
                        {
                            ShopId = shop.ShopId,
                            Filter = Constant.NEWSLETTER_FILTER_INCLUDE,
                            CreateBy = email,
                            CreateOn = currentDt,
                            UpdateBy = email,
                            UpdateOn = currentDt
						});
                    }
                }
            }
            if(includeShop != null && includeShop.Count > 0)
            {
                db.NewsletterShopMaps.RemoveRange(includeShop);
            }
            #endregion
            #region Exclude Shop
            var excludeShop = shopMap.Where(w => w.Filter.Equals(Constant.NEWSLETTER_FILTER_EXCLUDE)).ToList();
            if (request.ExcludeShop != null && request.ExcludeShop.Count > 0)
            {
                foreach (var shop in request.ExcludeShop)
                {
                    if (shop.ShopId == 0) { continue; }
                    bool isNew = false;
                    if (excludeShop == null || excludeShop.Count == 0)
                    {
                        isNew = true;
                    }
                    if (!isNew)
                    {
                        var current = excludeShop.Where(w => w.ShopId == shop.ShopId).SingleOrDefault();
                        if (current != null)
                        {
                            excludeShop.Remove(current);
                        }
                        else
                        {
                            isNew = true;
                        }
                    }
                    if (isNew)
                    {
                        newsLetter.NewsletterShopMaps.Add(new NewsletterShopMap()
                        {
                            ShopId = shop.ShopId,
                            Filter = Constant.NEWSLETTER_FILTER_EXCLUDE,
                            CreateBy = email,
                            CreateOn = currentDt,
                            UpdateBy = email,
                            UpdateOn = currentDt
						});
                    }
                }
            }
            if (excludeShop != null && excludeShop.Count > 0)
            {
                db.NewsletterShopMaps.RemoveRange(excludeShop);
            }
            #endregion
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