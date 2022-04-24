@echo off
SETLOCAL EnableDelayedExpansion
SET ERROR=0

REM Change to root directory specified by argument
IF "%3%" NEQ "" (
    cd "%3%"
    ECHO Root directory is %3%
)

REM Get Start Time.
setlocal EnableDelayedExpansion
set "startTime=%time: =0%"

REM ========================================================================================
REM Define all the Configuration types to compile to. 
SET BUILDS=Release Debug

REM ========================================================================================
REM Setup for MSBUILD.
IF EXIST "C:\Program Files (x86)\Microsoft Visual Studio\2019\Enterprise\VC\Auxiliary\Build\vcvars64.bat" (
    call "C:\Program Files (x86)\Microsoft Visual Studio\2019\Enterprise\VC\Auxiliary\Build\vcvars64.bat"
) ELSE (
    IF EXIST "C:\Program Files (x86)\Microsoft Visual Studio\2019\Professional\VC\Auxiliary\Build\vcvars64.bat" (
        call "C:\Program Files (x86)\Microsoft Visual Studio\2019\Professional\VC\Auxiliary\Build\vcvars64.bat"
    ) ELSE (
        ECHO.
        ECHO ***********************************************
        ECHO ERROR: VS vcvars64.bat not found under Visual Studio 2019, please check your installation.
        ECHO ***********************************************
        GOTO END
    )
)

echo Done sourcing VS stuff
echo.
echo.


IF "%3%" NEQ "" (
    SET workingPath=%3%
) ELSE (
    SET workingPath="%~dp0\.."
)

ECHO %workingPath%

REM ========================================================================================
REM CLEAN UP EVERYTHING


IF "%1%"=="All" (
    ECHO %workingPath%\src\
    cd /d %workingPath%\src\
) ELSE (
    ECHO %workingPath%\src\%1%\
    cd /d %workingPath%\src\%1%\
)

set MSBUILDDISABLENODEREUSE=1

CALL :CLEAN_ALL_SLN
if exist "%workingPath%\temp" RMDIR /Q /S "%workingPath%\temp"

REM ========================================================================================
REM CREATE LIB DIRECTORIES (BUILD FAILS WITHOUT THEM)
FOR %%b IN (%BUILDS%) do (
    if not exist "%workingPath%\lib\%%b" mkdir %workingPath%\lib\%%b
)

REM ========================================================================================
REM COMPILE ALL THE CODE
IF "%1%"=="All" (
    cd /d %workingPath%\src\
) ELSE (
    cd /d %workingPath%\src\%1%\
)
CALL :COMPILE_ALL_SLN
IF %ERROR% EQU 1 (
    ECHO Error detected during compilation.
) ELSE (
    ECHO Successfully compiled everything.
)
CALL :COPY_SUPERCEDES
CALL :UPDATE_PREHEADER_COMMON_PARAMS
GOTO END

REM ========================================================================================
REM ========================================================================================
:CLEAN_ALL_SLN
FOR /f %%a IN ('dir /s/b *.sln'    ) do   (
    cd %%~dpa
    FOR %%b IN (%BUILDS%) do (
        CALL :SINGLE_CLEAN %%~na%%~xa %%b
    )
)
GOTO :EOF

REM ========================================================================================
REM ========================================================================================
:COMPILE_ALL_SLN
FOR /f %%a IN ('dir /s/b *.sln'    ) do   (
    echo Directory: %%~dpa
    echo Solution: %%~na%%~xa
    cd %%~dpa
    FOR %%b IN (%BUILDS%) do (
        CALL :SINGLE_COMPILE %%~na%%~xa %%b
        echo.
    )
)
GOTO :EOF

REM ========================================================================================
REM ========================================================================================
:COPY_SUPERCEDES
if exist %workingPath%\lib\Supercedes (
	copy %workingPath%\lib\Supercedes\* %workingPath%\lib\Release
	copy %workingPath%\lib\Supercedes\* %workingPath%\lib\Debug
)
GOTO :EOF

REM ========================================================================================
REM ========================================================================================
:UPDATE_PREHEADER_COMMON_PARAMS
ECHO Running pipeline_scripts/create_common_preheaders.py to create/update the commonparams preheader files
%workingPath%\pipeline_scripts\create_common_preheaders.py %workingPath%
IF ERRORLEVEL 1 SET ERROR=1
GOTO :EOF

REM ========================================================================================
REM ========================================================================================
:SINGLE_CLEAN
ECHO Cleaning Project=%1 Configuration=%2 Platform=Any CPU
MSBuild %1 -verbosity:minimal -nologo -target:Clean -p:Configuration=%2 -p:Platform="Any CPU"
GOTO :EOF

REM ========================================================================================
REM ========================================================================================
:SINGLE_COMPILE
ECHO.
ECHO ======================================================================================================================
ECHO Current Directory: %CD%
ECHO Compiling Project=%1 Configuration=%2 Platform=Any CPU
ECHO.

MSBuild %1 -m -verbosity:normal -nologo -restore -p:Configuration=%2 -p:Platform="Any CPU"
IF ERRORLEVEL 1 SET ERROR=1
GOTO :EOF

REM ========================================================================================
REM ========================================================================================
:END

REM Get End and Elapsed Time.
set "endTime=%time: =0%"
set "end=!endTime:%time:~8,1%=%%100)*100+1!"  &  set "start=!startTime:%time:~8,1%=%%100)*100+1!"
set /A "elap=((((10!end:%time:~2,1%=%%100)*60+1!%%100)-((((10!start:%time:~2,1%=%%100)*60+1!%%100), elap-=(elap>>31)*24*60*60*100"
set /A "cc=elap%%100+100,elap/=100,ss=elap%%60+100,elap/=60,mm=elap%%60+100,hh=elap/60+100"
echo Start:    %startTime%
echo End:      %endTime%
echo Elapsed:  %hh:~1%%time:~2,1%%mm:~1%%time:~2,1%%ss:~1%%time:~8,1%%cc:~1%

IF "%2%"=="-ci" (
    pause
    IF %ERROR% EQU 1 exit 1
    exit 0
) ELSE (
    ECHO.
    pause
)