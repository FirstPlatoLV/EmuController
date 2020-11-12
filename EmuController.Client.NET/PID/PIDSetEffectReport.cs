﻿// Copyright 2020 Maris Melbardis
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
using System.Runtime.InteropServices;

namespace EmuController.Client.NET.PID
{
    public class PIDSetEffectReport : FFBPacket
    {
        public byte EffectBlockIndex { get; private set; }

        public PIDEffectTypeEnum EffectType { get; private set; }
        public ushort Duration { get; private set;  }
        public ushort TriggerRepeatInterval { get; private set; }
        public ushort SamplePeriod { get; private set;  }
        public ushort Gain { get; private set; }
        public byte TriggerButton { get; private set; }

        public AxesDirFlags EnableAxesDirFlags { get; private set; }
        public ushort DirectionX { get; private set; }
        public ushort DirectionY { get; private set; }
        public ushort StartDelay { get; private set; }


        public PIDSetEffectReport(byte[] packet): base(packet)
        {

        }

        protected override void Deserialize()
        {
            EffectBlockIndex = DataPacket[1];
            EffectType = (PIDEffectTypeEnum)DataPacket[2];

            ReadOnlySpan<byte> packetSpan = DataPacket;
            ReadOnlySpan<ushort> shortSpan = MemoryMarshal.Cast<byte, ushort>(packetSpan[3..]);


            Duration = shortSpan[0];
            TriggerRepeatInterval = shortSpan[1];
            SamplePeriod = shortSpan[2];
            Gain = shortSpan[3];
            TriggerButton = DataPacket[11];
            EnableAxesDirFlags = (AxesDirFlags)DataPacket[12];
            DirectionX = shortSpan[5];
            DirectionY = shortSpan[6];
            StartDelay = shortSpan[7];
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
