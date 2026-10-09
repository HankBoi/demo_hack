@echo off
setlocal

REM Starts the Tender Evidence Checker locally: API, job worker, and web app.
REM Requirements: .NET SDK 10, Node.js, pnpm (all on PATH).
REM Each service runs in its own window. Close the windows (or run stop-app.bat) to stop.

set "ROOT=%~dp0"
if "%ROOT:~-1%"=="\" set "ROOT=%ROOT:~0,-1%"

set "DOTNET_CLI_TELEMETRY_OPTOUT=1"
if not defined ASPNETCORE_ENVIRONMENT set "ASPNETCORE_ENVIRONMENT=Development"
if not defined API_PROXY_TARGET set "API_PROXY_TARGET=http://127.0.0.1:43124"
REM The page calls its own /api path. Next forwards that to the API.
set "NEXT_PUBLIC_API_BASE_URL="

where dotnet >nul 2>nul
if errorlevel 1 (
  echo [ERROR] dotnet was not found on PATH. Install the .NET SDK 10.
  pause
  exit /b 1
)
where pnpm >nul 2>nul
if errorlevel 1 (
  echo [ERROR] pnpm was not found on PATH. Install Node.js and pnpm.
  pause
  exit /b 1
)

if not exist "%ROOT%\data" mkdir "%ROOT%\data"

if not exist "%ROOT%\web\node_modules" (
  echo Installing web dependencies...
  pushd "%ROOT%\web"
  call pnpm install
  if errorlevel 1 (
    popd
    echo [ERROR] pnpm install failed.
    pause
    exit /b 1
  )
  popd
)

echo Starting API on http://127.0.0.1:43124 ...
start "Tender Check - API" /D "%ROOT%" cmd /k "set ASPNETCORE_URLS=http://127.0.0.1:43124&& dotnet run --project api\TenderEvidenceChecker.Api"

echo Starting worker ...
start "Tender Check - Worker" /D "%ROOT%" cmd /k "set ASPNETCORE_URLS=http://127.0.0.1:0&& dotnet run --project api\TenderEvidenceChecker.Api -- --worker"

echo Starting web on http://127.0.0.1:43123 ...
start "Tender Check - Web" /D "%ROOT%\web" cmd /k "set API_PROXY_TARGET=http://127.0.0.1:43124&& set NEXT_PUBLIC_API_BASE_URL=&& pnpm exec next dev --hostname 127.0.0.1 --port 43123"

echo Waiting for the API window to answer...
set /a TRIES=0
:waitapi
set /a TRIES+=1
curl -sf http://127.0.0.1:43124/api/health >nul 2>&1
if not errorlevel 1 goto apiready
if %TRIES% GEQ 40 (
  echo [ERROR] The API is not answering on http://127.0.0.1:43124
  echo Read the "Tender Check - API" window. The page cannot upload until that window stays open.
  pause
  exit /b 1
)
timeout /t 2 /nobreak >nul
goto waitapi
:apiready

start "" "http://127.0.0.1:43123"

echo.
echo App:    http://127.0.0.1:43123
echo API:    http://127.0.0.1:43124
echo Health: http://127.0.0.1:43123/api/health
echo.
echo Keep the three windows open. Closing the API window makes upload fail.
endlocal
