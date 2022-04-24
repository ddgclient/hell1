@echo off

SET TPDIR=%CD%
SET PXRDIR=%CD%
SET PINOBJDIR=%CD%

FOR %%? IN (tgl_com*.pat) DO (
echo.
echo.
echo Compiling Pattern %%?
"%HDMTTOS%\Runtime\Release\HdmtPatternCompiler\HdmtPatternCompiler.exe" --pin %TPDIR% --pxr %PXRDIR% -o %PINOBJDIR% --pat %%?
)


FOR %%? IN (g*.pat) DO (
echo.
echo.
echo Compiling Pattern %%?
"%HDMTTOS%\Runtime\Release\HdmtPatternCompiler\HdmtPatternCompiler.exe" --pin %TPDIR% --pxr %PXRDIR% -o %PINOBJDIR% --pat %%?
)

