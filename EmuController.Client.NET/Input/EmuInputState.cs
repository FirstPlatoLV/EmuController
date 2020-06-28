using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
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
        public DPad DPads { get; private set; }
        
        /// <summary>
        /// Stores values for button type controls.
        /// </summary>
        public Buttons Buttons { get; private set; }

        internal EmuInputState()
        {
            CreateDefaultState();
        }

        internal void CreateDefaultState()
        {
            Axes = new Axes();
            Buttons = new Buttons();
            DPads = new DPad();
        }

        internal byte[] GetStateUpdateMessage()
        {
            byte[] message;
            byte messageLen;

            MessageHeader msgHeader = new MessageHeader()
            {
                Type = MessageHeader.MessageType.Input
            };

            MemoryStream stream = new MemoryStream();

            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                writer.Write((byte)msgHeader.Type);
                writer.Write(msgHeader.Length);


                byte[] arrayMap = new byte[1];
                Axes.ArrayMap.CopyTo(arrayMap, 0);

                if (arrayMap[0] != 0)
                {
                    writer.Write((byte)Axes.Usage);
                    writer.Write(arrayMap);

                    for (int i = 0; i < Axes.ArrayMap.Count; i++)
                    {
                        if (Axes.ArrayMap.Get(i))
                        {
                            writer.Write(Axes.AxisValues[i]);
                        }
                    }
                }

                Buttons.ArrayMap.CopyTo(arrayMap, 0);

                if (arrayMap[0] != 0)
                {
                    Buttons.GetButtons();
                    writer.Write((byte)Buttons.Usage);
                    writer.Write(arrayMap);

                    for (int i = 0; i < Buttons.ArrayMap.Count; i++)
                    {
                        if (Buttons.ArrayMap.Get(i))
                        {
                            writer.Write(Buttons.ButtonValues[i]);
                        }
                    }
                }

                DPads.ArrayMap.CopyTo(arrayMap, 0);

                if (arrayMap[0] != 0)
                {
                    writer.Write((byte)DPads.Usage);
                    writer.Write(arrayMap);
                    for (int i = 0; i < DPads.ArrayMap.Count; i++)
                    {
                        if (DPads.ArrayMap.Get(i))
                        {
                            writer.Write(DPads.DPads[i]);
                        }
                    }
                }
            }
            message = stream.ToArray();
            

            if (message.Length > MessageLengthMax)
            {
                throw new Exception(Resources.ErrorMessageLengthMax);
            }
            messageLen = (byte)(message.Length - MessageHeaderLength);
            
            // We change the MessageHeader.Length to the MessageLength;
            message[1] = messageLen;

            // Nothing to update
            if (messageLen < 3)
            {
                return null;
            }

            CreateDefaultState();
            return message;
        }

        internal byte[] GetInputReportMessage()
        {
            byte[] message;
            byte messageLen;

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

                Buttons.GetButtons();

                for (int i = 0; i < Buttons.ButtonValues.Length; i++)
                {
                    writer.Write(Buttons.ButtonValues[i]);
                }

                for (int i = 0; i < DPads.DPads.Length; i++)
                {
                    writer.Write(DPads.DPads[i]);
                }

                for (int i = 0; i < Axes.AxisValues.Length; i++)
                {
                    writer.Write(Axes.AxisValues[i]);
                }
            }

            message = stream.ToArray();
            


            messageLen = (byte)(message.Length - MessageHeaderLength);

            // We change the MessageHeader.Length to the MessageLength;
            message[1] = messageLen;

            // Nothing to update
            if (messageLen < 3)
            {
                return null;
            }

            CreateDefaultState();
            return message;
        }
    }
}

