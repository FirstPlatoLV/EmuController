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

    driver.h

Abstract:

    This file contains the driver definitions.

Environment:

    User-mode Driver Framework 2

--*/

#include <windows.h>
#include <wdf.h>
#include <stdio.h>
#include <initguid.h>
#include <hidport.h>
#include <sddl.h>
#include <Cfgmgr32.h>

#include "device.h"
#include "queue.h"
#include "ioctl.h"
#include "namedpipeserver.h"
#include "messageprocessor.h"
#include "trace.h"

EXTERN_C_START

//
// WDFDRIVER Events
//

DRIVER_INITIALIZE DriverEntry;
EVT_WDF_DRIVER_DEVICE_ADD EmuControllerEvtDeviceAdd;
EVT_WDF_OBJECT_CONTEXT_CLEANUP EmuControllerEvtDriverContextCleanup;

EXTERN_C_END
