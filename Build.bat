ECHO OFF

SET Target=%1
SET Configuration=%2

CALL "%~dp0\..\TinyCLR_Firmware\src\setenv_gcc.cmd" 5.4.1 "C:\Program Files (x86)\GNU Tools ARM Embedded\5.4 2016q3"

msbuild "%~dp0\Subset_of_CorLib\SpotCorLib.csproj" /p:TinyCLR_Platform=Client;SuppressMscorlibReference=True;AssemblyName=GHIElectronics.TinyCLR.Core;Flavor=%Configuration%;Configuration=%Configuration% /t:%Target%

robocopy "%~dp0\..\TinyCLR_Firmware\src\BuildOutput\Public\%Configuration%\Client\dll" "%~dp0\Build" *.dll *.pdb *.pe *.pdbx

nuget pack "%~dp0\NuGet\GHIElectronics.TinyCLR.Core.nuspec" -OutputDirectory "%~dp0\Build"

robocopy "%~dp0\Build" "%~dp0\..\TinyCLR SDK\Build\bin\%Configuration%" *.nupkg

IF %ERRORLEVEL% LSS 8 EXIT /b 0