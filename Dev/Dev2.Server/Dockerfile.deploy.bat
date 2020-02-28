@echo off
echo.
echo Please enter any username: (for example: WarewolfAdmin)
set /p Username=
echo.
echo Please enter any password: (for example: W@rEw0lf@dm1n)
set /p Password=
@echo on

docker run -d -m 4g -e SERVER_USERNAME=%Username% -e SERVER_PASSWORD=%Password% -p 3142:3142 -p 3143:3143 warewolfserver/warewolfserver