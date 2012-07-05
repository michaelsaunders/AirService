using System.Linq;
using System.Net;
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
    public class iPadsController : Controller
    {
        private readonly IIPadService _ipadService;
        private readonly IMenuService _menuService;
        private readonly IVenueAreaService _venueAreaService;

        public iPadsController(IIPadService ipadService,
                               IMenuService menuService,
                               IVenueAreaService venueAreaService)
        {
            this._ipadService = ipadService;
            this._menuService = menuService;
            this._venueAreaService = venueAreaService;
        }

        //
        // GET: /iPads/

        public ViewResult Index()
        {
            var userId = ((AirServiceIdentity)User.Identity).UserId;
            return View(_ipadService.FindAllByUser(userId));
        }
         
        // GET: /iPads/Create
        [AjaxOnly]
        public ActionResult Create()
        {
            var userId = ((AirServiceIdentity) User.Identity).UserId;
            var venueId = ((AirServiceIdentity) User.Identity).VenueId;
            var viewModel = new IPadViewModel
                                {
                                    iPad = new iPad {VenueId = venueId, Pin = _ipadService.GeneratePinCode(venueId)},
                                    AvailableMenus = _menuService.FindAllByUser(userId).ToList(),
                                    AvailableVenueAreas = _venueAreaService.FindAllByUser(userId).ToList()
                                };
            
            return PartialView("_iPad", viewModel);
        }

        //
        // POST: /iPads/Create

        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            iPad ipad = new iPad();
            IPadViewModel viewModel = new IPadViewModel();
            if (TryUpdateModel(ipad, "iPad") && TryUpdateModel(viewModel))
            {
                if (viewModel.SelectedMenus == null)
                {
                    viewModel.SelectedMenus = new int[] { };
                }

                foreach (var selectedMenu in viewModel.SelectedMenus)
                {
                    Menu menu = this._menuService.Find(selectedMenu);
                    var relation = new DeviceMenuOption
                                       {
                                           Menu = menu,
                                           MenuId = menu.Id,
                                           iPad = ipad,
                                           iPadId = ipad.Id
                                       };
                    ipad.AssignedMenus.Add(relation);
                    menu.AssignedDevices.Add(relation);
                }

                _ipadService.InsertOrUpdate(ipad);
                _ipadService.Save();

                if (Request.IsAjaxRequest())
                {
                    var userId = ((AirServiceIdentity)User.Identity).UserId;
                    return PartialView("_iPads", _ipadService.FindAllByUser(userId));
                }
                return RedirectToAction("Index");
            }

            this.Response.StatusCode = (int) HttpStatusCode.InternalServerError;
            return new JsonResult
                       {
                           Data = new
                                      {
                                          HasError = true,
                                          ErrorKeys = this.ModelState.Keys.ToArray(),
                                          ErrorMessages = this.ModelState.Values.ToArray()
                                      }
                       };
        }

        //
        // GET: /iPads/Edit/5
        [AjaxOnly]
        public ActionResult Edit(int id)
        {
            var userId = ((AirServiceIdentity) User.Identity).UserId;
            var currentiPad = _ipadService.Find(id);
            var viewModel = new IPadViewModel
                                {
                                    iPad = currentiPad,
                                    AvailableMenus = this._menuService.FindAllByUser(userId).ToList(),
                                    SelectedMenus = currentiPad.AssignedMenus.Select(m => m.MenuId).ToArray(),
                                    AvailableVenueAreas = this._venueAreaService.FindAllByUser(userId).ToList()
                                };
            
            return PartialView("_iPad", viewModel);
        }

        //
        // POST: /iPads/Edit/5
        [AjaxOnly]
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            var ipad = _ipadService.Find(id);
            var viewModel = new IPadViewModel();
            if (TryUpdateModel(ipad, "iPad") && TryUpdateModel(viewModel))
            {
                viewModel.SelectedMenus = viewModel.SelectedMenus ?? new int[] {};

                _ipadService.UpdateModelForMenus(ref ipad, viewModel.SelectedMenus);
                _ipadService.InsertOrUpdate(ipad);
                _ipadService.Save();
                var userId = ((AirServiceIdentity) User.Identity).UserId;
                return PartialView("_iPads", _ipadService.FindAllByUser(userId));
            }

            return this.ResponseWithJsonErrors();
        }
         
        //
        // POST: /iPads/Delete/5
        [AjaxOnly]
        [HttpPost, ActionName("Delete")] 
        public ActionResult DeleteConfirmed(int id)
        {
            _ipadService.Delete(id);
            _ipadService.Save();
            var userId = ((AirServiceIdentity) User.Identity).UserId;
            return PartialView("_iPads", _ipadService.FindAllByUser(userId));
        }

        [HttpPost]
        public JsonResult GenerateNewPinCode()
        {
            var venueId = ((AirServiceIdentity) User.Identity).VenueId;
            var newPinCode = _ipadService.GeneratePinCode(venueId);
            var result = new JsonResult { Data = new { success = true, code = newPinCode } };
            return result;
        }
    }
}