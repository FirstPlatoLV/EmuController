/*++

Module Name:

	ioctl.c

Abstract:

	This file contains the ioctl entry points and callbacks.

Environment:

	User-mode Driver Framework 2

--*/

#include "driver.h"
#include "ioctl.tmh"


NTSTATUS
RequestCopyFromBuffer(
	_In_  WDFREQUEST        Request,
	_In_  PVOID             SourceBuffer,
	_When_(NumBytesToCopyFrom == 0, __drv_reportError(NumBytesToCopyFrom cannot be zero))
	_In_  size_t            NumBytesToCopyFrom
)
/*++
Routine Description:
	A helper function to copy specified bytes to the request's output memory
Arguments:
	Request - A handle to a framework request object.
	SourceBuffer - The buffer to copy data from.
	NumBytesToCopyFrom - The length, in bytes, of data to be copied.
Return Value:
	NTSTATUS
--*/
{
	NTSTATUS                status;
	WDFMEMORY               memory;
	size_t                  outputBufferLength;

	status = WdfRequestRetrieveOutputMemory(Request, &memory);
	if (!NT_SUCCESS(status)) {
		TraceEvents(TRACE_LEVEL_ERROR, TRACE_IOCTL, 
	    "WdfRequestRetrieveOutputMemory failed %!STATUS!", 
			status);
		return status;
	}

	WdfMemoryGetBuffer(memory, &outputBufferLength);
	if (outputBufferLength < NumBytesToCopyFrom) {
		status = STATUS_INVALID_BUFFER_SIZE;

		TraceEvents(TRACE_LEVEL_ERROR, TRACE_IOCTL, 
			"RequestCopyFromBuffer: buffer too small. Size %d, expect %d\n",
			(int)outputBufferLength, 
			(int)NumBytesToCopyFrom);
		return status;
	}

	status = WdfMemoryCopyFromBuffer(memory,
		0,
		SourceBuffer,
		NumBytesToCopyFrom);
	if (!NT_SUCCESS(status)) {
		TraceEvents(TRACE_LEVEL_ERROR, TRACE_IOCTL, 
		"WdfMemoryCopyFromBuffer failed %!STATUS!", status);
		return status;
	}

	WdfRequestSetInformation(Request, NumBytesToCopyFrom);
	return status;
}

NTSTATUS
ReadReport(
	_In_  PQUEUE_CONTEXT    QueueContext,
	_In_  WDFREQUEST        Request,
	_Always_(_Out_)
	BOOLEAN* CompleteRequest
)
/*++
Routine Description:
	Handles IOCTL_HID_READ_REPORT for the HID collection. Normally the request
	will be forwarded to a manual queue for further process. In that case, the
	caller should not try to complete the request at this time, as the request
	will later be retrieved back from the manually queue and completed there.
	However, if for some reason the forwarding fails, the caller still need
	to complete the request with proper error code immediately.
Arguments:
	QueueContext - The object context associated with the queue
	Request - Pointer to  Request Packet.
	CompleteRequest - A boolean output value, indicating whether the caller
			should complete the request or not
Return Value:
	NT status code.
--*/
{
	NTSTATUS                status;
	PVOID					pOutputBuffer = NULL;
	size_t					outputBufferLength;

//	TraceEvents(TRACE_LEVEL_INFORMATION, TRACE_IOCTL, "%!FUNC! Entry");


	status = WdfRequestRetrieveOutputBuffer(
		Request,
		sizeof(UCHAR),
		&pOutputBuffer,
		&outputBufferLength
	);


	if (!NT_SUCCESS(status)) {
		TraceEvents(TRACE_LEVEL_ERROR, TRACE_IOCTL,
			"WdfRequestRetrieveOutputBuffer failed with status %!STATUS!",
			status
		);

		*CompleteRequest = TRUE;
		return status;
	}

	//
	// forward the request to manual queue
	//

	status = WdfRequestForwardToIoQueue(
		Request,
		QueueContext->DeviceContext->ManualQueue);
	if (!NT_SUCCESS(status)) {
		TraceEvents(TRACE_LEVEL_ERROR, TRACE_IOCTL, 
		"WdfRequestForwardToIoQueue failed with %!STATUS!", 
			status);
		*CompleteRequest = TRUE;

	}
	else {
		*CompleteRequest = FALSE;
	}

	return status;
}

