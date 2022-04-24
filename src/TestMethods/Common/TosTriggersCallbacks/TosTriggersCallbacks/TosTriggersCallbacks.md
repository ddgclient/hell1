# DDG Prime TOSTriggersCallbacks
### Rev 0 4/27/2021 (fmurillo)

## Introduction
The main purpose of this callback is to provide a common callbacks manager for different TOSTriggers types. Currently supports SetPinAttributes only.

## Enabling the Callback
Add CallbacksManager instance in your INIT flow. 

## Using callback
1- Add TosTriggersCallbackSetup callback as pre-instance to setup your settings.<br>
2- Functional service will need to set TosTriggersCallbackExecute callback.<br>

## TosTriggersCallbackSetup CommandLine Options and Switches

### --type SetPinAttributes
Sets callback type. Right now only SetPinAttributes is supported.
### --prepause [time in mS]
Pause before callbacks execution.
### --postpause [time in mS]
Pause after callbacks execution.
### --settings index1:pin1:attribute1:value1 indexN:pinN:attributeN:valueN]
List of settings to apply. [TOSTrigger operand (uint)]:[PinName]:[AttibuteName]:[AttributeValue].
#### Note: TOS3.9+ requires tpo set multiple attributes for DPS settings.
#### Examples:
           TosTriggersCallbackSetup("--type=setpinattributes --prepause=1 --postpause=2 --settings=1:PinA:VForce:1.1");
           TosTriggersCallbackExecute("1");

           TosTriggersCallbackSetup("--type setpinattributes --settings 2:PinA:VForce:1.1 2:PinB:VForce:1.2");
           TosTriggersCallbackExecute("1");