using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using AirService.Data;
using AirService.Data.Contracts;
using AirService.Model;
using AirService.Services.Contracts;

namespace AirService.Services
{
    public class MenuService : SimpleService<Menu>, IMenuService
    {
        private IMenuCategoryService _menuCategoryService;

        public MenuService(IRepository<Menu> menuRepository,
                           IMenuCategoryService menuCategoryService)
        {
            this.Repository = menuRepository;
            this._menuCategoryService = menuCategoryService;
        }

        #region IMenuService Members

        public override void InsertOrUpdate(Menu menu)
        {
            if (menu.Id == 0)
            {
                // set the sort order
                var currentMax = (from item in this.Repository.FindAll()
                                  where item.VenueId == menu.VenueId
                                  select (int?)item.SortOrder).Max();
                var iPads = this.Repository.Set<iPad>().Where(ipad => ipad.VenueId == menu.VenueId);
                menu.AssignedDevices = new List<DeviceMenuOption>();
                foreach (var ipad in iPads)
                {
                    var relation = new DeviceMenuOption
                                       {
                                           Menu = menu,
                                           MenuId = menu.Id,
                                           iPad = ipad,
                                           iPadId = ipad.Id,
                                           Print = true
                                       };
                    menu.AssignedDevices.Add(relation);
                }

                menu.SortOrder = currentMax.HasValue ? currentMax.Value + 1 : 1;
            }

            base.InsertOrUpdate(menu);
        }

        public List<Menu> GetMenus(int venueId, string udid)
        {
            this.Repository.SetContextOption(ContextOptions.ProxyCreation,
                                             false);
            // EF limitation on Projection of multiple tables with "include" when not using lazy loading
            // http://social.msdn.microsoft.com/Forums/en/adodotnetentityframework/thread/0877f2f3-32d4-43cd-bd13-90e6c2840e34
            var set = from menu in this.Repository.FindAll()
                      where menu.VenueId == venueId &&
                            menu.Status == SimpleModel.StatusActive
                      orderby menu.SortOrder
                      select new
                                 {
                                     menu,
                                     categories = from category in menu.MenuCategories
                                                  where category.Status == SimpleModel.StatusActive
                                                  orderby category.SortOrder
                                                  select category,
                                     items = from category in menu.MenuCategories
                                             from item in category.MenuItems
                                             where category.Status == SimpleModel.StatusActive &&
                                                   item.Status == SimpleModel.StatusActive
                                             orderby item.SortOrder
                                             select item,
                                     itemOptions = from category in menu.MenuCategories
                                                   from item in category.MenuItems
                                                   from itemOption in item.MenuItemOptions
                                                   where category.Status == SimpleModel.StatusActive &&
                                                         item.Status == SimpleModel.StatusActive &&
                                                         itemOption.Status == SimpleModel.StatusActive
                                                   orderby itemOption.Id
                                                   /* Sort order not implemented yet */
                                                   select itemOption
                                 };

            // AsEnumerable() is necessary which will trigger categories, items and itemOptions to be filled. 
            var data = set.AsEnumerable().Select(item => item.menu).ToList();
            SetupPrintingPermission(venueId, data, udid: udid); 
            return data;
        }

        public bool IsMenuItemAvailable(int venueId,
                                        int menuItemId)
        {
            this.Repository.SetContextOption(ContextOptions.ProxyCreation,
                                             false);
            IQueryable<MenuItem> menuItem = from menu in this.Repository.FindAll()
                                            from category in menu.MenuCategories
                                            from item in category.MenuItems
                                            where
                                                menu.VenueId == venueId &&
                                                menu.Status == SimpleModel.StatusActive &&
                                                menu.MenuStatus &&
                                                category.Status == SimpleModel.StatusActive &&
                                                category.IsLive &&
                                                item.Id == menuItemId &&
                                                item.Status == SimpleModel.StatusActive &&
                                                item.MenuItemStatus
                                            select item;

            return menuItem.FirstOrDefault() != null;
        }

