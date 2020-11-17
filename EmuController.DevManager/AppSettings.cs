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
