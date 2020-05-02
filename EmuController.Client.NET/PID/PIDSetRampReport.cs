using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmuController.Client.NET.PID
{ 
    public class PIDSetRampReport : FFBPacket
    {
        public byte EffectBlockIndex { get; }
        public short RampStart { get; }
        public short RampEnd { get; }

        public PIDSetRampReport(byte[] packet)
        {
            if (packet == null)
            {
                throw new ArgumentNullException(nameof(packet));
            }

            EffectBlockIndex = packet[1];
            RampStart = BitConverter.ToInt16(packet, 2);
            RampEnd = BitConverter.ToInt16(packet, 4);
        }
    }
}
