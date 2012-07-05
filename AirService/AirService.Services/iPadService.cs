using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using AirService.Data;
using AirService.Data.Contracts;
using AirService.Model;
using AirService.Services.Contracts;

namespace AirService.Services
{
    public class iPadService : SimpleService<iPad>, IIPadService
    {
        private readonly IMenuService _menuService;

        public iPadService(IRepository<iPad> iPadRepository, IMenuService menuService)
        {
            this.Repository = iPadRepository;
            this._menuService = menuService;
        }

        public override IQueryable<iPad> FindAllByUser(Guid userId)
        {
            return ((IPadRepository)Repository).FindAllByUser(userId);
        }

        public IQueryable<iPad> FindAllByUserIncluding(Guid userId, params System.Linq.Expressions.Expression<Func<iPad, object>>[] includeProperties)
        {
            return ((IPadRepository)Repository).FindAllByUserIncluding(userId, true, includeProperties);
        }

        public void UpdateModelForMenus(ref iPad ipad, int[] selectedMenus)
        {
            ipad.AssignedMenus = ipad.AssignedMenus ?? new List<DeviceMenuOption>();
            // remove deleted items
            var deletedMenus = ipad.AssignedMenus.Where(m => !selectedMenus.Contains(m.MenuId)).ToList();
            foreach (var deletedMenu in deletedMenus)
            {
                ipad.AssignedMenus.Remove(deletedMenu);
                deletedMenu.Menu.AssignedDevices.Remove(deletedMenu);
            }

            // add new items
            foreach (var selectedMenu in selectedMenus)
            {
                if (ipad.AssignedMenus.FirstOrDefault(m => m.MenuId == selectedMenu) == null)
                {
                    var menu = this._menuService.Find(selectedMenu);
                    var relation = new DeviceMenuOption
                                       {
                                           iPad = ipad,
                                           iPadId = ipad.Id,
                                           Menu = menu,
                                           MenuId = menu.Id,
                                           Print = true
                                       };
                    ipad.AssignedMenus.Add(relation);
                    menu.AssignedDevices.Add(relation);
                }
            }
        }

        public iPad FindByUdid(string udid)
        {
            this.Repository.SetContextOption(ContextOptions.ProxyCreation, false);
            return this.Repository.FindAll().Include(i => i.Venue).FirstOrDefault(i => i.Udid == udid);
        }

        public iPad FindByVenueIdAndPin(int venueId, string pin)
        {
            this.Repository.SetContextOption(ContextOptions.ProxyCreation, false);
            return (from ipad in this.Repository.FindAll().Include(p => p.Venue)
                    where
                        ipad.VenueId == venueId &&
                        ipad.Pin == pin &&
                        ((ipad.Venue.VenueAccountType & (int) VenueAccountTypes.AccountTypeEvaluation) == 0)
                    select ipad).FirstOrDefault();
        }

        public void Update(iPad iPad)
        {
            if (!iPad.IsDeliveryEnabled && !iPad.IsPickupEnabled)
            {
                throw new ServiceFaultException(1041,
                                                Resources.Err1041BothDeliveryAndPickupDisabled);
            }

            this.Repository.Update(iPad);
            this.Repository.Save();
        }

        public List<iPad> FindAllByVenueId(int venueId)
        {
            return this.Repository.FindAll().Where(ipad => ipad.VenueId == venueId).ToList();
        }

        /// <summary>
        /// Get all ipads. venueId is used as a safeguard to ensure no other venue's ipads are returned. 
        /// </summary>
        public IList<iPad> FindAllByIds(int venueId, params int[] ids)
        {
            return this.Repository.FindAll().Where(ipad => ipad.VenueId == venueId && ids.Contains(ipad.Id)).ToList();
        }

        public string GeneratePinCode(int venueId)
        {
            var ipadPinCodes = this.Repository.FindAll().Where(ipad => ipad.VenueId == venueId).Select(ipad => ipad.Pin);
            var random = new Random();
            var pinCode = random.Next(0, 999999);
            while (ipadPinCodes.Contains(pinCode.ToString().PadLeft(6, '0')))
            {
                pinCode = random.Next(0, 999999);
            }

            return pinCode.ToString().PadLeft(6, '0');
        }
    }
}