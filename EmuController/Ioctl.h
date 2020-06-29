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

	ioctl.h

Abstract:

	This file contains the ioctl definitions.

Environment:

	User-mode Driver Framework 2

--*/

EXTERN_C_START

NTSTATUS
ReadReport(
	_In_  PQUEUE_CONTEXT    QueueContext,
	_In_  WDFREQUEST        Request,
	_Always_(_Out_)
	BOOLEAN* CompleteRequest
);

NTSTATUS
WriteReport(
	_In_  PQUEUE_CONTEXT    QueueContext,
	_In_  WDFREQUEST        Request
);

NTSTATUS
GetFeature(
	_In_  PQUEUE_CONTEXT    QueueContext,
	_In_  WDFREQUEST        Request
);

NTSTATUS
SetFeature(
	_In_  PQUEUE_CONTEXT    QueueContext,
	_In_  WDFREQUEST        Request
);

NTSTATUS
GetInputReport(
	_In_  PQUEUE_CONTEXT    QueueContext,
	_In_  WDFREQUEST        Request
);

NTSTATUS
SetOutputReport(
	_In_  PQUEUE_CONTEXT    QueueContext,
	_In_  WDFREQUEST        Request
);

NTSTATUS
GetString(
	_In_  WDFREQUEST        Request
);

NTSTATUS
GetIndexedString(
	_In_  WDFREQUEST        Request
);

NTSTATUS
GetStringId(
	_In_  WDFREQUEST        Request,
	_Out_ ULONG* StringId,
	_Out_ ULONG* LanguageId
);

NTSTATUS
RequestCopyFromBuffer(
	_In_  WDFREQUEST        Request,
	_In_  PVOID             SourceBuffer,
	_When_(NumBytesToCopyFrom == 0, __drv_reportError(NumBytesToCopyFrom cannot be zero))
	_In_  size_t            NumBytesToCopyFrom
);

NTSTATUS
RequestGetHidXferPacket_ToReadFromDevice(
	_In_  WDFREQUEST        Request,
	_Out_ HID_XFER_PACKET* Packet
);

NTSTATUS
RequestGetHidXferPacket_ToWriteToDevice(
	_In_  WDFREQUEST        Request,
	_Out_ HID_XFER_PACKET* Packet
);

EXTERN_C_END
