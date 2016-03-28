using Colsp.Api.Constants;
using Colsp.Model.Responses;
using System;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Colsp.Api.Helpers
{
    public static class Util
    {
        public static async Task<FileUploadRespond> SetupImage(HttpRequestMessage Request, string rootPath, string folderName
            , int minWidth, int minHeight, int maxWidth, int maxHeight, int maxSize, bool isSquare)
        {
            if (!Request.Content.IsMimeMultipartContent())
            {
                throw new Exception("In valid content multi-media");
            }
            var streamProvider = new MultipartFormDataStreamProvider(Path.Combine(rootPath, folderName));
            try
            {
                await Request.Content.ReadAsMultipartAsync(streamProvider);
            }
            catch (Exception)
            {
                throw new Exception("Image size exceeded " + maxSize + " mb");
            }
            

            string fileName = string.Empty;
            string ext = string.Empty;
            foreach (MultipartFileData fileData in streamProvider.FileData)
            {
                fileName = fileData.LocalFileName;
                bool isLogo = false;
                bool.TryParse(streamProvider.FormData["IsLogo"], out isLogo);
                if (isLogo)
                {
                    Validation.ValidateImage(fileName, 100, 100, 100, 100, Int32.MaxValue, true);
                }
                else
                {
                    Validation.ValidateImage(fileName, minWidth, minHeight, maxWidth, maxHeight, maxSize, isSquare);
                }
                
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
                    return;
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
                                throw sqlException;
                            case 18456:
                                // Login Failed
                                throw sqlException;
                            case 547:
                                // ForeignKey Violation
                                throw sqlException;
                            case 2627:
                                // Unique Index/Constriant Violation
                                var splitMessage = sqlException.Message.Split('\'');
                                if(splitMessage.Length > 1)
                                {
                                    var message = splitMessage.ElementAt(1)
                                        .Replace(Constant.UNIQUE_CONSTRAIN_PREFIX, string.Empty)
                                        .Replace(tableName, string.Empty)
                                        .Replace(Constant.UNIQUE_CONSTRAIN_DELIMETER, " ");
                                    throw new Exception(string.Concat(message," ", Constant.UNIQUE_CONSTRAIN_SURFFIX));
                                }
                                throw sqlException;
                            case 2601:
                                // Unique Index/Constriant Violation (Primary key violation)
                                throw sqlException;
                            default:
                                // throw a general DAL Exception
                                throw sqlException;
                        }
                    }
                    else
                    {
                        throw tmpException;
                    }
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
            throw new Exception("Wait sometime and try again");
        }



        public static DateTime StartOfWeek(this DateTime dt, DayOfWeek startOfWeek)
        {
            int diff = dt.DayOfWeek - startOfWeek;
            if (diff < 0)
            {
                diff += 7;
            }
            return dt.AddDays(-1 * diff).Date;
        }

    }
}