﻿using System;
using System.IO;
using System.Management;

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
            if (queryObj == null)
            {
                throw new ArgumentNullException(nameof(queryObj));
            }

            string[] hwid = (string[])queryObj.Properties[nameof(HardwareId)].Value;



            // We need only the Vīd/Pid information about the device
            string prefix = "ROOT\\";

            if (hwid != null)
            {
                HardwareId = hwid[0].Substring(prefix.Length);
            }
            else
            {
                // In case user installed the device manually through device manager (which should not be done), the hardwareId will be incorrect,
                // So we replace it with predefined one. 
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