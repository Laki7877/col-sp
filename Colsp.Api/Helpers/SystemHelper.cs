using Colsp.Api.Constants;
using Colsp.Entity.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;

namespace Colsp.Api.Helpers
{
    public static class SystemHelper
    {
        public static void InstantiateObject(object obj)
        {
            foreach (var propertyInfo in obj.GetType().GetProperties())
            {
                if (propertyInfo.PropertyType == typeof(string))
                {
                    propertyInfo.SetValue(obj, Convert.ChangeType(string.Empty, propertyInfo.PropertyType), null);
                }
            }
        }

        public static void DbSaveChange(ColspEntities db, string tableName)
        {
            int retryCount = 0;
            while (retryCount < Constant.MAX_RETRY_DEADLOCK)
            {
                try
                {
                    db.SaveChanges();
                    return;
                }
                catch (Exception e)
                {
                    if (e is DbEntityValidationException)
                    {
                        HashSet<string> errormessages = new HashSet<string>();
                        var validationException = (DbEntityValidationException)e;
                        foreach (var entityValidationErrors in validationException.EntityValidationErrors)
                        {
                            foreach (var validationError in entityValidationErrors.ValidationErrors)
                            {
                                errormessages.Add(string.Format(validationError.ErrorMessage));
                            }
                        }
                        throw new Exception(string.Join("<br>", errormessages));
                    }
                    else if (e is DbUpdateException)
                    {
                        var tmpException = e.GetBaseException();
                        if (tmpException is SqlException)
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
                                    if (splitMessage.Length > 1)
                                    {
                                        var message = splitMessage.ElementAt(1)
                                            .Replace(Constant.UNIQUE_CONSTRAIN_PREFIX, string.Empty)
                                            .Replace(tableName, string.Empty)
                                            .Replace(Constant.UNIQUE_CONSTRAIN_DELIMETER, " ");
                                        throw new Exception(string.Concat(message, " ", Constant.UNIQUE_CONSTRAIN_SURFFIX));
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
                    }
                }
           }
        }

        public static void DeadlockRetry<T>(Func<T> repositoryMethod, string tableName)
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
                    if (tmpException is SqlException)
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
                                if (splitMessage.Length > 1)
                                {
                                    var message = splitMessage.ElementAt(1)
                                        .Replace(Constant.UNIQUE_CONSTRAIN_PREFIX, string.Empty)
                                        .Replace(tableName, string.Empty)
                                        .Replace(Constant.UNIQUE_CONSTRAIN_DELIMETER, " ");
                                    throw new Exception(string.Concat(message, " ", Constant.UNIQUE_CONSTRAIN_SURFFIX));
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


		public static string SendRequest(string url, string method, Dictionary<string, string> headers, string jsonString
			, string email, DateTime currentDt, string source, string destination, ColspEntities db)
		{
			string responseFromServer = string.Empty;
			int statusCodeValue = 200;
			try
			{
				WebRequest request = WebRequest.Create(url);
				if (headers != null)
				{
					foreach (var header in headers)
					{
						request.Headers.Add(header.Key, header.Value);
					}
				}
				request.Method = method;
				byte[] byteArray = Encoding.UTF8.GetBytes(jsonString);
				request.ContentType = ContentType.ApplicationJson;
				request.ContentLength = byteArray.Length;
				using (Stream dataStream = request.GetRequestStream())
				{
					dataStream.Write(byteArray, 0, byteArray.Length);
					using (var response = request.GetResponse())
					{
						using (Stream responseDataStream = response.GetResponseStream())
						{
							using (StreamReader reader = new StreamReader(responseDataStream))
							{
								responseFromServer = reader.ReadToEnd();
								var statusCode = ((HttpWebResponse)response).StatusCode;
								statusCodeValue = (int)statusCode;
							}
						}
					}
				}
			}
			catch (WebException ex)
			{
				using (var response = ex.Response)
				{
					using (var stream = response.GetResponseStream())
					{
						using (var reader = new StreamReader(stream))
						{
							responseFromServer = reader.ReadToEnd();
							var statusCode = ((HttpWebResponse)response).StatusCode;
							statusCodeValue = (int)statusCode;
						}
					}
				}
			}
			finally
			{
				db.ApiLogs.Add(new ApiLog()
				{
					LogId = db.GetNextAppLogId().SingleOrDefault().Value,
					CreateBy = email,
					CreateOn = currentDt,
					DestinationApp = destination,
					SourceApp = source,
					Method = method,
					RequestData = jsonString,
					RequestUrl = url,
					ResponseData = responseFromServer,
					ResponseCode = statusCodeValue,
				});
			}
			return responseFromServer;
		}
	}

    
}