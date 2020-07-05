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
    public class PIDNewEffectReport : FFBPacket
    {
        public PIDEffectTypeEnum EffectType { get; private set; }
        public byte ByteCount { get; private set; }
        public PIDNewEffectReport(byte[] packet): base(packet)
        {
           
        }

        protected override void Deserialize()
        {
            EffectType = (PIDEffectTypeEnum)DataPacket[1];
            ByteCount = DataPacket[2];
        }
    }
}
