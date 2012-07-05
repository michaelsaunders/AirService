using AirService.Model;

namespace AirService.Services.Security
{
    public class AirServiceVenueUserIdentity : AirServiceIdentity
    {
        private readonly iPad _device;
        private readonly Venue _venue;

        public AirServiceVenueUserIdentity(string name, Venue venue)
            : base(name)
        {
            this._venue = venue;
        }

        public AirServiceVenueUserIdentity(string name, iPad device)
            : base(name)
        {
            this._device = device;
            this._venue = device.Venue;
        }

        public Venue Venue
        {
            get { return this._venue; }
        }

        public iPad Device
        {
            get { return this._device; }
        }

        public override int VenueId
        {
            get { return this._venue.Id; }
        }

        public string DeviceUuid
        {
            get
            {
                if (this._device != null)
                {
                    return this._device.Udid;
                }

                return null;
            }
        }

        public int DeviceId
        {
            get { return this._device.Id; }
        }
    }
}
