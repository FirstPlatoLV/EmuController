/*++

Module Name:

    public.h

Abstract:

    This module contains the common declarations shared by driver
    and user applications.

Environment:

    driver and application

--*/

EXTERN_C_START


//
// Use devcon to install a device: 
//
// devcon install EmuController.inf "Root\VID_DEED&PID_FE00"
//

//
// Define an Interface Guid so that apps can find the device and talk to it.
//
DEFINE_GUID (GUID_DEVINTERFACE_EmuController,
    0x3d877443,0x4dda,0x4bab,0xa3,0xe2,0xdf,0x60,0x7b,0x30,0xde,0x4d);
// {3d877443-4dda-4bab-a3e2-df607b30de4d}


// Input client message header
typedef struct _MESSAGE_HEADER
{
	UCHAR MssageType;
	UCHAR MessageLength;

} MESSAGE_HEADER, * PMESSAGE_HEADER;

#define JOY_INPUT_REPORT_PARTIAL 0x01
#define JOY_INPUT_REPORT_FULL 0x02
#define JOY_INPUT_PID_STATE_REPORT 0x03

// Input report Ids. 
#define JOY_INPUT_REPORT_ID 0x01
#define JOY_PID_INPUT_REPORT_ID 0x02  // Not implemented

// Output report Ids.
#define PID_SET_EFFECT_REPORT_ID 0x10
#define PID_SET_ENVELOPE_REPORT_ID 0x11
#define PID_SET_CONDITION_REPORT_ID 0x12
#define PID_SET_PERIODIC_REPORT_ID 0x13
#define PID_SET_CONSTANT_FORCE_REPORT_ID 0x14
#define PID_SET_RAMP_FORCE_REPORT_ID 0x15
#define PID_SET_CUSTOM_FORCE_DATA_REPORT_ID 0x16
#define PID_DOWNLOAD_SAMPLE_REPORT_ID 0x17
#define PID_EFFECT_OPERATION_REPORT 0x18
#define PID_DEVICE_CONTROL_REPORT_ID 0x19
#define PID_BLOCK_FREE_REPORT_ID 0x1A
#define PID_DEVICE_GAIN_REPORT_ID 0x1B
#define PID_SET_CUSTOM_FORCE_REPORT_ID 0x1C

// Feature report Ids.
#define PID_NEW_EFFECT_REPORT_ID 0x20
#define PID_BLOCK_LOAD_REPORT_ID 0x21
#define PID_POOL_REPORT_ID 0x22

// Maximum number of EffectIndexBlocks & Simulatenous effects playing.
#define MAX_EFFECT_BLOCKS 0x7F

// The command in DeviceControl packet for reseting the PID device
// (which implies clearing all effects and hence resetting EffectIndexBlock)
#define PID_DEVICE_RESET_CMD 0x04

#include <pshpack1.h>

typedef struct _JOYSTICK_INPUT_REPORT
{
	UCHAR  ReportId;                      // Report ID = 0x01
	USHORT Buttons[8];					  // Each 16-bit value represents a set of states of 16 buttons (Bit set = Button Down)

	UCHAR  HatSwitches[4];				  // 8 possible states for each DPad, 
										  // 0 = North, with each next value advancing the position of DPad 45 degrees clock-wise. 
										  // 0xFF = Neutral state.

	USHORT Axes[8];						  // Unsigned 16-bit resolution axes.

} JOYSTICK_INPUT_REPORT, * PJOYSTICK_INPUT_REPORT;


//
// [OPTIONAL] The virtual device does not require this input report to be sent,
// in order for proper functioning
//
typedef struct _PID_STATE_REPORT
{
	UCHAR  ReportId;                      // Report ID = 0x02
	UCHAR  DevicePaused : 1;			  // Value = 0 to 1
	UCHAR  ActuatorsEnabled : 1;		  // Value = 0 to 1
	UCHAR  SafetySwitch : 1;			  // Value = 0 to 1
	UCHAR  ActuatorOverrideSwitch : 1;    // Value = 0 to 1
	UCHAR  ActuatorPower : 1;			  // Value = 0 to 1
	UCHAR : 3;                            // Pad

    UCHAR ReportEffectPlaying : 1;		  // Value = 0 to 1
    UCHAR EffectBlockIndex : 7;			  // Value = 1 to 127 (MAX_EFFECT_BLOCKS)
} PID_STATE_REPORT, * PPID_STATE_REPORT;






typedef struct _PID_DEVICE_CONTROL_REPORT
{
	UCHAR ReportId;						  // Report ID = 0x19
	UCHAR DeviceControlCommand;			  // EnableActuactors = 1,
										  // DisableActuactors = 2,
										  // StopAllEffects = 3,
										  // Reset = 4,
										  // Pause = 5,
										  // Continue = 6

} PID_DEVICE_CONTROL_REPORT, * PPID_DEVICE_CONTROL_REPORT;


typedef struct _PID_NEW_EFFECT_REPORT
{
	UCHAR  ReportId;                                 // Report ID = 0x01 (1)
													   // Collection: CA:Joystick CL:CreateNewEffectReport CL:EffectType
	UCHAR  EffectType; // Usage 0x000F0028: ET Custom Force Data, Value = 1 to 12, Physical = ((Value - 1) + 1)
													   // Collection: CA:Joystick CL:CreateNewEffectReport
	UCHAR ByteCount;

} PID_NEW_EFFECT_REPORT, * PPID_NEW_EFFECT_REPORT;


typedef struct _PID_BLOCK_LOAD_REPORT
{
	UCHAR  ReportId;                                 // Report ID = 0x02 (2)
													   // Collection: CA:Joystick CL:PIDBlockLoadReport
	UCHAR  EffectBlockIndex; // Usage 0x000F0022: Effect Block Index, Value = 1 to 40, Physical = ((Value - 1) + 1)
													   // Collection: CA:Joystick CL:PIDBlockLoadReport CL:BlockLoadStatus
	UCHAR  BlockLoadStatus; // Usage 0x000F008C: Block Load Success, Value = 1 to 3, Physical = ((Value - 1) + 1)
													   // Collection: CA:Joystick CL:PIDBlockLoadReport
	USHORT RamPoolAvailable; // Usage 0x000F00AC: RAM Pool Available, Value = 0 to 65535, Physical = Value
} PID_BLOCK_LOAD_REPORT, * PPID_BLOCK_LOAD_REPORT;


//
// Handled by driver internally.
//
typedef struct _PID_POOL_REPORT
{
	UCHAR  ReportId;
	USHORT RamPoolSize;  
	UCHAR  SimultaneousEffectsMax;
	UCHAR  DeviceManagedPool : 1;
	UCHAR  SharedParameterBlocks : 1; 
	UCHAR : 6;
} PID_POOL_REPORT, * PPID_POOL_REPORT;


#include <poppack.h>

EXTERN_C_END