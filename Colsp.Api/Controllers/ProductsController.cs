using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.Description;
using Colsp.Entity.Models;
using Colsp.Model.Requests;
using Colsp.Model.Responses;
using Colsp.Api.Filters;
using Colsp.Api.Extensions;
namespace Colsp.Api.Controllers
{
	public class ProductsController : ApiController
    {
        private ColspEntities db = new ColspEntities();

        // GET: api/Products
		[ClaimsAuthorize(Permission = "ListProduct, ListOwnedProduct")]
		[ResponseType(typeof(PaginatedResponse))]
        public IHttpActionResult GetProducts([FromUri] ProductRequest request)
        {
			request.DefaultOnNull();
			IQueryable<Product> products = null;
			
			if (User.HasPermission("ListProduct"))
			{
				// List all product
				products = db.Products.Where( p => true);
				if(request.Sku != null)
				{
					products = products.Where(p => p.Sku.Equals(request.Sku));
				}
				if(request.SellerId != null)
				{
					products = products.Where(p => p.SellerId.Equals(request.SellerId));
				}
			}
			else if (User.HasPermission("ListOwnedProduct"))
			{
				// List only owned product
				products = db.Products.Where(p => p.SellerId.Equals(User.UserId()));
				if(request.Sku != null)
				{
					products = products.Where(p => p.Sku.Equals(request.Sku));
				}
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
											Modified = p.UpdatedDt
										});
			var response = PaginatedResponse.CreateResponse(pagedProducts, request, total);
			return Ok(response);
        }

        // GET: api/Products/5
		[ClaimsAuthorize(Permission = "GetProduct, GetOwnedProduct")]
        [ResponseType(typeof(Product))]
        public IHttpActionResult GetProduct(int id)
        {
			Product product = null;

			if (User.HasPermission("GetProduct")) {
				product = db.Products.Find(id);
			}
			else if (User.HasPermission("GetOwnedProduct"))
			{
				product = db.Products.Where(p => p.SellerId.Equals(User.UserId()) && p.ProductId.Equals(id)).FirstOrDefault();
			}
			if (product == null)
            {
                return NotFound();
            }
			
            return Ok(product);
        }

		// PUT: api/Products/5
		[ClaimsAuthorize(Permission = "UpdateProduct")]
		[ResponseType(typeof(void))]
        public IHttpActionResult PutProduct(int id, Product product)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != product.ProductId)
            {
                return BadRequest();
            }

            db.Entry(product).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(id))
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

		// POST: api/Products
		[ClaimsAuthorize(Permission = "AddProduct")]
		[ResponseType(typeof(Product))]
        public IHttpActionResult PostProduct(Product product)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.Products.Add(product);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = product.ProductId }, product);
        }

		// DELETE: api/Products/5
		[ClaimsAuthorize(Permission = "DeleteProduct")]
		[ResponseType(typeof(Product))]
        public IHttpActionResult DeleteProduct(int id)
        {
            Product product = db.Products.Find(id);
            if (product == null)
            {
                return NotFound();
            }

            db.Products.Remove(product);
            db.SaveChanges();

            return Ok(product);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool ProductExists(int id)
        {
            return db.Products.Count(e => e.ProductId == id) > 0;
        }
    }
}