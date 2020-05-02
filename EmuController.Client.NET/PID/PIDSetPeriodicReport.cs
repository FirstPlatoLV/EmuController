using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmuController.Client.NET.PID
{
    public class PIDSetPeriodicReport : FFBPacket
    {
        public byte EffectBlockIndex { get; }
        public ushort Magnitude { get; }
        public short Offset { get; }
        public ushort Phase { get; }
        public ushort Period { get; }
        public PIDSetPeriodicReport(byte[] packet)
        {
            if (packet == null)
            {
                throw new ArgumentNullException(nameof(packet));
            }

            EffectBlockIndex = packet[1];
            Magnitude = BitConverter.ToUInt16(packet, 2);
            Offset = BitConverter.ToInt16(packet, 4);
            Phase = BitConverter.ToUInt16(packet, 6);
            Period = BitConverter.ToUInt16(packet, 8);
        }
    }
}
