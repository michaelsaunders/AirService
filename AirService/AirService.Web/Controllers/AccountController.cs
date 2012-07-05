using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.Mvc;
using System.Web.Security;
using AirService.Model;
using AirService.Services;
using AirService.Services.Contracts;
using AirService.Services.Security;
using AirService.Web.Content.EmailTemplates;
using AirService.Web.Infrastructure;
using AirService.Web.Infrastructure.Filters;
using AirService.Web.ViewModels;

namespace AirService.Web.Controllers
{
    [ForceHttp]
    public partial class AccountController : Controller
    {
        private readonly IMembershipService _membershipService; 
        private readonly IServiceProviderService _serviceProviderService;
        private readonly IService<VenueType> _venueTypeService;
        private readonly IService<Country> _countryService;
        private readonly IStateService _stateService;
        private readonly IVenueService _venueService;

        public AccountController(IServiceProviderService serviceProviderService,
                                 IVenueService venueService,
                                 IService<VenueType> venueTypeService,
                                 IService<Country> countryService,
                                 IStateService stateService,
                                 IMembershipService membershipService)
        {
            this._serviceProviderService = serviceProviderService;
            this._venueTypeService = venueTypeService;
            this._countryService = countryService;
            this._stateService = stateService;
            this._membershipService = membershipService;
            this._venueService = venueService;
        } 

        // **************************************
        // URL: /Account/LogOn
        // **************************************

        [RequireHttps, AllowAnonymous]
        public ActionResult LogOn()
        {
            if (this.Request.IsAjaxRequest())
            {
                return this.PartialView("LogonRedirect");
            }

            return View();
        }

        [RequireHttps, AllowAnonymous, HttpPost]
        public ActionResult LogOn(LogOnModel model, string returnUrl)
        {
            string errorMessage = null;
            if (ModelState.IsValid)
            {
                if (this._membershipService.ValidateUser(model.Email, model.Password))
                {
                    MembershipUser user = this._membershipService.GetUserByEmail(model.Email);
                    // unique ID for the user
                    var userId = (Guid) user.ProviderUserKey;
                    int? venueId = _serviceProviderService.FindAll().First(u => u.UserId.Equals(userId)).VenueId;
                    if (venueId.HasValue)
                    {
                        Venue venue = this._venueService.Find(venueId.Value);
                        if (venue.Status == SimpleModel.StatusActive)
                        {
                            this._membershipService.SignIn(this.HttpContext,
                                                           model.Email,
                                                           model.RememberMe,
                                                           String.Format("{0}|{1}", userId, venueId));

                            if (!venue.IsPaidAccount)
                            {
                                return this.RedirectToAbsoluteUri("Activate", "Account", false);
                            }

                            if (Url.IsLocalUrl(returnUrl))
                            {
                                return Redirect(returnUrl);
                            }

                            return this.RedirectToAbsoluteUri("Index", "Home", false);
                        }
                    }
                    
                    if(this._membershipService.IsUserInRole(user.UserName, WellKnownSecurityRoles.SystemAdministrators))
                    {
                        this._membershipService.SignIn(this.HttpContext,
                                                       model.Email,
                                                       model.RememberMe,
                                                       String.Format("{0}|{1}", userId, 0));
                        return this.RedirectToAction("Index", "AdminHome", new {area = "Admin"});
                    }
                }
                else
                {
                    var member = Membership.GetUser(model.Email);
                    if (member != null && member.IsLockedOut)
                    {
                        errorMessage = Resources.AccountController_LogOn_Locked; 
                    }
                }
            }

            errorMessage = errorMessage?? Resources.AccountController_Logon_InvalidCredential;
            ModelState.AddModelError("", errorMessage);
            return View(model);
        }

        // **************************************
        // URL: /Account/LogOff
        // **************************************

        public ActionResult LogOff()
        {
            this._membershipService.SignOut();
            return RedirectToAction("Index", "Home");
        }

        // **************************************
        // URL: /Account/Register
        // **************************************

        [RequireHttps, AllowAnonymous]
        public ActionResult Register(string email = null)
        {
            var identity = User.Identity as AirServiceIdentity;
            if (identity != null)
            {
                return this.RedirectToAction("Index", "Home");
            }

            ViewBag.PasswordLength = this._membershipService.MinPasswordLength;
            ViewBag.HideVenueValidationSummary = true;
            var model = new AccountModel
                            {
                                VenueViewModel = new VenueViewModel
                                                     {
                                                         Venue = this._venueService.Create(),
                                                         States = new List<State>(),
                                                         Countries = this._countryService.FindAll().ToList(),
                                                         AvailableVenueTypes = this._venueTypeService.FindAll().ToList()
                                                     },
                                Email = email,
                                ConfirmEmail = email,
                                CreditCard = new CreditCard()
                            };

            return View(model);
        }

