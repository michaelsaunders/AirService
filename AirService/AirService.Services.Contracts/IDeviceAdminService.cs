using System.Collections.Generic;
using AirService.Model;

namespace AirService.Services.Contracts
{
    public interface IDeviceAdminService
    {
        IEnumerable<DeviceAdmin> FindAllByVenueId(int venueId);

        DeviceAdmin FindByDeviceAdminUserEmail(int venueId,
                                               string deviceAdminEmail);

        void Insert(DeviceAdmin deviceAdmin);

        void Update(DeviceAdmin deviceAdmin);

        void Delete(int venueId,
                    int deviceAdminId);

        DeviceAdmin GetDeviceAdmin(string email,
                                   string password);

        bool SendPasswordEmail(DeviceAdmin deviceAdmin, string emailTemplatePath);

        bool SendPasswordEmail(string deviceAdminEmail,
                               string emailTemplatePath);

        iPad GetAdminDeviceByMatchingPin(string deviceAdminEmail,
                                         string devicePin);

        DeviceAdmin FindByDeviceAdminId(int venueId,
                                        int deviceAdminId);
    }
}