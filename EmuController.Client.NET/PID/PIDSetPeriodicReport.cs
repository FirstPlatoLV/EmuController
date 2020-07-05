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
    public class PIDSetPeriodicReport : FFBPacket
    {
        public byte EffectBlockIndex { get; private set; }
        public ushort Magnitude { get; private set; }
        public short Offset { get; private set; }
        public ushort Phase { get; private set; }
        public ushort Period { get; private set; }
        public PIDSetPeriodicReport(byte[] packet): base(packet)
        {

        }

        protected override void Deserialize()
        {
            EffectBlockIndex = DataPacket[1];
            Magnitude = BitConverter.ToUInt16(DataPacket, 2);
            Offset = BitConverter.ToInt16(DataPacket, 4);
            Phase = BitConverter.ToUInt16(DataPacket, 6);
            Period = BitConverter.ToUInt16(DataPacket, 8);
        }
    }
}
