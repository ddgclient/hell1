# DDG Prime UserCodeTC Specification REP

## Methodology

This test method is intended for rapid script-like capabiliy where user can load and run a .cs or .py file without the need to compiling a dll.
Verify will automatically compile the object and store a reference so it can be invoked during Execute.



## Test Instance Parameters

| **Parameter Name** | **Required?** | **Type** | **Values** | **Comments** |
| ------------------ | ------------- | -------- | ---------- | ------------ |
| NamespaceClass     | Yes           | String   |            |Target Class. Example: SomeNamesapace.SomeClass|
| Method             | Yes           | String   |            |Target method. Example: PrintHello
| InputFile          | Yes           | String   |            |.cs or .py Input File.|

## Coding
C# code has access to all Prime Services and MUST return port number as string value.<br>
There is no support for input parameters so values should be passed thru SharedStorage.<br>
Python requires to set scope ExitPort variable before returning.<br>

**Limited Support of Prime Services and/or types. Submit issue in case of new requirement.**

### C# Example:
``` C#
namespace SomeNamespace
{
    using Prime.ConsoleService;
    public class SomeClass
    {
        public string HelloWorld()
        {
            Prime.Services.ConsoleService.PrintDebug("Hello world!");
            return "1";    
        }
    }
}
```
### Python Example:
``` Python
SharedStorageService.InsertRowAtTable('Key', -7, Context.DUT)
value = SharedStorageService.GetIntegerRowFromTable('Key', Context.DUT)
ConsoleService.PrintDebug('value=' + str(value))
ExitPort = 1
```


## OTPL Sample1:
``` Perl
Test UserCodeTC UserCode_P1
{
	InputFile = "SomeFile";	
	NamespaceClass = "SomeNamespace.SomeClass";
	Method = "PrintHello";
}
```
## Exit Ports

The CallbacksRegistrar test method supports the following exit ports:

| **Exit Port** | **Condition**   | **Description**              |
| ------------- | --------------- | ---------------------------- |
| **-2**        | ***Alarm***     | Any alarm condition          |
| **-1**        | ***Error***     | Any software condition error |
| **0**         | ***Fail***      | Failing condition            |
| **>=1**       | ***Pass***      | User to return port          |

## Additional Dependencies
Only Prime services and loaded DLLs are available for Referenced dependencies.

### Related Test Methods
#### Callbacks
UserCode execution is also supported as callback. <br>
User can compile and run code using registered callback in CallbacksManager. <br>
Argument uses CommandLine parser to get required parameters.<br>
Compiled objects are stored in a ConcurrentDictionary to avoid test time impact from compilation.<br>

### Syntax
**CompileUserCode(--file SomeFileName --namespace.class SomeNameSpace.SomeClass --method SomeMethod)**<br>
Returns "fail" or "pass" after compilation.<br>
**RunUserCode(--file SomeFileName --namespace.class SomeNameSpace.SomeClass --method SomeMethod)**<br>
Returns expected string value from invoked method.

### Related Services
## Version tracking


| **Date**       | **Version** | **Author**   | **Comments** |
| -------------- | ----------- | ------------ | ------------ |
| May 17, 2021   | 1.0.0       | fmurillo     |  Initial doc|       
| Apr 20, 2022   | 2.0.0       | fmurillo     |  Adding IronPython 2.7 support|       