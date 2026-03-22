# BridgeSystems Bridgemate DataConnector ScoringProgramClient
Welcome to the source code for the scoring program client to the Bridgemate Data Connector.
Communication through the Bridgemate Data Connector is the preferred way for communication between bridge scoring programs and the Bridgemate 3, the Bridgemate 2 and the Bridgemate App back end. Bridgemate Control Software 5 is needed to receive, process and return data from the Data Connector.
In this repository you will find the documentation on how to write a client yourself as well as the source code for a scoring program client, written in C# for .Net Standard 2.0, provided by Bridge Systems BV. This client takes care of connecting, reconnecting and communication with the Data Connector. You can either use the source code as a reference to write your own code, or you can interface with the compiled BridgeSystems.Bridgemate.DataConnector.ScoringProgram.dll.

# The scoring program emulator (BridgeSystems.Bridgemate.DataConnectorClientEmulator)
In the [Scoring Program Emulator folder](https://github.com/bridgesystems/Bridgemate-Data-Connector-Scoring-Program-Client/tree/master/Scoring%20Program%20Emulator) you will find a zipped Visual Studio 2022 project that demonstrates how to use the ScoringProgramClient. Be free to use and adapt this to learn and test how to use the ScoringProgramClient for communication with the Bridgemate Data Connector. Redistribution of this code is not allowed.

## The compiled libraries
The client is available as a NuGet package on [nuget.org](https://www.nuget.org/packages/BridgeSystems.Bridgemate.DataConnector.ScoringProgramClient). To install:
```
dotnet add package BridgeSystems.Bridgemate.DataConnector.ScoringProgramClient
```
A symbol package (.snupkg) is included for debugging support.

Alternatively, the compiled libraries can be found in the Dll folder. Make sure to copy all *.dll files. The .pdb file can be useful when debugging. The .deps file may make it possible that the dlls other than the main one do not need to be copied. This may be the case when the programming environment can parse it and knows where to find the dependencies.

### Logging
The library uses `Microsoft.Extensions.Logging.Abstractions` so you can plug in any logging framework (NLog, Serilog, etc.). To enable logging, set the logger factory before using the client:
```csharp
DataConnectorLogging.LoggerFactory = yourLoggerFactory;
```
If not set, logging is silently disabled.

## Documentation
There are two main sources of documentation:
### 1. The general description of how to implement communication with the Bridgemate Data Connector.
Help documents that describe en detail how to connect to and communicate with the Data Connector, enabling creation of your own client on the programming platform of your choice:
1. Pdf: [Bridgemate Data Connector developer's guide](https://github.com/bridgesystems/BridgeSystems.Bridgemate.DataConnector.ScoringProgramClient/blob/master/Documentation/Doc/Bridgemate%20Data%20Connector%20developers%20guide.pdf)
2. Markdown: [Bridgemate Data Connector developer's guide](https://github.com/bridgesystems/Bridgemate-Data-Connector-Scoring-Program-Client/blob/master/Documentation/MD/index.md)

### 2. The context sensitive help for the source code of the BridgeSystems.Bridgemate.DataConnector.ScoringProgram.dll.
A help file that describes the methods and properties of the scoring program client provided by Bridge Systems BV.
1. Html: [Data Connector context sensitive help ](https://bridgesystems.github.io/Bridgemate-Data-Connector-Scoring-Program-Client/html/b11ca58b-c149-48f8-af9a-cf6a2c7bfe53.htm)

As of March 10th 2026, this repository has been moved from "bridgesystemsbv" to "BridgeSystems"