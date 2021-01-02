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
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Threading.Tasks;
using SetupAPI.NET;

namespace EmuController.DevManager.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindowLoaded;
        }

        private async void MainWindowLoaded(object sender, RoutedEventArgs e)
        {
            if (!IsOSSupported())
            {
                await ShowUnsupportedOsMessage();
                Application.Current.Shutdown();
            }

            if (!AppSettings.Instance.IsValid())
            {
                await ShowDriverFilesNotFound();
                Application.Current.Shutdown();
            }
            DataContext = this;
        }

        private static bool IsOSSupported()
        {
            bool result = true;
            OperatingSystem env = Environment.OSVersion;
            if (env.Version.Major != 10 && env.Version.Build < 16299)
            {
                result = false;
            }

            return result;
        }

        private async Task ShowUnsupportedOsMessage()
        {
            await this.ShowMessageAsync("Unsupported OS detected",
            "This application is designed to run on Microsoft Windows 10. ", MessageDialogStyle.Affirmative);
        }

        private async Task ShowDriverFilesNotFound()
        {
            await this.ShowMessageAsync("Driver files not found",
            "Could not find driver files in the driver installation directory.",
            MessageDialogStyle.Affirmative);
        }
    }
}