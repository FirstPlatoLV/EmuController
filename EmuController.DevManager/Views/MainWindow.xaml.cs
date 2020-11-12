using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Threading.Tasks;
using SetupAPI.NET;

namespace EmuController.DevManager.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private ObservableCollection<EmuControllerDevice> Devices;
        private const int DeviceCount = 8;
        private bool DeviceInitComplete = false;
        private readonly MetroDialogSettings dialogSettings = new MetroDialogSettings()
        {
            AnimateShow = false,
            AnimateHide = false
        };


        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindowLoaded;
        }

        private async void MainWindowLoaded(object sender, RoutedEventArgs e)
        {
            OperatingSystem env = Environment.OSVersion;
            if (env.Version.Major != 10 && env.Version.Build < 16299)
            {
                await ShowUnsupportedOsMessage().ConfigureAwait(true);
                Application.Current.Shutdown();
            }

            Devices = new ObservableCollection<EmuControllerDevice>();
            lvDeviceList.DataContext = Devices;

            SetDefaultData();
            await GetStatusForDevices();
        }

        private void SetDefaultData()
        {
            Devices.Clear();
            for (int i = 0; i < DeviceCount; i++)
            {
                EmuControllerDevice dev = new EmuControllerDevice
                {
                    DeviceId = @"VID_DEED&PID_FE0" + i.ToString(),
                    DeviceName = "EmuController Device #" + (i + 1).ToString(),
                    DeviceIndex = i,
                    IdPrefix = "Root\\"
                };
                Devices.Add(dev);
            }
        }

        private async Task GetStatusForDevices()
        {
            ProgressDialogController progressController = await this.ShowProgressAsync("Enumerating devices", "Please wait...", false, dialogSettings);

            foreach (EmuControllerDevice dev in Devices)
            {
                WMIPnpEntity.SetDeviceStatus(dev);
                if (dev.ErrorCode != (uint)DeviceState.OK && dev.ErrorCode != (uint)DeviceState.DeviceDisabled)
                {
                    await this.ShowMessageAsync("A problem with a device",
                        string.Format("Device '{0}' is not working correctly. If restarting the system does not restore the functionality of the device,"
                        + "uninstall and reinstall device again.", dev.DeviceName), 
                        MessageDialogStyle.Affirmative);
                }
            }

            DeviceInitComplete = true;
            await progressController.CloseAsync();
        }

        private async void InstallClick(object sender, RoutedEventArgs e)
        {
            DeviceInitComplete = false;
            MenuItem deviceClicked = sender as MenuItem;
            EmuControllerDevice device = (EmuControllerDevice)deviceClicked.DataContext;
            ProgressDialogController progressController = await this.ShowProgressAsync("Installing device", "Please wait...", false, dialogSettings);

            
            DeviceConfig.InstallDeviceFromInf(Path.Combine(Directory.GetCurrentDirectory() , "Emucontroller.inf"), 
                Path.Combine(device.IdPrefix, device.DeviceId), 
                out bool restartNeeded);

            device.SetDeviceFriendlyName("EmuController Device #" + (device.DeviceIndex + 1).ToString());

            WMIPnpEntity.SetDeviceStatus(device);
            DeviceInitComplete = true;
            await progressController.CloseAsync();

            if (restartNeeded)
            {
                ShowRestartRequiredMessage();
            }

        }

        private async void UninstallClick(object sender, RoutedEventArgs e)
        {
            DeviceInitComplete = false;
            MenuItem deviceClicked = sender as MenuItem;
            EmuControllerDevice device = (EmuControllerDevice)deviceClicked.DataContext;
            ProgressDialogController progressController = await this.ShowProgressAsync("Uninstalling device", "Please wait...", false, dialogSettings);

            DeviceConfig.UninstallDevice(Path.Combine(device.IdPrefix, device.DeviceId));
            device.SetDeviceFriendlyName("EmuController Device #" + (device.DeviceIndex + 1).ToString());
            WMIPnpEntity.SetDeviceStatus(device);
            DeviceInitComplete = true;
            await progressController.CloseAsync();

        }

        private void ToggleSwitchToggled(object sender, RoutedEventArgs e)
        {
            if (!DeviceInitComplete)
            {
                return;
            }

            ToggleSwitch toggleSwitch = sender as ToggleSwitch;
            EmuControllerDevice device = (EmuControllerDevice)toggleSwitch.DataContext;


            if (toggleSwitch == null)
            {
                return;
            }

            DeviceInitComplete = false;

            if (toggleSwitch.IsOn)
            {
                DeviceConfig.EnableDevice(Path.Combine(device.IdPrefix, device.DeviceId));
                WMIPnpEntity.SetDeviceStatus(device);
                if (!device.IsEnabled)
                {
                    ShowRestartRequiredMessage();
                }
            }
            else
            {
                DeviceConfig.DisableDevice(Path.Combine(device.IdPrefix, device.DeviceId));
                WMIPnpEntity.SetDeviceStatus(device);
                if (device.IsEnabled)
                {
                    ShowRestartRequiredMessage();
                }
            }
            DeviceInitComplete = true;
        }

        private async void RenameClick(object sender, RoutedEventArgs e)
        {
            MenuItem deviceClicked = sender as MenuItem;
           
            EmuControllerDevice device = (EmuControllerDevice)deviceClicked.DataContext;

            SingleLineInputDialog nameChangeDialog = new SingleLineInputDialog
            {
                Title = "Device rename",
                Message = "Set the device name:",
            };

            await this.ShowMetroDialogAsync(nameChangeDialog);
            string newName = await nameChangeDialog.WaitForButtonPressAsync();
            await this.HideMetroDialogAsync(nameChangeDialog);
        
            if (string.IsNullOrEmpty(newName))
            {
                return;
            }

            device.SetDeviceFriendlyName(newName);
        }

        private async void ShowRestartRequiredMessage()
        {
            await this.ShowMessageAsync("A system restart is required",
            "To complete operation on the device, please restart your system. ", MessageDialogStyle.Affirmative);
        }

        private async Task ShowUnsupportedOsMessage()
        {
            await this.ShowMessageAsync("Unsupported OS detected",
            "This application is designed to run on Microsoft Windows 10. ", MessageDialogStyle.Affirmative);
        }

        private void ImageMouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is Image image)
            {
                image.ContextMenu.DataContext = image.DataContext;
                image.ContextMenu.IsOpen = true;
            }

        }
    }
}