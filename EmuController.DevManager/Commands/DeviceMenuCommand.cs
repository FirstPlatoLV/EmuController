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
