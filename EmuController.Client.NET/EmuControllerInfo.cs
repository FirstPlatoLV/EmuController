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
using System;
using System.IO;
using System.Management;
using System.Reflection;

namespace EmuController.Client.NET
{
    public class EmuControllerInfo
    {
        /// <summary>
        ///  HardwareId of the EmuController device  
        /// </summary>
        public string HardwareId { get; }

        /// <summary>
        ///  Name of the EmuController device.
        /// </summary>
        public string Name { get; }


        /// <summary>
        ///  Creates a new EmuControllerInfo object.
        /// </summary>
        /// <param name="queryObj">Provides ManageentObject data returned by ManagementObjectSearcher</param>
        public EmuControllerInfo(ManagementObject queryObj)
        {

            string hwid = (string)queryObj.Properties[nameof(HardwareId)].Value;

            Version driverVersion = new Version((string)queryObj.Properties["DriverVersion"].Value);
            Version clientVersion = Assembly.GetExecutingAssembly().GetName().Version;

            if (clientVersion.Major != driverVersion.Major || clientVersion.Minor != driverVersion.Minor)
            {
                throw new EmuControllerException(string.Format("Version mismatch! Driver: {0}.{1}, Client: {2}.{3}", 
                    driverVersion.Major, driverVersion.Minor, 
                    clientVersion.Major, clientVersion.Minor));
            }

            // We need only the Vīd/Pid information about the device
            string prefix = "ROOT\\";

            if (hwid != null)
            {
                HardwareId = hwid.Substring(prefix.Length);
            }
            else
            {
                // In case user installed the device manually through device manager (which should not be done), the hardwareId will be incorrect,
                // So we replace it with a predefined one. 
                HardwareId = "VID_DEED&PID_FE00";
            }

            Name = GetDeviceFriendlyName(HardwareId);
        }

        private string GetDeviceFriendlyName(string hwId)
        {
            if (string.IsNullOrEmpty(hwId))
            {
                return string.Empty;
            }

            const string regJoystickPath = @"System\CurrentControlSet\Control\MediaProperties\PrivateProperties\Joystick\OEM";
            const string regKeyName = "OEMName";
            string[] strArr = hwId.Split('&');

            string path = Path.Combine(regJoystickPath, strArr[0] + "&" + strArr[1]);
            return Utils.GetStringFromRegistry(path, regKeyName);
        }
    }
}
