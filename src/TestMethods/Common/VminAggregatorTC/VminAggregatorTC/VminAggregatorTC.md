# DDG Prime VminAggregatorTC Specification REP

Revision 3/8/2022

## Methodology

This test method is intended for Vmin datalogging and DFF at SORT locations where VminForwarding service is not used.<br>
User is required to set a json input file with all the domain, corner, vmin tokens and DFF information.<br>
The test method supports multiple VminTokens per domain+corner and built-in expression handling for Frequency and Vmin values.<br>


## Test Instance Parameters

| **Parameter Name** | **Required?** | **Type** | **Values** | **Comments** |
| ------------------ | ------------- | -------- | ---------- | ------------ |
| InputFile          | Yes           | String   |            |.json Input File.|


## OTPL Sample:
``` Perl
Test VminAggregatorTC InstanceName
{
	InputFile = "SomeFile";	
}
```

## JSON Input File:
Input file consist of a list of Domain+Corner configurations where user can list the tokens to aggregate and/or DFF.
### Parameters:
  - **Domain**: Domain name. i.e. CORE
  - **Corner**: Corner name. i.e. F1, F2, F3
  - **Frequency**: Frequency value. It can take literal values with units or an expression value.
  - **VminExpressions**: List of Lists of expressions for voltage aggregation. Final Vmin is -9999 in case of any fail, highest vmin out of the list or -8888 for untested.
  - **DffToken: (Optional)** Token name for DFF store.

``` JSON
[
    {
        "Domain": "CORE",
        "Corner": "F1",
        "Frequency": "[Collection.Uservar]",
        "VminExpressions": [
            ["[G.U.D.ARR_Core1]", "[G.U.D.FUN_Core1]"],
            ["[G.U.D.ARR_Core0]+0.01", "[G.U.D.FUN_Core0]"]
        ],
        "DffToken": "COREF1"
    },
    {
        "Domain": "CCF",
        "Corner": "F1",
        "Frequency": "'0.8GHz'",
        "VminExpressions": [
            ["[G.U.D.ARR_CCF]", "[G.U.D.FUN_CCF]"]
        ]
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

### ITUFF:
```
2_tname_PVAL_VMINAGG::VMIN_AGG_END_X_X_X_X_P1|CORE@F1
2_strgval_1.200@0.500|-9999
2_tname_PVAL_VMINAGG::VMIN_AGG_END_X_X_X_X_P1|CCF@F1
2_strgval_0.800@0.800
```

##### Supported functions:
https://github.com/ncalc/ncalc/wiki/Functions

##### Additional functions:
  - **ToInt32**: As the name suggests it converts a double or string to Integer.
  - **ToDouble**: As the name suggests it converts an int or string to Double.
  - **Random**: Generates a random double value between 0 and 1.

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
| March 8, 2022  | 1.0.0       | fmurillo     |  Initial doc|       