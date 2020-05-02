using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmuController.Client.NET.Input
{
    public class Axis
    {
        private const ushort DefaultSymmAxisValue = 32768;
        private const ushort DefaultAsymmAxisValue = 65535;

        internal readonly UsageId Usage = UsageId.Axis;
        internal readonly BitArray ArrayMap;

        internal ushort[] Axes { get; private set; }

        internal ushort AxisX { get { return Axes[0]; } set { Axes[0] = value; ArrayMap.Set(0, true); } }
        internal ushort AxisY { get { return Axes[1]; } set { Axes[1] = value; ArrayMap.Set(1, true); } }
        internal ushort AxisZ { get { return Axes[2]; } set { Axes[2] = value; ArrayMap.Set(2, true); } }
        internal ushort AxisRx { get { return Axes[3]; } set { Axes[3] = value; ArrayMap.Set(3, true); } }
        internal ushort AxisRy { get { return Axes[4]; } set { Axes[4] = value; ArrayMap.Set(4, true); } }
        internal ushort AxisRz { get { return Axes[5]; } set { Axes[5] = value; ArrayMap.Set(5, true); } }
        internal ushort AxisSlider { get { return Axes[6]; } set { Axes[6] = value; ArrayMap.Set(6, true); } }
        internal ushort AxisDial { get { return Axes[7]; } set { Axes[7] = value; ArrayMap.Set(7, true); } }

        internal Axis()
        {
            Axes = new ushort[8] { DefaultSymmAxisValue, DefaultSymmAxisValue,
                                                   DefaultAsymmAxisValue,  DefaultAsymmAxisValue,
                                                   DefaultAsymmAxisValue,  DefaultAsymmAxisValue,
                                                   DefaultAsymmAxisValue,  DefaultAsymmAxisValue};

            ArrayMap = new BitArray(8);
        }

        public void SetAxis(int index, ushort value)
        {
            ArrayMap.Set(index, true);
            Axes[index] = value;
        }
    }
}
