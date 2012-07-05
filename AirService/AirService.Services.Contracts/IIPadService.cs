using System.Collections.Generic;
using AirService.Model;

namespace AirService.Services.Contracts
{
    public interface IIPadService : IService<iPad>
    {
        iPad FindByVenueIdAndPin(int venueId,
                                 string pin);

        void Update(iPad iPad);

        List<iPad> FindAllByVenueId(int venueId);

        IList<iPad> FindAllByIds(int venueId, params int[] ids);

        string GeneratePinCode(int venueId);

        void UpdateModelForMenus(ref iPad ipad, int[] selectedMenus);

        iPad FindByUdid(string udid);
    }
}
