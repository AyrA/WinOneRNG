# WinOneRNG
provides OneRNG Support for Windows

This application offers a TCP server that tries to distribute the random data
evenly over all connected devices.

# Requirements

Runs under any Windows OS that will also run .NET 2.0. Generally speaking,
you are safe if you use Windows XP or newer.
You can open the Project in Visual Studio 2008 and newer and it should migrate up
to .NET 4.5 without errors, if you prefer to upgrade the project.

# Build

The application should build as-is, since there are no additional dependencies.

Have a look at rnd\bin\Release\rnd.exe for a pre-built executable and DLL if you
prefer to run precompiled versions.

# Configuration

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