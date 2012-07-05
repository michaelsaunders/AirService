using System;
using System.Net;
using System.Web;

namespace AirService.Services
{
    public class GeoLocation
    {
        private static readonly string _privateKey = "ABQIAAAA3SuGOGe8DseKejh8h6vocBQ996puoSw8kMBhoeK0tXU2BxXYkhRWb_fApTy3PCq1aFXQTfZEOIB4NQ";

        private static Uri GetGeocodeUri(string address, string googleUri, string outputType)
        {
            address = HttpUtility.UrlEncode(address);
            return new Uri(String.Format("{0}{1}&output={2}&key={3}", googleUri, address, outputType, _privateKey));
        }

        /// <summary>
        /// Gets a Coordinate from an address
        /// </summary>
        /// <param name="address">Address</param>
        /// <returns>
        /// A spatial coordinate that contains the latitude and longitude of the address.
        /// </returns>
        /// <remarks>
        /// 	<example>1600 Amphitheatre Parkway Mountain View, CA 94043</example>
        /// </remarks>
        public static Coordinate GetCoordinates(string address)
        {
            return GetCoordinates(address, "http://maps.google.com/maps/geo?q=", "csv");
        }

        /// <summary>
        /// Gets a Coordinate from a address.
        /// </summary>
        /// <param name="address">Address</param>
        /// <param name="googleUri">http://maps.google.com/maps/geo?q=</param>
        /// <param name="outputType">Available options: csv, xml, kml, json</param>
        /// <remarks>
        /// <example>1600 Amphitheatre Parkway Mountain View, CA 94043</example>
        /// </remarks>
        /// <returns>A spatial coordinate that contains the latitude and longitude of the address.</returns>
        public static Coordinate GetCoordinates(string address, string googleUri, string outputType)
        {

            var client = new WebClient();
            var uri = GetGeocodeUri(address, googleUri, outputType);


            /* The first number is the status code, 
            * the second is the accuracy, 
            * the third is the latitude, 
            * the fourth one is the longitude.
            */
            var geocodeInfo = client.DownloadString(uri).Split(',');

            return new Coordinate(Convert.ToDouble(geocodeInfo[2]), Convert.ToDouble(geocodeInfo[3]));
        }

    }

    public interface ISpatialCoordinate
    {
        double Latitude { get; set; }
        double Longitude { get; set; }
    }

    /// <summary>
    /// Coordinate structure for latitude and longitude.
    /// </summary>
    public struct Coordinate : ISpatialCoordinate
    {
        private double _latitude;
        private double _longitude;

        public Coordinate(double latitude, double longitude)
        {
            _latitude = latitude;
            _longitude = longitude;
        }

        #region ISpatialCoordinate Members

        public double Latitude
        {
            get
            {
                return _latitude;
            }
            set
            {
                this._latitude = value;
            }
        }

        public double Longitude
        {
            get
            {
                return _longitude;
            }
            set
            {
                this._longitude = value;
            }
        }

        #endregion
    }

}
