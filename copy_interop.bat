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
CALL :Copy GHIElectronics.TinyCLR.Native "TinyCLR_Core\CLR\Libraries\GHIElectronics_TinyCLR_Native"
CALL :Copy GHIElectronics.TinyCLR.Drawing "TinyCLR Devices\Drivers\Graphics\Drawing"
CALL :Copy GHIElectronics.TinyCLR.IO "TinyCLR Devices\Drivers\FileSystem\FAT\Interop"
CALL :Copy GHIElectronics.TinyCLR.Devices.Adc "TinyCLR Ports\Drivers\DevicesInterop\Adc"
CALL :Copy GHIElectronics.TinyCLR.Devices.Can "TinyCLR Ports\Drivers\DevicesInterop\Can"
CALL :Copy GHIElectronics.TinyCLR.Devices.Dac "TinyCLR Ports\Drivers\DevicesInterop\Dac"
CALL :Copy GHIElectronics.TinyCLR.Devices.Display "TinyCLR Ports\Drivers\DevicesInterop\Display"
CALL :Copy GHIElectronics.TinyCLR.Devices.Gpio "TinyCLR Ports\Drivers\DevicesInterop\Gpio"
CALL :Copy GHIElectronics.TinyCLR.Devices.I2c "TinyCLR Ports\Drivers\DevicesInterop\I2c"
CALL :Copy GHIElectronics.TinyCLR.Devices.Pwm "TinyCLR Ports\Drivers\DevicesInterop\Pwm"
CALL :Copy GHIElectronics.TinyCLR.Devices.Storage "TinyCLR Ports\Drivers\DevicesInterop\Storage"
CALL :Copy GHIElectronics.TinyCLR.Devices.Uart "TinyCLR Ports\Drivers\DevicesInterop\Uart"
CALL :Copy GHIElectronics.TinyCLR.Devices.Rtc "TinyCLR Ports\Drivers\DevicesInterop\Rtc"
CALL :Copy GHIElectronics.TinyCLR.Devices.Signals "TinyCLR Ports\Drivers\DevicesInterop\Signals"
CALL :Copy GHIElectronics.TinyCLR.Devices.Spi "TinyCLR Ports\Drivers\DevicesInterop\Spi"

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
