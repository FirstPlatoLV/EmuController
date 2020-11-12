using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace EmuController.DevManager
{
    public class EmuControllerDevice : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string caller = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(caller));
        }

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
            get => isInstalled; set { isInstalled = value; OnPropertyChanged(); }
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
    }

    public enum DeviceState : int
    {
        OK = 0,
        DeviceDataError = 10,
        DeviceRestartNeeded = 14,
        DeviceDisabled = 22,
    }
}
