using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Colsp.Entity.Models;
using Colsp.Logic;
using Colsp.Model;
using Colsp.Model.Requests;

namespace Colsp.ApiTests.BusinessLogic
{
    [TestClass]
    public class CMSLogic_Test
    {
        [TestMethod]
        public void AddCMSCategory_Test()
        {
            // Category
            CMSCategoryRequest category = new CMSCategoryRequest();
            category.CMSCategoryNameEN = "cosmetics";
            category.CMSCategoryNameTH = "เครื่องสำอาง";
            category.IsActive = true;

            // Products
            CMSCategoryProductMapRequest product1 = new CMSCategoryProductMapRequest();
            product1.IsActive = true;
            product1.ProductBoxBadge = "Lipaholic #Lax 03";
            product1.ProductPID = "3";
            product1.Sequence = 1;
            product1.ShopId = 0;

            CMSCategoryProductMapRequest product2 = new CMSCategoryProductMapRequest();
            product2.IsActive = true;
            product2.ProductBoxBadge = "Professional Eye Palette";
            product2.ProductPID = "4";
            product2.Sequence = 2;
            product2.ShopId = 0;

            // Add Product to Category
            category.CategoryProductList.Add(product1);
            category.CategoryProductList.Add(product2);

            CMSLogic cms = new CMSLogic();
            Assert.IsTrue(cms.AddCMSCategory(category));

        }

        [TestMethod]
        public void EditCMSCategory_Test()
        {
            // Category
            CMSCategoryRequest category = new CMSCategoryRequest();
            category.CMSCategoryId = 2;
            category.CMSCategoryNameEN = "cosmetics(edit)";
            category.CMSCategoryNameTH = "เครื่องสำอาง(edit)";
            category.IsActive = true;

            // Products
            CMSCategoryProductMapRequest product1 = new CMSCategoryProductMapRequest();
            product1.IsActive = true;
            product1.ProductBoxBadge = "Lipaholic #Lax 03";
            product1.ProductPID = "3";
            product1.Sequence = 1;
            product1.ShopId = 0;

            CMSCategoryProductMapRequest product2 = new CMSCategoryProductMapRequest();
            product2.IsActive = true;
            product2.ProductBoxBadge = "Professional Eye Palette";
            product2.ProductPID = "4";
            product2.Sequence = 2;
            product2.ShopId = 0;

            // Add Product to Category
            category.CategoryProductList.Add(product1);
            category.CategoryProductList.Add(product2);

            CMSLogic cms = new CMSLogic();
            Assert.IsTrue(cms.EditCMSCategory(category));

        }

        [TestMethod]
        public void GetAllCMSCategory_Test()
        {
            //PaginatedRequest request = new PaginatedRequest();
            //CMSLogic cms = new CMSLogic();
            //var items = cms.GetAllCMSCategory(request);

            //Assert.IsNotNull(items);
        }

        [TestMethod]
        public void GetBrand_Test()
        {
            CMSLogic cms = new CMSLogic();
            var items = cms.GetBrand(new BrandCondition() { BrandNameEn = "Nike" });

            Assert.IsNotNull(items);
        }
    }
}
