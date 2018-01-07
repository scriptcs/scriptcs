@if not defined _echo echo off
setlocal EnableExtensions EnableDelayedExpansion

@echo Start pre-build setup
@echo    Restoring global NuGet packages...
@echo.
.nuget\NuGet.exe restore .nuget\packages.config -PackagesDirectory .\packages
echo.

REM Find the Visual Studio Locator utility
REM https://github.com/Microsoft/vswhere

for /f "tokens=*" %%a in ('dir packages\vswhere.exe /b /s') do set p=%%a
if not defined p (
   @echo Could not locate the Visual Studio Locator
   goto End
) 


REM Visual Studio Version
REM Legacy will find all version
for /f "tokens=1* usebackq delims=" %%v in (`%p% -legacy -version "[14.0,16.0)" -property installationVersion`) do (
   set VSVersion=%%v
   REM Only get the first item
   goto PointA
)

:PointA

REM Now find the installation directory
REM https://support.microsoft.com/en-us/help/2524009/error-running-command-shell-scripts-that-include-parentheses

REM MSBuild is not in VS2015 folder
REM So we set location to search for it  
set InstallDir=%PROGRAMFILES(X86)%\msbuild

if not "%VSVersion%" equ "14.0" (
   REM Visual Studio Installation Directory
   for /f "usebackq delims=" %%i in (`%p% -latest -version "[15.0,16.0)" -requires Microsoft.Component.MSBuild -property InstallationPath`) do (
      set InstallDir=%%i
   )
)

REM Now locate the MSBuild.exe by searching for it
set MSBuildSearch="%InstallDir%\msbuild.exe"
for /f "tokens=*" %%b in ('dir %MSBuildSearch% /b /s') do (
   set MSBuildExe=%%b
   REM only want the first one
   goto PointB
)
:PointB
if not defined MSBuildExe (
   @echo Could not locate MSBuild.exe
   goto End
) 

:Start
if exist artifacts goto Build
mkdir artifacts

:Build
@echo Starting msbuild...
"%MSBuildExe%" Build\Build.proj /nologo /m /v:M %* /fl /flp:LogFile=artifacts\msbuild.log;Verbosity=Diagnostic;DetailedSummary /nr:false 

if %ERRORLEVEL% neq 0 goto BuildFail
goto BuildSuccess

:BuildFail
echo.
echo *** BUILD FAILED ***
goto End

:BuildSuccess
echo.
echo **** BUILD SUCCESSFUL ***
goto end

:End
echo.
exit /b %ERRORLEVEL%