        public void EnableMenu(int venueId,
                               int menuId,
                               bool enable)
        {
            Menu menuToUpdate = (from menu in this.Repository.FindAll()
                                 where menu.VenueId == venueId
                                       && menu.Id == menuId
                                 select menu).FirstOrDefault();
            if (menuToUpdate == null)
            {
                throw new ServiceFaultException(1030, Resources.Err1030MenuNotFound);
            }

            menuToUpdate.MenuStatus = enable;
            this.Repository.Update(menuToUpdate);
            this.Repository.Save();
        }

        public void EnableMenuCategory(int venueId,
                                       int categoryId,
                                       bool enable)
        {
            MenuCategory categoryToUpdate = (from category in this.Repository.Set<MenuCategory>()
                                             where category.Menu.VenueId == venueId
                                                   && category.Id == categoryId
                                             select category).FirstOrDefault();
            if (categoryToUpdate == null)
            {
                throw new ServiceFaultException(1031,
                                                Resources.Err1031MenuCategoryNotFound);
            }

            categoryToUpdate.IsLive = enable;
            this.Repository.Update(categoryToUpdate);
            this.Repository.Save();
        }

        public void EnableMenuItem(int venueId,
                                   int menuItemId,
                                   bool enable)
        {
            MenuItem menuItemToUpdate = (from menuItem in this.Repository.Set<MenuItem>()
                                         where menuItem.MenuCategory.Menu.VenueId == venueId
                                               && menuItem.Id == menuItemId
                                         select menuItem).FirstOrDefault();

            if (menuItemToUpdate == null)
            {
                throw new ServiceFaultException(1032, Resources.Err1032MenuItemNotFound);
            }

            menuItemToUpdate.MenuItemStatus = enable;
            this.Repository.Update(menuItemToUpdate);
            this.Repository.Save();
        }

        public void EnableMenuPrinting(int venueId, string udid, int menuId, bool enabled)
        {
            Menu menuToUpdate = (from menu in this.Repository.FindAll()
                                 where menu.VenueId == venueId
                                       && menu.Id == menuId
                                 select menu).FirstOrDefault();
            if (menuToUpdate == null)
            {
                throw new ServiceFaultException(1030, Resources.Err1030MenuNotFound);
            }

            var option = (from deviceOption in this.Repository.Set<DeviceMenuOption>()
                          where deviceOption.MenuId == menuId &&
                                deviceOption.iPad.Udid == udid
                          select deviceOption).FirstOrDefault();
            if (option == null)
            {
                var iPad = this.Repository.Set<iPad>().First(ipad => ipad.Udid == udid);
                option = new DeviceMenuOption
                             {
                                 iPad = iPad,
                                 iPadId = iPad.Id,
                                 Menu = menuToUpdate,
                                 MenuId = menuId, 
                                 Print = enabled
                             };

                this.Repository.Insert(option);
            }
            else
            {
                option.Print = enabled;
                this.Repository.Update(option);
            }

            this.Repository.Save();
        }

        public void EnableMenuItemPrinting(int venueId, string udid, int menuItemId, bool enabled)
        {
            MenuItem menuItemToUpdate = (from menuItem in this.Repository.Set<MenuItem>()
                                         where menuItem.MenuCategory.Menu.VenueId == venueId
                                               && menuItem.Id == menuItemId
                                         select menuItem).FirstOrDefault();

            if (menuItemToUpdate == null)
            {
                throw new ServiceFaultException(1032, Resources.Err1032MenuItemNotFound);
            }

            var option = (from deviceOption in this.Repository.Set<DeviceMenuItemOption>()
                          where deviceOption.MenuItemId == menuItemId &&
                                deviceOption.iPad.Udid == udid
                          select deviceOption).FirstOrDefault();

            if (option == null)
            {
                var iPad = this.Repository.Set<iPad>().First(ipad => ipad.Udid == udid);
                option = new DeviceMenuItemOption
                             {
                                 iPadId = iPad.Id,
                                 iPad = iPad,
                                 MenuItem = menuItemToUpdate, 
                                 MenuItemId = menuItemId, 
                                 Print = enabled
                             };

                this.Repository.Insert(option);
            }
            else
            {
                option.Print = enabled;
                this.Repository.Update(option);
            }

            this.Repository.Save();
        }

