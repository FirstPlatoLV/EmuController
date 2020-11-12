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
#include "driver.h"
#include "namedpipeserver.tmh"


DWORD
CreateNamedPipeServer(
	LPVOID Params
)
{
	LPWSTR inputPipeName = NULL;
	LPWSTR pidPipeName = NULL;
	size_t bufferCount = 48;

	LPCWSTR PszLocalPipeTxt = L"\\\\.\\pipe\\";
	LPCWSTR PszPipeIdFormat = L"%sVID_%X&PID_%X_%s";
	LPCWSTR PszInputPipeSuffix = L"input";
	LPCWSTR PszPidPipeSuffix = L"ffb";


	PDEVICE_CONTEXT     devContext = DeviceGetContext(Params);


	PSECURITY_ATTRIBUTES pSa = NULL;

	// Prepare the security attributes (the lpSecurityAttributes parameter in 
	// CreateNamedPipe) for the pipe. This is optional. If the 
	// lpSecurityAttributes parameter of CreateNamedPipe is NULL, the named 
	// pipe gets a default security descriptor and the handle cannot be 
	// inherited. The ACLs in the default security descriptor of a pipe grant 
	// full control to the LocalSystem account, (elevated) administrators, 
	// and the creator owner. They also give only read access to members of 
	// the Everyone group and the anonymous account. However, if you want to 
	// customize the security permission of the pipe, (e.g. to allow 
	// Authenticated Users to read from and write to the pipe), you need to 
	// create a SECURITY_ATTRIBUTES structure.

	if (!CreatePipeSecurity(&pSa))
	{
		TraceEvents(TRACE_LEVEL_ERROR, TRACE_PIPE,
			"CreatePipeSecurity failed with %d\n", GetLastError()
		);
		FreePipeSecurity(pSa);
		pSa = NULL;
		return 1;
	}


	devContext->PipeServerAttributes.PipeSecurityAttr = pSa;

	inputPipeName = (LPWSTR)devContext->PipeServerAttributes.InputPipePathName;
	pidPipeName = (LPWSTR)devContext->PipeServerAttributes.PidPipePathName;
	
	// The pipe name is generated using VendorId and ProductId values, 
	// latter of which is specified in the .inf for each EmuController device,
	int res = swprintf_s(inputPipeName, 
		bufferCount, 
		PszPipeIdFormat, 
		PszLocalPipeTxt, 
		devContext->HidDeviceAttributes.VendorID, 
		devContext->HidDeviceAttributes.ProductID, 
		PszInputPipeSuffix);
	
	if (res == 0 || inputPipeName == NULL)
	{
		TraceEvents(TRACE_LEVEL_ERROR, TRACE_PIPE,
			"Failed generating input pipe name."
		);

		return 1;
	}
	
	res = swprintf_s(pidPipeName, 
		bufferCount, 
		PszPipeIdFormat, 
		PszLocalPipeTxt, 
		devContext->HidDeviceAttributes.VendorID, 
		devContext->HidDeviceAttributes.ProductID, 
		PszPidPipeSuffix);
	
	
	if (res == 0 || pidPipeName == NULL)
	{
		TraceEvents(TRACE_LEVEL_ERROR, TRACE_PIPE,
			"Failed generating ffb pipe name."
		);

		return 1;
	}

	devContext->PipeServerAttributes.InputPipeHandle = CreateNamedPipe(
		(LPCWSTR)devContext->PipeServerAttributes.InputPipePathName,    // Pipe name 
		PIPE_ACCESS_INBOUND, // Server will be receiving messages only
		PIPE_TYPE_MESSAGE | 
		PIPE_READMODE_MESSAGE | // Data is read from the pipe as a stream of messages.
		PIPE_WAIT, // blocking mode 
		INPUT_INSTANCES,
		BUFFER_SIZE,  // Output buffer size 
		BUFFER_SIZE,  // Input buffer size 
		0,   // Client time-out 
		devContext->PipeServerAttributes.PipeSecurityAttr);

	if (devContext->PipeServerAttributes.InputPipeHandle == INVALID_HANDLE_VALUE)
	{
		TraceEvents(TRACE_LEVEL_ERROR, TRACE_PIPE,
			"CreateNamedPipe failed with %d\n", GetLastError()
		);

		return 1;
	}

	// Create a thread for handling input pipe server.
	devContext->PipeServerAttributes.InputServerHandle = CreateThread(
		NULL,
		0,
		InputPipeServerThread,
		Params,
		0,
		NULL
	);

	// Create a thread for handling PID pipe server related data.
	devContext->PipeServerAttributes.PidServerHandle = CreateThread(
		NULL,
		0,
		PidPipeServerThread,
		Params,
		0,
		NULL
	);

	return 0;
}


