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
using EmuController.DevManager.ViewModels;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using SetupAPI.NET;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace EmuController.DevManager.Views
{
    /// <summary>
    /// Interaction logic for DeviceList.xaml
    /// </summary>
    public partial class DeviceList : UserControl
    {
        private MetroWindow parentWindow;

        private readonly MetroDialogSettings dialogSettings = new MetroDialogSettings()
        {
            AnimateShow = false,
            AnimateHide = false
        };
        public BindingList<DeviceMenuCommand> DeviceOptionCommands { get; set; }
        public DeviceMenuCommand SetActiveOptionsCommand { get; set; }
        public DeviceMenuCommand ToggleDeviceStateCommand { get; set; }

        public ObservableCollection<EmuControllerDevice> Devices { get; set; }
        public DeviceList()
        {
            InitializeComponent();
            SetActiveOptionsCommand = new DeviceMenuCommand(DeviceCommmandEnum.SetActive, new DeviceCommand(SetActiveOptions, x => true));
            ToggleDeviceStateCommand = new DeviceMenuCommand(DeviceCommmandEnum.Toggle, new DeviceCommand(ToggleSwitchToggled, x => true));
            DeviceOptionCommands = new BindingList<DeviceMenuCommand>()
            {
                new DeviceMenuCommand(DeviceCommmandEnum.Install, new DeviceCommand(InstallDevice, x => false)),
                new DeviceMenuCommand(DeviceCommmandEnum.Rename, new DeviceCommand(RenameDevice, x => false)),
                new DeviceMenuCommand(DeviceCommmandEnum.Uninstall, new DeviceCommand(UninstallDevice, x => false)),
            };
            DeviceOptionCommands.ListChanged += DeviceOptionCommandsListChanged;
            Loaded += DeviceListLoaded;
        }

        private void DeviceOptionCommandsListChanged(object sender, ListChangedEventArgs e)
        {
            OnPropertyChanged();
        }

        private void DeviceListLoaded(object sender, RoutedEventArgs e)
        {
            parentWindow = (MetroWindow)Window.GetWindow(this);
            PopulateDeviceList();
            DataContext = this;
        }

        private void PopulateDeviceList()
        {
            const int deviceCount = 8;
            Devices = new ObservableCollection<EmuControllerDevice>();
            for (int i = 0; i < deviceCount; i++)
            {
                EmuControllerDevice dev = new EmuControllerDevice
                {
                    DeviceId = @"VID_DEED&PID_FE0" + i.ToString(),
                    DeviceName = "EmuController Device #" + (i + 1).ToString(),
                    DeviceIndex = i,
                    DeviceStatus = "Not Installed",
                    IdPrefix = "Root\\"
                };
                Devices.Add(dev);
            }
            _ = UpdateDeviceStates();
        }

        private async void InstallDevice(object device)
        {
            if (device is EmuControllerDevice dev)
            {
                ProgressDialogController progressController = await parentWindow.ShowProgressAsync("Installing device", "Please wait...", false, dialogSettings);
                bool restartNeeded = dev.InstallDevice();
                await progressController.CloseAsync();

                if (restartNeeded)
                {
                    ShowRestartRequiredMessage();
                }
            }
        }

        private async void UninstallDevice(object device)
        {
            if (device is EmuControllerDevice dev)
            {
                ProgressDialogController progressController = await parentWindow.ShowProgressAsync("Uninstalling device", "Please wait...", false, dialogSettings);
                dev.UninstallDevice();
                await progressController.CloseAsync();
            }

        }
        private async void RenameDevice(object device)
        {
            if (device is EmuControllerDevice dev)
            {
                SingleLineInputDialog nameChangeDialog = new SingleLineInputDialog
                {
                    Title = "Device rename",
                    Message = "Set the device name:",
                };

                await parentWindow.ShowMetroDialogAsync(nameChangeDialog);
                string newName = await nameChangeDialog.WaitForButtonPressAsync();
                await parentWindow.HideMetroDialogAsync(nameChangeDialog);

                if (!string.IsNullOrEmpty(newName))
                {
                    dev.SetDeviceFriendlyName(newName);
                }
            }
        }

        private void ToggleSwitchToggled(object device)
        {

            if (device is EmuControllerDevice dev)
            {
                if (dev.IsEnabled)
                {
                    DeviceConfig.DisableDevice(Path.Combine(dev.IdPrefix, dev.DeviceId));
                    WMIPnpEntity.SetDeviceStatus(dev);
                    if (dev.IsEnabled)
                    {
                        ShowRestartRequiredMessage();
                    }
                }
                else
                {
                    DeviceConfig.EnableDevice(Path.Combine(dev.IdPrefix, dev.DeviceId));
                    WMIPnpEntity.SetDeviceStatus(dev);
                    if (!dev.IsEnabled)
                    {
                        ShowRestartRequiredMessage();
                    }
                }
            }
        }

        private async Task UpdateDeviceStates()
        {
            ProgressDialogController progressController = await parentWindow.ShowProgressAsync("Enumerating devices", "Please wait...", false, dialogSettings);

            foreach (EmuControllerDevice dev in Devices)
            {
                WMIPnpEntity.SetDeviceStatus(dev);
                if (dev.ErrorCode != (uint)DeviceState.OK && dev.ErrorCode != (uint)DeviceState.DeviceDisabled)
                {
                    await parentWindow.ShowMessageAsync("A problem with a device",
                        string.Format("Device '{0}' is not working correctly. If restarting the system does not restore the functionality of the device,"
                        + "uninstall and reinstall device again.", dev.DeviceName),
                        MessageDialogStyle.Affirmative);
                }
            }
            await progressController.CloseAsync();
        }

        private void SetActiveOptions(object device)
        {
            if (device is EmuControllerDevice dev)
            {
                for (int i = 0; i < DeviceOptionCommands.Count; i++)
                {
                    bool enable = dev.IsInstalled;
                    if (DeviceOptionCommands[i].CommandType == DeviceCommmandEnum.Install)
                    {
                        enable = !enable;
                    }
                    DeviceOptionCommands[i].Command.SetExecuteState(enable);
                }
            }
        }

        private async void ShowRestartRequiredMessage()
        {
            await parentWindow.ShowMessageAsync("A system restart is required",
            "To complete operation on the device, please restart your system. ", MessageDialogStyle.Affirmative);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string caller = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(caller));
        }
    }
}
