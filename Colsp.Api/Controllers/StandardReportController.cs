using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Colsp.Entity.Models;
using Colsp.Model.Responses;
using Colsp.Api.Extensions;
using Colsp.Model.Requests;
using Colsp.Api.Constants;
using Colsp.Api.Helper;
using System.Data.Entity;
using Colsp.Api.Helpers;
using System.Web;
using System.Threading.Tasks;
using System.IO;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using CsvHelper;

namespace Colsp.Api.Controllers
{
    public class StandardReportController : ApiController
    {
        #region properties
        public class SaleReportForSellerList
        {
            public Nullable<System.DateTime> OrderDateTime { get; set; }
            public string GlobalCategoryNameEN { get; internal set; }
            public string GlobalCategoryNameTH { get; internal set; }
            public string ItemStatus { get; internal set; }
            public string LocalCategoryNameEN { get; internal set; }
            public string LocalCategoryNameTH { get; internal set; }
            public decimal LocalDiscount { get; internal set; }
            public string OrderDate { get; internal set; }
            public string OrderId { get; internal set; }
            public string PID { get; internal set; }
            public string ProductNameEN { get; internal set; }
            public string ProductNameTH { get; internal set; }
            public int QTY { get; internal set; }
            public decimal RetailPrice { get; internal set; }
            public decimal? SalePrice { get; internal set; }
            public string TimeOfOrderDate { get; internal set; }
            public decimal TotalPrice { get; internal set; }
            public int? BrandId { get; set; }
            public int? GlobalCatId { get; set; }
            public int? LocalCatId { get; set; }

            public SaleReportForSellerList()
            {
                GlobalCategoryNameEN = string.Empty;
                GlobalCategoryNameTH = string.Empty;
                ItemStatus = string.Empty;
                LocalCategoryNameEN = string.Empty;
                LocalCategoryNameTH = string.Empty;
                LocalDiscount = 0;
                OrderDate = null;
                OrderId = string.Empty;
                PID = string.Empty;
                ProductNameEN = string.Empty;
                ProductNameTH = string.Empty;
                QTY = 0;
                RetailPrice = 0;
                SalePrice = 0;
                TimeOfOrderDate = null;
                TotalPrice = 0;
                BrandId = 0;
                GlobalCatId = 0;
                LocalCatId = 0;
            }
        }

        public class StockStatusReportList
        {
            public string PID { get; set; }
            public string ProductNameEN { get; set; }
            public string ProductNameTH { get; set; }
            public string variant1 { get; set; }
            public string variant2 { get; set; }
            public int OnHand { get; set; }
            public int Reserve { get; set; }
            public int OnHold { get; set; }
            public int Defect { get; set; }
            public int StockAvailable { get; set; }
            public int FirstReceiveQTY { get; set; }
            public string FirstReceiveDate { get; set; }
            public int SafetyStockAdmin { get; set; }
            public int SafetyStockSeller { get; set; }
            public string LastSoldDate { get; set; }
            public decimal? SalePrice { get; set; }
            public int AgingDay { get; set; }

            public StockStatusReportList()
            {
                PID = string.Empty;
                ProductNameEN = string.Empty;
                ProductNameTH = string.Empty;
                variant1 = string.Empty;
                variant2 = string.Empty;
                OnHand = 0;
                Reserve = 0;
                OnHold = 0;
                Defect = 0;
                StockAvailable = 0;
                FirstReceiveQTY = 0;
                FirstReceiveDate = string.Empty;
                SafetyStockAdmin = 0;
                SafetyStockSeller = 0;
                LastSoldDate = string.Empty;
                SalePrice = 0;
                AgingDay = 0;
            }
        }

        public class ItemOnHoldList
        {
            public string PID { get; set; }
            public string ItemName { get; set; }
            public Nullable<System.DateTime> OnHoldDate { get; set; }
            public string Remark { get; set; }
            public string OrderId { get; set; }

            public ItemOnHoldList()
            {
                PID = string.Empty;
                ItemName = string.Empty;
                Remark = string.Empty;
                OrderId = string.Empty;
            }
        }

        public class CommissionReportList
        {

            public string OrderId { get; set; }
            public string PID { get; set; }
            public string Itemname { get; set; }
            public int QTY { get; set; }
            public decimal SalePrice { get; set; }
            public decimal CommissionRate { get; set; }
            public decimal CommissionFee { get; set; }
            public DateTime? OrderDate { get; internal set; }
        }

        public class ReturnReportList
        {
            public string OrderId { get; set; }
            public string PID { get; set; }
            public string ItemName { get; set; }
            public string ItemStatus { get; set; }
            public decimal SalePrice { get; set; }
            public decimal LocalDiscount { get; set; }
            public decimal ReturnPrice { get; set; }
            public decimal ReturnFee { get; set; }
            public decimal ReturnAmount { get; set; }
            public DateTime? ReturnDate { get; set; }
        }

        private ColspEntities db = new ColspEntities();

        #endregion

        #region Sale Report For Seller

        [Route("api/StandardReport/GetSaleReportForSeller")]
        [HttpGet]
        public HttpResponseMessage GetSaleReportForSeller([FromUri]SaleReportForSellerRequest request)
        {
            try
            {
                return GetSaleReportForSellerDetail(request);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }

        }