DWORD WINAPI InputPipeServerThread(
	LPVOID Params)
{
	/* Params = WDFDEVICE */
	PDEVICE_CONTEXT     devContext = DeviceGetContext(Params);
	DWORD bytesRead;
	UCHAR buffer[BUFFER_SIZE];
	PQUEUE_CONTEXT   queueContext;

	// Processes named pipe server operations synchronously. 
	// All calls are blocking, but this loop runs in a separate thread.

	while (1)

	{

		TraceEvents(TRACE_LEVEL_INFORMATION, TRACE_PIPE,
			"Waiting for input client to connect...");


		// Will return only when a client attempts connection.
		if (!ConnectNamedPipe(devContext->PipeServerAttributes.InputPipeHandle, NULL))
		{
			TraceEvents(TRACE_LEVEL_ERROR, TRACE_PIPE,
				"ConnectNamedPipe failed with %d\n", GetLastError()
			);
			return 0;

		}

		TraceEvents(TRACE_LEVEL_INFORMATION, TRACE_PIPE,
			"Input client connected to %ws\n", 
			(LPCWSTR)devContext->PipeServerAttributes.InputPipePathName
		);

		queueContext = QueueGetContext(devContext->ManualQueue);
		devContext->PipeServerAttributes.InputPipeConnected = TRUE;

		// Initializing JoyInputReport with default values.
		SetDefaultControllerState(&queueContext->DeviceContext->JoyInputReport);

		// This loop will continue as long as client is connected and messages it sends are valid.
		// The ReadFile will return false as soon as client drops, 
		// even if it doesn't close the pipe handle properly.
		
		while (ReadFile(devContext->PipeServerAttributes.InputPipeHandle,
			buffer, 
			BUFFER_SIZE, 
			&bytesRead, 
			NULL))
		{
			switch (buffer[0])
			{
			// Contains partial updates for JoystickInputReport that need to be processed first.
			// This is useful for event based updates from client, 
		    // which contains only controls that have changed,
			// consequently, anything that is not updated explicitly 
		    // will remain the same as it was in previous update.
			case JOY_INPUT_REPORT_PARTIAL:
			{
				DeserializeJoyInput(buffer, &queueContext->DeviceContext->JoyInputReport);
				CompleteReadRequest(devContext, buffer[0]);
				break;
			}
			// Contains the full JOYSTICK_INPUT_REPORT as array of bytes.
			// Client will likely send this report when updates 
			// are polled continously at the set interval
			// instead of waiting for update event to trigger.
			case JOY_INPUT_REPORT_FULL:
			{
				if (buffer[1] != sizeof(JOYSTICK_INPUT_REPORT))
				{
					TraceEvents(TRACE_LEVEL_ERROR, TRACE_PIPE,
						"JOYSTICK_INPUT_REPORT has incorrect size. Expected: %d, Received: %d\n", 
						sizeof(JOYSTICK_INPUT_REPORT), 
						buffer[1]
					);
					break;

				}

				RtlCopyMemory(&queueContext->DeviceContext->JoyInputReport, 
					&buffer[MESSAGE_HEADER_LEN], 
					sizeof(JOYSTICK_INPUT_REPORT));

				CompleteReadRequest(devContext, buffer[0]);
				break;
			}
			// Contains the full PID_STATE_REPORT as array of bytes. 
			// There appears to be no need to send this report 
			// due to the EmuController being a virtual device.
			case JOY_INPUT_PID_STATE_REPORT:
			{
				if (buffer[1] != sizeof(PID_STATE_REPORT))
				{
					TraceEvents(TRACE_LEVEL_ERROR, TRACE_PIPE,
						"PID_STATE_REPORT has incorrect size. Expected: %d, Received: %d\n", 
						sizeof(PID_STATE_REPORT), 
						buffer[1]
					);
					break;

				}

				RtlCopyMemory(&queueContext->DeviceContext->JoyPidStateReport, 
					&buffer[MESSAGE_HEADER_LEN], 
					sizeof(PID_STATE_REPORT));

				CompleteReadRequest(devContext, buffer[0]);
				break;
			}
			default:
			{
				TraceEvents(TRACE_LEVEL_ERROR, TRACE_PIPE,
					"Unknown ReportId: %d\n", buffer[0]
				);
				break;
			}
			}
		}

		// Either a read error occured or client disconnected / dropped.
		if (!DisconnectNamedPipe(devContext->PipeServerAttributes.InputPipeHandle))
		{

			TraceEvents(TRACE_LEVEL_ERROR, TRACE_PIPE,
				"DisconnectNamedPipe failed with %d\n", GetLastError()
			);
			CloseHandle(devContext->PipeServerAttributes.InputPipeHandle);
			return 0;

		}

		devContext->PipeServerAttributes.InputPipeConnected = FALSE;

		TraceEvents(TRACE_LEVEL_INFORMATION, TRACE_PIPE,
			"Input client disconnected from %ws\n", 
			(LPCWSTR)devContext->PipeServerAttributes.InputPipePathName
		);

		// Restore the default state of the device, to prevent unexpected behaviour 
		// in applications that access the device.
		SetDefaultControllerState(&queueContext->DeviceContext->JoyInputReport);
		CompleteReadRequest(devContext, JOY_INPUT_REPORT_FULL);
	}

	CloseHandle(devContext->PipeServerAttributes.InputPipeHandle);
	return 0;

}

