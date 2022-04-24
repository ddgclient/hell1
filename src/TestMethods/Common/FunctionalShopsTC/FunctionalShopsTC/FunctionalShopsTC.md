# DDG Prime FunctionalShopsTC Specification REP

Revision 1.0.0 6/2/2021

## Methodology

This test method performs the shorts & opens testings in both production and engineering mode utilizing walking 1 functional SHOPS pattern

Production: 
Pattern is run with predefined VOX value set in the PinConfigFile JSON file. <br>

Characterization:
Search is done on VOX values for from VOX_LL to VOX_HL. Before each point, test method will update the VOX value of pins through setPinAttributes call. Once a passing point is found, it VOX won't be updated.<br>

The attributes get restored to the original values before moving to next pin.

## Test Instance Parameters

| **Parameter Name** | **Required?** | **Type** | **Values** | **Comments** |
| ------------------ | ------------- | -------- | ---------- | ------------ |
| TestMode           | Yes           | ENUM     |            |Produciton or Characterization |
| PinConfigFile      | Yes           | String   |            |JSON file that sets the VOX limits |


## Exit Ports

The CallbacksRegistrar test method supports the following exit ports:

| **Exit Port** | **Condition**   | **Description**                             |
| ------------- | --------------- | ------------------------------------------- |
| **-2**        | ***Alarm***     | Any alarm condition.|
| **-1**        | ***Error***     | Any software condition error.|
| **0**         | ***Fail***      | Fail condition, failing more than one pin or number of pins overflow(more than 28).|
| **1**         | ***Pass***      | Passing condition.|
| **2-30**      | ***Fail***      | Fail single rail. Failing por is PinIndex+; meaning if you fail first pin exits port 2, second pin port 3 and so on.|

## Additional Dependencies
### Related Test Methods
* PrimeDcTestMethod
## Version tracking


| **Date**       | **Version** | **Author**   | **Comments** |
| -------------- | ----------- | ------------ | ------------ |
| June 2nd, 2021 | 1.0.0       | jhanbaba     |  Initial doc |        