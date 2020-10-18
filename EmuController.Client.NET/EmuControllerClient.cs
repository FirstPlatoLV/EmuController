// Copyright 2020 Maris Melbardis
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
using EmuController.Client.NET.Input;
using EmuController.Client.NET.PID;
using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Management;
using System.Threading;
using System.Threading.Tasks;

namespace EmuController.Client.NET
{
    public class EmuControllerClient : IDisposable
    {

        private readonly Guid EmuControllerClassGuid = Guid.Parse("4DB9B76D-4379-437E-9457-4F00B3090C1F");

        /// <summary>
        /// The event that is signaled once FFB data is sent from application (game) to EmuController device.
        /// The FFBPacket property contains the data.
        /// </summary>
        public event EventHandler<FFBDataReceivedEventArgs> FFBDataReceived;

        private ManagementObjectSearcher ManagementObjectSearcher;
        private CancellationTokenSource TaskCancelToken;

        private EmuControllerInfo InstanceInfo;
        protected virtual void OnFFBDataReceived(FFBDataReceivedEventArgs e)
        {
            EventHandler<FFBDataReceivedEventArgs> handler = FFBDataReceived;
            handler?.Invoke(this, e);
        }


        /// <summary>
        /// The input state of the EmuController device with default values set.
        /// </summary>
        public EmuInputState InputState { get; private set; }

        /// <summary>
        /// List of detected active EmuController devices.
        /// To establish connection with EmuController device, one of EmuControllerInfo objects must be passed as parameter.
        /// </summary>
        public IList<EmuControllerInfo> EmuControllerDevices { get; private set; }

        private NamedPipeClientStream InputPipeClient;
        private NamedPipeClientStream FFBPipeClient;


        /// <summary>
        /// Exposes InputClient connection state
        /// </summary>
        public bool InputClientConnected
        {
            get
            {
                if (InputPipeClient == null)
                {
                    return false;
                }
                else
                {
                    return InputPipeClient.IsConnected;
                }
            }
        }

        /// <summary>
        /// Exposes FFBClient connection state
        /// </summary>
        public bool FFBClientConnected
        {
            get
            {
                if (FFBPipeClient == null)
                {
                    return false;
                }
                else
                {
                    return FFBPipeClient.IsConnected;
                }
            }
        }


        private const string InputPipeSuffix = "_input";
        private const string FFBPipeSuffix = "_ffb";

        private const int PipeTimeOut = 1000;

        /// <summary>
        /// Initializes EmuControllerClient object and automatically populates ConnectedEmuControllerDevices with available devices.
        /// </summary>
        public EmuControllerClient()
        {
            InputState = new EmuInputState();
            GetEmuControllers();
        }

        ~EmuControllerClient()
        {
            Dispose();
        }


        /// <summary>
        /// Establishes a connection with EmuController device
        /// to send input state reports.
        ///
        /// </summary>
        /// <param name="instanceInfo">Parameter containg the data required to connect to the Emucontroller device.
        /// The available EmuControllerInfo objects are contained in the 'ConnectedEmuControllerDevices' which is populated
        /// automatically when initializing EmuControllerClient.</param>
        public bool ConnectInputClient(EmuControllerInfo instanceInfo)
        {
            if (InputPipeClient != null && InputPipeClient.IsConnected)
            {
                return false;
            }

            InstanceInfo = instanceInfo;

            InputPipeClient = new NamedPipeClientStream(".", instanceInfo.HardwareId + InputPipeSuffix, PipeDirection.Out);
            InputPipeClient.Connect(PipeTimeOut);

            InputPipeClient.ReadMode = PipeTransmissionMode.Message;

            return true;
        }


        /// <summary>
        /// Establishes a connection with EmuController device
        /// to receive FFB data from applications.
        ///
        /// </summary>
        /// <param name="instanceInfo">Parameter containg the data required to connect to the Emucontroller device.
        /// The available EmuControllerInfo objects are contained in the 'ConnectedEmuControllerDevices' which is populated
        /// automatically when initializing EmuControllerClient.</param>
        public bool ConnectFFBClient(EmuControllerInfo instanceInfo)
        {
            if (FFBPipeClient != null && FFBPipeClient.IsConnected)
            {
                return false;
            }

            InstanceInfo = instanceInfo;

            FFBPipeClient = new NamedPipeClientStream(".", InstanceInfo.HardwareId + FFBPipeSuffix, PipeDirection.InOut);
            FFBPipeClient.Connect(PipeTimeOut);

            FFBPipeClient.ReadMode = PipeTransmissionMode.Message;

            TaskCancelToken = new CancellationTokenSource();

            _ = Task.Run(() => GetFFBMessages(), TaskCancelToken.Token).ConfigureAwait(false);

            return true;
        }

