using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmuController.Client.NET.PID
{
    public class PIDSetEffectReport : FFBPacket
    {
        public byte EffectBlockIndex { get; }

        public PIDEffectTypeEnum EffectType { get; }
        public ushort Duration { get;  }
        public ushort TriggerRepeatInterval { get; }
        public ushort SamplePeriod { get;  }
        public ushort Gain { get; }
        public byte TriggerButton { get; }

        public AxesDirFlags EnableAxesDirFlags { get; }
        public ushort DirectionX { get; }
        public ushort DirectionY { get; }
        public ushort StartDelay { get; }


        public PIDSetEffectReport(byte[] packet)
        {
            if (packet == null)
            {
                throw new ArgumentNullException(nameof(packet));
            }

            EffectBlockIndex = packet[1];
            EffectType = (PIDEffectTypeEnum)packet[2];
            Duration = BitConverter.ToUInt16(packet, 3);
            TriggerRepeatInterval = BitConverter.ToUInt16(packet, 5);
            SamplePeriod = BitConverter.ToUInt16(packet, 7);
            Gain = BitConverter.ToUInt16(packet, 9);
            TriggerButton = packet[11];
            EnableAxesDirFlags = (AxesDirFlags)packet[12];
            DirectionX = BitConverter.ToUInt16(packet, 13);
            DirectionY = BitConverter.ToUInt16(packet, 15);
            StartDelay = BitConverter.ToUInt16(packet, 17);
        }
    }

    [Flags]
    public enum AxesDirFlags
    {
        EnableAxisX = 0x01,
        EnableAxisY = 0x02,
        EnableDirection = 0x04
    }
    
}
