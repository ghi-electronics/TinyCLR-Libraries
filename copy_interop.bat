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
CALL :Copy GHIElectronics.TinyCLR.Drawing "TinyCLR Devices\Drivers\Graphics\interop"
CALL :Copy GHIElectronics.TinyCLR.IO "TinyCLR Devices\Drivers\Filesystem\interop"
CALL :Copy GHIElectronics.TinyCLR.Networking "TinyCLR Devices\Drivers\DevicesInterop\Network"
CALL :Copy GHIElectronics.TinyCLR.Devices.Adc "TinyCLR Devices\Drivers\DevicesInterop\Adc"
CALL :Copy GHIElectronics.TinyCLR.Devices.Can "TinyCLR Devices\Drivers\DevicesInterop\Can"
CALL :Copy GHIElectronics.TinyCLR.Devices.Dac "TinyCLR Devices\Drivers\DevicesInterop\Dac"
CALL :Copy GHIElectronics.TinyCLR.Devices.Display "TinyCLR Devices\Drivers\DevicesInterop\Display"
CALL :Copy GHIElectronics.TinyCLR.Devices.Gpio "TinyCLR Devices\Drivers\DevicesInterop\Gpio"
CALL :Copy GHIElectronics.TinyCLR.Devices.I2c "TinyCLR Devices\Drivers\DevicesInterop\I2c"
CALL :Copy GHIElectronics.TinyCLR.Devices.Network "TinyCLR Devices\Drivers\DevicesInterop\Network"
CALL :Copy GHIElectronics.TinyCLR.Devices.Pwm "TinyCLR Devices\Drivers\DevicesInterop\Pwm"
CALL :Copy GHIElectronics.TinyCLR.Devices.Storage "TinyCLR Devices\Drivers\DevicesInterop\Storage"
CALL :Copy GHIElectronics.TinyCLR.Devices.Uart "TinyCLR Devices\Drivers\DevicesInterop\Uart"
CALL :Copy GHIElectronics.TinyCLR.Devices.Rtc "TinyCLR Devices\Drivers\DevicesInterop\Rtc"
CALL :Copy GHIElectronics.TinyCLR.Devices.Signals "TinyCLR Devices\Drivers\DevicesInterop\Signals"
CALL :Copy GHIElectronics.TinyCLR.Devices.Spi "TinyCLR Devices\Drivers\DevicesInterop\Spi"

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
