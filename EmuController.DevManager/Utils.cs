using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Text;

namespace EmuController.DevManager
{
    public static class Utils
    {
        public static string GetStringFromRegistry(string regPath, string regKeyName)
        {
            RegistryKey key = null;
            string res = string.Empty;
            try
            {
                key = Registry.CurrentUser.OpenSubKey(regPath);
                if (key != null)
                {
                    res = (string)key.GetValue(regKeyName);
                }
            }
            finally
            {
                if (key != null)
                {
                    key.Dispose();
                }
            }

            return res;
        }

        public static string SetStringFromRegistry(string regPath, string regKeyName, string textString)
        {
            RegistryKey key = null;
            string res = string.Empty;
            try
            {
                key = Registry.CurrentUser.CreateSubKey(regPath, true);
                if (key != null)
                {
                    key.SetValue(regKeyName, textString, RegistryValueKind.String);
                }
            }
            finally
            {
                if (key != null)
                {
                    key.Dispose();
                }
            }

            return res;
        }
    }
}
