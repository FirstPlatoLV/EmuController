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

#define MESSAGE_HEADER_LEN sizeof(MESSAGE_HEADER)

#define JOY_INPUT_REPORT_PARTIAL 0x01
#define JOY_INPUT_REPORT_FULL 0x02
#define JOY_INPUT_PID_STATE_REPORT 0x03

// Input report Ids. 
#define JOY_INPUT_REPORT_ID 0x01
#define PID_INPUT_REPORT_ID 0x02

// Output report Ids.
#define PID_SET_EFFECT_REPORT_ID 0x10
#define PID_SET_ENVELOPE_REPORT_ID 0x11
#define PID_SET_CONDITION_REPORT_ID 0x12
#define PID_SET_PERIODIC_REPORT_ID 0x13
#define PID_SET_CONSTANT_FORCE_REPORT_ID 0x14
#define PID_SET_RAMP_FORCE_REPORT_ID 0x15
#define PID_SET_CUSTOM_FORCE_DATA_REPORT_ID 0x16
#define PID_DOWNLOAD_SAMPLE_REPORT_ID 0x17
#define PID_EFFECT_OPERATION_REPORT_ID 0x18
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

#define PID_DEVICE_RESET_CMD 0x04 

#include <pshpack1.h>

typedef struct _JOYSTICK_INPUT_REPORT
{
	UCHAR  ReportId;	// Report ID = 0x01
	USHORT Buttons[8];	// Each 16-bit value represents a set of states for 16 buttons 
	// (Bit set = Button Down)

	UCHAR  HatSwitches[4];	// 8 possible states for each DPad, 
	// 0 = North, with each next value advancing the position of DPad 45 degrees clock-wise. 
	// 0xFF = Neutral state.

	USHORT Axes[8];	// Unsigned 16-bit resolution axes.

} JOYSTICK_INPUT_REPORT, * PJOYSTICK_INPUT_REPORT;


//
// [OPTIONAL] The virtual device does not require this input report 
// to be sent by the client in order for proper functioning
//
typedef struct _PID_STATE_REPORT
{
	UCHAR  ReportId;	// Report ID = 0x02
	UCHAR  DevicePaused : 1;	// Value = 0 to 1
	UCHAR  ActuatorsEnabled : 1;	// Value = 0 to 1
	UCHAR  SafetySwitch : 1;	// Value = 0 to 1
	UCHAR  ActuatorOverrideSwitch : 1;	// Value = 0 to 1
	UCHAR  ActuatorPower : 1;	// Value = 0 to 1
	UCHAR : 3;	// Pad

    UCHAR ReportEffectPlaying : 1;	// Value = 0 to 1
    UCHAR EffectBlockIndex : 7;	// Value = 1 to MAX_EFFECT_BLOCKS (127)
} PID_STATE_REPORT, * PPID_STATE_REPORT;


typedef struct _PID_SET_EFFECT_REPORT
{
	UCHAR ReportId;	// Report ID = 0x10
	UCHAR EffectBlockIndex; // Value = 1 to MAX_EFFECT_BLOCKS(127); 
	UCHAR EffectType;	// Constant = 1,
				// Ramp = 2,
				// Square = 3,
				// Sine = 4,
				// Triangle = 5,
				// SawtoothUp = 6,
				// SawtoothDown = 7,
				// Spring = 8,
				// Damper = 9,
				// Inertia = 10,
				// Friction = 11,
				// CustomForce = 12

	USHORT Duration;   // Value = 0 to 10000
	USHORT TriggerRepeatInterval; // Value = 0 to 10000
	USHORT SamplePeriod; // Value = 0 to 10000
	USHORT Gain; // Value = 0 to 10000
	UCHAR TriggerButton; // Value = 1 to 128

	UCHAR AxesEnableX : 1; // Value = 0 to 1
	UCHAR AxesEnableY : 1; // Value = 0 to 1
	UCHAR DirectionEnable : 1; // Value = 0 to 1, Physical = Value
	UCHAR : 5;	// Pad

	USHORT DirectionInstance1;	// Value = 0 to 36000
	USHORT DirectionInstance2;	// Value = 0 to 36000
	USHORT StartDelay;	// Value = 0 to 10000
} PID_SET_EFFECT_REPORT, * PPID_SET_EFFECT_REPORT;


