
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using WeatherStationLib;
using Windows.ApplicationModel.Core;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace WeatherStation
{
    public class MainPageViewModel : INotifyPropertyChanged
    {
        private readonly WeatherClient weatherClient;
        private readonly ApplicationDataContainer localSettings;

        public MainPageViewModel()
        {
            this.weatherClient = new WeatherClient();
            this.localSettings = ApplicationData.Current.LocalSettings;
            if (localSettings.Values["temperatureMin"] == null)
            {
                localSettings.Values["temperatureMin"] = -50;
            }
            if (localSettings.Values["temperatureMax"] == null)
            {
                localSettings.Values["temperatureMax"] = 50;
            }
            if (localSettings.Values["humidityMin"] == null)
            {
                localSettings.Values["humidityMin"] = 0;
            }
            if (localSettings.Values["windspeedMin"] == null)
            {
                localSettings.Values["windspeedMin"] = 0;
            }
            if (localSettings.Values["cloudinessMin"] == null)
            {
                localSettings.Values["cloudinessMin"] = 0;
            }
            if (localSettings.Values["cloudinessMax"] == null)
            {
                localSettings.Values["cloudinessMax"] = 100;
            }
            if (localSettings.Values["weatherCondition"] == null)
            {
                localSettings.Values["weatherCondition"] = (int)WeatherCondition.Any;
            }
            if (localSettings.Values["humidityMax"] == null)
            {
                localSettings.Values["humidityMax"] = 100;
            }
            if (localSettings.Values["windspeedMax"] == null)
            {
                localSettings.Values["windspeedMax"] = 500;
            }
            Task.Run(() => InitAsync());
        }

        public int Temperature { get; private set; }

        public int TemperatureFeelsLike { get; private set; }

        public int Humidity { get; private set; }

        public string WeatherDesc { get; private set; }

        public int WindSpeed { get; private set; }

        public int Cloudiness { get; private set; }

        public string StationLocation { get; private set; }

        public bool ToggleSwitchOn
        {
            get
            {
                return (bool)localSettings.Values["notifEnabled"];
            }
            set
            {
                if (value != (bool)localSettings.Values["notifEnabled"])
                {
                    localSettings.Values["notifEnabled"] = value;
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(this.ToggleSwitchOn)));
                }
            }
        }

        public int inputTemperatureMin
        {
            get
            {
                return (int)this.localSettings.Values["temperatureMin"];
            }
            set
            {
                localSettings.Values["temperatureMin"] = value;
                localSettings.Values["compareTemperature"] = true;
            }
        }

        public int inputTemperatureMax
        {
            get
            {
                return (int)localSettings.Values["temperatureMax"];
            }
            set
            {
                localSettings.Values["temperatureMax"] = value;
                localSettings.Values["compareTemperature"] = true;
            }
        }

        public int inputHumidityMin
        {
            get
            {
                return (int)localSettings.Values["humidityMin"];
            }
            set
            {
                localSettings.Values["humidityMin"] = value;
                localSettings.Values["compareHumidity"] = true;
            }
        }

        public int inputHumidityMax
        {
            get
            {
                return (int)localSettings.Values["humidityMax"];
            }
            set
            {
                localSettings.Values["humidityMax"] = value;
                localSettings.Values["compareHumidity"] = true;
            }
        }

        public int inputWindSpeedMin
        {
            get
            {
                return (int)localSettings.Values["windspeedMin"];
            }
            set
            {
                localSettings.Values["windspeedMin"] = value;
                localSettings.Values["compareWindSpeed"] = true;
            }
        }

        public int inputWindSpeedMax
        {
            get
            {
                return (int)localSettings.Values["windspeedMax"];
            }
            set
            {
                localSettings.Values["windspeedMax"] = value;
                localSettings.Values["compareWindSpeed"] = true;
            }
        }

        public int inputCloudinessMin
        {
            get
            {
                return (int)localSettings.Values["cloudinessMin"];
            }
            set
            {
                localSettings.Values["cloudinessMin"] = value;
                localSettings.Values["compareCloudiness"] = true;
            }
        }

        public int inputCloudinessMax
        {
            get
            {
                return (int)localSettings.Values["cloudinessMax"];
            }
            set
            {
                localSettings.Values["cloudinessMax"] = value;
                localSettings.Values["compareCloudiness"] = true;
            }
        }

        public WeatherCondition SelectedWeatherCondition
        {
            get
            {
                return (WeatherCondition)(int)localSettings.Values["weatherCondition"];
            }
            set
            {
                localSettings.Values["weatherCondition"] = (int)value;
                if (value != WeatherCondition.Any)
                {
                    localSettings.Values["compareWeatherCondition"] = true;
                }
                else
                {
                    localSettings.Values["compareWeatherCondition"] = false;
                }
            }
        }

        public int SelectedWeatherIndex
        {
            get
            {
                return (int)localSettings.Values["weatherCondition"];
            }
        }

        public ObservableCollection<WeatherCondition> WeatherConditions { get; set; } = new ObservableCollection<WeatherCondition>();

        public event PropertyChangedEventHandler PropertyChanged;


        private async void InitAsync()
        {
            Location location = UserLocationUtil.GetGeoLocationFromSettings();
            if (location == null)
            {
                location = await UserLocationUtil.GetGeoLocationAsync();
            }
            if (location != null)
            {
                CurrentWeatherApiResponse currentWeather = await this.weatherClient.GetCurrentWeatherUsingLatLongAsync(location.lat, location.lng);
                if (currentWeather != null)
                {
                    this.Temperature = (int)currentWeather.Main.Temp;
                    this.TemperatureFeelsLike = (int)currentWeather.Main.FeelsLike;
                    this.Humidity = (int)currentWeather.Main.Humidity;
                    this.WindSpeed = (int)currentWeather.Wind.Speed;
                    this.WeatherDesc = currentWeather.Weather.FirstOrDefault().Description.ToUpper();
                    this.Cloudiness = currentWeather.Clouds.All;
                    this.StationLocation = currentWeather.Name + ", " + currentWeather.Sys.Country;
                    await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
                        () =>
                        {
                            this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(this.Temperature)));
                            this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(this.TemperatureFeelsLike)));
                            this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(this.Humidity)));
                            this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(this.WindSpeed)));
                            this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(this.WeatherDesc)));
                            this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(this.Cloudiness)));
                            this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(this.StationLocation)));
                            var weatherConditionEnums = Enum.GetValues(typeof(WeatherCondition));
                            foreach (var weatherCondition in weatherConditionEnums)
                            {
                                this.WeatherConditions.Add((WeatherCondition)weatherCondition);
                            }
                        });
                }
            }
        }
    }
}
