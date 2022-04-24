<h1>Prime Test-Method Specification REP</h1>
Revision 1.0.0

June 2021

[[_TOC_]]

## Methodology
The DUSTI_StopLogging test method was created to work with the DUSTI hardware in the SDX cells.  The method performs the following functions:

 Stops the monitoring of the designated pin to capture DTS data using DUSTI.


The DUSTI test method has been created to assist in the Digital Thermal Sensor (DTS) data collection during Singulated Die Test (SDT). 

This will allow us to continuously monitor the temperature during test, outside of the data provided from the Thermal Diode (TD).  

Current methodologies use the TD to monitor temperature during performance.  Due to the side of the TD, they are typically placed on the periphery of the cores.  

The DTS is smaller, and able to fit closer to the most active portion of the device, allowing for a more respective measurement of the temperature.

This will work on testers with and without the DUSTI installed.  The ports are defined as such to allow manufacturing to continue with its flows.

## Test Instance Parameters



| **Parameter Name** |   **Required?**    | **Type** |                          **Values**                                | **Default Value** | **Comments** 										 |
| ------------------ | ------------------ | -------- | ------------------------------------------------------------------ | ----------------- | -----------------------------------------------------|
| LevelsOption       |       Yes          |  String  |  User selectable.                                                  |     None          |              										 |
| PinName            |       Yes          |  String  |  User defined pin name from soc file.							  |     None          |             										 |
| ForceFlow          |       Yes          |  String  |  True, False														  |     None          |  Should set to True unless for Debugging             |
| AttemptCount       |       No			  |  String  |	Number of attempts to send I2C commands.						  |     None          | Numeric value										 |
| AckWaitTime        |       No			  |  String  |	Amount of time to wait for an I2C acknowledgement.				  |     None          | Numeric value										 |
| FpgaWaitTime       |       No			  |  String  |	Amount of time to wait for the FPGA programming to complete.	  |     None          | Numeric value										 |


**Notes:**
- Stop Logging will be executed once per DUT, and can be placed in the Test Plan Stop flow.

- Console Example:
There are no regular console outputs.  Only console data for debug purposes.

## Datalog Output
Currently, there is no datalog output, as the DUSTI runs in the back ground and transmits the data through the DTC.  



| **Exit Port** | **Condition**   | **Description**              		 |
| ------------- | --------------- | ------------------------------------ |
| **0**         | ***Fail***      | Failing condition            		 |
| **1**         | ***Pass***      | Passing condition            		 |
| **2**         | ***Pass***      | Passing condition/force flow 		 |
| **4**         | ***Pass***      | Passing condition/DUSTI not present  |
  
  

  
  
## Additional Dependencies

N/A

## Version tracking

| **Date**       | **Version** | **Author**   | **Comments** |
| -------------- | ----------- | ------------ | ------------ |
| October, 2021  | 1.0.0       | Matthew Brown|              |
|                |             |              |              |

## Acronyms

Definition of acronyms used in this document:

  - **REP**: P**r**ime T**e**st-Method S**p**ecification
  - **HDMT**: High Density Modular Tester
  - **TPL**: Test Programming Language
  - **TOS**: Test Operating System
  - **DFF**: Data Feed Forward


