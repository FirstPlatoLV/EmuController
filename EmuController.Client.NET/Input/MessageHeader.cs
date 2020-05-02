using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmuController.Client.NET.Input
{
    class MessageHeader
    {
        public MessageType Type { get; set; } = MessageType.Input;
        public byte Length { get; set; }

        public enum MessageType
        {
            Input = 0x01,
            InputFull = 0x02,
            PID = 0x03,
        }
    }
}
