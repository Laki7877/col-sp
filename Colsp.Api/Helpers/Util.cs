using Colsp.Api.Constants;
using Colsp.Model.Responses;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace Colsp.Api.Helpers
{
    public static class Util
    {
        public static async Task<FileUploadRespond> SetupImage(HttpRequestMessage Request, string rootPath, string folderName
            , int minWidth, int minHeight, int maxWidth, int maxHeight, int maxSize, bool isSquare)
        {
            if (!Request.Content.IsMimeMultipartContent())
            {
                throw new Exception("Content Multimedia");
            }
            var streamProvider = new MultipartFormDataStreamProvider(Path.Combine(rootPath, folderName));
            await Request.Content.ReadAsMultipartAsync(streamProvider);

            string fileName = string.Empty;
            string ext = string.Empty;
            foreach (MultipartFileData fileData in streamProvider.FileData)
            {
                fileName = fileData.LocalFileName;
                Validation.ValidateImage(fileName, minWidth, minHeight, maxWidth, maxHeight, maxSize, isSquare);
                string tmp = fileData.Headers.ContentDisposition.FileName;
                if (tmp.StartsWith("\"") && tmp.EndsWith("\""))
                {
                    tmp = tmp.Trim('"');
                }
                ext = Path.GetExtension(tmp);
                break;
            }
            string newName = string.Concat(fileName, ext);
            File.Move(fileName, newName);
            FileUploadRespond fileUpload = new FileUploadRespond();
            var name = Path.GetFileName(newName);
            var schema = Request.GetRequestContext().Url.Request.RequestUri.Scheme;
            var imageUrl = Request.GetRequestContext().Url.Request.RequestUri.Authority;
            fileUpload.url = string.Concat(schema, "://", imageUrl, "/", AppSettingKey.IMAGE_ROOT_FOLDER, "/", folderName, "/", name);
            return fileUpload;
        }

        public static void DeadlockRetry<T>(Func<T> repositoryMethod,string tableName)
        {
            int retryCount = 0;

            while (retryCount < Constant.MAX_RETRY_DEADLOCK)
            {
                try
                {
                    repositoryMethod();
                }
                catch (DbUpdateException e)
                {
                    var tmpException = e.GetBaseException();
                    if(tmpException is SqlException)
                    {
                        var sqlException = (SqlException)tmpException;

                        switch (sqlException.Number)
                        {
                            case 1205:
                                // Deadlock
                                retryCount++;
                                break;
                            case 4060:
                                // Invalid Database
                                throw new Exception("Invalid Database");
                            case 18456:
                                // Login Failed
                                throw new Exception("Database Login Failed");
                            case 547:
                                // ForeignKey Violation
                                throw new Exception("Invalid entry in " + tableName);
                            case 2627:
                                // Unique Index/Constriant Violation
                                var splitMessage = sqlException.Message.Split('\'');
                                if(splitMessage.Length > 1)
                                {
                                    var message = splitMessage.ElementAt(1).Replace(
                                        Constant.UNIQUE_CONSTRAIN_SUFFIX 
                                        + Constant.UNIQUE_CONSTRAIN_DELIMETER 
                                        + tableName 
                                        + Constant.UNIQUE_CONSTRAIN_DELIMETER
                                        , string.Empty);
                                    message = message.Replace(Constant.UNIQUE_CONSTRAIN_DELIMETER, " ");
                                    throw new Exception(string.Concat(message," ", Constant.UNIQUE_CONSTRAIN_PREFIX));
                                }
                                throw sqlException;
                            case 2601:
                                // Unique Index/Constriant Violation (Primary key violation)
                                throw new Exception("Duplicate entry in " + tableName);
                            default:
                                // throw a general DAL Exception
                                throw sqlException;
                        }
                    }
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }

    }
}