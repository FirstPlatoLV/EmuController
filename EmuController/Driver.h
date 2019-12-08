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
