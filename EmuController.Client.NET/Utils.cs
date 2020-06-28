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
            string res = string.Empty;
            try
            {
                RegistryKey key = Registry.CurrentUser.OpenSubKey(regPath);
                if (key != null)
                {
                    res = (string)key.GetValue(regKeyName);
                }
            }
            catch
            {
                // If GetValue() fails we just don't want to make a fuss about it,
                // since having no device friendly name does not really affect functionality of our freeder.
            }

            return res;
        }
    }
}
