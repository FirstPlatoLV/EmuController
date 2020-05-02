using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmuController.Client.NET.PID
{
    public enum PIDReportIdEnum
    {
        // Output report Ids.
        SetEffect = 0x10,
        SetEnvelope = 0x11,
        SetCondition = 0x12,
        SetPeriodic = 0x13,
        SetConstant = 0x14,
        SetRamp = 0x15,
        SetCustomForceData = 0x16,
        DownloadSample = 0x17,
        EffectOperation = 0x18,
        DeviceControl = 0x19,
        BlockFree = 0x1A,
        SetGain = 0x1B,
        SetCustomForce = 0x1C,

        // Feature report Ids.
        NewEffect = 0x20,
        BlockLoad = 0x21,
    }
}