NTSTATUS
WriteReport(
	_In_  PQUEUE_CONTEXT    QueueContext,
	_In_  WDFREQUEST        Request
)
/*++
Routine Description:
	Handles IOCTL_HID_WRITE_REPORT all the collection.
Arguments:
	QueueContext - The object context associated with the queue
	Request - Pointer to  Request Packet.
Return Value:
	NT status code.
--*/

{
	NTSTATUS                status;
	HID_XFER_PACKET         packet;
	ULONG                   reportSize;

	status = RequestGetHidXferPacket_ToWriteToDevice(
		Request,
		&packet);
	if (!NT_SUCCESS(status)) {
		TraceEvents(TRACE_LEVEL_ERROR, TRACE_IOCTL, 
			"RequestGetHidXferPacket_ToWriteToDevice failed %!STATUS!", 
			status);
		return status;
	}

	reportSize = packet.reportBufferLen;

	//
	// before touching buffer make sure buffer is big enough.
	//

	if (packet.reportBufferLen < sizeof(UCHAR)) {
		status = STATUS_BUFFER_TOO_SMALL;
		TraceEvents(TRACE_LEVEL_ERROR, TRACE_IOCTL, 
			"WriteReport: input buffer too small!");
		return status;
	}

	if (packet.reportId == PID_DEVICE_CONTROL_REPORT_ID)
	{
		PPID_DEVICE_CONTROL_REPORT pDc = (PPID_DEVICE_CONTROL_REPORT)packet.reportBuffer;

		if (pDc->DeviceControlCommand == PID_DEVICE_RESET_CMD)
		{
			QueueContext->DeviceContext->EffectBlockIndex = 0;
		}
	}

	//
	// Store the device data in device extension.
	//

	QueueContext->DeviceContext->ReportPacket = packet;

	WriteResponseToPidClient(QueueContext);
	//
	// set status and information
	//
	WdfRequestSetInformation(Request, reportSize);
	return status;
}

NTSTATUS
RequestGetHidXferPacket_ToReadFromDevice(
	_In_  WDFREQUEST        Request,
	_Out_ HID_XFER_PACKET* Packet
)
{
	//
	// Driver need to write to the output buffer (so that App can read from it)
	//
	//   Report Buffer: Output Buffer
	//   Report Id    : Input Buffer
	//

	NTSTATUS                status;
	WDFMEMORY               inputMemory;
	WDFMEMORY               outputMemory;
	size_t                  inputBufferLength;
	size_t                  outputBufferLength;
	PVOID                   inputBuffer;
	PVOID                   outputBuffer;

	//TraceEvents(TRACE_LEVEL_INFORMATION, TRACE_IOCTL, "%!FUNC! Entry");
	//
	// Get report Id from input buffer
	//
	status = WdfRequestRetrieveInputMemory(Request, &inputMemory);
	if (!NT_SUCCESS(status)) {
		TraceEvents(TRACE_LEVEL_ERROR, TRACE_IOCTL, 
			"WdfRequestRetrieveInputMemory failed %!STATUS!", 
			status);
		return status;
	}
	inputBuffer = WdfMemoryGetBuffer(inputMemory, &inputBufferLength);

	if (inputBufferLength < sizeof(UCHAR)) {
		status = STATUS_INVALID_BUFFER_SIZE;
		TraceEvents(TRACE_LEVEL_ERROR, TRACE_IOCTL, 
			"WdfRequestRetrieveInputMemory: input buffer. size %d, expect %d\n",
			(int)inputBufferLength, 
			(int)sizeof(UCHAR));
		return status;
	}

	Packet->reportId = *(PUCHAR)inputBuffer;

	//
	// Get report buffer from output buffer
	//
	status = WdfRequestRetrieveOutputMemory(Request, &outputMemory);
	if (!NT_SUCCESS(status)) {
		TraceEvents(TRACE_LEVEL_ERROR, TRACE_IOCTL, 
			"WdfRequestRetrieveOutputMemory failed %!STATUS!", 
			status);
		return status;
	}

	outputBuffer = WdfMemoryGetBuffer(outputMemory, &outputBufferLength);

	Packet->reportBuffer = (PUCHAR)outputBuffer;
	Packet->reportBufferLen = (ULONG)outputBufferLength;

	return status;
}

