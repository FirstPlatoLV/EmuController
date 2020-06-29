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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmuController.Client.NET.Input
{
    public class DPad
    {
        internal readonly UsageId Usage = UsageId.DPad;

        internal readonly BitArray ArrayMap;

        internal byte[] DPads { get; private set; }

        internal DPad()
        {
            ArrayMap = new BitArray(8);
            DPads = Enumerable.Repeat((byte)0xFF, 4).ToArray();
        }

        public void SetDPad(int index, DPadDirectionEnum value)
        {
            if (index > 3)
            {
                throw new IndexOutOfRangeException();
            }
            ArrayMap.Set(index, true);
            DPads[index] = (byte)value;
        }
    }

    /// <summary>
    /// DPad directions expressed as Cardinal directions 
    /// </summary>
    public enum DPadDirectionEnum
    {
        None = -1,
        North = 0,
        NorthEast = 1,
        East = 2,
        SouthEast = 3,
        South = 4,
        SouthWest = 5,
        West = 6,
        NorthWest = 7
    }
}