        /// <summary>
        /// Sends states for controls that are updated by user.
        /// Any control not updated by user will retain the last value the driver
        /// received for that control.
        /// </summary>
        public void SendUpdate()
        {
            byte[] buffer = InputState.GetStateUpdateMessage();
            InputPipeClient.Write(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// Sends complete input state report to the driver.
        /// Any control not updated by user will have the default value.
        /// </summary>
        public void SendInputReport()
        {
            byte[] buffer = InputState.GetInputReportMessage();
            InputPipeClient.Write(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// Reports to host about the FFB system of the EmuController
        /// </summary>
        /// <param name="isPaused">Report the state of FFB system</param>
        public void ReportFFBState(bool isPaused)
        {
            byte[] message = new byte[6];

            MessageHeader msgHeader = new MessageHeader()
            {
                Type = MessageHeader.MessageType.PID
            };

            message[1] = 4;
            message[2] = (byte)(isPaused? 1 : 0);
            InputPipeClient.Write(message, 0, message.Length);
        }

        /// <summary>
        /// Reports to host about the state of the FFB Effect.
        /// </summary>
        /// <param name="index">The Effect Block Index</param>
        /// <param name="isPlaying">State of the effect</param>
        public void ReportFFBEffectState(int index, bool isPlaying)
        {
            if (index < 1 || index > 127)
            {
                throw new EmuControllerException("Index out of range!");
            }

            byte[] message = new byte[6];

            MessageHeader msgHeader = new MessageHeader()
            {
                Type = MessageHeader.MessageType.PID
            };

            message[1] = 4;
            message[3] = (byte)(isPlaying ? 1 : 0);
            message[4] = (byte)index;
            InputPipeClient.Write(message, 0, message.Length);
        }


        private Task GetFFBMessages()
        {
            // No output report for current EmuController HID report exceeds 32 bytes.
            byte[] buffer = new byte[32];

            // Internally driver restarts FFB named pipe server whenever connection is closed with input client,
            // which closes the current connection with FFB client, causing Read() to fail.
            while (FFBPipeClient.IsConnected)
            {
                FFBPipeClient.Read(buffer, 0, buffer.Length);
                FFBDataReceivedEventArgs eventArgs = new FFBDataReceivedEventArgs(buffer);
                OnFFBDataReceived(eventArgs);
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Disconnects input client from the EmuController device.
        /// Will also disconnect FFBPipeClient if it is connected.
        /// </summary>
        public void CloseInputClient()
        {
            if (InputPipeClient.IsConnected)
            {
                InputPipeClient.Close();
                if (FFBPipeClient.IsConnected)
                {
                    FFBPipeClient.Close();
                }
            }
        }

        /// <summary>
        /// Disconnects FFB client from the EmuController device.
        /// </summary>
        public void CloseFFBClient()
        {
            if (FFBPipeClient.IsConnected)
            {
                TaskCancelToken.Cancel();
                FFBPipeClient.Close();
            }
        }

        /// <summary>
        /// Dispose the EmuControllerClient object. 
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                CloseInputClient();
                InputPipeClient.Dispose();
                FFBPipeClient.Dispose();
                TaskCancelToken.Dispose();
                ManagementObjectSearcher.Dispose();
            }
        }

        private void GetEmuControllers()
        {

            string scope = "root\\CIMV2";
            string classGuid = "{" + EmuControllerClassGuid.ToString() + "}";
         

            string queryString = $"SELECT * FROM Win32_PnPSignedDriver WHERE ClassGuid = '{classGuid}'";
            ManagementObjectSearcher = new ManagementObjectSearcher(scope, queryString);
            EmuControllerDevices = new List<EmuControllerInfo>();

            // All active EmuController devices will be returned when quering PnPEntities 
            // matching EmuController classGuid.
            foreach (ManagementObject queryObj in ManagementObjectSearcher.Get())
            {
                EmuControllerInfo info = new EmuControllerInfo(queryObj);
                EmuControllerDevices.Add(info);
            }
        }

    }
}
