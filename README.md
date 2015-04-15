# WinOneRNG
provides OneRNG Support for Windows

This application offers a TCP server that tries to distribute the random data
evenly over all connected devices.

## Requirements

Runs under any Windows OS that will also run .NET 2.0. Generally speaking,
you are safe if you use Windows XP or newer.
You can open the Project in Visual Studio 2008 and newer and it should migrate up
to .NET 4.5 without errors, if you prefer to upgrade the project.

## Build

The application should build as-is, since there are no additional dependencies.

Have a look at rnd\bin\Release\rnd.exe for a pre-built executable and DLL if you
prefer to run precompiled versions.

## Configuration

Place a config.ini in the folder, where the exe is located and add this content:

```
[NET]
IP=127.0.0.1
Port=48879
BlockSize=1024

[SerialPort]
PortName=COM1

```

The values you see above are the defaults, that get applied, if no file is present.

# libOneRNG

This is the library for OneRNG.
It's a simple serial port wrapper with some OneRNG specific functions.

# fileGen

This tool allows you to generate files from OneRNG.

## Usage

Below is the command line help, that is shown, if no arguments are provided.
fileGen also reports all available ports below the help.

```
fileGen.exe <Port> [/D Directory] [/M Mask] [/S FileSize] [/N NumFiles]

Port   Serial Port to use. See below for the list of available ports.

/D     Directory to save files into. Defaults to current directory.

/M     File name mask. Use an exclamation mark to specify, where the number
       is put. Defaults to '!.bin'. If Mask is a single dash (-),
       then output is sent to stdout. This enforces /S to be non-zero.
       If the mask does not contains a '!', then /N is ignored

/S     File size in Megabytes. Defaults to 10. (0 = Infinite)

/N     Number of files. Defaults to 1. (0 = Infinite). Ignored if file size
       is also infinite.

```
