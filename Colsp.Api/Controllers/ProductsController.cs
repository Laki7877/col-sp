using System.Linq;
using System.Web.Http;
using Colsp.Entity.Models;
namespace Colsp.Api.Controllers
{
    public class ProductsController : ApiController
    {
        private ColspEntities db = new ColspEntities();

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