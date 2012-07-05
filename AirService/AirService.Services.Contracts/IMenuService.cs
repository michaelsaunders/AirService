using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using AirService.Model;

namespace AirService.Services.Contracts
{
    public interface IMenuService : IService<Menu>, ICascadeEntity
    {
        List<Menu> GetMenus(int venueId, string udid);

        List<Menu> GetDeviceMenus(int venueId,
                                  int deviceId);

        bool IsMenuItemAvailable(int venueId,
                                 int menuItemId);

        void EnableMenu(int venueId,
                        int menuId,
                        bool enable);

        void EnableMenuCategory(int venueId,
                                int categoryId,
                                bool enable);

        void EnableMenuItem(int venueId,
                            int menuItemId,
                            bool enable);

        void EnableMenuPrinting(int venueId, string udid, int menuId, bool enabled);

        void EnableMenuItemPrinting(int venueId, string udid, int menuItemId, bool enabled);

        bool CanModifyMenu(string deviceAdminEmail,
                           int menuId);

        bool CanModifyMenuCategory(string deviceAdminEmail,
                                   int menuCategoryId);

        bool CanModifyMenuItem(string deviceAdminEmail,
                               int menuItemId);

        IQueryable<Menu> FindAllByUserIncluding(Guid userId,
                                                params Expression<Func<Menu, object>>[] includeProperties);

        void UpdateMenuSortOrders(int menuId, int index);

        Menu UpdateMenu(Menu menu);
    }
}