using System;
using System.Linq;
using System.Web.Mvc;
using AirService.Model;
using AirService.Services;
using AirService.Services.Contracts;
using AirService.Services.Security;
using AirService.Web.Infrastructure;
using AirService.Web.Infrastructure.Filters;
using AirService.Web.ViewModels;

namespace AirService.Web.Controllers
{
    [PaidCustomerOnly(Roles = WellKnownSecurityRoles.SystemAdministratorsAndVenueAdministrators)]
    public class MenusController : Controller
    {
        private IMenuService _menuService;

        public MenusController(IMenuService menuService)
        {
			this._menuService = menuService;
        }

        //
        // GET: /Menus/
        public ViewResult Index()
        {
            var userId = ((AirServiceIdentity)this.User.Identity).UserId;
            var menus = _menuService.FindAllByUserIncluding(userId, m => m.MenuCategories);
            return View(MenuViewModel.Wrap(menus));
        }
         
        //
        // GET: /Menus/Create
        [AjaxOnly]
        public ActionResult Create()
        {
            var venueId = ((AirServiceIdentity) User.Identity).VenueId;
            return PartialView("_Menu", new Menu {VenueId = venueId});
        }

        //
        // POST: /Menus/Create

        [HttpPost]
        [AjaxOnly]
        public ActionResult Create(Menu menu)
        {
            if (ModelState.IsValid)
            {
                menu.VenueId = Convert.ToInt32(((AirServiceIdentity) User.Identity).VenueId); 
                _menuService.InsertOrUpdate(menu);
                _menuService.Save();
                return Json(new MenuViewModel(menu));
            }

            return this.ResponseWithJsonErrors();
        }

        // GET: /Menus/Edit/5
        [AjaxOnly]
        public ActionResult Edit(int id)
        {
            return PartialView("_Menu", _menuService.Find(id)); 
        }

        //
        // POST: /Menus/Edit/5

        [HttpPost]
        [AjaxOnly]
        public ActionResult Edit(Menu menu)
        {
            if (ModelState.IsValid)
            {
                menu.VenueId = Convert.ToInt32(((AirServiceIdentity)User.Identity).VenueId);
                menu = _menuService.UpdateMenu(menu);
                return Json(new MenuViewModel(menu));
            }

            return this.ResponseWithJsonErrors();
        }
          
        [HttpPost]
        [AjaxOnly]
        public ActionResult DeleteMenuWithCascade(int id)
        {
            string message = null;
            var userId =  ((AirServiceIdentity) User.Identity).UserId;
            var menu = this._menuService.FindAllByUser(userId).FirstOrDefault(m => m.Id == id);
            var success = menu != null && _menuService.CascadeDelete(id, true, out message); 
            if (!success)
            {
                return this.ResponseWithJsonError(message);
            }

            // normal view
            this._menuService.Save();
            return this.Json(true);
        }

        public void UpdateMenuSortOrder(int menuId, int index)
        {
            this._menuService.UpdateMenuSortOrders(menuId, index); 
        }
    }
}