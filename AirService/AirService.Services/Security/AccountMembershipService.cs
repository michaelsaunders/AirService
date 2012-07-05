using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.Security;
using AirService.Data.Contracts;
using AirService.Model;
using AirService.Services.Contracts;

namespace AirService.Services.Security
{
    public class AccountMembershipService : IMembershipService
    {
        private readonly IRepository<Venue> _repository;
        private readonly MembershipProvider _provider;

        public AccountMembershipService(IPaymentService paymentService, IRepository<Venue> repository)
            : this(paymentService, repository, null)
        {
        }

        public AccountMembershipService(IPaymentService paymentService, IRepository<Venue> repository, MembershipProvider provider)
        {
            this.PaymentService = paymentService;
            this._repository = repository;
            this._provider = provider ?? Membership.Provider;
        }

        public int MinPasswordLength
        {
            get { return this._provider.MinRequiredPasswordLength; }
        }

        public IPaymentService PaymentService { get; private set; }
        
        public bool ValidateUser(string userName, string password)
        {
            if (String.IsNullOrEmpty(userName)) throw new ArgumentException("Value cannot be null or empty.", "userName");
            if (String.IsNullOrEmpty(password)) throw new ArgumentException("Value cannot be null or empty.", "password");

            return _provider.ValidateUser(userName, password);
        }

        public MembershipUser GetUser(string userName, string password)
        {
            if (this.ValidateUser(userName,
                                  password))
            {
                return Membership.GetUser(Membership.GetUserNameByEmail(userName));
            }

            return null;
        }

        public MembershipUser GetUser(Guid userId)
        {
            return Membership.GetUser(userId,
                                      false);
        }

        public MembershipUser GetUserByEmail(string email)
        {
            return Membership.GetUser(email);
        }

        public MembershipCreateStatus CreateUser(string userName, string password, string email)
        {
            if (String.IsNullOrEmpty(userName)) throw new ArgumentException("Value cannot be null or empty.", "userName");
            if (String.IsNullOrEmpty(password)) throw new ArgumentException("Value cannot be null or empty.", "password");
            if (String.IsNullOrEmpty(email)) throw new ArgumentException("Value cannot be null or empty.", "email");

            MembershipCreateStatus status;
            _provider.CreateUser(userName, password, email, null, null, false, null, out status);
            return status;
        }

        public bool ChangePassword(string userName, string oldPassword, string newPassword)
        {
            if (String.IsNullOrEmpty(userName))
            {
                throw new ArgumentException("Value cannot be null or empty.", "userName");
            }

            if (String.IsNullOrEmpty(oldPassword))
            {
                throw new ArgumentException("Value cannot be null or empty.", "oldPassword");
            }

            if (String.IsNullOrEmpty(newPassword))
            {
                throw new ArgumentException("Value cannot be null or empty.", "newPassword");
            }

            // The underlying ChangePassword() will throw an exception rather
            // than return false in certain failure scenarios.
            try
            {
                MembershipUser currentUser = _provider.GetUser(userName, true /* userIsOnline */);
                return currentUser.ChangePassword(oldPassword, newPassword);
            }
            catch (ArgumentException)
            {
                return false;
            }
            catch (MembershipPasswordException)
            {
                return false;
            }
        }

        public string[] GetUserRoles(string userName)
        { 
            return Roles.Provider.GetRolesForUser(userName);
        }

        public void SendAccountConfirmationEmail(string emailAddress, string templatePath, Uri activationUri)
        {
            var content = System.IO.File.ReadAllText(templatePath).Replace("{activationUrl}", activationUri.ToString());
            using(var client = new SmtpClient())
            {
                var message = new MailMessage(ConfigurationManager.AppSettings["DefaultEmailFromAddress"] ?? "noreply@airservice.com.au", emailAddress)
                {
                    Subject = "Thanks for joining AirService",
                    Body = content,
                    IsBodyHtml = true
                };

                client.Send(message);
            }
        }

        public void ResetPassword(string emailAddress, string templatePath)
        {
            var users = Membership.FindUsersByEmail(emailAddress);
            if (users.Count > 0)
            {
                var enumerator = users.GetEnumerator();
                enumerator.MoveNext();
                MembershipUser user = (MembershipUser) enumerator.Current;
                if (user.IsLockedOut)
                {
                    throw new ApplicationException("Your account is locked out. Please contact us.");
                }
                
                if(!user.IsApproved)
                {
                    throw new ApplicationException("Account not found or not a valid account.");
                }

                string newPassword;
                try
                {
                    newPassword = user.ResetPassword();
                }
                catch(Exception e)
                {
                    Logger.Log("Unable to change password", e);
                    throw new ApplicationException("Failed to change pasword");
                }

                var content = System.IO.File.ReadAllText(templatePath).Replace("{password}", newPassword);
                using (var client = new SmtpClient())
                {
                    var message =
                        new MailMessage(
                            ConfigurationManager.AppSettings["DefaultEmailFromAddress"] ?? "noreply@airservice.com.au",
                            emailAddress)
                            {
                                Subject = "Forgotten password reminder",
                                Body = content,
                                IsBodyHtml = true
                            };
                    try
                    {
                        client.Send(message);
                    }
                    catch(Exception e)
                    {
                        Logger.Log(string.Format("Failed to send email to {0}", emailAddress), e);
                    }
                }

            }
        }

