echo off

pushd "%~dp0"

::-------·þÎñÆ÷---------------------



echo create common.proto
set "errorlevel="
protogen\protogen.exe -i:proto\common.proto -o:common.cs -p:detectMissing -p:lightFramework > nul
IF %ERRORLEVEL% NEQ 0 goto ErrorLabel

echo del .\Assets\Script\Net\Proto\common.cs
if exist .\Assets\Script\Net\Proto\common.cs (
  del .\Assets\Script\Net\Proto\common.cs /F /Q
)

echo copy common.cs .\Assets\Script\Net\Proto\common.cs
copy common.cs .\Assets\Script\Net\Proto\common.cs > nul

echo del common.cs
del common.cs /F /Q

echo create ss510k.proto
set "errorlevel="
protogen\protogen.exe -i:proto\ss510k.proto -o:ss510k.cs -p:detectMissing -p:lightFramework > nul
IF %ERRORLEVEL% NEQ 0 goto ErrorLabel

echo del .\Assets\Script\Net\Proto\ss510k.cs
if exist .\Assets\Script\Net\Proto\ss510k.cs (
  del .\Assets\Script\Net\Proto\ss510k.cs /F /Q
)

echo copy ss510k.cs .\Assets\Script\Net\Proto\ss510k.cs
copy ss510k.cs .\Assets\Script\Net\Proto\ss510k.cs > nul

echo del ss510k.cs
del ss510k.cs /F /Q


goto SuccessLabel

:ErrorLabel
pause


:SuccessLabel
popd