@echo off
cls
echo Build preparation....
echo.
echo PRESS CTRL+C for end all this.
echo.
pause
cd %CD%
echo Select you platform (30 secs - default platform = Any CPU)
echo.
echo 1 = Any CPU 
echo 2 = x86
echo 3 = x64
CHOICE /C 123 /N /T 30 /D 1 /M "Select you build:"
IF ERRORLEVEL 1 SET Platform=Any CPU
IF ERRORLEVEL 2 SET Platform=x86
IF ERRORLEVEL 3 SET Platform=x64
echo.
echo ............ You chose %Platform%
echo.
echo Select you configuration (30 secs - default configuration = Release)
echo.
echo 1 = Release
echo 2 = Debug
CHOICE /C 12 /N /T 30 /D 1 /M "Select you build configuration:"
IF ERRORLEVEL 1 SET Release=Release
IF ERRORLEVEL 2 SET Release=Debug
echo.
ECHO ............ You chose %Release%
echo.
echo Build initialised .... Wait..
echo.
echo ............ Build: %Release% - %Platform%
echo.
nuget.exe restore "PokemonGoGUI.sln"
echo.
echo ............ Search builders ............
echo.
echo ............ Wait ............
echo.
echo ............ Search ............
echo.
for /f "delims=" %%i in ('dir /s /b /a-d "%programfiles(x86)%\MSBuild.exe"') do (set PokemonGoGUI="%%i")
%PokemonGoGUI% "PokemonGoGUI.sln" /property:Configuration="%Release%" /property:Platform="%Platform%"
echo.
set PokemonGoGUI=
echo.
echo ............ Finished :)
echo.
pause
