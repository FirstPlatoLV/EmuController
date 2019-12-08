#include "driver.h"
#include "messageprocessor.tmh"


BOOL
IsBitSet(
	UCHAR Value, 
	INT Bit)
{
	BOOL result = 0;
	return result = (Value >> Bit) & 1U;
}

VOID 
DeserializeJoyInput(
	_In_ UCHAR* JoystickInputMessage, 
	_Out_ JOYSTICK_INPUT_REPORT* Report)
{

	/*++

	Routine Description:

		Deserializes message received from named pipe client.

	Arguments:

		ControllerInputMessage - The message sent by named pipe client
		Report - Pointer to JOYSTIC_INPUT_REPORT struct that will be updated with values in ControllerInputMessage

	Return Value:

		VOID

	--*/

	PMESSAGE_HEADER msgHeader = (PMESSAGE_HEADER)JoystickInputMessage;

	PJOYSTICK_INPUT_REPORT inputReport = (PJOYSTICK_INPUT_REPORT)Report;

	INT i = sizeof(MESSAGE_HEADER);


	/*++

		The JoystickInputMessage has the following format:
		ControlId byte = INPUT_AXIS | INPUT_BUTTON | INPUT_DPAD

		MapByte - Contains information about which values for a given control array are to be updated.
				  For example, given 8 axes defined in HID descriptor, 
				  the set bits in the MapByte correspond to indices of Axes array in JOYSTICK_INPUT_REPORT

		Data - The actual values for the control with following sizes:
			   INPUT_AXIS = USHORT
			   INPUT_BUTTON = USHORT (each USHORT represents state of 16 buttons, where bit set = BUTTON DOWN)
			   INPUT_DPAD = UCHAR
			   
			   The value count corresponds to the count of bits set in the MapByte for the given control type.

			   Example: 0x10,		0x42,		0xFFFF, 0x8000 
						INPUT_AXIS, 0b01000010, 65535,	32768

						The message above will result in the following updates:

						PJOYSTICK_INPUT_REPORT Report;
						Report->Axes[1] = 65535;
						Report->Axes[6] = 32768;
	--*/

	while (i < msgHeader->MessageLength)
	{

		switch (JoystickInputMessage[i])
		{
			case INPUT_AXIS:
			{
				i++;   // Mapbyte
				int offset = i + 1; //data byte;
				int readBytes = 0;
				for (int a = 0; a < AXIS_COUNT; a++)
				{
					if (IsBitSet(JoystickInputMessage[i], a))
					{
						inputReport->Axes[a] = JoystickInputMessage[offset + 1] << 8 | JoystickInputMessage[offset];
						offset += sizeof(USHORT);
						readBytes += sizeof(USHORT);
					}
				}
				i++;
				i += readBytes;
				break;
			}
			case INPUT_BUTTON:
			{
				i++;
				int offset = i + 1;
				int readBytes = 0;
				for (int a = 0; a < BUTTON_SET_COUNT; a++)
				{
					if (IsBitSet(JoystickInputMessage[i], a))
					{
						inputReport->Buttons[a] = JoystickInputMessage[offset + 1] << 8 | JoystickInputMessage[offset];
						offset += sizeof(USHORT);
						readBytes += sizeof(USHORT);
					}
				}
				i++;
				i += readBytes;
				break;
			}
			case INPUT_DPAD:
			{
				i++;
				int offset = i + 1;
				int readBytes = 0;
				for (int a = 0; a < DPAD_COUNT; a++)
				{
					if (IsBitSet(JoystickInputMessage[i], a))
					{
						inputReport->HatSwitches[a] = JoystickInputMessage[offset];
						offset += sizeof(UCHAR);
						readBytes += sizeof(UCHAR);
					}
				}
				i++;
				i += readBytes;
				break;
			}
			default:
			{
				SetDefaultControllerState(Report);
				TraceEvents(TRACE_LEVEL_ERROR, TRACE_PIPE,
					"DeserializeInput failed: Malformed message.\n"
				);
				return;
			}
		}

	}
	Report = inputReport;
}

VOID
SetDefaultControllerState(
	_Out_ JOYSTICK_INPUT_REPORT* Report)
{

	/*++

	Routine Description:

		 Sets the default values for JOYSTICK_INPUT_REPORT consistent with a wheel like HID device.
		 Can be used to initialize JOYSTICK_INPUT_REPORT with valid data.

	Arguments:

		Report - Pointer to JOYSTIC_INPUT_REPORT struct that will be updated with the default values.

	Return Value:

		VOID

	--*/


	PJOYSTICK_INPUT_REPORT inputReport = (PJOYSTICK_INPUT_REPORT)Report;
	inputReport->ReportId = JOY_INPUT_REPORT_ID;
	
	inputReport->Axes[0] = 32768;
	inputReport->Axes[1] = 32768;

	for (int i = 2; i < AXIS_COUNT; i++)
	{
		inputReport->Axes[i] = 65535;
	}

	for (int i = 0; i < BUTTON_SET_COUNT; i++)
	{
		inputReport->Buttons[i] = 0;
	}

	for (int i = 0; i < DPAD_COUNT; i++)
	{
		inputReport->HatSwitches[i] = 0xFF;
	}

	Report = inputReport;

}