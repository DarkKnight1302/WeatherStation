using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.Foundation;

namespace WeatherStationLib
{
    public sealed class WeatherClient
    {
        private const string AppId = "53d9fe6f16a4937defff2c0fb9ab8466";
        private readonly HttpClient httpClient;
        public CacheService CacheService { get; private set; }

        public WeatherClient() 
        {
            httpClient = new HttpClient();
            CacheService = new CacheService();
        }

        public IAsyncOperation<CurrentWeatherApiResponse> GetCurrentWeatherUsingLatLongAsync(double lat, double lng)
        {
            return this.GetCurrentWeatherUsingLatLong(lat, lng).AsAsyncOperation<CurrentWeatherApiResponse>();
        }

        public IAsyncOperation<CurrentWeatherApiResponse> GetCurrentWeatherUsingZipCodeAsync(string zipCode, string countryCode)
        {
            return this.GetCurrentWeatherUsingZipCode(zipCode, countryCode).AsAsyncOperation<CurrentWeatherApiResponse>();
        }

        public IAsyncOperation<ForecastedWeatherApiResponse> GetForecastedWeatherUsingLatLongAsync(double lat, double lng)
        {
            return this.GetForecastedWeatherUsingLatLong(lat, lng).AsAsyncOperation<ForecastedWeatherApiResponse>();
        }

        public bool AreConditionsPreferableAsync(CurrentWeatherApiResponse current, UserCustomWeather userWeather)
        {
            return this.CompareWeather(current, userWeather);
        }

        private bool CompareWeather(CurrentWeatherApiResponse current, UserCustomWeather userWeather)
        {
            int currentTemp = (int)current.Main.Temp;
            int currentHumidity = current.Main.Humidity;
            int currentCloudiness = current.Clouds.All;
            int currentWinds = (int)current.Wind.Speed;
            if (userWeather.compareTemperature && (currentTemp < userWeather.temperatureMin || currentTemp > userWeather.temperatureMax))
            {
                return false;
            }
            else if (userWeather.compareHumidity && (currentHumidity < userWeather.humidityMin || currentHumidity > userWeather.humidityMax))
            {
                return false;
            }
            else if (userWeather.compareWindSpeed && (currentWinds < userWeather.windspeedMin || currentWinds > userWeather.windspeedMax))
            {
                return false;
            }
            else if (userWeather.compareCloudiness && (currentCloudiness < userWeather.cloudinessMin || currentCloudiness > userWeather.cloudinessMax))
            {
                return false;
            }
            
            if (userWeather.compareWeatherCondition)
            {
                foreach (Weather wthr in current.Weather)
                {
                    if (Enum.TryParse(wthr.Main, out WeatherCondition realWeatherCondition))
                    {
                        if (realWeatherCondition == userWeather.weatherCondition)
                        {
                            return true;
                        }
                        return false;
                    }
                }
                return false;
            }
            return true;
        }

        public bool IsMatchForcastedWeather(Hourly current, UserCustomWeather userWeather)
        {
            int currentTemp = (int)current.Temp;
            int currentHumidity = current.Humidity;
            int currentCloudiness = current.Clouds;
            int currentWinds = (int)current.WindSpeed;
            if (userWeather.compareTemperature && (currentTemp < userWeather.temperatureMin || currentTemp > userWeather.temperatureMax))
            {
                return false;
            }
            else if (userWeather.compareHumidity && (currentHumidity < userWeather.humidityMin || currentHumidity > userWeather.humidityMax))
            {
                return false;
            }
            else if (userWeather.compareWindSpeed && (currentWinds < userWeather.windspeedMin || currentWinds > userWeather.windspeedMax))
            {
                return false;
            }
            else if (userWeather.compareCloudiness && (currentCloudiness < userWeather.cloudinessMin || currentCloudiness > userWeather.cloudinessMax))
            {
                return false;
            }

            if (userWeather.compareWeatherCondition)
            {
                foreach (Weather wthr in current.Weather)
                {
                    if (Enum.TryParse(wthr.Main, out WeatherCondition realWeatherCondition))
                    {
                        if (realWeatherCondition == userWeather.weatherCondition)
                        {
                            return true;
                        }
                        return false;
                    }
                }
                return false;
            }
            return true;
        }

        private async Task<CurrentWeatherApiResponse> GetCurrentWeatherUsingLatLong(double lat, double lng)
        {
            string uri = $"https://api.openweathermap.org/data/2.5/weather?lat={lat}&lon={lng}&appid={AppId}&units=metric";
            var response = await this.httpClient.GetAsync(uri).ConfigureAwait(false);
            return JsonConvert.DeserializeObject<CurrentWeatherApiResponse>(response.Content.ReadAsStringAsync().Result);
        }

        private async Task<CurrentWeatherApiResponse> GetCurrentWeatherUsingZipCode(string zipCode, string countryCode)
        {
            string uri = $"https://api.openweathermap.org/data/2.5/weather?zip={zipCode},{countryCode}&appid={AppId}&units=metric";
            var response = await this.httpClient.GetAsync(uri).ConfigureAwait(false);
            return JsonConvert.DeserializeObject<CurrentWeatherApiResponse>(response.Content.ReadAsStringAsync().Result);
        }

        private async Task<ForecastedWeatherApiResponse> GetForecastedWeatherUsingLatLong(double lat, double lng)
        {
            string exclude = "minutely,daily,current,alerts";
            string uri = $"https://api.openweathermap.org/data/2.5/onecall?lat={lat}&lon={lng}&exclude={exclude}&appid={AppId}&units=metric";
            var response = await this.httpClient.GetAsync(uri).ConfigureAwait(false);
            var data = JsonConvert.DeserializeObject<ForecastedWeatherApiResponse>(response.Content.ReadAsStringAsync().Result);
            await this.CacheService.UpdateDataAsync(data);
            return data;
        }


    }
}
