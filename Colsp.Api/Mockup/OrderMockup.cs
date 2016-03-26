
using Colsp.Model.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Colsp.Api.Constants;

namespace Colsp.Api.Mockup
{
    public static class OrderMockup
    {
        public static PurchaseOrderReuest O1 = new PurchaseOrderReuest()
        {
            OrderId = "OI0001",
            TrackingNumber = "TR0001",
            ShopId = 3,
            CustomerName = "Laki Sik",
            OrderDate = new DateTime(2016,3,24,1,10,0),
            ShippingType = "Kerry",
            Status = Constant.ORDER_PAYMENT_PENDING,
            BillAddress1 = "This is billing address 1",
            BillAddress2 = "This is billing address 2",
            BillAddress3 = "This is billing address 3",
            BillAddress4 = "This is billing address 4",
            ShipContactor = "Ship Contactor",
            ShipAddress1 = "This is shipping address 1",
            ShipAddress2 = "This is shipping address 2",
            ShipAddress3 = "This is shipping address 3",
            ShipAddress4 = "This is shipping address 4",
            InvoiceAddress1 = "This is invoice address 1",
            InvoiceAddress2 = "This is invoice address 2",
            InvoiceAddress3 = "This is invoice address 3",
            InvoiceAddress4 = "This is invoice address 4",
            ShipAreaCode = "Area Code",
            ShipCity = "This is City",
            ShipDistrict = "This is District",
            ShipProvince = "This is Province",
            OrdDiscAmt = 0,
            TotalAmt = 1000,
            GrandTotalAmt = 1000,
            Products = new List<PurchaseOrderDetailReuest>()
            {
                new PurchaseOrderDetailReuest()
                {
                    ProductId = 57,
                    Pid = "1111188",
                    ProductNameEn = "ALIVE 5",
                    Quantity = 2,
                    ShipQuantity = 2,
                    UnitPrice = 250
                },
                new PurchaseOrderDetailReuest()
                {
                    ProductId = 68,
                    Pid = "1111189",
                    ProductNameEn = "Test World",
                    Quantity = 1,
                    ShipQuantity = 1,
                    UnitPrice = 500
                }
            }
        };

