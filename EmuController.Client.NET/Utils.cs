using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Text;

namespace EmuController.Client.NET
{
    public static class Utils
    {
        public static string GetStringFromRegistry(string regPath, string regKeyName)
        {
            try
            {
                RegistryKey key = Registry.CurrentUser.OpenSubKey(regPath);
                if (key != null)
                {
                    return (string)key.GetValue(regKeyName);
                }
            }
            finally
            {

            }

            return string.Empty;
        }
    }
}
