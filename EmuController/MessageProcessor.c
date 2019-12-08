#include "driver.h"
#include "messageprocessor.tmh"



BOOL
IsBitSet(UCHAR Value, INT Bit)
{
	BOOL result = 0;
	return result = (Value >> Bit) & 1U;
}

BOOL 
DeserializeJoyInput(UCHAR* controllerInputMessage, JOYSTICK_INPUT_REPORT* report)
{
	PMESSAGE_HEADER msgHeader = (PMESSAGE_HEADER)controllerInputMessage;



	PJOYSTICK_INPUT_REPORT inputReport = (PJOYSTICK_INPUT_REPORT)report;

	INT i = sizeof(MESSAGE_HEADER);

	while (i < msgHeader->MessageLength)
	{

		switch (controllerInputMessage[i])
		{
			case INPUT_AXIS:
			{
				i++;   // Mapbyte
				int offset = i + 1; //data byte;
				int readBytes = 0;
				for (int a = 0; a < AXIS_COUNT; a++)
				{
					if (IsBitSet(controllerInputMessage[i], a))
					{
						inputReport->Axes[a] = controllerInputMessage[offset + 1] << 8 | controllerInputMessage[offset];
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
					if (IsBitSet(controllerInputMessage[i], a))
					{
						inputReport->Buttons[a] = controllerInputMessage[offset + 1] << 8 | controllerInputMessage[offset];
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
					if (IsBitSet(controllerInputMessage[i], a))
					{
						inputReport->HatSwitches[a] = controllerInputMessage[offset];
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
				SetDefaultControllerState(report);
				TraceEvents(TRACE_LEVEL_ERROR, TRACE_PIPE,
					"DeserializeInput failed: Malformed message.\n"
				);
				return 0;
			}
		}

	}
	report = inputReport;
	return i;
}

VOID
SetDefaultControllerState(JOYSTICK_INPUT_REPORT* report)
{

	PJOYSTICK_INPUT_REPORT inputReport = (PJOYSTICK_INPUT_REPORT)report;
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

	report = inputReport;

}