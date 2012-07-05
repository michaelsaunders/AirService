using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IdentityModel.Claims;
using System.IdentityModel.Policy;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Security;
using System.ServiceModel.Web;
using System.Text;
using System.Web;
using System.Web.Security;
using AirService.Model;
using AirService.Services;
using AirService.Services.Contracts;
using AirService.Services.Security;
using Ninject;
using Ninject.Extensions.Wcf;

namespace AirService.WebServices.Framework
{
    /// <summary>
    ///   [BasicAuth("*")] means all authenticated users
    ///   [BasicAuth("?")] means all users including anonymous
    ///   [BasicAuth("Administrator, Venu Administrator")] list of allowed roles
    ///   [BasicAuth] is equivant to [BasicAuth("*")]
    /// 
    ///   To see how it works; 
    ///   http://msdn.microsoft.com/en-us/magazine/cc163382.aspx
    ///   http://www.netframeworkdev.com/windows-communication-foundation/methodlevel-authorization-in-a-wcf-rest-service-49338.shtml
    /// 
    ///   BASIC HTTP Authentication implementation code from;
    ///   Pablo M. Cibraro http://weblogs.asp.net/cibrax/
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public class BasicAuthAttribute : Attribute, IServiceBehavior, IOperationBehavior, IDispatchMessageInspector,
                                      IParameterInspector
    {
        public const string DeviceCredentialUpdateAction = "venue/device/updatecredential";
        private bool _allowAllAuthenicatedUsers;
        private bool _allowAnonymous;
        private string[] _roles;

        public BasicAuthAttribute()
        {
        }

        public BasicAuthAttribute(string roles)
        {
            this.Roles = roles;
            this.Realm = "SecuredArea";
        }

        public string Roles
        {
            get;
            private set;
        }

        public string Realm
        {
            get;
            set;
        }

        #region IDispatchMessageInspector Members

        object IDispatchMessageInspector.AfterReceiveRequest(ref Message request,
                                                             IClientChannel channel,
                                                             InstanceContext instanceContext)
        {
            this.Authorize();
            return null;
        }

        void IDispatchMessageInspector.BeforeSendReply(ref Message reply,
                                                       object correlationState)
        {
        }

        #endregion

        #region IOperationBehavior Members

        void IOperationBehavior.Validate(OperationDescription operationDescription)
        {
            this.ValidateRoles();
        }

        void IOperationBehavior.ApplyDispatchBehavior(OperationDescription operationDescription,
                                                      DispatchOperation dispatchOperation)
        {
            dispatchOperation.ParameterInspectors.Add(this);
        }

        void IOperationBehavior.ApplyClientBehavior(OperationDescription operationDescription,
                                                    ClientOperation clientOperation)
        {
        }

        void IOperationBehavior.AddBindingParameters(OperationDescription operationDescription,
                                                     BindingParameterCollection bindingParameters)
        {
        }

        #endregion

        #region IParameterInspector Members

        object IParameterInspector.BeforeCall(string operationName,
                                              object[] inputs)
        {
            this.Authorize();
            return null;
        }

        void IParameterInspector.AfterCall(string operationName,
                                           object[] outputs,
                                           object returnValue,
                                           object correlationState)
        {
        }

        #endregion

        #region IServiceBehavior Members

        void IServiceBehavior.Validate(ServiceDescription serviceDescription,
                                       ServiceHostBase serviceHostBase)
        {
            this.ValidateRoles();
        }

        void IServiceBehavior.AddBindingParameters(ServiceDescription serviceDescription,
                                                   ServiceHostBase serviceHostBase,
                                                   Collection<ServiceEndpoint> endpoints,
                                                   BindingParameterCollection bindingParameters)
        {
        }

        void IServiceBehavior.ApplyDispatchBehavior(ServiceDescription serviceDescription,
                                                    ServiceHostBase serviceHostBase)
        {
            foreach (ChannelDispatcher channelDispatcher in serviceHostBase.ChannelDispatchers)
            {
                foreach (EndpointDispatcher endpoint in channelDispatcher.Endpoints)
                {
                    endpoint.DispatchRuntime.MessageInspectors.Add(this);
                }
            }
        }

        #endregion

        private void ValidateRoles()
        {
            if (this.Roles == null || this.Roles.Trim().Length == 0)
            {
                this._allowAllAuthenicatedUsers = true;
                return;
            }

            string roleNames = this.Roles.Trim();
            if (roleNames.Length == 1)
            {
                if (roleNames == "?")
                {
                    this._allowAnonymous = true;
                    return;
                }

                if (roleNames == "*")
                {
                    this._allowAllAuthenicatedUsers = true;
                    return;
                }
            }

            if (roleNames.Contains("*") || roleNames.Contains("?"))
            {
                throw new InvalidOperationException("* or ? only can be used exclusively.");
            }

            this._roles = roleNames.Split(new[]
                                              {
                                                  ","
                                              },
                                          StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < this._roles.Length; i++)
            {
                this._roles[i] = this._roles[i].Trim();
            }
        }

        private string[] ExtractCredentials(Message requestMessage)
        {
            var request = (HttpRequestMessageProperty) requestMessage.Properties[HttpRequestMessageProperty.Name];
            string authHeader = request.Headers["Authorization"];

            if (authHeader != null && authHeader.StartsWith("Basic",
                                                            StringComparison.CurrentCultureIgnoreCase))
            {
                string encodedUserPass = authHeader.Substring(6).Trim();

                Encoding encoding = Encoding.GetEncoding("iso-8859-1");
                string userPass = encoding.GetString(Convert.FromBase64String(encodedUserPass));
                int separator = userPass.IndexOf(':');

                var credentials = new string[2];
                credentials[0] = userPass.Substring(0,
                                                    separator);
                credentials[1] = userPass.Substring(separator + 1);

                return credentials;
            }

            return null;
        }

        private IPrincipal AuthenticateUser(Message requestMessage, string userName, string password)
        {
            // are we checking for a customer role?
            IKernel iocContainer = KernelContainer.Kernel;
            if (this._roles.FirstOrDefault(r => r == WellKnownSecurityRoles.Customers) != null)
            {
                var customerService = iocContainer.Get<ICustomerService>();
                Customer customer = customerService.FindByUdid(userName);
                if (customer != null)
                {
                    if (customer.Status != SimpleModel.StatusActive)
                    {
                        throw new SecurityAccessDeniedException();
                    }

                    return new GenericPrincipal(new AirServiceCustomerIdentity(userName,
                                                                               customer),
                                                new[]
                                                    {
                                                        WellKnownSecurityRoles.Customers
                                                    });
                }
            }

            if (this._roles.FirstOrDefault(r => r == WellKnownSecurityRoles.VenueUsers) != null)
            {
                string[] passwordsParts = password.Split('/');
                int venueId;
                if (passwordsParts.Length != 2 || !int.TryParse(passwordsParts[0],
                                                                out venueId))
                {
                    return null;
                }

                var iPadService = iocContainer.Get<IIPadService>();
                iPad iPad = iPadService.FindByVenueIdAndPin(venueId,
                                                            passwordsParts[1]);
                if (iPad == null)
                {
                    return null;
                }

                if (iPad.Udid != userName)
                {
                    if (!string.IsNullOrWhiteSpace(iPad.Udid))
                    {
                        string credentialUpdateAction = HttpRuntime.AppDomainAppVirtualPath;
                        if (credentialUpdateAction == null || !credentialUpdateAction.EndsWith("/"))
                        {
                            credentialUpdateAction += "/";
                        }

                        credentialUpdateAction += DeviceCredentialUpdateAction;
                        string currentPath =
                            OperationContext.Current.RequestContext.RequestMessage.Headers.To.AbsolutePath;
                        // if device is data already associated with an UDID, throw error unless we are forcefully updating the UDID 
                        bool foceUpdate = string.Compare(credentialUpdateAction,
                                                         currentPath,
                                                         StringComparison.
                                                             InvariantCultureIgnoreCase) == 0;

                        if (!foceUpdate)
                        {
                            throw new WebFaultException<OperationResult>(
                                new OperationResult
                                    {
                                        ErrorCode = 1014,
                                        IsError = true,
                                        Message = Resources.Err1014DevicePinAlreadyUsedByAnotherDevice
                                    },
                                HttpStatusCode.InternalServerError);
                        }
                    }

                    iPad.Udid = userName;
                    iPadService.Update(iPad);
                }

                return new GenericPrincipal(new AirServiceVenueUserIdentity(userName,
                                                                            iPad),
                                            new[]
                                                {
                                                    WellKnownSecurityRoles.VenueUsers
                                                });
            }

            // otherwise we need a valid user name and password. 
            if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(password))
            {
                return null;
            }

            try
            {
                var request = (HttpRequestMessageProperty)requestMessage.Properties[HttpRequestMessageProperty.Name];
                var udid = request.Headers["Device-ID"];
                var ipad = string.IsNullOrWhiteSpace(udid) ? null : iocContainer.Get<IIPadService>().FindByUdid(udid);
                if (this._roles.FirstOrDefault(r => r == WellKnownSecurityRoles.VenueAdministrators) != null)
                {
                    var membshipService = iocContainer.Get<IMembershipService>();
                    MembershipUser user = membshipService.GetUser(userName,
                                                                  password);
                    if (user != null)
                    {
                        AirServiceVenueUserIdentity identity;
                        var providerUserKey = (Guid) user.ProviderUserKey;
                        var service = KernelContainer.Kernel.Get<IServiceProviderService>();
                        var venue = service.GetVenueByVenueAdminUserName(providerUserKey);
                        if (ipad != null && ipad.VenueId == venue.Id)
                        {
                            identity = new AirServiceVenueUserIdentity(userName, ipad);
                        }
                        else
                        {
                            identity = new AirServiceVenueUserIdentity(userName, venue);
                        }

                        identity.ProviderKey = providerUserKey;
                        return new GenericPrincipal(identity,
                                                    membshipService.GetUserRoles(userName));
                    }
                }

                if (this._roles.FirstOrDefault(r => r == WellKnownSecurityRoles.DeviceAdministrators) != null)
                {
                    var service = KernelContainer.Kernel.Get<IDeviceAdminService>();
                    var deviceAdmin = service.GetDeviceAdmin(userName,
                                                             password);

                    if (deviceAdmin != null)
                    {
                        ipad = ipad ?? deviceAdmin.iPads.FirstOrDefault();
                        return new GenericPrincipal(new AirServiceVenueUserIdentity(userName,
                                                                                    ipad),
                                                    new[]
                                                        {
                                                            WellKnownSecurityRoles.DeviceAdministrators
                                                        });
                    }
                }
            }
            catch(Exception e)
            {
                Logger.Log("Exception thrown while attempting to authenticating user", e);
            }

            return null;
        }