DWORD WINAPI PidPipeServerThread(
	LPVOID Params)
{
	/* Params = WDFDEVICE */
	PDEVICE_CONTEXT     devContext = DeviceGetContext(Params);

	TraceEvents(TRACE_LEVEL_INFORMATION, TRACE_PIPE,
		"Waiting for FFB client to connect...\n");

	devContext->PipeServerAttributes.PidPipeHandle = NULL;

	while (1)
	{

		// Create a named pipe server instance. While max instances is set to
		// PIPE_UNLIMITED_INSTANCES, there will never ever be more than two instances.

		HANDLE hPipe = CreateNamedPipe(
			(LPCWSTR)devContext->PipeServerAttributes.PidPipePathName,             // pipe name 
			PIPE_ACCESS_DUPLEX,       // read/write access 
			PIPE_TYPE_MESSAGE |       // message type pipe 
			PIPE_READMODE_MESSAGE |   // message-read mode 
			PIPE_WAIT,                // blocking mode 
			PIPE_UNLIMITED_INSTANCES,  // max. instances  
			BUFFER_SIZE,              // output buffer size 
			BUFFER_SIZE,              // input buffer size 
			0,                        // client time-out 
			devContext->PipeServerAttributes.PipeSecurityAttr);



		if (hPipe == INVALID_HANDLE_VALUE)
		{
			TraceEvents(TRACE_LEVEL_ERROR, TRACE_PIPE,
				"CreateNamedPipe failed with %d\n", GetLastError()
			);

			return 0;
		}


		// Wait for the client to connect; if it succeeds, 
		// the function returns a nonzero value. If the function
		// returns zero, GetLastError returns ERROR_PIPE_CONNECTED. 

		// Will return only when a client attempts connection.

		if (!ConnectNamedPipe(hPipe, NULL))
		{
			TraceEvents(TRACE_LEVEL_ERROR, TRACE_PIPE,
				"ConnectNamedPipe failed with %d\n", GetLastError()
			);

			return 0;
		}

		// A client has connected to named pipe server, but it is possible
		// that there is already a client present.
		if (devContext->PipeServerAttributes.PidPipeHandle != NULL)
		{
			// If so, we need to see if the client is still alive.
			// If not, the PeekNamedPipe() will fail.
			if (!PeekNamedPipe(
				devContext->PipeServerAttributes.PidPipeHandle,
				NULL,
				0,
				NULL,
				NULL,
				NULL))
			{
				TraceEvents(TRACE_LEVEL_INFORMATION, TRACE_PIPE,
					"Old FFB client lost. Changing client...\n");

				// The newly connected client candidate is now the actual client.
				CloseHandle(devContext->PipeServerAttributes.PidPipeHandle);
			    devContext->PipeServerAttributes.PidPipeHandle = hPipe;
			}
			// PeekNamedPipe not faling, means that there is a working connection with a client,
			// that should not be interrupted.
			else
			{
				continue;
			}
		}
		// If there was no client before, our candidate is promoted to actual client.
		else
		{
			devContext->PipeServerAttributes.PidPipeHandle = hPipe;
		}

		TraceEvents(TRACE_LEVEL_INFORMATION, TRACE_PIPE,
			"FFB client connected to %ws\n",
			(LPCWSTR)devContext->PipeServerAttributes.PidPipePathName
		);
	}
}

