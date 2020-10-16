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

namespace EmuController.Client.NET.Input
{
    public class Axes : ControlCollection<ushort>
    {
        private const ushort DefaultSymmAxisValue = 32768;
        private const ushort DefaultAsymmAxisValue = 65535;

        internal Axes()
        {
            values = new ushort[8] { DefaultSymmAxisValue, DefaultSymmAxisValue,
                                                   DefaultAsymmAxisValue,  DefaultAsymmAxisValue,
                                                   DefaultAsymmAxisValue,  DefaultAsymmAxisValue,
                                                   DefaultAsymmAxisValue,  DefaultAsymmAxisValue};

        }

        /// <summary>
        /// Set value for Axis with the specified index.
        /// </summary>
        /// <param name="axisType">Axis type</param>
        /// <param name="value">Axis value</param>
        public void SetValue(AxisEnum axisType, ushort value)
        {
            SetValue((int)axisType, value);
        }
    }

    /// <summary>
    /// Axis types for HID game controllers.
    /// Values correspond to indices of DirectInput controller
    /// </summary>
    public enum AxisEnum
    {
        AxisX,
        AxisY,
        AxisZ,
        AxisRx,
        AxisRy,
        AxisRz,
        AxisSlider,
        AxisDial
    }
}
