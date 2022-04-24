﻿<h1>Prime Test-Method Specification REP</h1>
Revision 1.0.0

June 2021

[[_TOC_]]

## Methodology
The DUSTI_Configure test method was created to work with the DUSTI hardware in the SDX cells.  The method performs multiple functions:

1.) Initializes the onboard UART which allows comms between the tester and DUSTI.
2.) Transmits the contents of the prescribed XML file over I2C to the DUSTI board to program it to the correct state.
3.) Initialized the calibration of the on board FPGA.


The DUSTI test method has been created to assist in the Digital Thermal Sensor (DTS) data collection during Singulated Die Test (SDT). 

This will allow us to continuously monitor the temperature during test, outside of the data provided from the Thermal Diode (TD).  

Current methodologies use the TD to monitor temperature during performance.  Due to the size of the TD, they are typically placed on the periphery of the cores.  

The DTS is smaller, and able to fit closer to the most active portion of the device, allowing for a more respective measurement of the temperature.

This will work on testers with and without the DUSTI installed.  The ports are defined as such to allow manufacturing to continue with its flows.

For DUSTI usage,a DUSTI_Configure instance needs to be placed in the Init flow of the given test program.  The usage will also require instances of DUSTI_StartLogging and DUSTI_StopLogging.

## Test Instance Parameters



| **Parameter Name** |   **Required?**    | **Type** |                          **Values**                                | **Default Value** | **Comments** 										 |
| ------------------ | ------------------ | -------- | ------------------------------------------------------------------ | ----------------- | -----------------------------------------------------|
| XMLInputFile       |       Yes          |  String  |  User defined path                                                 |      None		  |            										     |
| PlistOption        |       Yes          |  String  |  User selectable.                                                  |     None          |              										 |
| LevelsOption       |       Yes          |  String  |  User selectable.                                                  |     None          |              										 |
| TimingsOption      |       Yes          |  String  |  User selectable.                                                  |     None          |              										 |
| PinName            |       Yes          |  String  |  User defined pin name from soc file.							  |     None          |             										 |
| ForceFlow          |       Yes          |  String  |  True, False														  |     None          |  Should set to True unless for Debugging             |
| AttemptCount       |       No			  |  String  |	Number of attempts to send I2C commands.						  |     None          | Numeric value										 |
| AckWaitTime        |       No			  |  String  |	Amount of time to wait for an I2C acknowledgement.				  |     None          | Numeric value										 |
| FpgaWaitTime       |       No			  |  String  |	Amount of time to wait for the FPGA programming to complete.	  |     None          | Numeric value										 |
| McuReset           |       Yes          |  String  |  True, False														  |     None          |   Should set to True unless for Debugging            |


**Notes:**
- Initialization of UART, transmission of XML, FPGA initializeation and calibration only needs to take place once, and should be placed in the init flow.

- Console Example:
There are no regular console outputs.  Only console data for debug purposes.

## Datalog Output
Currently, there is no datalog output, as the DUSTI runs in the back ground and transmits the data through the DPC.  



## Exit Ports


| **Exit Port** | **Condition**   | **Description**              		 |
| ------------- | --------------- | ------------------------------------ |
| **0**         | ***Fail***      | Failing condition            		 |
| **1**         | ***Pass***      | Passing condition            		 |
| **2**         | ***Pass***      | Passing condition/force flow 		 |
| **4**         | ***Pass***      | Passing condition/DUSTI not present  |
  
  
## Input XML example

