using System;
using System.Collections.Generic;
using System.Text;

namespace EmuController.Client.NET.PID
{
    [Flags]
    public enum PIDStateFlags : byte
    {
        DevicePaused = 0x01,
        ActuatorsEnabled = 0x02,
        ActuatorsPowered = 0x10,
        Paused = DevicePaused | ActuatorsEnabled | ActuatorsPowered,
        Default = ActuatorsEnabled | ActuatorsPowered,
        Disabled = 0
    }
}
