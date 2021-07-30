using System;
using System.Collections.Generic;
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
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            int notifAllowedHrMin = (int)(localSettings.Values["hourMin"] ?? 0);
            int notifAllowedHrMax = (int)(localSettings.Values["hourMax"] ?? 24);
            bool notificationsEnabled = (bool)(localSettings.Values["notifEnabled"] ?? false);

            if (DateTime.Now.Hour < notifAllowedHrMin || DateTime.Now.Hour > notifAllowedHrMax || !notificationsEnabled)
            {
                Debug.WriteLine("Outside time range or notif disabled.");
                return;
            }
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
            List<Task> taskList = new List<Task>();
            taskList.Add(Task.Run(async() => await ShowCurrentWeatherToast(weatherClient, location, localSettings).ConfigureAwait(false)));
            await Task.WhenAll(taskList).ConfigureAwait(false);
        }

        private async Task ShowFutureWeatherToast(WeatherClient weatherClient, Location location, ApplicationDataContainer localSettings)
        {
            CacheService cacheService = weatherClient.CacheService;
            IList<DateTimeOffset> next24Hours = new List<DateTimeOffset>();
            DateTimeOffset current = DateTimeOffset.Now;
            for (int i = 1; i <= 24; i++) {
                next24Hours.Add(current.AddHours(i));
            }
            var forecastResponse = await cacheService.FetchDataAsync(next24Hours);
            if (forecastResponse != null && forecastResponse.Hourly.Any())
            {
                UserCustomWeather userCustomWeather = GetUserCustomWeather(localSettings);
                int dateOfLastNotif = (int)(localSettings.Values["notifDateFuture"] ?? 0);
#if DEBUG
                dateOfLastNotif = 0;
#endif
               /* foreach (var hourWeather in forecastResponse.Hourly)
                {
                    if (weatherClient.AreConditionsPreferableAsync(hourWeather, userCustomWeather)
                        && DateTime.Now.Day != dateOfLastNotif)
                    {
                        var content = CurrentWeatherToast.GenerateCurrentWeatherToast();
                        ToastNotification toastNotification = new ToastNotification(content);
                        ToastNotificationManager.CreateToastNotifier().Show(toastNotification);
                        localSettings.Values["notifDateFuture"] = DateTime.Now.Day;
                    }
                }*/
            }
        }

        private async Task ShowCurrentWeatherToast(WeatherClient weatherClient, Location location, ApplicationDataContainer localSettings)
        {
            CurrentWeatherApiResponse currentWeather = await weatherClient.GetCurrentWeatherUsingLatLongAsync(location.lat, location.lng);
            UserCustomWeather userCustomWeather = GetUserCustomWeather(localSettings);
            int dateOfLastNotif = (int)(localSettings.Values["notifDate"] ?? 0);
#if DEBUG
            dateOfLastNotif = 0;
#endif
            if (weatherClient.AreConditionsPreferableAsync(currentWeather, userCustomWeather)
                && DateTime.Now.Day != dateOfLastNotif)
            {
                var content = CurrentWeatherToast.GenerateCurrentWeatherToast(currentWeather.Weather.First().Icon);
                ToastNotification toastNotification = new ToastNotification(content);
                ToastNotificationManager.CreateToastNotifier().Show(toastNotification);
                localSettings.Values["notifDate"] = DateTime.Now.Day;
            }
        }

        private UserCustomWeather GetUserCustomWeather(ApplicationDataContainer localSettings)
        {
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
            return userCustomWeather;
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
            taskBuilder.AddCondition(new SystemCondition(SystemConditionType.UserPresent));
            taskBuilder.Register();
        }
    }
}
