using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using AirService.Model;
using AirService.Services.Contracts;
using AirService.Services.Security;
using AirService.Web.Infrastructure;
using AirService.Web.Infrastructure.Filters;
using AirService.Web.ViewModels;

namespace AirService.Web.Controllers
{
    [PaidCustomerOnly(Roles = WellKnownSecurityRoles.SystemAdministratorsAndVenueAdministrators)]
    public class IPadAdminController : Controller
    {
        private readonly IDeviceAdminService _deviceAdminService;
        private readonly IIPadService _iPadService;
        private readonly IVenueService _venueService;

        public IPadAdminController(IDeviceAdminService deviceAdminService,
                                   IVenueService venueService,
                                   IIPadService iPadService)
        {
            this._deviceAdminService = deviceAdminService;
            this._venueService = venueService;
            this._iPadService = iPadService;
        }

        public ActionResult Index()
        {
            return this.View("Index",
                             this.GetDeviceAdmins());
        }

        //
        // GET: /iPadAdmin/Create
        [AjaxOnly]
        public ActionResult Create()
        {
            this.ViewBag.IsEdit = false;
            var venueId = ((AirServiceIdentity) this.User.Identity).VenueId;
            var viewModel = new DeviceAdminViewModel
                                {
                                    AlliPads = this._iPadService.FindAllByVenueId(venueId),
                                    SelectedDeviceIds = new int[] {},
                                    Password = Guid.NewGuid().ToString("N").Substring(0, 6)
                                };
            
            return this.PartialView("_iPadAdmin",
                                    viewModel);
        }

        [HttpPost]
        [AjaxOnly]
        public ActionResult Create(FormCollection formCollection)
        {
            this.ViewBag.IsEdit = false;
            var venue = this._venueService.Find(((AirServiceIdentity) this.User.Identity).VenueId);
            var viewModel = new DeviceAdminViewModel();
            Exception error = null;
            if (this.TryUpdateModel(viewModel))
            {
                DeviceAdmin deviceAdmin = viewModel.DeviceAdmin;
                if (viewModel.SelectedDeviceIds != null && viewModel.SelectedDeviceIds.Length > 0)
                {
                    deviceAdmin.iPads = this._iPadService.FindAllByIds(venue.Id,
                                                                       viewModel.SelectedDeviceIds);
                    foreach (var ipad in deviceAdmin.iPads)
                    {
                        ipad.DeviceAdmins.Add(deviceAdmin);
                    }
                }

                deviceAdmin.VenueId = venue.Id;
                deviceAdmin.Venue = venue;
                deviceAdmin.Password = viewModel.Password;
                deviceAdmin.CreateDate = DateTime.Now;
                deviceAdmin.UpdateDate = DateTime.Now;
                try
                {
                    this._deviceAdminService.Insert(deviceAdmin);
                    this._deviceAdminService.SendPasswordEmail(deviceAdmin,
                                                               this.Request.MapPath(
                                                                   "~/Content/EmailTemplates/iPadAdmin.htm"));
                    return this.PartialView("_iPadAdmins",
                                            this.GetDeviceAdmins());
                }
                catch (Exception e)
                {
                    error = e;
                }
            }

            return this.AjaxErrorResponse(error);
        }

        public ActionResult Edit(int id)
        {
            this.ViewBag.IsEdit = true;
            var venueId = ((AirServiceIdentity) this.User.Identity).VenueId;
            var deviceAdmin = this._deviceAdminService.FindByDeviceAdminId(venueId,
                                                                           id);
            var viewModel = new DeviceAdminViewModel(deviceAdmin)
                                {
                                    SelectedDeviceIds = deviceAdmin.iPads.Select(ipad => ipad.Id).ToArray(),
                                    AlliPads = this._iPadService.FindAllByVenueId(venueId)
                                };

            return this.PartialView("_iPadAdmin",
                                    viewModel);
        }

        [HttpPost]
        [AjaxOnly]
        public ActionResult Edit(DeviceAdminViewModel viewModel)
        {
            this.ViewBag.IsEdit = true;
            var venueId = ((AirServiceIdentity) this.User.Identity).VenueId;
            Exception error = null;
            if (this.ModelState.IsValid)
            {
                viewModel.SelectedDeviceIds = viewModel.SelectedDeviceIds ?? new int[]{};
                var existingData = this._deviceAdminService.FindByDeviceAdminId(venueId, viewModel.Id);
                var pinChanged = existingData.Password != viewModel.Password;
                var iPadsToDisassociate = (from ipad in existingData.iPads
                                           where !viewModel.SelectedDeviceIds.Contains(ipad.Id)
                                           select ipad).ToArray();
                foreach (var iPadToDisassociate in iPadsToDisassociate)
                {
                    pinChanged = true;
                    existingData.iPads.Remove(iPadToDisassociate);
                }

                var selectediPadIds = existingData.iPads.Select(ipad => ipad.Id).ToArray();
                var iPadsToAssociate = (from id in viewModel.SelectedDeviceIds
                                        where !selectediPadIds.Contains(id)
                                        select id).ToArray();
                if (iPadsToAssociate.Length > 0)
                {
                    pinChanged = true;
                    var ipads = this._iPadService.FindAllByIds(venueId,
                                                               iPadsToAssociate);
                    foreach (var ipad in ipads)
                    {
                        existingData.iPads.Add(ipad);
                        ipad.DeviceAdmins.Add(existingData);
                    }
                }

                existingData.UserName = viewModel.UserName;
                existingData.UpdateDate = DateTime.Now;
                existingData.Email = viewModel.Email;
                existingData.Password = viewModel.Password;
                try
                {
                    this._deviceAdminService.Update(existingData);
                    if (pinChanged)
                    {
                        this._deviceAdminService.SendPasswordEmail(existingData,
                                                                   this.Request.MapPath(
                                                                       "~/Content/EmailTemplates/iPadAdmin.htm"));
                    }

                    return this.PartialView("_iPadAdmins",
                                            this.GetDeviceAdmins());
                }
                catch (Exception e)
                {
                    error = e;
                }
            }

            return this.AjaxErrorResponse(error);
        }

        private ActionResult AjaxErrorResponse(Exception exception)
        {
            return this.ResponseWithJsonErrors(exception,
                                               errorNumber => errorNumber == 2601 ? "Email alreay exist" : null);
        }

        [HttpPost]
        [AjaxOnly]
        public ActionResult Delete(int id)
        {
            var venueId = ((AirServiceIdentity) this.User.Identity).VenueId;
            var succeeded = false;
            try
            {
                this._deviceAdminService.Delete(venueId,
                                                id);
            }
            catch(Exception e)
            {
                return this.ResponseWithJsonErrors(e);
            }

            return this.PartialView("_iPadAdmins",
                                    this.GetDeviceAdmins());
        }

        private IEnumerable<DeviceAdminViewModel> GetDeviceAdmins()
        {
            var venueId = ((AirServiceIdentity) this.User.Identity).VenueId;
            var deviceAdmins = this._deviceAdminService.FindAllByVenueId(venueId);
            var model = new List<DeviceAdminViewModel>();
            // this is ok as long as we don't have too many device admins
            // once database is finalized, we can create a class that is mapped to AspNet membership table 
            // so that we can do this in one query

            foreach (var deviceAdmin in deviceAdmins)
            {
                model.Add(new DeviceAdminViewModel(deviceAdmin)
                              {
                                  SelectedDeviceNames = deviceAdmin.iPads.Select(ipad => ipad.Name).ToArray()
                              });
            }

            return model.OrderBy(m => m.UserName);
        }

    }
}