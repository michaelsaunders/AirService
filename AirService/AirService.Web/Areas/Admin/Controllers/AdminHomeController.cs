using System;
using System.Configuration;
using System.IO;
using System.Web.Mvc;
using AirService.Services.Contracts;
using AirService.Services.Security;
using AirService.Web.Areas.Admin.Models;

namespace AirService.Web.Areas.Admin.Controllers
{
    [RequireHttps]
    [Authorize(Roles = WellKnownSecurityRoles.SystemAdministrators)]
    public class AdminHomeController : Controller
    {
        private readonly IMembershipService _membershipService;
        private readonly IPaymentService _paymentService;
        private readonly IVenueService _venueService;

        public AdminHomeController(IVenueService venueService,
                                   IMembershipService membershipService,
                                   IPaymentService paymentService)
        {
            this._venueService = venueService;
            this._membershipService = membershipService;
            this._paymentService = paymentService;
        }

        public ViewResult Index(int? id)
        {
            if (id.HasValue)
            {
                var viewModel = new VenueDetailViewModel();
                viewModel.Venue = this._venueService.Find(id.Value);
                viewModel.HasLockedUsers = this._membershipService.HasLockedAccount(id.Value);
                viewModel.ConnectedCustomers = this._venueService.GetVenueConnectionCount(id.Value);
                viewModel.VenueAdmins = this._membershipService.GetVenueAdmins(id.Value);
                return this.View("Details", viewModel);
            }

            return this.VenueList(1, false);
        }

        public ViewResult VenueList(int sortColumn, bool ascending)
        {
            var stat = this._venueService.GetStatistics();
            var venueSummaries = this._venueService.GetVenueSummaries(sortColumn,
                                                                      ascending);
            var serviceStatusFolder = GetServiceStatusFolder();
            var statusFile = Path.Combine(serviceStatusFolder, "app_offline.htm");
            var serviceDisabled = System.IO.File.Exists(statusFile);
            return this.View("Index", new AdminHomeViewModel
                                          {
                                              VenueListSortColumn = sortColumn,
                                              VenueListSortAscending = ascending,
                                              Statistics = stat.Tables[0],
                                              Venues = venueSummaries.Tables[0],
                                              WebServicesEnabled = !serviceDisabled,
                                          });
        }

        public ActionResult Transactions(int id)
        {
            var venue = this._venueService.Find(id);
            string errorMessage;
            var transactions = this._paymentService.QueryTransactions(id, out errorMessage);
            return this.PartialView("_Transactions", new TransactionViewModel(venue, transactions, errorMessage));
        }

        [HttpPost]
        public ActionResult DisableWebServicesForAllVenues()
        {
            var serviceStatusFolder = GetServiceStatusFolder();
            var offlineFile = Path.Combine(serviceStatusFolder, "app_offline.htm");
            var serviceDisabled = System.IO.File.Exists(offlineFile);
            if (!serviceDisabled)
            {
                // if possible rename file,  this is to avoid giving permission to entier folder.  
                var onlineFile = Path.Combine(serviceStatusFolder, "app_online.htm");
                if (System.IO.File.Exists(onlineFile))
                {
                    System.IO.File.Move(onlineFile, offlineFile);
                }
                else
                {
                    System.IO.File.WriteAllText(
                        offlineFile,
                        @"{
    ""ErrorCode"" : -1,
    ""IsError"" : true,
    ""Message"" : ""AirService is temporarily unavailable."",
   ""Items"" : []
}");
                }
            }

            return this.RedirectToAction("Index", new {id = ""});
        }

        [HttpPost]
        public ActionResult SuspendSubscription(int id)
        {
            string errorMessage;
            if(!this._membershipService.SuspendSubscription(id, out errorMessage))
            {
                TempData["Error"] = errorMessage;
            }
            
            return RedirectToAction("Index", new {id});
        }

        [HttpPost]
        public ActionResult EnableSubscription(int id)
        {
            this._membershipService.EnableSubscription(id);
            return this.RedirectToAction("Index", new {id});
        }

        [HttpPost]
        public ActionResult UnlockUsers(int id)
        {
            this._membershipService.UnlockAllUsers(id);
            return this.RedirectToAction("Index", new { id });
        }

        [HttpPost]
        public ActionResult LoginAsVenueAdmin(int id)
        {
            var identity = (AirServiceIdentity)User.Identity;
            this._membershipService.LoginAsVenueAdmin(this.HttpContext, id, identity.UserId);
            return this.RedirectToAction("Index", "Home", new {area = ""});
        }
         
        [HttpPost]
        public ActionResult EnableWebServicesAllVenues()
        {
            var serviceStatusFolder = GetServiceStatusFolder();
            var offlineFile = Path.Combine(serviceStatusFolder, "app_offline.htm");
            var serviceDisabled = System.IO.File.Exists(offlineFile);
            if (serviceDisabled)
            {
                // if possible rename file,  this is to avoid giving permission to entier folder.  
                var onlineFile = Path.Combine(serviceStatusFolder, "app_online.htm");
                if (System.IO.File.Exists(onlineFile))
                {
                    System.IO.File.Delete(offlineFile);
                }
                else
                {
                    System.IO.File.Move(offlineFile, onlineFile);
                }
            }

            return this.RedirectToAction("Index", new { id = "" });
        }


        private static string GetServiceStatusFolder()
        {
            return ConfigurationManager.AppSettings["ServiceStatusFileFolder"];
        }

        public ActionResult ResetConnections(int id)
        {
            this._venueService.ResetConnections(id);
            return this.RedirectToAction("Index", new {id }); 
        }
    }
}
