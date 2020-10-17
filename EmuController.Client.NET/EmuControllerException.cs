using System;
using System.Collections.Generic;
using System.Text;

namespace EmuController.Client.NET
{
    class EmuControllerException : Exception
    {
        public EmuControllerException(string message) : base(message)
        {

        }
    }
}
