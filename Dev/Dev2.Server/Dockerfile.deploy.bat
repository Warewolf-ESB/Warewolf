@echo off
if "%WarewolfContainerUsername%"=="" (
    echo.
    echo Please enter any username: \(for example: WarewolfAdmin\)
    set /p WarewolfContainerUsername=
)
if "%WarewolfContainerPassword%"=="" (
    echo.
    echo Please enter any password greater than 8 upper, lower, number and symbol characters: \(for example: W@rEw0lf@dm1n\)
    set /p WarewolfContainerPassword=
)
if "%WarewolfContainerPort%"=="" (
    echo.
    echo Please enter any valid port number:
    set /p WarewolfContainerPort=
)
set /A WarewolfContainerSecurePort=%WarewolfContainerPort%+1
@echo on

docker run -d -m 4g -e SERVER_USERNAME=%WarewolfContainerUsername% -e SERVER_PASSWORD=%WarewolfContainerPassword% -p %WarewolfContainerPort%:3142 -p %WarewolfContainerSecurePort%:3143 warewolfserver/warewolfserver