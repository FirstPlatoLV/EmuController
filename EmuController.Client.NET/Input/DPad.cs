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
