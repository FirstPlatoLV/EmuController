using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmuController.Client.NET.PID
{
    public class PIDSetCustomForceDataReport : FFBPacket
    {
        private const int DataLength = 12;
        public byte EffectBlockIndex { get; private set; }
        public ushort DataOffset { get; private set; }
        public ReadOnlyCollection<byte> Data { get; }
        public PIDSetCustomForceDataReport(byte[] packet)
        {

            if (packet == null)
            {
                throw new ArgumentNullException(nameof(packet));
            }
            byte[] temp = new byte[DataLength];

            EffectBlockIndex = packet[1];
            DataOffset = BitConverter.ToUInt16(packet, 2);
            Array.Copy(packet, 4, temp, 0, DataLength);
            Data = new ReadOnlyCollection<byte>(temp);
        }
    }
}
