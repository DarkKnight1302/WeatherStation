using Microsoft.Toolkit.Uwp.Notifications;
using System;
using Windows.Data.Xml.Dom;

namespace BackgroundTasks
{
    public sealed class CurrentWeatherToast
    {
        public static XmlDocument GenerateCurrentWeatherToast(string icon)
        {
            return new ToastContent()
            {
                Scenario = ToastScenario.Reminder,
                Visual = new ToastVisual()
                {
                    BindingGeneric = new ToastBindingGeneric()
                    {
                        Children =
                        {
                            new AdaptiveText()
                            {
                                Text = "Your Favourite Weather is here!",
                                HintMaxLines = 1,
                            },
                            new AdaptiveText()
                            {
                                Text = "Go out and have fun!",
                                HintMaxLines = 1,
                            },
                        },
                        AppLogoOverride = new ToastGenericAppLogo()
                        {
                            Source = $"http://openweathermap.org/img/wn/{icon}@2x.png",
                            HintCrop = ToastGenericAppLogoCrop.None,
                        },
                        HeroImage = new ToastGenericHeroImage()
                        {
                            Source = "ms-appx:///Assets/rsz_weatherstation.jpg",
                        },
                    },
                },
                Actions = new ToastActionsCustom()
                {
                    Buttons =
                    {
                        new ToastButton("Weather Details", new Uri("weather-station:").OriginalString)
                         {
                            ActivationType = ToastActivationType.Protocol,
                        },
                        new ToastButtonDismiss("Got It!")
                    },
                },
                Duration = ToastDuration.Long,
                Audio = new ToastAudio()
                {
                    Silent = true,
                },
            }.GetXml();
        }
    }
}
