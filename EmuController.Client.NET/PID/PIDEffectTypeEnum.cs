using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmuController.Client.NET.PID
{
    public enum PIDEffectTypeEnum
    {
        Constant = 1,
        Ramp = 2,
        Square = 3,
        Sine = 4,
        Triangle = 5,
        SawtoothUp = 6,
        SawtoothDown = 7,
        Spring = 8,
        Damper = 9,
        Inertia = 10,
        Friction = 11,
        CustomForce = 12
    }
}
