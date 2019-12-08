EXTERN_C_START

#define INPUT_AXIS 0x10
#define INPUT_BUTTON 0x11
#define INPUT_DPAD	0x12


#define AXIS_COUNT 8
#define BUTTON_SET_COUNT 8
#define DPAD_COUNT 4

BOOL
IsBitSet(
	UCHAR Value, 
	INT Bit);

BOOL
DeserializeJoyInput(
	UCHAR* ControllerInputMessage, 
	JOYSTICK_INPUT_REPORT* Report);

VOID
SetDefaultControllerState(
	JOYSTICK_INPUT_REPORT* Report);

EXTERN_C_END