NTSTATUS
RequestGetHidXferPacket_ToWriteToDevice(
	_In_  WDFREQUEST        Request,
	_Out_ HID_XFER_PACKET* Packet
)
{
	//TraceEvents(TRACE_LEVEL_INFORMATION, TRACE_IOCTL, "%!FUNC! Entry");
	//
	// Driver need to read from the input buffer (which was written by App)
	//
	//   Report Buffer: Input Buffer
	//   Report Id    : Output Buffer Length
	//
	// Note that the report id is not stored inside the output buffer, as the
	// driver has no read-access right to the output buffer, and trying to read
	// from the buffer will cause an access violation error.
	//
	// The workaround is to store the report id in the OutputBufferLength field,
	// to which the driver does have read-access right.
	//

	NTSTATUS                status;
	WDFMEMORY               inputMemory;
	WDFMEMORY               outputMemory;
	size_t                  inputBufferLength;
	size_t                  outputBufferLength;
	PVOID                   inputBuffer;

	//
	// Get report Id from output buffer length
	//
	status = WdfRequestRetrieveOutputMemory(Request, &outputMemory);
	if (!NT_SUCCESS(status)) {
		TraceEvents(TRACE_LEVEL_ERROR, TRACE_IOCTL, 
			"WdfRequestRetrieveOutputMemory failed %!STATUS!",
			status);
		return status;
	}
	WdfMemoryGetBuffer(outputMemory, &outputBufferLength);
	Packet->reportId = (UCHAR)outputBufferLength;

	//
	// Get report buffer from input buffer
	//
	status = WdfRequestRetrieveInputMemory(Request, &inputMemory);
	if (!NT_SUCCESS(status)) {
		TraceEvents(TRACE_LEVEL_ERROR, TRACE_IOCTL, 
			"WdfRequestRetrieveInputMemory failed %!STATUS!", 
			status);
		return status;
	}
	inputBuffer = WdfMemoryGetBuffer(inputMemory, &inputBufferLength);

	Packet->reportBuffer = (PUCHAR)inputBuffer;
	Packet->reportBufferLen = (ULONG)inputBufferLength;

	return status;
}

