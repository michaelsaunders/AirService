using System;
using System.IO;
using System.Net;
using System.Security.Principal;
using System.ServiceModel.Security;
using System.ServiceModel.Web;
using System.Threading;
using System.Web;
using AirService.Services;
using AirService.Services.Security;

namespace AirService.WebServices
{
    public abstract class WebServiceBase
    {
        protected AirServiceVenueUserIdentity GetAuthenticatedVenueUser()
        {
            var identity = Thread.CurrentPrincipal.Identity as AirServiceVenueUserIdentity;
            if (identity == null)
            {
                throw new SecurityAccessDeniedException();
            }

            return identity;
        }

        protected AirServiceCustomerIdentity GetAuthenticatedCustomer()
        {
            var identity = Thread.CurrentPrincipal.Identity as AirServiceCustomerIdentity;
            if (identity == null)
            {
                throw new SecurityAccessDeniedException();
            }

            return identity;
        }

        protected static T Response<T>(Func<T> function, bool use200ForServiceFault = false)
        {
            try
            {
                return function();
            }
            catch (WebFaultException)
            {
                throw;
            }
            catch (ServiceFaultException faultException)
            {
                HttpStatusCode statusCode = use200ForServiceFault
                                                ? HttpStatusCode.OK
                                                : HttpStatusCode.InternalServerError;
                throw new WebFaultException<OperationResult>(
                    new OperationResult
                        {
                            ErrorCode = faultException.ErrorNumber,
                            IsError = true,
                            Message = faultException.Message,
                            Items = faultException.OperationResults
                        },
                    statusCode);
            }
            catch (FormatException)
            {
                throw new WebFaultException(HttpStatusCode.BadRequest);
            }
            catch (Exception exception)
            {
                Logger.Log("Unexpected Error", exception);
                throw new WebFaultException<OperationResult>(
                    new OperationResult
                        {
                            ErrorCode = 1004,
                            IsError = true,
                            Message = Resources.Err1004UnexpectedError
                        },
                    HttpStatusCode.InternalServerError);
            }
        }

        protected string ResolveUrl(string url)
        {
            if (url == null)
            {
                return "/";
            }

            if (url.StartsWith("/"))
            {
                return url;
            }

            if (url.StartsWith("~/"))
            {
                string appPath = HttpContext.Current.Request.ApplicationPath;
                if (string.IsNullOrWhiteSpace(appPath) || appPath == "/")
                {
                    return url.Substring(1);
                }

                if (!appPath.EndsWith("/"))
                {
                    appPath += "/";
                }

                if (url.Length > 2)
                {
                    return appPath + url.Substring(2);
                }

                return appPath;
            }

            string parentPath = Path.GetDirectoryName(HttpContext.Current.Request.Url.AbsolutePath);
            return Path.Combine(parentPath,
                                url);
        } 
    }
}