typedef struct _PID_SET_ENVELOPE_REPORT
{
	UCHAR ReportId;	// Report ID = 0x11
	UCHAR EffectBlockIndex; // Value = 1 to MAX_EFFECT_BLOCKS(127); 
	USHORT AttackLevel; // Value = 0 to 10000
	USHORT FadeLevel; // Value = 0 to 10000
	USHORT AttackTime; // Value = 0 to 10000
	USHORT FadeTime; // Value = 0 to 10000
} PID_SET_ENVELOPE_REPORT, * PPID_SET_ENVELOPE_REPORT;


typedef struct _PID_SET_CONDITION_REPORT
{
	UCHAR ReportId;	// Report ID = 0x12
	UCHAR EffectBlockIndex; // Value = 1 to MAX_EFFECT_BLOCKS(127); 
	UCHAR ParameterBlockOffset : 4; // Value = 0 to 1
	UCHAR TypeSpecificBlockOffsetInstance1 : 2; // Value = 0 to 1
	UCHAR TypeSpecificBlockOffsetInstance2 : 2; // Value = 0 to 1
	SHORT CpOffset;	// Value = -10000 to 10000
	SHORT PositiveCoefficient; // Value = -10000 to 10000
	SHORT NegativeCoefficient; // Value = -10000 to 10000
	USHORT PositiveSaturation; // Value = 0 to 10000
	USHORT NegativeSaturation; // Value = 0 to 10000
	USHORT DeadBand;  // Value = 0 to 10000
} PID_SET_CONDITION_REPORT, * PPID_SET_CONDITION_REPORT;

typedef struct _PID_SET_PERIODIC_REPORT
{
	UCHAR ReportId;	// Report ID = 0x13
	UCHAR EffectBlockIndex; // Value = 1 to MAX_EFFECT_BLOCKS(127); 
	USHORT Magnitude;	// Value = 0 to 10000
	SHORT Offset;	// Value = -10000 to 10000
	USHORT Phase;	// Value = 0 to 36000
	USHORT Period;	// Value = 0 to 10000
} PID_SET_PERIODIC_REPORT, * PPID_SET_PERIODIC_REPORT;

typedef struct _PID_SET_CONSTANT_FORCE_REPORT
{
	UCHAR ReportId;	// Report ID = 0x14
	UCHAR EffectBlockIndex; // Value = 1 to MAX_EFFECT_BLOCKS(127); 
	SHORT Magnitude; // Value = -10000 to 10000
} PID_SET_CONSTANT_FORCE_REPORT, * PPID_SET_CONSTANT_FORCE_REPORT;

typedef struct _PID_SET_RAMP_FORCE_REPORT
{
	UCHAR ReportId;	// Report ID = 0x15
	UCHAR EffectBlockIndex; // Value = 1 to MAX_EFFECT_BLOCKS(127); 
	SHORT RampStart;  // Value = -10000 to 10000
	SHORT RampEnd;    // Value = -10000 to 10000
} PID_SET_RAMP_FORCE_REPORT, * PPID_SET_RAMP_FORCE_REPORT;

typedef struct _PID_SET_CUSTOM_FORCE_DATA_REPORT
{
	UCHAR ReportId;	// Report ID = 0x16
	UCHAR EffectBlockIndex; // Value = 1 to MAX_EFFECT_BLOCKS(127); 
	USHORT CustomForceDataOffset; // Value = 0 to 10000
	UCHAR CustomForceData[12];
} PID_SET_CUSTOM_FORCE_DATA_REPORT, * PPID_SET_CUSTOM_FORCE_DATA_REPORT;

typedef struct _PID_DOWNLOAD_SAMPLE_REPORT
{
	UCHAR ReportId;	// Report ID = 0x17
	UCHAR EffectBlockIndex; // Value = 1 to MAX_EFFECT_BLOCKS(127); 
	CHAR  ForceSampleX; // Value = -127 to 127
	CHAR  ForceSampleY; // Value = -127 to 127
} PID_DOWNLOAD_SAMPLE_REPORT, * PPID_DOWNLOAD_SAMPLE_REPORT;


