using EmuController.Client.NET;
using EmuUdpServer;
using System;

namespace EmuUdpServerDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            Server server = new Server(10000);
            server.Listen();


            Console.ReadLine();
        }
    }
}
