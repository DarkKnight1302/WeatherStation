using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.Storage;

namespace WeatherStationLib
{
    public static class UserLocationUtil
    {
        private static ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
        private static string locationKey = "Location";
        private static Geolocator geolocator = new Geolocator();

        public static IAsyncOperation<Location> GetGeoLocationAsync()
        {
            return GetGeoLocationLatLngAsync().AsAsyncOperation();
        }

        public static Location GetGeoLocationFromSettings()
        {
            Task.Run(async () => await GetGeoLocationLatLngAsync());
            if (localSettings.Values.ContainsKey(locationKey))
            {
                var composite = (ApplicationDataCompositeValue)localSettings.Values[locationKey];
                return new Location { lat = (double)composite["lat"], lng = (double)composite["lng"] };
            }
            return null;
        }

        private static async Task<Location> GetGeoLocationLatLngAsync()
        {
            geolocator.DesiredAccuracy = PositionAccuracy.High;
            var pos = await geolocator.GetGeopositionAsync();
            double latitude = pos.Coordinate.Point.Position.Latitude;
            double longitude = pos.Coordinate.Point.Position.Longitude;
            Location location = new Location { lat = latitude, lng = longitude };
            ApplicationDataCompositeValue composite = new ApplicationDataCompositeValue();
            composite["lat"] = latitude;
            composite["lng"] = longitude;
            localSettings.Values[locationKey] = composite;
            return location;
        }
    }

    [Serializable]
    public sealed class Location
    {

        public double lat { get; set; }

        public double lng { get; set; }
    }
}
