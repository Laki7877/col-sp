using System;
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
                var newsLetter = db.Newsletters.Where(w => true).Select(s=>s);
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
                FileUploadRespond fileUpload = await Util.SetupImage(Request, AppSettingKey.IMAGE_ROOT_PATH, AppSettingKey.NEWSLETTER_FOLDER,1500, 1500, 2000, 2000, 5, true);
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
                var newsletter = db.Newsletters.Where(w => w.NewsletterId == newsletterId).SingleOrDefault();
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
        public HttpResponseMessage SaveChangeNewsletter([FromUri]int newsletterId, NewsletterRequest request)
        {
            try
            {
                if (newsletterId == 0 || request == null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, "Invalid request");
                }
                var newsLetter = db.Newsletters.Where(w => w.NewsletterId == newsletterId).SingleOrDefault();
                if(newsLetter == null)
                {
                    throw new Exception("Cannot find Newsletter");
                }
                SetupnewsLetter(newsLetter,request);
                newsLetter.Status = Constant.STATUS_ACTIVE;
                newsLetter.UpdatedBy = this.User.UserRequest().Email;
                newsLetter.UpdatedDt = DateTime.Now;
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
        public HttpResponseMessage AddNewsletter(NewsletterRequest request)
        {
            try
            {
                Newsletter newsLetter = new Newsletter();
                SetupnewsLetter(newsLetter, request);
                newsLetter.Status = Constant.STATUS_ACTIVE;
                newsLetter.CreatedBy = this.User.UserRequest().Email;
                newsLetter.CreatedDt = DateTime.Now;
                newsLetter.UpdatedBy = this.User.UserRequest().Email;
                newsLetter.UpdatedDt = DateTime.Now;
                db.Newsletters.Add(newsLetter);
                Util.DeadlockRetry(db.SaveChanges, "Newsletter");
                return GetNewsletter(newsLetter.NewsletterId); 
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }

        private void SetupnewsLetter(Newsletter newsLetter, NewsletterRequest request)
        {
            newsLetter.Subject = request.Subject;
            newsLetter.Description = request.Description;
            newsLetter.VisbleShopGroup = request.VisbleShopGroup;
            if (request.Image != null)
            {
                newsLetter.ImageUrl = request.Image.url;
            }
            else
            {
                newsLetter.ImageUrl = string.Empty;
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