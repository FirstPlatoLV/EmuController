using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmuController.Client.NET.PID
{
    public class PIDSetGainReport : FFBPacket
    {
        public ushort Gain { get; }
        public PIDSetGainReport(byte[] packet)
        {
            if (packet == null)
            {
                throw new ArgumentNullException(nameof(packet));
            }

            Gain = BitConverter.ToUInt16(packet, 1);
        }
    }
}
