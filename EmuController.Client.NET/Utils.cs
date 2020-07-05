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
                // since having no device friendly name does not really affect functionality of our feeder.
            }

            return res;
        }
    }
}
