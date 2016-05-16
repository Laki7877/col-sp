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
        public class SaleReportForSellerList : PaginatedResponse
        {
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
            }
        }

        public class StockStatusReportList : PaginatedResponse
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


        private ColspEntities db = new ColspEntities();

        #endregion

        #region Sale Report For Seller


        [Route("api/StandardReport/GetSaleReport")]
        [HttpGet]
        public HttpResponseMessage GetSaleReportForSeller([FromUri]SaleReportForSellerRequest request)
        {
            try
            {

                List<SaleReportForSellerList> report = new List<SaleReportForSellerList>();

                var List = db.SaleReportForSeller().ToList();
                foreach (var item in List)
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

                if (request == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, report);
                }
                request.DefaultOnNull();


                var total = report.Count();
                //var pagedProducts = report.Paginate(request);
                //var response = PaginatedResponse.CreateResponse(pagedProducts, request, total);
                return Request.CreateResponse(HttpStatusCode.OK, report);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }

        }

        [Route("api/SaleReport/ExportSaleReportForSeller")]
        [HttpGet]
        public HttpResponseMessage ExportSaleReportForSeller([FromUri]SaleReportForSellerRequest request)
        {
            MemoryStream stream = null;
            StreamWriter writer = null;
            try
            {
                using (ColspEntities dbx = new ColspEntities())
                {


                    if (request == null)
                    {
                        throw new Exception("Invalid request");
                    }

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

                    var List = db.SaleReportForSeller().ToList();
                    foreach (var item in List)
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
        [Route("api/StandardReport/GetStockStausReport")]
        [HttpGet]
        public HttpResponseMessage GetStockStausReport([FromUri]SaleReportForSellerRequest request)
        {
            try
            {

                List<StockStatusReportList> report = new List<StockStatusReportList>();

                var List = db.StockStatusReport().ToList();
                foreach (var item in List)
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


                var total = report.Count();
                //var pagedProducts = report.Paginate(request);
                //var response = PaginatedResponse.CreateResponse(pagedProducts, request, total);
                return Request.CreateResponse(HttpStatusCode.OK, report);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }

        }

        [Route("api/SaleReport/ExportStockStatusReport")]
        [HttpGet]
        public HttpResponseMessage ExportStockStatusReport([FromUri]SaleReportForSellerRequest request)
        {
            MemoryStream stream = null;
            StreamWriter writer = null;
            try
            {
                using (ColspEntities dbx = new ColspEntities())
                {


                    if (request == null)
                    {
                        throw new Exception("Invalid request");
                    }

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

                    var List = db.StockStatusReport().ToList();
                    foreach (var item in List)
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
