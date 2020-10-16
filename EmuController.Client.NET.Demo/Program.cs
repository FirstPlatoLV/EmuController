using EmuController.Client.NET.PID;
using System;
using System.Text;
using System.Threading;
using System.Security.Cryptography;

namespace EmuController.Client.NET.Demo
{
    class Program
    {
        static void Main()
        { 
            FeederReceptor feedReceive = new FeederReceptor();
            feedReceive.Connect();
            feedReceive.Feed();
            Console.ReadLine();
        }
    }
}
