using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Security;
using System.ServiceModel.Web;
using System.Threading;
using System.Web;
using AirService.Model;
using AirService.Services;
using AirService.Services.Contracts;
using AirService.Services.Security;
using AirService.WebServices.Framework;
using AirService.WebServices.Models;
using Ninject;
using Ninject.Extensions.Wcf;

namespace AirService.WebServices
{
    [ServiceContract(Namespace = "urn:airservice:customer")]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class CustomerWebService : WebServiceBase
    {
        [BasicAuth(WellKnownSecurityRoles.Customers)]
        [WebInvoke(Method = "POST", UriTemplate = "Update", BodyStyle = WebMessageBodyStyle.Bare,
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json)]
        public Customer Update(Customer customerToUpdate)
        {
            return Response(() =>
                                {
                                    var customer = this.GetAuthenticatedCustomer().Customer;
                                    var service = KernelContainer.Kernel.Get<ICustomerService>();
                                    customer.FirstName = customerToUpdate.FirstName;
                                    customer.LastName = customerToUpdate.LastName;
                                    customer.Email = customerToUpdate.Email;
                                    customer.Mobile = customerToUpdate.Mobile;
                                    customer.FacebookId = customerToUpdate.FacebookId;
                                    customer.ReceiveEmailNotification = customerToUpdate.ReceiveEmailNotification;
                                    customer.ReceiveSpecialOffers = customerToUpdate.ReceiveSpecialOffers;
                                    service.Update(customer);
                                    return customer;
                                });
        }

        /// <summary>
        ///   Customers only can access thier own information.
        /// </summary>
        /// <returns></returns>
        [WebGet(UriTemplate = "", ResponseFormat = WebMessageFormat.Json)]
        [BasicAuth(WellKnownSecurityRoles.Customers)]
        public Customer GetCustomerInformation()
        {
            return Response(() => this.GetAuthenticatedCustomer().Customer);
        }

        [WebInvoke(Method = "POST", UriTemplate = "", BodyStyle = WebMessageBodyStyle.Bare,
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json)]
        public Customer Insert(Customer customer)
        {
            return Response(() =>
                                {
                                    customer.Id = 0; //ensure id isn't set
                                    var service = KernelContainer.Kernel.Get<ICustomerService>();
                                    return service.Insert(customer);
                                });
        }

        [BasicAuth(WellKnownSecurityRoles.Customers)]
        [WebInvoke(Method = "POST", UriTemplate = "UpdatePhoto/{fileName}", BodyStyle = WebMessageBodyStyle.Bare,
            ResponseFormat = WebMessageFormat.Json)]
        public OperationResult UpdatePhoto(string fileName, Stream photoStream)
        {
            return Response(() =>
                                {
                                    var customer = this.GetAuthenticatedCustomer().Customer;
                                    string extension = Path.GetExtension(fileName);
                                    if (extension != null)
                                    {
                                        switch (extension.ToLower())
                                        {
                                            case ".bmp":
                                            case ".jpg":
                                            case ".png":
                                            case ".gif":
                                                break;
                                            default:
                                                extension = null;
                                                break;
                                        }
                                    }

                                    if (extension == null)
                                    {
                                        return new OperationResult
                                                   {
                                                       ErrorCode = 2001,
                                                       IsError = true,
                                                       Message = Resources.Err2001InvalidImageFile
                                                   };
                                    }

                                    HttpContext httpContext = HttpContext.Current;
                                    string profileImagePath =
                                        this.ResolveUrl(ConfigurationManager.AppSettings["profileImageFolder"]);
                                    string profileImageFolder = httpContext.Server.MapPath(profileImagePath);

                                    // to secure UDID, made customer photo image as random guid. 
                                    var photoFileName = Guid.NewGuid().ToString("N") + extension;
                                    string path = Path.Combine(profileImageFolder,
                                                               photoFileName);

                                    try
                                    {
                                        var bitmap = new Bitmap(photoStream);
                                        bitmap.Save(path);
                                        var oldImagePath = string.IsNullOrWhiteSpace(customer.Image)
                                                               ? null
                                                               : httpContext.Server.MapPath(customer.Image);
                                        customer.Image = Path.Combine(profileImagePath,
                                                                      photoFileName).Replace('\\',
                                                                                             '/');
                                        var service = KernelContainer.Kernel.Get<ICustomerService>();
                                        service.Update(customer);
                                        if (oldImagePath != null && File.Exists(oldImagePath))
                                        {
                                            File.Delete(oldImagePath);
                                        } 

                                        return new OperationResult
                                                   {
                                                       Message = customer.Image
                                                   };
                                    }
                                    catch
                                    {
                                        return new OperationResult
                                                   {
                                                       ErrorCode = 1004,
                                                       IsError = true,
                                                       Message = Resources.Err1004UnexpectedError
                                                   };
                                    }
                                });
        }

        [BasicAuth(WellKnownSecurityRoles.VenueUsers)]
        [WebInvoke(Method = "POST", UriTemplate = "{customerId}/message/{message}",
            ResponseFormat = WebMessageFormat.Json)]
        public OperationResult SendMessage(string customerId, string message)
        {
            return Response(() =>
                                {
                                    var venueUser = this.GetAuthenticatedVenueUser();
                                    var service = KernelContainer.Kernel.Get<INotificationService>();
                                    service.SendMessageToCustomer(venueUser.Venue.Id,
                                                                  int.Parse(customerId),
                                                                  message);
                                    return new OperationResult
                                               {
                                                   Message = "OK"
                                               };
                                }); 
        }

        [WebInvoke(Method = "POST", UriTemplate = "registerForNotification", BodyStyle = WebMessageBodyStyle.Bare,
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json)]
        public OperationResult RegisterForNotification(NotificationToken token)
        {
            return Response(() =>
            { 
                //TODO: customer specific validation
                var service = KernelContainer.Kernel.Get<INotificationService>();
                service.InsertOrUpdate(token);
                return new OperationResult
                {
                    Message = "OK"
                };
            });
        }

        [BasicAuth(WellKnownSecurityRoles.Customers)]
        [WebGet(UriTemplate = "Venues", BodyStyle = WebMessageBodyStyle.Bare,
            ResponseFormat = WebMessageFormat.Json)]
        public List<CustomerVenueConnection> GetConnectedVenues()
        {
            return Response(() =>
                                {
                                    var customer = this.GetAuthenticatedCustomer().Customer;
                                    var service = KernelContainer.Kernel.Get<IVenueService>();
                                    var connections = service.GetConnectedVenueIdsForCustomer(customer.Id);
                                    return (from connection in connections
                                            select new CustomerVenueConnection
                                                       {
                                                           VenueId = connection.VenueId,
                                                           SessionTotalAmount =
                                                               connection.Orders.Where(o => o.Status != Order.OrderStatusCancelled).Select(
                                                                   o => o.TotalAmount).Sum(),
                                                           DateConnected = connection.UtcConnectedSince
                                                       }).ToList();
                                });
        }
    }
}