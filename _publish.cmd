@ECHO OFF
SET Param=-p:DebugType=None -p:DebugSymbols=false -p:SelfContained=false -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true
rmdir /s/q "%~dp0\publish"

FOR %%x IN (x86 x64) DO (
	dotnet publish -c Release -o "publish" -r win-%%x -p:PlatformTarget=%%x %Param% Clock.csproj
	rename "%~dp0\publish\Clock.exe" "Clock_%%x.exe"
)