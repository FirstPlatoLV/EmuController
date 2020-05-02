using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmuController.Client.NET.PID
{ 
    public class PIDDeviceControlReport : FFBPacket
    {
        public DeviceControlEnum ControlCommand { get; }

        public PIDDeviceControlReport(byte[] packet)
        {
            if (packet == null)
            {
                throw new ArgumentNullException(nameof(packet));
            }

            ControlCommand = (DeviceControlEnum)packet[1];
        }
        public enum DeviceControlEnum
        {
            EnableActuactors = 1,
            DisableActuactors = 2,
            StopAllEffects = 3,
            Reset = 4,
            Pause = 5,
            Continue = 6
        }
    }
}
