﻿using Microsoft.Toolkit.Uwp.Notifications;
using System;
using Windows.Data.Xml.Dom;

namespace BackgroundTasks
{
    public sealed class FutureWeatherToast
    {
        public static XmlDocument GenerateFutureWeatherToast(string icon, DateTimeOffset dateTime)
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
                                Text = $"Your Favourite Weather is Arriving soon! at {dateTime.DateTime}",
                                HintMaxLines = 2,
                            },
                            new AdaptiveText()
                            {
                                Text = "Plan ahead!",
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
                            Source = "ms-appx:///Assets/beautiful-weather_364x180.jpg",
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