using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmuController.Client.NET.PID
{
    public class PIDSetConstantForce : FFBPacket
    {
        public byte EffectBlockIndex { get; }
        public short Magnitude { get; }

        public PIDSetConstantForce(byte[] packet)
        {
            if (packet == null)
            {
                throw new ArgumentNullException(nameof(packet));
            }

            EffectBlockIndex = packet[1];
            Magnitude = BitConverter.ToInt16(packet, 2);
        }

    }
}
