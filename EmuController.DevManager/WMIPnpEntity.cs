// Copyright 2020 Maris Melbardis
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
using EmuController.DevManager.ViewModels;
using System;
using System.Collections.Generic;
using System.Management;
using System.Text;

namespace EmuController.DevManager
{
    public sealed class WMIPnpEntity
    {
        private static readonly string Scope = "root\\CIMV2";
        private const string ClassGuid = "{4DB9B76D-4379-437E-9457-4F00B3090C1F}";
        private readonly ManagementObjectSearcher Mos;
        private static readonly Lazy<WMIPnpEntity>
            lazy =
            new Lazy<WMIPnpEntity>
                (() => new WMIPnpEntity());

        public static WMIPnpEntity Instance { get { return lazy.Value; } }

        private WMIPnpEntity()
        {
            string queryString = $"SELECT * FROM Win32_PnPEntity WHERE ClassGuid = '{ClassGuid}'";
            Mos = new ManagementObjectSearcher(Scope, queryString);
        }

        public static void SetDeviceStatus(EmuControllerDevice device)
        {
            device.IsInstalled = false;
            device.IsEnabled = false;

            foreach (ManagementObject queryObj in Instance.Mos.Get())
            {
                string hardwareId = ((string[])queryObj.Properties["HardwareID"].Value)[0][device.IdPrefix.Length..];
                if (device.DeviceId.Equals(hardwareId))
                {
                    device.IsInstalled = true;
                    string devName = device.GetDeviceFriendlyNameFromRegistry();
                    if (!string.IsNullOrEmpty(devName))
                    {
                        device.DeviceName = devName;
                    }

                    if ((uint)queryObj.Properties["ConfigManagerErrorCode"].Value == (uint)DeviceState.OK)
                    {
                        device.IsEnabled = true;
                    }
                    else
                    {
                        device.ErrorCode = (uint)queryObj.Properties["ConfigManagerErrorCode"].Value;
                    }
                    break;
                }
            }
        }
    }
}