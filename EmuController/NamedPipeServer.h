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
#pragma once
EXTERN_C_START


#define BUFFER_SIZE 128
#define INPUT_INSTANCES 1
#define PID_INSTANCES 2

DWORD
CreateNamedPipeServer(
	LPVOID Params
);

DWORD WINAPI 
InputPipeServerThread(
	LPVOID Params);

DWORD WINAPI 
PidPipeServerThread(
	LPVOID Params);

VOID
WriteResponseToPidClient(
	PQUEUE_CONTEXT queueContext);

VOID
CompleteReadRequest(
	PDEVICE_CONTEXT devContext,
	UCHAR ReportId);

BOOL 
CreatePipeSecurity(
	PSECURITY_ATTRIBUTES* pSecurityAttributes);

VOID
FreePipeSecurity(
	PSECURITY_ATTRIBUTES psa);

EXTERN_C_END