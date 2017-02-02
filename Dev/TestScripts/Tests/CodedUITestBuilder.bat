REM ** Check for admin **
echo Administrative permissions required. Detecting permissions...
REM using the "net session" command to detect admin, it requires elevation in the most operating systems - Ashley
IF EXIST %windir%\nircmd.exe (nircmd elevate net session >nul 2>&1) else (net session >nul 2>&1)
if %errorLevel% == 0 (
	echo Success: Administrative permissions confirmed.
) else (
	echo Failure: Current permissions inadequate.
	pause
	GOTO exit
)

IF EXIST "%vs120comntools%" GOTO vs2013
IF EXIST "%vs140comntools%" GOTO vs2015
Echo No visual studio installation found!
pause
GOTO exit

:vs2013
IF EXIST %windir%\nircmd.exe (nircmd elevate "%vs120comntools%..\IDE\CodedUITestBuilder.exe" /standalone) else (START "%vs120comntools%..\IDE\CodedUITestBuilder.exe" /D "%vs120comntools%..\IDE" "CodedUITestBuilder.exe" /standalone)
GOTO exit

:vs2015
IF EXIST %windir%\nircmd.exe (nircmd elevate "%vs140comntools%..\IDE\CodedUITestBuilder.exe" /standalone) else (START "%vs140comntools%..\IDE\CodedUITestBuilder.exe" /D "%vs140comntools%..\IDE" "CodedUITestBuilder.exe" /standalone)


:exit