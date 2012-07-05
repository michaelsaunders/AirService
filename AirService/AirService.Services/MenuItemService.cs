using System;
using System.Data;
using System.Linq;
using AirService.Data;
using AirService.Data.Contracts;
using AirService.Model;
using AirService.Services.Contracts;

namespace AirService.Services
{
    public class MenuItemService : SimpleService<MenuItem>, IMenuItemService
    {
        private IMenuItemOptionService _menuItemOptionService;

        public MenuItemService(IRepository<MenuItem> menuItemRepository, IMenuItemOptionService menuItemOptionService)
        {
            this.Repository = menuItemRepository;
            this._menuItemOptionService = menuItemOptionService;
        }

        public new IQueryable<MenuItem> FindAllByUser(Guid userId)
        {
            return ((MenuItemRepository)Repository).FindAllByUser(userId);
        }

        public override void InsertOrUpdate(MenuItem entity)
        {
            if (entity.Id == 0)
            {
                // set the sort order
                var currentMax = this.Repository.FindAll().Count(item => item.MenuCategoryId == entity.MenuCategoryId);
                entity.SortOrder = currentMax + 1;
            }

            base.InsertOrUpdate(entity);
        }

        public MenuItem GetMenuItemWithKids(int menuItemId)
        {
            this.Repository.SetContextOption(ContextOptions.LazyLoading, false);
            var menuWithProjection = from item in this.Repository.FindAll()
                                     where item.Id == menuItemId &&
                                           item.Status == SimpleModel.StatusActive
                                     select new
                                     {
                                         item,
                                         itemOptions =
                                              from itemOption in item.MenuItemOptions
                                              where itemOption.Status == SimpleModel.StatusActive
                                              select itemOption
                                     };

            return menuWithProjection.AsEnumerable().Select(i => i.item).Single();

        }

        public void UpdateSortOrder(int menuItemId, int index)
        {
            var menuItem = Repository.Find(menuItemId);
            var categoryId = menuItem.MenuCategoryId;
            var items = (from category in this.Repository.Set<MenuCategory>()
                         from item in category.MenuItems
                         where category.Id == categoryId
                               && item.Id != menuItemId &&
                               item.Status == SimpleModel.StatusActive
                         orderby item.SortOrder
                         select item).ToList();
            
            int i = 0;
            index = Math.Min(index, items.Count);
            items.Insert(index, menuItem);
            foreach (var item in items)
            {
                item.SortOrder = i++;
            }

            Repository.Save();
        }

        public MenuItem Update(int venueId, MenuItem menuItem)
        {
            if (this.Repository.Context.GetState(menuItem) == EntityState.Detached)
            {
                var existingItem = (from menu in this.Repository.Set<Menu>()
                                    from category in menu.MenuCategories
                                    from item in category.MenuItems
                                    where menu.VenueId == venueId &&
                                          item.Id == menuItem.Id
                                    select item).FirstOrDefault();
                if (existingItem == null)
                {
                    return null;
                }

                existingItem.Image = menuItem.Image;
                existingItem.Description = menuItem.Description;
                existingItem.Price = menuItem.Price;
                existingItem.Title = menuItem.Title;
                existingItem.MenuItemStatus = menuItem.MenuItemStatus; 
                 
                if(menuItem.MenuItemOptions != null)
                {
                    var existingOptions = existingItem.MenuItemOptions.Where(o=>o.Status != SimpleModel.StatusDeleted).ToList();
                    var newOptions = menuItem.MenuItemOptions.Where(o => o.Id == 0).ToList();
                    var optionsToUpdate = from submitted in menuItem.MenuItemOptions
                                          join previous in existingOptions on submitted.Id equals
                                              previous.Id
                                          select new
                                                     {
                                                         submitted,
                                                         previous
                                                     };
                    
                    var optionsToDelete = from previous in existingOptions
                                          where !(from submitted in menuItem.MenuItemOptions
                                                  select submitted.Id).Contains(previous.Id)
                                          select previous;
                    
                    foreach (var option in newOptions)
                    {
                        option.MenuItem = existingItem;
                        existingItem.MenuItemOptions.Add(option); 
                        this.Repository.Insert(option);
                    }

                    foreach (var option in optionsToDelete)
                    {
                        if (this.Repository.Set<OrderItem>().FirstOrDefault(o => o.MenuItemOptionId == option.Id) == null)
                        {
                            this.Repository.Set<MenuItemOption>().Remove(option);
                        }
                        else
                        {
                            option.Status = SimpleModel.StatusDeleted;
                        }
                    }

                    foreach (var optionPair in optionsToUpdate)
                    {
                        optionPair.previous.Title = optionPair.submitted.Title;
                        optionPair.previous.Price = optionPair.submitted.Price; 
                    }
                }

                menuItem = existingItem; 
            }

            this.Save();
            return menuItem;
        }

        public MenuItem Insert(MenuItem menuItem)
        {
            if (menuItem.MenuItemOptions != null)
            {
                foreach (MenuItemOption option in menuItem.MenuItemOptions)
                {
                    option.MenuItem = menuItem;
                    this.Repository.Insert(option);
                }
            }

            this.InsertOrUpdate(menuItem);
            this.Save();
            return menuItem;
        }

        public MenuItem CreateCopy(MenuItem menuItem)
        {
            var clone = (MenuItem)menuItem.Clone();
            this.InsertOrUpdate(clone);
            this.Save();
            return clone;
        }

        public override void Delete(int id)
        {
            var entity = Repository.Find(id);
            foreach (var menuItemOption in entity.MenuItemOptions)
            {
                _menuItemOptionService.Delete(menuItemOption.Id);
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
                                        where orderItem.MenuItemId == id
                                        && orderItem.OrderStatus != Order.OrderStatusFinalized
                                        && orderItem.OrderStatus != Order.OrderStatusCancelled
                                        select orderItem;
                if (currentOrderItems.Any())
                {
                    message = "Unable to delete. Current orders exist for this item.";
                    return false;
                }
            }

            this.Delete(id);

            return true;
        }

        #endregion

        protected override void OnClone(IAirServiceContext context)
        {
            _menuItemOptionService = (IMenuItemOptionService) this._menuItemOptionService.Clone(true);
            base.OnClone(context);
        }
    }
}