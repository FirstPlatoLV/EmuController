========================================================================
    EmuController Project Overview
========================================================================

This file contains a summary of what you will find in each of the files that make up your project.

EmuController.vcxproj
    This is the main project file for projects generated using an Application Wizard.
    It contains information about the version of the product that generated the file, and
    information about the platforms, configurations, and project features selected with the
    Application Wizard.

EmuController.vcxproj.filters
    This is the filters file for VC++ projects generated using an Application Wizard.
    It contains information about the association between the files in your project
    and the filters. This association is used in the IDE to show grouping of files with
    similar extensions under a specific node (for e.g. ".cpp" files are associated with the
    "Source Files" filter).

Public.h
    Header file to be shared with applications.

Driver.c & Driver.h
    DriverEntry and WDFDRIVER related functionality and callbacks.

Device.c & Device.h
    WDFDEVICE related functionality and callbacks.

Queue.c & Queue.h
    WDFQUEUE related functionality and callbacks.

Ioctl.c & Ioctl.h
    Functionality for handling various input/output control calls.

NamedPipeServer.c & NamedPipeServer.h
    Functionality for setting up a side-communication with driver via two named pipe server instances.

MessageProcessor.c & MessageProcessor.h
    Functionality for processing messages from input client that contains non-complete input report updates.

Trace.h
    Definitions for WPP tracing.
