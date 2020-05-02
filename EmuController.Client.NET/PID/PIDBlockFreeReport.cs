using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmuController.Client.NET.PID
{
    public class PIDBlockFreeReport : FFBPacket
    {
        public byte FreeReportEffectBlockIndex { get; }
        public PIDBlockFreeReport(byte[] packet)
        {
            if (packet == null)
            {
                throw new ArgumentNullException(nameof(packet));
            }

            FreeReportEffectBlockIndex = packet[1];
        }
    }
}
