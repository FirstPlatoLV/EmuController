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
    public enum PIDReportIdEnum
    {
        // Output report Ids.
        SetEffect = 0x10,
        SetEnvelope = 0x11,
        SetCondition = 0x12,
        SetPeriodic = 0x13,
        SetConstant = 0x14,
        SetRamp = 0x15,
        SetCustomForceData = 0x16,
        DownloadSample = 0x17,
        EffectOperation = 0x18,
        DeviceControl = 0x19,
        BlockFree = 0x1A,
        SetGain = 0x1B,
        SetCustomForce = 0x1C,

        // Feature report Ids.
        NewEffect = 0x20,
        BlockLoad = 0x21,
    }
}
