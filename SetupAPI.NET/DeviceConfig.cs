using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace SetupAPI.NET
{
    public partial class DeviceConfig
    {
        /// <summary>
        /// Install a device defined in the INF file with matching hardwareId
        /// </summary>
        /// <param name="infFilePath">Full path to the driver INF file</param>
        /// <param name="hardwareId">Identification string for the hardware to be installed</param>
        /// <param name="restartNeeded">Returns 'true' if OS restart is needed for installation to complete</param>
        public static void InstallDeviceFromInf(string infFilePath, string hardwareId, out bool restartNeeded)
        {
            restartNeeded = false;

            infFilePath = Path.GetFullPath(infFilePath);
            Guid classGuid = new Guid();
            char[] className = new char[MAX_CLASS_NAME_LEN];
            uint requiredSize = 0;
            SP_DEVINFO_DATA deviceInfoData = new SP_DEVINFO_DATA();
            deviceInfoData.CbSize = (uint)Marshal.SizeOf(deviceInfoData);
            IntPtr deviceInfoSet = IntPtr.Zero;
            try
            {
                SetupDiGetINFClass(
                    infFilePath,
                    ref classGuid,
                    className,
                    MAX_CLASS_NAME_LEN,
                    ref requiredSize);

                deviceInfoSet = SetupDiCreateDeviceInfoList(
                    ref classGuid,
                    IntPtr.Zero);

                string description = string.Empty;
                SetupDiCreateDeviceInfo(
                    deviceInfoSet,
                    new string(className),
                    ref classGuid, description,
                    IntPtr.Zero, DICD_GENERATE_ID,
                    ref deviceInfoData);

                SetupDiSetDeviceRegistryProperty(
                    deviceInfoSet,
                    ref deviceInfoData,
                    SPDRP_HARDWAREID,
                    Encoding.Unicode.GetBytes(hardwareId),
                    (uint)Encoding.Unicode.GetByteCount(hardwareId));

                SetupDiCallClassInstaller(
                    DI_FUNCTION.DIF_REGISTERDEVICE,
                    deviceInfoSet,
                    ref deviceInfoData);

                UpdateDriverForPlugAndPlayDevices(
                    IntPtr.Zero,
                    hardwareId,
                    infFilePath,
                    INSTALLFLAG_FORCE,
                    ref restartNeeded);
            }
            finally
            {
                if (deviceInfoSet != IntPtr.Zero)
                {
                    SetupDiDestroyDeviceInfoList(deviceInfoSet);
                }
            }
            SetupDiDestroyDeviceInfoList(deviceInfoSet);
        }

        /// <summary>
        /// Removes devices with the matching hardwareId
        /// </summary>
        /// <param name="hardwareId">Identification string for the hardware to be removed</param>
        public static void UninstallDevice(string hardwareId)
        {
            IntPtr deviceInfoSet = IntPtr.Zero;

            try
            {
                deviceInfoSet = SetupDiGetClassDevsEx(
                    IntPtr.Zero,
                    IntPtr.Zero,
                    IntPtr.Zero,
                    DIGCF_ALLCLASSES,
                    IntPtr.Zero,
                    IntPtr.Zero,
                    IntPtr.Zero);

                SP_DEVINFO_DATA deviceInfoData = new SP_DEVINFO_DATA();

                deviceInfoData.CbSize = (uint)Marshal.SizeOf(deviceInfoData);
                uint index = 0;
                while (SetupDiEnumDeviceInfo(deviceInfoSet, index, ref deviceInfoData))
                {
                    index++;

                    uint requiredSize = 0;
                    SetupDiGetDeviceRegistryProperty(
                        deviceInfoSet,
                        ref deviceInfoData,
                        SPDRP_HARDWAREID,
                        IntPtr.Zero,
                        IntPtr.Zero,
                        0,
                        ref requiredSize);

                    if (requiredSize == 0)
                    {
                        continue;
                    }

                    IntPtr pPropertyBuffer = Marshal.AllocHGlobal((int)requiredSize);

                    if (!SetupDiGetDeviceRegistryProperty(
                        deviceInfoSet,
                        ref deviceInfoData,
                        SPDRP_HARDWAREID,
                        IntPtr.Zero,
                        pPropertyBuffer,
                        requiredSize,
                        IntPtr.Zero))
                    {
                        continue;
                    }

                    string hwId = Marshal.PtrToStringAuto(pPropertyBuffer);
                    Marshal.FreeHGlobal(pPropertyBuffer);

                    if (hwId.Equals(hardwareId))
                    {
                        SP_REMOVEDEVICE_PARAMS classInstallParams = new SP_REMOVEDEVICE_PARAMS();
                        classInstallParams.ClassInstallHeader.CbSize = (uint)Marshal.SizeOf(classInstallParams.ClassInstallHeader);
                        classInstallParams.ClassInstallHeader.InstallFunction = DI_FUNCTION.DIF_REMOVE;
                        classInstallParams.Scope = DI_REMOVEDEVICE_GLOBAL;

                        SetupDiSetClassInstallParams(
                            deviceInfoSet,
                            ref deviceInfoData,
                            ref classInstallParams,
                            (uint)Marshal.SizeOf(classInstallParams));

                        SetupDiCallClassInstaller(
                            DI_FUNCTION.DIF_REMOVE,
                            deviceInfoSet,
                            ref deviceInfoData);
                    }
                }
            }
            finally
            {
                if (deviceInfoSet != IntPtr.Zero)
                {
                    SetupDiDestroyDeviceInfoList(deviceInfoSet);
                }
            }
        }
        /// <summary>
        /// Enables all devices with the matching hardwareId
        /// </summary>
        /// <param name="hardwareId">Identification string for the hardware to be enabled</param>
        public static void EnableDevice(string hardwareId)
        {
            ChangeDeviceState(hardwareId, DIC_STATE.DICS_ENABLE);
        }

        /// <summary>
        /// Disables all devices with the matching hardwareId
        /// </summary>
        /// <param name="hardwareId">Identification string for the hardware to be disabled</param>
        public static void DisableDevice(string hardwareId)
        {
            ChangeDeviceState(hardwareId, DIC_STATE.DICS_DISABLE);
        }

        private static int ChangeDeviceState(string hardwareId, DIC_STATE stateChange)
        {

            IntPtr deviceInfoSet = IntPtr.Zero;
            int error = 0;

            try
            {
                deviceInfoSet = SetupDiGetClassDevsEx(
                    IntPtr.Zero,
                    IntPtr.Zero,
                    IntPtr.Zero,
                    DIGCF_ALLCLASSES,
                    IntPtr.Zero,
                    IntPtr.Zero,
                    IntPtr.Zero);

                SP_DEVINFO_DATA deviceInfoData = new SP_DEVINFO_DATA();

                deviceInfoData.CbSize = (uint)Marshal.SizeOf(deviceInfoData);
                uint index = 0;
                while (SetupDiEnumDeviceInfo(
                    deviceInfoSet,
                    index,
                    ref deviceInfoData))
                {
                    index++;

                    uint requiredSize = 0;
                    SetupDiGetDeviceRegistryProperty(
                        deviceInfoSet,
                        ref deviceInfoData,
                        SPDRP_HARDWAREID,
                        IntPtr.Zero,
                        IntPtr.Zero, 
                        0,
                        ref requiredSize);

                    if (requiredSize == 0)
                    {
                        continue;
                    }

                    IntPtr pPropertyBuffer = Marshal.AllocHGlobal((int)requiredSize);

                    if (!SetupDiGetDeviceRegistryProperty(
                        deviceInfoSet,
                        ref deviceInfoData,
                        SPDRP_HARDWAREID,
                        IntPtr.Zero,
                        pPropertyBuffer,
                        requiredSize,
                        IntPtr.Zero))
                    {
                        continue;
                    }

                    string hwId = Marshal.PtrToStringAuto(pPropertyBuffer);
                    Marshal.FreeHGlobal(pPropertyBuffer);

                    if (hwId.Equals(hardwareId))
                    {
                        SP_PROPCHANGE_PARAMS propChangeParams = new SP_PROPCHANGE_PARAMS();
                        propChangeParams.ClassInstallHeader.CbSize = (uint)Marshal.SizeOf(propChangeParams.ClassInstallHeader);
                        propChangeParams.ClassInstallHeader.InstallFunction = DI_FUNCTION.DIF_PROPERTYCHANGE;
                        propChangeParams.Scope = DI_REMOVEDEVICE_GLOBAL;
                        propChangeParams.StateChange = stateChange;
                        if(!SetupDiSetClassInstallParams(
                            deviceInfoSet, 
                            ref deviceInfoData, 
                            ref propChangeParams, 
                            (uint)Marshal.SizeOf(propChangeParams)))
                        {
                            error = Marshal.GetLastWin32Error();
                        }
                        if(!SetupDiChangeState(
                            deviceInfoSet,
                            ref deviceInfoData))
                        {
                            error = Marshal.GetLastWin32Error();
                        }
                    }
                }
            }
            finally
            {
                if (deviceInfoSet != IntPtr.Zero)
                {
                    SetupDiDestroyDeviceInfoList(deviceInfoSet);
                }
            }

            return error;
        }
    }
}