        private void Authorize()
        {
            if (this._allowAnonymous)
            {
                return;
            }

            OperationContext operationContext = OperationContext.Current;
            RequestContext requestContext = operationContext.RequestContext;
            string[] credentials = this.ExtractCredentials(requestContext.RequestMessage);
            if (credentials != null)
            {
                IPrincipal principal = this.AuthenticateUser(requestContext.RequestMessage,
                                                             credentials[0],
                                                             credentials[1]);
                if (principal != null)
                {
                    var request = (HttpRequestMessageProperty) requestContext.RequestMessage.Properties[HttpRequestMessageProperty.Name];
                    string secondsFromGmt = request.Headers["SecondsFromGMT"];
                    if (secondsFromGmt != null)
                    {
                        int seconds;
                        if (int.TryParse(secondsFromGmt, out seconds))
                        {
                            ((AirServiceIdentity) principal.Identity).SecondsFromGmt = seconds;
                        }
                    }

                    var policies = new List<IAuthorizationPolicy>
                                       {
                                           new PrincipalAuthorizationPolicy(principal)
                                       };

                    var securityContext = new ServiceSecurityContext(policies.AsReadOnly());
                    Message requestMessage = requestContext.RequestMessage;
                    if (requestMessage.Properties.Security != null)
                    {
                        requestMessage.Properties.Security.ServiceSecurityContext = securityContext;
                    }
                    else
                    {
                        requestMessage.Properties.Security = new SecurityMessageProperty
                                                                 {
                                                                     ServiceSecurityContext = securityContext
                                                                 };
                    }

                    if (this._allowAllAuthenicatedUsers)
                    {
                        return;
                    }

                    foreach (string roleName in this._roles)
                    {
                        if (principal.IsInRole(roleName.Trim()))
                        {
                            return;
                        }
                    }

                    throw new SecurityAccessDeniedException();
                }
            }

            WebOperationContext.Current.OutgoingResponse.Headers.Add("WWW-Authenticate",
                                                                     String.Format("Basic realm=\"{0}\"",
                                                                                   this.Realm));
            throw new WebFaultException(HttpStatusCode.Unauthorized);
        }

        #region Nested type: PrincipalAuthorizationPolicy

        private class PrincipalAuthorizationPolicy : IAuthorizationPolicy
        {
            private readonly string _id = Guid.NewGuid().ToString();
            private readonly IPrincipal _user;

            public PrincipalAuthorizationPolicy(IPrincipal user)
            {
                this._user = user;
            }

            #region IAuthorizationPolicy Members

            public ClaimSet Issuer
            {
                get
                {
                    return ClaimSet.System;
                }
            }

            public string Id
            {
                get
                {
                    return this._id;
                }
            }

            public bool Evaluate(EvaluationContext evaluationContext,
                                 ref object state)
            {
                evaluationContext.AddClaimSet(this,
                                              new DefaultClaimSet(Claim.CreateNameClaim(this._user.Identity.Name)));
                evaluationContext.Properties["Identities"] = new List<IIdentity>(new[]
                                                                                     {
                                                                                         this._user.Identity
                                                                                     });
                evaluationContext.Properties["Principal"] = this._user;
                return true;
            }

            #endregion
        }

        #endregion
    }
}