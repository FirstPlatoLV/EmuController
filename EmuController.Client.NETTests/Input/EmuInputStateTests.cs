using Microsoft.VisualStudio.TestTools.UnitTesting;
using EmuController.Client.NET.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace EmuController.Client.NET.Input.Tests
{
    [TestClass()]
    public class EmuInputStateTests
    {
        [TestMethod()]
        public void GetInputReportMessageTest()
        {
            int headerSize = 2;
            EmuInputState emu = new EmuInputState();
            byte[] message = emu.GetInputReportMessage();
            Assert.IsTrue(message.Length == message[1] + headerSize);

            int totalLength = headerSize + 1;
            totalLength += emu.Axes.AllValues.Length * sizeof(ushort);
            totalLength += emu.Buttons.AllValues.Length * sizeof(ushort);
            totalLength += emu.DPads.AllValues.Length;

            Assert.IsTrue(message.Length == totalLength);
        }
    }
}