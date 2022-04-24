# DDG Prime PTHGetSetGsdsDffTC
### Rev 0

## Introduction
This TestMethod is used set/get GSDS or DFF.

## Methodology
1. Setup the **ConfigurationFile** as JSON.  More info on the JSON property parameters below.
2. Each GSDS2DFF or DFF2GSDS is considered as an object and requires the property parameters.

## Errors and Exceptions
- Throws Exception
   - if both **GSDS2DFF** and **DFF2GSDS** are set to true.
   - if **Delimiter** is required but not specified 
   - if **Delimiter** is greater  1. Allows only 1 delimiter
   - if **GSDSScopeType, DFF, DFFOpType** parameters have more than 1 value.
   - if **GSDS** already exits while converting **DFF** to **GSDS** to avoid overwriting of the GSDS
   - if **GSDSScopeType** does not follow the required format - **scope.type**
     - scope = U for unit/dut, L for lot
     - type = S for string, D for double, or I for integer.

## Parameters
### Template parameters
#### OPType
- To choose the type of operation. This enables the use of single json file for both GSDS2DFF and DFF2GSDS. Two options of enum datatype.
  - DFF2GSDS (default)
    - converts all the operations in json with "DFF2GSDS": true
  - GSDS2DFF
    - converts all the operations in json with "GSDS2DFF": true
#### ConfigurationFile
- Input JSON based on which the DFF or GSDS are decoded.
### JSON property parameters
- **GSDSList**
  - **DFF2GSDS** mode - list of GSDS tokens to get the values from DFF  
  - **GSDS2DFF** mode - list of GSDS tokens to be copied over to DFF  
- **GSDSScopeType** - format of GSDS scope and type  
- **DFF** - Name of the DFF to be to set or value to get  
- **DFFOpType**  
  - Location from which DFF value is to be set/get.  
  - eg: SORT, PBIC_DAB  
- **GSDS2DFF** - bool parameter to enable conversion of GSDS to DFF, defaults to false.  
- **DFF2GSDS** - bool parameter to enable conversion of DFF to GSDS, defaults to false.  
- **Delimiter** - single string value
- **PrintDFF** - bool parameter to enable conversion of GSDS to DFF, defaults to false.  
- **GSDS2DFFAllowedList** - list of static values allowed to be in the **GSDSList** in **GSDS2DFF** mode  
- **SearchReplace** - Dictionary of Search/Replace pairs. Should be valid C# Regular expressions. Can be used for simple data/format changes. In DFF2GSDS mode will be run on the DFF data after being read. In GSDS2DFF mode will be run after the GSDS is concatenated together.   

## Ports
```
[Returns(1, PortType.Pass, "Pass!")]
[Returns(0, PortType.Fail, "Fail!")]
[Returns(2, PortType.Fail, "Failed to Convert DFF to GSDS!")]
[Returns(3, PortType.Fail, "Failed to Convert GSDS to DFF!")]
[Returns(4, PortType.Fail, "No valid operation. Check OPType!")]
```
## mtpl Example

```
Test PTHGetSetGsdsDffTC PTHGetSetGsdsDff_tst
{
	ConfigurationFile = "./Modules/PTH_DTS/InputFiles/PTHGetSetDffGsds.json";
	LogLevel = "DISABLED";
}
```

## Input JSON Example
### Single DFF to GSDS
![Single DFF2GSDS](images/single_DFF2GSDS.jpg)
#### Ituff print
```
0_tname_IP_CPU::PTH_DTS::PTHGetSetGsdsDff_DFF2GSDS_PREDTS
0_strgval_-10
```
### Multiple DFF to GSDS
![Multiple DFF2GSDS](images/multiple_DFF2GSDS.jpg)
#### Ituff print
```
0_tname_IP_CPU::PTH_DTS::PTHGetSetGsdsDff_DFF2GSDS_TSDT0
0_strgval_446|446|445|451|449|450|451|451|450|447|445|445|449|449|449|451|453|449|450|449|449|449|445|445|445|445|451|452|453|454|453|454|454|446|447|445
```
### Single GSDS to DFF
![Single GSDS2DFF](images/single_GSDS2DFF.jpg)
#### Ituff print
```
0_tname_IP_CPU::PTH_DTS::PTHGetSetGsdsDff_GSDS2DFF_PRETS
0_strgval_99.8|dummy|1|1001|0x8
```
### Multiple GSDS to DFF
![Multiple GSDS2DFF](images/multiple_GSDS2DFF.jpg)
#### Ituff print
```
0_tname_IP_CPU::PTH_DTS::PTHGetSetGsdsDff_GSDS2DFF_TCSL0
0_strgval_2183|2249|2271|2271|2205|2271|2183|2205|2205|2271|2318|2295|2205|2226|2226|2183|2205|2249|2295|2295|2295|2295|2318|2318|2342|2342|2342|2318|2318|2318|2318|2342|2318|2183|2162|2205
```

### Simple DFF to GSDS with Search/Replace
The full G.U.S.token format can be used within the "GSDSList" field instead of specifying "GSDSScopeType".  
The "DFF" field can be optype:token format instead of specifying the "DFFOpType" field.  

```json
{
    "GSDSList": ["G.U.S.VminCF5CR_Test"],
    "DFF" : "SORT:CF5CR",
    "DFF2GSDS" : true,
    "SearchReplace": { "v": "," }
}
```

If SORT.CF5CR is "1.0v1.1|-8.888v-8.888|0.77v0.8", then G.U.S.VminCF5CR_Test will be written to "1.0,1.1|-8.888,-8.888|0.77,0.8".  

