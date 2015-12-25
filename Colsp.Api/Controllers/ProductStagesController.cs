using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using Colsp.Entity.Models;
using Colsp.Model.Responses;
using Colsp.Api.Extensions;
using Colsp.Model.Requests;

namespace Colsp.Api.Controllers
{
    public class ProductStagesController : ApiController
    {
        private ColspEntities db = new ColspEntities();

		// GET: api/ProductStages
		[ResponseType(typeof(PaginatedResponse))]
		public IHttpActionResult GetProductStages([FromUri] ProductRequest request)
		{
			request.DefaultOnNull();
			IQueryable<ProductStage> products = null;
			
			// List all product
			products = db.ProductStages.Where(p => true);
			if (request.Sku != null)
			{
				products = products.Where(p => p.Sku.Contains(request.Sku));
			}
			if (request.SellerId != null)
			{
				products = products.Where(p => p.SellerId.Equals(request.SellerId));
			}

			var total = products.Count();
			var pagedProducts = products.Paginate(request)
										.Select(p => new
										{
											p.ProductId,
											p.ProductNameEn,
											p.ProductNameTh,
											p.OriginalPrice,
											p.SalePrice,
											p.Status,
											p.ImageFlag,
											p.InfoFlag,
											Modified = p.UpdatedDt
										});
			var response = PaginatedResponse.CreateResponse(pagedProducts, request, total);
			return Ok(response);
		}

		// GET: api/ProductStages/5
		[ResponseType(typeof(ProductStage))]
        public IHttpActionResult GetProductStage(int id)
        {
            ProductStage productStage = db.ProductStages.Find(id);
            if (productStage == null)
            {
                return NotFound();
            }

            return Ok(productStage);
        }

        // PUT: api/ProductStages/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PutProductStage(int id, ProductStage productStage)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != productStage.ProductId)
            {
                return BadRequest();
            }

            db.Entry(productStage).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductStageExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST: api/ProductStages
        [ResponseType(typeof(ProductStage))]
        public IHttpActionResult PostProductStage(ProductStage productStage)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.ProductStages.Add(productStage);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = productStage.ProductId }, productStage);
        }

        // DELETE: api/ProductStages/5
        [ResponseType(typeof(ProductStage))]
        public IHttpActionResult DeleteProductStage(int id)
        {
            ProductStage productStage = db.ProductStages.Find(id);
            if (productStage == null)
            {
                return NotFound();
            }

            db.ProductStages.Remove(productStage);
            db.SaveChanges();

            return Ok(productStage);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool ProductStageExists(int id)
        {
            return db.ProductStages.Count(e => e.ProductId == id) > 0;
        }
    }
}