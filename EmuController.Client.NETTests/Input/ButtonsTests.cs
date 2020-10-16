using Microsoft.VisualStudio.TestTools.UnitTesting;
using EmuController.Client.NET.Input;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit.Sdk;
using System.Security.Cryptography;

namespace EmuController.Client.NET.Input.Tests
{
    [TestClass()]
    public class ButtonsTests
    {
        [TestMethod()]
        public void SetValueTest()
        {
            EmuInputState emu = new EmuInputState();

            Random rndVal = new Random();

            for (int i = 0; i < emu.Buttons.AllValues.Length * sizeof(ushort); i++)
            {
                int index = rndVal.Next(0, emu.Buttons.AllValues.Length * sizeof(ushort));
                emu.Buttons.SetValue(index, true);

                Assert.IsTrue(Utils.IsBitSet(emu.Buttons.AllValues[index / 16], index % 16));
                
                emu.Buttons.SetValue(index, false);
                Assert.IsFalse(Utils.IsBitSet(emu.Buttons.AllValues[index / 16], index % 16));
            }
        }
    }
}