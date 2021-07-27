using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;

namespace WeatherStationLib
{
    public sealed class WeatherClient
    {
        private const string AppId = "53d9fe6f16a4937defff2c0fb9ab8466";
        private readonly HttpClient httpClient;

        public WeatherClient() 
        {
            httpClient = new HttpClient();
        }

        public CurrentWeatherApiResponse GetCurrentWeatherUsingLatLong(double lat, double lng)
        {
            string uri = $"https://api.openweathermap.org/data/2.5/weather?lat={lat}&lon={lng}&appid={AppId}";
            string jsonResponseString = this.httpClient.GetAsync(uri).GetAwaiter().GetResult().Content.ReadAsStringAsync().Result;
            return JsonConvert.DeserializeObject<CurrentWeatherApiResponse>(jsonResponseString);
        }

        public CurrentWeatherApiResponse GetCurrentWeatherUsingZipCode(string zipCode, string countryCode)
        {
            string uri = $"https://api.openweathermap.org/data/2.5/weather?zip={zipCode},{countryCode}&appid={AppId}";
            string jsonResponseString = this.httpClient.GetAsync(uri).GetAwaiter().GetResult().Content.ReadAsStringAsync().Result;
            return JsonConvert.DeserializeObject<CurrentWeatherApiResponse>(jsonResponseString);
        }

        public ForecastedWeatherApiResponse GetForecastedWeatherUsingLatLong(double lat, double lng)
        {
            string exclude = "minutely,daily,current,alerts";
            string uri = $"https://api.openweathermap.org/data/2.5/onecall?lat={lat}&lon={lng}&exclude={exclude}&appid={AppId}";
            string jsonResponseString = this.httpClient.GetAsync(uri).GetAwaiter().GetResult().Content.ReadAsStringAsync().Result;
            return JsonConvert.DeserializeObject<ForecastedWeatherApiResponse>(jsonResponseString);
        }


    }
}
