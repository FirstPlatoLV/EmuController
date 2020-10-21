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

/*++
Module Name:
	device.h
Abstract:
	This file contains the device definitions.
Environment:
	User-mode Driver Framework 2
--*/

#include "public.h"

EXTERN_C_START

typedef UCHAR HID_REPORT_DESCRIPTOR, * PHID_REPORT_DESCRIPTOR;

#define HID_DEVICE_VID 0xDEED  // VendorID
#define HID_DEVICE_VERSION 0x0001

#define MAXIMUM_STRING_LENGTH (126 * sizeof(WCHAR))
#define HID_DEVICE_STRING L"EmuController Device"  
#define HID_DEVICE_MANUFACTURER_STRING L"Maris Melbardis"  
#define HID_DEVICE_PRODUCT_STRING L"EmuController"  
#define HID_DEVICE_SERIAL_NUMBER_STRING L"23908912390812"  
#define HID_DEVICE_STRING_INDEX 5


typedef struct _NAMED_PIPE_SERVER_ATTRIBUTES 
{
	WCHAR* InputPipePathName[48];
	WCHAR* PidPipePathName[48];
	HANDLE InputServerHandle;
	HANDLE PidServerHandle;
	HANDLE InputPipeHandle;
	HANDLE PidPipeHandle;
	BOOL InputPipeConnected;
	PSECURITY_ATTRIBUTES PipeSecurityAttr;
} NAMED_PIPE_SERVER_ATTRIBUTES, * PNAMED_PIPE_SERVER_ATTRIBUTES;


//
// The device context performs the same job as
// a WDM device extension in the driver frameworks
//


typedef struct _DEVICE_CONTEXT
{
	WDFDEVICE Device;
	WDFQUEUE DefaultQueue;
	WDFQUEUE ManualQueue;
	HID_DESCRIPTOR HidDescriptor;
	PHID_REPORT_DESCRIPTOR ReportDescriptor;
	HID_DEVICE_ATTRIBUTES HidDeviceAttributes;
	HID_XFER_PACKET ReportPacket;
	JOYSTICK_INPUT_REPORT JoyInputReport;
	PID_STATE_REPORT JoyPidStateReport;
	UCHAR EffectBlockIndex;
	NAMED_PIPE_SERVER_ATTRIBUTES PipeServerAttributes;
} DEVICE_CONTEXT, * PDEVICE_CONTEXT;

//
// This macro will generate an inline function called DeviceGetContext
// which will be used to get a pointer to the device context memory
// in a type safe manner.
//
WDF_DECLARE_CONTEXT_TYPE_WITH_NAME(DEVICE_CONTEXT, DeviceGetContext)

//
// Function to initialize the device and its callbacks
//
NTSTATUS
EmuControllerCreateDevice(
	_Inout_ PWDFDEVICE_INIT DeviceInit
);

//
// Function to retreive ProductId value from registry (under HKR hardware key)
// 

NTSTATUS
GetProductIdFromRegistry(
	_In_ WDFDEVICE Device,
	_Out_ PULONG ProductId);

EXTERN_C_END