VOID
WriteResponseToPidClient(
	PQUEUE_CONTEXT queueContext
)
{
	if (queueContext->DeviceContext->PipeServerAttributes.PidPipeHandle == NULL ||
		queueContext->DeviceContext->PipeServerAttributes.PidPipeHandle == INVALID_HANDLE_VALUE)
	{
		return;
	}

	DWORD bytesWritten = 0;

	if (!WriteFile(queueContext->DeviceContext->PipeServerAttributes.PidPipeHandle,
		queueContext->DeviceContext->ReportPacket.reportBuffer,
		queueContext->DeviceContext->ReportPacket.reportBufferLen,
		&bytesWritten, NULL) ||
		bytesWritten < queueContext->DeviceContext->ReportPacket.reportBufferLen)
	{
		if (!DisconnectNamedPipe(queueContext->DeviceContext->PipeServerAttributes.PidPipeHandle))
		{

			TraceEvents(TRACE_LEVEL_INFORMATION, TRACE_PIPE,
				"FFB client disconnected from %ws\n",
				(LPCWSTR)queueContext->DeviceContext->PipeServerAttributes.PidPipePathName
			);
			DWORD lastError = GetLastError();
			if (lastError != ERROR_PIPE_NOT_CONNECTED && lastError != ERROR_INVALID_HANDLE)
			{
				TraceEvents(TRACE_LEVEL_ERROR, TRACE_PIPE,
					"DisconnectNamedPipe failed with %d\n", lastError
				);
			}
			CloseHandle(queueContext->DeviceContext->PipeServerAttributes.PidPipeHandle);
			queueContext->DeviceContext->PipeServerAttributes.PidPipeHandle = NULL;
			return;
		}
	}
}