        public void SignIn(HttpContextBase context, string userName, bool createPersistentCookie, string userData)
        {
            if (String.IsNullOrEmpty(userName))
            {
                throw new ArgumentException("Value cannot be null or empty.", "userName");
            }

            var authenticationTicket = new FormsAuthenticationTicket(1, userName,
                                                                     DateTime.Now,
                                                                     DateTime.Now.AddMinutes(30),
                                                                     createPersistentCookie,
                                                                     userData);
            string encTicket = FormsAuthentication.Encrypt(authenticationTicket);
            context.Response.Cookies.Add(new HttpCookie(FormsAuthentication.FormsCookieName, encTicket));
        }

        public void SignOut()
        {
            FormsAuthentication.SignOut();
        }

        public bool SuspendSubscription(int venueId, out string errorMessage)
        {
            var venue = this._repository.Find(venueId);
            if (!string.IsNullOrWhiteSpace(venue.EwayRebillId))
            {
                this.PaymentService.QueryTransactions(venueId, out errorMessage);
                if (errorMessage != null)
                {
                    return false;
                }

                if (!this.PaymentService.CancelRebill(venueId, out errorMessage))
                {
                    return false;
                }

                venue.EwayRebillId = null;
            }

            venue.Status = SimpleModel.StatusFrozen;
            venue.IsActive = false;
            this._repository.Save();
            foreach (var serviceProvider in this._repository.Set<ServiceProvider>().Where(s => s.VenueId == venueId))
            {
                var user = this._provider.GetUser(serviceProvider.UserId, false);
                if (user != null && !IsUserInRole(user.UserName, WellKnownSecurityRoles.SystemAdministrators))
                {
                    user.IsApproved = false;
                    this._provider.UpdateUser(user);
                }
            }

            errorMessage = null;
            return true;
        }

        public void EnableSubscription(int venueId)
        {
            var venue = this._repository.Find(venueId);
            venue.Status = SimpleModel.StatusActive;
            venue.VenueAccountType = (int) VenueAccountTypes.AccountTypeEvaluation;
            venue.IsActive = false;
            this._repository.Save();
            foreach (var serviceProvider in this._repository.Set<ServiceProvider>().Where(s=>s.VenueId == venueId))
            {
                var user = this._provider.GetUser(serviceProvider.UserId, false);
                if (user != null)
                {
                    user.IsApproved = true;
                    if (user.IsLockedOut)
                    {
                        user.UnlockUser();
                    }

                    this._provider.UpdateUser(user);
                }
            }
        }

        public bool HasLockedAccount(int venueId)
        {
            foreach (var serviceProvider in this._repository.Set<ServiceProvider>().Where(s => s.VenueId == venueId))
            {
                var user = this._provider.GetUser(serviceProvider.UserId, false);
                if (user != null)
                {
                    if (user.IsLockedOut)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public void UnlockAllUsers(int venueId)
        {
            foreach (var serviceProvider in this._repository.Set<ServiceProvider>().Where(s => s.VenueId == venueId))
            {
                var user = this._provider.GetUser(serviceProvider.UserId, false);
                if (user != null)
                {
                    if (user.IsLockedOut)
                    {
                        user.UnlockUser();
                        this._provider.UpdateUser(user);
                    }
                }
            }
        }

        public void LoginAsVenueAdmin(HttpContextBase context, int venueId, Guid userId)
        {
            var venue = this._repository.Find(venueId);
            var sysUser = this._repository.Set<ServiceProvider>().FirstOrDefault(s => s.VenueId == venueId && s.IsSystemUser);
            MembershipUser member;
            if (sysUser == null)
            {
                var email = Guid.NewGuid().ToString("N") + "@" + Guid.NewGuid().ToString("N") + ".com";
                MembershipCreateStatus status;
                member = this._provider.CreateUser(email,
                                                       Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N"),
                                                       email,
                                                       null,
                                                       null,
                                                       true,
                                                       null,
                                                       out status);
                if (status == MembershipCreateStatus.Success)
                {
                    Roles.AddUserToRole(email, WellKnownSecurityRoles.VenueAdministrators); 
                }

                sysUser = new ServiceProvider
                              {
                                  UserId = (Guid) member.ProviderUserKey,
                                  IsSystemUser = true,
                                  Venue = venue,
                                  VenueId = venue.Id,
                                  Status = SimpleModel.StatusActive
                              };

                this._repository.Insert(sysUser);  
                this._repository.Save();
            }
            else
            {
                member = this._provider.GetUser(sysUser.UserId, true);
            }

            this.SignIn(context,
                        member.Email,
                        false,
                        String.Format("{0}|{1}|{2}|{3}", sysUser.UserId, venueId, "System User", userId.ToString("N")));
        }

        public void LoginAsSystemAdmin(HttpContextBase context, Guid adminUserId)
        {
            MembershipUser member = this._provider.GetUser(adminUserId, false);
            if (!this.IsUserInRole(member.UserName, WellKnownSecurityRoles.SystemAdministrators))
            {
                return;
            }

            this.SignIn(context,
                        member.Email,
                        false,
                        String.Format("{0}|{1}", adminUserId, 0));
        }

        public bool IsUserInRole(string userName, string roleName)
        {
            return Roles.IsUserInRole(userName, roleName);
        }

        public List<MembershipUser> GetVenueAdmins(int venueId)
        {
            var members = new List<MembershipUser>();
            var providers = this._repository.Set<ServiceProvider>().Where(sp => sp.VenueId == venueId && !sp.IsSystemUser);
            foreach (var provider in providers)
            {
                var member = this._provider.GetUser(provider.UserId, false);
                if(member != null)
                {
                    members.Add(member);
                }
            }

            return members;
        }
    }
}