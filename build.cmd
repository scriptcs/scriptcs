@if not defined _echo echo off
setlocal enabledelayedexpansion

@REM Determine if MSBuild is already in the PATH
for /f "usebackq delims=" %%I in (`where msbuild.exe 2^>nul`) do (
    goto Start
)

echo.
echo MSBuild is not found in your PATH
echo Run build.cmd from Visual Studio Command Prompt or Developer Command Prompt
goto end

:Start
if exist artifacts goto Build
mkdir artifacts

:Build
msbuild Build\Build.proj /nologo /m /v:M %* /fl /flp:LogFile=artifacts\msbuild.log;Verbosity=Diagnostic;DetailedSummary /nr:false 

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