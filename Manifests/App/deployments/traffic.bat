@echo off
setlocal enabledelayedexpansion

set CONTEXT=minikube
set POD=envoy-default-eg-e41e7b31-68d8689d4c-fjrjp
set NS=default
set LOCAL_PORT=8080
set REMOTE_PORT=10080
set PATH_SUFFIX=/Film/01a15843-85a5-7828-9bc9-9d319dd99257/details
set INTERVAL=1

set URL=http://127.0.0.1:%LOCAL_PORT%%PATH_SUFFIX%

echo Context: %CONTEXT%
echo Starting port-forward: localhost:%LOCAL_PORT% -^> %NS%/pod/%POD%:%REMOTE_PORT%
start kubectl port-forward pod/%POD% %LOCAL_PORT%:%REMOTE_PORT%

timeout /t 3 /nobreak >nul

echo.
echo Loop: %URL% every %INTERVAL%s  (Ctrl+C to stop)
echo.

:loop
set "CODE="
for /f "delims=" %%C in ('curl.exe -s -o NUL -w "%%{http_code}\n" "%URL%" 2^>nul') do set "CODE=%%C"
if not defined CODE set "CODE=ERR"
echo [!TIME!] !CODE! %URL%
timeout /t %INTERVAL% /nobreak >nul
goto loop