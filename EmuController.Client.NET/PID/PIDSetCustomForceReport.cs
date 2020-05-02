using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmuController.Client.NET.PID
{
    public class PIDSetCustomForceReport : FFBPacket
    {
        public byte EffectBlockIndex { get; }
        public byte SampleCount { get; }
        public ushort SamplePeriod { get; }
        public PIDSetCustomForceReport(byte[] packet)
        {
            if (packet == null)
            {
                throw new ArgumentNullException(nameof(packet));
            }

            EffectBlockIndex = packet[1];
            SampleCount = packet[2];
            SamplePeriod = BitConverter.ToUInt16(packet, 3);
        }
    }
}
