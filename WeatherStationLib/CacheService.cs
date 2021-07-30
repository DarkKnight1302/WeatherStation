using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Streams;

namespace WeatherStationLib
{
    public sealed class CacheService
    {
        private const string CacheFileName = "ForecastedCacheFile";

        public IAsyncOperation<bool> UpdateDataAsync(ForecastedWeatherApiResponse data)
        {
            return this.SetDataAsync(data).AsAsyncOperation<bool>();
        }

        public IAsyncOperation<HourlyForecastedResponse> FetchDataAsync(IList<DateTimeOffset> dtOffsetList)
        {
            return this.GetDataAsync(dtOffsetList).AsAsyncOperation<HourlyForecastedResponse>();
        }

        private async Task<bool> SetDataAsync(ForecastedWeatherApiResponse data)
        {
            try
            {
                StorageFile forecastedDataCacheFile = await ApplicationData.Current.LocalCacheFolder.CreateFileAsync(CacheFileName, CreationCollisionOption.ReplaceExisting);
                await FileIO.WriteBytesAsync(forecastedDataCacheFile, ToByteArray(data));
            }
            catch (Exception e)
            {
                return false;
            }
            return true;
        }

        private async Task<HourlyForecastedResponse> GetDataAsync(IList<DateTimeOffset> dtOffsetList)
        {
            ForecastedWeatherApiResponse obj = null;
            IStorageItem item = await ApplicationData.Current.LocalCacheFolder.TryGetItemAsync(CacheFileName);
            if (item?.IsOfType(StorageItemTypes.File) == true)
            {
                StorageFile forecastedDataCacheFile = await ApplicationData.Current.LocalCacheFolder.GetFileAsync(CacheFileName);
                using (IRandomAccessStreamWithContentType stream = await forecastedDataCacheFile.OpenReadAsync())
                using (DataReader reader = new DataReader(stream))
                {
                    await reader.LoadAsync((uint)stream.Size);
                    string jsonString = reader.ReadString((uint)stream.Size);
                    if(String.IsNullOrEmpty(jsonString))
                    {
                        return null;
                    }
                    obj = JsonConvert.DeserializeObject<ForecastedWeatherApiResponse>(jsonString);
                }
            }
            List<Hourly> hourlyList = new List<Hourly>();
            foreach (DateTimeOffset dtOffset in dtOffsetList)
            {
                long unixTime = dtOffset.ToUnixTimeSeconds();
                long diff = long.MaxValue;
                Hourly hourly = null;
                if (obj != null)
                {
                    foreach (Hourly hrly in obj.Hourly)
                    {
                        long absDiff = Math.Abs(hrly.Dt - unixTime);
                        if (absDiff < diff)
                        {
                            hourly = hrly;
                            diff = absDiff;
                        }
                    }
                    hourlyList.Add(hourly);
                }
            }
            var response = new HourlyForecastedResponse
            {
                Hourly = hourlyList,
            };
            return response;
        }

        private byte[] ToByteArray(ForecastedWeatherApiResponse data)
        {
            string jsonString = JsonConvert.SerializeObject(data);
            return Encoding.ASCII.GetBytes(jsonString); // use this to convert back: ```string someString = Encoding.ASCII.GetString(bytes);```

        }
    }
}
