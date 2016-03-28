
using Colsp.Entity.Models;
using Colsp.Model;
using Colsp.Model.Requests;
using Colsp.Model.Responses;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System;
using System.Data.Entity;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colsp.Logic
{
    public class CMSLogic
    {

        #region Start CMS Category Method

        // Get All CMS Category
        //public PaginatedResponse GetAllCMSCategory(PaginatedRequest request)
        //{
        //    try
        //    {
        //        using (ColspEntities db = new ColspEntities())
        //        {
        //            var query = from cate in db.CMSCategories select cate;
                    
        //            var total = query.Count();
        //            var response = PaginatedResponse.CreateResponse(query.Paginate(request), request, total);

        //            return response;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception(ex.Message + " /Logic/GetAllCMSCategory");
        //    }
            
        //}

        // Add CMS Category

        // Get Brand

        public List<Brand> GetBrand(BrandCondition condition)
        {
            try
            {
                using (ColspEntities db = new ColspEntities())
                {
                    var query = from brand in db.Brands select brand;

                    if (condition.BrandId != null)
                        query = query.Where(x => x.BrandId == condition.BrandId);
                    
                    if (condition.BrandNameEn != null)
                        query = query.Where(x => x.BrandNameEn.Equals(condition.BrandNameEn));

                    if (condition.BrandNameTh != null)
                        query = query.Where(x => x.BrandNameTh.Equals(condition.BrandNameTh));
                    
                    if (!query.Any())
                        return null;

                    return query.ToList();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + " /Logic/GetBrand");
            }
        }

        // Add CMS Category
        public bool AddCMSCategory(CMSCategoryRequest request)
        {
            bool success = false;

            try
            {
                using (ColspEntities db = new ColspEntities())
                {
                    int row             = -1;
                    DateTime dateNow    = DateTime.Now;

                    CMSCategory cmsCategory         = new CMSCategory();
                    cmsCategory.CMSCategoryNameEN   = request.CMSCategoryNameEN;
                    cmsCategory.CMSCategoryNameTH   = request.CMSCategoryNameTH;
                    cmsCategory.IsActive            = request.IsActive;
                    cmsCategory.CreateBy            = request.CreateBy;
                    cmsCategory.CreateDate          = dateNow;
                    cmsCategory.CreateIP            = request.CreateIP;
                    db.CMSCategories.Add(cmsCategory);
                    db.SaveChanges();

                    int? cmsCategoryId = CMSHelper.GetCMSCategoryId(db, cmsCategory);

                    if (cmsCategory != null) {
                     
                        foreach (var product in request.CategoryProductList)
                        {
                            CMSCategoryProductMap cmsCategoryProduct    = new CMSCategoryProductMap();
                            cmsCategoryProduct.CMSCategoryId            = cmsCategoryId.Value;
                            cmsCategoryProduct.CMSCategoryProductMapId  = product.CMSCategoryProductMapId;
                            cmsCategoryProduct.IsActive                 = product.IsActive;
                            cmsCategoryProduct.ProductBoxBadge          = product.ProductBoxBadge;
                            cmsCategoryProduct.ProductPID               = product.ProductPID;
                            cmsCategoryProduct.Sequence                 = product.Sequence;
                            cmsCategoryProduct.CreateBy                 = product.CreateBy;
                            cmsCategoryProduct.CreateDate               = dateNow;
                            cmsCategoryProduct.CreateIP                 = product.CreateIP;

                            db.CMSCategoryProductMaps.Add(cmsCategoryProduct);
                        }
                    }
                    
                    row     = db.SaveChanges();
                    success = row > -1;
            }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + " /Logic/AddCMSCategory");
            }

            return success;
        }

        // Edit CMS Category
        public bool EditCMSCategory(CMSCategoryRequest request)
        {
            bool success = false;

            try
            {
                using (ColspEntities db = new ColspEntities())
                {
                    int row = -1;
                    DateTime dateNow = DateTime.Now;

                    var queryCMSCategory = db.CMSCategories.Where(x => x.CMSCategoryId == request.CMSCategoryId);

                    if (!queryCMSCategory.Any())
                        return false;

                    var cmsCategory = queryCMSCategory.First();
                    cmsCategory.CMSCategoryNameEN = request.CMSCategoryNameEN;
                    cmsCategory.CMSCategoryNameTH = request.CMSCategoryNameTH;
                    cmsCategory.IsActive = request.IsActive;
                    cmsCategory.UpdateBy = request.UpdateBy;
                    cmsCategory.UpdateDate = dateNow;
                    cmsCategory.UpdateIP = request.UpdateIP;

                    // Remove Category Product
                    var queryCMSCategoryProducts = db.CMSCategoryProductMaps.Where(x => x.CMSCategoryId == cmsCategory.CMSCategoryId);
                    if (queryCMSCategoryProducts.Any())
                    {
                        foreach (var cmsCategoryProduct in queryCMSCategoryProducts)
                        {
                            db.CMSCategoryProductMaps.Remove(cmsCategoryProduct);
                        }
            }
                    
                    if (cmsCategory != null)
                    {

                        foreach (var product in request.CategoryProductList)
                        {
                            CMSCategoryProductMap cmsCategoryProduct = new CMSCategoryProductMap();
                            cmsCategoryProduct.CMSCategoryId = cmsCategory.CMSCategoryId;
                            cmsCategoryProduct.CMSCategoryProductMapId = product.CMSCategoryProductMapId;
                            cmsCategoryProduct.IsActive = product.IsActive;
                            cmsCategoryProduct.ProductBoxBadge = product.ProductBoxBadge;
                            cmsCategoryProduct.ProductPID = product.ProductPID;
                            cmsCategoryProduct.Sequence = product.Sequence;
                            cmsCategoryProduct.CreateBy = product.CreateBy;
                            cmsCategoryProduct.CreateDate = dateNow;
                            cmsCategoryProduct.CreateIP = product.CreateIP;

                            db.CMSCategoryProductMaps.Add(cmsCategoryProduct);
                        }
                    }

                    row = db.SaveChanges();
                    success = row > -1;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + " /Logic/EditCMSCategory");
            }

            return success;
        }

        // Delete CMS Category
        public bool DeleteCMSCategory(List<CMSCategoryRequest> request)
        {
            bool success = false;

            try
            {
                using (ColspEntities db = new ColspEntities())
                {
                    foreach (var cmsCategory in request)
                    {
                        var query = db.CMSCategories.Where(x => x.CMSCategoryId == cmsCategory.CMSCategoryId);
                        if (query.Any())
                            query.First().IsActive = false;
                    }

                    db.SaveChanges();
                }

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + " /Logic/DeleteCMSCategory");
            }

            return success;
        }
        #endregion


        #region Start CMS Master Method

        #endregion

        #region Start CMS Group Method

        #endregion

        #region Get All CMS Master

        #endregion
    }
}
