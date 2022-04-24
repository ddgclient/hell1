# DDG Prime FlowControlCallbacks
### Rev 0
----
## Enabling the Callbacks
Add an instance of CallbacksManager in your INIT flow. No parameters are necessary.

## Using callbacks
Callbacks can be executed from SingleRecoveryCallbackName parameter from FlowIndexCommonParam.xml.
``` Xml
<?xml version="1.0" encoding="utf-8"?>
<TestLibraryInterfaces xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema" xsi:schemaLocation="http://vtsm.intel.com/2009/TestLibraryInterfaces file:///C:/Intel/hdmt/hdmtOS_3.4.4.5_Release/TOSRelease/bin/release/TestLibraryInterfaces.xsd" xmlns="http://vtsm.intel.com/2009/TestLibraryInterfaces">
  <TestLibrary name="PrimeTestInstance">
    <TestClass name="PrimePauseTestMethodCommonParams" />
    <Imports>
    <FileName>FlowIndexCommonParam.xml</FileName>
    </Imports>
    <PublicBases />
    <Parameters />
    <ExitPorts />
  </TestLibrary>
</TestLibraryInterfaces>
```

[Prime WIKI - Calling Cs from Evg](https://dev.azure.com/mit-us/PRIME/_wiki/wikis/PRIME.wiki/3019/Calling-Prime-C-Sharp-Code-from-EVG-UF)

[Prime WIKI - Common Parameters](https://dev.azure.com/mit-us/PRIME/_wiki/wikis/PRIME.wiki/3020/Common-Parameters)

#### SetFlow
Sets active flow for the entered domain using FlowIndex parameter. <br>

Example: 
``` Perl
Test PrimePauseTestMethod Pause_IO_c1_r1_P0
{
	SleepTime = 10;
	FlowIndex = "1";
	ExitPort = 0;
	SingleRecoveryCallbackName = "SetFlow(GT)";
	LogLevel = "PRIME_DEBUG";
}
```

#### CheckFlow
Checks that FlowInstance FlowIndex is matching active flow while running in SinglePointTest mode. <br>
If flow is not matching, the instance will exit Port 0 skipping execution.<br>

Example: 
``` Perl
Test PrimePauseTestMethod Pause_IO_c1_r1_P0
{
	SleepTime = 10;
	FlowIndex = "1";
	ExitPort = 0;
	SingleRecoveryCallbackName = "CheckFlow(CPU)";
	LogLevel = "PRIME_DEBUG";
}
```