VOID 
CompleteReadRequest(
	PDEVICE_CONTEXT devContext,
	UCHAR ReportId)
{
	NTSTATUS status = STATUS_UNSUCCESSFUL;
	WDFREQUEST          reqRead = NULL;
	PVOID               pReadReport = NULL;
	size_t              bytesReturned = 0;


	// We retreive a request forwarded to ManualQueue whenever IOCTL_HID_READ_REPORT is requested.
	status = WdfIoQueueRetrieveNextRequest(
		devContext->ManualQueue,
		&reqRead
	);

	if (!NT_SUCCESS(status))
	{
		// If there are no requests in queue, no need to do anything.
		if (status == STATUS_NO_MORE_ENTRIES)
		{
			return;
		}

		TraceEvents(TRACE_LEVEL_INFORMATION, TRACE_PIPE,
			"WdfIoQueueRetrieveNextRequest failed with status %!STATUS!",
			status
		);

		goto errorExit;
	}

	status = WdfRequestRetrieveOutputBuffer(
		reqRead,
		sizeof(UCHAR),
		&pReadReport,
		&bytesReturned
	);

	if (!NT_SUCCESS(status))
	{
		TraceEvents(TRACE_LEVEL_INFORMATION, TRACE_PIPE,
			"WdfRequestRetrieveOutputBuffer failed with status %!STATUS!",
			status
		);

		goto errorExit;
	}


	if (bytesReturned > 1)
	{
		// We copy the report that was populated by data received from Input client
		if (ReportId == JOY_INPUT_REPORT_PARTIAL || ReportId == JOY_INPUT_REPORT_FULL)
		{
			RtlCopyMemory(pReadReport, &devContext->JoyInputReport, bytesReturned);
		}
		else if (ReportId == JOY_INPUT_PID_STATE_REPORT)
		{
			RtlCopyMemory(pReadReport, &devContext->JoyPidStateReport, bytesReturned);
		}
	}

	WdfRequestCompleteWithInformation(reqRead, status, bytesReturned);
	return;

errorExit:
	if (reqRead != NULL)
	{
		WdfRequestComplete(reqRead, status);
	}
}


BOOL
CreatePipeSecurity(
	PSECURITY_ATTRIBUTES* ppSa)
{
	BOOL fSucceeded = TRUE;
	DWORD dwError = ERROR_SUCCESS;

	PSECURITY_DESCRIPTOR pSd = NULL;
	PSECURITY_ATTRIBUTES pSa = NULL;

	// Define the SDDL for the security descriptor.
	PCWSTR szSDDL = L"D:"       // Discretionary ACL
		L"(A;OICI;GRGW;;;AU)"   // Allow read/write to authenticated users
		L"(A;OICI;GA;;;BA)";    // Allow full control to administrators

	if (!ConvertStringSecurityDescriptorToSecurityDescriptor(szSDDL,
		SDDL_REVISION_1, &pSd, NULL))
	{
		fSucceeded = FALSE;
		dwError = GetLastError();
		goto Cleanup;
	}

	// Allocate the memory of SECURITY_ATTRIBUTES.
	pSa = (PSECURITY_ATTRIBUTES)LocalAlloc(LPTR, sizeof(*pSa));
	if (pSa == NULL)
	{
		fSucceeded = FALSE;
		dwError = GetLastError();
		goto Cleanup;
	}

	pSa->nLength = sizeof(*pSa);
	pSa->lpSecurityDescriptor = pSd;
	pSa->bInheritHandle = FALSE;

	*ppSa = pSa;

Cleanup:
	// Clean up the allocated resources if something is wrong.
	if (!fSucceeded)
	{
		if (pSd)
		{
			LocalFree(pSd);
			pSd = NULL;
		}
		if (pSa)
		{
			LocalFree(pSa);
			pSa = NULL;
		}

		SetLastError(dwError);
	}

	return fSucceeded;
}


//
//   FUNCTION: FreePipeSecurity(PSECURITY_ATTRIBUTES)
//
//   PURPOSE: The FreePipeSecurity function frees a SECURITY_ATTRIBUTES 
//   structure that was created by the CreatePipeSecurity function. 
//
//   PARAMETERS:
//   * pSa - pointer to a SECURITY_ATTRIBUTES structure that was created by 
//     the CreatePipeSecurity function. 
//
VOID
FreePipeSecurity(
	PSECURITY_ATTRIBUTES pSa)
{
	if (pSa)
	{
		if (pSa->lpSecurityDescriptor)
		{
			LocalFree(pSa->lpSecurityDescriptor);
		}
		LocalFree(pSa);
	}
}