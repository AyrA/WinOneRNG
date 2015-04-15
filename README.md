# General

This is a collection of tools for [the OneRNG Device](http://onerng.info/),
which is an open source hardware entropy source (random number generator).

# Driver

Please install the driver from the DRV subdirectory or the original site.
A readme with instructions is given in the DRV subdirectory.
uder Windows, OneRNG operates as a serial Port device. You must install the driver before
you can use any of the applications in this repository.

## Backup driver

If the original driver is unavailable (see README.md in DRV directory),
the .inf file required for installation is located in the DRV subdirectory.
No other files are required.

# OneRNG Applications

All OneRNG Applications listed below are written in C# using .NET 2.0.
Therefore they share some requirements:

## Requirements

Runs under any Windows OS that will also run .NET 2.0. Generally speaking,
you are safe if you use Windows XP or newer.
You can open the Project in Visual Studio 2008 and newer and it should migrate up
to .NET 4.5 without errors, if you prefer to upgrade the project.

## Build

All applications should build as-is, since there are no additional dependencies.
if you do not want to build them yourself,
see for the bin/Release folder inside each project for precompiled binaries.

# WinOneRNG
provides OneRNG Support for Windows

This application offers a TCP server that tries to distribute the random data
evenly over all connected devices.

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

The values you see above are the defaults which are applied, if no file is present.

### [NET]

Settings in the NET section

#### IP

This is the IP address to listen on. You can specify any IP your host identifies with,
including: any localhost address, any network interface address, the global 0.0.0.0 address.

To find all network intrerface addresses, open a command prompt and type
```ipconfig /all | find "IPv"```

You can also use IPv6 addresses, if you prefer.

#### Port

This is the TCP port to listen on. Use any number in the range from 1 to ushort.MaxValue

#### BlockSize

This is the number of bytes, each connected node gets before the application switches to the next node.
OneRNG is not very fast at generating numbers,
if you find people having to wait a long time for their turn, you can decrease this number.
This number only reflects the random bytes and not the header.
If you set this number to a very small number, you will have quite high network overhead.

### [SerialPort]

Settings in the SerialPort section

#### PortName

The name of the serial port to use. This is case insensitive.
You can get the port name from device manager or from the command prompt, by typing
```MODE | find "COM"``` or by running fileGen without arguments (see below).

# libOneRNG

This is the library for OneRNG.
It's a simple serial port wrapper with some OneRNG specific functions.

# fileGen

This tool allows you to generate files from OneRNG.

## Usage

Below is the command line help, that is shown, if no arguments are provided.
fileGen also reports all available ports below the help.

Information during generation is sent to stderr.

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

## Notes

/M argument: Counting files starts at 1. You can specify a mask,
that generates names identical to existing files.
FileGen will append random data to it,
if it is too short for the specified size (/S argument).
If it is the same length or longer, it is skipped.
