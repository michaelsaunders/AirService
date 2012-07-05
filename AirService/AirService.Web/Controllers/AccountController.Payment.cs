using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using AirService.Model;
using AirService.Model.Eway;
using AirService.Services;
using AirService.Services.Security;
using AirService.Web.Content.EmailTemplates;
using AirService.Web.Infrastructure.Filters;
using AirService.Web.ViewModels;

namespace AirService.Web.Controllers
{
    public partial class AccountController
    {
        [RequireHttps, Authorize(Roles = WellKnownSecurityRoles.VenueAdministrators)]
        public ActionResult UpdatePaymentDetail()
        {
            int venueId = ((AirServiceIdentity) this.User.Identity).VenueId;
            var venue = this._venueService.Find(venueId);
            if (!venue.IsPaidAccount)
            {
                return this.RedirectToAction("Activate");
            }

            IRebillEventDetails details = null;
            if(venue.EwayCustomerId != null)
            {
                details = this._membershipService.PaymentService.GetRebillEvents(venueId);
            }
            
            return this.View("PaymentDetail", new PaymentDetailViewModel
                                                  {
                                                      CreditCard = new CreditCard() , 
                                                      RebillDetails = details
                                                  });
        }

        [RequireHttps, HttpPost]
        [Authorize(Roles = WellKnownSecurityRoles.VenueAdministrators)]
        public ActionResult UpdatePaymentDetail(PaymentDetailViewModel viewModel)
        {
            if ( this.ModelState.IsValid)
            {
                int venueId = ((AirServiceIdentity) User.Identity).VenueId;
                var venue = this._venueService.Find(venueId);
                if (!venue.IsPaidAccount)
                {
                    return RedirectToAction("Activate");
                }

                string errorMessage;
                var result = this._membershipService.PaymentService.UpdateRebillingEvent(venueId, viewModel.CreditCard, out errorMessage);
                if (result != null)
                {
                    this.ViewBag.Title = "Credit Card Details";
                    this.ViewBag.Message = "Your credit card details are successfully updated";
                    return this.View("GenericMessage");
                }
                
                this.ModelState.AddModelError("", errorMessage);
            }

            return this.View("PaymentDetail", viewModel);
        }

        [RequireHttps, Authorize(Roles = WellKnownSecurityRoles.VenueAdministrators)]
        public ActionResult Activate(int? type)
        {
            int id = ((AirServiceIdentity) this.User.Identity).VenueId;
            Venue venue = this._venueService.FindAllIncluding(v => v.VenueTypes).Single(v => v.Id == id);
            if (venue.EwayRebillId != null && venue.IsPaidAccount)
            {
                return this.RedirectToAction("Index", "Home");
            }

            var detail = new PaymentDetailViewModel
                             {
                                 CreditCard = new CreditCard()
                             };
            return View(detail);
        }

        [RequireHttps, HttpPost]
        [Authorize(Roles = WellKnownSecurityRoles.VenueAdministrators)]
        public ActionResult Activate(PaymentDetailViewModel viewModel)
        {
            var identity = ((AirServiceIdentity) this.User.Identity);
            int id = identity.VenueId;
            Venue venue = this._venueService.FindAllIncluding(v => v.VenueTypes).Single(v => v.Id == id);
            if (venue.EwayRebillId != null && ((venue.VenueAccountType & (int)VenueAccountTypes.AccountTypeEvaluation)) == 0)
            {
                return this.RedirectToAction("Index", "Home");
            }

            if (this.ModelState.IsValid)
            {
                string errorMessage;
                var result = this._membershipService.PaymentService.UpdateRebillingEvent(venue.Id,
                                                                                         viewModel.CreditCard,
                                                                                         out errorMessage);
                if (result != null)
                {
                    venue.IsActive = true;
                    venue.VenueAccountType = (int) VenueAccountTypes.AccountTypeFull;
                    this._venueService.InsertOrUpdate(venue);
                    this._venueService.Save();
                    this.ViewBag.Title = "Your Account is Activated";
                    this.ViewBag.MessageTitle = "Congratulation";
                    this.ViewBag.Message = "Thank you. Your account is now activated.";
                    SubscriptionEmail.Send(venue, Membership.GetUser(identity.UserId));  
                    return this.View("GenericMessage");
                }
                
                this.ModelState.AddModelError("", errorMessage);
            }

            viewModel.CreditCard = new CreditCard();
            return View(viewModel);
        }

        [Authorize(Roles = WellKnownSecurityRoles.VenueAdministrators)]
        public ActionResult CancelSubscription()
        {
            return this.View("CancelSubscription");
        }

        [HttpPost]
        [ActionName("CancelSubscription")]
        [Authorize(Roles = WellKnownSecurityRoles.VenueAdministrators)]
        public ActionResult ConfirmCancelSubscription(object sendEmail)
        {
            try
            {
                var identity = (AirServiceIdentity) this.User.Identity;
                string errorMessage;
                if (!this._membershipService.SuspendSubscription(identity.VenueId, out errorMessage))
                {
                    this.ModelState.AddModelError("", errorMessage);
                }
                else
                {
                    this._membershipService.SignOut();
                    var venue = this._venueService.Find(identity.VenueId);
                    SubscriptionCancelEmail.Send(identity.Name, venue);
                    return this.RedirectToAction("Index", "Home");
                }
            }
            catch (Exception e)
            {
                Logger.Log("Unexpected error while attempting to cancel subscription", e);
                this.ModelState.AddModelError("", "Unexpected error. Please try again later.");
            }

            return this.View("CancelSubscription");
        }
    }
}