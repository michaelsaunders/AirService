using System;
using System.Data;
using System.Linq;
using AirService.Data;
using AirService.Data.Contracts;
using AirService.Model;
using AirService.Services.Contracts;

namespace AirService.Services
{
    public class MenuCategoryService : SimpleService<MenuCategory>, IMenuCategoryService
    {
        private IMenuItemService _menuItemService;

        public MenuCategoryService(IRepository<MenuCategory> menuCategoryRepository, IMenuItemService menuItemService)
        {
            this.Repository = menuCategoryRepository;
            this._menuItemService = menuItemService;
        }

        public override void InsertOrUpdate(MenuCategory entity)
        {
            if (entity.Id == default(int))
            {
                // set the sort order
                var currentMax = this.Repository.FindAll().Count(item => item.MenuId == entity.MenuId);
                entity.SortOrder = currentMax + 1;
            }

            base.InsertOrUpdate(entity);
        }

        public new IQueryable<MenuCategory> FindAllByUser(Guid userId)
        {
            return ((MenuCategoryRepository)this.Repository).FindAllByUser(userId);
        }

        public IQueryable<MenuCategory> GetMenuCategoriesWithKids(int menuId)
        {
            this.Repository.SetContextOption(ContextOptions.LazyLoading, false);
            var categoryWithProjection = from category in this.Repository.FindAll()
                                         where category.MenuId == menuId &&
                                               category.Status == SimpleModel.StatusActive
                                         select new
                                                    {
                                                        category,
                                                        menuItems = from menuItem in category.MenuItems
                                                                    where menuItem.Status == SimpleModel.StatusActive
                                                                    select menuItem,
                                                        options = from menuItem in category.MenuItems
                                                                  from option in menuItem.MenuItemOptions
                                                                  where menuItem.Status == SimpleModel.StatusActive &&
                                                                        option.Status == SimpleModel.StatusActive
                                                                  select option,
                                                    };

            return categoryWithProjection.AsEnumerable().Select(i => i.category).AsQueryable();
        }

        public void UpdateSortOrder(int menuCategoryId, int index)
        {
            var menuCategory = this.Repository.Find(menuCategoryId);
            var menuId = menuCategory.MenuId;
            var items = (from menu in this.Repository.Set<Menu>()
                         from category in menu.MenuCategories
                         where
                             menu.Id == menuId && category.Id != menuCategoryId &&
                             category.Status == SimpleModel.StatusActive
                         orderby category.SortOrder
                         select category).ToList();

            int i = 0;
            index = Math.Min(index, items.Count);
            items.Insert(index, menuCategory);
            foreach (var category in items)
            {
                category.SortOrder = i++;
            }

            Repository.Save();
        }

        public MenuCategory Update(int venueId, MenuCategory menuCategory)
        {
            if (this.Repository.Context.GetState(menuCategory) == EntityState.Detached)
            {
                var existingCategory =
                    this.FindAll().FirstOrDefault(c => c.Menu.VenueId == venueId && c.Id == menuCategory.Id);
                if (existingCategory == null)
                {
                    return null;
                }

                existingCategory.IsLive = menuCategory.IsLive;
                existingCategory.Title = menuCategory.Title;
                menuCategory = existingCategory; 
            }

            this.Save();
            return menuCategory;
        }

        public override void Delete(int id)
        {
            var entity = Repository.Find(id);
            foreach (var menuItem in entity.MenuItems)
            {
                _menuItemService.Delete(menuItem.Id);
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
                                        where orderItem.MenuItem.MenuCategoryId == id
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
            _menuItemService = (IMenuItemService)this._menuItemService.Clone(true);
            base.OnClone(context);
        }
    }
}