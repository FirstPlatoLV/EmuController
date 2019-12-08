EXTERN_C_START

// Control type ids in input report message
#define INPUT_AXIS 0x10
#define INPUT_BUTTON 0x11
#define INPUT_DPAD	0x12

// Maximum number of value updates in message for each control type
#define AXIS_COUNT 8
#define BUTTON_SET_COUNT 8
#define DPAD_COUNT 4

//
// Checks if specified bit in UCHAR value is set.
//
BOOL
IsBitSet(
	UCHAR Value, 
	INT Bit);


//
// Deserializes joystick input report message received from named pipe client.
//
VOID
DeserializeJoyInput(
	_In_ UCHAR* JoystickInputMessage,
	_Out_ JOYSTICK_INPUT_REPORT* Report);


//
// Sets the joystick inptu report to default values.
//
VOID
SetDefaultControllerState(
	_Out_ JOYSTICK_INPUT_REPORT* Report);

EXTERN_C_END