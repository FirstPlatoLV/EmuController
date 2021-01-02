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
using EmuController.DevManager.Commands;
using SetupAPI.NET;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;

namespace EmuController.DevManager.ViewModels
{
    public class EmuControllerDevice : INotifyPropertyChanged
    {   
        private string deviceName;
        public string DeviceName
        {
            get => deviceName; set { deviceName = value; OnPropertyChanged(); }
        }

        private string deviceId;
        public string DeviceId
        {
            get => deviceId; set { deviceId = value; OnPropertyChanged(); }
        }

        private bool isEnabled;
        public bool IsEnabled
        {
            get => isEnabled; set { isEnabled = value; OnPropertyChanged(); }
        }

        private bool isInstalled;
        public bool IsInstalled
        {
            get => isInstalled; 
            set 
            { 
                isInstalled = value;                       

                if (IsInstalled)
                {
                    DeviceStatus = "Disabled";
                }
                else
                {
                    DeviceStatus = "Not Installed";
                }

                OnPropertyChanged();
            }
        }

        private string deviceStatus;
        public string DeviceStatus
        {
            get => deviceStatus; set { deviceStatus = value; OnPropertyChanged(); }
        }

        public int DeviceIndex { get; set; }

        public string IdPrefix { get; set; }

        public uint ErrorCode { get; set; }

        private const string JoystickRegPath = @"System\CurrentControlSet\Control\MediaProperties\PrivateProperties\Joystick\OEM\";
        private const string RegValueName = "OEMName";

        public void SetDeviceFriendlyName(string devName)
        {
            string path = Path.Combine(JoystickRegPath, DeviceId);
            Utils.SetStringFromRegistry(path, RegValueName, devName);
            DeviceName = devName;
        }

        public string GetDeviceFriendlyNameFromRegistry()
        {
            string path = Path.Combine(JoystickRegPath, DeviceId);
            return Utils.GetStringFromRegistry(path, RegValueName);
        }
        public bool InstallDevice()
        {
            DeviceConfig.InstallDeviceFromInf(Path.Combine(Directory.GetCurrentDirectory(), AppSettings.Instance.FilePath, @"Emucontroller.inf"),
               Path.Combine(IdPrefix, DeviceId),
               out bool restartNeeded);

            SetDeviceFriendlyName("EmuController Device #" + (DeviceIndex + 1).ToString());
            WMIPnpEntity.SetDeviceStatus(this);

            return restartNeeded;
        }

        public void UninstallDevice()
        {
            DeviceConfig.UninstallDevice(Path.Combine(IdPrefix, DeviceId));
            SetDeviceFriendlyName("EmuController Device #" + (DeviceIndex + 1).ToString());
            WMIPnpEntity.SetDeviceStatus(this);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string caller = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(caller));
        }
    }

    public enum DeviceState : int
    {
        OK = 0,
        DeviceDataError = 10,
        DeviceRestartNeeded = 14,
        DeviceDisabled = 22,
    }
}
