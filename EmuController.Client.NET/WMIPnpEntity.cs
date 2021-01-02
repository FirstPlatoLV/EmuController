using System;
using System.Collections.Generic;
using System.Management;
using System.Runtime.Versioning;
using System.Text;

namespace EmuController.Client.NET
{
    public sealed class WMIPnpEntity
    {
        private static readonly string Scope = "root\\CIMV2";
        private const string ClassGuid = "{4DB9B76D-4379-437E-9457-4F00B3090C1F}";
        private readonly ManagementObjectSearcher Mos;
        private static readonly Lazy<WMIPnpEntity>
            lazy =
            new Lazy<WMIPnpEntity>
                (() => new WMIPnpEntity());

        public static WMIPnpEntity Instance { get { return lazy.Value; } }

        private WMIPnpEntity()
        {
            string queryString = $"SELECT * FROM Win32_PnPEntity WHERE ClassGuid = '{ClassGuid}'";
            Mos = new ManagementObjectSearcher(Scope, queryString);
        }

        public IList<EmuControllerInfo> GetEmuControllers()
        {
            List<EmuControllerInfo> devices = new List<EmuControllerInfo>();

            // All active EmuController devices will be returned when quering PnPEntities 
            // matching EmuController classGuid.
            foreach (ManagementObject queryObj in Mos.Get())
            {
                EmuControllerInfo info = new EmuControllerInfo(queryObj);
                devices.Add(info);
            }

            return devices;
        }
    }
}
