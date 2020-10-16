using EmuController.Client.NET;
using EmuController.Client.NET.Input;
using EmuController.Client.NET.PID;
using EmuController.Udp.Packet;
using EmuUdpServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace EmuUdpServerDemo
{

    class FeederReceptor
    {
        private EmuControllerClient EmuClient;
        public FeederReceptor()
        {
            // We initialize the EmuControllerupdater;
            // The list EmuClient.ConnectedEmuControllerDevices will be populated
            // by available EmuController devices upon intitializing EmuControllerClient.
            EmuClient = new EmuControllerClient();
            Connect();

        }

        private void Server_InputReportReceived(object sender, InputReportEventArgs e)
        {
            Feed(e.InputReport);
        }

        public void Connect()
        {
            if (EmuClient.EmuControllerDevices.Count == 0)
            {
                Console.WriteLine("No EmuController devices detected");
                return;
            }
            // Connect to EmuController device to send input reports.
            EmuClient.ConnectInputClient(EmuClient.EmuControllerDevices[0]);

            // Optionally connect to EmuController device to receive FFB data.
            //EmuClient.ConnectFFBClient(EmuClient.EmuControllerDevices[0]);


            // When we want to receive FFB data packets, we should subscribe to the FFBDataReceived event.
            // It's up to user to decide what to do with the packets.
            //EmuClient.FFBDataReceived += FFBDataReceived;
        }


        public void Disconnect()
        {
            EmuClient.CloseInputClient();
            EmuClient.CloseFFBClient();
        }

        public void Feed(InputReportPacket iRep)
        {
            int[] axes = iRep.Axes.ToArray();
            for (int i = 0; i < axes.Length; i++)
            {
                EmuClient.InputState.Axes.SetAxis((AxisEnum)i, (ushort)axes[i]);
            }
            bool[] buttons = iRep.Buttons.ToArray();
            for (int i = 0; i < 32; i++)
            {
                EmuClient.InputState.Buttons.SetButton(i, buttons[i]);
            }

            var dpads = iRep.Dpads.ToArray(); 
            for (int i = 0; i < dpads.Length; i++)
            {
                EmuClient.InputState.DPads.SetDPad(i, (DPadDirectionEnum)((int)dpads[i] - 1));
            }

            EmuClient.SendInputReport();
        }

        private void FFBDataReceived(object sender, FFBDataReceivedEventArgs e)
        {
            // Outputs packet type that EmuController receive
            Console.WriteLine(e.ReportId.ToString());
        }
    }
}

