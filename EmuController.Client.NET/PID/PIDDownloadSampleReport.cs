using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmuController.Client.NET.PID
{
    public class PIDDownloadSampleReport : FFBPacket
    {
        public sbyte DownloadForceSampleX { get; }
        public sbyte DownloadForceSampleY { get; }
        public PIDDownloadSampleReport(byte[] packet)
        {
            if (packet == null)
            {
                throw new ArgumentNullException(nameof(packet));
            }

            DownloadForceSampleX = (sbyte)packet[1];
            DownloadForceSampleY = (sbyte)packet[2];
        }
    }
}
