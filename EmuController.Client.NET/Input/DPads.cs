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
using System.Linq;

namespace EmuController.Client.NET.Input
{
    public class DPads : ControlCollection<byte>
    {    
        internal DPads()
        {
            values = Enumerable.Repeat((byte)0xFF, 4).ToArray();
        }

        /// <summary>
        /// Set value for DPad with the specified index.
        /// </summary>
        /// <param name="index">Index of the DPad</param>
        /// <param name="direction">Cardinal Direction</param>
        public void SetValue(int index, DPadDirectionEnum direction)
        {
            SetValue(index, (byte)direction);
        }   
    }

    /// <summary>
    /// DPad values expressed as Cardinal directions 
    /// </summary>
    public enum DPadDirectionEnum
    {
        Null = -1,
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
