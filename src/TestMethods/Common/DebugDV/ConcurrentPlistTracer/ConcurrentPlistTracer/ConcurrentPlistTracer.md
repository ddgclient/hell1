# ConcurrentPlistTracer template

### Rev 0

| Contents      |    
| :----------- |  
| 1. [Introduction](#introduction)      |   
| 2. [Test Instance Parameters](#test-instance-parameters)   |   
| 3. [PatConfig Collateral](#patconfig-collateral) |
| 4. [VminTC Usage](#vmintc-usage)  |
| 5. [TPL Samples](#tpl-samples)   |  
| 6. [Console Output](#console-output)   |   
| 7. [Datalog Output](#datalog-output)   |   
| 8. [Exit Ports](#exit-ports)   |  

###

----  
## 1. Introduction  

This is a debug testmethod used to trace the execution of ConcurrentPlists. It performs the same function as the Evergreen InsertCTVs and TraceCTVs Userfunctions -- to insert CTVs in a concurrentplist and then use them to trace the execution path (how the plists are interleaved).  
It exists as a stand-along test template and is also integrated with VminTC.  

----  
## 2. Test Instance Parameters  

| Parameter Name       | Required? | Type | Values |  Comments |  
| :-----------         | :----------- | :----------- | :----------- | :----------- |   
| Patlist              | Yes | Plist           | Plist name to be executed |  |  
| TimingsTc            | Yes | TimingCondition | Timing test condition required for plist execution |  |  
| LevelsTc             | Yes | LevelsCondition | Levels test condition required for plist execution |  |  
| MaskPins             | No  | CommaSeparatedString | Comma separated list of pins to mask before executing Plist |  |  
| CtvCapturePins       | Yes  | CommaSeparatedString | Comma separated list of pins | Any valid pin name is fine.  |  
| CtvCapturePerCycleMode | Yes | ENABLED | MUST BE ENABLED |
| PatConfigForCtv      | Yes | String               | patmod configuration | Name of the Prime PatConfig to use to add/remove CTVs from the plist | |

----  
## 3. PatConfig Collateral  
The base testclass requires a Prime PatConfig setup to insert/remove the CTVs. A PatConfigSetpoint wrapper is useful for easy integration into VminTC.  
At a minimum the Patconfig should insert CTVs at the beginning of every pattern, before the call to the arbiter, and after the return from the arbiter.  

Here is an example of a PatConfig for TGL which does this. The labels/offsets will change for different products.  
It has 3 elements:
- Address=2 for all patterns. This is generally the first non instruction vector for a pattern. No label is needed.  
- Label=".\*_SMARTCC.\*". This matches the label before the "Call" instruction to subroutine which returns to the Arbiter.  
- Label=".\*_SMARTCC.\*" + 4 Vectors. This matches the first non-instruction vector after the return from the Arbiter.  

#### TGL Example   
```json
{
  "Configurations": [
    {
      "Name": "ConcurrentPlistTracerPatMod",
      "ConfigurationElement": [
        {
          "Type": "CTV",
          "Domain": "IP_CPU::LEG",
          "StartAddress": "2",
          "StartAddressOffset": 0,
          "EndAddress": "2",
          "EndAddressOffset": 0,
          "PatternsRegEx": [
            "^[dgs].*"
          ]
        },
        {
          "Type": "CTV",
          "Domain": "IP_CPU::LEG",
          "StartAddress": ".*_SMARTCC.*",
          "StartAddressOffset": 0,
          "EndAddress": "^",
          "EndAddressOffset": 0,
          "PatternsRegEx": [
            "^[dgs].*"
          ],
		  "ValidationMode": "ALLOW_LABEL_NO_MATCHING"
        },
        {
          "Type": "CTV",
          "Domain": "IP_CPU::LEG",
          "StartAddress": ".*_SMARTCC.*",
          "StartAddressOffset": 4,
          "EndAddress": "^",
          "EndAddressOffset": 4,
          "PatternsRegEx": [
            "^[dgs].*"
          ],
		  "ValidationMode": "ALLOW_LABEL_NO_MATCHING"
        }
      ]
    }
  ]
}

```

#### MTL Example   
```json
{
  "Configurations": [
    {
      "Name": "ConcurrentPlistTracerPatMod",
      "ConfigurationElement": [
        {
          "Type": "CTV",
          "Domain": "CPU_TAP_ALL",
          "StartAddress": "2",
          "StartAddressOffset": 0,
          "EndAddress": "2",
          "EndAddressOffset": 0,
          "PatternsRegEx": [
            "^[dgs].{74}xxMT.*"
          ]
        },
        {
          "Type": "CTV",
          "Domain": "CPU_TAP_ALL",
          "StartAddress": ".*_SMARTCC.*",
          "StartAddressOffset": 0,
          "EndAddress": "^",
          "EndAddressOffset": 0,
          "PatternsRegEx": [
            "^[dgs].{74}xxMT.*"
          ],
          "ValidationMode": "ALLOW_LABEL_NO_MATCHING"
        },
        {
          "Type": "CTV",
          "Domain": "CPU_TAP_ALL",
          "StartAddress": ".*_SMARTCC.*",
          "StartAddressOffset": 8,
          "EndAddress": "^",
          "EndAddressOffset": 8,
          "PatternsRegEx": [
            "^[dgs].{74}xxMT.*"
          ],
          "ValidationMode": "ALLOW_LABEL_NO_MATCHING"
        }
      ]
    }
  ]
}
```



Here is an example PatConfigSetPoint wrapper. It is not required for the stand-alone ConcurrentPlistTracer template, but it is useful for using the functionality in a VminTC testclass.  
Other than the names matching the previously defined PatConfig, this should not need to change.  
```json
{
  "Module": "ConcurrentPlist",
  "Groups": [
    {
      "Name": "TraceCTV",
      "Default": "ON",
      "SetPoints": [
        {
          "Name": "ON",
          "Configurations": [
            {
              "Name": "ConcurrentPlistTracerPatMod",
              "Data": "1+"
            }
          ]
        },
        {
          "Name": "OFF",
          "Configurations": [
            {
              "Name": "ConcurrentPlistTracerPatMod",
              "Data": "0+"
            }
          ]
        }
      ]
    }
  ]
}
```

----  
## 4. VminTC Usage  
The Tracer functionality can be enabled in a VminTC instance with the following Parameters:
- FeatureSwitchSettings = "trace_ctv_on"  
- CtvPins = "<any_pin>" # Any Valid pin name will work here, TDO or IP_CPU::TDO works well.  
- SetPointsPreInstance = "ConcurrentPlist:TraceCTV:ON" # needs to match the PatConfigSetPoint which inserts the CTVs.  
- SetPointsPostInstance = "ConcurrentPlist:TraceCTV:OFF" # needs to match the PatConfigSetPoint which removes the CTVs.  
- SetPointsPlistParamName = "Patlist"; # to enable PatConfig on the test instances patterns only.  
  
----  
## 5. TPL Samples  

As a Stand-Alone test instance:
```c-sharp  
Test ConcurrentPlistTracer ConcurrentPListTracer_Test1
{
   Patlist = "arrccf_p2t1_funia_p2t1";
   TimingsTc = "FUNC_CORE_CCR::cpu_func_sdr_univ_sta_univ_univ_b100_t100_d100_SHARED_4E4544E4FF049D6BB804AE1CA00CDB223B543606964DFB443809F4779A13015D";
   LevelsTc = "FUNC_CORE_CCR::DDR_univ_lvl_nom_lvl_SHARED_2CF7899CDA9884B7A0D9A50144A1ECC26DCA136C46665B6F40329E901EB0C000";
   PatConfigForCtv = "ConcurrentPlistTracerPatMod";
   CtvCapturePins = "IP_CPU::TDO";
   CtvCapturePerCycleMode = "ENABLED",
   LogLevel = "PRIME_DEBUG";
}
```

Integrated with VminTC:  
```c-sharp  
Test VminTC ALL_GTALL_VMIN_K_SRHGTSF1_X_VCCGT_MEDIA_F1_0300_1504
{
   # These parameters are required to enable the CTV Tracer
   FeatureSwitchSettings = "fivr_mode_on,trace_ctv_on"; # the fivr_mode_on is not a ctv trace requirement
   CtvPins = "IP_CPU::TDO";
   SetPointsPlistParamName = "Patlist";
   SetPointsPreInstance = "ConcurrentPlist:TraceCTV:ON";
   SetPointsPostInstance = "ConcurrentPlist:TraceCTV:OFF";

   # These rest of the parameters can be anything valid for VminTC
   ForwardingMode = "None";
   LogLevel = "PRIME_DEBUG";
   Patlist = "arrccf_p2t1_funia_p2t1";
   TimingsTc = "FUNC_CORE_CCR::cpu_func_sdr_univ_sta_univ_univ_b100_t100_d100_SHARED_4E4544E4FF049D6BB804AE1CA00CDB223B543606964DFB443809F4779A13015D";
   LevelsTc = "FUNC_CORE_CCR::DDR_univ_lvl_nom_lvl_SHARED_2CF7899CDA9884B7A0D9A50144A1ECC26DCA136C46665B6F40329E901EB0C000";
   MaskPins = "IP_CPU::all_leg,IP_CPU::all_ddr";

   TestMode = "Functional";
   StartVoltages = "0.6V";
   EndVoltageLimits = "1.2V";
   StepSize = 0.01;

   FivrCondition = "NOM";
   VoltageTargets = "CORE0";
}
```   


----  
## 6. Console Output  
The console output is similar to the Evergereen format, but it does remove consecutive prints of the same pattern.  So if the arbiter is exectuted 100 times before returning to the plist, it will still only print once.  
Column Order:
- Domain
- Pattern
- Plist
- Address        - Vector address in the pattern
- Cycle          - Cycle address in the pattern
- TraceRegister  - Indicates the IP index
- TraceCycle     - Total cycle count of this IP
- BurstIndex     - Pattern Burst count
- BurstCycle     - Total cycle count of this Burst

Example:
```
CTV TRACE BEGIN ============================================================================
CTV Trace: Domain:[IP_CPU::LEG] Pattern:[d1652093F0713976I_6I_VTB046T_Ccna0m1h00AA_a080816xx00044xbx1xxxalb_TB5PrhTC004J36z_LJx0A42x0nxx0000_ccf_shortentry_mciiso] Plist:[IP_CPU::arrccf_p2t1] Address:[2] Cycle:[2] TraceReg1:[0] TraceCycle:[2] BusrtIndex:[0] BurstCycle:[2]
CTV Trace: Domain:[IP_CPU::LEG] Pattern:[g1855918F3270261I_6I_VTB049T_Rcnm0m1s0015_a080816xx00044xbx1xxxalb_TB5PrhTC013J36z_LJP0A42x0nxx0000_start_pat] Plist:[IP_CPU::arrccf_p2t1] Address:[2] Cycle:[2] TraceReg1:[0] TraceCycle:[2002] BusrtIndex:[0] BurstCycle:[2002]
CTV Trace: Domain:[IP_CPU::LEG] Pattern:[d1707475F0631220I_8L_VTB044T_Fina042u00AA_a040416xx00022xbx1xxxalb_TB5PrhTC003J36z_LJx0A42x0nxx0000_mciiso_only] Plist:[IP_CPU::arrccf_p2t1] Address:[2] Cycle:[2] TraceReg1:[0] TraceCycle:[4002] BusrtIndex:[0] BurstCycle:[4002]
CTV Trace: Domain:[IP_CPU::LEG] Pattern:[g1152655F0631220I_GQ_VTB044T_Fina042u00AA_a040416xx00022xbx1xxxalb_TB5PrhTC003J36z_LJx0A42x0nxx0000_Sbft_Stage2_Flows] Plist:[IP_CPU::arrccf_p2t1] Address:[2] Cycle:[2] TraceReg1:[0] TraceCycle:[6002] BusrtIndex:[0] BurstCycle:[6002]
CTV Trace: Domain:[IP_CPU::LEG] Pattern:[d1652092F0713976I_CE_VTB046T_Ccna0m1h00AA_a080816xx00044xbx1xxxalb_TB5PrhTC004J36z_LJx0A42x0nxx0000_ccf_precat_mciiso] Plist:[IP_CPU::arrccf_p2t1] Address:[2] Cycle:[2] TraceReg1:[0] TraceCycle:[8002] BusrtIndex:[0] BurstCycle:[8002]
CTV Trace: Domain:[IP_CPU::LEG] Pattern:[g2390702F0801547I_KB_VTB046T_Ccnm0m1h0005_a080816xx00044xbx1xxxalb_TB5PrhTC004J36z_LJP0A42x0nxx0000_llc_cv_scan_x_s_ff] Plist:[IP_CPU::arrccf_p2t1] Address:[2] Cycle:[2] TraceReg1:[0] TraceCycle:[10002] BusrtIndex:[0] BurstCycle:[10002]
CTV Trace: Domain:[IP_CPU::LEG] Pattern:[d1707475F0631220I_8L_VTB044T_Fina042u00AA_a040416xx00022xbx1xxxalb_TB5PrhTC003J36z_LJx0A42x0nxx0000_mciiso_only] Plist:[IP_CPU::funia_p2t1] Address:[2] Cycle:[2] TraceReg1:[1] TraceCycle:[2] BusrtIndex:[0] BurstCycle:[12002]
CTV Trace: Domain:[IP_CPU::LEG] Pattern:[g1328059F0600746I_Lx_VTB044T_Finm042u2vbn_a040416xx00022xbx1xxxalb_TB5PrhTC003J36z_LJx0A42x3nxx0000_ATPG_id_small_dec_patterns_legacy_1] Plist:[IP_CPU::funia_p2t1] Address:[2] Cycle:[2] TraceReg1:[1] TraceCycle:[2002] BusrtIndex:[0] BurstCycle:[14002]
CTV Trace: Domain:[IP_CPU::LEG] Pattern:[g1449911F2648901I_6I_VTB046T_Rcnm0m1s0015_a080816xx00044xbx1xxxalb_TB5PrhTC013J36z_LJP0A42x0nxx0000_arbiter_precat] Plist:[IP_CPU::arrccf_p2t1_funia_p2t1] Address:[2] Cycle:[2] TraceReg1:[1] TraceCycle:[4002] BusrtIndex:[0] BurstCycle:[16002]
CTV Trace: Domain:[IP_CPU::LEG] Pattern:[d1652093F0713976I_6I_VTB046T_Ccna0m1h00AA_a080816xx00044xbx1xxxalb_TB5PrhTC004J36z_LJx0A42x0nxx0000_ccf_shortentry_mciiso] Plist:[IP_CPU::arrccf_p2t1] Address:[2] Cycle:[2] TraceReg1:[0] TraceCycle:[12002] BusrtIndex:[0] BurstCycle:[18002]
CTV Trace: Domain:[IP_CPU::LEG] Pattern:[g2390702F0801547I_KB_VTB046T_Ccnm0m1h0005_a080816xx00044xbx1xxxalb_TB5PrhTC004J36z_LJP0A42x0nxx0000_llc_cv_scan_x_s_ff] Plist:[IP_CPU::arrccf_p2t1] Address:[357] Cycle:[2] TraceReg1:[0] TraceCycle:[14002] BusrtIndex:[0] BurstCycle:[20002]
CTV Trace: Domain:[IP_CPU::LEG] Pattern:[d1707475F0631220I_8L_VTB044T_Fina042u00AA_a040416xx00022xbx1xxxalb_TB5PrhTC003J36z_LJx0A42x0nxx0000_mciiso_only] Plist:[IP_CPU::funia_p2t1] Address:[2] Cycle:[2] TraceReg1:[1] TraceCycle:[6002] BusrtIndex:[0] BurstCycle:[22002]
CTV Trace: Domain:[IP_CPU::LEG] Pattern:[g1328059F0600746I_Lx_VTB044T_Finm042u2vbn_a040416xx00022xbx1xxxalb_TB5PrhTC003J36z_LJx0A42x3nxx0000_ATPG_id_small_dec_patterns_legacy_1] Plist:[IP_CPU::funia_p2t1] Address:[10745] Cycle:[2] TraceReg1:[1] TraceCycle:[8002] BusrtIndex:[0] BurstCycle:[24002]
CTV Trace: Domain:[IP_CPU::LEG] Pattern:[g1328070F0600765I_Ls_VTB044T_Finm042u2vbn_a040416xx00022xbx1xxxalb_TB5PrhTC003J36z_LJx0A42x3nxx0000_DCU_eviction] Plist:[IP_CPU::funia_p2t1] Address:[2] Cycle:[2] TraceReg1:[1] TraceCycle:[10002] BusrtIndex:[0] BurstCycle:[26002]
CTV Trace: Domain:[IP_CPU::LEG] Pattern:[g1449911F2648901I_6I_VTB046T_Rcnm0m1s0015_a080816xx00044xbx1xxxalb_TB5PrhTC013J36z_LJP0A42x0nxx0000_arbiter_precat] Plist:[IP_CPU::arrccf_p2t1_funia_p2t1] Address:[2] Cycle:[2] TraceReg1:[1] TraceCycle:[12002] BusrtIndex:[0] BurstCycle:[28002]
CTV Trace: Domain:[IP_CPU::LEG] Pattern:[d1652093F0713976I_6I_VTB046T_Ccna0m1h00AA_a080816xx00044xbx1xxxalb_TB5PrhTC004J36z_LJx0A42x0nxx0000_ccf_shortentry_mciiso] Plist:[IP_CPU::arrccf_p2t1] Address:[2] Cycle:[2] TraceReg1:[0] TraceCycle:[16002] BusrtIndex:[0] BurstCycle:[30002]
CTV Trace: Domain:[IP_CPU::LEG] Pattern:[g2390702F0801547I_KB_VTB046T_Ccnm0m1h0005_a080816xx00044xbx1xxxalb_TB5PrhTC004J36z_LJP0A42x0nxx0000_llc_cv_scan_x_s_ff] Plist:[IP_CPU::arrccf_p2t1] Address:[793] Cycle:[2] TraceReg1:[0] TraceCycle:[18002] BusrtIndex:[0] BurstCycle:[32002]
CTV Trace: Domain:[IP_CPU::LEG] Pattern:[g1052176F0801503I_Th_VTB044T_Ccnm0m1h0005_a080816xx00044xbx1xxxalb_TB5PrhTC004J36z_LJP0A42x0nxx0000_llc_cv_mchraw_x_s] Plist:[IP_CPU::arrccf_p2t1] Address:[2] Cycle:[2] TraceReg1:[0] TraceCycle:[20002] BusrtIndex:[0] BurstCycle:[34002]
CTV Trace: Domain:[IP_CPU::LEG] Pattern:[d1707475F0631220I_8L_VTB044T_Fina042u00AA_a040416xx00022xbx1xxxalb_TB5PrhTC003J36z_LJx0A42x0nxx0000_mciiso_only] Plist:[IP_CPU::funia_p2t1] Address:[2] Cycle:[2] TraceReg1:[1] TraceCycle:[14002] BusrtIndex:[0] BurstCycle:[36002]
CTV Trace: Domain:[IP_CPU::LEG] Pattern:[g1328070F0600765I_Ls_VTB044T_Finm042u2vbn_a040416xx00022xbx1xxxalb_TB5PrhTC003J36z_LJx0A42x3nxx0000_DCU_eviction] Plist:[IP_CPU::funia_p2t1] Address:[10745] Cycle:[2] TraceReg1:[1] TraceCycle:[16002] BusrtIndex:[0] BurstCycle:[38002]
CTV Trace: Domain:[IP_CPU::LEG] Pattern:[g1328083F0600771I_Lr_VTB044T_Finm042u2vbn_a040416xx00022xbx1xxxalb_TB5PrhTC003J36z_LJx0A42x3nxx0000_F16_template] Plist:[IP_CPU::funia_p2t1] Address:[2] Cycle:[2] TraceReg1:[1] TraceCycle:[18002] BusrtIndex:[0] BurstCycle:[40002]
CTV Trace: Domain:[IP_CPU::LEG] Pattern:[g1449911F2648901I_6I_VTB046T_Rcnm0m1s0015_a080816xx00044xbx1xxxalb_TB5PrhTC013J36z_LJP0A42x0nxx0000_arbiter_precat] Plist:[IP_CPU::arrccf_p2t1_funia_p2t1] Address:[2] Cycle:[2] TraceReg1:[1] TraceCycle:[20002] BusrtIndex:[0] BurstCycle:[42002]
CTV Trace: Domain:[IP_CPU::LEG] Pattern:[d1707475F0631220I_8L_VTB044T_Fina042u00AA_a040416xx00022xbx1xxxalb_TB5PrhTC003J36z_LJx0A42x0nxx0000_mciiso_only] Plist:[IP_CPU::funia_p2t1] Address:[2] Cycle:[2] TraceReg1:[1] TraceCycle:[22002] BusrtIndex:[0] BurstCycle:[44002]
CTV Trace: Domain:[IP_CPU::LEG] Pattern:[g1328083F0600771I_Lr_VTB044T_Finm042u2vbn_a040416xx00022xbx1xxxalb_TB5PrhTC003J36z_LJx0A42x3nxx0000_F16_template] Plist:[IP_CPU::funia_p2t1] Address:[10745] Cycle:[2] TraceReg1:[1] TraceCycle:[24002] BusrtIndex:[0] BurstCycle:[46002]
CTV Trace: Domain:[IP_CPU::LEG] Pattern:[g1328082F0600794I_Lv_VTB044T_Finm042u2vbn_a040416xx00022xbx1xxxalb_TB5PrhTC003J36z_LJx0A42x3nxx0000_HT_hvm_gsse_v112_2_limited2_2M_1845684582] Plist:[IP_CPU::funia_p2t1] Address:[2] Cycle:[2] TraceReg1:[1] TraceCycle:[26002] BusrtIndex:[0] BurstCycle:[48002]
CTV Trace: Domain:[IP_CPU::LEG] Pattern:[g1449911F2648901I_6I_VTB046T_Rcnm0m1s0015_a080816xx00044xbx1xxxalb_TB5PrhTC013J36z_LJP0A42x0nxx0000_arbiter_precat] Plist:[IP_CPU::arrccf_p2t1_funia_p2t1] Address:[2] Cycle:[2] TraceReg1:[1] TraceCycle:[28002] BusrtIndex:[0] BurstCycle:[50002]
CTV Trace: Domain:[IP_CPU::LEG] Pattern:[d1707475F0631220I_8L_VTB044T_Fina042u00AA_a040416xx00022xbx1xxxalb_TB5PrhTC003J36z_LJx0A42x0nxx0000_mciiso_only] Plist:[IP_CPU::funia_p2t1] Address:[2] Cycle:[2] TraceReg1:[1] TraceCycle:[30002] BusrtIndex:[0] BurstCycle:[52002]
CTV Trace: Domain:[IP_CPU::LEG] Pattern:[g1328082F0600794I_Lv_VTB044T_Finm042u2vbn_a040416xx00022xbx1xxxalb_TB5PrhTC003J36z_LJx0A42x3nxx0000_HT_hvm_gsse_v112_2_limited2_2M_1845684582] Plist:[IP_CPU::funia_p2t1] Address:[11347] Cycle:[2] TraceReg1:[1] TraceCycle:[32002] BusrtIndex:[0] BurstCycle:[54002]
CTV Trace: Domain:[IP_CPU::LEG] Pattern:[g1328129F0600795I_Lw_VTB044T_Finm042u2vbn_a040416xx00022xbx1xxxalb_TB5PrhTC003J36z_LJx0A42x3nxx0000_HXS_MEMRA_T2M_AVX2_5_mlc] Plist:[IP_CPU::funia_p2t1] Address:[2] Cycle:[2] TraceReg1:[1] TraceCycle:[34002] BusrtIndex:[0] BurstCycle:[56002]
...
CTV TRACE END ============================================================================


```
  

----  
## 7. Datalog Output  
No data is logged to ituff.  


----  
## 8. Exit Ports  

| Exit Port       | Condition   | Description |   
| -----------     | ----------- | ----------- |    
| -2  | Alarm | Any HW alarm condition |    
| -1  | Error | Any software error |   
| 0   | Fail  | - |   
| 1   | Pass  | Ran successfully. |   

###

----  
