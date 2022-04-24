# DDG Prime IVCurve Test-Method Specification REP

Revision 1.0.1 5/20/2021

## Methodology

This is new DC test method based on SetPinAttributes methodology.<br>
It supports characterization but will always run production mode at the defined ForceSetPoint to determine pass/fail status (port).<br>

Before each pin execution the method will ReadPinAttributes.<br>
When more than one PinNames is being used the execution will be done serially (one pin at the time).<br>
After each pin is completed. The attributes get restored to the original values before moving to next pin.<br>
There are 28 total ports to assign different bins for different failing pins.<br>

**Note:** Every parameter lister as CommaSeparated* has to match the number of pins. If there is required value for one or more pins it could be left empty between the commas.

## Test Instance Parameters

Following optional parameters are set in additional to base values set from PrimeDcTestMethod. They are comma-separated an the number of values have to match the number of pins.

| **Parameter Name** | **Required?** | **Type**               | **Values**     | **Comments** |
| ------------------ | ------------- | ---------------------- | -------------- | ------------ |
| LevelsTc           | yes           | Test Condition Name    |                ||
| Pins               | yes           | CommaSeparatedString   |                |HDDPS pins to be tested.|
| MeasurementType    | yes           | MeasurementTypes       |Current,Voltage |ISVM or VSIM modes.|
| DatalogLevel       | yes           | DatalogLevels          |FailOnly,All    ||
| LowLimits          | yes           | CommaSeparatedString   |                |Low Limits (including units).|
| Highimits          | yes           | CommaSeparatedString   |                |High Limits (including units).|
| SharedStorageTokens| No            | CommaSeparatedString   |                |Store dc results in shared storage tokens.|
| ForceStartValue    | No            | CommaSeparatedDouble   |                |Start value. MIN value for characterization mode.|
| ForceStopValue     | No            | CommaSeparatedDouble   |                |Stop value. MAX value for characterization mode.|
| ForceStepSize      | No            | CommaSeparatedDouble   |                |Step size while sweeping voltage or current for characterization mode.|
| ForceSetPoint      | yes           | CommaSeparatedDouble   |                |Production mode setpoint.|
| SamplingCount      | yes           | CommaSeparatedDouble   |                |SamplingCount attribute.|
| SamplingRatio      | yes           | CommaSeparatedDouble   |                |SamplingRatio attribute.|
| PreMeasurementDelay| yes           | CommaSeparatedDouble   |                |PreMeasurementDelay in seconds.|
| IRange             | yes           | CommaSeparatedString   |                |Use for all pins.|
| VRange             | No            | CommaSeparatedString   |                |Use for HV pins.|
| IClampHi           | No            | CommaSeparatedDouble   |                |Use for all pins in VSIM mode.|
| IClampLo           | No            | CommaSeparatedDouble   |                |Use for all pins in VSIM mode.|
| FreeDriveTime      | No            | CommaSeparatedDouble   |                |Use for all pins VSIM mode.|
| FreeDriveCurrentHi | No            | CommaSeparatedDouble   |                |Use for VLC in VSIM mode.|
| FreeDriveCurrentLo | No            | CommaSeparatedDouble   |                |Use for VLC in VSIM mode.|
| VSlewStepRatio     | No            | CommaSeparatedDouble   |                |Use for HV pins in VSIM mode.|
| VClamp             | No            | CommaSeparatedDouble   |                |Use for VLC pins in ISVM mode.|
| OverVoltageLimit   | No            | CommaSeparatedDouble   |                |Use for HV, HC and LC in ISVM mode.|
| UnderVoltageLimit  | No            | CommaSeparatedDouble   |                |Use for HV, HC and LC in ISVM mode.|
| AlarmMode          | No            | Enum                   |                |When enabled Alarms will exit port 2.|



## Exit Ports

The CallbacksRegistrar test method supports the following exit ports:

| **Exit Port** | **Condition**   | **Description**                             |
| ------------- | --------------- | ------------------------------------------- |
| **-2**        | ***Alarm***     | Any alarm condition.|
| **-1**        | ***Error***     | Any software condition error.|
| **0**         | ***Fail***      | Fail condition, failing more than one pin or number of pins overflow(more than 28).|
| **1**         | ***Pass***      | Passing condition.|
| **2**         | ***Alarm***     | Alarm port when AlarmMode is Enabled.|
| **3-30**      | ***Fail***      | Fail single rail. Failing por is PinIndex+3; meaning if you fail first pin exits port 3, second pin port 4 and so on.|

## Additional Dependencies
### Related Test Methods
* PrimeDcTestMethod
## Version tracking


| **Date**       | **Version** | **Author**   | **Comments** |
| -------------- | ----------- | ------------ | ------------ |
| May 10, 2021   | 1.0.0       | fmurillo     |  Initial doc|       
| May 20, 2021   | 1.0.1       | fmurillo     |  Moving production mode to SetPinAttributes methodology.|       
| Nov 1,  2021   | 1.0.2       | fmurillo     |  Implementing AlarmMode.|       