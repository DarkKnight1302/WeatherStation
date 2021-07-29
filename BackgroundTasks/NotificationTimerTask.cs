using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WeatherStationLib;
using Windows.ApplicationModel.Background;
using Windows.Storage;
using Windows.UI.Notifications;

namespace BackgroundTasks
{
    public sealed class NotificationTimerTask : IBackgroundTask
    {
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            await BackgroundTaskUtils.RunBackgroundTaskAsync(taskInstance, this.RunCoreAsync).ConfigureAwait(false);
        }

        private async Task RunCoreAsync(CancellationToken arg)
        {
            WeatherClient weatherClient = new WeatherClient();
            Location location = UserLocationUtil.GetGeoLocationFromSettings();
            if (location == null)
            {
                location = await UserLocationUtil.GetGeoLocationAsync();
                if (location == null)
                {
                    Debug.WriteLine("Cannot pick user location");
                    return;
                }
            }
            CurrentWeatherApiResponse currentWeather = await weatherClient.GetCurrentWeatherUsingLatLongAsync(location.lat, location.lng);
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            UserCustomWeather userCustomWeather = new UserCustomWeather
            {
                compareTemperature = (bool)(localSettings.Values["compareTemperature"] ?? false),
                temperatureMin = (int)(localSettings.Values["temperatureMin"] ?? 0),
                temperatureMax = (int)(localSettings.Values["temperatureMax"] ?? 0),
                compareHumidity = (bool)(localSettings.Values["compareHumidity"] ?? false),
                humidityMin = (int)(localSettings.Values["humidityMin"] ?? 0),
                humidityMax = (int)(localSettings.Values["humidityMax"] ?? 0),
                compareWeatherCondition = (bool)(localSettings.Values["compareWeatherCondition"] ?? false),
                weatherCondition = (WeatherCondition)(localSettings.Values["weatherCondition"] ?? WeatherCondition.Any),
                compareWindSpeed = (bool)(localSettings.Values["compareWindSpeed"] ?? false),
                windspeedMax = (int)(localSettings.Values["windspeedMax"] ?? 0),
                windspeedMin = (int)(localSettings.Values["windspeedMin"] ?? 0),
                compareCloudiness = (bool)(localSettings.Values["compareCloudiness"] ?? false),
                cloudinessMin = (int)(localSettings.Values["cloudinessMin"] ?? 0),
                cloudinessMax = (int)(localSettings.Values["cloudinessMax"] ?? 0),
            };
            int notifAllowedHrMin = (int)(localSettings.Values["hourMin"] ?? 0);
            int notifAllowedHrMax = (int)(localSettings.Values["hourMax"] ?? 24);
            int dateOfLastNotif = (int)(localSettings.Values["notifDate"] ?? 0);
            bool notificationsEnabled = (bool)(localSettings.Values["notifEnabled"] ?? false);
            if (weatherClient.AreConditionsPreferableAsync(currentWeather, userCustomWeather) 
                && (DateTime.Now.Hour >= notifAllowedHrMin && DateTime.Now.Hour <= notifAllowedHrMax) 
                && DateTime.Now.Day != dateOfLastNotif && notificationsEnabled)
            {
                var content = CurrentWeatherToast.GenerateCurrentWeatherToast();
                ToastNotification toastNotification = new ToastNotification(content);
                ToastNotificationManager.CreateToastNotifier().Show(toastNotification);
                localSettings.Values["notifDate"] = DateTime.Now.Day;
            }
        }

        public static void Register()
        {
            if (BackgroundTaskRegistration.AllTasks.Any(
                r => string.Equals(r.Value.Name, nameof(NotificationTimerTask), StringComparison.Ordinal)))
            {
                // Background task is already registered
                return;
            }

            BackgroundTaskBuilder taskBuilder = new BackgroundTaskBuilder()
            {
                Name = nameof(NotificationTimerTask),
                TaskEntryPoint = typeof(NotificationTimerTask).FullName,
            };
            taskBuilder.SetTrigger(new TimeTrigger(15, false));
            taskBuilder.Register();
        }
    }
}