        public bool CanModifyMenu(string deviceAdminEmail,
                                  int menuId)
        {
            return (from admin in this.Repository.Set<DeviceAdmin>()
                    from ipad in admin.iPads
                    from relation in ipad.AssignedMenus
                    where relation.MenuId == menuId && admin.Email == deviceAdminEmail
                    select relation).Any();
        }

        public bool CanModifyMenuCategory(string deviceAdminEmail,
                                          int menuCategoryId)
        {
            return (from admin in this.Repository.Set<DeviceAdmin>()
                    from ipad in admin.iPads
                    from relation in ipad.AssignedMenus
                    from category in relation.Menu.MenuCategories
                    where category.Id == menuCategoryId && admin.Email == deviceAdminEmail
                    select category).Any();
        }

        public bool CanModifyMenuItem(string deviceAdminEmail,
                                      int menuItemId)
        {
            return (from admin in this.Repository.Set<DeviceAdmin>()
                    from ipad in admin.iPads
                    from relation in ipad.AssignedMenus
                    from category in relation.Menu.MenuCategories
                    from item in category.MenuItems
                    where item.Id == menuItemId && admin.Email == deviceAdminEmail
                    select item).Any();
        }  

        public List<Menu> GetDeviceMenus(int venueId,
                                         int deviceId)
        {
            this.Repository.SetContextOption(ContextOptions.ProxyCreation,
                                             false);
            var set = from menu in this.Repository.FindAll().Where(m => m.VenueId == venueId &&
                                                                        m.AssignedDevices.Any(ipad => ipad.iPadId == deviceId))
                      select new
                                 {
                                     menu,
                                     categories = from category in menu.MenuCategories
                                                  where category.Status == SimpleModel.StatusActive
                                                  select category,
                                     items = from category in menu.MenuCategories
                                             from item in category.MenuItems
                                             where category.Status == SimpleModel.StatusActive &&
                                                   item.Status == SimpleModel.StatusActive
                                             select item,
                                     itemOptions = from category in menu.MenuCategories
                                                   from item in category.MenuItems
                                                   from itemOption in item.MenuItemOptions
                                                   where category.Status == SimpleModel.StatusActive &&
                                                         item.Status == SimpleModel.StatusActive &&
                                                         itemOption.Status == SimpleModel.StatusActive
                                                   select itemOption
                                 };

            var data = set.AsEnumerable().Select(item => item.menu).ToList();
            SetupPrintingPermission(venueId, data, deviceId); 
            return data; 
        }

        #endregion

        public override IQueryable<Menu> FindAllByUser(Guid userId)
        {
            return ((MenuRepository) this.Repository).FindAllByUser(userId);
        }

        public IQueryable<Menu> FindAllByUserIncluding(Guid userId, 
                                                       params Expression<Func<Menu, object>>[] includeProperties)
        {
            return ((MenuRepository) this.Repository).FindAllByUserIncluding(userId, 
                                                                             includeProperties);
        }

        public void UpdateMenuSortOrders(int menuId, int index)
        {
            var currentMenu = this.Repository.Find(menuId);
            var venueId = currentMenu.VenueId;
            var menus = (from venue in Repository.Set<Venue>()
                         from menu in venue.Menus
                         where venue.Id == venueId &&
                               menu.Status == SimpleModel.StatusActive
                               && menu.Id != menuId
                         orderby menu.SortOrder
                         select menu).ToList();
            int i = 0;
            index = Math.Min(index, menus.Count);
            menus.Insert(index, currentMenu);
            foreach (var menu in menus)
            {
                menu.SortOrder = i++;
            }

            this.Repository.Save();
        }

