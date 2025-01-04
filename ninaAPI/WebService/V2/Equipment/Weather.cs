#region "copyright"

/*
    Copyright © 2024 Christian Palm (christian@palm-family.de)
    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

#endregion "copyright"

using EmbedIO;
using EmbedIO.Routing;
using NINA.Core.Utility;
using NINA.Equipment.Equipment.MyWeatherData;
using NINA.Equipment.Interfaces.Mediator;
using ninaAPI.Utility;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ninaAPI.WebService.V2
{
    public partial class ControllerV2
    {
        private static readonly Func<object, EventArgs, Task> WeatherConnectedHandler = async (_, _) => await WebSocketV2.SendAndAddEvent("WEATHER-CONNECTED");
        private static readonly Func<object, EventArgs, Task> WeatherDisconnectedHandler = async (_, _) => await WebSocketV2.SendAndAddEvent("WEATHER-DISCONNECTED");

        public static void StartWeatherWatchers()
        {
            AdvancedAPI.Controls.Weather.Connected += WeatherConnectedHandler;
            AdvancedAPI.Controls.Weather.Disconnected += WeatherDisconnectedHandler;
        }

        public static void StopWeatherWatchers()
        {
            AdvancedAPI.Controls.Weather.Connected -= WeatherConnectedHandler;
            AdvancedAPI.Controls.Weather.Disconnected -= WeatherDisconnectedHandler;
        }


        [Route(HttpVerbs.Get, "/equipment/weather/info")]
        public void WeatherInfo()
        {
            Logger.Debug($"API call: {HttpContext.Request.Url.AbsoluteUri}");
            HttpResponse response = new HttpResponse();

            try
            {
                IWeatherDataMediator Weather = AdvancedAPI.Controls.Weather;

                WeatherDataInfo info = Weather.GetInfo();
                response.Response = info;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                response = CoreUtility.CreateErrorTable(CommonErrors.UNKNOWN_ERROR);
            }

            HttpContext.WriteToResponse(response);
        }

        [Route(HttpVerbs.Get, "/equipment/weather/connect")]
        public async Task WeatherConnect()
        {
            Logger.Debug($"API call: {HttpContext.Request.Url.AbsoluteUri}");
            HttpResponse response = new HttpResponse();

            try
            {
                IWeatherDataMediator weather = AdvancedAPI.Controls.Weather;

                if (!weather.GetInfo().Connected)
                {
                    await weather.Rescan();
                    await weather.Connect();
                }
                response.Response = "Weather connected";
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                response = CoreUtility.CreateErrorTable(CommonErrors.UNKNOWN_ERROR);
            }

            HttpContext.WriteToResponse(response);
        }

        [Route(HttpVerbs.Get, "/equipment/weather/disconnect")]
        public async Task WeatherDisconnect()
        {
            Logger.Debug($"API call: {HttpContext.Request.Url.AbsoluteUri}");
            HttpResponse response = new HttpResponse();

            try
            {
                IWeatherDataMediator weather = AdvancedAPI.Controls.Weather;

                if (weather.GetInfo().Connected)
                {
                    await weather.Disconnect();
                }
                response.Response = "Weather disconnected";
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                response = CoreUtility.CreateErrorTable(CommonErrors.UNKNOWN_ERROR);
            }

            HttpContext.WriteToResponse(response);
        }
    }
}
