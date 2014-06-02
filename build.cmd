@echo Off
setlocal

if exist artifacts goto Build
mkdir artifacts

:Build
%WINDIR%\Microsoft.NET\Framework\v4.0.30319\msbuild Build\Build.proj /nologo /m /v:M %* /fl /flp:LogFile=artifacts\msbuild.log;Verbosity=Diagnostic;DetailedSummary /nr:false 

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