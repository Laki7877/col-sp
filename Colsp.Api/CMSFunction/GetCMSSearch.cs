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


namespace Colsp.Api.CMSFunction
{
    public class GetCMSSearch
    {
        //Abstract
        //public GlobalCategory GetCategoryBeforeAddItemByGobal(CMSSearchForAddRequest model)
        //{
        //    try
        //    {
        //        using (ColspEntities db = new ColspEntities())
        //        {
        //            return db.GlobalCategories.Where(c => (c.NameEn.Contains(model.SearchText) || c.NameTh.Contains(model.SearchText)));
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw;
        //    }
        //}


        public void GetBeforeAddItemByShop(int ShopId, int UserId, string SearchType, string SearchText)
        {
        }
        public void GetItemAfterAddItemByCategory(int ShopId, int CMSId, int CategoryId)
        {
            try
            {
                using (ColspEntities db = new ColspEntities())
                {

                }
            }
            catch (Exception ex)
            {

                throw;
            }
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