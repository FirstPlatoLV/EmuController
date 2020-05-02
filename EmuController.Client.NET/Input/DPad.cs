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

        public byte DPad1 { get { return DPads[0]; } set { DPads[0] = value; ArrayMap.Set(0, true); } }
        public byte DPad2 { get { return DPads[1]; } set { DPads[1] = value; ArrayMap.Set(1, true); } }
        public byte DPad3 { get { return DPads[2]; } set { DPads[2] = value; ArrayMap.Set(2, true); } }
        public byte DPad4 { get { return DPads[3]; } set { DPads[3] = value; ArrayMap.Set(3, true); } }


        internal DPad()
        {
            ArrayMap = new BitArray(8);
            DPads = Enumerable.Repeat((byte)0xFF, 4).ToArray();
        }

        public void SetDPad(int index, byte value)
        {
            if (index > 3)
            {
                throw new IndexOutOfRangeException();
            }
            ArrayMap.Set(index, true);
            DPads[index] = value;
        }
    }
}
