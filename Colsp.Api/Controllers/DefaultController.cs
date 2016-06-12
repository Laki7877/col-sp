using Cenergy.Dazzle.Admin.Security.Cryptography;
using Colsp.Api.Constants;
using Colsp.Entity.Models;
using Colsp.Model.Requests;
using CsvHelper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Script.Serialization;

namespace Colsp.Api.Controllers
{
    public class DefaultController : ApiController
    {
        private ColspEntities db = new ColspEntities();

        [Route("api/Default")]
        [HttpGet]
        [OverrideAuthentication, OverrideAuthorization]
        public HttpResponseMessage GetTest()
        {




            return Request.CreateErrorResponse(HttpStatusCode.NotFound, "false");
        }

        [Route("api/Test")]
        [HttpGet]
        [OverrideAuthentication, OverrideAuthorization]
        public HttpResponseMessage Test()
        {
            string tmp = "{ 'AutoPlay': true, 'Images': [{ 'url': '', 'position': 1, 'ImageId': 0, 'SlideDuration': 1.2 }] }";
            var json = new JavaScriptSerializer().Deserialize<BannerComponent>(tmp);


            SaltedSha256PasswordHasher salt = new SaltedSha256PasswordHasher();

            return Request.CreateResponse(HttpStatusCode.OK, salt.HashPassword("vader"));
        }

        private readonly string root = HttpContext.Current.Server.MapPath("~/Migrate");

