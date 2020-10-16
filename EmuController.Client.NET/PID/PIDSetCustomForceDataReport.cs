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
    public class PIDSetCustomForceDataReport : FFBPacket
    {
        private const int DataLength = 12;
        public byte EffectBlockIndex { get; private set; }
        public ushort DataOffset { get; private set; }

        public ReadOnlyCollection<byte> Data => new ReadOnlyCollection<byte>(data);
        
        private byte[] data;
        public PIDSetCustomForceDataReport(byte[] packet): base(packet)
        {

        }

        protected override void Deserialize()
        {
            EffectBlockIndex = DataPacket[1];
            DataOffset = BitConverter.ToUInt16(DataPacket, 2);

            ReadOnlySpan<byte> packetSpan = DataPacket;
            data = packetSpan.Slice(4).ToArray();
        }
    }
}
