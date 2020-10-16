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
using EmuController.Client.NET.PID;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace EmuController.Client.NET.Input
{
    public class EmuInputState
    {
        private const int MessageLengthMax = 128;
        private const int MessageHeaderLength = 2;

        /// <summary>
        /// Stores values for axis type controls.
        /// </summary>
        public Axes Axes { get; private set; }
        
        /// <summary>
        /// Stores values for dpad type controls.
        /// </summary>
        public DPads DPads { get; private set; }
        
        /// <summary>
        /// Stores values for button type controls.
        /// </summary>
        public Buttons Buttons { get; private set; }

        public EmuInputState()
        {
            GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;
            CreateDefaultState();
        }

        internal void CreateDefaultState()
        {
            Axes = new Axes();
            Buttons = new Buttons();
            DPads = new DPads();
        }

        public byte[] GetStateUpdateMessage()
        {
            byte[] message;

            MessageHeader msgHeader = new MessageHeader()
            {
                Type = MessageHeader.MessageType.Input
            };

            MemoryStream stream = new MemoryStream();

            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                writer.Write((byte)msgHeader.Type);
                writer.Write(msgHeader.Length);

                ReadOnlySpan<ushort> axes = Axes.GetUpdatedValues();
                if (axes != null)
                {
                    writer.Write((byte)UsageId.Axis);
                    writer.Write(Axes.ArrayMap);
                    writer.Write(MemoryMarshal.Cast<ushort, byte>(axes));
                }

                ReadOnlySpan<ushort> buttons = Buttons.GetUpdatedValues();
                if (buttons != null)
                {
                    writer.Write((byte)UsageId.Button);
                    writer.Write(Buttons.ArrayMap);
                    writer.Write(MemoryMarshal.Cast<ushort, byte>(buttons));
                }

                ReadOnlySpan<byte> dpads = DPads.GetUpdatedValues();

                if (dpads != null)
                {
                    writer.Write((byte)UsageId.DPad);
                    writer.Write(DPads.ArrayMap);
                    writer.Write(dpads);
                }
                message = stream.ToArray();
            }

            if (message.Length > MessageLengthMax)
            {
                throw new ArgumentException(Resources.ErrorMessageLengthMax);
            }
            else if (message.Length < 3)
            {
                return null;
            }

            // We change the MessageHeader.Length to the MessageLength;
            message[1] = (byte)(message.Length - MessageHeaderLength);
            CreateDefaultState();
            return message;
        }

        public byte[] GetInputReportMessage()
        {
            byte[] message;

            MessageHeader msgHeader = new MessageHeader()
            {
                Type = MessageHeader.MessageType.InputFull
            };

            MemoryStream stream = new MemoryStream();

            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                writer.Write((byte)msgHeader.Type);
                writer.Write(msgHeader.Length);

                byte reportId = 1;
                writer.Write(reportId);

                writer.Write(MemoryMarshal.Cast<ushort, byte>(Buttons.AllValues));
                writer.Write(DPads.AllValues);
                writer.Write(MemoryMarshal.Cast<ushort, byte>(Axes.AllValues));
            }
;
            message = stream.ToArray();

            // We change the MessageHeader.Length to the MessageLength;
            message[1] = (byte)(message.Length - MessageHeaderLength);
            CreateDefaultState();
            return message;
        }
    }
}

