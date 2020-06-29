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
    public class PIDSetEnvelopeReport : FFBPacket
    {
        public byte EffectBlockIndex { get; }
        public ushort AttackLevel { get; }
        public ushort FadeLevel { get; }
        public ushort AttackTime { get; }
        public ushort FadeTime { get; }
        public PIDSetEnvelopeReport(byte[] packet)
        {
            if (packet == null)
            {
                throw new ArgumentNullException(nameof(packet));
            }

            EffectBlockIndex = packet[1];
            AttackLevel = BitConverter.ToUInt16(packet, 2);
            FadeLevel = BitConverter.ToUInt16(packet, 4);
            AttackTime = BitConverter.ToUInt16(packet, 6);
            FadeTime = BitConverter.ToUInt16(packet, 8);
        }
    }
}