<!-- Notes and Assumptions: -->
<!-- - Assumes NULL is 0x0, there is a risk an IR or DR is 0x0.  How to handle (do we pass in additional context bytes)? -->
<!-- - Agreed on DTS offsets.  -->
<!-- - Outline below is flat. -->
<!-- # Mode1 = SNOOP_TAP_SINGLE_IR -->
<!-- # Mode2 = SNOOP_TAPLINK -->
<!-- # Mode3 = ALTPIN_LISTEN -->
<DustiConfiguration ConfigurationVersion="1.0.0">
	<Product></Product>
	<!-- SNOOP_TAP_SINGLE_IR[0x1], TAPLINK_SNOOP[0x2], or ALTPIN_LISTEN[0x3] -->
	<PatternMatchMode Type="hex">2</PatternMatchMode>			
	<!-- Cell Parallelism, x1[0x1], x2[0x2] -->
	<CellParallelism Type="hex">2</CellParallelism>			
	<!-- Reserved Bytes (e.g. 0xF) -->
	<Reserved0 Type="hex">0</Reserved0>			
	<Reserved1 Type="hex">0</Reserved1>			
	<Reserved2 Type="hex">0</Reserved2>			
	<Reserved3 Type="hex">0</Reserved3>			
	<!-- Network Settings -->
	<Networking>
		<IpConfig Subnet="0.0.0.0" IpAddress="192.168.200.x" />
		<TidiConnection Server="192.168.200.206" Port="5858" />
	</Networking>
	<SnoopyDigitalPotentiometers>
		<!-- Value is two bytes per dpot, range is lower byte 0x0 (no update) to 0xFF (255)  -->
		<Dpot Id="1" Name="DUT1 TDI" DpotValue="0x50"/>		
		<Dpot Id="2" Name="DUT1 TMS" DpotValue="0x50"/>		
		<Dpot Id="3" Name="DUT1 TCK" DpotValue="0x30"/>		
		<Dpot Id="4" Name="DUT1 TDO" DpotValue="0x50"/>		
		<Dpot Id="5" Name="DUT1 UNUSED" DpotValue="0x0"/>		
		<Dpot Id="6" Name="DUT1 UNUSED" DpotValue="0x0"/>
		<Dpot Id="7" Name="DUT2 TDI" DpotValue="0x50"/>		
		<Dpot Id="8" Name="DUT2 TMS" DpotValue="0x50"/>		
		<Dpot Id="9" Name="DUT2 TCK" DpotValue="0x30"/>		
		<Dpot Id="10" Name="DUT2 TDO" DpotValue="0x50"/>		
		<Dpot Id="11" Name="DUT2 UNUSED" DpotValue="0x0"/>		
		<Dpot Id="12" Name="DUT2 UNUSED" DpotValue="0x0"/>
		<!-- Choices are "Static"=00h or "Dynamic"=01h, one byte  -->
		<TrainingStateDpots Value="Static" />		
	</SnoopyDigitalPotentiometers>
	<Mode Id="1" Name="SNOOP_TAP_SINGLE_IR">
		<!-- variable 8 to 16 bits -->
		<TapIRInstructionLength Type="int" />
		<!-- variable 8 to 16 bits -->
		<DtsDataSize Type="int" />
		<Dts Id="1">
			<TapIRInstruction/>
			<Diode Id="1" Name="" TapClksToDtsCurrent="" X="" Y=""/>
			<Diode Id="2" Name="" TapClksToDtsCurrent="" X="" Y=""/>
			<Diode Id="3" Name="" TapClksToDtsCurrent="" X="" Y=""/>
			<Diode Id="4" Name="" TapClksToDtsCurrent="" X="" Y=""/>
			<Diode Id="5" Name="" TapClksToDtsCurrent="" X="" Y=""/>
			<Diode Id="6" Name="" TapClksToDtsCurrent="" X="" Y=""/>
			<Diode Id="7" Name="" TapClksToDtsCurrent="" X="" Y=""/>
			<Diode Id="8" Name="" TapClksToDtsCurrent="" X="" Y=""/>
		</Dts>
		<!-- Up to 32 -->
	</Mode>
	<Mode Id="2" Name="SNOOP_TAPLINK">
		<!-- variable 8 to 16 bits -->
		<FirstTapIRInstructionLength Type="int">11<FirstTapIRInstructionLength>
		<!-- variable 8 to 16 bits -->
		<FirstTapDRLength Type="int" >7<FirstTapDRLength>
		<!-- variable 8 to 16 bits -->
		<SecondTapIRInstructionLength Type="int" >11<SecondTapIRInstructionLength>
		<!-- variable 8 to 16 bits -->
		<DtsDataSize Type="int" >8<DtsDataSize>
		<Dts Id="1">
			<FirstTapIRInstruction Type="hex" >188<FirstTapIRInstruction> 
			<FirstTapDRInstruction Type="hex" >32<FirstTapDRInstruction>
			<SecondTapIRInstruction Type="hex" >189<SecondTapIRInstruction>
			<!--  Unique IR for data file -->
			<!-- DTS offsets in second DR.  0 means no DTS value.  1 means first bit in DR shift. -->
			<Diode Id="1" Name="CCU_TS_IPU_2" TapDrOffsetToDtsRead="131" X="" Y=""/>
			<Diode Id="2" Name="CCU_TS_IPU_1" TapDrOffsetToDtsRead="122" X="" Y=""/>
			<Diode Id="3" Name="CCU_TS_IPU_0" TapDrOffsetToDtsRead="113" X="" Y=""/>
			<Diode Id="4" Name="" TapDrOffsetToDtsRead="" X="" Y=""/>
			<Diode Id="5" Name="" TapDrOffsetToDtsRead="" X="" Y=""/>
			<Diode Id="6" Name="" TapDrOffsetToDtsRead="" X="" Y=""/>
			<Diode Id="7" Name="" TapDrOffsetToDtsRead="" X="" Y=""/>
			<Diode Id="8" Name="" TapDrOffsetToDtsRead="" X="" Y=""/>
		</Dts>
		<Dts Id="2">
			<FirstTapIRInstruction Type="hex" >28</FirstTapIRInstruction> 
			<FirstTapDRInstruction Type="hex" >32</FirstTapDRInstruction>
			<SecondTapIRInstruction Type="hex" >29</SecondTapIRInstruction>
			<!--  Unique IR for data file -->
			<!-- DTS offsets in second DR.  0 means no DTS value.  1 means first bit in DR shift. -->
			<Diode Id="1" Name="CCU_TS_SA_0" TapDrOffsetToDtsRead="113" X="" Y=""/>
			<Diode Id="2" Name="" TapDrOffsetToDtsRead="" X="" Y=""/>
			<Diode Id="3" Name="" TapDrOffsetToDtsRead="" X="" Y=""/>
			<Diode Id="4" Name="" TapDrOffsetToDtsRead="" X="" Y=""/>
			<Diode Id="5" Name="" TapDrOffsetToDtsRead="" X="" Y=""/>
			<Diode Id="6" Name="" TapDrOffsetToDtsRead="" X="" Y=""/>
			<Diode Id="7" Name="" TapDrOffsetToDtsRead="" X="" Y=""/>
			<Diode Id="8" Name="" TapDrOffsetToDtsRead="" X="" Y=""/>
		</Dts>
		<Dts Id="3">
			<FirstTapIRInstruction Type="hex" >950</FirstTapIRInstruction> 
			<FirstTapDRInstruction Type="hex" >32</FirstTapDRInstruction>
			<SecondTapIRInstruction Type="hex" >951</SecondTapIRInstruction>
			<!--  Unique IR for data file -->
			<!-- DTS offsets in second DR.  0 means no DTS value.  1 means first bit in DR shift. -->
			<Diode Id="1" Name="CORE0_DTS0_2" TapDrOffsetToDtsRead="131" X="" Y=""/>
			<Diode Id="2" Name="CORE0_DTS0_1" TapDrOffsetToDtsRead="122" X="" Y=""/>
			<Diode Id="3" Name="CORE0_DTS0_0" TapDrOffsetToDtsRead="113" X="" Y=""/>
			<Diode Id="4" Name="" TapDrOffsetToDtsRead="" X="" Y=""/>
			<Diode Id="5" Name="" TapDrOffsetToDtsRead="" X="" Y=""/>
			<Diode Id="6" Name="" TapDrOffsetToDtsRead="" X="" Y=""/>
			<Diode Id="7" Name="" TapDrOffsetToDtsRead="" X="" Y=""/>
			<Diode Id="8" Name="" TapDrOffsetToDtsRead="" X="" Y=""/>
		</Dts>
		<Dts Id="4">
			<FirstTapIRInstruction Type="hex" >990</FirstTapIRInstruction> 
			<FirstTapDRInstruction Type="hex" >32</FirstTapDRInstruction>
			<SecondTapIRInstruction Type="hex" >991</SecondTapIRInstruction>
			<!--  Unique IR for data file -->
			<!-- DTS offsets in second DR.  0 means no DTS value.  1 means first bit in DR shift. -->
			<Diode Id="1" Name="CORE1_DTS0_2" TapDrOffsetToDtsRead="131" X="" Y=""/>
			<Diode Id="2" Name="CORE1_DTS0_1" TapDrOffsetToDtsRead="122" X="" Y=""/>
			<Diode Id="3" Name="CORE1_DTS0_0" TapDrOffsetToDtsRead="113" X="" Y=""/>
			<Diode Id="4" Name="" TapDrOffsetToDtsRead="" X="" Y=""/>
			<Diode Id="5" Name="" TapDrOffsetToDtsRead="" X="" Y=""/>
			<Diode Id="6" Name="" TapDrOffsetToDtsRead="" X="" Y=""/>
			<Diode Id="7" Name="" TapDrOffsetToDtsRead="" X="" Y=""/>
			<Diode Id="8" Name="" TapDrOffsetToDtsRead="" X="" Y=""/>
		</Dts>
		<Dts Id="5">
			<FirstTapIRInstruction Type="hex" >9D0</FirstTapIRInstruction> 
			<FirstTapDRInstruction Type="hex" >32</FirstTapDRInstruction>
			<SecondTapIRInstruction Type="hex" >9D1</SecondTapIRInstruction>
			<!--  Unique IR for data file -->
			<!-- DTS offsets in second DR.  0 means no DTS value.  1 means first bit in DR shift. -->
			<Diode Id="1" Name="CORE2_DTS0_2" TapDrOffsetToDtsRead="131" X="" Y=""/>
			<Diode Id="2" Name="CORE2_DTS0_1" TapDrOffsetToDtsRead="122" X="" Y=""/>
			<Diode Id="3" Name="CORE2_DTS0_0" TapDrOffsetToDtsRead="113" X="" Y=""/>
			<Diode Id="4" Name="" TapDrOffsetToDtsRead="" X="" Y=""/>
			<Diode Id="5" Name="" TapDrOffsetToDtsRead="" X="" Y=""/>
			<Diode Id="6" Name="" TapDrOffsetToDtsRead="" X="" Y=""/>
			<Diode Id="7" Name="" TapDrOffsetToDtsRead="" X="" Y=""/>
			<Diode Id="8" Name="" TapDrOffsetToDtsRead="" X="" Y=""/>
		</Dts>
		<Dts Id="6">
			<FirstTapIRInstruction Type="hex" >A10</FirstTapIRInstruction> 
			<FirstTapDRInstruction Type="hex" >32</FirstTapDRInstruction>
			<SecondTapIRInstruction Type="hex" >A11</SecondTapIRInstruction>
			<!--  Unique IR for data file -->
			<!-- DTS offsets in second DR.  0 means no DTS value.  1 means first bit in DR shift. -->
			<Diode Id="1" Name="CORE3_DTS0_2" TapDrOffsetToDtsRead="131" X="" Y=""/>
			<Diode Id="2" Name="CORE3_DTS0_1" TapDrOffsetToDtsRead="122" X="" Y=""/>
			<Diode Id="3" Name="CORE3_DTS0_0" TapDrOffsetToDtsRead="113" X="" Y=""/>
			<Diode Id="4" Name="" TapDrOffsetToDtsRead="" X="" Y=""/>
			<Diode Id="5" Name="" TapDrOffsetToDtsRead="" X="" Y=""/>
			<Diode Id="6" Name="" TapDrOffsetToDtsRead="" X="" Y=""/>
			<Diode Id="7" Name="" TapDrOffsetToDtsRead="" X="" Y=""/>
			<Diode Id="8" Name="" TapDrOffsetToDtsRead="" X="" Y=""/>
		</Dts>
		<Dts Id="7">
			<FirstTapIRInstruction Type="hex" >1AC</FirstTapIRInstruction> 
			<FirstTapDRInstruction Type="hex" >32</FirstTapDRInstruction>
			<SecondTapIRInstruction Type="hex" >1AD</SecondTapIRInstruction>
			<!--  Unique IR for data file -->
			<!-- DTS offsets in second DR.  0 means no DTS value.  1 means first bit in DR shift. -->
			<Diode Id="1" Name="FABRIC_TS_TPC_1" TapDrOffsetToDtsRead="122" X="" Y=""/>
			<Diode Id="2" Name="FABRIC_TS_TPC_0" TapDrOffsetToDtsRead="113" X="" Y=""/>
			<Diode Id="3" Name="" TapDrOffsetToDtsRead="" X="" Y=""/>
			<Diode Id="4" Name="" TapDrOffsetToDtsRead="" X="" Y=""/>
			<Diode Id="5" Name="" TapDrOffsetToDtsRead="" X="" Y=""/>
			<Diode Id="6" Name="" TapDrOffsetToDtsRead="" X="" Y=""/>
			<Diode Id="7" Name="" TapDrOffsetToDtsRead="" X="" Y=""/>
			<Diode Id="8" Name="" TapDrOffsetToDtsRead="" X="" Y=""/>
		</Dts>
		<Dts Id="8">
			<FirstTapIRInstruction Type="hex" >1F0</FirstTapIRInstruction> 
			<FirstTapDRInstruction Type="hex" >32</FirstTapDRInstruction>
			<SecondTapIRInstruction Type="hex" >1F1</SecondTapIRInstruction>
			<!--  Unique IR for data file -->
			<!-- DTS offsets in second DR.  0 means no DTS value.  1 means first bit in DR shift. -->
			<Diode Id="1" Name="GT_SLICE0_TS0_3" TapDrOffsetToDtsRead="140" X="" Y=""/>
			<Diode Id="2" Name="GT_SLICE0_TS0_2" TapDrOffsetToDtsRead="131" X="" Y=""/>
			<Diode Id="3" Name="GT_SLICE0_TS0_1" TapDrOffsetToDtsRead="122" X="" Y=""/>
			<Diode Id="4" Name="GT_SLICE0_TS0_0" TapDrOffsetToDtsRead="113" X="" Y=""/>
			<Diode Id="5" Name="" TapDrOffsetToDtsRead="" X="" Y=""/>
			<Diode Id="6" Name="" TapDrOffsetToDtsRead="" X="" Y=""/>
			<Diode Id="7" Name="" TapDrOffsetToDtsRead="" X="" Y=""/>
			<Diode Id="8" Name="" TapDrOffsetToDtsRead="" X="" Y=""/>
		</Dts>
		<Dts Id="9">
			<FirstTapIRInstruction Type="hex" >280</FirstTapIRInstruction> 
			<FirstTapDRInstruction Type="hex" >32</FirstTapDRInstruction>
			<SecondTapIRInstruction Type="hex" >281</SecondTapIRInstruction>
			<!--  Unique IR for data file -->
			<!-- DTS offsets in second DR.  0 means no DTS value.  1 means first bit in DR shift. -->
			<Diode Id="1" Name="GT_SLICE0_TS1_3" TapDrOffsetToDtsRead="140" X="" Y=""/>
			<Diode Id="2" Name="GT_SLICE0_TS1_2" TapDrOffsetToDtsRead="131" X="" Y=""/>
			<Diode Id="3" Name="GT_SLICE0_TS1_1" TapDrOffsetToDtsRead="122" X="" Y=""/>
			<Diode Id="4" Name="GT_SLICE0_TS1_0" TapDrOffsetToDtsRead="113" X="" Y=""/>
			<Diode Id="5" Name="" TapDrOffsetToDtsRead="" X="" Y=""/>
			<Diode Id="6" Name="" TapDrOffsetToDtsRead="" X="" Y=""/>
			<Diode Id="7" Name="" TapDrOffsetToDtsRead="" X="" Y=""/>
			<Diode Id="8" Name="" TapDrOffsetToDtsRead="" X="" Y=""/>
		</Dts>
		<Dts Id="10">
			<FirstTapIRInstruction Type="hex" >75C</FirstTapIRInstruction> 
			<FirstTapDRInstruction Type="hex" >32</FirstTapDRInstruction>
			<SecondTapIRInstruction Type="hex" >75D</SecondTapIRInstruction>
			<!--  Unique IR for data file -->
			<!-- DTS offsets in second DR.  0 means no DTS value.  1 means first bit in DR shift. -->
			<Diode Id="1" Name="GT_SLICE0_TS2_2" TapDrOffsetToDtsRead="131" X="" Y=""/>
			<Diode Id="2" Name="GT_SLICE0_TS2_1" TapDrOffsetToDtsRead="122" X="" Y=""/>
			<Diode Id="3" Name="GT_SLICE0_TS2_0" TapDrOffsetToDtsRead="113" X="" Y=""/>
			<Diode Id="4" Name="" TapDrOffsetToDtsRead="113" X="" Y=""/>
			<Diode Id="5" Name="" TapDrOffsetToDtsRead="" X="" Y=""/>
			<Diode Id="6" Name="" TapDrOffsetToDtsRead="" X="" Y=""/>
			<Diode Id="7" Name="" TapDrOffsetToDtsRead="" X="" Y=""/>
			<Diode Id="8" Name="" TapDrOffsetToDtsRead="" X="" Y=""/>
		</Dts>
		<Dts Id="11">
			<FirstTapIRInstruction Type="hex" >260</FirstTapIRInstruction> 
			<FirstTapDRInstruction Type="hex" >32</FirstTapDRInstruction>
			<SecondTapIRInstruction Type="hex" >261</SecondTapIRInstruction>
			<!--  Unique IR for data file -->
			<!-- DTS offsets in second DR.  0 means no DTS value.  1 means first bit in DR shift. -->
			<Diode Id="1" Name="GT_SLICE3_TS2_2" TapDrOffsetToDtsRead="131" X="" Y=""/>
			<Diode Id="2" Name="GT_SLICE3_TS2_1" TapDrOffsetToDtsRead="122" X="" Y=""/>
			<Diode Id="3" Name="GT_SLICE3_TS2_0" TapDrOffsetToDtsRead="113" X="" Y=""/>
			<Diode Id="4" Name="" TapDrOffsetToDtsRead="113" X="" Y=""/>
			<Diode Id="5" Name="" TapDrOffsetToDtsRead="" X="" Y=""/>
			<Diode Id="6" Name="" TapDrOffsetToDtsRead="" X="" Y=""/>
			<Diode Id="7" Name="" TapDrOffsetToDtsRead="" X="" Y=""/>
			<Diode Id="8" Name="" TapDrOffsetToDtsRead="" X="" Y=""/>
		</Dts>
		<Dts Id="12">
			<FirstTapIRInstruction Type="hex" >740</FirstTapIRInstruction> 
			<FirstTapDRInstruction Type="hex" >32</FirstTapDRInstruction>
			<SecondTapIRInstruction Type="hex" >741</SecondTapIRInstruction>
			<!--  Unique IR for data file -->
			<!-- DTS offsets in second DR.  0 means no DTS value.  1 means first bit in DR shift. -->
			<Diode Id="1" Name="GT_UNSLICE_TS_2" TapDrOffsetToDtsRead="131" X="" Y=""/>
			<Diode Id="2" Name="GT_UNSLICE_TS_1" TapDrOffsetToDtsRead="122" X="" Y=""/>
			<Diode Id="3" Name="GT_UNSLICE_TS_0" TapDrOffsetToDtsRead="113" X="" Y=""/>
			<Diode Id="4" Name="" TapDrOffsetToDtsRead="113" X="" Y=""/>
			<Diode Id="5" Name="" TapDrOffsetToDtsRead="" X="" Y=""/>
			<Diode Id="6" Name="" TapDrOffsetToDtsRead="" X="" Y=""/>
			<Diode Id="7" Name="" TapDrOffsetToDtsRead="" X="" Y=""/>
			<Diode Id="8" Name="" TapDrOffsetToDtsRead="" X="" Y=""/>
		</Dts>
	<!-- Up to 32 -->
	</Mode>
	<Mode Id="3" Name="ALTPIN_LISTEN">
		<!-- variable 8 to 16 bits -->
		<TapHeaderLength Type="int" />
		<!-- variable 8 to 16 bits -->
		<DtsIdLength Type="int" />
		<!-- variable 8 to 16 bits -->
		<DtsDataSize  Type="int" />
		<Packet Id="1">
			<HeaderId Type="hex" />
			<DtsIdName Type="hex" />
			<!-- # TCKs relative to last DtsId bit falling edge? -->
			<Diode Id="1" Name="" TapClksToDtsCurrent="" X="" Y=""/> 
			<Diode Id="2" Name="" TapClksToDtsCurrent="" X="" Y=""/> 
			<Diode Id="3" Name="" TapClksToDtsCurrent="" X="" Y=""/> 
			<Diode Id="4" Name="" TapClksToDtsCurrent="" X="" Y=""/> 
			<Diode Id="5" Name="" TapClksToDtsCurrent="" X="" Y=""/> 
			<Diode Id="6" Name="" TapClksToDtsCurrent="" X="" Y=""/> 
			<Diode Id="6" Name="" TapClksToDtsCurrent="" X="" Y=""/> 
			<Diode Id="7" Name="" TapClksToDtsCurrent="" X="" Y=""/> 
		</Packet>
		<!-- Up to 32 -->
	</Mode>
        <!-- TIDI DTS log filter settings -->	
	<Parsing>
		<EnableDtsTemperatureCalculation>False</EnableDtsTemperatureCalculation>
		<EnableDtsEmptyContent>False</EnableDtsEmptyContent>
		<EnableDtsSampleValidation>True</EnableDtsSampleValidation>
		<EnableDtsTemperatureFiltering>False</EnableDtsTemperatureFiltering>
		<DtsMinimumTemperature>50</DtsMinimumTemperature>
		<DtsMaximumTemperature>150</DtsMaximumTemperature>
	</Parsing>	
</DustiConfiguration>

  
  
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


