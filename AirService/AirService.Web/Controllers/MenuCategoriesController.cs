using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using AirService.Model;
using AirService.Services.Contracts;
using AirService.Services.Security;
using AirService.Web.Helpers;
using AirService.Web.Infrastructure;
using AirService.Web.Infrastructure.Filters;
using AirService.Web.ViewModels;

namespace AirService.Web.Controllers
{
    [PaidCustomerOnly(Roles = WellKnownSecurityRoles.SystemAdministratorsAndVenueAdministrators)]
    public class MenuCategoriesController : Controller
    {
        private readonly IMenuService _menuService;
        private IMenuCategoryService _menuCategoryService;
        private IMenuItemService _menuItemService;
        
        public MenuCategoriesController(IMenuService menuService,
                                        IMenuCategoryService menuCategoryService,
                                        IMenuItemService menuItemService)
        {
            this._menuService = menuService;
            this._menuCategoryService = menuCategoryService;
            this._menuItemService = menuItemService;
        }

        #region Menu Categories

        //
        // GET: /MenuCategories/

        public ViewResult Index(int menuId)
        {
            ViewBag.MenuId = menuId;
            ViewBag.MenuName = _menuService.Find(menuId).Title;
            var categories = _menuCategoryService.GetMenuCategoriesWithKids(menuId);
            return View(MenuCategoryViewModel.Wrap(categories));
        }
         
        //
        // GET: /MenuCategories/Create
        [AjaxOnly]
        public ActionResult Create(int menuId)
        {
            ViewBag.MenuId = menuId;
            var menuCategory = new MenuCategory {MenuId = menuId};
            return this.PartialView("_MenuCategory",
                                    new MenuCategoryDetailViewModel
                                        {
                                            MenuCategory = menuCategory,
                                            AvailableStockImages = Server.GetMenuCategoryStockImages()
                                        });
        }

        //
        // POST: /MenuCategories/Create

        [HttpPost]
        [AjaxOnly]
        public ActionResult Create(MenuCategoryDetailViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var menuCategory = viewModel.MenuCategory;
                if (viewModel.SelectedImageType == (int) MenuCategoryDetailViewModel.ImageType.None)
                {
                    menuCategory.CategoryImage = String.Empty;
                }
                else if (viewModel.SelectedImageType == (int) MenuCategoryDetailViewModel.ImageType.Custom)
                {
                    menuCategory.CategoryImage = viewModel.SelectedCustomImage;
                }
                else if (viewModel.SelectedImageType == (int) MenuCategoryDetailViewModel.ImageType.Stock)
                {
                    menuCategory.CategoryImage = viewModel.SelectedStockImage;
                }

                _menuCategoryService.InsertOrUpdate(viewModel.MenuCategory);
                _menuCategoryService.Save();
                return this.Json(new MenuCategoryViewModel(viewModel.MenuCategory)); 
            }

            return this.ResponseWithJsonErrors();
        }

        //
        // GET: /MenuCategories/Edit/5
        [AjaxOnly]
        public ActionResult Edit(int id)
        {
            ViewBag.MenuCategoryId = id;
            var menuCategory = _menuCategoryService.Find(id);
            var viewModel = new MenuCategoryDetailViewModel
                                {
                                    MenuCategory = menuCategory,
                                    AvailableStockImages = Server.GetMenuCategoryStockImages()
                                };

            if (!String.IsNullOrWhiteSpace(menuCategory.CategoryImage))
            {
                if (
                    menuCategory.CategoryImage.Contains(
                        ConfigurationManager.AppSettings["CustomMenuCategoryImageLocation"]))
                {
                    viewModel.SelectedImageType = (int) MenuCategoryDetailViewModel.ImageType.Custom;
                    viewModel.SelectedCustomImage = menuCategory.CategoryImage;
                }
                else if (
                    menuCategory.CategoryImage.Contains(
                        ConfigurationManager.AppSettings["SharedMenuCategoryImageLocation"]))
                {
                    viewModel.SelectedImageType = (int) MenuCategoryDetailViewModel.ImageType.Stock;
                    viewModel.SelectedStockImage = menuCategory.CategoryImage;
                }
            }
            
            return PartialView("_MenuCategory", viewModel);
        }

        //
        // POST: /MenuCategories/Edit/5