typedef struct _PID_EFFECT_OPERATION_REPORT
{
	UCHAR ReportId;	// Report ID = 0x18
	UCHAR EffectBlockIndex; // Value = 1 to MAX_EFFECT_BLOCKS(127); 
	UCHAR EffectOperation;	// Start = 1,
					// StartSolo = 2,
					// Stop = 3
	UCHAR LoopCount; // Value = 0 to 255;
} PID_EFFECT_OPERATION_REPORT, * PPID_EFFECT_OPERATION_REPORT;

typedef struct _PID_DEVICE_CONTROL_REPORT
{
	UCHAR ReportId;	// Report ID = 0x19
	UCHAR DeviceControlCommand;	// EnableActuactors = 1,
						// DisableActuactors = 2,
						// StopAllEffects = 3,
						// Reset = 4,
						// Pause = 5,
						// Continue = 6

} PID_DEVICE_CONTROL_REPORT, * PPID_DEVICE_CONTROL_REPORT;


typedef struct _PID_BLOCK_FREE_REPORT
{
	UCHAR ReportId;	// Report ID = 0x1A
	UCHAR EffectBlockIndex; // Value = 1 to MAX_EFFECT_BLOCKS(127); 
} PID_BLOCK_FREE_REPORT, * PPID_BLOCK_FREE_REPORT;



typedef struct _PID_DEVICE_GAIN_REPORT
{
	UCHAR ReportId;	// Report ID = 0x1B
	USHORT DeviceGain;	// Value = 0 to 10000
} PID_DEVICE_GAIN_REPORT, * PPID_DEVICE_GAIN_REPORT;

typedef struct _PID_SET_CUSTOM_FORCE_REPORT
{
	UCHAR ReportId;	// Report ID = 0x1C
	UCHAR EffectBlockIndex; // Value = 1 to MAX_EFFECT_BLOCKS(127); 
	UCHAR SampleCount; // Value = 0 to 255
	USHORT SamplePeriod; // Value = 0 to 10000
} PID_SET_CUSTOM_FORCE_REPORT, * PPID_SET_CUSTOM_FORCE_REPORT;

typedef struct _PID_NEW_EFFECT_REPORT
{
	UCHAR ReportId;	// Report ID = 0x20
	UCHAR EffectType;	// Constant = 1,
				// Ramp = 2,
				// Square = 3,
				// Sine = 4,
				// Triangle = 5,
				// SawtoothUp = 6,
				// SawtoothDown = 7,
				// Spring = 8,
				// Damper = 9,
				// Inertia = 10,
				// Friction = 11,
				// CustomForce = 12
	UCHAR ByteCount;	// Applies to CustomForce only.

} PID_NEW_EFFECT_REPORT, * PPID_NEW_EFFECT_REPORT;


typedef struct _PID_BLOCK_LOAD_REPORT
{
	UCHAR  ReportId;	// Report ID = 0x21
	UCHAR  EffectBlockIndex; // Value = 1 to MAX_EFFECT_BLOCKS(127); 
					// Value = 0 if operation fails.
	UCHAR  BlockLoadStatus; // Success = 1
				// Full = 2
				// Error = 3
	USHORT RamPoolAvailable; //Value = 0 to 65535
} PID_BLOCK_LOAD_REPORT, * PPID_BLOCK_LOAD_REPORT;


//
// Handled by the driver internally.
//
typedef struct _PID_POOL_REPORT
{
	UCHAR  ReportId; // Report ID = 0x22
	USHORT RamPoolSize;  // Value = 0 to 65535
	UCHAR  SimultaneousEffectsMax; // MAX_EFFECT_BLOCKS(127);
	UCHAR  DeviceManagedPool : 1; // Value = 0 to 1
	UCHAR  SharedParameterBlocks : 1; // Value = 0 to 1
	UCHAR : 6;
} PID_POOL_REPORT, * PPID_POOL_REPORT;


#include <poppack.h>

EXTERN_C_END