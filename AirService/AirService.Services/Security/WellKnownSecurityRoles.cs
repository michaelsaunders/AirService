namespace AirService.Services.Security
{
    public static class WellKnownSecurityRoles
    {
        public const string AllVenueUsers = "Venue Administrator,Device Administrator,Venue User";
        public const string SystemAdministrators = "System Administrator";
        public const string VenueAdministrators = "Venue Administrator";
        public const string DeviceAdministrators = "Device Administrator";
        public const string SystemAdministratorsAndVenueAdministrators = "System Administrator,Venue Administrator";
        public const string VenueAdministratorAndDeviceAdministrators = "Venue Administrator,Device Administrator";
        public const string Customers = "Customer";
        public const string VenueUsers = "Venue User";
    }
}