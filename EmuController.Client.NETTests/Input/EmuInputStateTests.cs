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
            EmuControllerInputState emu = new EmuControllerInputState();
            byte[] message = emu.GetInputReportMessage();
            Assert.IsTrue(message.Length == message[1] + headerSize);

            int totalLength = headerSize + 1;
            totalLength += emu.Axes.AllValues.Length * sizeof(ushort);
            totalLength += emu.Buttons.AllValues.Length * sizeof(ushort);
            totalLength += emu.DPads.AllValues.Length;

            Assert.IsTrue(message.Length == totalLength);
        }

        [TestMethod()]
        public void GetStateUpdateMessageTest()
        {
            Random rndVal = new Random();
            int headerSize = 2;
            EmuControllerInputState emu = new EmuControllerInputState();

            for (int i = 0; i < 100; i++)
            {

                AxisEnum ax1 = (AxisEnum)rndVal.Next(0, 8);

                ushort ax1Val = (ushort)rndVal.Next(0, 65536);

                int buttonIndex1 = rndVal.Next(0, 128);

                int dpad1 = rndVal.Next(0, 4);

                DPadDirectionEnum dpad1Val = (DPadDirectionEnum)rndVal.Next(-1, 8);

                emu.Axes.SetValue(ax1, ax1Val);

                emu.Buttons.SetValue(buttonIndex1, true);

                emu.DPads.SetValue(dpad1, dpad1Val);

                byte[] message = emu.GetStateUpdateMessage();

                int expectedMessageLen = 2 * sizeof(ushort) + sizeof(byte) + 6 + headerSize;

                Assert.IsTrue(message.Length == expectedMessageLen);
                Assert.IsTrue(message[1] == message.Length - headerSize);
                Assert.IsTrue(message[2] == (byte)UsageId.Axis);
                Assert.IsTrue(Utils.IsBitSet(message[3], (byte)ax1));
                Assert.IsTrue(BitConverter.ToUInt16(message, 4) == ax1Val);
                Assert.IsTrue(message[6] == (byte)UsageId.Button);
                Assert.IsTrue(Utils.IsBitSet(message[7], buttonIndex1 / 16));
                Assert.IsTrue(Utils.IsBitSet(BitConverter.ToUInt16(message, 8), buttonIndex1 % 16));
                Assert.IsTrue(message[10] == (byte)UsageId.DPad);
                Assert.IsTrue(Utils.IsBitSet(message[11], (byte)dpad1));
                Assert.IsTrue(message[12] == (byte)dpad1Val);
            }

        }
    }
}