        [HttpPost]
        [AjaxOnly]
        public ActionResult Edit(MenuCategoryDetailViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var venueId = ((AirServiceIdentity)User.Identity).VenueId;
                var menuCategory = viewModel.MenuCategory;
                if (viewModel.SelectedImageType == (int) MenuCategoryDetailViewModel.ImageType.None)
                {
                    menuCategory.CategoryImage = String.Empty;
                }
                else if (viewModel.SelectedImageType == (int) MenuCategoryDetailViewModel.ImageType.Custom)
                {
                    menuCategory.CategoryImage = viewModel.SelectedCustomImage;
                }
                else if (viewModel.SelectedImageType == (int) MenuCategoryDetailViewModel.ImageType.Stock)
                {
                    menuCategory.CategoryImage = viewModel.SelectedStockImage;
                }

                menuCategory = _menuCategoryService.Update(venueId, viewModel.MenuCategory);
                return Json(new MenuCategoryViewModel(menuCategory));
            }

            return this.ResponseWithJsonErrors();
        } 
         
        [HttpPost]
        [AjaxOnly]
        public ActionResult DeleteMenuCategoryWithCascade(int id)
        {
            int venueId = ((AirServiceIdentity) this.User.Identity).VenueId;
            var category =
                this._menuCategoryService.FindAll().FirstOrDefault(c => c.Menu.VenueId == venueId && c.Id == id);
            if (category != null)
            {
                string message;
                bool success = this._menuCategoryService.CascadeDelete(id, true, out message);
                if (!success)
                {
                    return this.ResponseWithJsonError(message);
                }

                this._menuCategoryService.Save();
            }

            return this.Json(true);
        }

        #endregion

        #region Menu Items
        [AjaxOnly]
        public ActionResult CreateMenuItem(int menuCategoryId)
        {
            ViewBag.MenuCategoryId = menuCategoryId;
            var menuItem = new MenuItem {MenuCategoryId = menuCategoryId};
            return PartialView("_MenuItem",
                               new MenuItemDetailViewModel
                                   {
                                       MenuItem = menuItem, 
                                       AvailableStockImages = Server.GetMenuItemStockImages()
                                   });
        }

        //
        // POST: /MenuItems/Create

        [HttpPost] 
        [AjaxOnly]
        public ActionResult CreateMenuItem(MenuItemDetailViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var menuItem = viewModel.MenuItem;
                if (!string.IsNullOrWhiteSpace(viewModel.MenuItemOptions))
                {
                    menuItem.MenuItemOptions = new JavaScriptSerializer().Deserialize<List<MenuItemOption>>(viewModel.MenuItemOptions);
                }

                int venuId = ((AirServiceIdentity)this.User.Identity).VenueId;
                var category = this._menuCategoryService.FindAll().FirstOrDefault( c => c.Menu.VenueId == venuId && c.Id == menuItem.MenuCategoryId);
                if (category != null)
                {
                    if (viewModel.SelectedImageType == (int)MenuCategoryDetailViewModel.ImageType.None)
                    {
                        menuItem.Image = String.Empty;
                    }
                    else if (viewModel.SelectedImageType == (int)MenuCategoryDetailViewModel.ImageType.Custom)
                    {
                        menuItem.Image = viewModel.SelectedCustomImage;
                    }
                    else if (viewModel.SelectedImageType == (int)MenuCategoryDetailViewModel.ImageType.Stock)
                    {
                        menuItem.Image = viewModel.SelectedStockImage;
                    }

                    menuItem = _menuItemService.Insert(menuItem);
                    return Json(new MenuItemViewModel(menuItem));
                }
            }

            return this.ResponseWithJsonErrors();
        }

        //
        // GET: /MenuItems/Edit/5 
        [AjaxOnly]
        public ActionResult EditMenuItem(int id)
        {
            ViewBag.MenuItemId = id;
            var menuItem = _menuItemService.GetMenuItemWithKids(id);
            var viewModel = new MenuItemDetailViewModel
                                {
                                    MenuItem = menuItem,
                                    AvailableStockImages = this.Server.GetMenuItemStockImages()
                                };

            if (!String.IsNullOrWhiteSpace(menuItem.Image))
            {
                if (menuItem.Image.Contains(ConfigurationManager.AppSettings["CustomMenuItemImageLocation"]))
                {
                    viewModel.SelectedImageType = (int) MenuItemDetailViewModel.ImageType.Custom;
                    viewModel.SelectedCustomImage = menuItem.Image;
                }
                else if (menuItem.Image.Contains(ConfigurationManager.AppSettings["SharedMenuItemImageLocation"]))
                {
                    viewModel.SelectedImageType = (int) MenuItemDetailViewModel.ImageType.Stock;
                    viewModel.SelectedStockImage = menuItem.Image;
                }
            }
            
            return PartialView("_MenuItem", viewModel);
        }
         
        [AjaxOnly]
        [HttpPost]
        public ActionResult EditMenuItem(MenuItemDetailViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var venueId = ((AirServiceIdentity) this.User.Identity).VenueId;
                var menuItem = viewModel.MenuItem;
                if (!string.IsNullOrWhiteSpace(viewModel.MenuItemOptions))
                {
                    menuItem.MenuItemOptions = new JavaScriptSerializer().Deserialize<List<MenuItemOption>>(viewModel.MenuItemOptions);
                }

                ViewBag.MenuItemId = menuItem.Id;

                if (viewModel.SelectedImageType == (int) MenuCategoryDetailViewModel.ImageType.None)
                {
                    menuItem.Image = String.Empty;
                }
                else if (viewModel.SelectedImageType == (int) MenuCategoryDetailViewModel.ImageType.Custom)
                {
                    menuItem.Image = viewModel.SelectedCustomImage;
                }
                else if (viewModel.SelectedImageType == (int) MenuCategoryDetailViewModel.ImageType.Stock)
                {
                    menuItem.Image = viewModel.SelectedStockImage;
                }

                menuItem = _menuItemService.Update(venueId, menuItem);
                return Json(new MenuItemViewModel(menuItem));
            }

            return this.ResponseWithJsonErrors();
        }
         
        [AjaxOnly]
        public ActionResult CopyMenuItem(int id)
        {
            var venueId = ((AirServiceIdentity) this.User.Identity).VenueId;
            var menuItem =
                this._menuItemService.FindAll().FirstOrDefault(
                    item => item.MenuCategory.Menu.VenueId == venueId && item.Id == id);
            if (menuItem != null)
            {
                var newMenuItem = this._menuItemService.CreateCopy(menuItem);
                return this.Json(new MenuItemViewModel(newMenuItem));
            }

            return this.Json("");
        }
     

        [HttpPost]
        [AjaxOnly]
        public ActionResult DeleteMenuItemWithCascade(int id)
        {
            string message;
            var success = _menuItemService.CascadeDelete(id, true, out message);
            if (!success)
            {
                return this.ResponseWithJsonError(message);
            }

            // normal view
            this._menuItemService.Save();
            return this.Json(true);
        }

        #endregion

        #region Menu Item Options
 
        #endregion

        #region Image Upload

        [HttpPost]
        [AjaxOnly]
        public ActionResult UploadImageFile(string qqfile)
        {
            var result = new FileUploadResult();

            var fileName = qqfile;
            var venueId = ((AirServiceIdentity)User.Identity).VenueId;
            var directoryName = Path.Combine(Server.MapPath(ConfigurationManager.AppSettings["CustomMenuCategoryImageLocation"]), venueId.ToString());
            var path = Path.Combine(directoryName, fileName);
            if (!Directory.Exists(directoryName))
            {
                Directory.CreateDirectory(directoryName);
            }
             
            try
            {
                using (var fileStream = new FileStream(path, FileMode.Create))
                {
                    int length = 4096;
                    int bytesRead;
                    Byte[] buffer = new Byte[length];
                    do
                    {
                        bytesRead = System.Web.HttpContext.Current.Request.InputStream.Read(buffer, 0, length);
                        fileStream.Write(buffer, 0, bytesRead);
                    }
                    while (bytesRead > 0);
                }

                fileName = ControllerHelper.ResizeImage(path);
                result.success = true;
                result.url = Path.Combine(ConfigurationManager.AppSettings["CustomMenuCategoryImageLocation"], venueId.ToString() + Path.AltDirectorySeparatorChar, fileName); ;
            }
            catch (UnauthorizedAccessException ex)
            {
                // log error hinting to set the write permission of ASPNET or the identity accessing the code
                result.success = false;
                result.url = ex.Message;
            }

            return Json(result);
        }

        #endregion

        [HttpPost] 
        public void UpdateItemSortOrder(int menuItemId, int index)
        {
            _menuItemService.UpdateSortOrder(menuItemId, index);
        }

        [HttpPost]
        public void UpdateCategorySortOrder(int id, int index)
        {
            _menuCategoryService.UpdateSortOrder(id, index);
        }
    }
}

