using System;

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
