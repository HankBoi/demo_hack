#!/usr/bin/env bash
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
export PATH="${HOME}/.dotnet:${PATH}"
export DOTNET_ROOT="${HOME}/.dotnet"
export DOTNET_CLI_TELEMETRY_OPTOUT=1
export ASPNETCORE_ENVIRONMENT="${ASPNETCORE_ENVIRONMENT:-Development}"
# The browser calls the Next server. Next forwards /api to the API on this machine.
export API_PROXY_TARGET="${API_PROXY_TARGET:-http://127.0.0.1:43124}"
unset NEXT_PUBLIC_API_BASE_URL

mkdir -p "${ROOT}/data"
cd "${ROOT}"

echo "API http://127.0.0.1:43124"
ASPNETCORE_URLS="http://127.0.0.1:43124" dotnet run --no-launch-profile --project api/TenderEvidenceChecker.Api &
API_PID=$!
dotnet run --no-launch-profile --project api/TenderEvidenceChecker.Api -- --worker &
WORKER_PID=$!

cleanup() {
  kill "${API_PID}" "${WORKER_PID}" 2>/dev/null || true
}
trap cleanup EXIT INT TERM

cd "${ROOT}/web"
pnpm exec next dev --hostname 127.0.0.1 --port 43123