        [Route("api/Migrate/Attribute")]
        [HttpPost]
        [OverrideAuthentication, OverrideAuthorization]
        public async Task<HttpResponseMessage> MigrateAttribute()
        {
            string fileName = string.Empty;
            try
            {
                if (!Request.Content.IsMimeMultipartContent())
                {
                    throw new Exception("Content Multimedia");
                }
                var streamProvider = new MultipartFormDataStreamProvider(root);
                await Request.Content.ReadAsMultipartAsync(streamProvider);

                if (streamProvider.FileData == null || streamProvider.FileData.Count == 0)
                {
                    throw new Exception("No file uploaded");
                }
                fileName = streamProvider.FileData[0].LocalFileName;

                using (var fileReader = File.OpenText(fileName))
                {

                    using (var csvResult = new CsvReader(fileReader, new CsvHelper.Configuration.CsvConfiguration() { HasHeaderRecord = false}))
                    {
                        if (!csvResult.Read())
                        {
                            throw new Exception("File is not in a proper format");
                        }
                        var headerCount = csvResult.CurrentRecord.Count();
                        List<Entity.Models.Attribute> attributeList = new List<Entity.Models.Attribute>();
                        string tmpColumn = string.Empty;
                        
                        while (csvResult.Read())
                        {
                            tmpColumn = csvResult.GetField<string>(0);
                            if (string.IsNullOrWhiteSpace(tmpColumn))
                            {
                                continue;
                            }
                            Entity.Models.Attribute attribute = new Entity.Models.Attribute()
                            {
                                AllowHtmlFlag = false,
                                AttributeNameEn = string.Empty,
                                CreateBy = "Ahancer",
                                CreateOn = DateTime.Now,
                                DataType = string.Empty,
                                DataValidation = string.Empty,
                                DefaultAttribute = false,
                                DefaultValue = string.Empty,
                                DisplayNameEn = string.Empty,
                                DisplayNameTh = string.Empty,
                                Filterable = false,
                                Required = false,
                                ShowAdminFlag = false,
                                ShowGlobalFilterFlag = false,
                                ShowGlobalSearchFlag = false,
                                ShowLocalFilterFlag = false,
                                ShowLocalSearchFlag = false,
                                Status = Constant.STATUS_ACTIVE,
                                UpdateBy = "Ahancer",
                                UpdateOn = DateTime.Now,
                                VariantDataType = string.Empty,
                                VariantStatus = false,
                                VisibleTo = Constant.ATTRIBUTE_VISIBLE_ALL_USER,
                            };
                            attribute.AttributeNameEn = tmpColumn.Trim();

                            tmpColumn = csvResult.GetField<string>(3);
                            attribute.DisplayNameEn = tmpColumn.Trim();


                            tmpColumn = csvResult.GetField<string>(4);
                            attribute.DisplayNameTh = tmpColumn.Trim();

                            tmpColumn = csvResult.GetField<string>(5).Trim();
                            if (string.Equals("Dropdown", tmpColumn, StringComparison.OrdinalIgnoreCase))
                            {
                                attribute.DataType = Constant.DATA_TYPE_LIST;
                            }
                            else if (string.Equals("Free Text", tmpColumn, StringComparison.OrdinalIgnoreCase))
                            {
                                attribute.DataType = Constant.DATA_TYPE_STRING;
                            }
                            else if (string.Equals("Checkbox", tmpColumn, StringComparison.OrdinalIgnoreCase))
                            {
                                attribute.DataType = Constant.DATA_TYPE_CHECKBOX;
                            }

                            tmpColumn = csvResult.GetField<string>(6).Trim();
                            if (string.Equals("Yes", tmpColumn, StringComparison.OrdinalIgnoreCase))
                            {
                                attribute.VariantStatus = true;
                                attribute.VariantDataType = attribute.DataType;
                            }
                            else if (string.Equals("No", tmpColumn, StringComparison.OrdinalIgnoreCase))
                            {
                                attribute.VariantStatus = false;
                            }

                            tmpColumn = csvResult.GetField<string>(7).Trim();
                            if (string.Equals("Yes", tmpColumn, StringComparison.OrdinalIgnoreCase))
                            {
                                attribute.Required = true;
                            }
                            else if (string.Equals("No", tmpColumn, StringComparison.OrdinalIgnoreCase))
                            {
                                attribute.Required = false;
                            }

                            tmpColumn = csvResult.GetField<string>(8).Trim();
                            if (string.Equals("Yes", tmpColumn, StringComparison.OrdinalIgnoreCase))
                            {
                                attribute.Filterable = true;
                            }
                            else if (string.Equals("No", tmpColumn, StringComparison.OrdinalIgnoreCase))
                            {
                                attribute.Filterable = false;
                            }

                            for(int j = 9; j < headerCount; j=j+4)
                            {
                                if(j+4 > headerCount)
                                {
                                    break;
                                }

                                var propertyID = csvResult.GetField<string>(j).Trim();
                                var filterID = csvResult.GetField<string>(j+1).Trim();
                                var valueEN = csvResult.GetField<string>(j+2).Trim();
                                var valueTH = csvResult.GetField<string>(j+3).Trim();

                                if(string.IsNullOrWhiteSpace(valueEN))
                                {
                                    continue;
                                }
                                AttributeValue value = new AttributeValue()
                                {
                                    AttributeValueEn = valueEN,
                                    AttributeValueTh = valueTH,
                                    ImageUrl = string.Empty,
                                    Status = Constant.STATUS_ACTIVE,
                                    CreateBy = "Ahancer",
                                    CreateOn = DateTime.Now,
                                    UpdateBy = "Ahancer",
                                    UpdateOn = DateTime.Now,
                                    MapValue = string.Empty,
                                };

                                var spiltProperty = propertyID.Split(',');
                                if(spiltProperty != null && spiltProperty.Count() > 0)
                                {
                                    foreach(var id in spiltProperty)
                                    {
                                        int val = 0;
                                        if(int.TryParse(id.Trim(), out val))
                                        {
                                            value.AttributePropertyMaps.Add(new AttributePropertyMap()
                                            {
                                                PropertyId = val
                                            });
                                        }
                                    }
                                }
                                var spiltFilter = filterID.Split(',');
                                if(spiltFilter != null && spiltFilter.Count() > 0)
                                {
                                    foreach (var id in spiltFilter)
                                    {
                                        int val = 0;
                                        if (int.TryParse(id.Trim(), out val))
                                        {
                                            value.AttributeFilterMaps.Add(new AttributeFilterMap()
                                            {
                                                FilterId = val
                                            });
                                        }
                                    }
                                }
                                attribute.AttributeValueMaps.Add(new AttributeValueMap()
                                {
                                    AttributeValue = value
                                });
                            }
                            attributeList.Add(attribute);
                        }
                        if (attributeList != null && attributeList.Count > 0)
                        {
                            db.Attributes.AddRange(attributeList);
                            db.SaveChanges();
                        }
                    }
                }
                

                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.GetBaseException().Message);
            }
            finally
            {
                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                }
            }
        }


        [Route("api/Migrate/AttributeSet")]
        [HttpPost]
        [OverrideAuthentication, OverrideAuthorization]
        public async Task<HttpResponseMessage> MigrateAttributeSet()
        {
            string fileName = string.Empty;
            List<AttributeSet> attributeSetList = new List<AttributeSet>();
            try
            {
                if (!Request.Content.IsMimeMultipartContent())
                {
                    throw new Exception("Content Multimedia");
                }
                var streamProvider = new MultipartFormDataStreamProvider(root);
                await Request.Content.ReadAsMultipartAsync(streamProvider);

                if (streamProvider.FileData == null || streamProvider.FileData.Count == 0)
                {
                    throw new Exception("No file uploaded");
                }
                fileName = streamProvider.FileData[0].LocalFileName;

                using (var fileReader = File.OpenText(fileName))
                {

                    using (var csvResult = new CsvReader(fileReader, new CsvHelper.Configuration.CsvConfiguration() { HasHeaderRecord = false }))
                    {
                        if (!csvResult.Read())
                        {
                            throw new Exception("File is not in a proper format");
                        }
                        var headerCount = csvResult.CurrentRecord.Count();
                       
                        string tmpColumn = string.Empty;
                        var attributeList = db.Attributes.Select(s => new { s.AttributeId, s.AttributeNameEn }).ToList();
                        while (csvResult.Read())
                        {
                            tmpColumn = csvResult.GetField<string>(0).Trim();
                            if (string.IsNullOrWhiteSpace(tmpColumn))
                            {
                                continue;
                            }
                            AttributeSet set = null;
                            try
                            {
                                set = attributeSetList.Where(w => w.AttributeSetNameEn.Equals(tmpColumn)).SingleOrDefault();
                            }
                            catch(Exception ex)
                            {
                                throw ex;
                            }
                            
                            if(set == null)
                            {
                                set = new AttributeSet()
                                {
                                    AttributeSetDescriptionEn = string.Empty,
                                    AttributeSetNameEn = string.Empty,
                                    Status = Constant.STATUS_ACTIVE,
                                    Visibility = true,
                                    CreateBy = "Ahancer",
                                    CreateOn = DateTime.Now,
                                    UpdateBy = "Ahancer",
                                    UpdateOn = DateTime.Now,
                                };
                                set.AttributeSetNameEn = tmpColumn;
                                attributeSetList.Add(set);
                            }

                            tmpColumn = csvResult.GetField<string>(1).Trim();
                            var spilt = tmpColumn.Split(',');
                            if (spilt != null && spilt.Count() > 0)
                            {
                                foreach (var attribute in spilt)
                                {
                                    try
                                    {
                                        var current = attributeList.Where(w => w.AttributeNameEn.Equals(attribute.Trim())).SingleOrDefault();
                                        if (current != null)
                                        {
                                            if (!set.AttributeSetMaps.Any(a => a.AttributeId == current.AttributeId))
                                            {
                                                set.AttributeSetMaps.Add(new AttributeSetMap()
                                                {
                                                    AttributeId = current.AttributeId,
                                                    CreateBy = "Ahancer",
                                                    CreateOn = DateTime.Now,
                                                    UpdateBy = "Ahancer",
                                                    UpdateOn = DateTime.Now,
                                                });
                                            }
                                        }
                                    }
                                    catch(Exception ex)
                                    {
                                        throw ex;
                                    }
                                    
                                }
                            }

                            
                        }
                        if (attributeSetList != null && attributeSetList.Count > 0)
                        {
                            db.AttributeSets.AddRange(attributeSetList);
                            db.SaveChanges();
                        }
                    }
                }


                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.GetBaseException().Message);
            }
            finally
            {
                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                }
            }
        }


        [Route("api/Migrate/GlobalCateAttributeSet")]
        [HttpPost]
        [OverrideAuthentication, OverrideAuthorization]
        public async Task<HttpResponseMessage> MigrateGlobalCateAttributeSet()
        {
            string fileName = string.Empty;
            try
            {
                if (!Request.Content.IsMimeMultipartContent())
                {
                    throw new Exception("Content Multimedia");
                }
                var streamProvider = new MultipartFormDataStreamProvider(root);
                await Request.Content.ReadAsMultipartAsync(streamProvider);

                if (streamProvider.FileData == null || streamProvider.FileData.Count == 0)
                {
                    throw new Exception("No file uploaded");
                }
                fileName = streamProvider.FileData[0].LocalFileName;

                using (var fileReader = File.OpenText(fileName))
                {

                    using (var csvResult = new CsvReader(fileReader, new CsvHelper.Configuration.CsvConfiguration() { HasHeaderRecord = false }))
                    {
                        if (!csvResult.Read())
                        {
                            throw new Exception("File is not in a proper format");
                        }
                        var headerCount = csvResult.CurrentRecord.Count();

                        string tmpColumn = string.Empty;
                        var attributeSetList = db.AttributeSets.Select(s => new { s.AttributeSetId, s.AttributeSetNameEn }).ToList();
                        while (csvResult.Read())
                        {
                            tmpColumn = csvResult.GetField<string>(0).Trim();
                            int categoryId = 0;
                            if(int.TryParse(tmpColumn,out categoryId))
                            {
                                tmpColumn = csvResult.GetField<string>(1).Trim();
                                var split = tmpColumn.Split(',');
                                foreach (var val in split)
                                {
                                    var set = attributeSetList.Where(w => w.AttributeSetNameEn.Equals(val.Trim().ToLower().Replace(' ','_'))).SingleOrDefault();
                                    if (set == null)
                                    {
                                        set = attributeSetList.Where(w => w.AttributeSetNameEn.Equals(val.Trim().Replace(' ', '_'))).SingleOrDefault();
                                        if (set == null)
                                        {
                                            continue;
                                        }
                                    }
                                    db.GlobalCatAttributeSetMaps.Add(new GlobalCatAttributeSetMap()
                                    {
                                        CategoryId = categoryId,
                                        AttributeSetId = set.AttributeSetId,
                                        CreateBy = "Ahancer",
                                        CreateOn = DateTime.Now,
                                        UpdateBy = "Ahancer",
                                        UpdateOn = DateTime.Now
                                    });
                                }
                                
                            }
                        }
                        db.SaveChanges();
                    }
                }
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.GetBaseException().Message);
            }
            finally
            {
                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                }
            }
        }


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