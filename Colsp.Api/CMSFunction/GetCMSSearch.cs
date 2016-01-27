using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Colsp.Api.CMSFunction
{
    public class GetCMSSearch
    {
        //Abstract
        public void GetBeforeAddItemByGobal(int UserId, string SearchType, string SearchText)
        {
        }
        public void GetBeforeAddItemByShop(int ShopId, int UserId, string SearchType, string SearchText)
        {
        }
        public void GetItemAfterAddItemByCategory(int ShopId, int CMSId, int CategoryId)
        {
        }
        public void GetItemAfterAddItemByBrand(int ShopId, int CMSId, int BrandId)
        {
        }
        /// <summary>
        /// All Item
        /// </summary>
        /// <param name="ShopId"></param>
        /// <param name="CMSId"></param>
        public void GetItemAfterAddItemByShop(int ShopId, int CMSId)
        {
        }

        public void GetCategoryListAfterAddItemByGobal(int UserId, int CMSId) { }
        public void GetCategoryListAfterAddItemByShop(int ShopId, int CMSId) { }
        public void GetBrandListAfterAddItemByGobal(int UserId, int CMSId) { }
        public void GetBrandListAfterAddItemByShop(int ShopId, int CMSId) { }
    }
}