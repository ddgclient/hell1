# DDG Prime PatConfig Test Class  

## Methodology
PatConfigTC is another PatConfig option to apply PatConfig using dynamic Tag or Dynamic data using our built-in NCalc Expression evaluator.

## Test Instance Parameters

| **Parameter Name** | **Required?** | **Type** | **Values** | **Comments** |
| ------------------ | ------------- | -------- | ---------- | ------------ |
| InputFile          | Yes           | String   |            | .json Input File.|
| PlistRegEx         | No            | String   |            | Optional plist regular expression.|
| Tags               | Yes           | String   |            | List of PatConfig tags to apply. Values can be Literal or Dynamically assigned tokens.|


## OTPL Sample:
``` Perl
Test PatConfigTC InstanceName
{
	InputFile = "SomeFile.json";	
	PlistRegEx = ".*MyPRECATPlist.*";
	Tags = "Tag1,[G.U.S.SharedStorageToken],[Collection.Uservar]";
}
```

## JSON Input File:
Input file consist of a list of Tags with the corresponding PatConfig and data values.
### Parameters:
  - **Tag**: Tag name to be used as filter to select PatConfigs to be applied.
  - **PatConfig**: PatConfig as in your ALEPH_FILE. This parameter does not support expression evaluation.
  - **Data**: Data to be applied. Expecting string value and it supports Expression evaluator for dynamic tokens using SharedSotrage, Uservar or DFF (including functions for binary conversion functions, reverse and padding).

``` JSON
[
    {
        "Tag": "Tag1",
        "PatConfig": "conf1",
        "Data": "GetPatSymbolString('0d11',8)"
    },
    {
        "Tag": "Tag2",
        "PatConfig": "conf2",
        "Data": "Reverse(GetPatSymbolString('0xC', 4))"
    },
    {
        "Tag": "Tag3",
        "PatConfig": "conf3",
        "Data": "'01100'"
    },
    {
        "Tag": "Tag4",
        "PatConfig": "conf3",
        "Data": "'01100'"
    },
    {
        "Tag": "Tag5",
        "PatConfig": "conf3",
        "Data": "GetPatSymbolString('0b01011111', 8)"
    }    
]
```

### Expression Engine:
This test method supports Ncalc as expression engine. https://github.com/ncalc/ncalc 
It is able to evaluate simple mathematical expression using SharedStorage, Uservar and/or DFF tokens.
All tokens must be specified using '[]'. Value will be evaluated in the following order:

   - **SharedStorage**: Should be of the form G.context.type.Token where context is L for Lot, U for DUT/Unit or I for IP, and type is S for string, D for double or I for integer.
   - **UserVar**: String, Double and Integer. Always using full collection.name format with the full IP/Module scoping on the collection.
   - **DFF**: Using token name. It will be read from the current optype and die id.

##### Supported functions:
https://github.com/ncalc/ncalc/wiki/Functions

##### Key Additional functions:
  - **GetPatSymbolString**:
    - Hex: GetPatSymbolString('0xC', 4)
    - Binary1: GetPatSymbolString('0b0100', 4)
    - Binary2: GetPatSymbolString('1111', 4)
    - Decimal: GetPatSymbolString('0d8', 4)
    - Strobes: GetPatSymbolString('LHLH', 4)
    - Strobes: GetPatSymbolString('LHLH', 4)
    - Reverse: Reverse(GetPatSymbolString('0b0100', 4))
   

## Exit Ports

The CallbacksRegistrar test method supports the following exit ports:

| **Exit Port** | **Condition**   | **Description**              |
| ------------- | --------------- | ---------------------------- |
| **-2**        | ***Alarm***     | Any alarm condition          |
| **-1**        | ***Error***     | Any software condition error |
| **0**         | ***Fail***      | Failing condition            |
| **1**         | ***Pass***      | Passed                       |

## Version tracking

| **Date**       | **Version** | **Author**   | **Comments** |
| -------------- | ----------- | ------------ | ------------ |
| April 19, 2022 | 1.0.0       | fmurillo     |  Initial doc |       