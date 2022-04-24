# DDG Prime Power Sequence Handler
### Rev 11/1/2021 (fmurillo)

## Introduction
The main purpose of this test class is to control power-down and power-up sequence in the test program. It provides knobs to force test conditions or apply them only when the power-on levels are changed.

## Parameters
### PowerOnTc
--Test Condition Name. Required while using ApplyPowerOn "Always" or "Switch".
### PowerDownTc
--Test Condition Name. Required while using ApplyPowerOn "Always" or "Switch".
### ApplyPowerDown
#### Skip:
--Sets power-on TC name but skips apply.
#### Swtich:
--Sets power-on TC name and applies TC when is different than the previous configuration.
#### Always:
--Dets power-on TC name and forces TC apply.
### ApplyPowerOn
#### Skip:
--Skips TC.
#### Switch:
--Applies TC when is different than the previous configuration.
#### Always:
--Forces TC apply.
### AlarmMode
-- Enabled will redirect all alarms to port 2.
## Ports
1 = alarm<br>
1 = pass<br>
0 = fail
