/*++

Module Name:

    queue.h

Abstract:

    This file contains the queue definitions.

Environment:

    User-mode Driver Framework 2

--*/

EXTERN_C_START

//
// This is the context that can be placed per queue
// and would contain per queue information.
//
typedef struct _QUEUE_CONTEXT {

	WDFQUEUE                Queue;
	PDEVICE_CONTEXT         DeviceContext;

} QUEUE_CONTEXT, *PQUEUE_CONTEXT;

WDF_DECLARE_CONTEXT_TYPE_WITH_NAME(QUEUE_CONTEXT, QueueGetContext)

NTSTATUS
EmuControllerQueueInitialize(
    _In_ WDFDEVICE Device,
	_Out_ WDFQUEUE *Queue
    );

NTSTATUS
EmuControllerManualQueueInitialize(
	_In_ WDFDEVICE Device,
	_Out_ WDFQUEUE* Queue
);

//
// Events from the IoQueue object
//
EVT_WDF_IO_QUEUE_IO_DEVICE_CONTROL EmuControllerEvtIoDeviceControl;

EXTERN_C_END