        public static PurchaseOrderReuest O1_1 = new PurchaseOrderReuest() { OrderId = "OI0001", TrackingNumber = "TR0001", ShopId = 3, CustomerName = "Laki Sik", OrderDate = new DateTime(2016, 3, 24, 2, 10, 0), ShippingType = "Kerry", Status = Constant.ORDER_PAYMENT_PENDING, BillAddress1 = "This is billing address 1", BillAddress2 = "This is billing address 2", BillAddress3 = "This is billing address 3", BillAddress4 = "This is billing address 4", ShipContactor = "Ship Contactor", ShipAddress1 = "This is shipping address 1", ShipAddress2 = "This is shipping address 2", ShipAddress3 = "This is shipping address 3", ShipAddress4 = "This is shipping address 4", InvoiceAddress1 = "This is invoice address 1", InvoiceAddress2 = "This is invoice address 2", InvoiceAddress3 = "This is invoice address 3", InvoiceAddress4 = "This is invoice address 4", ShipAreaCode = "Area Code", ShipCity = "This is City", ShipDistrict = "This is District", ShipProvince = "This is Province", OrdDiscAmt = 0, TotalAmt = 1000, GrandTotalAmt = 1000, Products = new List<PurchaseOrderDetailReuest>() { new PurchaseOrderDetailReuest() { ProductId = 57, Pid = "1111188", ProductNameEn = "ALIVE 5", Quantity = 2, ShipQuantity = 2, UnitPrice = 250 }, new PurchaseOrderDetailReuest() { ProductId = 68, Pid = "1111189", ProductNameEn = "Test World", Quantity = 1, UnitPrice = 500 } } };
        public static PurchaseOrderReuest O1_2 = new PurchaseOrderReuest() { OrderId = "OI0001", TrackingNumber = "TR0001", ShopId = 3, CustomerName = "Laki Sik", OrderDate = new DateTime(2016, 3, 24, 3, 10, 0), ShippingType = "Kerry", Status = Constant.ORDER_PAYMENT_PENDING, BillAddress1 = "This is billing address 1", BillAddress2 = "This is billing address 2", BillAddress3 = "This is billing address 3", BillAddress4 = "This is billing address 4", ShipContactor = "Ship Contactor", ShipAddress1 = "This is shipping address 1", ShipAddress2 = "This is shipping address 2", ShipAddress3 = "This is shipping address 3", ShipAddress4 = "This is shipping address 4", InvoiceAddress1 = "This is invoice address 1", InvoiceAddress2 = "This is invoice address 2", InvoiceAddress3 = "This is invoice address 3", InvoiceAddress4 = "This is invoice address 4", ShipAreaCode = "Area Code", ShipCity = "This is City", ShipDistrict = "This is District", ShipProvince = "This is Province", OrdDiscAmt = 0, TotalAmt = 1000, GrandTotalAmt = 1000, Products = new List<PurchaseOrderDetailReuest>() { new PurchaseOrderDetailReuest() { ProductId = 57, Pid = "1111188", ProductNameEn = "ALIVE 5", Quantity = 2, ShipQuantity = 2, UnitPrice = 250 }, new PurchaseOrderDetailReuest() { ProductId = 68, Pid = "1111189", ProductNameEn = "Test World", Quantity = 1, UnitPrice = 500 } } };
        public static PurchaseOrderReuest O1_3 = new PurchaseOrderReuest() { OrderId = "OI0001", TrackingNumber = "TR0001", ShopId = 3, CustomerName = "Laki Sik", OrderDate = new DateTime(2016, 3, 24, 5, 10, 0), ShippingType = "Kerry", Status = Constant.ORDER_PAYMENT_PENDING, BillAddress1 = "This is billing address 1", BillAddress2 = "This is billing address 2", BillAddress3 = "This is billing address 3", BillAddress4 = "This is billing address 4", ShipContactor = "Ship Contactor", ShipAddress1 = "This is shipping address 1", ShipAddress2 = "This is shipping address 2", ShipAddress3 = "This is shipping address 3", ShipAddress4 = "This is shipping address 4", InvoiceAddress1 = "This is invoice address 1", InvoiceAddress2 = "This is invoice address 2", InvoiceAddress3 = "This is invoice address 3", InvoiceAddress4 = "This is invoice address 4", ShipAreaCode = "Area Code", ShipCity = "This is City", ShipDistrict = "This is District", ShipProvince = "This is Province", OrdDiscAmt = 0, TotalAmt = 1000, GrandTotalAmt = 1000, Products = new List<PurchaseOrderDetailReuest>() { new PurchaseOrderDetailReuest() { ProductId = 57, Pid = "1111188", ProductNameEn = "ALIVE 5", Quantity = 2, ShipQuantity = 2, UnitPrice = 250 }, new PurchaseOrderDetailReuest() { ProductId = 68, Pid = "1111189", ProductNameEn = "Test World", Quantity = 1, UnitPrice = 500 } } };
        public static PurchaseOrderReuest O1_4 = new PurchaseOrderReuest() { OrderId = "OI0001", TrackingNumber = "TR0001", ShopId = 3, CustomerName = "Laki Sik", OrderDate = new DateTime(2016, 3, 24, 5, 10, 0), ShippingType = "Kerry", Status = Constant.ORDER_PAYMENT_PENDING, BillAddress1 = "This is billing address 1", BillAddress2 = "This is billing address 2", BillAddress3 = "This is billing address 3", BillAddress4 = "This is billing address 4", ShipContactor = "Ship Contactor", ShipAddress1 = "This is shipping address 1", ShipAddress2 = "This is shipping address 2", ShipAddress3 = "This is shipping address 3", ShipAddress4 = "This is shipping address 4", InvoiceAddress1 = "This is invoice address 1", InvoiceAddress2 = "This is invoice address 2", InvoiceAddress3 = "This is invoice address 3", InvoiceAddress4 = "This is invoice address 4", ShipAreaCode = "Area Code", ShipCity = "This is City", ShipDistrict = "This is District", ShipProvince = "This is Province", OrdDiscAmt = 0, TotalAmt = 1000, GrandTotalAmt = 1000, Products = new List<PurchaseOrderDetailReuest>() { new PurchaseOrderDetailReuest() { ProductId = 57, Pid = "1111188", ProductNameEn = "ALIVE 5", Quantity = 2, ShipQuantity = 2, UnitPrice = 250 }, new PurchaseOrderDetailReuest() { ProductId = 68, Pid = "1111189", ProductNameEn = "Test World", Quantity = 1, UnitPrice = 500 } } };

