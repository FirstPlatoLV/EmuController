using System;
using System.Collections.Generic;
using System.Text;

namespace EmuController.Client.NET.Input
{
    public interface IInputState
    {
        void CreateDefaultState();
        byte[] GetStateUpdateMessage();
        byte[] GetInputReportMessage();
    }
}
