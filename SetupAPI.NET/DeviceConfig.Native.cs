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
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SetupAPI.NET
{
    public partial class DeviceConfig
    {
        private const uint MAX_PATH = 260;
        private const uint MAX_CLASS_NAME_LEN = 128;
        private const uint DI_REMOVEDEVICE_GLOBAL = 1;
        private const uint DICD_GENERATE_ID = 1;
        private const uint SPDRP_HARDWAREID = 1;
        private const uint DIGCF_ALLCLASSES = 4;
        private const uint SP_MAX_MACHINENAME_LENGTH = MAX_PATH + 3;

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct SP_DEVINFO_LIST_DETAIL_DATA
        {
            public uint CbSize;
            public Guid ClassGuid;
            public IntPtr RemoteMachineHandle;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = (int)SP_MAX_MACHINENAME_LENGTH)]
            public string RemoteMachineName;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SP_CLASSINSTALL_HEADER
        {
            public uint CbSize;
            public DI_FUNCTION InstallFunction;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SP_REMOVEDEVICE_PARAMS
        {
            public SP_CLASSINSTALL_HEADER ClassInstallHeader;
            public uint Scope;
            public uint HwProfile;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SP_PROPCHANGE_PARAMS
        {
            public SP_CLASSINSTALL_HEADER ClassInstallHeader;
            public DIC_STATE StateChange;
            public uint Scope;
            public uint HwProfile;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct SP_DEVINFO_DATA
        {
            public uint CbSize;
            public Guid ClassGuid;
            public uint DevInst;
            public IntPtr Reserved;
        }


        private enum DIC_STATE
        {
            DICS_ENABLE = 1,
            DICS_DISABLE = 2,
            DICS_PROPCHANGE = 3,
            DICS_START = 4,
            DICS_STOP = 5
        }

        private enum DI_FUNCTION
        {
            DIF_REMOVE = 5,
            DIF_PROPERTYCHANGE = 18,
            DIF_REGISTERDEVICE = 25
        }

        private const uint INSTALLFLAG_FORCE = 0x00000001;  // Force the installation of the specified driver

        [DllImport("Setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool SetupDiGetINFClass(
            string InfName,
            ref Guid ClassGuid,
            [Out] char[] ClassName,
            uint ClassNameSize,
            ref uint RequiredSize);

        [DllImport("Setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetupDiGetClassDevsEx(
            IntPtr ClassGuid,
            IntPtr Enumerator,
            IntPtr hwndParent,
            uint Flags,
            IntPtr DeviceInfoSet,
            IntPtr MachineName,
            IntPtr Reserved);

        [DllImport("Setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool SetupDiCreateDeviceInfo(
            IntPtr DeviceInfoSet,
            string ClassName,
            ref Guid ClassGuid,
            string DeviceDescription,
            IntPtr hwndParent,
            uint CreationFlags,
            ref SP_DEVINFO_DATA DeviceInfoData);

        [DllImport("Setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetupDiCreateDeviceInfoList(
            ref Guid ClassGuid,
            IntPtr hwndParent);

        [DllImport("Setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool SetupDiDestroyDeviceInfoList(
            IntPtr DeviceInfoSet);

        [DllImport("Setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool SetupDiEnumDeviceInfo(
            IntPtr DeviceInfoSet,
            uint MemberIndex,
            ref SP_DEVINFO_DATA DeviceInfoData);

        [DllImport("Setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool SetupDiGetDeviceRegistryProperty(
            IntPtr DeviceInfoSet,
            ref SP_DEVINFO_DATA DeviceInfoData,
            uint Property, IntPtr PropertyRegDataType,
            IntPtr PropertyBuffer,
            uint PropertyBufferSize,
            IntPtr RequiredSize);

        [DllImport("Setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool SetupDiGetDeviceRegistryProperty(
            IntPtr DeviceInfoSet,
            ref SP_DEVINFO_DATA DeviceInfoData,
            uint Property, IntPtr PropertyRegDataType,
            IntPtr PropertyBuffer,
            uint PropertyBufferSize,
            ref uint RequiredSize);

        [DllImport("Setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool SetupDiSetDeviceRegistryProperty(
            IntPtr DeviceInfoSet,
            ref SP_DEVINFO_DATA DeviceInfoData,
            uint Property,
            byte[] PropertyBuffer,
            uint PropertyBufferSize);

        [DllImport("Setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool SetupDiCallClassInstaller(
            DI_FUNCTION InstallFunction,
            IntPtr DeviceInfoSet,
            ref SP_DEVINFO_DATA DeviceInfoData);

        [DllImport("Setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool SetupDiSetClassInstallParams(
            IntPtr DeviceInfoSet,
            ref SP_DEVINFO_DATA DeviceInfoData,
            ref SP_REMOVEDEVICE_PARAMS ClassInstallParams,
            uint ClassInstallParamsSize);

        [DllImport("Setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool SetupDiSetClassInstallParams(
            IntPtr DeviceInfoSet,
            ref SP_DEVINFO_DATA DeviceInfoData,
            ref SP_PROPCHANGE_PARAMS ClassInstallParams,
            uint ClassInstallParamsSize);

        [DllImport("Setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool SetupDiChangeState(
            IntPtr DeviceInfoSet,
            ref SP_DEVINFO_DATA DeviceInfoData);

        [DllImport("NewDev.dll", CharSet = CharSet.Auto)]
        private extern static bool UpdateDriverForPlugAndPlayDevices(
            IntPtr HwndParent,
            string SzHardwordID,
            string SzINFName,
            uint InstallFlags,
            ref bool BRebootRequired);
    }
}


