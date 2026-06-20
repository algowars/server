#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(dirname "$SCRIPT_DIR")"
PROJECT_PATH="$REPO_ROOT/src/PublicApi/PublicApi.csproj"
SKIP_TOKEN="__SKIP__"

if ! command -v dotnet >/dev/null 2>&1; then
  echo "dotnet CLI was not found in PATH." >&2
  exit 1
fi

if [ ! -f "$PROJECT_PATH" ]; then
  echo "Could not find project at $PROJECT_PATH" >&2
  exit 1
fi

read_value() {
  local prompt="$1"
  local default_value="${2-}"
  local secret="${3-false}"
  local value=""

  if [ -n "$default_value" ]; then
    prompt="$prompt [$default_value]"
  fi

  if [ "$secret" = "true" ]; then
    read -r -s -p "$prompt: " value
    echo >&2
  else
    read -r -p "$prompt: " value
  fi

  if [ "$value" = "skip" ] || [ "$value" = "SKIP" ]; then
    printf '%s' "$SKIP_TOKEN"
    return
  fi

  if [ -z "$value" ]; then
    value="$default_value"
  fi

  printf '%s' "$value"
}

set_secret() {
  local key="$1"
  local value="$2"

  if [ "$value" = "$SKIP_TOKEN" ] || [ -z "$value" ]; then
    echo "Skipping $key"
    return
  fi

  dotnet user-secrets set --project "$PROJECT_PATH" "$key" "$value" >/dev/null
  echo "Set $key"
}

echo "Configure PublicApi user secrets from appsettings values."
echo "Press Enter to accept a default value when shown, or type 'skip' to leave a key unchanged."
echo

KEYS=(
  "Auth0:Audience"
  "Auth0:Domain"
  "Auth0:Management:ClientId"
  "ConnectionStrings:DefaultConnection"
  "ExecutionEngines:Judge0:Enabled"
  "ExecutionEngines:Judge0:RunWorker"
  "ExecutionEngines:Judge0:BaseUrl"
  "ExecutionEngines:Judge0:ApiKey"
  "ExecutionEngines:Judge0:Host"
  "ExecutionEngines:Judge0:ShouldWait"
  "ExecutionEngines:Judge0:IsEncoded"
  "ExecutionEngines:Judge0:DefaultTimeoutInSeconds"
)

PROMPTS=(
  "Auth0 Audience"
  "Auth0 Domain"
  "Auth0 Management ClientId"
  "Connection string"
  "Judge0 Enabled (true/false)"
  "Judge0 RunWorker (true/false)"
  "Judge0 BaseUrl"
  "Judge0 ApiKey"
  "Judge0 Host"
  "Judge0 ShouldWait (true/false)"
  "Judge0 IsEncoded (true/false)"
  "Judge0 timeout in seconds"
)

DEFAULTS=(
  ""
  ""
  ""
  "Host=localhost;Port=5432;Database=algowars;Username=myuser;Password=mypassword"
  "true"
  "true"
  ""
  ""
  ""
  "false"
  "true"
  "10"
)

SECRETS=(
  "false"
  "false"
  "false"
  "false"
  "false"
  "false"
  "false"
  "true"
  "false"
  "false"
  "false"
  "false"
)

for i in "${!KEYS[@]}"; do
  value="$(read_value "${PROMPTS[$i]}" "${DEFAULTS[$i]}" "${SECRETS[$i]}")"
  set_secret "${KEYS[$i]}" "$value"
done

transport="$(read_value "MessageBus Transport (RabbitMQ/AzureServiceBus)" "RabbitMQ")"
set_secret "MessageBus:Transport" "$transport"

if [ "$transport" = "AzureServiceBus" ]; then
  value="$(read_value "Azure Service Bus ConnectionString" "" "true")"
  set_secret "MessageBus:AzureServiceBus:ConnectionString" "$value"
else
  value="$(read_value "RabbitMQ Host" "localhost")"
  set_secret "MessageBus:RabbitMQ:Host" "$value"

  value="$(read_value "RabbitMQ VirtualHost" "/")"
  set_secret "MessageBus:RabbitMQ:VirtualHost" "$value"

  value="$(read_value "RabbitMQ Username" "guest")"
  set_secret "MessageBus:RabbitMQ:Username" "$value"

  value="$(read_value "RabbitMQ Password" "guest" "true")"
  set_secret "MessageBus:RabbitMQ:Password" "$value"
fi

echo
echo "Done. User secrets have been applied to PublicApi."