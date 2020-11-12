// Copyright 2020 Maris Melbardis
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
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
