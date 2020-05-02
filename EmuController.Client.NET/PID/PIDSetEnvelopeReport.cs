using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmuController.Client.NET.PID
{
    public class PIDSetEnvelopeReport : FFBPacket
    {
        public byte EffectBlockIndex { get; }
        public ushort AttackLevel { get; }
        public ushort FadeLevel { get; }
        public ushort AttackTime { get; }
        public ushort FadeTime { get; }
        public PIDSetEnvelopeReport(byte[] packet)
        {
            if (packet == null)
            {
                throw new ArgumentNullException(nameof(packet));
            }

            EffectBlockIndex = packet[1];
            AttackLevel = BitConverter.ToUInt16(packet, 2);
            FadeLevel = BitConverter.ToUInt16(packet, 4);
            AttackTime = BitConverter.ToUInt16(packet, 6);
            FadeTime = BitConverter.ToUInt16(packet, 8);
        }
    }
}
