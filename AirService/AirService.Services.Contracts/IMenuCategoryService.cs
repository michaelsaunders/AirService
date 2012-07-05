using System.Linq;
using AirService.Model;

namespace AirService.Services.Contracts
{
    public interface IMenuCategoryService : IService<MenuCategory>, ICascadeEntity
    {
        IQueryable<MenuCategory> GetMenuCategoriesWithKids(int menuId);
        
        void UpdateSortOrder(int menuCategoryId, int index);
        
        MenuCategory Update(int venueId, MenuCategory menuCategory);
    }
}