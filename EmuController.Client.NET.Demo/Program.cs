using EmuController.Client.NET.PID;
using System;
using System.Threading;

namespace EmuController.Client.NET.Demo
{
    class Program
    {
        static void Main()
        {
            FeederReceptor feedReceive = new FeederReceptor();
            feedReceive.Connect();
            feedReceive.Feed();
        }
    }
}
