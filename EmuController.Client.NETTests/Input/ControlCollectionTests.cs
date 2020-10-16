using Microsoft.VisualStudio.TestTools.UnitTesting;
using EmuController.Client.NET.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace EmuController.Client.NET.Input.Tests
{
    [TestClass()]
    public class ControlCollectionTests
    {
        [TestMethod()]
        public void GetUpdatedValuesTest()
        {
            EmuInputState emu = new EmuInputState();

            emu.Buttons.SetValue(0, true);
            emu.Buttons.SetValue(0, false);

            emu.Buttons.SetValue(8, true);

            emu.Buttons.SetValue(16, true);
            emu.Buttons.SetValue(16, false);

            ReadOnlySpan<ushort> updatedButtons = emu.Buttons.GetUpdatedValues();

            Assert.IsTrue(updatedButtons.Length == 2);
        }
    }
}