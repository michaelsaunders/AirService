using System;
using System.Linq;
using System.Web.Mvc;
using AirService.Services.Contracts;
using AirService.Services.Security;
using AirService.Web.Infrastructure.Filters;
using AirService.Web.ViewModels;

namespace AirService.Web.Controllers
{
    [PaidCustomerOnly(Roles = WellKnownSecurityRoles.SystemAdministratorsAndVenueAdministrators)]
    public class VenueReportsController : Controller
    {
        private readonly IIPadService _iPadService;
        private readonly IMenuService _menuService;
        private readonly IVenueReportService _reportService;

        public VenueReportsController(IVenueReportService reportService, IIPadService iPadService,
                                 IMenuService menuService)
        {
            this._reportService = reportService;
            this._iPadService = iPadService;
            this._menuService = menuService;
        }

        public ActionResult Index(VenueReportViewModel model)
        {
            this.InitializeModel(model);
            return this.View(model);
        }

        private void InitializeModel(VenueReportViewModel model)
        {
            var userId = ((AirServiceIdentity) this.User.Identity).UserId;
            model.iPads = this._iPadService.FindAllByUser(userId).ToList(); 
            model.Menus = this._menuService.FindAllByUser(userId).ToList();
        }

        [HttpPost]
        public ActionResult ExportToCsv(VenueReportViewModel reportViewModel)
        {
            if (!this.ModelState.IsValid)
            {
                this.InitializeModel(reportViewModel);
                return this.View("Index",
                                 reportViewModel);
            }

            this.SetiPadOrderReportData(reportViewModel);
            // may need to create a custom stream action result instead of working on string data
            this.HttpContext.Response.AddHeader("content-disposition",
                                                "attachment; filename=report.csv");
            return this.Content(reportViewModel.GetDeviceReportDataAsCsv(),
                                "text/csv");
        }

        [HttpPost]
        public ActionResult iPadOrderReport(VenueReportViewModel reportViewModel)
        {
            if (!this.ModelState.IsValid)
            {
                this.InitializeModel(reportViewModel);
                return this.View("Index",
                                 reportViewModel);
            }

            this.SetiPadOrderReportData(reportViewModel);
            return this.View(reportViewModel);
        }

        private void SetiPadOrderReportData(VenueReportViewModel reportViewModel)
        {
            int venueId = ((AirServiceIdentity) this.User.Identity).VenueId;
            int[] selectediPads = reportViewModel.SelectediPadIds;  
            int[] selectedMenus = reportViewModel.SelectedMenuIds;
            if (selectedMenus != null && selectedMenus.Any(id => id == 0))
            {
                selectedMenus = null;
            }

            reportViewModel.DeviceReportData = this._reportService.GetVenueDeviceOrderSummary(
                venueId,
                reportViewModel.DateFrom,
                reportViewModel.DateTo,
                reportViewModel.TimeFrom,
                reportViewModel.TimeTo,
                selectediPads,
                selectedMenus
                );
        }
    }
}