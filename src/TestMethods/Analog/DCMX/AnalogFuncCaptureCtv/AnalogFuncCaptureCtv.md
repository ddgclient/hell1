# DDG Prime AnalogFuncCaptureCtv
### Rev 0

## Introduction
This TestMethod is an **PrimeFuncCaptureCtvTestMethod** Extention that post processes the CTVs based on the input csv specified in **ConfigurationFile** parameter.

## Methodology
1. Setup the **ConfigurationFile** as csv.  More info on the csv file below.
2. Captured CTV bits are split based on the input csv **Field** parameter and coverted to integer.
3. The split CTVs can be printed in ituff.
4. The split CTVs can be stored in the SharedStorage as integer.
5. Math operations and kills can be performed on the split CTVs.

## Errors and Exceptions
- Throws Exception
   - if the number of CTVs bits  don't match the **Size** parameter in the input csv.
   - if the input csv column name are not supported.
   - if both **ExpectedData** parameter and **Limit** parameters are defined
   - if **FailPort** parameter is blank in the input csv
   - if **FailPort** parameter is greater than **6**  in the input csv

## Parameters
### Template parameters
#### ConfigurationFile
- input csv based on which the CTVs are decoded.
- More info
#### CtvCapturePins
- Comma-separated list of pins to capture CTV data.
#### Kill
- Enables/disables the kills provided in the input csv
#### CsvDelimiter (Optional)
- Default delimiter is **","**
- Used to define the separator used in input csv.
- <span style='color:red'> Needs to be used while including math equation like max(3,4)  in the **Equation** parameter in the input csv else the equation would be considered as extra column in the input csv.</span>
#### PinRename
- renames in the CtvCapturePins within the input csv for ituff/console printing and SharedStorage names
#### LevelsTc
- Levels test condition.
#### Patlist
- Plist name. Important: This test class process CTV data only. No fail data.
#### TimingsTc
- Timings test condition.
### Input csv parameters/column names
- Pin <span style='color:red'>(required)</span>
  - CTV pin to capture
- IP
- Sequence
- Subsequence
- Field <span style='color:red'>(required)</span>
  - Register name of the split CTV.
  - Integer datatype
- Size<span style='color:red'>(required)</span>
  - bit length of the split CTV
- PerBit
- LowLimit
- HighLimit
- ExpectedData
- Equation
- ItuffToken
  - Groups the registers to be printed in ituff
  - uses **"|"** as the delimiter
- Base
  - Supports Base 2,4,8,16 for ituff printing
- SharedStorageToken
  - Stores values in the SharedStorage
## Ports
- All fail ports are user definied using **FailPort** parameter in the input csv.
- if **FailPort** is defined to be  greater than 6, it would be routed to Port0
```
[Returns(1, PortType.Pass, "Passed.")]
[Returns(0, PortType.Fail, "Failed Port0")]
[Returns(2, PortType.Fail, "Failed Port2")]
[Returns(3, PortType.Fail, "Failed Port3")]
[Returns(4, PortType.Fail, "Failed Port4")]
[Returns(5, PortType.Fail, "Failed Port5")]
[Returns(6, PortType.Fail, "Failed Port6")]
```
## mtpl Example

```
Test AnalogFuncCaptureCtv DTS_X_DTSTRIM_E_BEGIN_X_DTSTEMP_X_X_X_1
{
	ConfigurationFile = "./Modules/PTH_DTS/InputFiles/PTH_DTS_TEMPRead.csv";
	CtvCapturePins = "IP_CPU::TDO";
	Kill = "DISABLED";
	LevelsTc = "PTH_DTS::DDR_univ_lvl_TS_lvl_SHARED_454FAB810542456F96BE44A40A6D7FC8241A5624609BCC08C80456A065CF045E";
	Patlist = "dts_temp2_fc_plist";
	PinRename = "IP_CPU::TDO";
	TimingsTc = "IP_CPU_BASE::cpu_func_sdr_univ_sta_univ_univ_b100_t100_d100";
}
```

## Input csv Example
### Simple decode
![Simple decode](images/simple_decode.jpg)
### Decode with Math functions and ";" delimiter to support it
#### CSV format
![Simple decode](images/math_sharedStorage_decode.jpg)
#### TXT format
![Simple decode](images/math_sharedStorage_decode_txt.jpg)
## Ituff printing
![ituff print](images/Ituff_info.jpg)