using Colsp.Entity.Models;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Colsp.Api.Controllers
{
    public class DefaultController : ApiController
    {
        //[Route("api/Default")]
        //[HttpGet]
        //public HttpResponseMessage GetTest()
        //{

        //    ProductStageVariant var = new ProductStageVariant();
        //    var.ProductId = 1;
        //    var.ProductNameEn = "Test";
        //    var.Pid = "TEST";
        //    var.ShopId = 1;
        //    for(int i = 1; i < 5; i++)
        //    {
        //        var.ProductStageVariantArrtibuteMaps.Add(new ProductStageVariantArrtibuteMap()
        //        {
        //            VariantId = var.VariantId,
        //            AttributeId = 1,
        //            Value = ""+ i 
        //        });
        //    }

        //    using (ColspEntities db = new ColspEntities())
        //    {
        //        db.ProductStageVariants.Add(var);
        //        db.SaveChanges();
        //    }

        //    return Request.CreateErrorResponse(HttpStatusCode.NotFound, "false");
        //}
    }
}
