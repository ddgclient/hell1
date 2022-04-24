REM ECHO off
SET SSCEXEPATH=%HDMTTOS%\Runtime\Release
SET TPDIR=%CD%

REM LOAD THE TESTPROGRAM
%SSCEXEPATH%\SingleScriptCmd.exe loadTP %TPDIR% BaseTestPlan.tpl  PLIST_LOCAL.xml  EnvironmentFile_Local.env  SubTestPlan_MTL_CPU28_SDS.stpl  SDX_C28_X1_1.soc

REM LOAD THE QUICKSIM FILE
%SSCEXEPATH%\SingleScriptCmd.exe LoadQuickSimResponseFile %TPDIR%\Quicksim_PrimeValidation_x1.xml

REM INIT THE TESTPROGRAM
%SSCEXEPATH%\SingleScriptCmd.exe init

