
using System.ComponentModel;

namespace WeatherStation
{
    public class MainPageViewModel : INotifyPropertyChanged
    {
        public int Temperature => 40;

        public int TemperatureFeelsLike => 45;

        public int Humidity => 60;

        public string WeatherDesc => "Raining";

        public int WindSpeed => 20;

        public int Cloudiness => 5;

        public bool ToggleSwitchOn { get; set; } = true;

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
