
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
        public static List<PurchaseOrderReuest> OrderList = new List<PurchaseOrderReuest>() {
            new PurchaseOrderReuest() { OrderId = "OI0001", ShopId = 3 , CustomerName = "Laki Sik" ,OrderDate = DateTime.Now, ShippingType = "Kerry", Status = Constant.ORDER_PAYMENT_PENDING, BillAddress = "This is billing address", ShipAddress = "This is shipping address", TotalAmt = 1000, GrandTotalAmt = 1000, Products = new List<PurchaseOrderDetailReuest>() { new PurchaseOrderDetailReuest() { Pid = "1111188", ProductNameEn = "ALIVE 5", Quantity = 2, UnitPrice = 250 }, new PurchaseOrderDetailReuest() { Pid = "111119J", ProductNameEn = "Test World", Quantity = 1, UnitPrice = 500 } } },
            new PurchaseOrderReuest() { OrderId = "OI0002", ShopId = 3 , CustomerName = "Poon" ,OrderDate = DateTime.Now, ShippingType = "DHL", Status = Constant.ORDER_PAYMENT_CONFIRM , BillAddress = "This is billing address", ShipAddress = "This is shipping address", TotalAmt = 1000, GrandTotalAmt = 1000, Products = new List<PurchaseOrderDetailReuest>() { new PurchaseOrderDetailReuest() { Pid = "1111188", ProductNameEn = "ALIVE 5", Quantity = 2, UnitPrice = 250 }, new PurchaseOrderDetailReuest() { Pid = "111119J", ProductNameEn = "Test World", Quantity = 1, UnitPrice = 500 } } },
            new PurchaseOrderReuest() { OrderId = "OI0003", ShopId = 3 , CustomerName = "Eart" ,OrderDate = DateTime.Now, ShippingType = "DHL", Status = Constant.ORDER_PREPARING , BillAddress = "This is billing address", ShipAddress = "This is shipping address", TotalAmt = 1000, GrandTotalAmt = 1000, Products = new List<PurchaseOrderDetailReuest>() { new PurchaseOrderDetailReuest() { Pid = "1111188", ProductNameEn = "ALIVE 5", Quantity = 2, UnitPrice = 250 }, new PurchaseOrderDetailReuest() { Pid = "111119J", ProductNameEn = "Test World", Quantity = 1, UnitPrice = 500 } } },
            new PurchaseOrderReuest() { OrderId = "OI0004", ShopId = 3 , CustomerName = "Nat" ,OrderDate = DateTime.Now, ShippingType = "DHL", Status = Constant.ORDER_READY_TO_SHIP , BillAddress = "This is billing address", ShipAddress = "This is shipping address", TotalAmt = 1000, GrandTotalAmt = 1000, Products = new List<PurchaseOrderDetailReuest>() { new PurchaseOrderDetailReuest() { Pid = "1111188", ProductNameEn = "ALIVE 5", Quantity = 2, UnitPrice = 250 }, new PurchaseOrderDetailReuest() { Pid = "111119J", ProductNameEn = "Test World", Quantity = 1, UnitPrice = 500 } } },
            new PurchaseOrderReuest() { OrderId = "OI0005", ShopId = 3 , CustomerName = "Up" ,OrderDate = DateTime.Now, ShippingType = "Kerry", Status = Constant.ORDER_SHIPPING, BillAddress = "This is billing address", ShipAddress = "This is shipping address", TotalAmt = 1000, GrandTotalAmt = 1000, Products = new List<PurchaseOrderDetailReuest>() { new PurchaseOrderDetailReuest() { Pid = "1111188", ProductNameEn = "ALIVE 5", Quantity = 2, UnitPrice = 250 }, new PurchaseOrderDetailReuest() { Pid = "111119J", ProductNameEn = "Test World", Quantity = 1, UnitPrice = 500 } } },
            new PurchaseOrderReuest() { OrderId = "OI0006", ShopId = 3 , CustomerName = "Noon" ,OrderDate = DateTime.Now, ShippingType = "Kerry", Status = Constant.ORDER_DELIVERED , BillAddress = "This is billing address", ShipAddress = "This is shipping address", TotalAmt = 1000, GrandTotalAmt = 1000, Products = new List<PurchaseOrderDetailReuest>() { new PurchaseOrderDetailReuest() { Pid = "1111188", ProductNameEn = "ALIVE 5", Quantity = 2, UnitPrice = 250 }, new PurchaseOrderDetailReuest() { Pid = "111119J", ProductNameEn = "Test World", Quantity = 1, UnitPrice = 500 } } },
            new PurchaseOrderReuest() { OrderId = "OI0007", ShopId = 3 , CustomerName = "Pong" ,OrderDate = DateTime.Now, ShippingType = "DHL", Status = Constant.ORDER_CANCELLED , BillAddress = "This is billing address", ShipAddress = "This is shipping address", TotalAmt = 1000, GrandTotalAmt = 1000, Products = new List<PurchaseOrderDetailReuest>() { new PurchaseOrderDetailReuest() { Pid = "1111188", ProductNameEn = "ALIVE 5", Quantity = 2, UnitPrice = 250 }, new PurchaseOrderDetailReuest() { Pid = "111119J", ProductNameEn = "Test World", Quantity = 1, UnitPrice = 500 } } },

            new PurchaseOrderReuest() { OrderId = "OI0001", ShopId = 2 , CustomerName = "Laki Sik" ,OrderDate = DateTime.Now, ShippingType = "Kerry", Status = "PP" },
            new PurchaseOrderReuest() { OrderId = "OI0002", ShopId = 2 , CustomerName = "Poon" ,OrderDate = DateTime.Now, ShippingType = "DHL", Status = "PC" },
            new PurchaseOrderReuest() { OrderId = "OI0003", ShopId = 2 , CustomerName = "Eart" ,OrderDate = DateTime.Now, ShippingType = "DHL", Status = "PE" },
            new PurchaseOrderReuest() { OrderId = "OI0004", ShopId = 2 , CustomerName = "Nat" ,OrderDate = DateTime.Now, ShippingType = "DHL", Status = "RS" },
            new PurchaseOrderReuest() { OrderId = "OI0005", ShopId = 2 , CustomerName = "Up" ,OrderDate = DateTime.Now, ShippingType = "Kerry", Status = "SH" },
            new PurchaseOrderReuest() { OrderId = "OI0006", ShopId = 2 , CustomerName = "Noon" ,OrderDate = DateTime.Now, ShippingType = "Kerry", Status = "DE" },
            new PurchaseOrderReuest() { OrderId = "OI0007", ShopId = 2 , CustomerName = "Pong" ,OrderDate = DateTime.Now, ShippingType = "DHL", Status = "CA" },
        };
    }
}