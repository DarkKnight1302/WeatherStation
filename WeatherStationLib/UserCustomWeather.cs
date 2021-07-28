
namespace WeatherStationLib
{
    public sealed class UserCustomWeather
    {
        /// <summary>
        /// Compare temperature only when this is true.
        /// </summary>
        public bool compareTemperature { get; set; }

        public int temperatureMin { get; set; }

        public int temperatureMax { get; set; }

        /// <summary>
        /// Compare humidity when this is true.
        /// </summary>
        public bool compareHumidity { get; set; }

        public int humidityMin { get; set; }

        public int humidityMax { get; set; }

        /// <summary>
        /// Compare weather condition only when this is true.
        /// </summary>
        public bool compareWeatherCondition { get; set; }

        public WeatherCondition weatherCondition { get; set; }

        /// <summary>
        /// Compare wind speed only when this is true.
        /// </summary>
        public bool compareWindSpeed { get; set; }

        public int windspeedMin { get; set; }

        public int windspeedMax { get; set; }

        /// <summary>
        /// Compare cloudiness only when this is true.
        /// </summary>
        public bool compareCloudiness { get; set; }

        public int cloudinessMin { get; set; }

        public int cloudinessMax { get; set; }
    }
}
