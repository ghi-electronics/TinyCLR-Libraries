pushd .

D:
cd D:\Repos\TinyCLR_Firmware\src\

call build_firmware.bat G80 build release
call msbuild Framework\Subset_of_CorLib\SpotCorLib.csproj /p:TinyCLR_Platform=Client;SuppressMscorlibReference=True;AssemblyName=GHIElectronics.TinyCLR.Core

popd

robocopy "D:\Repos\TinyCLR_Firmware\src\BuildOutput\Public\release\Client\dll" "D:\Repos\TinyCLR Libraries\Build" *.dll *.pdb
robocopy "D:\Repos\TinyCLR_Firmware\src\BuildOutput\Public\release\Client\pe\le" "D:\Repos\TinyCLR Libraries\Build\le" *.pe *.pdbx
robocopy "D:\Repos\TinyCLR_Firmware\src\BuildOutput\Public\release\Client\pe\be" "D:\Repos\TinyCLR Libraries\Build\be" *.pe *.pdbx

cd NuGet

nuget pack GHIElectronics.TinyCLR.Core.nuspec

robocopy "." "D:\Build\NuGet" *.nupkg

cd ..