        [Route("api/StandardReport/GetSearchSaleReportForSeller")]
        [HttpPost]
        public HttpResponseMessage GetSearchSaleReportForSeller(SaleReportForSellerRequest request)
        {
            try
            {
                return GetSaleReportForSellerDetail(request);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }

        }

        public HttpResponseMessage GetSaleReportForSellerDetail(SaleReportForSellerRequest request)
        {
            try
            {
                List<SaleReportForSellerList> report = new List<SaleReportForSellerList>();
                int ShopId = User.ShopRequest() == null ? 0 : User.ShopRequest().ShopId;
                var Query = (from n in db.SaleReportForSeller() where n.ShopId == ShopId select n);
                foreach (var item in Query)
                {
                    SaleReportForSellerList model = new SaleReportForSellerList();
                    model.OrderDate = item.OrderDate;
                    model.TimeOfOrderDate = item.TimeOfOrderDate;
                    model.ItemStatus = item.ItemStatus;
                    model.PID = item.PID;
                    model.ProductNameTH = item.ItemName;
                    model.ProductNameEN = item.ItemNameEN;
                    model.QTY = item.QTY;
                    model.RetailPrice = item.RetailPrice;
                    model.LocalDiscount = item.LocalDiscount;
                    model.SalePrice = item.SalePrice;
                    model.TotalPrice = item.TotalPrice;
                    model.GlobalCategoryNameEN = item.GlobalCategoryNameEN;
                    model.GlobalCategoryNameTH = item.GlobalCategoryNameTH;
                    model.LocalCategoryNameEN = item.LocalCategoryNameEN;
                    model.LocalCategoryNameTH = item.LcalCategoryNameTH;
                    model.OrderId = item.OrderId;
                    model.GlobalCatId = item.GlobalCatId;
                    model.LocalCatId = item.LocalCatId;
                    model.BrandId = item.BrandId;
                    report.Add(model);

                }
                if (request == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, report);
                }
                request.DefaultOnNull();
                if (request.GlobalCategoryId != null && request.GlobalCategoryId.HasValue)
                {
                    report = report.Where(c => c.GlobalCatId == (int)request.GlobalCategoryId).ToList();
                }
                if (request.BrandId != null && request.BrandId.HasValue)
                {
                    report = report.Where(c => c.BrandId == request.BrandId).ToList();
                }
                if (request.LocalCategoryId != null && request.LocalCategoryId.HasValue)
                {
                    report = report.Where(c => c.LocalCatId == request.LocalCategoryId).ToList();
                }
                if (!string.IsNullOrWhiteSpace(request.ItemStatus))
                {
                    report = report.Where(c => c.ItemStatus.Contains(request.ItemStatus)).ToList();
                }
                if (request.PID != null && request.PID.HasValue)
                {
                    report = report.Where(c => c.PID.Contains(request.PID.ToString())).ToList();
                }
                if (!string.IsNullOrWhiteSpace(request.ItemName))
                {
                    report = report.Where(c => c.ProductNameEN.Contains(request.ItemName) || c.ProductNameTH.Contains(request.ItemName)).ToList();
                }
                if (!string.IsNullOrWhiteSpace(request.OrderDateFrom))
                {
                    DateTime from = Convert.ToDateTime(request.OrderDateFrom);
                    report = report.Where(c => c.OrderDateTime >= (DateTime)from).ToList();
                }
                if (!string.IsNullOrWhiteSpace(request.OrderDateTo))
                {
                    DateTime to = Convert.ToDateTime(request.OrderDateTo);
                    report = report.Where(c => c.OrderDateTime <= to).ToList();
                }

                var total = report.Count();
                var pagedProducts = Query.AsQueryable().Paginate(request);
                var response = PaginatedResponse.CreateResponse(pagedProducts, request, total);


                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }

        }