NTSTATUS
GetFeature(
	_In_  PQUEUE_CONTEXT    QueueContext,
	_In_  WDFREQUEST        Request
)
/*++
Routine Description:
	Handles IOCTL_HID_GET_FEATURE for all the collection.
Arguments:
	QueueContext - The object context associated with the queue
	Request - Pointer to  Request Packet.
Return Value:
	NT status code.
--*/
{
	NTSTATUS                status;
	HID_XFER_PACKET         packet;
	ULONG                   reportSize;


	status = RequestGetHidXferPacket_ToReadFromDevice(
		Request,
		&packet);
	if (!NT_SUCCESS(status)) {
		TraceEvents(TRACE_LEVEL_ERROR, TRACE_IOCTL, 
			"RequestGetHidXferPacket_ToReadFromDevice failed %!STATUS!", 
			status);
		return status;
	}	

	//
	// Since output buffer is for write only (no read allowed by UMDF in output
	// buffer), any read from output buffer would be reading garbage), so don't
	// let app embed custom control code in output buffer. The minidriver can
	// support multiple features using separate report ID instead of using
	// custom control code. Since this is targeted at report ID 1, we know it
	// is a request for getting attributes.
	//
	// While KMDF does not enforce the rule (disallow read from output buffer),
	// it is good practice to not do so.
	//

	reportSize = packet.reportBufferLen;


	if (packet.reportBufferLen < sizeof(UCHAR)) {
		status = STATUS_BUFFER_TOO_SMALL;
		TraceEvents(TRACE_LEVEL_ERROR, TRACE_IOCTL, 
			"GetFeature: input buffer too small!");
		return status;
	}

	switch (packet.reportId)
	{
	case PID_BLOCK_LOAD_REPORT_ID:
	{
		PPID_BLOCK_LOAD_REPORT pidBlockLoadReport = (PPID_BLOCK_LOAD_REPORT)packet.reportBuffer;

		pidBlockLoadReport->RamPoolAvailable = 65535;

		if (QueueContext->DeviceContext->EffectBlockIndex > MAX_EFFECT_BLOCKS)
		{
			pidBlockLoadReport->BlockLoadStatus = 2;
		}
		else
		{
			pidBlockLoadReport->BlockLoadStatus = 1;
			pidBlockLoadReport->EffectBlockIndex = QueueContext->DeviceContext->EffectBlockIndex;
		}

		pidBlockLoadReport->ReportId = PID_BLOCK_LOAD_REPORT_ID;

	
		QueueContext->DeviceContext->ReportPacket = packet;

		WriteResponseToPidClient(QueueContext);
		break;
	}

	case PID_POOL_REPORT_ID:
	{
		PPID_POOL_REPORT pidPoolReport = (PPID_POOL_REPORT)packet.reportBuffer;
		pidPoolReport->ReportId = PID_POOL_REPORT_ID;
		pidPoolReport->DeviceManagedPool = 1;
		pidPoolReport->RamPoolSize = 65535;
		pidPoolReport->SharedParameterBlocks = 0;
		pidPoolReport->SimultaneousEffectsMax = MAX_EFFECT_BLOCKS;

		break;
	}
	case PID_BLOCK_FREE_REPORT_ID:
	{
		QueueContext->DeviceContext->ReportPacket = packet;
		WriteResponseToPidClient(QueueContext);
		break;
	}
	default:
	{
		status = STATUS_NOT_IMPLEMENTED;
		TraceEvents(TRACE_LEVEL_ERROR, TRACE_IOCTL, 
			"Unknown ReportId: %d", 
			packet.reportId);
		reportSize = 0;
	}
	}

	//
	// Report how many bytes were copied
	//
	QueueContext->DeviceContext->ReportPacket = packet;
	WdfRequestSetInformation(Request, reportSize);
	return status;
}

NTSTATUS
SetFeature(
	_In_  PQUEUE_CONTEXT    QueueContext,
	_In_  WDFREQUEST        Request
)
/*++
Routine Description:
	Handles IOCTL_HID_SET_FEATURE for all the collection.
	For control collection (custom defined collection) it handles
	the user-defined control codes for sideband communication
Arguments:
	QueueContext - The object context associated with the queue
	Request - Pointer to Request Packet.
Return Value:
	NT status code.
--*/
{
	NTSTATUS                status;
	HID_XFER_PACKET         packet;
	ULONG                   reportSize;


	status = RequestGetHidXferPacket_ToWriteToDevice(
		Request,
		&packet);
	if (!NT_SUCCESS(status)) {
		TraceEvents(TRACE_LEVEL_ERROR, TRACE_IOCTL, 
			"RequestGetHidXferPacket_ToWriteToDevice failed %!STATUS!", 
			status);
		return status;
	}

	//
	// before touching control code make sure buffer is big enough.
	//
	reportSize = packet.reportBufferLen;

	if (packet.reportBufferLen < sizeof(UCHAR)) {
		status = STATUS_BUFFER_TOO_SMALL;
		TraceEvents(TRACE_LEVEL_ERROR, TRACE_IOCTL, 
			"SetFeature: input buffer too small!");
		return status;
	}

	if (packet.reportId == PID_NEW_EFFECT_REPORT_ID)
	{
		QueueContext->DeviceContext->EffectBlockIndex++;
	}


	QueueContext->DeviceContext->ReportPacket = packet;

	WriteResponseToPidClient(QueueContext);

	return status;
}

