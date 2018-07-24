@ECHO OFF

SETLOCAL ENABLEDELAYEDEXPANSION

SET ScriptRoot=%~dp0
SET ScriptRoot=%ScriptRoot:~,-1%

PUSHD "%ScriptRoot%"

SET Target=%1
SET Configuration=%2

IF "%Target%" == "" SET Target=checksum
IF "%Configuration%" == "" SET Configuration=debug

IF NOT "%Target%" == "all" IF NOT "%Target%" == "checksum" (
    ECHO Unsupported target passed: %Target%
    GOTO :EOF
)

IF NOT "%Configuration%" == "debug" IF NOT "%Configuration%" == "release" (
    ECHO Unsupported configuration passed: %Configuration%
    GOTO :EOF
)

CALL :Copy mscorlib "TinyCLR_Core\CLR\Libraries\mscorlib"
CALL :Copy GHIElectronics.TinyCLR.Native "TinyCLR_Core\CLR\Libraries\GHIElectronics.TinyCLR.Native"
CALL :Copy GHIElectronics.TinyCLR.Devices "TinyCLR Ports\Drivers\DevicesInterop"
CALL :Copy GHIElectronics.TinyCLR.Drawing "TinyCLR Devices\Drivers\Graphics\Drawing"
CALL :Copy GHIElectronics.TinyCLR.IO "TinyCLR Devices\Drivers\FileSystem\FAT\Interop"

POPD

ENDLOCAL

GOTO :EOF



:Copy

IF "%Target%" == "all" (
	SET Prefix=*
) ELSE (
	SET Prefix=%1
	SET Prefix=!Prefix:.=_!
)

XCOPY "%ScriptRoot%\%~1\bin\%Configuration%\pe\Interop\!Prefix!.*" "%ScriptRoot%\..\%~2\" /Y /Q

GOTO :EOF
