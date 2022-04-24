@echo off
SETLOCAL EnableDelayedExpansion
echo Setting up MSBuild

IF "%2%" NEQ "" (
    cd "%2%"
    ECHO Root directory is %2%
) ELSE (
	REM if running from the pipeline_scripts directory
	IF EXIST "..\unit_test\Release" (
		cd ".."
    )
)



IF EXIST "C:\Program Files (x86)\Microsoft Visual Studio\2019\Enterprise\VC\Auxiliary\Build\vcvars64.bat" (
    call "C:\Program Files (x86)\Microsoft Visual Studio\2019\Enterprise\VC\Auxiliary\Build\vcvars64.bat"
) ELSE (
    IF EXIST "C:\Program Files (x86)\Microsoft Visual Studio\2019\Professional\VC\Auxiliary\Build\vcvars64.bat" (
        call "C:\Program Files (x86)\Microsoft Visual Studio\2019\Professional\VC\Auxiliary\Build\vcvars64.bat"
    )
)

echo Done sourcing VS stuff
echo.
echo.

vstest.console.exe /Parallel unit_test\Release\*.UnitTest.dll

IF "%1%"=="-ci" (
    pause
    IF ERRORLEVEL 1 exit 1
    exit 0
) ELSE (
    echo.
    echo.
    pause
)
