# CLLogger
An easy to use file logging system.
## Create Logger
### Default Values
- Log files path defaults to "exe-directory/logs/latest.log"
- Minimum log type defaults to "All Log Levels"
- Default pattern is "[ %TYPE%: %DATE% ]: %MESSAGE%"
- Default dump type is No Dump
- Default dump interval is 10 seconds
### Default Logger
```csharp
ILog log = LogManager.Init();
```
### Default Logger with modified path
```csharp
// This file and directories will be created for you.
ILog log = LogManager.Init().SetLogDirectory("relative/or/absolute/path/to/latest.log");
```
### Default Logger with modified minimum log type
#### Log Types
- All
- Debug
- Info
- Warn
- Error
- Fatal
```csharp
// This will only show logs at and above info
// Ex: Info, Warn, Error, Fatal
ILog log = LogManager.Init().SetMinimumLogType(LogTypes.Info);
```
### Default Logger with modified pattern
#### Pattern variables
- %TYPE% = Log Type
- %DATE% = Current Date and Time down to the millisecond
- %MESSAGE% = The actual log message
```csharp
// [DEBUG: 1/01/2000 5:00:00 PM]: Hello World
ILog log = LogManager.Init().SetPattern("[%TYPE%: %DATE%]: %MESSAGE%");
```
### Default Logger with modified dump type
#### Dump Types
- NoDump = Will never write log to file
- NoBuffer = Will write to file as soon as it was written to console
- Interval = Will dump all cached logs every x milliseconds
```csharp
ILog log = LogManager.Init().SetDumpMethod(DumpType.NoBuffer);
```
If you want a interval of 5 seconds or 5000 milliseconds
``` csharp
ILog log = LogManager.Init().SetDumpMethod(DumpType.Interval, 5000);
```
You can also use just the interval and it will automatically be DumpType of Interval
```csharp
ILog log = LogManager.Init().SetDumpMethod(5000);
```

## Create Log
### Send Info Log Message
``` csharp
log.Info("Hello World");
```
### Send Multiple Messages
``` csharp
log.Info("Hello World", "Good Bye World");
```
### Send Error Message with exception
``` csharp
try {
  /// Something that fails
} catch(Exception e) {
  log.Error("Hello World", e);
}
```
