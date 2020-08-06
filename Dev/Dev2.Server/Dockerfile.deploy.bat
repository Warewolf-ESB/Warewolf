@echo off
if "%Username%"=="" (
    echo.
    echo Please enter any username: (for example: WarewolfAdmin)
    set /p Username=
)
if "%Password%"=="" (
    echo.
    echo Please enter any password greater than 8 upper, lower, number and symbol characters: (for example: W@rEw0lf@dm1n)
    set /p Password=
)
if "%Port%"=="" (
    echo.
    echo Please enter any valid port number:
    set /p Port=
)
set /A SecurePort=%Port%+1
@echo on

docker run -d -m 4g -e SERVER_USERNAME=%Username% -e SERVER_PASSWORD=%Password% -p %SecurePort%:3142 -p %Port%:3143 warewolfserver/warewolfserver