# DDG Prime Functional Test Class  
#### Latest: 2/22/22 (fmurillo)  

# Introduction
DDGFunctionalTC is a simple extension over Prime Function to add 1)Print previous label, 2) Capture CTV tokens and 3) Assign different port for AMBLE fails.
# Methodology
Overriding IFunctionalExtensions.ProcessCtvPerPin and IFunctionalExtensions.ProcessFailures.

# Additional Parameters
## CapturedDataTokens
List os SharedStorage tokens to record per-pin CTV data. Number of tokens must match number of captured pins.
## PrintPreviousLabel
Enables/Disabled ITUFF printing for first capture fail.
# Ports
| Exit Port       | Condition   | Description |   
| -----------     | ----------- | ----------- |    
| -2  | Alarm | Any HW alarm condition |    
| -1  | Error | Any software error |   
| 0   | Fail  | At least one point failed in the shmoo. |   
| 1   | Pass  | Pass. |   
| 2   | Fail  | Fail amble. |   
| 3   | Fail  | Fail (unused). |
| 4   | Fail  | Alarm. |   
| 5   | Fail  | DTS. |   
