# DDG Prime Pattern Modifications Base
### Rev 2/3/2022

## Introduction
DDG Prime PatternModificationsBase test class is an auxiliary test class to load a FreqSetPoint map into Prime SharedStorage.<br>
The map enables the user to apply multiple PatConfigSetPoints for a given list for CornerIdentifiers while using BinMatrix tokens.

## Parameters
#### ConfigurationFile
Configuration file in json format. Supported format in the example below. 
User can defined as many CornerIdentifiers as needed and each corner can defined as many SetPoints as needed.
For "SetPoint" value the user can enter \{specset\} which will be automatically evaluated using Prime BinMatrixService and current flow for each corner.
##### Condition:
Each SetPoint includes a condition to validate if the patconfig must be applied. When evaluated condition is false, the patconfig gets skipped.<br>
Expression evaluator supports Gsds, Usrv, DFF and SpecSet tokens.

<pre>
{
	"CornerIdentifiers": {
		"CRF1": [ 
			{
			"Module": "FUN",
			"Group": "core_freq",
			"SetPoint": "{binmatrix_core_F1_spec}"
			},
			{
			"Module": "FUN",
			"Group": "ring_freq",
			"SetPoint": "3GHz",
			"Condition": "[S.T.bclk_per_spec] == '9nS'"
			}
		],
		"CRF2": [ 
			{
			"Module": "FUN",
			"Group": "core_freq",
			"SetPoint": "{binmatrix_core_F2_spec}"
			},
			{
			"Module": "FUN",
			"Group": "ring_freq",
			"SetPoint": "1.2GHz",
            "Condition": "[G.U.I.Token] > 2"
			}
		]
	}
}
</pre>

#### SharedStorageKey
String key for Shared Storage where serialized json object will be stored. Set to Contex.DUT.