        [Route("api/StandardReport/ExportSaleReportForSeller")]
        [HttpPost]
        public HttpResponseMessage ExportSaleReportForSeller(SaleReportForSellerRequest request)
        {
            MemoryStream stream = null;
            StreamWriter writer = null;
            try
            {
                using (ColspEntities dbx = new ColspEntities())
                {

                    List<string> header = new List<string>();

                    //header.Add("OrderDateTime");
                    header.Add("OrderDate");
                    header.Add("TimeOfOrderDate");
                    header.Add("ItemStatus");
                    header.Add("PID");
                    //header.Add("ItemName");
                    header.Add("ItemNameEN");
                    header.Add("QTY");
                    header.Add("RetailPrice");
                    header.Add("LocalDiscount");
                    header.Add("SalePrice");
                    header.Add("TotalPrice");
                    header.Add("GlobalCategoryNameEN");
                    header.Add("GlobalCategoryNameTH");
                    header.Add("LocalCategoryNameEN");
                    header.Add("LocalCategoryNameTH");
                    header.Add("OrderId");
                    //header.Add("BrandId");
                    //header.Add("GlobalCatId");
                    //header.Add("LocalCatId");

                    List<SaleReportForSellerList> report = new List<SaleReportForSellerList>();

                    int ShopId = User.ShopRequest() == null ? 0 : User.ShopRequest().ShopId;
                    var Query = (from n in db.SaleReportForSeller() where n.ShopId == ShopId select n);
                    foreach (var item in Query)
                    {

                        SaleReportForSellerList model = new SaleReportForSellerList();
                        model.OrderDate = item.OrderDate;
                        model.TimeOfOrderDate = item.TimeOfOrderDate;
                        model.ItemStatus = item.ItemStatus;
                        model.PID = item.PID;
                        model.ProductNameTH = item.ItemName;
                        model.ProductNameEN = item.ItemNameEN;
                        model.QTY = item.QTY;
                        model.RetailPrice = item.RetailPrice;
                        model.LocalDiscount = item.LocalDiscount;
                        model.SalePrice = item.SalePrice;
                        model.TotalPrice = item.TotalPrice;
                        model.GlobalCategoryNameEN = item.GlobalCategoryNameEN;
                        model.GlobalCategoryNameTH = item.GlobalCategoryNameTH;
                        model.LocalCategoryNameEN = item.LocalCategoryNameEN;
                        model.LocalCategoryNameTH = item.LcalCategoryNameTH;
                        model.OrderId = item.OrderId;
                        report.Add(model);
                    }

                    if (request != null && request.GlobalCategoryId != null && !(request.GlobalCategoryId.HasValue))
                    {
                        report = report.Where(c => c.GlobalCatId == (int)request.GlobalCategoryId).ToList();
                    }
                    if (request != null && request.BrandId != null && !(request.BrandId.HasValue))
                    {
                        report = report.Where(c => c.BrandId == request.BrandId).ToList();
                    }
                    if (request != null && request.LocalCategoryId != null && !(request.LocalCategoryId.HasValue))
                    {
                        report = report.Where(c => c.LocalCatId == request.LocalCategoryId).ToList();
                    }
                    if (request != null && !string.IsNullOrWhiteSpace(request.ItemStatus))
                    {
                        report = report.Where(c => c.ItemStatus.Contains(request.ItemStatus)).ToList();
                    }
                    if (request != null && request.PID != null && !(request.PID.HasValue))
                    {
                        report = report.Where(c => c.PID.Contains(request.PID.ToString())).ToList();
                    }
                    if (request != null && !string.IsNullOrWhiteSpace(request.ItemName))
                    {
                        report = report.Where(c => c.ProductNameEN.Contains(request.ItemName) || c.ProductNameTH.Contains(request.ItemName)).ToList();
                    }
                    if (request != null && !string.IsNullOrWhiteSpace(request.OrderDateFrom))
                    {
                        DateTime from = Convert.ToDateTime(request.OrderDateFrom);
                        report = report.Where(c => c.OrderDateTime >= (DateTime)from).ToList();
                    }
                    if (request != null && !string.IsNullOrWhiteSpace(request.OrderDateTo))
                    {
                        DateTime to = Convert.ToDateTime(request.OrderDateTo);
                        report = report.Where(c => c.OrderDateTime <= to).ToList();
                    }
                    stream = new MemoryStream();
                    writer = new StreamWriter(stream);
                    var csv = new CsvWriter(writer);
                    foreach (string h in header)
                    {
                        csv.WriteField(h);
                    }

                    csv.NextRecord();

                    foreach (var row in report)
                    {
                        csv.WriteField(row.OrderDate);
                        csv.WriteField(row.TimeOfOrderDate);
                        csv.WriteField(row.ItemStatus);
                        csv.WriteField("'" + row.PID);
                        csv.WriteField(row.ProductNameEN);
                        //csv.WriteField(row.ProductNameTH);
                        csv.WriteField(row.QTY);
                        csv.WriteField(row.RetailPrice);
                        csv.WriteField(row.LocalDiscount);
                        csv.WriteField(row.SalePrice);
                        csv.WriteField(row.TotalPrice);
                        csv.WriteField(row.GlobalCategoryNameEN);
                        csv.WriteField(row.GlobalCategoryNameTH);
                        csv.WriteField(row.LocalCategoryNameEN);
                        csv.WriteField(row.LocalCategoryNameTH);
                        csv.WriteField(row.OrderId);

                        csv.NextRecord();
                    }

                    writer.Flush();
                    stream.Position = 0;
                    HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);
                    result.Content = new StreamContent(stream);
                    result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream")
                    {
                        CharSet = Encoding.UTF8.WebName
                    };
                    result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
                    result.Content.Headers.ContentDisposition.FileName = "file.csv";
                    return result;
                }
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }

        #endregion

        #region Stock Status Report
        [Route("api/StandardReport/GetStockStatusReport")]
        [HttpGet]
        public HttpResponseMessage GetStockStatusReport([FromUri]StockStatusReportRequest request)
        {
            try
            {
                return GetStockStatusReportDetail(request);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }

        }

        [Route("api/StandardReport/GetSearchStockStatusReport")]
        [HttpPost]
        public HttpResponseMessage GetSearchStockStatusReport(StockStatusReportRequest request)
        {
            try
            {
                return GetStockStatusReportDetail(request);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }

        }

