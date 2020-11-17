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
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace EmuController.DevManager
{
    [XmlRoot]
    public class AppSettings
    {
        private static readonly Lazy<AppSettings> lazy = new Lazy<AppSettings>(() => GetSettings()
        );

        public static AppSettings Instance { get { return lazy.Value; } }

        private AppSettings()
        {
        }

        [XmlElement(IsNullable = false)]
        public string FilePath { get; set; }

        [XmlArray(IsNullable = false)]
        public List<string> ExpectedFiles { get; set; }

        public bool IsValid()
        {
            if (ExpectedFiles == null)
            {
                return false;
            }
            foreach (string fileName in ExpectedFiles)
            {
                string fullPath = Path.Combine(Path.GetFullPath(FilePath, Directory.GetCurrentDirectory()));
                if (!File.Exists(Path.Combine(fullPath, fileName)))
                {
                    return false;
                }
            }
            return true;
        }

        private void GetDefaultSettings()
        {
            FilePath = @"drv\";
            ExpectedFiles = new List<string>
            {
                "emucontroller.inf",
                "emucontroller.cat",
                "emucontroller.dll"
            };
        }

        private static AppSettings GetSettings()
        {
            XmlSerializer<AppSettings> appset = new XmlSerializer<AppSettings>();
            AppSettings settings = new AppSettings();
            try
            {
                settings = appset.Read("AppSettings.xml");
            }
            catch
            {
                settings.GetDefaultSettings();
            }
            return settings;
        }
    }
}
