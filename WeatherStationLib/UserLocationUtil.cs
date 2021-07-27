using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.Foundation;

namespace WeatherStationLib
{
    public static class UserLocationUtil
    {
        private static Geolocator geolocator = new Geolocator();

        public static IAsyncOperation<Location> GetGeoLocationAsync()
        {
            return GetGeoLocationLatLngAsync().AsAsyncOperation();
        }

        private static async Task<Location> GetGeoLocationLatLngAsync()
        {
            geolocator.DesiredAccuracy = PositionAccuracy.High;
            var pos = await geolocator.GetGeopositionAsync();
            double latitude = pos.Coordinate.Point.Position.Latitude;
            double longitude = pos.Coordinate.Point.Position.Longitude;
            return new Location(latitude, longitude);
        }
    }

    public sealed class Location
    {
        public Location(double lat, double lng)
        {
            this.lat = lat;
            this.lng = lng;
        }

        public double lat { get; private set; }

        public double lng { get; private set; }
    }
}
