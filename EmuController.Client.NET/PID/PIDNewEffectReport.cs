using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmuController.Client.NET.PID
{
    public class PIDNewEffectReport : FFBPacket
    {
        public PIDEffectTypeEnum EffectType { get; }
        public byte ByteCount { get; }
        public PIDNewEffectReport(byte[] packet)
        {
            if (packet == null)
            {
                throw new ArgumentNullException(nameof(packet));
            }

            EffectType = (PIDEffectTypeEnum)packet[1];
            ByteCount = packet[2];
        }
    }
}
