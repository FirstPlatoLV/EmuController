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
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace EmuController.DevManager.Commands
{
    public class DeviceMenuCommand
    {
        public DeviceCommmandEnum CommandType { get; set; }
        public string CommandName { get => CommandType.ToString(); }
        public DeviceCommand Command { get; set; }

        public DeviceMenuCommand(DeviceCommmandEnum commandType, DeviceCommand command)
        {
            CommandType = commandType;
            Command = command;
        }
    }

    public enum DeviceCommmandEnum
    {
        Install,
        Rename,
        Uninstall,
        SetActive,
        Toggle
    }
}