        public Menu UpdateMenu(Menu menu)
        {
            if(this.Repository.Context.GetState(menu) == EntityState.Detached)
            {
                var existingMenu = this.FindAll().FirstOrDefault(m => m.Id == menu.Id && m.VenueId == menu.VenueId);
                if (existingMenu == null)
                {
                    return null;
                }

                existingMenu.Description = menu.Description;
                existingMenu.DisplayTitle = menu.DisplayTitle;
                existingMenu.Is24Hour = menu.Is24Hour;
                existingMenu.IsSpecialsMenu = menu.IsSpecialsMenu;
                existingMenu.MenuStatus = menu.MenuStatus;
                existingMenu.ShowFrom = menu.ShowFrom;
                existingMenu.ShowTo = menu.ShowTo;
                existingMenu.Title = menu.Title;
                menu = existingMenu; 
            }
            else
            {
                this.InsertOrUpdate(menu); 
            }

            this.Repository.Save();
            return menu;
        }

        public override void Delete(int id)
        {
            var entity = Repository.Find(id);
            foreach (var menuCategory in entity.MenuCategories)
            {
                _menuCategoryService.Delete(menuCategory.Id);
            }

            base.Delete(id);
        }

        #region ICascadeEntity Members

        public bool CascadeDelete(int id, bool confirmCascadeEligible, out string message)
        {
            message = String.Empty;

            if (confirmCascadeEligible)
            {
                var currentOrderItems = from orderItem in Repository.Set<OrderItem>()
                                        where orderItem.MenuItem.MenuCategory.MenuId == id
                                        && orderItem.OrderStatus != Order.OrderStatusFinalized
                                        && orderItem.OrderStatus != Order.OrderStatusCancelled
                                        select orderItem;
                if (currentOrderItems.Any())
                {
                    message = "Unable to delete. Current orders exist using this item.";
                    return false;
                }
            }

            this.Delete(id);

            return true;
        }

        #endregion

        protected override void OnClone(IAirServiceContext context)
        {
            this._menuCategoryService = (IMenuCategoryService) this._menuCategoryService.Clone(true);
            base.OnClone(context);
        }

        private void SetupPrintingPermission(int venueId, List<Menu> menus, int? deviceId = null, string udid = null)
        {
            if ((!deviceId.HasValue || deviceId.Value == 0) && string.IsNullOrWhiteSpace(udid))
            {
                return;
            }

            if (!deviceId.HasValue || deviceId.Value == 0)
            {
                var target = this.Repository.Set<iPad>().FirstOrDefault(ipad => ipad.VenueId == venueId && ipad.Udid == udid);
                if (target == null)
                {
                    return;
                }

                deviceId = target.Id;
            }

            var menuOptions = (from option in this.Repository.Set<DeviceMenuOption>()
                               where option.iPadId == deviceId.Value
                               select new {option.MenuId, option.Print}).ToDictionary(m => m.MenuId);
            var menuItemOptions = (from option in this.Repository.Set<DeviceMenuItemOption>()
                                   where option.iPadId == deviceId.Value
                                   select new {option.MenuItemId, option.Print}).ToDictionary(m => m.MenuItemId);
            foreach (var menu in menus)
            {
                menu.Print = !menuOptions.ContainsKey(menu.Id) || menuOptions[menu.Id].Print;

                foreach (var menuItem in (from category in menu.MenuCategories.EmptyIfNull()
                                          from item in category.MenuItems.EmptyIfNull()
                                          select item))
                {
                    menuItem.Print = !menuItemOptions.ContainsKey(menuItem.Id) || menuItemOptions[menuItem.Id].Print;
                }
            }
        }
    }
}