        [RequireHttps, HttpPost, AllowAnonymous]
        public ActionResult Register(AccountModel accountModel)
        { 
            if (this.ModelState.IsValid)
            {
                if (!IsValidCaptcha(accountModel.recaptcha_challenge_field, accountModel.recaptcha_response_field))
                {
                    this.ModelState.AddModelError("", "Please type two words correctly as you see in the image at the bottom.");
                } 
                else
                {
                    var venue = accountModel.VenueViewModel.Venue;
                    // Attempt to register the user
                    MembershipCreateStatus createStatus = this._membershipService.CreateUser(accountModel.Email, accountModel.Password,
                                                                                             accountModel.Email);

                    if (createStatus == MembershipCreateStatus.Success)
                    {
                        MembershipUser newUser = Membership.GetUser(accountModel.Email);
                        venue.VenueAccountType = (int) VenueAccountTypes.AccountTypeEvaluation;
                        venue.PromoCode = accountModel.PromotionCode;
                        this._venueService.UpdateModelForVenueTypes(venue, accountModel.VenueViewModel.SelectedVenueTypes);
                        _venueService.InsertOrUpdate(venue);
                        _venueService.Save();

                        // set up the initial role - relies on the role being there in the first place
                        Roles.AddUserToRole(accountModel.Email, WellKnownSecurityRoles.VenueAdministrators);

                        // set up the associate table
                        var userId = (Guid) newUser.ProviderUserKey;
                        var provider = new ServiceProvider {UserId = userId, Venue = venue};

                        // save our associated information table
                        _serviceProviderService.InsertOrUpdate(provider);
                        _serviceProviderService.Save();

                        SendActivationEmail(venue, accountModel.Email, userId);
                        ViewBag.MessageTitle = "Account Created Successfully";
                        ViewBag.MessageSubTitle = "Thank you";
                        this.ViewBag.Message = Resources.AccountController_RegisterSucceeded;
                        return this.View("GenericMessage");
                    }

                    this.ModelState.AddModelError("", AccountValidation.ErrorCodeToString(createStatus));
                }
            }

            // If we got this far, something failed, redisplay form
            ViewBag.PasswordLength = this._membershipService.MinPasswordLength;
            ViewBag.HideVenueValidationSummary = true;
            var countryId = accountModel.VenueViewModel.Venue.CountryId;
            accountModel.VenueViewModel.States = countryId == 0
                                                     ? new List<State>()
                                                     : this._stateService.FindAllByCountry(countryId).ToList();
            accountModel.VenueViewModel.Countries = this._countryService.FindAll().ToList();
            accountModel.VenueViewModel.AvailableVenueTypes = this._venueTypeService.FindAll().ToList();
            return View(accountModel);
        }

        private bool IsValidCaptcha(string recaptchaChallengeField, string recaptchaResponseField)
        { 
            try
            {
                var url = ConfigurationManager.AppSettings["ReCaptchaVerificationUrl"] ?? "http://www.google.com/recaptcha/api/verify";
                var privateKey = ConfigurationManager.AppSettings["ReCaptchaPrivateKey"] ?? "6Lc6sc8SAAAAAJ3_XgBahYjXh1yLfqK0hSCfg6qY";
                var request = WebRequest.Create(url);
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                var data = new StringBuilder()
                    .Append("privatekey=").Append(this.Server.UrlEncode(privateKey))
                    .Append("&remoteip=").Append(this.Request.UserHostAddress)
                    .Append("&challenge=").Append(recaptchaChallengeField)
                    .Append("&response=").Append(recaptchaResponseField).ToString();
                var bytes = Encoding.UTF8.GetBytes(data);

                request.ContentLength = bytes.Length;
                var requestStream = request.GetRequestStream();
                requestStream.Write(bytes, 0, bytes.Length);
                requestStream.Close();
                var response = request.GetResponse();
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    return reader.ReadToEnd().Trim().StartsWith("true");
                }
            }
            catch(Exception e)
            {
                Debug.Write(e.Message);
                return false;
            }
        }

        // **************************************
        // URL: /Account/ChangePassword
        // **************************************

        [Authorize, RequireHttps]
        public ActionResult ChangePassword()
        {
            ViewBag.PasswordLength = this._membershipService.MinPasswordLength;
            return View();
        }

        [Authorize, RequireHttps, HttpPost]
        public ActionResult ChangePassword(ChangePasswordModel model)
        {
            if (ModelState.IsValid)
            {
                if (this._membershipService.ChangePassword(User.Identity.Name, model.OldPassword, model.NewPassword))
                {
                    return RedirectToAction("ChangePasswordSuccess");
                }
                
                this.ModelState.AddModelError("", "The current password is incorrect or the new password is invalid.");
            }

            // If we got this far, something failed, redisplay form
            ViewBag.PasswordLength = this._membershipService.MinPasswordLength;
            return View(model);
        }

