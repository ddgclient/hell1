# AtomDecoder PinMap
### Rev 0

##

## Contents

1. [Configuration](#configuration)     
2. [PList Result Decoder](#plist-result-decoder)       
3. [PList Masking](#plist-masking)   

## Configuration

AtomDecoder PinMap fields.

-    <u>Name</u> - name of the pinmap. This is the name that will be used for the VMin Templates “PinMap” parameter. It must be unique.
-    <u>Module</u> - index of atom module. The atom module will be used in generating patmod names.
-    <u>Pin</u> - name of pin. This should be the name of the pin on which the results are output for the configured Atom module.
-    <u>Size</u> – number of bits returned by this PinMap. This should match the number of cores in an Atom module and the tracker size associated with this pinmap (the RecoveryTracking parameter of the VMin template).
-    <u>Content</u> - content type pinmap is configured for. Valid values: 'ARRAY', 'FUNC'
-    <u>PatternModify</u> - unrequired. This is required by all PinMaps but AtomDecoder does not use the field and be set to null.
-    <u>PatternModifyUniq</u> - unrequired. This is added to the patmod names if defined. 
-    <u>Reverse</u> - unrequired. This reverses the core number to tracker index mapping. Default: false.

## PList Result Decoder

This pinmap determines pass/fail status by decoding the labels associated with failing vectors. It decodes the failing core dependent on which content is configured.  
-    <u>Array</u> - Label must match regex '.*CORE\d.*'
-    <u>Func</u> - Label must match regex '.*C\d.*'  
If the label does not match the regex then AtomDecoder will mark all cores as failing.  
It will ignore the failing vector if it is not for the configured pin.  
It returns a BitArray with the indices associated with the failing cores set to 'true' indicating a failure.


## PList Masking

This pinmap will apply a patmod for every bit defined in the tracker, either a 'mask' or a 'restore' patmod. These patmods must be defined in the TP and must be named "atom\_<array|func>\_m\<module>\_c\<core>\_mask" and "atom\_<array|func>\_m\<module>\_c\<core>\_restore".  
For example, these eight patmods must all be defined for an AtomDecoder configured for array content, module one, and four cores.  
- atom\_array\_m1\_c0\_mask  
- atom\_array\_m1\_c1\_mask  
- atom\_array\_m1\_c2\_mask  
- atom\_array\_m1\_c3\_mask  
- atom\_array\_m1\_c0\_restore  
- atom\_array\_m1\_c1\_restore  
- atom\_array\_m1\_c2\_restore  
- atom\_array\_m1\_c3\_restore

If PatternModifyUniq was defined for the pinmap then it is added to the patmod name. For example, if we added PatternModifyUniq = 'dragon' to our previous AtomDecoder then the patmods would become the following.
- atom\_array\_dragon\_m1\_c0\_mask  
- atom\_array\_dragon\_m1\_c1\_mask  
- atom\_array\_dragon\_m1\_c2\_mask  
- atom\_array\_dragon\_m1\_c3\_mask  
- atom\_array\_dragon\_m1\_c0\_restore  
- atom\_array\_dragon\_m1\_c1\_restore  
- atom\_array\_dragon\_m1\_c2\_restore  
- atom\_array\_dragon\_m1\_c3\_restore
- 
If all cores in the module are failing then it will return the configured pin instead of applying patmods.