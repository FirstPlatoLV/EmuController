using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmuController.Client.NET.PID
{
    public class PIDSetConditionReport : FFBPacket
    {
        public byte EffectBlockIndex { get; }
        public ConditionParamBlockEnum ParameterBlockOffset { get; }
        public short CenterPointOffset { get; }
        public short NegativeCoefficient { get; }
        public short PositiveCoefficient { get; }
        public ushort NegativeSaturation { get; }
        public ushort PositiveSaturation { get; }
        public ushort DeadBand { get; }
        public PIDSetConditionReport(byte[] packet)
        {

            if (packet == null)
            {
                throw new ArgumentNullException(nameof(packet));
            }

            EffectBlockIndex = packet[1];
            ParameterBlockOffset = (ConditionParamBlockEnum)(packet[2] & 0x0F);

            CenterPointOffset = BitConverter.ToInt16(packet, 3);
            NegativeCoefficient = BitConverter.ToInt16(packet, 5);
            PositiveCoefficient = BitConverter.ToInt16(packet, 7);
            NegativeSaturation = BitConverter.ToUInt16(packet, 9);
            PositiveSaturation = BitConverter.ToUInt16(packet, 11);
            DeadBand = BitConverter.ToUInt16(packet, 13);
        }
    }
    public enum ConditionParamBlockEnum
    {
        AxisX,
        AxisY
    }
}
