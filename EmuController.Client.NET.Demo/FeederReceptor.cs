using EmuController.Client.NET.Input;
using EmuController.Client.NET.PID;
using System;
using System.Threading;

namespace EmuController.Client.NET.Demo
{
    class FeederReceptor
    {
        private readonly EmuControllerClient EmuClient;
        public FeederReceptor()
        {
            // We initialize the EmuControllerupdater;
            // The list EmuClient.ConnectedEmuControllerDevices will be populated
            // by available EmuController devices upon intitializing EmuControllerClient.
            EmuClient = new EmuControllerClient();

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

            // Just a randomizer to provide random values to simulate input;
            Random rndVal = new Random();
            while (EmuClient.InputClientConnected)
            {

                // Provide axistype and value to set it's state. When AxisEnum is cast as integer, the values correspond to DirectInput axis gamepad values.
                // This is useful, when you want to directly bind from Axis array returned by DirectInput to Emucontroller, specifying indices for both, or example:
                // DirectInput Device Axis Array[i].value to EmuController Axes.SetAxis((AxisEnum)i, value);

                EmuClient.InputState.Axes.SetValue(AxisEnum.AxisY, (ushort)rndVal.Next(0, 65535));
                EmuClient.InputState.Axes.SetValue(AxisEnum.AxisY, (ushort)rndVal.Next(0, 65535));
                EmuClient.InputState.Axes.SetValue(AxisEnum.AxisZ, (ushort)rndVal.Next(0, 65535));
                EmuClient.InputState.Axes.SetValue(AxisEnum.AxisRx, (ushort)rndVal.Next(0, 65535));
                EmuClient.InputState.Axes.SetValue(AxisEnum.AxisRy, (ushort)rndVal.Next(0, 65535));
                EmuClient.InputState.Axes.SetValue(AxisEnum.AxisRz, (ushort)rndVal.Next(0, 65535));
                EmuClient.InputState.Axes.SetValue(AxisEnum.AxisSlider, (ushort)rndVal.Next(0, 65535));
                EmuClient.InputState.Axes.SetValue(AxisEnum.AxisDial, (ushort)rndVal.Next(0, 65535));

                // Supports 128 buttons that fit in 16 bytes.
                EmuClient.InputState.Buttons.SetValue(rndVal.Next(0, 31), true);
                EmuClient.InputState.Buttons.SetValue(rndVal.Next(0, 31), false);

                EmuClient.InputState.Buttons.SetValue(rndVal.Next(0, 31), true);
                EmuClient.InputState.Buttons.SetValue(rndVal.Next(0, 31), false);
                EmuClient.InputState.Buttons.SetValue(rndVal.Next(32, 127), true);
                EmuClient.InputState.Buttons.SetValue(rndVal.Next(32, 127), false);
                EmuClient.InputState.Buttons.SetValue(rndVal.Next(32, 127), true);
                EmuClient.InputState.Buttons.SetValue(rndVal.Next(32, 127), false);

                // Supports 4 dpads that have 8 directions. -1 = Null, 0 = North, 
                // incrementing value will advance the dpad position 45 degrees clockwise;
                EmuClient.InputState.DPads.SetValue(0, (DPadDirectionEnum)rndVal.Next(-1, 7));
                EmuClient.InputState.DPads.SetValue(1, (DPadDirectionEnum)rndVal.Next(-1, 7));
                EmuClient.InputState.DPads.SetValue(2, (DPadDirectionEnum)rndVal.Next(-1, 7));
                EmuClient.InputState.DPads.SetValue(3, (DPadDirectionEnum)rndVal.Next(-1, 7));

                // There are two ways to update EmuController.
                // First one sends the whole input report.
                // The controls for which values are not set by user, will have the default values.

                //EmuClient.SendInputReport();

                // The other option is to send only the values for controls that are updated by user.
                // Any control that has not been updated will retain the previous value that was sent
                // to Emucontroller device.
                EmuClient.SendUpdate();
            }
        }

        private void FFBDataReceived(object sender, FFBDataReceivedEventArgs e)
        {
            // Outputs packet type that EmuController received.
           // Console.WriteLine(e.ReportId.ToString());
        }
    }
}
