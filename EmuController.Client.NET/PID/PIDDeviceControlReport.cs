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
    public class PIDDeviceControlReport : FFBPacket
    {
        public DeviceControlEnum ControlCommand { get; private set; }

        public PIDDeviceControlReport(byte[] packet): base(packet)
        {

        }
        public enum DeviceControlEnum
        {
            EnableActuactors = 1,
            DisableActuactors = 2,
            StopAllEffects = 3,
            Reset = 4,
            Pause = 5,
            Continue = 6
        }

        protected override void Deserialize()
        {
            ControlCommand = (DeviceControlEnum)DataPacket[1];
        }
    }
}