NTSTATUS
GetInputReport(
	_In_  PQUEUE_CONTEXT    QueueContext,
	_In_  WDFREQUEST        Request
)
/*++
Routine Description:
	Handles IOCTL_HID_GET_INPUT_REPORT for all the collection.
Arguments:
	QueueContext - The object context associated with the queue
	Request - Pointer to Request Packet.
Return Value:
	NT status code.
--*/
{
	NTSTATUS                status;
	HID_XFER_PACKET         packet;
	ULONG                   reportSize;

	UNREFERENCED_PARAMETER(QueueContext);
	TraceEvents(TRACE_LEVEL_INFORMATION, TRACE_IOCTL, "%!FUNC! Entry");

	status = RequestGetHidXferPacket_ToReadFromDevice(
		Request,
		&packet);
	if (!NT_SUCCESS(status)) {
		TraceEvents(TRACE_LEVEL_ERROR, TRACE_IOCTL, 
			"RequestGetHidXferPacket_ToReadFromDevice failed %!STATUS!", 
			status);
		return status;
	}

	reportSize = packet.reportBufferLen;

	if (packet.reportBufferLen < sizeof(UCHAR)) {
		status = STATUS_BUFFER_TOO_SMALL;
		TraceEvents(TRACE_LEVEL_ERROR, TRACE_IOCTL, 
			"GetInputReport: input buffer too small!");
		return status;
	}

	//
	// Report how many bytes were copied
	//
	WdfRequestSetInformation(Request, reportSize);
	return status;
}

NTSTATUS
SetOutputReport(
	_In_  PQUEUE_CONTEXT    QueueContext,
	_In_  WDFREQUEST        Request
)
/*++
Routine Description:
	Handles IOCTL_HID_SET_OUTPUT_REPORT for all the collection.
Arguments:
	QueueContext - The object context associated with the queue
	Request - Pointer to Request Packet.
Return Value:
	NT status code.
--*/
{
	NTSTATUS                status;
	HID_XFER_PACKET         packet;
	ULONG                   reportSize;

	status = RequestGetHidXferPacket_ToWriteToDevice(
		Request,
		&packet);
	if (!NT_SUCCESS(status)) {
		TraceEvents(TRACE_LEVEL_ERROR, TRACE_IOCTL, 
			"RequestGetHidXferPacket_ToWriteToDevice failed %!STATUS!", 
			status);
		return status;
	}

	//
	// before touching buffer make sure buffer is big enough.
	//
	reportSize = packet.reportBufferLen;

	if (packet.reportBufferLen < sizeof(UCHAR)) {
		status = STATUS_BUFFER_TOO_SMALL;

		TraceEvents(TRACE_LEVEL_ERROR, TRACE_IOCTL, 
			"GetInputReport: input buffer too small!");
		return status;
	}

	TraceEvents(TRACE_LEVEL_INFORMATION, TRACE_IOCTL,
		"ReportId: %d, ReportBufferLength: %d\n",
		packet.reportId,
		reportSize);


	QueueContext->DeviceContext->ReportPacket = packet;

	//
	// Report how many bytes were copied
	//
	WdfRequestSetInformation(Request, reportSize);
	return status;
}