        public HttpResponseMessage GetStockStatusReportDetail(StockStatusReportRequest request)
        {
            try
            {
                List<StockStatusReportList> report = new List<StockStatusReportList>();
                int ShopId = User.ShopRequest() == null ? 0 : User.ShopRequest().ShopId;
                var Query = (from n in db.StockStatusReport() where n.ShopId == ShopId select n);
                foreach (var item in Query)
                {
                    StockStatusReportList model = new StockStatusReportList();
                    model.PID = item.Pid;
                    model.ProductNameEN = item.ProductNameEn;
                    model.ProductNameTH = item.ProductNameTh;
                    model.variant1 = item.variant1;
                    model.variant2 = item.variant2;
                    model.OnHand = item.OnHand;
                    model.Reserve = item.Reserve;
                    model.OnHold = item.OnHold;
                    model.Defect = item.Defect;
                    model.StockAvailable = (int)item.StockAvailable;
                    model.FirstReceiveQTY = item.FirstReceiveQTY;
                    model.FirstReceiveDate = item.FirstReceiveDate;
                    model.SafetyStockAdmin = item.SafetyStockAdmin;
                    model.SafetyStockSeller = item.SafetyStockSeller;
                    model.LastSoldDate = item.LastSoldDate;
                    model.SalePrice = item.SalePrice;
                    model.AgingDay = (int)item.AgingDay;
                    report.Add(model);

                }


                if (request == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, report);
                }
                request.DefaultOnNull();
                if (!string.IsNullOrWhiteSpace(request.Pid))
                {
                    report = report.Where(c => c.PID.Contains(request.Pid)).ToList();
                }
                if (!string.IsNullOrWhiteSpace(request.variant))
                {
                    report = report.Where(c => (c.variant1.Contains(request.variant) || c.variant2.Contains(request.variant))).ToList();
                }
                if (!string.IsNullOrWhiteSpace(request.ProductName))
                {
                    report = report.Where(c => c.ProductNameEN.Contains(request.ProductName) || c.ProductNameTH.Contains(request.ProductName)).ToList();
                }
                if (!string.IsNullOrWhiteSpace(request.LastSoldDate))
                {

                    report = report.Where(c => c.LastSoldDate == request.LastSoldDate).ToList();
                }

                var total = report.Count();
                var pagedProducts = report.AsQueryable().Paginate(request);
                var response = PaginatedResponse.CreateResponse(pagedProducts, request, total);

                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }

        }

        [Route("api/StandardReport/ExportStockStatusReport")]
        [HttpPost]
        public HttpResponseMessage ExportStockStatusReport(StockStatusReportRequest request)
        {
            MemoryStream stream = null;
            StreamWriter writer = null;
            try
            {
                using (ColspEntities dbx = new ColspEntities())
                {


                    List<string> header = new List<string>();

                    header.Add("PID");
                    header.Add("ProductNameEN");
                    header.Add("ProductNameTH");
                    header.Add("variant1");
                    header.Add("variant2");
                    header.Add("OnHand");
                    header.Add("Reserve");
                    header.Add("OnHold");
                    header.Add("Defect");
                    header.Add("StockAvailable");
                    header.Add("FirstReceiveQTY");
                    header.Add("FirstReceiveDate");
                    header.Add("SafetyStockAdmin");
                    header.Add("SafetyStockSeller");
                    header.Add("LastSoldDate");
                    header.Add("SalePrice");
                    header.Add("AgingDay");

                    List<StockStatusReportList> report = new List<StockStatusReportList>();

                    int ShopId = User.ShopRequest() == null ? 0 : User.ShopRequest().ShopId;
                    var Query = (from n in db.StockStatusReport() where n.ShopId == ShopId select n);

                    foreach (var item in Query)
                    {
                        StockStatusReportList model = new StockStatusReportList();
                        model.PID = item.Pid;
                        model.ProductNameEN = item.ProductNameEn;
                        model.ProductNameTH = item.ProductNameTh;
                        model.variant1 = item.variant1;
                        model.variant2 = item.variant2;
                        model.OnHand = item.OnHand;
                        model.Reserve = item.Reserve;
                        model.OnHold = item.OnHold;
                        model.Defect = item.Defect;
                        model.StockAvailable = (int)item.StockAvailable;
                        model.FirstReceiveQTY = item.FirstReceiveQTY;
                        model.FirstReceiveDate = item.FirstReceiveDate;
                        model.SafetyStockAdmin = item.SafetyStockAdmin;
                        model.SafetyStockSeller = item.SafetyStockSeller;
                        model.LastSoldDate = item.LastSoldDate;
                        model.SalePrice = item.SalePrice;
                        model.AgingDay = (int)item.AgingDay;
                        report.Add(model);

                    }

                    if (request != null && !string.IsNullOrWhiteSpace(request.Pid))
                    {
                        report = report.Where(c => c.PID.Contains(request.Pid)).ToList();
                    }
                    if (request != null && !string.IsNullOrWhiteSpace(request.variant))
                    {
                        report = report.Where(c => (c.variant1.Contains(request.variant) || c.variant2.Contains(request.variant))).ToList();
                    }
                    if (request != null && !string.IsNullOrWhiteSpace(request.ProductName))
                    {
                        report = report.Where(c => c.ProductNameEN.Contains(request.ProductName) || c.ProductNameTH.Contains(request.ProductName)).ToList();
                    }
                    if (request != null && !string.IsNullOrWhiteSpace(request.LastSoldDate))
                    {

                        report = report.Where(c => c.LastSoldDate == request.LastSoldDate).ToList();
                    }

                    stream = new MemoryStream();
                    writer = new StreamWriter(stream);
                    var csv = new CsvWriter(writer);
                    foreach (string h in header)
                    {
                        csv.WriteField(h);
                    }

                    csv.NextRecord();

                    foreach (var row in report.Take(500))
                    {

                        csv.WriteField("'" + row.PID);
                        csv.WriteField(row.ProductNameEN);
                        csv.WriteField(row.ProductNameTH);
                        csv.WriteField(row.variant1);
                        csv.WriteField(row.variant2);
                        csv.WriteField(row.OnHand);
                        csv.WriteField(row.Reserve);
                        csv.WriteField(row.OnHold);
                        csv.WriteField(row.Defect);
                        csv.WriteField(row.StockAvailable);
                        csv.WriteField(row.FirstReceiveQTY);
                        csv.WriteField(row.FirstReceiveDate);
                        csv.WriteField(row.SafetyStockAdmin);
                        csv.WriteField(row.SafetyStockSeller);
                        csv.WriteField(row.LastSoldDate);
                        csv.WriteField(row.AgingDay);

                        csv.NextRecord();
                    }

                    writer.Flush();
                    stream.Position = 0;
                    HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);
                    result.Content = new StreamContent(stream);
                    result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream")
                    {
                        CharSet = Encoding.UTF8.WebName
                    };
                    result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
                    result.Content.Headers.ContentDisposition.FileName = "file.csv";
                    return result;
                }
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }
        #endregion

        #region Item On Hold
        [Route("api/StandardReport/GetItemOnHoldReport")]
        [HttpGet]
        public HttpResponseMessage GetItemOnHoldReport([FromUri]ItemOnHoldReportRequest request)
        {
            try
            {
                return GetItemOnHoldReportDetail(request);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }

        [Route("api/StandardReport/GetSearchItemOnHoldReport")]
        [HttpPost]
        public HttpResponseMessage GetSearchItemOnHoldReport(ItemOnHoldReportRequest request)
        {
            try
            {
                return GetItemOnHoldReportDetail(request);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }

        public HttpResponseMessage GetItemOnHoldReportDetail(ItemOnHoldReportRequest request)
        {

            try
            {
                List<ItemOnHoldList> report = new List<ItemOnHoldList>();
                int ShopId = User.ShopRequest() == null ? 0 : User.ShopRequest().ShopId;
                var Query = (from n in db.ItemOnHoldReport() where n.ShopId == ShopId select n);

                foreach (var item in Query)
                {
                    ItemOnHoldList model = new ItemOnHoldList();
                    model.OnHoldDate = item.OnHoldDate;
                    model.PID = item.PID;
                    model.ItemName = item.Itemname;
                    //model.OrderId = item.Orderid;
                    model.Remark = item.OnHoldRemark;
                    report.Add(model);
                }

                if (request == null || string.IsNullOrWhiteSpace(request.OrderDateFrom))
                {
                    return Request.CreateResponse(HttpStatusCode.OK, report);
                }
                request.DefaultOnNull();

                if (!string.IsNullOrWhiteSpace(request.ItemName))
                {
                    report = report.Where(c => c.ItemName.Contains(request.ItemName)).ToList();
                }
                if (!string.IsNullOrWhiteSpace(request.PID))
                {
                    report = report.Where(c => c.PID.Contains(request.PID)).ToList();
                }
                if (!string.IsNullOrWhiteSpace(request.OrderDateFrom))
                {
                    DateTime from = Convert.ToDateTime(request.OrderDateFrom);
                    report = report.Where(c => c.OnHoldDate >= (DateTime)from).ToList();
                }
                if (!string.IsNullOrWhiteSpace(request.OrderDateEnd))
                {
                    DateTime to = Convert.ToDateTime(request.OrderDateEnd);
                    report = report.Where(c => c.OnHoldDate <= to).ToList();
                }

                var total = report.Count();
                var pagedProducts = report.AsQueryable().Paginate(request);
                var response = PaginatedResponse.CreateResponse(pagedProducts, request, total);
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }

        [Route("api/StandardReport/ExportItemOnHoldReport")]
        [HttpPost]
        public HttpResponseMessage ExportItemOnHoldReport(ItemOnHoldReportRequest request)
        {
            MemoryStream stream = null;
            StreamWriter writer = null;
            try
            {
                using (ColspEntities dbx = new ColspEntities())
                {
                    List<string> header = new List<string>();
                    header.Add("PID");
                    header.Add("ItemName");
                    header.Add("OnHoldDate");
                    header.Add("OnHoldRemark");
                    List<ItemOnHoldList> report = new List<ItemOnHoldList>();

                    int ShopId = User.ShopRequest() == null ? 0 : User.ShopRequest().ShopId;
                    var Query = (from n in db.ItemOnHoldReport() where n.ShopId == ShopId select n);
                    foreach (var item in Query)
                    {

                        ItemOnHoldList model = new ItemOnHoldList();
                        model.OnHoldDate = item.OnHoldDate;
                        model.PID = item.PID;
                        model.ItemName = item.Itemname;
                        //model.OrderId = item.Orderid;
                        model.Remark = item.OnHoldRemark;
                        report.Add(model);
                    }


                    if (request != null && !string.IsNullOrWhiteSpace(request.ItemName))
                    {
                        report = report.Where(c => c.ItemName.Contains(request.ItemName)).ToList();
                    }
                    if (request != null && !string.IsNullOrWhiteSpace(request.PID))
                    {
                        report = report.Where(c => c.PID.Contains(request.PID)).ToList();
                    }
                    if (request != null && !string.IsNullOrWhiteSpace(request.OrderDateFrom))
                    {
                        DateTime from = Convert.ToDateTime(request.OrderDateFrom);
                        report = report.Where(c => c.OnHoldDate >= (DateTime)from).ToList();
                    }
                    if (request != null && !string.IsNullOrWhiteSpace(request.OrderDateEnd))
                    {
                        DateTime to = Convert.ToDateTime(request.OrderDateEnd);
                        report = report.Where(c => c.OnHoldDate <= to).ToList();
                    }
                    stream = new MemoryStream();
                    writer = new StreamWriter(stream);
                    var csv = new CsvWriter(writer);
                    foreach (string h in header)
                    {
                        csv.WriteField(h);
                    }

                    csv.NextRecord();

                    foreach (var row in report)
                    {
                        csv.WriteField("'" + row.PID);
                        csv.WriteField(row.ItemName);
                        csv.WriteField(row.OnHoldDate);
                        csv.WriteField(row.Remark);
                        //csv.WriteField(row.OrderId);

                        csv.NextRecord();
                    }

                    writer.Flush();
                    stream.Position = 0;
                    HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);
                    result.Content = new StreamContent(stream);
                    result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream")
                    {
                        CharSet = Encoding.UTF8.WebName
                    };
                    result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
                    result.Content.Headers.ContentDisposition.FileName = "file.csv";
                    return result;
                }
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }
        #endregion

        #region Commission Report
        [Route("api/StandardReport/GetCommissionReport")]
        [HttpGet]
        public HttpResponseMessage GetCommissionReport([FromUri]CommissionReportRequest request)
        {
            try
            {
                return GetCommissionReportDetial(request);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }

        }

        [Route("api/StandardReport/GetSearchCommissionReport")]
        [HttpPost]
        public HttpResponseMessage GetSearchCommissionReport(CommissionReportRequest request)
        {
            try
            {
                return GetCommissionReportDetial(request);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }

        }

        public HttpResponseMessage GetCommissionReportDetial(CommissionReportRequest request)
        {
            try
            {
                List<CommissionReportList> report = new List<CommissionReportList>();
                int ShopId = User.ShopRequest() == null ? 0 : User.ShopRequest().ShopId;
                var Query = (from n in db.ReportProductsCommissionForOrder() where n.ShopId == ShopId select n);
                foreach (var item in Query)
                {
                    CommissionReportList com = new CommissionReportList();
                    com.CommissionFee = item.CommissionFee;
                    com.CommissionRate = item.CommissionRate;
                    com.Itemname = item.ProductName;
                    com.OrderId = item.OrderId;
                    com.PID = item.PID;
                    com.QTY = item.QTY;
                    com.SalePrice = (decimal)item.SalePrice;
                    com.OrderDate = item.OrderDate;
                    report.Add(com);
                }
                if (request == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, report);
                }
                request.DefaultOnNull();
                if (!string.IsNullOrWhiteSpace(request.PID))
                {
                    report = report.Where(c => c.PID.Contains(request.PID)).ToList();
                }
                if (!string.IsNullOrWhiteSpace(request.ProductName))
                {
                    report = report.Where(c => c.Itemname.Contains(request.ProductName)).ToList();
                }
                if (!string.IsNullOrWhiteSpace(request.OrderId))
                {
                    report = report.Where(c => c.OrderId.Contains(request.OrderId)).ToList();
                }
                if (!string.IsNullOrWhiteSpace(request.OrderDateFrom))
                {
                    DateTime from = Convert.ToDateTime(request.OrderDateFrom);
                    report = report.Where(c => c.OrderDate >= (DateTime)from).ToList();
                }
                if (!string.IsNullOrWhiteSpace(request.OrderDateEnd))
                {
                    DateTime to = Convert.ToDateTime(request.OrderDateEnd);
                    report = report.Where(c => c.OrderDate <= to).ToList();
                }
                var total = report.Count();
                var pagedProducts = report.AsQueryable().Paginate(request);
                var response = PaginatedResponse.CreateResponse(pagedProducts, request, total);

                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }

        }

        [Route("api/StandardReport/ExportCommissionReport")]
        [HttpPost]
        public HttpResponseMessage ExportCommissionReport(CommissionReportRequest request)
        {
            MemoryStream stream = null;
            StreamWriter writer = null;
            try
            {
                using (ColspEntities dbx = new ColspEntities())
                {
                    List<string> header = new List<string>();
                    header.Add("OrderDate");
                    header.Add("OrderId");
                    header.Add("PID");
                    header.Add("ProductName");
                    header.Add("QTY");
                    header.Add("SalePrice");
                    header.Add("CommissionRate");
                    header.Add("CommissionFee");
                    List<CommissionReportList> report = new List<CommissionReportList>();
                    int ShopId = User.ShopRequest() == null ? 0 : User.ShopRequest().ShopId;
                    var Query = (from n in db.ReportProductsCommissionForOrder() where n.ShopId == ShopId select n);
                    foreach (var item in Query)
                    {
                        CommissionReportList com = new CommissionReportList();
                        com.CommissionFee = item.CommissionFee;
                        com.CommissionRate = item.CommissionRate;
                        com.Itemname = item.ProductName;
                        com.OrderId = item.OrderId;
                        com.PID = item.PID;
                        com.QTY = item.QTY;
                        com.SalePrice = (decimal)item.SalePrice;
                        com.OrderDate = item.OrderDate;
                        report.Add(com);
                    }
                    if (request == null)
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, report);
                    }
                    request.DefaultOnNull();
                    if (!string.IsNullOrWhiteSpace(request.PID))
                    {
                        report = report.Where(c => c.PID.Contains(request.PID)).ToList();
                    }
                    if (!string.IsNullOrWhiteSpace(request.ProductName))
                    {
                        report = report.Where(c => c.Itemname.Contains(request.ProductName)).ToList();
                    }
                    if (!string.IsNullOrWhiteSpace(request.OrderId))
                    {
                        report = report.Where(c => c.OrderId.Contains(request.OrderId)).ToList();
                    }
                    if (!string.IsNullOrWhiteSpace(request.OrderDateFrom))
                    {
                        DateTime from = Convert.ToDateTime(request.OrderDateFrom);
                        report = report.Where(c => c.OrderDate >= (DateTime)from).ToList();
                    }
                    if (!string.IsNullOrWhiteSpace(request.OrderDateEnd))
                    {
                        DateTime to = Convert.ToDateTime(request.OrderDateEnd);
                        report = report.Where(c => c.OrderDate <= to).ToList();
                    }
                    stream = new MemoryStream();
                    writer = new StreamWriter(stream);
                    var csv = new CsvWriter(writer);
                    foreach (string h in header)
                    {
                        csv.WriteField(h);
                    }

                    csv.NextRecord();

                    foreach (var row in report)
                    {
                        csv.WriteField(row.OrderDate);
                        csv.WriteField(row.OrderId);
                        csv.WriteField("'" + row.PID);
                        csv.WriteField(row.Itemname);
                        csv.WriteField(row.QTY);
                        csv.WriteField(row.SalePrice);
                        csv.WriteField(row.CommissionRate);
                        csv.WriteField(row.CommissionFee);
                        csv.NextRecord();
                    }

                    writer.Flush();
                    stream.Position = 0;
                    HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);
                    result.Content = new StreamContent(stream);
                    result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream")
                    {
                        CharSet = Encoding.UTF8.WebName
                    };
                    result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
                    result.Content.Headers.ContentDisposition.FileName = "file.csv";
                    return result;
                }
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }
        #endregion

        #region Return Report
        [Route("api/StandardReport/GetReturnReport")]
        [HttpGet]
        public HttpResponseMessage GetReturnReport([FromUri]ReturnReportRequest request)
        {
            try
            {
                return GetReturnReportDetial(request);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }

        }

        [Route("api/StandardReport/GetSearchReturnReport")]
        [HttpPost]
        public HttpResponseMessage GetSearchReturnReport(ReturnReportRequest request)
        {
            try
            {
                return GetReturnReportDetial(request);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }

        }

        public HttpResponseMessage GetReturnReportDetial(ReturnReportRequest request)
        {
            try
            {
                List<ReturnReportList> report = new List<ReturnReportList>();
                int ShopId = User.ShopRequest() == null ? 0 : User.ShopRequest().ShopId;
                var Query = (from n in db.ReportReturnItemByOrderReport() where n.ShopId == ShopId select n);
                foreach (var item in Query)
                {
                    ReturnReportList com = new ReturnReportList();
                    com.ReturnDate = item.ReturnDate;
                    com.ItemName = item.ProductName;
                    com.ItemStatus = item.ItemStatus;
                    com.ItemName = item.ProductName;
                    com.OrderId = item.OrderId;
                    com.PID = item.Pid;
                    com.LocalDiscount = item.LocalDiscount;
                    com.SalePrice = (decimal)item.SalePrice;
                    com.OrderId = item.OrderId;
                    com.ReturnAmount = item.ReturnAmount;
                    com.ReturnFee = item.ReturnFee;
                    com.ReturnPrice = item.ReturnPrice;
                    com.SalePrice = (decimal)item.SalePrice;
                    report.Add(com);
                }
                if (request == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, report);
                }
                request.DefaultOnNull();
                if (!string.IsNullOrWhiteSpace(request.PID))
                {
                    report = report.Where(c => c.PID.Contains(request.PID)).ToList();
                }
                if (!string.IsNullOrWhiteSpace(request.ItemName))
                {
                    report = report.Where(c => c.ItemName.Contains(request.ItemName)).ToList();
                }
                if (!string.IsNullOrWhiteSpace(request.OrderId))
                {
                    report = report.Where(c => c.OrderId.Contains(request.OrderId)).ToList();
                }
                if (!string.IsNullOrWhiteSpace(request.ReturnDateFrom))
                {
                    DateTime from = Convert.ToDateTime(request.ReturnDateFrom);
                    report = report.Where(c => c.ReturnDate >= (DateTime)from).ToList();
                }
                if (!string.IsNullOrWhiteSpace(request.ReturnDateTo))
                {
                    DateTime to = Convert.ToDateTime(request.ReturnDateTo);
                    report = report.Where(c => c.ReturnDate <= to).ToList();
                }
                var total = report.Count();
                var pagedProducts = report.AsQueryable().Paginate(request);
                var response = PaginatedResponse.CreateResponse(pagedProducts, request, total);

                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }

        }

        [Route("api/StandardReport/ExportReturnReport")]
        [HttpPost]
        public HttpResponseMessage ExportReturnReport(ReturnReportRequest request)
        {
            MemoryStream stream = null;
            StreamWriter writer = null;
            try
            {
                using (ColspEntities dbx = new ColspEntities())
                {
                    List<string> header = new List<string>();
                    header.Add("OrderDate");
                    header.Add("OrderId");
                    header.Add("PID");
                    header.Add("ProductName");
                    header.Add("Status");
                    header.Add("SalePrice");
                    header.Add("LocalDiscount");
                    header.Add("ReturnPrice");
                    header.Add("ReturnFee");
                    header.Add("ReturnAmount");
                    List<ReturnReportList> report = new List<ReturnReportList>();
                    int ShopId = User.ShopRequest() == null ? 0 : User.ShopRequest().ShopId;
                    var Query = (from n in db.ReportReturnItemByOrderReport() where n.ShopId == ShopId select n);
                    foreach (var item in Query)
                    {
                        ReturnReportList com = new ReturnReportList();
                        com.ReturnDate = item.ReturnDate;
                        com.ItemName = item.ProductName;
                        com.ItemStatus = item.ItemStatus;
                        com.ItemName = item.ProductName;
                        com.OrderId = item.OrderId;
                        com.PID = item.Pid;
                        com.LocalDiscount = item.LocalDiscount;
                        com.SalePrice = (decimal)item.SalePrice;
                        com.OrderId = item.OrderId;
                        com.ReturnAmount = item.ReturnAmount;
                        com.ReturnFee = item.ReturnFee;
                        com.ReturnPrice = item.ReturnPrice;
                        com.SalePrice = (decimal)item.SalePrice;
                        report.Add(com);
                    }
                    if (request == null)
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, report);
                    }
                    request.DefaultOnNull();
                    if (!string.IsNullOrWhiteSpace(request.PID))
                    {
                        report = report.Where(c => c.PID.Contains(request.PID)).ToList();
                    }
                    if (!string.IsNullOrWhiteSpace(request.ItemName))
                    {
                        report = report.Where(c => c.ItemName.Contains(request.ItemName)).ToList();
                    }
                    if (!string.IsNullOrWhiteSpace(request.OrderId))
                    {
                        report = report.Where(c => c.OrderId.Contains(request.OrderId)).ToList();
                    }
                    if (!string.IsNullOrWhiteSpace(request.ReturnDateFrom))
                    {
                        DateTime from = Convert.ToDateTime(request.ReturnDateFrom);
                        report = report.Where(c => c.ReturnDate >= (DateTime)from).ToList();
                    }
                    if (!string.IsNullOrWhiteSpace(request.ReturnDateTo))
                    {
                        DateTime to = Convert.ToDateTime(request.ReturnDateTo);
                        report = report.Where(c => c.ReturnDate <= to).ToList();
                    }
                    stream = new MemoryStream();
                    writer = new StreamWriter(stream);
                    var csv = new CsvWriter(writer);
                    foreach (string h in header)
                    {
                        csv.WriteField(h);
                    }

                    csv.NextRecord();

                    foreach (var row in report)
                    {
                        csv.WriteField(row.OrderId);
                        csv.WriteField("'" + row.PID);
                        csv.WriteField(row.ItemName);
                        csv.WriteField(row.ItemStatus);
                        csv.WriteField(row.SalePrice);
                        csv.WriteField(row.LocalDiscount);
                        csv.WriteField(row.ReturnPrice);
                        csv.WriteField(row.ReturnFee);
                        csv.WriteField(row.ReturnAmount);
                        csv.NextRecord();
                    }

                    writer.Flush();
                    stream.Position = 0;
                    HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);
                    result.Content = new StreamContent(stream);
                    result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream")
                    {
                        CharSet = Encoding.UTF8.WebName
                    };
                    result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
                    result.Content.Headers.ContentDisposition.FileName = "file.csv";
                    return result;
                }
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }
        #endregion

        #region OI Report

        #endregion

        #region Non-Move Report

        #endregion

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
