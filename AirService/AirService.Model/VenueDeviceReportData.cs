namespace AirService.Model
{
    /// <summary>
    /// This is not a EF data model. just POCO
    /// </summary>
    public class VenueDeviceReportData
    {
        public string DeviceName
        {
            get;
            set;
        }

        public string Menu
        {
            get;
            set;
        }

        public string Category
        {
            get;
            set;
        }

        public string Item
        {
            get;
            set;
        }

        public int NumOrders
        {
            get;
            set;
        }

        public decimal Total
        {
            get;
            set;
        }
    }
}