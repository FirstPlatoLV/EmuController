// Copyright 2020 Maris Melbardis
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
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
        public byte EffectBlockIndex { get; private set; }
        public ConditionParamBlockEnum ParameterBlockOffset { get; private set; }
        public short CenterPointOffset { get; private set; }
        public short NegativeCoefficient { get; private set; }
        public short PositiveCoefficient { get; private set; }
        public ushort NegativeSaturation { get; private set; }
        public ushort PositiveSaturation { get; private set; }
        public ushort DeadBand { get; private set; }

        public PIDSetConditionReport(byte[] packet): base(packet)
        {

        }

        protected override void Deserialize()
        {
            EffectBlockIndex = DataPacket[1];
            ParameterBlockOffset = (ConditionParamBlockEnum)(DataPacket[2] & 0x0F);

            CenterPointOffset = BitConverter.ToInt16(DataPacket, 3);
            NegativeCoefficient = BitConverter.ToInt16(DataPacket, 5);
            PositiveCoefficient = BitConverter.ToInt16(DataPacket, 7);
            NegativeSaturation = BitConverter.ToUInt16(DataPacket, 9);
            PositiveSaturation = BitConverter.ToUInt16(DataPacket, 11);
            DeadBand = BitConverter.ToUInt16(DataPacket, 13);
        }
    }
    public enum ConditionParamBlockEnum
    {
        AxisX,
        AxisY
    }
}
