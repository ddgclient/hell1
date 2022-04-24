# ConcurrentTracesDecoder PinMap
### Rev 1 10/28/2021 (fmurillo)

## Contents

1. [Configuration](#configuration)     
2. [ApplyPlistSettings](#applyplistsettings)       
3. [MaskPlistFromTracker](#maskplistfromtracker)       
4. [GetFailTrackerFromPlistResults](#getfailtrackerfromplistresults)   
4. [Restore](#restore)   

## Configuration

ConcurrentTraces PinMap fields.

-    <b>Name</b> - name of the pinmap. This is the name that will be used for the Vmin Templates “PinMap” parameter. It must be unique.
-    <b>Size</b> – number of bits returned by this PinMap. This should match the number of  targets and the tracker size associated with this pinmap (refer to VminTC doccumentation).
-    <b>Description</b> – optional field for user comments.
-    <b>MaskConfigurations</b> – sets initial plist masking based on incoming/initial mask bits. Intended for disabled IPs and/or cores at the beginning of the search or when a target fails at the upper search limit.
     -    <u>Comment</u> - optional parameter to introduce comments.
     -    <u>TargePositions</u> - list of mask bits positions associated to the masked/disabled IP/Core.
     -    <u>PatternNames</u> - list of pattern name regular expressions for plist elements (patterns) where MaskPins or DisableCapture will be applied.
     -    <u>Options</u> - dictionary with PListElement options to be set for 'masked' patterns. E.g. Mask, DisableCompare, DisableCapture, etc.
-    <b>Entries</b> - provides decode information and start pattern info for each plist entry.
     -    <u>Comment</u> - optional parameter to introduce comments.
     -    <u>FailFilters</u> - matching cycle fail data. All filters need to match fail data. If there is no match decoder will error out.
          -    <i>Burst</i> - burst number.
          -    <i>PatternName</i> - full pattern name (no regex support).
          -    <i>PatternOccurrence</i> - pattern occurrence within current burst.
          -    <i>FailingPins</i> - list of list of failing patterns. Each list is associated to each TargetPosition. FailingPins and TargetPositions size must be equal.
          -    <i>TargetPositions</i> - list of target positions associated to the current fail. FailingPins and TargetPositions size must be equal.
     -    <u>StartPattern</u> - optional - pointer to start pattern information and entry pre-plist.
          -    <i>Burst</i> - burst number.
          -    <i>PatternName</i> - full pattern name (no regex support).
          -    <i>PatternOccurrence</i> - pattern occurrence within current burst.
          -    <i>PreBurstPList</i> - optional - replace current pre-plist with this value. Original value can be restored using Restore method.
     -    <u>PreBurstPList</u> - optional - pointer to preburst plist information.
          -    <i>Patlist</i> - optional - indicates specific patlist name where PreBurstPList should be applied. Needed for complex nested plist setups.
          -    <i>PreBurstPList</i> - replace current pre-plist with this value. Original value will be restored using Restore method.
     -    <u>PlistElementOptions</u> - optional - enables masking and disable capture for individual pattern occurrences in the plist.
          -    <i>Patlist</i> - optional - local plist where pattern is located. In a nested Plist structure this is the last level sub-plist.
          -    <i>Index</i> - list of pattern positions within current plist. It does not include PreBurstPList but will include individual sub-plist at the same level.
          -    <i>Options</i> - dictionary with PListElement options to be set for 'masked' patterns. E.g. Mask, DisableCompare, DisableCapture, etc.

### Example
```Json
[
{
    "Name": "CCR_map",
    "Size": 3,
    "Description": "IP_CPU::ccr_poc5af1_keep_ww36p4 CCF,GT,SA",
    "MaskConfigurations": [
        {
            "Comment": "CCF",
            "PatternNames": ["g.{38}05_"],
            "TargetPositions": [0],
            "Options":
            {
                "Mask": "IP_CPU::all_leg"
            }
        },
        {
            "Comment": "GT",
            "PatternNames": ["g.{38}(0z_)", "g.{38}(08_)"],
            "TargetPositions": [1],
            "Options":
            {
                "Mask": "IP_CPU::all_leg"
            }
        },
        {
            "Comment": "SA",
            "PatternNames": ["g.{38}ae_"],
            "TargetPositions": [2],
            "Options":
            {
                "Mask": "IP_CPU::all_leg"
            }
        }
    ],
    "Entries": [
        {
            "Comment": "Failing reset",
            "FailFilters": {
                "Burst": 0,
                "PatternName": "tgl_pre_F9999991G_040816xxx10040x66xxalb_Tbax2j_0g20_Mgt_0_vrevTB2C_ccrddr_hvm_hdmt2_QXJ_cf3rv_0_ccrddr",
                "PatternOccurrence": 1,
                "TargetPositions": [0,1,2]
            },
            "StartPattern": {
                "Burst": 0,
                "PatternName": "g2390745F0801504I_6I_VTB046T_Ccnm0m1h0005_a080816xx00044xbx1xxxalb_TB2CdhTQ000J3rv_LJP0A42x0nxx0000_poc5af1_p00_llc_cv_matsp_x_1",
                "PatternOccurrence": 1,
                "PreBurstPList": "IP_CPU::ccr_poc5af1_pbist_ccf_visa_disabled"
            },
            "PreBurstPList": {
                "Patlist": "ccr_poc5af1_keep_ww36p4_1",
                "PreBurstPList": "IP_CPU::ccr_poc5af1_pbist_ccf_visa_disabled"
            }
        },
        {
            "Comment": "Failing CCF",
            "FailFilters": {
                "Burst": 0,
                "PatternName": "g2390745F0801504I_IO_VTB046T_Ccnm0m1h0005_a080816xx00044xbx1xxxalb_TB2CdhTQ000J3rv_LJP0A42x0nxx0000_poc5af1_c00_llc_cv_matsp_x_1",
                "PatternOccurrence": 1,
                "TargetPositions": [0]
            },
            "StartPattern": {
                "Burst": 0,
                "PatternName": "g2390745F0801504I_6I_VTB046T_Ccnm0m1h0005_a080816xx00044xbx1xxxalb_TB2CdhTQ000J3rv_LJP0A42x0nxx0000_poc5af1_p00_llc_cv_matsp_x_1",
                "PatternOccurrence": 1,
            },
            "PreBurstPList": {
                "Patlist": "ccr_poc5af1_keep_ww36p4_1",
                "PreBurstPList": "IP_CPU::ccr_poc5af1_pbist_ccf_visa_disabled"
            }
        },
        {
            "Comment": "Failing GT",
            "FailFilters": {
                "Burst": 0,
                "PatternName": "g1962330F1421793I_Na_VTB049T_Gjnm0g20000z_a040816xx00066xax1xxxalb_TB2CdhTQ000J3rv_LJxbA42x0nxx0001_GT_PAR_tk3_ca1tf_mIn_ph15RO_poc5af1_p0",
                "PatternOccurrence": 1,
                "TargetPositions": [1]
            },
            "StartPattern": {
                "Burst": 0,
                "PatternName": "g2390745F0801504I_6I_VTB046T_Ccnm0m1h0005_a080816xx00044xbx1xxxalb_TB2CdhTQ000J3rv_LJP0A42x0nxx0000_poc5af1_p00_llc_cv_matsp_x_1",
                "PatternOccurrence": 1,
            },
            "PreBurstPList": {
                "Patlist": "ccr_poc5af1_keep_ww36p4_1",
                "PreBurstPList": "IP_CPU::ccr_poc5af1_pbist_ccf_visa_disabled"
            }
        },
        {
            "Comment": "Failing GT",
            "FailFilters": {
                "Burst": 0,
                "PatternName": "g1951882F1421793I_JX_VTB049T_Gjnm0g20000z_a040816xx00066xax1xxxalb_TB2CdhTQ000J3rv_LJxbA42x0nxx0091_GT_PAR_tk3_ca1tf_mIn_ph15RO_poc5af1_c5",
                "PatternOccurrence": 1,
                "TargetPositions": [1]
            },
            "StartPattern": {
                "Burst": 0,
                "PatternName": "d1618676F0850637I_6I_VTB046T_Gjna0g2000AA_a040816xx00066xax1xxxalb_TB2CdhTQ000J3rv_LJx2A42x0nxx0000_core_ccf_disable",
                "PatternOccurrence": 1,
            },
            "PreBurstPList": {
                "Patlist": "ccr_poc5af1_keep_ww36p4_1",
                "PreBurstPList": "IP_CPU::ccr_poc5af1_pbist_ccf_visa_disabled"
            },
            "PlistElementOptions": [
                {
                    "Patlist": "IP_CPU::ccr_poc5af1_keep_ww36p4_1",
                    "Index": [6],
                    "Options": 
                    {
                        "Mask": "IP_CPU::all_leg"
                    }
                },
                {
                    "Patlist": "IP_CPU::ccr_poc5af1_keep_ww36p4_1",
                    "Index": [4],
                    "Options": 
                    {
                        "Mask": "IP_CPU::all_leg"
                    }
                }
            ]
        },
        {
            "Comment": "Failing GT",
            "FailFilters": {
                "Burst": 0,
                "PatternName": "g1962330F1421793I_Nq_VTB049T_Gjnm0g20000z_a040816xx00066xax1xxxalb_TB2CdhTQ000J3rv_LJxbA42x0nxx0001_GT_PAR_tk3_ca1tf_mIn_ph15RO_poc5af1_c2",
                "PatternOccurrence": 1,
                "TargetPositions": [1]
            },
            "StartPattern": {
                "Burst": 0,
                "PatternName": "d1618676F0850637I_6I_VTB046T_Gjna0g2000AA_a040816xx00066xax1xxxalb_TB2CdhTQ000J3rv_LJx2A42x0nxx0000_core_ccf_disable",
                "PatternOccurrence": 1,
            },
            "PreBurstPList": {
                "Patlist": "ccr_poc5af1_keep_ww36p4_1",
                "PreBurstPList": "IP_CPU::ccr_poc5af1_pbist_ccf_visa_disabled"
            },
            "PlistElementOptions": [
                {
                    "Index": [4,6],
                    "Options": 
                    {
                        "Mask": "IP_CPU::all_leg"
                    }
                }
            ]
        },
        {
            "Comment": "Failing MBIST",
            "FailFilters": {
                "Burst": 1,
                "PatternName": "d1486772F0000795I_GV_XTB044T_Cknc3s1q00ae_a040811xx0b1222xx1xabald_TB2CdhTQ000J3rv_LJx0A42x0nxx0000_concurrent_A090000000000000g40000000000",
                "PatternOccurrence": 1,
                "TargetPositions": [2]
            },
            "StartPattern": {
                "Burst": 1,
                "PatternName": "g1054416F0801867I_6I_VTB044T_Ccnm0m1h0005_a080816xx00044xbx1xxxalb_TB2CdhTQ000J3rv_LJP0A42x0nxx0000_poc5af1_p00_llc_sta_galwrc_x_s",
                "PatternOccurrence": 1,
            },
            "PreBurstPList": {
                "Patlist": "ccr_poc5af1_keep_ww36p4_2",
                "PreBurstPList": "IP_CPU::ccr_poc5af1_pbist_ccf"
            }
        },
        {
            "Comment": "Failing MBIST",
            "FailFilters": {
                "Burst": 1,
                "PatternName": "d1491371F0002018I_K8_XTB044T_Cknc3s1q00ae_a040811xx0b1222xx1xabald_TB2CdhTQ000J3rv_LJx0A42x0nxx0000_concurrent_A1n000000000v1vv3000000000c0",
                "PatternOccurrence": 1,
                "TargetPositions": [2]
            },
            "StartPattern": {
                "Burst": 1,
                "PatternName": "g1054416F0801867I_6I_VTB044T_Ccnm0m1h0005_a080816xx00044xbx1xxxalb_TB2CdhTQ000J3rv_LJP0A42x0nxx0000_poc5af1_p00_llc_sta_galwrc_x_s",
                "PatternOccurrence": 1,
            },
            "PreBurstPList": {
                "Patlist": "ccr_poc5af1_keep_ww36p4_2",
                "PreBurstPList": "IP_CPU::ccr_poc5af1_pbist_ccf"
            }
        },
        {
            "Comment": "Failing MBIST",
            "FailFilters": {
                "Burst": 1,
                "PatternName": "d1491374F0002017I_K0_XTB044T_Cknc3s1q00ae_a040811xx0b1222xx1xabald_TB2CdhTQ000J3rv_LJx0A42x0nxx0000_concurrent_A0C000000000v1vvj400000000c0",
                "PatternOccurrence": 1,
                "TargetPositions": [2]
            },
            "StartPattern": {
                "Burst": 1,
                "PatternName": "g1054416F0801867I_6I_VTB044T_Ccnm0m1h0005_a080816xx00044xbx1xxxalb_TB2CdhTQ000J3rv_LJP0A42x0nxx0000_poc5af1_p00_llc_sta_galwrc_x_s",
                "PatternOccurrence": 1,
            },
            "PreBurstPList": {
                "Patlist": "ccr_poc5af1_keep_ww36p4_2",
                "PreBurstPList": "IP_CPU::ccr_poc5af1_pbist_ccf"
            }
        },
        {
            "Comment": "Failing MBIST",
            "FailFilters": {
                "Burst": 1,
                "PatternName": "d1486773F0000798I_GV_XTB044T_Cknc3s1q00ae_a040811xx0b1222xx1xabald_TB2CdhTQ000J3rv_LJx0A42x0nxx0000_concurrent_A0a0000000000000g40000000000",
                "PatternOccurrence": 1,
                "TargetPositions": [2]
            },
            "StartPattern": {
                "Burst": 1,
                "PatternName": "g1054416F0801867I_6I_VTB044T_Ccnm0m1h0005_a080816xx00044xbx1xxxalb_TB2CdhTQ000J3rv_LJP0A42x0nxx0000_poc5af1_p00_llc_sta_galwrc_x_s",
                "PatternOccurrence": 1,
            },
            "PreBurstPList": {
                "Patlist": "ccr_poc5af1_keep_ww36p4_2",
                "PreBurstPList": "IP_CPU::ccr_poc5af1_pbist_ccf"
            }
        }
    ]
}
]
```

## ApplyPlistSettings
This method is expected be called before each search point execution. Its primary function of to mask/disable IPS that are incoming bad/disabled or reached the upper search limit and failed.
- 1: Stores original PrePList option for Restore.
- 2: Find all MaskConfigurations settings where TargetPositions are set to 1 in the initial mask.
- 3: Reads and stores current PlistElementOptions for restore.
- 3: Applies PlistElementOptions if it is different than last applied value and updates cache storage.

## MaskPlistFromTracker
This method is expected be called before each search point execution. Utilizes last failing pattern insformation to set up your plist for next search point.
- 1: Checks if there is a last failing pattern. If there is not, it will default to the very first burst and pattern in the plist sequence.
- 2: Pulls and iterates through all plist cycle fail data. Matching failing data and FailFilters
- 3: Looks for first matching entry
- 4: Looks for first matching entry
- 5: Reads and stores current PlistElementOptions for restore.
- 6: Applies PlistElementOptions if it is different than last applied value and updates cache storage.
- 7: Sets StartPattern using input file configuration. If no StartPattern is specified it will use last failing pattern as default.
- 8: If there are entries wite FailFilters without PatternNames it will return FailingPins as as Plist wide pin masking option..

## GetFailTrackerFromPlistResults
This method is expected to be called after plist execution and decodes failing data returning a bit vector indicating failing voltage targets.
- 1: Pulls and iterates through all plist cycle fail data.
- 2: Matches each fail cycle data against FailFilters: Burst, PatternName, PatternOccurence.
- 3: Process optional FailFilters: FailingPins
- 4: If there is a matching entry TargetPositions will be marked as as bad (1).
- 5: If there was no matching entry ALL TargetPositions will be marked as as bad (1).

## Restore
This method restores PList to its original condition when the PlistTree was created. 
- 1: Decoder keeps track of all PlistOptions and PListElementOptions for restore.
- 2: Restore PreBurstPList to the DefaultPreBurstPList.