using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmuController.Client.NET.PID
{
    public class PIDEffectOperationReport : FFBPacket
    {
        public byte EffectBlockIndex { get; }
        public EffectOpEnum EffectOperation { get; }
        public byte LoopCount { get; }

     
        public PIDEffectOperationReport(byte[] packet)
        {

            if (packet == null)
            {
                throw new ArgumentNullException(nameof(packet));
            }

            EffectBlockIndex = packet[1];
            EffectOperation = (EffectOpEnum)packet[2];
            LoopCount = packet[3];
        }    
    }
    public enum EffectOpEnum
    {
        Start = 1,
        StartSolo = 2,
        Stop = 3
    }
}
