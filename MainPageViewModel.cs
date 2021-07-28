
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using WeatherStationLib;
using Windows.ApplicationModel.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace WeatherStation
{
    public class MainPageViewModel : INotifyPropertyChanged
    {
        private readonly WeatherClient weatherClient;

        public MainPageViewModel()
        {
            this.weatherClient = new WeatherClient();
            Task.Run(() => InitAsync());
        }

        private bool toggleSwitchOn = true;

        public int Temperature { get; private set; }

        public int TemperatureFeelsLike { get; private set; }

        public int Humidity { get; private set; }

        public string WeatherDesc { get; private set; }

        public int WindSpeed { get; private set; }

        public int Cloudiness { get; private set; }

        public bool ToggleSwitchOn
        {
            get
            {
                return this.toggleSwitchOn;
            }
            set
            {
                if (value != this.toggleSwitchOn)
                {
                    this.toggleSwitchOn = value;
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(this.ToggleSwitchOn)));
                }
            }
        }

        public int inputTemperatureMin { get; set; } = -50;

        public int inputTemperatureMax { get; set; } = 50;

        public int inputHumidityMin { get; set; } = 0;

        public int inputHumidityMax { get; set; } = 100;

        public WeatherCondition SelectedWeatherCondition { get; set; }

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
                    this.WeatherDesc = currentWeather.Weather.FirstOrDefault().Description;
                    this.Cloudiness = currentWeather.Clouds.All;
                    await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
                        () =>
                        {
                            this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(this.Temperature)));
                            this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(this.TemperatureFeelsLike)));
                            this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(this.Humidity)));
                            this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(this.WindSpeed)));
                            this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(this.WeatherDesc)));
                            this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(this.Cloudiness)));
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
