using EmuController.Client.NET.Input;
using EmuController.Client.NET.PID;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EmuController.Client.NET.Demo
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

        }

        public void Connect()
        {
            // Connect to EmuController device to send input reports.
            EmuClient.ConnectInputClient(EmuClient.EmuControllerDevices[0]);

            // Optionally connect to EmuController device to receive FFB data.
            EmuClient.ConnectFFBClient(EmuClient.EmuControllerDevices[0]);


            // When we want to receive FFB data packets, we should subscribe to the FFBDataReceived event.
            // It's up to user to decide what to do with the packets.
            EmuClient.FFBDataReceived += FFBDataReceived;
        }


        public void Disconnect()
        {
            EmuClient.CloseInputClient();
            EmuClient.CloseFFBClient();
        }

        public void Feed()
        {

            //// Just a randomizer to provide random values to simulate input;
            Random rndVal = new Random();

            while (EmuClient.InputClientConnected)
            {

                // There are two ways to set values for axes. One is explicit way refering to Axis name. Like this:
                //EmuClient.InputState.AxisZ = 5353;  // Center value is 32768 which will correspond to Raw value of 32767;


                // The other way to set axes value by providing axis index and value, where index corresponds to DirectInput axis index.
                // This is useful in the following scenario: if binding profiles for your feeder store the source axis index and destination axis index,
                // it's convenient to update the values, because you can just do something like:
                // DirectInput Device Axis Array[i].value to EmuController Axes.SetAxis(j, value);

                EmuClient.InputState.Axes.SetAxis(0, (ushort)rndVal.Next(0, 65535));
                EmuClient.InputState.Axes.SetAxis(1, (ushort)rndVal.Next(0, 65535));
                EmuClient.InputState.Axes.SetAxis(2, (ushort)rndVal.Next(0, 65535));
                EmuClient.InputState.Axes.SetAxis(3, (ushort)rndVal.Next(0, 65535));
                EmuClient.InputState.Axes.SetAxis(4, (ushort)rndVal.Next(0, 65535));
                EmuClient.InputState.Axes.SetAxis(5, (ushort)rndVal.Next(0, 65535));
                EmuClient.InputState.Axes.SetAxis(6, (ushort)rndVal.Next(0, 65535));
                EmuClient.InputState.Axes.SetAxis(7, (ushort)rndVal.Next(0, 65535));

                //// Supports 128 buttons that fit in 16 bytes, just like vJoy.
                EmuClient.InputState.Buttons.SetButton(rndVal.Next(0, 31), true);
                EmuClient.InputState.Buttons.SetButton(rndVal.Next(0, 31), true);
                EmuClient.InputState.Buttons.SetButton(rndVal.Next(0, 31), false);
                EmuClient.InputState.Buttons.SetButton(rndVal.Next(0, 31), false);

                // Supports 4 dpads that have 8 directions. >0x07 = NULL, 0 = top, 
                // incrementing value will advance the dpad position 45 degrees clockwise;
                // Perhaps some enum needs to be defined for more readability.
                EmuClient.InputState.DPads.SetDPad(0, (DPadDirectionEnum)rndVal.Next(-1, 7));
                EmuClient.InputState.DPads.SetDPad(1, (DPadDirectionEnum)rndVal.Next(-1, 7));
                EmuClient.InputState.DPads.SetDPad(2, (DPadDirectionEnum)rndVal.Next(-1, 7));
                EmuClient.InputState.DPads.SetDPad(3, (DPadDirectionEnum)rndVal.Next(-1, 7));

                // There are two ways to update EmuController.
                // First one sends the whole input report.
                // The controls for which values are not set by user, will have the default values.

                // EmuClient.SendInputReport();

                // The other option is to send only the values for controls that are updated by user.
                // Any control that has not been updated will retain the previous value that was sent
                // to Emucontroller device.
                EmuClient.SendUpdate();

                Thread.Sleep(1000);

            }
        }

        private static void FFBDataReceived(object sender, FFBDataReceivedEventArgs e)
        {
            Console.WriteLine(e.ReportId.ToString());
        }
    }
}
