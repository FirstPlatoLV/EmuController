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


	// Creating two named pipe servers for handling input report updates 
	// from client and optionally FFB data packets redirected from driver to the client.
	// Since only one instance is allowed for each named pipe server, 
	// attempting to create another instance with same name will result in error,
	// which we can use to prevent users from installing multiple Emucontroller devices 
	// with the same VendorId & ProductId,

	devContext->InputPipeHandle = CreateNamedPipe(
		inputPipeName,    // Pipe name 
		PIPE_ACCESS_INBOUND, // Server will be receiving messages only
		PIPE_TYPE_MESSAGE | PIPE_READMODE_MESSAGE, // Data is read from the pipe as a stream of messages.
		INSTANCES,   // No more than onw instance can be created for this pipe
		BUFFER_SIZE,  // Output buffer size 
		BUFFER_SIZE,  // Input buffer size 
		INFINITE,   // Client time-out 
		pSa
	);

	devContext->PidPipeHandle = CreateNamedPipe(
		pidPipeName,     // Pipe name 
		PIPE_ACCESS_DUPLEX, // Although only server will be sending actual messages to client, 
							// we need to allow the client to set it's read mode to PIPE_READMODE_MESSAGE.

		PIPE_TYPE_MESSAGE | PIPE_READMODE_MESSAGE,  // Data is read from the pipe as a stream of messages.
		INSTANCES,    // No more than onw instance can be created for this pipe
		BUFFER_SIZE,  // Output buffer size 
		BUFFER_SIZE,  // Input buffer size 
		INFINITE,   // Client time-out 
		pSa
	);


	if (devContext->InputPipeHandle == INVALID_HANDLE_VALUE || 
		devContext->PidPipeHandle == INVALID_HANDLE_VALUE)
	{
		TraceEvents(TRACE_LEVEL_ERROR, TRACE_PIPE,
			"CreateNamedPipe failed with %d\n", GetLastError()
		);

		return 1;
	}

	CreateThread(
		NULL,
		0,
		InputPipeServerThread,
		Params,
		0,
		NULL
	);

	CreateThread(
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
			"Waiting for client to connect...");


		// Will return only when a client attempts connection.
		if (!ConnectNamedPipe(devContext->InputPipeHandle, NULL))
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
		
		// Initializing JoyInputReport with default values.
		SetDefaultControllerState(&queueContext->DeviceContext->JoyInputReport);

		// This loop will continue as long as client is connected and messages it sends are valid.
		// The ReadFile will return false as soon as client drops, 
		// even if it doesn't close the pipe handle properly.
		while (ReadFile(devContext->InputPipeHandle, 
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
		if (!DisconnectNamedPipe(devContext->InputPipeHandle))
		{

			TraceEvents(TRACE_LEVEL_ERROR, TRACE_PIPE,
				"DisconnectNamedPipe failed with %d\n", GetLastError()
			);
			CloseHandle(devContext->InputPipeHandle);
			CloseHandle(devContext->PidPipeHandle);
			return 0;

		}
		TraceEvents(TRACE_LEVEL_INFORMATION, TRACE_PIPE,
			"Input client disconnected from %ws\n", 
			(LPCWSTR)devContext->PipeServerAttributes.InputPipePathName
		);


		SetDefaultControllerState(&queueContext->DeviceContext->JoyInputReport);
		CompleteReadRequest(devContext, JOY_INPUT_REPORT_FULL);

		// If a client that listens for FFB packets is connected we should disconnect it now
		// since it requires a client that sends input reports to be active.
		// We then start the FFB pipe server thread again and wait for new connection from client.
		if (devContext->PipeServerAttributes.PidPipeClientConnected)
		{
			DisconnectPidServer(devContext);

			CreateThread(
				NULL,
				0,
				PidPipeServerThread,
				Params,
				0,
				NULL
			);
		}
	}

	CloseHandle(devContext->InputPipeHandle);
	CloseHandle(devContext->PidPipeHandle);

	return 0;

}

DWORD WINAPI PidPipeServerThread(
	LPVOID Params)
{
	/* Params = WDFDEVICE */
	PDEVICE_CONTEXT     devContext = DeviceGetContext(Params);

	// The FFB pipe server is dependent on input pipe server,
	// which will close and create this thread again if needed, so no loop here.
	if (!ConnectNamedPipe(devContext->PidPipeHandle, NULL))
	{
		TraceEvents(TRACE_LEVEL_ERROR, TRACE_PIPE,
			"ConnectNamedPipe failed with %d\n", GetLastError()
		);
		return 0;

	}

	TraceEvents(TRACE_LEVEL_INFORMATION, TRACE_PIPE,
		"FFB client connected to %ws\n", 
		(LPCWSTR)devContext->PipeServerAttributes.PidPipePathName
	);

	// Input pipe server will use this value to determine 
	// whether FFB named pipe should be stopped and server restarted.
	devContext->PipeServerAttributes.PidPipeClientConnected = TRUE;

	return 0;

}

VOID
WriteResponseToPidClient(
	PQUEUE_CONTEXT queueContext
)
{
	// If FFB named pipe has no client connected, no point in trying to send FFB packets.
	if (!queueContext->DeviceContext->PipeServerAttributes.PidPipeClientConnected)
	{
		return;
	}

	DWORD bytesWritten = 0;
	

	if (!WriteFile(queueContext->DeviceContext->PidPipeHandle, 
		queueContext->DeviceContext->ReportPacket.reportBuffer,
		queueContext->DeviceContext->ReportPacket.reportBufferLen, 
		&bytesWritten, NULL) || 
		bytesWritten < queueContext->DeviceContext->ReportPacket.reportBufferLen)
	{
		DisconnectPidServer(queueContext->DeviceContext);		

		CreateThread(
			NULL,
			0,
			PidPipeServerThread,
			(LPVOID)queueContext->DeviceContext,
			0,
			NULL
		);
	}
}

VOID
DisconnectPidServer(
	PDEVICE_CONTEXT devContext)
{
	if (!DisconnectNamedPipe(devContext->PidPipeHandle))
	{

		TraceEvents(TRACE_LEVEL_ERROR, TRACE_PIPE,
			"DisconnectNamedPipe failed with %d\n", GetLastError()
		);
		CloseHandle(devContext->PidPipeHandle);
		return;

	}
	devContext->PipeServerAttributes.PidPipeClientConnected = FALSE;
	TraceEvents(TRACE_LEVEL_INFORMATION, TRACE_PIPE,
		"FFB client disconnected from %ws\n", 
		(LPWSTR)devContext->PipeServerAttributes.PidPipePathName
	);
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
		TraceEvents(TRACE_LEVEL_INFORMATION, TRACE_PIPE,
			"WdfIoQueueRetrieveNextRequest failed with status %!STATUS!",
			status
		);

		goto errorExit;
	}

	// If there are no requests in queue, no need to do anything.
	if (status == STATUS_NO_MORE_ENTRIES)
	{
		return;
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
		if (ReportId < JOY_INPUT_PID_STATE_REPORT)
		{
			RtlCopyMemory(pReadReport, &devContext->JoyInputReport, bytesReturned);
		}
		else
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