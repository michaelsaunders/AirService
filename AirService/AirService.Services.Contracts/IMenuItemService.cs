using AirService.Model;

namespace AirService.Services.Contracts
{
    public interface IMenuItemService : IService<MenuItem>, ICascadeEntity
    {
        MenuItem GetMenuItemWithKids(int menuItemId);

        void UpdateSortOrder(int menuItemId, int index);

        MenuItem Update(int venueId, MenuItem menuItem);

        MenuItem Insert(MenuItem menuItem);

        MenuItem CreateCopy(MenuItem menuItem);
    }
}