        [Authorize(Roles = WellKnownSecurityRoles.VenueAdministrators)]
        public ActionResult Edit()
        {
            var airServiceIdentity = (AirServiceIdentity) this.User.Identity;
            var venueId = airServiceIdentity.VenueId; 
            Venue venue = this._venueService.FindAllIncluding(v => v.VenueTypes).Single(v => v.Id == venueId);
            return this.View(new VenueViewModel
                                 {
                                     Venue = venue,
                                     States = this._stateService.FindAllByCountry(venue.CountryId).ToList(),
                                     Countries = this._countryService.FindAll().ToList(),
                                     AvailableVenueTypes = this._venueTypeService.FindAll().ToList(),
                                     SelectedVenueTypes = venue.VenueTypes.Select(v => v.Id).ToArray()
                                 });
        }

        [Authorize(Roles = WellKnownSecurityRoles.VenueAdministrators)]
        [HttpPost]
        public ActionResult Edit(VenueViewModel viewModel)
        {
            if (this.ModelState.IsValid)
            {
                var airServiceIdentity = (AirServiceIdentity)this.User.Identity; 
                Venue venue = this._venueService.Find(airServiceIdentity.VenueId);
                UpdateModel(venue, "Venue");
                this._venueService.UpdateModelForVenueTypes(venue, viewModel.SelectedVenueTypes);
                this._venueService.InsertOrUpdate(venue);
                this._venueService.Save();
                return this.RedirectToAction("Index", "Home");
            }

            viewModel.States = this._stateService.FindAllByCountry(viewModel.Venue.CountryId).ToList();
            viewModel.Countries = this._countryService.FindAll().ToList();
            viewModel.AvailableVenueTypes = this._venueTypeService.FindAll().ToList();
            return this.View(viewModel);
        }

        // **************************************
        // URL: /Account/ChangePasswordSuccess
        // **************************************

        public ActionResult ChangePasswordSuccess()
        {
            this.ViewBag.Title = "Change Password"; 
            this.ViewBag.MessageTitle = "Password Changed";
            this.ViewBag.Message = Resources.AccountController_ChangePasswordSuccess;
            return this.View("GenericMessage");
        }

        // **************************************
        // URL: /Account/ForgottenPassword
        // 

        public ActionResult ForgottenPassword()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ForgottenPassword(ForgottenPasswordModel model)
        {
            this.ViewBag.Title = "Forgotten Password";
            this.ViewBag.MessageTitle = "Forgotten Password";
            string templatePath = this.Request.MapPath("~/Content/EmailTemplates/forgotPassword.htm");
            try
            {
                this._membershipService.ResetPassword(model.EmailAddress, templatePath);
                this.ViewBag.Message = Resources.AccountController_ForgottenPasswordSuccess;
            }
            catch(ApplicationException e)
            {
                this.ViewBag.Message = e.Message;  
            }

            return View("GenericMessage");
        } 

        public ActionResult ConfirmEmail(string id)
        {
            Guid providerId;
            if (id == null || !Guid.TryParse(id, out providerId))
            {
                this.TempData["tempMessage"] = "The user account is not valid. Please try clicking the link in your email again.";
                return this.RedirectToAction("LogOn");
            }
            
            MembershipUser user = this._membershipService.GetUser(providerId);

            if (!user.IsApproved)
            {
                user.IsApproved = true;
                Membership.UpdateUser(user);
                var serviceProvider = this._serviceProviderService.FindByUserId(new Guid(id));
                var venueId = serviceProvider.VenueId;
                this._membershipService.SignIn(this.HttpContext,
                                               user.UserName,
                                               false,
                                               String.Format("{0}|{1}", id, venueId));
                SignUpConfirmationEmail.Send(serviceProvider.Venue, user);
                return this.RedirectToAction("Activate", "Account");
            }

            this._membershipService.SignOut();
            this.TempData["tempMessage"] = "You have already confirmed your email address. Please log in to continue.";
            return this.RedirectToAction("LogOn");
        }
         
        private void SendActivationEmail(Venue venue, string email, Guid userId)
        {
            var activationUri = new Uri(String.Format("{0}/Account/ConfirmEmail?id={1}",
                                                      this.Request.Url.GetLeftPart(UriPartial.Authority), userId)); 
            try
            {
                // send the confirmation email
                string templatePath = this.Request.MapPath("~/Content/EmailTemplates/registration.htm");
                this._membershipService.SendAccountConfirmationEmail(email, templatePath, activationUri); 
            }
            catch (Exception e)
            {
                Logger.Log("Failed to send registration Email", e);
            }

            RegistrationNotificationEmail.Send(venue, activationUri.ToString(), email);
        }
    }
}
