@echo off
REM Bodge to move working directory to root of repo
if not exist "%~dp0\..\%1" mkdir %~dp0\..\%1
echo %1
robocopy %~dp0\..\src\ %~dp0\..\%1\src\ /e
robocopy %~dp0\..\lib %~dp0\..\%1\lib /e
robocopy %~dp0\..\preheaders %~dp0\..\%1\preheaders /e
robocopy %~dp0\..\logs\dotCover  %~dp0\..\%1\unittestCoverage /e
REM Xcopy /E /I "%~dp0\doc" "%~dp0\%1\doc"
robocopy %~dp0\..\documentation %~dp0\..\%1\documentation /e
robocopy %~dp0\..\%1 \\amr.corp.intel.com\ec\proj\mdl\jf\intel\tpapps\userlibs\mtl\staging\%1 /e
if exist "%~dp0\..\%1" RMDIR /Q /S "%~dp0\..\%1"