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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmuController.Client.NET.PID
{
    public class PIDSetEffectReport : FFBPacket
    {
        public byte EffectBlockIndex { get; }

        public PIDEffectTypeEnum EffectType { get; }
        public ushort Duration { get;  }
        public ushort TriggerRepeatInterval { get; }
        public ushort SamplePeriod { get;  }
        public ushort Gain { get; }
        public byte TriggerButton { get; }

        public AxesDirFlags EnableAxesDirFlags { get; }
        public ushort DirectionX { get; }
        public ushort DirectionY { get; }
        public ushort StartDelay { get; }


        public PIDSetEffectReport(byte[] packet)
        {
            if (packet == null)
            {
                throw new ArgumentNullException(nameof(packet));
            }

            EffectBlockIndex = packet[1];
            EffectType = (PIDEffectTypeEnum)packet[2];
            Duration = BitConverter.ToUInt16(packet, 3);
            TriggerRepeatInterval = BitConverter.ToUInt16(packet, 5);
            SamplePeriod = BitConverter.ToUInt16(packet, 7);
            Gain = BitConverter.ToUInt16(packet, 9);
            TriggerButton = packet[11];
            EnableAxesDirFlags = (AxesDirFlags)packet[12];
            DirectionX = BitConverter.ToUInt16(packet, 13);
            DirectionY = BitConverter.ToUInt16(packet, 15);
            StartDelay = BitConverter.ToUInt16(packet, 17);
        }
    }

    [Flags]
    public enum AxesDirFlags
    {
        EnableAxisX = 0x01,
        EnableAxisY = 0x02,
        EnableDirection = 0x04
    }
    
}
