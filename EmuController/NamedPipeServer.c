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
	

	int res = swprintf_s(inputPipeName, bufferCount, PszPipeIdFormat, PszLocalPipeTxt, devContext->HidDeviceAttributes.VendorID, devContext->HidDeviceAttributes.ProductID, PszInputPipeSuffix);
	
	if (res == 0 || inputPipeName == NULL)
	{
		TraceEvents(TRACE_LEVEL_ERROR, TRACE_PIPE,
			"Failed generating input pipe name."
		);

		return 1;
	}
	
	res = swprintf_s(pidPipeName, bufferCount, PszPipeIdFormat, PszLocalPipeTxt, devContext->HidDeviceAttributes.VendorID, devContext->HidDeviceAttributes.ProductID, PszPidPipeSuffix);
	
	
	if (res == 0 || pidPipeName == NULL)
	{
		TraceEvents(TRACE_LEVEL_ERROR, TRACE_PIPE,
			"Failed generating ffb pipe name."
		);

		return 1;
	}


	devContext->InputPipeHandle = CreateNamedPipe(
		inputPipeName,    // pipe name 
		/* write access, async */
		PIPE_ACCESS_INBOUND,
		/* Byte buffer transferred, blocking operation */
		PIPE_TYPE_MESSAGE | PIPE_READMODE_MESSAGE,
		INSTANCES,   // max. instances
		BUFFER_SIZE,  // output buffer size 
		BUFFER_SIZE,  // input buffer size 
		INFINITE,   // client time-out 
		pSa
	);

	devContext->PidPipeHandle = CreateNamedPipe(
		pidPipeName,    // pipe name 
		/* write access, async */
		PIPE_ACCESS_DUPLEX,
		/* Byte buffer transferred, blocking operation */
		PIPE_TYPE_MESSAGE | PIPE_READMODE_MESSAGE,
		INSTANCES,   // max. instances
		BUFFER_SIZE,  // output buffer size 
		BUFFER_SIZE,  // input buffer size 
		INFINITE,   // client time-out 
		pSa
	);


	if (devContext->InputPipeHandle == INVALID_HANDLE_VALUE || devContext->PidPipeHandle == INVALID_HANDLE_VALUE)
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

	while (1)

	{

		TraceEvents(TRACE_LEVEL_INFORMATION, TRACE_PIPE,
			"Waiting for client to connect...");

		if (!ConnectNamedPipe(devContext->InputPipeHandle, NULL))
		{
			TraceEvents(TRACE_LEVEL_ERROR, TRACE_PIPE,
				"ConnectNamedPipe failed with %d\n", GetLastError()
			);
			return 0;

		}

		TraceEvents(TRACE_LEVEL_INFORMATION, TRACE_PIPE,
			"Input client connected to %ws\n", (LPCWSTR)devContext->PipeServerAttributes.InputPipePathName
		);

		queueContext = QueueGetContext(devContext->ManualQueue);
		
		SetDefaultControllerState(&queueContext->DeviceContext->JoyInputReport);

		while (ReadFile(devContext->InputPipeHandle, buffer, BUFFER_SIZE, &bytesRead, NULL))
		{
			switch (buffer[0])
			{
			case JOY_INPUT_REPORT_PARTIAL:
			{
				DeserializeJoyInput(buffer, &queueContext->DeviceContext->JoyInputReport);
				CompleteReadRequest(devContext);
				break;
			}
			case JOY_INPUT_REPORT_FULL:
			{
				if (buffer[1] != sizeof(JOYSTICK_INPUT_REPORT))
				{
					TraceEvents(TRACE_LEVEL_ERROR, TRACE_PIPE,
						"JOYSTICK_INPUT_REPORT has incorrect size. Expected: %d, Received: %d\n", sizeof(JOYSTICK_INPUT_REPORT), buffer[1]
					);
					break;

				}

				RtlCopyMemory(&queueContext->DeviceContext->JoyInputReport, &buffer[MESSAGE_HEADER_LEN], sizeof(JOYSTICK_INPUT_REPORT));

				CompleteReadRequest(devContext);
				break;
			}
			case JOY_INPUT_PID_STATE_REPORT:
			{
				if (buffer[1] != sizeof(JOYSTICK_INPUT_REPORT))
				{
					TraceEvents(TRACE_LEVEL_ERROR, TRACE_PIPE,
						"PID_STATE_REPORT has incorrect size. Expected: %d, Received: %d\n", sizeof(PID_STATE_REPORT), buffer[1]
					);
					break;

				}

				RtlCopyMemory(&queueContext->DeviceContext->JoyPidStateReport, &buffer[MESSAGE_HEADER_LEN], sizeof(PID_STATE_REPORT));
				CompleteReadRequest(devContext);
				break;
			}
			}
		}

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
			"Input client disconnected from %ws\n", (LPCWSTR)devContext->PipeServerAttributes.InputPipePathName
		);


		SetDefaultControllerState(&queueContext->DeviceContext->JoyInputReport);
		CompleteReadRequest(devContext);


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

	if (!ConnectNamedPipe(devContext->PidPipeHandle, NULL))
	{
		TraceEvents(TRACE_LEVEL_ERROR, TRACE_PIPE,
			"ConnectNamedPipe failed with %d\n", GetLastError()
		);
		return 0;

	}

	TraceEvents(TRACE_LEVEL_INFORMATION, TRACE_PIPE,
		"FFB client connected to %ws\n", (LPCWSTR)devContext->PipeServerAttributes.PidPipePathName
	);

	devContext->PipeServerAttributes.PidPipeClientConnected = TRUE;

	return 0;

}

VOID
WriteResponseToPidClient(
	PQUEUE_CONTEXT queueContext
)
{
	if (!queueContext->DeviceContext->PipeServerAttributes.PidPipeClientConnected)
	{
		return;
	}

	DWORD bytesWritten = 0;
	

	if (!WriteFile(queueContext->DeviceContext->PidPipeHandle, 
		queueContext->DeviceContext->ReportPacket.reportBuffer,
		queueContext->DeviceContext->ReportPacket.reportBufferLen, 
		&bytesWritten, NULL) || bytesWritten < queueContext->DeviceContext->ReportPacket.reportBufferLen)
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
		"FFB client disconnected from %ws\n", (LPWSTR)devContext->PipeServerAttributes.PidPipePathName
	);
}



VOID 
CompleteReadRequest(
	PDEVICE_CONTEXT devContext)
{
	NTSTATUS status = STATUS_UNSUCCESSFUL;
	WDFREQUEST          reqRead = NULL;
	PVOID               pReadReport = NULL;
	size_t              bytesReturned = 0;


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
		RtlCopyMemory(pReadReport, &devContext->JoyInputReport, bytesReturned);
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