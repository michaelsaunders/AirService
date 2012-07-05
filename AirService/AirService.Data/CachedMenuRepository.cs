using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using AirService.Data.Contracts;
using AirService.Model;

namespace AirService.Data
{
    public class CachedMenuRepository<T> : SimpleRepository<T> where T : class
    {
        private const string VenueMenuCacheKeyFormat = "3AC41852-0F50-41C1-A0AF-5B1866CE8AB6-{0}-";
        private const string DeviceMenuCacheKeyFormat = "3AC41852-0F50-41C1-A0AF-5B1866CE8AB6-{0}-{1}";
        private readonly ICacheProvider _cacheProvider;

        protected CachedMenuRepository(IAirServiceContext context,
                                       ICacheProvider cacheProvider) : base(context)
        {
            this._cacheProvider = cacheProvider;
        }

        public override void Save()
        {
            try
            {
                var venuIds = new List<int>();
                const EntityState changedStates = EntityState.Added | EntityState.Modified | EntityState.Deleted;

                venuIds.AddRange((from menu in this.Context.GetEntitiesByStates<Menu>(changedStates)
                                  select menu.VenueId).Distinct());

                // some in-memory category may not have Menu object populated
                var modifiedMenuIds = (from category in this.Context.GetEntitiesByStates<MenuCategory>(changedStates)
                                       select category.MenuId).ToList();
                venuIds.AddRange((from menu in this.Context.Menus
                                  where modifiedMenuIds.Contains(menu.Id)
                                  select menu.VenueId).Distinct());

                // some in-memory item may not have menu category populated 
                var modifiedCategoryIds = (from item in this.Context.GetEntitiesByStates<MenuItem>(changedStates)
                                           select item.MenuCategoryId).ToList();
                venuIds.AddRange((from menu in this.Context.Menus
                                  from category in menu.MenuCategories
                                  where modifiedCategoryIds.Contains(category.Id)
                                  select menu.VenueId).Distinct());

                // some in-memory item option may not have item populated 
                var modifiedMenuItemsIds =
                    (from option in this.Context.GetEntitiesByStates<MenuItemOption>(changedStates)
                     select option.MenuItemId).ToList();
                venuIds.AddRange((from menu in this.Context.Menus
                                  from category in menu.MenuCategories
                                  from item in category.MenuItems
                                  where modifiedMenuItemsIds.Contains(item.Id)
                                  select menu.VenueId).Distinct());

                foreach (var venueId in venuIds.Distinct())
                {
                    var keyPattern = string.Format(DeviceMenuCacheKeyFormat , venueId ,"*");
                    this._cacheProvider.InvalidateCachesByMatchingKeyPattern(keyPattern);
                }
            }
            catch(Exception e)
            {
                Debug.WriteLine(e.Message);
#if DEBUG
                //if (Debugger.IsAttached)
                //{
                //    Debugger.Break();
                //}
#endif
            }

            base.Save();
        }

        public List<Menu> GetMenus(int venueId)
        {
            string cacheKey = string.Format(VenueMenuCacheKeyFormat,
                                            venueId);
            return this._cacheProvider.Get<List<Menu>>(cacheKey);
        }

        public void UpdateMenuCache(int venueId,
                                    List<Menu> menus)
        {
            string cacheKey = string.Format(VenueMenuCacheKeyFormat,
                                            venueId);
            this._cacheProvider.Cache(cacheKey, menus);
        }

        public DateTime? GetMenuLastModifiedDate(int venueId)
        {
            string cacheKey = string.Format(VenueMenuCacheKeyFormat,
                                            venueId);
            return this._cacheProvider.GetLastModifiedDate(cacheKey);
        }

        public List<Menu> GetDeviceMenus(int venueId,
                                         int deviceId)
        {
            string cacheKey = string.Format(DeviceMenuCacheKeyFormat,
                                            venueId,
                                            deviceId);
            return this._cacheProvider.Get<List<Menu>>(cacheKey);
        }

        public void UpdateDeviceMenuCache(int venueId,
                                          int deviceId,
                                          List<Menu> data)
        {
            string venueMenuCacheKey = string.Format(VenueMenuCacheKeyFormat,
                                                     venueId);
            string deviceMenuCacheKey = string.Format(DeviceMenuCacheKeyFormat,
                                                      venueId,
                                                      deviceId);
            this._cacheProvider.Cache(deviceMenuCacheKey,
                                      data,
                                      venueMenuCacheKey);
        }
    }
}