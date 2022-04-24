# VminForwardingSaveAsUpsGsdsTC Specification REP

### Rev 0

| Contents      |  
| :----------- |
| [Introduction](#introduction)      | 
| [Test Instance Parameters](#test-instance-parameters)   | 
| [Datalog output](#datalog-output)   | 
| [TPL Samples](#tpl-samples)   | 
| [Exit Ports](#exit-ports)   | 


##   

## Introduction
The main purpose of this test method is to copy vminforwarding data to GSDS tokens to be used by the YBS flows (insert names here)  

## Test Instance Parameters

| Parameter Name       | Required? | Type | Values | Default Value | Comments |
| :-----------          | :----------- | :----------- | :----------- | :----------- | :----------- | 
| UpsVfGsds            | Yes | string | Name of GSDS token | G.U.S.FAST_UPSVF | * see token format section for details * | 
| UpsVfPassinFlowGsds  | Yes | string | Name of GSDS token | G.U.S.FAST_UPSVFPASSFLOW | * see token format section for details * | 
| FastCornersGsds      | Yes | string | Name of GSDS token | G.U.S.FAST_CORNERS |  * see token format section for details *| 
| FastStcGsds          | Yes | string | Name of GSDS token | G.U.S.FAST_STC_V | * see token format section for details * | 
| PassingFlowInputGsds | Yes | string | Name of GSDS token | G.U.I.DDGVminForwardPassingFlow | * see token format section for details * | 
| MergeWithEvgData     | No  | bool   | True/False | False  | If True, will read G.U.S.FAST_STC_V to get EVG vmin values and merge them with Prime Vmin values | 

### GSDS Token Formats

##### UpsVfGsds
``` 
This contains all the Vmin data for every Domain/Corner for the first flow with data.  
Format - domain:freq^vmin%freq^vmin%freq^vmin_domain:freq^vmin%freq^vmin%freq^vmin
```

##### UpsVfPassinFlowGsds
``` 
This contains all the Vmin data for every Domain/Corner for the current/passing flow only (current/passing flow is contained in the GSDS token used as the PassingFlowInputGsds parameter).  
Format - domain:freq^vmin%freq^vmin%freq^vmin_domain:freq^vmin%freq^vmin%freq^vmin
``` 

##### FastCornersGsds
``` 
This contains all the Vmin data for every Domain/Corner for all flows using the EVG level_select identifier.  
Format - domainX=cornerId1:vmin|vmin|...|vmin_domainX=cornerIdY:vmin|vmin|...|vmin,...,domainZ=cornerId1:vmin|vmin|...|vmin_domainZ=cornerIdY:vmin|vmin|...|vmin

``` 

##### FastStcGsds   
``` 
This contains all the Vmin data for every Domain/Corner for all flows using the Domain and Corner names.  
Format - domainX=cornerName:vmin|vmin|...|vmin_domainX=cornerName:vmin|vmin|...|vmin,...,domainZ=cornerName:vmin|vmin|...|vmin_domainZ=cornerName:vmin|vmin|...|vmin

``` 

## Datalog output

The final value of all GSDS tokens are logged as strgval tokens using the tname *testname*\:\:PRIME\:\:*gsdstoken*.  If MergeWithEvgData is True, the initial (Evergreen) values will also be datalogged as tname *testname*\:\:EVG\:\:*gsdstoken*.

Example:
```
2_tname_BuildGsdsForYBS::EVG::FAST_CORNERS
2_strgval_CR=206:|||0.970v0.970v-9999v-9999v-9999v-9999v-9999v-9999_CR=205:|||0.901v0.902v-9999v-9999v-9999v-9999v-9999v-9999_CR=204:|||0.746v0.749v-9999v-9999v-9999v-9999v-9999v-9999_CR=203:|||0.605v0.595v-9999v-9999v-9999v-9999v-9999v-9999_CR=202:|||0.528v0.533v-9999v-9999v-9999v-9999v-9999v-9999_CR=201:|||0.490v0.500v-9999v-9999v-9999v-9999v-9999v-9999,CRF=226:|||_CRF=225:|||_CRF=224:|||_CRF=223:|||_CRF=222:|||_CRF=221:|||,CLR=216:|||0.913_CLR=215:|||0.835_CLR=214:|||0.734_CLR=213:|||0.590_CLR=212:|||0.526_CLR=211:|||0.500,CRX2=256:|||_CRX2=255:|||_CRX2=254:|||_CRX2=253:|||_CRX2=252:|||_CRX2=251:|||,CRX3=266:|||_CRX3=265:|||_CRX3=264:|||_CRX3=263:|||_CRX3=262:|||_CRX3=261:|||,GTS=305:|||0.877_GTS=304:|||0.773_GTS=303:|||0.680_GTS=302:|||0.600_GTS=301:|||0.540,SAQ=406:|||0.740_SAQ=404:|||0.640_SAQ=401:|||0.560,SAPS=515:|||0.830_SAPS=513:|||0.600_SAPS=511:|||0.530,SAIS=525:|||0.690_SAIS=521:|||0.530,SAF=535:|||0.640_SAF=531:|||0.560,SACD=504:|||0.746_SACD=502:|||0.670_SACD=501:|||0.570,GTSM=325:|||0.880_GTSM=323:|||0.760_GTSM=322:|||0.640_GTSM=321:|||0.530
2_tname_BuildGsdsForYBS::EVG::FAST_STC_V
2_strgval_CR=F6:|||0.970v0.970v-9999v-9999v-9999v-9999v-9999v-9999_CR=F5:|||0.901v0.902v-9999v-9999v-9999v-9999v-9999v-9999_CR=F4:|||0.746v0.749v-9999v-9999v-9999v-9999v-9999v-9999_CR=F3:|||0.605v0.595v-9999v-9999v-9999v-9999v-9999v-9999_CR=F2:|||0.528v0.533v-9999v-9999v-9999v-9999v-9999v-9999_CR=F1:|||0.490v0.500v-9999v-9999v-9999v-9999v-9999v-9999,CRF=F6:|||_CRF=F5:|||_CRF=F4:|||_CRF=F3:|||_CRF=F2:|||_CRF=F1:|||,CLR=F6:|||0.913_CLR=F5:|||0.835_CLR=F4:|||0.734_CLR=F3:|||0.590_CLR=F2:|||0.526_CLR=F1:|||0.500,CRX2=F6:|||_CRX2=F5:|||_CRX2=F4:|||_CRX2=F3:|||_CRX2=F2:|||_CRX2=F1:|||,CRX3=F6:|||_CRX3=F5:|||_CRX3=F4:|||_CRX3=F3:|||_CRX3=F2:|||_CRX3=F1:|||,GTS=F5:|||0.877_GTS=F4:|||0.773_GTS=F3:|||0.680_GTS=F2:|||0.600_GTS=F1:|||0.540,SAQ=F6:|||0.740_SAQ=F4:|||0.640_SAQ=F1:|||0.560,SAPS=F5:|||0.830_SAPS=F3:|||0.600_SAPS=F1:|||0.530,SAIS=F5:|||0.690_SAIS=F1:|||0.530,SAF=F5:|||0.640_SAF=F1:|||0.560,SACD=F4:|||0.746_SACD=F2:|||0.670_SACD=F1:|||0.570,GTSM=F5:|||0.880_GTSM=F3:|||0.760_GTSM=F2:|||0.640_GTSM=F1:|||0.530
2_tname_BuildGsdsForYBS::EVG::FAST_UPSVF
2_strgval_CR:4.400^0.970v0.970v-9999v-9999v-9999v-9999v-9999v-9999%4.200^0.901v0.902v-9999v-9999v-9999v-9999v-9999v-9999%3.400^0.746v0.749v-9999v-9999v-9999v-9999v-9999v-9999%2.200^0.605v0.595v-9999v-9999v-9999v-9999v-9999v-9999%1.200^0.528v0.533v-9999v-9999v-9999v-9999v-9999v-9999%0.400^0.490v0.500v-9999v-9999v-9999v-9999v-9999v-9999_CLR:4.000^0.913%3.600^0.835%3.000^0.734%1.800^0.590%0.800^0.526%0.400^0.500_GTS:1.300^0.877%1.100^0.773%0.900^0.680%0.600^0.600%0.300^0.540_SAQ:2.700^0.740%2.200^0.640%1.100^0.560_SAPS:1.000^0.830%0.400^0.600%0.200^0.530_SAIS:0.533^0.690%0.200^0.530_SAF:0.800^0.640%0.533^0.560_SACD:0.662^0.746%0.562^0.670%0.312^0.570_GTSM:1.100^0.880%0.900^0.760%0.600^0.640%0.300^0.530
2_tname_BuildGsdsForYBS::EVG::FAST_UPSVFPASSFLOW
2_strgval_CR:4.400^0.970v0.970v-9999v-9999v-9999v-9999v-9999v-9999%4.200^0.901v0.902v-9999v-9999v-9999v-9999v-9999v-9999%3.400^0.746v0.749v-9999v-9999v-9999v-9999v-9999v-9999%2.200^0.605v0.595v-9999v-9999v-9999v-9999v-9999v-9999%1.200^0.528v0.533v-9999v-9999v-9999v-9999v-9999v-9999%0.400^0.490v0.500v-9999v-9999v-9999v-9999v-9999v-9999_CLR:4.000^0.913%3.600^0.835%3.000^0.734%1.800^0.590%0.800^0.526%0.400^0.500_GTS:1.300^0.877%1.100^0.773%0.900^0.680%0.600^0.600%0.300^0.540_SAQ:2.700^0.740%2.200^0.640%1.100^0.560_SAPS:1.000^0.830%0.400^0.600%0.200^0.530_SAIS:0.533^0.690%0.200^0.530_SAF:0.800^0.640%0.533^0.560_SACD:0.662^0.746%0.562^0.670%0.312^0.570_GTSM:1.100^0.880%0.900^0.760%0.600^0.640%0.300^0.530

2_tname_BuildGsdsForYBS::PRIME::FAST_CORNERS
2_strgval_CR=206:|||0.970v0.970v-9999v-9999v-9999v-9999v-9999v-9999_CR=205:|||0.901v0.902v-9999v-9999v-9999v-9999v-9999v-9999_CR=204:|||0.746v0.749v-9999v-9999v-9999v-9999v-9999v-9999_CR=203:|||0.605v0.595v-9999v-9999v-9999v-9999v-9999v-9999_CR=202:|||0.528v0.533v-9999v-9999v-9999v-9999v-9999v-9999_CR=201:|||0.490v0.500v-9999v-9999v-9999v-9999v-9999v-9999,CRF=226:|||1.000v1.100v-9999v-9999v-9999v-9999v-9999v-9999_CRF=225:|||_CRF=224:|||_CRF=223:|||_CRF=222:|||_CRF=221:|||0.700v0.600v-9999v-9999v-9999v-9999v-9999v-9999,CLR=216:|||0.913_CLR=215:|||1.200_CLR=214:|||0.734_CLR=213:|||0.590_CLR=212:|||0.526_CLR=211:|||0.500,CRX2=256:|||_CRX2=255:|||_CRX2=254:|||_CRX2=253:|||_CRX2=252:|||_CRX2=251:|||,CRX3=266:|||_CRX3=265:|||_CRX3=264:|||_CRX3=263:|||_CRX3=262:|||_CRX3=261:|||,GTS=305:|||0.877_GTS=304:|||0.773_GTS=303:|||0.680_GTS=302:|||0.600_GTS=301:|||0.540,SAQ=406:|||0.740_SAQ=404:|||0.640_SAQ=401:|||0.560,SAPS=515:|||0.830_SAPS=513:|||0.600_SAPS=511:|||0.530,SAIS=525:|||0.690_SAIS=521:|||0.530,SAF=535:|||0.640_SAF=531:|||0.560,SACD=504:|||0.746_SACD=502:|||0.670_SACD=501:|||0.570,GTSM=325:|||0.880_GTSM=323:|||0.760_GTSM=322:|||0.640_GTSM=321:|||0.530
2_tname_BuildGsdsForYBS::PRIME::FAST_STC_V
2_strgval_CR=F6:|||0.970v0.970v-9999v-9999v-9999v-9999v-9999v-9999_CR=F5:|||0.901v0.902v-9999v-9999v-9999v-9999v-9999v-9999_CR=F4:|||0.746v0.749v-9999v-9999v-9999v-9999v-9999v-9999_CR=F3:|||0.605v0.595v-9999v-9999v-9999v-9999v-9999v-9999_CR=F2:|||0.528v0.533v-9999v-9999v-9999v-9999v-9999v-9999_CR=F1:|||0.490v0.500v-9999v-9999v-9999v-9999v-9999v-9999,CRF=F6:|||1.000v1.100v-9999v-9999v-9999v-9999v-9999v-9999_CRF=F5:|||_CRF=F4:|||_CRF=F3:|||_CRF=F2:|||_CRF=F1:|||0.700v0.600v-9999v-9999v-9999v-9999v-9999v-9999,CLR=F6:|||0.913_CLR=F5:|||1.200_CLR=F4:|||0.734_CLR=F3:|||0.590_CLR=F2:|||0.526_CLR=F1:|||0.500,CRX2=F6:|||_CRX2=F5:|||_CRX2=F4:|||_CRX2=F3:|||_CRX2=F2:|||_CRX2=F1:|||,CRX3=F6:|||_CRX3=F5:|||_CRX3=F4:|||_CRX3=F3:|||_CRX3=F2:|||_CRX3=F1:|||,GTS=F5:|||0.877_GTS=F4:|||0.773_GTS=F3:|||0.680_GTS=F2:|||0.600_GTS=F1:|||0.540,SAQ=F6:|||0.740_SAQ=F4:|||0.640_SAQ=F1:|||0.560,SAPS=F5:|||0.830_SAPS=F3:|||0.600_SAPS=F1:|||0.530,SAIS=F5:|||0.690_SAIS=F1:|||0.530,SAF=F5:|||0.640_SAF=F1:|||0.560,SACD=F4:|||0.746_SACD=F2:|||0.670_SACD=F1:|||0.570,GTSM=F5:|||0.880_GTSM=F3:|||0.760_GTSM=F2:|||0.640_GTSM=F1:|||0.530
2_tname_BuildGsdsForYBS::PRIME::FAST_UPSVF
2_strgval_CR:4.400^0.970v0.970v-9999v-9999v-9999v-9999v-9999v-9999%4.200^0.901v0.902v-9999v-9999v-9999v-9999v-9999v-9999%3.400^0.746v0.749v-9999v-9999v-9999v-9999v-9999v-9999%2.200^0.605v0.595v-9999v-9999v-9999v-9999v-9999v-9999%1.200^0.528v0.533v-9999v-9999v-9999v-9999v-9999v-9999%0.400^0.490v0.500v-9999v-9999v-9999v-9999v-9999v-9999_CRF:5.050^1.000v1.100v-9999v-9999v-9999v-9999v-9999v-9999%0.400^0.700v0.600v-9999v-9999v-9999v-9999v-9999v-9999_CLR:4.000^0.913%3.600^1.200%3.000^0.734%1.800^0.590%0.800^0.526%0.400^0.400_GTS:1.300^0.877%1.100^0.773%0.900^0.680%0.600^0.600%0.300^0.540_SAQ:2.700^0.740%2.200^0.640%1.100^0.560_SAPS:1.000^0.830%0.400^0.600%0.200^0.530_SAIS:0.533^0.690%0.200^0.530_SAF:0.800^0.640%0.533^0.560_SACD:0.662^0.746%0.562^0.670%0.312^0.570_GTSM:1.100^0.880%0.900^0.760%0.600^0.640%0.300^0.530
2_tname_BuildGsdsForYBS::PRIME::FAST_UPSVFPASSFLOW
2_strgval_CR:4.400^0.970v0.970v-9999v-9999v-9999v-9999v-9999v-9999%4.200^0.901v0.902v-9999v-9999v-9999v-9999v-9999v-9999%3.400^0.746v0.749v-9999v-9999v-9999v-9999v-9999v-9999%2.200^0.605v0.595v-9999v-9999v-9999v-9999v-9999v-9999%1.200^0.528v0.533v-9999v-9999v-9999v-9999v-9999v-9999%0.400^0.490v0.500v-9999v-9999v-9999v-9999v-9999v-9999_CRF:4.400^1.000v1.100v-9999v-9999v-9999v-9999v-9999v-9999%0.400^0.700v0.600v-9999v-9999v-9999v-9999v-9999v-9999_CLR:4.000^0.913%3.600^1.200%3.000^0.734%1.800^0.590%0.800^0.526%0.400^0.500_GTS:1.300^0.877%1.100^0.773%0.900^0.680%0.600^0.600%0.300^0.540_SAQ:2.700^0.740%2.200^0.640%1.100^0.560_SAPS:1.000^0.830%0.400^0.600%0.200^0.530_SAIS:0.533^0.690%0.200^0.530_SAF:0.800^0.640%0.533^0.560_SACD:0.662^0.746%0.562^0.670%0.312^0.570_GTSM:1.100^0.880%0.900^0.760%0.600^0.640%0.300^0.530
```

###

## TPL Samples

   
   

###

## Exit Ports

###

| Exit Port       | Condition | Description | 
| -----------          | ----------- | ----------- |  
| -2  | Alarm | Should not be possible |  
| -1  | Error | Any software error, GSDS/Vmin value not found | 
| 0   | Fail  | Failing condition. Currently not used, all failures will be caught by port -1. | 
| 1   | Pass  | Passing condition, all GSDS tokens updated correctly. | 


<br><br><br>