        public static PurchaseOrderReuest O2 = new PurchaseOrderReuest() { OrderId = "OI0002", TrackingNumber = "TR0002", ShopId = 3, CustomerName = "Poon",  OrderDate = DateTime.Now, ShippingType = "DHL",   Status = Constant.ORDER_PAYMENT_CONFIRM,  BillAddress1 = "This is billing address 1", BillAddress2 = "This is billing address 2", BillAddress3 = "This is billing address 3", BillAddress4 = "This is billing address 4", ShipContactor = "Ship Contactor", ShipAddress1 = "This is shipping address 1", ShipAddress2 = "This is shipping address 2", ShipAddress3 = "This is shipping address 3", ShipAddress4 = "This is shipping address 4", InvoiceAddress1 = "This is invoice address 1", InvoiceAddress2 = "This is invoice address 2", InvoiceAddress3 = "This is invoice address 3", InvoiceAddress4 = "This is invoice address 4", ShipAreaCode = "Area Code", ShipCity = "This is City", ShipDistrict = "This is District", ShipProvince = "This is Province", OrdDiscAmt = 0, TotalAmt = 1000, GrandTotalAmt = 1000, Products = new List<PurchaseOrderDetailReuest>() { new PurchaseOrderDetailReuest() { Pid = "1111188",ProductId = 57, ProductNameEn = "ALIVE 5", Quantity = 2, ShipQuantity = 2, UnitPrice = 250 }, new PurchaseOrderDetailReuest() { Pid = "111119J", ProductId = 68, ProductNameEn = "Test World", Quantity = 1, UnitPrice = 500 } } };
        public static PurchaseOrderReuest O3 = new PurchaseOrderReuest() { OrderId = "OI0003", TrackingNumber = "TR0003", ShopId = 3 , CustomerName = "Eart", OrderDate = DateTime.Now, ShippingType = "DHL",   Status = Constant.ORDER_PREPARING ,       BillAddress1 = "This is billing address 1", BillAddress2 = "This is billing address 2", BillAddress3 = "This is billing address 3", BillAddress4 = "This is billing address 4", ShipContactor = "Ship Contactor", ShipAddress1 = "This is shipping address 1", ShipAddress2 = "This is shipping address 2", ShipAddress3 = "This is shipping address 3", ShipAddress4 = "This is shipping address 4", InvoiceAddress1 = "This is invoice address 1", InvoiceAddress2 = "This is invoice address 2", InvoiceAddress3 = "This is invoice address 3", InvoiceAddress4 = "This is invoice address 4", ShipAreaCode = "Area Code", ShipCity = "This is City", ShipDistrict = "This is District", ShipProvince = "This is Province", OrdDiscAmt = 0, TotalAmt = 1000, GrandTotalAmt = 1000, Products = new List<PurchaseOrderDetailReuest>() { new PurchaseOrderDetailReuest() { Pid = "1111188",ProductId = 57, ProductNameEn = "ALIVE 5", Quantity = 2, ShipQuantity = 2, UnitPrice = 250 }, new PurchaseOrderDetailReuest() { Pid = "111119J", ProductId = 68, ProductNameEn = "Test World", Quantity = 1, UnitPrice = 500 } } };
        public static PurchaseOrderReuest O4 = new PurchaseOrderReuest() { OrderId = "OI0004", TrackingNumber = "TR0004", ShopId = 3 , CustomerName = "Nat",  OrderDate = DateTime.Now, ShippingType = "DHL",   Status = Constant.ORDER_READY_TO_SHIP ,   BillAddress1 = "This is billing address 1", BillAddress2 = "This is billing address 2", BillAddress3 = "This is billing address 3", BillAddress4 = "This is billing address 4", ShipContactor = "Ship Contactor", ShipAddress1 = "This is shipping address 1", ShipAddress2 = "This is shipping address 2", ShipAddress3 = "This is shipping address 3", ShipAddress4 = "This is shipping address 4", InvoiceAddress1 = "This is invoice address 1", InvoiceAddress2 = "This is invoice address 2", InvoiceAddress3 = "This is invoice address 3", InvoiceAddress4 = "This is invoice address 4", ShipAreaCode = "Area Code", ShipCity = "This is City", ShipDistrict = "This is District", ShipProvince = "This is Province", OrdDiscAmt = 0, TotalAmt = 1000, GrandTotalAmt = 1000, Products = new List<PurchaseOrderDetailReuest>() { new PurchaseOrderDetailReuest() { Pid = "1111188",ProductId = 57, ProductNameEn = "ALIVE 5", Quantity = 2, ShipQuantity = 2, UnitPrice = 250 }, new PurchaseOrderDetailReuest() { Pid = "111119J", ProductId = 68, ProductNameEn = "Test World", Quantity = 1, UnitPrice = 500 } } };
        public static PurchaseOrderReuest O5 = new PurchaseOrderReuest() { OrderId = "OI0005", TrackingNumber = "TR0005", ShopId = 3 , CustomerName = "Up",   OrderDate = DateTime.Now, ShippingType = "Kerry", Status = Constant.ORDER_SHIPPING,         BillAddress1 = "This is billing address 1", BillAddress2 = "This is billing address 2", BillAddress3 = "This is billing address 3", BillAddress4 = "This is billing address 4", ShipContactor = "Ship Contactor", ShipAddress1 = "This is shipping address 1", ShipAddress2 = "This is shipping address 2", ShipAddress3 = "This is shipping address 3", ShipAddress4 = "This is shipping address 4", InvoiceAddress1 = "This is invoice address 1", InvoiceAddress2 = "This is invoice address 2", InvoiceAddress3 = "This is invoice address 3", InvoiceAddress4 = "This is invoice address 4", ShipAreaCode = "Area Code", ShipCity = "This is City", ShipDistrict = "This is District", ShipProvince = "This is Province", OrdDiscAmt = 0, TotalAmt = 1000, GrandTotalAmt = 1000, Products = new List<PurchaseOrderDetailReuest>() { new PurchaseOrderDetailReuest() { Pid = "1111188",ProductId = 57, ProductNameEn = "ALIVE 5", Quantity = 2, ShipQuantity = 2, UnitPrice = 250 }, new PurchaseOrderDetailReuest() { Pid = "111119J", ProductId = 68, ProductNameEn = "Test World", Quantity = 1, UnitPrice = 500 } } };
        public static PurchaseOrderReuest O6 = new PurchaseOrderReuest() { OrderId = "OI0006", TrackingNumber = "TR0006", ShopId = 3 , CustomerName = "Noon", OrderDate = DateTime.Now, ShippingType = "Kerry", Status = Constant.ORDER_DELIVERED ,       BillAddress1 = "This is billing address 1", BillAddress2 = "This is billing address 2", BillAddress3 = "This is billing address 3", BillAddress4 = "This is billing address 4", ShipContactor = "Ship Contactor", ShipAddress1 = "This is shipping address 1", ShipAddress2 = "This is shipping address 2", ShipAddress3 = "This is shipping address 3", ShipAddress4 = "This is shipping address 4", InvoiceAddress1 = "This is invoice address 1", InvoiceAddress2 = "This is invoice address 2", InvoiceAddress3 = "This is invoice address 3", InvoiceAddress4 = "This is invoice address 4", ShipAreaCode = "Area Code", ShipCity = "This is City", ShipDistrict = "This is District", ShipProvince = "This is Province", OrdDiscAmt = 0, TotalAmt = 1000, GrandTotalAmt = 1000, Products = new List<PurchaseOrderDetailReuest>() { new PurchaseOrderDetailReuest() { Pid = "1111188",ProductId = 57, ProductNameEn = "ALIVE 5", Quantity = 2, ShipQuantity = 2, UnitPrice = 250 }, new PurchaseOrderDetailReuest() { Pid = "111119J", ProductId = 68, ProductNameEn = "Test World", Quantity = 1, UnitPrice = 500 } } };
        public static PurchaseOrderReuest O7 = new PurchaseOrderReuest() { OrderId = "OI0007", TrackingNumber = "TR0007", ShopId = 3 , CustomerName = "Pong", OrderDate = DateTime.Now, ShippingType = "DHL",   Status = Constant.ORDER_CANCELLED ,       BillAddress1 = "This is billing address 1", BillAddress2 = "This is billing address 2", BillAddress3 = "This is billing address 3", BillAddress4 = "This is billing address 4", ShipContactor = "Ship Contactor", ShipAddress1 = "This is shipping address 1", ShipAddress2 = "This is shipping address 2", ShipAddress3 = "This is shipping address 3", ShipAddress4 = "This is shipping address 4", InvoiceAddress1 = "This is invoice address 1", InvoiceAddress2 = "This is invoice address 2", InvoiceAddress3 = "This is invoice address 3", InvoiceAddress4 = "This is invoice address 4", ShipAreaCode = "Area Code", ShipCity = "This is City", ShipDistrict = "This is District", ShipProvince = "This is Province", OrdDiscAmt = 0, TotalAmt = 1000, GrandTotalAmt = 1000, Products = new List<PurchaseOrderDetailReuest>() { new PurchaseOrderDetailReuest() { Pid = "1111188",ProductId = 57, ProductNameEn = "ALIVE 5", Quantity = 2, ShipQuantity = 2, UnitPrice = 250 }, new PurchaseOrderDetailReuest() { Pid = "111119J", ProductId = 68, ProductNameEn = "Test World", Quantity = 1, UnitPrice = 500 } } };

        public static List<PurchaseOrderReuest> OrderList = new List<PurchaseOrderReuest>() {
            O1,O1_1,O1_2,O1_3,O1_4,O2,O3,O4,O5,O6,O7
        };
    }
}