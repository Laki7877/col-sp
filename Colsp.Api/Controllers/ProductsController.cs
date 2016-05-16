using System.Web.Http;
using Colsp.Entity.Models;
using System.Net.Http;
using Colsp.Model.Requests;
using System;
using System.Net;
using Colsp.Api.Helpers;
using Colsp.Api.Constants;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Colsp.Api.Extensions;

namespace Colsp.Api.Controllers
{
    public class ProductsController : ApiController
    {
        private ColspEntities db = new ColspEntities();

        

        private void SetupProductStageAttribute(ProductStageAttribute attribute, AttributeRequest request, List<AttributeRequest> attributeList)
        {
            //attribute.AttributeId = request.AttributeId;
            //attribute.ValueEn = request.ValueEn;
            //attribute.ValueTh = request.ValueTh;
            //if(request.AttributeValues != null &&  && request.AttributeValues[0])
            //attribute.AttributeValueId = request.;
            //attribute.CheckboxValue = request.;
            //attribute.Position = request.;
            //attribute.IsAttributeValue = request.;
            //attribute.CreateBy = request.;
            //attribute.CreateOn = request.;
            //attribute.UpdateBy = request.;
            //attribute.UpdateOn = request.;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }





        //private bool ProductExists(int id)
        //{
        //    return db.Products.Count(e => e.ProductId == id) > 0;
        //}
    }
}