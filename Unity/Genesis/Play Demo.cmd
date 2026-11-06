@echo off
set "DEMO=%~dp0Builds\GenesisDemo\GenesisDemo.exe"
if exist "%DEMO%" (
  start "Genesis Playtest" "%DEMO%" -screen-width 1280 -screen-height 800 -screen-fullscreen 0 -logFile "%~dp0Logs\playtest-player.log"
) else (
  echo The standalone demo has not finished building.
  echo In Unity open Assets/Genesis/Scenes/GenesisDemo.unity and press Play.
  pause
)