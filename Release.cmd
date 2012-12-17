@echo Off
set majorRev=%1
if "%majorRev%" == "" (
	GOTO MESSAGE   
)

set minorRev=%2
if "%minorRev%" == "" (
	GOTO MESSAGE   
)

set patch=%3
if "%patch%" == "" (
	GOTO MESSAGE
)

:CORE
attrib -R *.* /S /D
%WINDIR%\Microsoft.NET\Framework\v4.0.30319\msbuild Release\Release.proj /p:MajorVersion="%majorRev%";MinorVersion="%minorRev%";Build="%patch%" /m /v:M /fl /flp:LogFile=release.log;Verbosity=Normal /nr:false 
GOTO END

:MESSAGE
echo.
echo.
echo Creates a new release of the Candor Core Libraries
echo.
echo Release ^<major^> ^<minor^> ^<patch^> 
echo.release
echo i.e. Release 2 1 0
echo in order to create release versioned 2.1.0.
echo.
echo Saves results to .\NuGetRelease\ 
echo.
echo.

:END