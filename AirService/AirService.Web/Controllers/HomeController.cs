using System;
using System.Configuration;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Web.Mvc;
using AirService.Model;
using AirService.Services.Contracts;
using AirService.Services.Security;
using AirService.Web.Infrastructure.Filters;
using AirService.Web.ViewModels;

namespace AirService.Web.Controllers
{
    [AllowAnonymous, ForceHttp]
    public class HomeController : Controller
    {
        private readonly string EmailFrom = ConfigurationManager.AppSettings["ContactUsEmailFrom"];
        private readonly string EmailTo = ConfigurationManager.AppSettings["ContactUsEmailTo"];
        private readonly string EmailCC = ConfigurationManager.AppSettings["ContactUsEmailCC"];
        private readonly IVenueService _venueService;
        private readonly IMembershipService _membershipService;

        public HomeController(IVenueService venueService, IMembershipService membershipService)
        {
            this._venueService = venueService;
            this._membershipService = membershipService;
        }

        public ActionResult Index()
        {
            ViewBag.Message = "Welcome to AirService!"; 
            var identity = User.Identity as AirServiceIdentity;
            if (identity != null && identity.IsAuthenticated && identity.VenueId > 0)
            {
                if (User.IsInRole("Venue Administrator"))
                {
                    var venue = _venueService.FindAllIncluding(v => v.VenueTypes).Single(v => v.Id == identity.VenueId);
                    if(venue.IsPaidAccount)
                    {
                        return View("VenueManagement");
                    }

                    return this.RedirectToAction("Activate", "Account");
                }
            }

            return View("PublicIndex");
        }

        public ActionResult Page(string id)
        {
            var filePath = string.Format(Server.MapPath("~/Content/Custom/html/{0}.htm"), id);
            if (!System.IO.File.Exists(filePath))
            {
                return RedirectToAction("Index", "Home");
            }

            this.ViewBag.FilePath = filePath;
            return this.View();
        }

        [HttpPost]
        [AjaxOnly]
        public ActionResult ContactUs(ContactUsInput formData)
        {
            using (var smtpClient = new SmtpClient())
            {
                try
                {
                    var message = new StringBuilder();
                    message.Append("Form Type: ").AppendLine(formData.FormType);
                    message.Append("Name: ").AppendLine(formData.Name);
                    message.Append("Email: ").AppendLine(formData.Email);
                    message.Append("Subject: ").AppendLine(formData.Subject);
                    message.Append("Type: ").AppendLine(formData.Type);
                    message.Append("Message: ").AppendLine(formData.Message);
                    message.Append("Email Subscribe: ").Append(formData.ReceiveSpecialOffers);
                    var mailMessage = new MailMessage(EmailFrom, EmailTo, formData.Subject ?? formData.Type,
                                                      message.ToString());
                    if (EmailCC != null)
                    {
                        mailMessage.CC.Add(EmailCC);
                    }

                    smtpClient.Send(mailMessage);
                }
                catch
                {
                }
            }

            return Json(true);
        }

        public ActionResult Location()
        {
            var connection = this._venueService.GetVenueConnectionByRandom();
            if (connection == null)
            {
                return this.RedirectToAction("Index", "Home");
            }

            return this.View("Location", new LocationViewModel(connection));
        }

        [Authorize(Roles = WellKnownSecurityRoles.VenueAdministrators)]
        public ActionResult Admin()
        {
            var identity = (AirServiceIdentity) this.User.Identity;
            if (!identity.AdminUserId.HasValue)
            {
                return RedirectToAction("Index");
            }

            this._membershipService.LoginAsSystemAdmin(this.HttpContext, identity.AdminUserId.Value);
            return RedirectToAction("Index", "AdminHome", new {area = "Admin"});
        }
    }
}