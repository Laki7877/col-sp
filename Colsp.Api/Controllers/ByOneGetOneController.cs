using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Colsp.Entity.Models;
using Colsp.Model.Responses;
using Colsp.Model.Requests;
using Colsp.Api.Extensions;
using Colsp.Api.Constants;
using Colsp.Api.Helper;
using System.Data.Entity;
using Colsp.Api.CMSFunction;
using Colsp.Api.ByOneGetOneFunction;

namespace Colsp.Api.Controllers
{
    public class ByOneGetOneController : ApiController
    {


        #region Create By 1 Get 1
        [Route("api/CMSByGet")]
        [HttpPost]
        public HttpResponseMessage By1Get1Item(By1Get1ItemRequest model)
        {
            try
            {
                if (model != null)
                {
                    int Id = 0;

                    ByOneGetOneProcess bg = new ByOneGetOneProcess();
                    Id = bg.CreateBy1GetItem(model);
                    return FindBy1Get1Item(Id);
                }
                else
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, "Cannot save data");
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, ex.Message);
            }
        }
        #endregion

        #region Find By 1 Get 1
        public HttpResponseMessage FindBy1Get1Item(int? Id)
        {
            try
            {
                By1Get1ItemResponse response = new By1Get1ItemResponse();
                if (Id != null && Id.HasValue)
                {
                    if (Id == 0)
                        return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, "Item id is invalid. Cannot find item in System");
                    using (ColspEntities db = new ColspEntities())
                    {
                        var B1G = db.PromotionBy1Get1Item.Where(c => c.PromotionBy1Get1ItemId == Id).FirstOrDefault();
                        if (B1G != null)
                        {
                            response.PromotionBy1Get1ItemId = B1G.PromotionBy1Get1ItemId;
                            response.CreateBy = B1G.CreateBy;
                            response.NameEN = B1G.NameEN;
                            response.NameTH = B1G.NameTH;
                            response.EffectiveDate = B1G.EffectiveDate;
                            response.EffectiveTime = B1G.EffectiveTime;
                            response.ExpiryDate = B1G.ExpiryDate;
                            response.ExpiryTime = B1G.ExpiryTime;
                            response.CreateIP = B1G.CreateIP;
                            response.ShortDescriptionEN = B1G.ShortDescriptionEN;
                            response.ShortDescriptionTH = B1G.ShortDescriptionTH;
                            response.ShopId = B1G.ShopId;
                            response.ShortDetailEN = B1G.ShortDetailEN;
                            response.ShortDetailTH = B1G.ShortDetailTH;
                            response.ByPID = B1G.ByPID;
                            response.GetPID = B1G.GetPID;
                            response.URLKey = B1G.URLKey;
                            response.Visibility = B1G.Visibility;
                            response.Createdate = (DateTime)B1G.Createdate;
                            return Request.CreateResponse(HttpStatusCode.OK, response);
                        }
                        else
                        {
                            return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, "Cannot find item in System");
                        }
                    }
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, HttpErrorMessage.NotFound);
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }
        #endregion
    }
}