#region "copyright"

/*
    Copyright © 2025 Christian Palm (christian@palm-family.de)
    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

#endregion "copyright"

using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;
using NINA.Core.Interfaces;
using NINA.Core.Utility;
using NINA.Equipment.Equipment.MyRotator;
using NINA.Equipment.Interfaces.Mediator;
using ninaAPI.Utility;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ninaAPI.WebService.V2
{
    public partial class ControllerV2
    {
        private static CancellationTokenSource RotatorToken;

        private static readonly Func<object, EventArgs, Task> RotatorConnectedHandler = async (_, _) => await WebSocketV2.SendAndAddEvent("ROTATOR-CONNECTED");
        private static readonly Func<object, EventArgs, Task> RotatorDisconnectedHandler = async (_, _) => await WebSocketV2.SendAndAddEvent("ROTATOR-DISCONNECTED");

        public static void StartRotatorWatchers()
        {
            AdvancedAPI.Controls.Rotator.Connected += RotatorConnectedHandler;
            AdvancedAPI.Controls.Rotator.Disconnected += RotatorDisconnectedHandler;
        }

        public static void StopRotatorWatchers()
        {
            AdvancedAPI.Controls.Rotator.Connected -= RotatorConnectedHandler;
            AdvancedAPI.Controls.Rotator.Disconnected -= RotatorDisconnectedHandler;
        }


        [Route(HttpVerbs.Get, "/equipment/rotator/info")]
        public void RotatorInfo()
        {
            HttpResponse response = new HttpResponse();

            try
            {
                IRotatorMediator rotator = AdvancedAPI.Controls.Rotator;

                RotatorInfo info = rotator.GetInfo();
                response.Response = info;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                response = CoreUtility.CreateErrorTable(CommonErrors.UNKNOWN_ERROR);
            }

            HttpContext.WriteToResponse(response);
        }

        [Route(HttpVerbs.Get, "/equipment/rotator/connect")]
        public async Task RotatorConnect([QueryField] bool skipRescan)
        {
            HttpResponse response = new HttpResponse();

            try
            {
                IRotatorMediator rotator = AdvancedAPI.Controls.Rotator;

                if (!rotator.GetInfo().Connected)
                {
                    if (!skipRescan)
                    {
                        await rotator.Rescan();
                    }
                    await rotator.Connect();
                }
                response.Response = "Rotator connected";
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                response = CoreUtility.CreateErrorTable(CommonErrors.UNKNOWN_ERROR);
            }

            HttpContext.WriteToResponse(response);
        }

        [Route(HttpVerbs.Get, "/equipment/rotator/disconnect")]
        public async Task RotatorDisconnect()
        {
            HttpResponse response = new HttpResponse();

            try
            {
                IRotatorMediator rotator = AdvancedAPI.Controls.Rotator;

                if (rotator.GetInfo().Connected)
                {
                    await rotator.Disconnect();
                }
                response.Response = "Rotator disconnected";
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                response = CoreUtility.CreateErrorTable(CommonErrors.UNKNOWN_ERROR);
            }

            HttpContext.WriteToResponse(response);
        }

        [Route(HttpVerbs.Get, "/equipment/rotator/move")]
        public void RotatorMove([QueryField] float position)
        {
            HttpResponse response = new HttpResponse();

            try
            {
                IRotatorMediator rotator = AdvancedAPI.Controls.Rotator;

                if (!rotator.GetInfo().Connected)
                {
                    response = CoreUtility.CreateErrorTable(new Error("Rotator not connected", 409));
                }
                RotatorToken?.Cancel();
                RotatorToken = new CancellationTokenSource();
                rotator.Move(position, RotatorToken.Token);
                response.Response = "Rotator move started";
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                response = CoreUtility.CreateErrorTable(CommonErrors.UNKNOWN_ERROR);
            }

            HttpContext.WriteToResponse(response);
        }

        [Route(HttpVerbs.Get, "/equipment/rotator/move-mechanical")]
        public void RotatorMoveMechanical([QueryField] float position)
        {
            HttpResponse response = new HttpResponse();

            try
            {
                IRotatorMediator rotator = AdvancedAPI.Controls.Rotator;

                if (!rotator.GetInfo().Connected)
                {
                    response = CoreUtility.CreateErrorTable(new Error("Rotator not connected", 409));
                }
                RotatorToken?.Cancel();
                RotatorToken = new CancellationTokenSource();
                rotator.MoveMechanical(position, RotatorToken.Token);
                response.Response = "Rotator move started";
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
