using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmuController.Client.NET.Input
{
    public class Axes
    {
        private const ushort DefaultSymmAxisValue = 32768;
        private const ushort DefaultAsymmAxisValue = 65535;

        internal readonly UsageId Usage = UsageId.Axis;
        internal readonly BitArray ArrayMap;

        internal ushort[] AxisValues { get; private set; }

        internal Axes()
        {
            AxisValues = new ushort[8] { DefaultSymmAxisValue, DefaultSymmAxisValue,
                                                   DefaultAsymmAxisValue,  DefaultAsymmAxisValue,
                                                   DefaultAsymmAxisValue,  DefaultAsymmAxisValue,
                                                   DefaultAsymmAxisValue,  DefaultAsymmAxisValue};

            ArrayMap = new BitArray(8);
        }

        /// <summary>
        /// Set value for Axis with index specified.
        /// </summary>
        /// <param name="axisType">Axis type</param>
        /// <param name="value">Axis value</param>
        public void SetAxis(AxisEnum axisType, ushort value)
        {
            ArrayMap.Set((int)axisType, true);
            AxisValues[(int)axisType] = value;
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
