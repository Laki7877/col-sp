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
            category.Visibility = true;

            // Products
            CMSCategoryProductMapRequest product1 = new CMSCategoryProductMapRequest();
            product1.Status = null;
            product1.ProductBoxBadge = "Lipaholic #Lax 03";
            product1.Pid = "3";
            product1.Sequence = 1;
            product1.ShopId = 0;

            CMSCategoryProductMapRequest product2 = new CMSCategoryProductMapRequest();
            product2.Status = null;
            product2.ProductBoxBadge = "Professional Eye Palette";
            product2.Pid = "4";
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
            category.Visibility = true;

            // Products
            CMSCategoryProductMapRequest product1 = new CMSCategoryProductMapRequest();
            product1.Status = null;
            product1.ProductBoxBadge = "Lipaholic #Lax 03";
            product1.Pid = "3";
            product1.Sequence = 1;
            product1.ShopId = 0;

            CMSCategoryProductMapRequest product2 = new CMSCategoryProductMapRequest();
            product2.Status = null;
            product2.ProductBoxBadge = "Professional Eye Palette";
            product2.Pid = "4";
            product2.Sequence = 2;
            product2.ShopId = 0;

            // Add Product to Category
            category.CategoryProductList.Add(product1);
            category.CategoryProductList.Add(product2);

            CMSLogic cms = new CMSLogic();
            Assert.IsTrue(cms.EditCMSCategory(category));

        }

        [TestMethod]
        public void AddCMSGroup_Test()
        {
            // Group
            CMSGroupRequest group   = new CMSGroupRequest();
            group.CMSGroupNameEN    = "Test Group1";
            group.CMSGroupNameTH    = "Test Group1";
            //group.Status            = "";
            group.Visibility        = true;

            // Group Master
            CMSMasterGroupMapRequest master1 = new CMSMasterGroupMapRequest();
            master1.Status              = "AT";
            master1.CMSGroupId          = 1;
            master1.CMSMasterGroupMapId = 1;
            master1.Sequence            = 1;
            master1.CMSMasterId         = 1;
            
            // Add Master to Group
            group.GroupMasterList.Add(master1);

            CMSLogic cms = new CMSLogic();
            Assert.IsTrue(cms.AddCMSGroup(group));

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