NTSTATUS
GetStringId(
	_In_  WDFREQUEST        Request,
	_Out_ ULONG* StringId,
	_Out_ ULONG* LanguageId
)
/*++
Routine Description:
	Helper routine to decode IOCTL_HID_GET_INDEXED_STRING and IOCTL_HID_GET_STRING.
Arguments:
	Request - Pointer to Request Packet.
Return Value:
	NT status code.
--*/
{
	NTSTATUS                status;
	ULONG                   inputValue;
	WDFMEMORY               inputMemory;
	size_t                  inputBufferLength;
	PVOID                   inputBuffer;

	//
	// mshidumdf.sys updates the IRP and passes the string id (or index) through
	// the input buffer correctly based on the IOCTL buffer type
	//

	status = WdfRequestRetrieveInputMemory(Request, &inputMemory);
	if (!NT_SUCCESS(status)) {
		TraceEvents(TRACE_LEVEL_ERROR, TRACE_IOCTL, 
			"WdfRequestRetrieveInputMemory %!STATUS!", 
			status);
		return status;
	}
	inputBuffer = WdfMemoryGetBuffer(inputMemory, &inputBufferLength);

	//
	// make sure buffer is big enough.
	//
	if (inputBufferLength < sizeof(ULONG))
	{
		status = STATUS_INVALID_BUFFER_SIZE;
		TraceEvents(TRACE_LEVEL_ERROR, TRACE_IOCTL, 
			"GetStringId: invalid input buffer. size %d, expect %d\n",
			(int)inputBufferLength, 
			(int)sizeof(ULONG));
		return status;
	}

	inputValue = (*(PULONG)inputBuffer);

	//
	// The least significant two bytes of the INT value contain the string id.
	//
	* StringId = (inputValue & 0x0ffff);

	//
	// The most significant two bytes of the INT value contain the language
	// ID (for example, a value of 1033 indicates English).
	//
	*LanguageId = (inputValue >> 16);

	return status;
}


NTSTATUS
GetIndexedString(
	_In_  WDFREQUEST        Request
)
/*++
Routine Description:
	Handles IOCTL_HID_GET_INDEXED_STRING
Arguments:
	Request - Pointer to Request Packet.
Return Value:
	NT status code.
--*/
{
	NTSTATUS                status;
	ULONG                   languageId, stringIndex;


	status = GetStringId(Request, &stringIndex, &languageId);

	// While we don't use the language id, some minidrivers might.
	//
	UNREFERENCED_PARAMETER(languageId);

	if (NT_SUCCESS(status)) {

		if (stringIndex != HID_DEVICE_STRING_INDEX)
		{
			status = STATUS_INVALID_PARAMETER;
			TraceEvents(TRACE_LEVEL_ERROR, TRACE_IOCTL, 
				"GetString: unknown string index %d\n", 
				stringIndex);
			return status;
		}

		status = RequestCopyFromBuffer(Request, HID_DEVICE_STRING, sizeof(HID_DEVICE_STRING));
	}
	return status;
}

NTSTATUS
GetString(
	_In_  WDFREQUEST        Request
)
/*++
Routine Description:
	Handles IOCTL_HID_GET_STRING.
Arguments:
	Request - Pointer to Request Packet.
Return Value:
	NT status code.
--*/
{
	NTSTATUS                status;
	ULONG                   languageId, stringId;
	size_t                  stringSizeCb;
	PWSTR                   string;

	status = GetStringId(Request, &stringId, &languageId);

	// While we don't use the language id, some minidrivers might.
	//
	UNREFERENCED_PARAMETER(languageId);

	if (!NT_SUCCESS(status)) {
		return status;
	}

	switch (stringId) {
	case HID_STRING_ID_IMANUFACTURER:
		stringSizeCb = sizeof(HID_DEVICE_MANUFACTURER_STRING);
		string = HID_DEVICE_MANUFACTURER_STRING;
		break;
	case HID_STRING_ID_IPRODUCT:
		stringSizeCb = sizeof(HID_DEVICE_PRODUCT_STRING);
		string = HID_DEVICE_PRODUCT_STRING;
		break;
	case HID_STRING_ID_ISERIALNUMBER:
		stringSizeCb = sizeof(HID_DEVICE_SERIAL_NUMBER_STRING);
		string = HID_DEVICE_SERIAL_NUMBER_STRING;
		break;
	default:
		status = STATUS_INVALID_PARAMETER;
		TraceEvents(TRACE_LEVEL_ERROR, TRACE_IOCTL, 
			"GetString: unkown string id %d\n", 
			stringId);
		return status;
	}

	status = RequestCopyFromBuffer(Request, string, stringSizeCb);
	return status;
}



