$ErrorActionPreference = 'Stop'

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = Split-Path -Parent $scriptRoot
$projectPath = Join-Path $repoRoot 'src/PublicApi/PublicApi.csproj'
$skipToken = '__SKIP__'

if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    Write-Error 'dotnet CLI was not found in PATH.'
    exit 1
}

if (-not (Test-Path $projectPath)) {
    Write-Error "Could not find project at $projectPath"
    exit 1
}

function Read-SettingValue {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Prompt,
        [string]$Default = '',
        [switch]$Secret
    )

    $suffix = if ($Default -ne '') { " [$Default]" } else { '' }

    if ($Secret) {
        $secureInput = Read-Host -Prompt ($Prompt + $suffix) -AsSecureString
        $plain = [System.Net.NetworkCredential]::new('', $secureInput).Password

        if ($plain -ieq 'skip') {
            return $skipToken
        }

        if ([string]::IsNullOrWhiteSpace($plain)) {
            return $Default
        }

        return $plain
    }

    $value = Read-Host -Prompt ($Prompt + $suffix)

    if ($value -ieq 'skip') {
        return $skipToken
    }

    if ([string]::IsNullOrWhiteSpace($value)) {
        return $Default
    }

    return $value
}

function Set-UserSecret {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Key,
        [Parameter(Mandatory = $true)]
        [string]$Value
    )

    if ($Value -eq $skipToken -or [string]::IsNullOrEmpty($Value)) {
        Write-Host "Skipping $Key" -ForegroundColor Yellow
        return
    }

    dotnet user-secrets set --project "$projectPath" "$Key" "$Value" | Out-Null
    Write-Host "Set $Key" -ForegroundColor Green
}

Write-Host 'Configure PublicApi user secrets from appsettings values.' -ForegroundColor Cyan
Write-Host "Press Enter to accept a default value when shown, or type 'skip' to leave a key unchanged." -ForegroundColor Cyan
Write-Host ''

$commonSettings = @(
    @{ Key = 'Auth0:Audience'; Prompt = 'Auth0 Audience'; Default = '' },
    @{ Key = 'Auth0:Domain'; Prompt = 'Auth0 Domain'; Default = '' },
    @{ Key = 'Auth0:Management:ClientId'; Prompt = 'Auth0 Management ClientId'; Default = '' },
    @{ Key = 'ConnectionStrings:DefaultConnection'; Prompt = 'Connection string'; Default = 'Host=localhost;Port=5432;Database=algowars;Username=myuser;Password=mypassword' },
    @{ Key = 'ExecutionEngines:Judge0:Enabled'; Prompt = 'Judge0 Enabled (true/false)'; Default = 'true' },
    @{ Key = 'ExecutionEngines:Judge0:RunWorker'; Prompt = 'Judge0 RunWorker (true/false)'; Default = 'true' },
    @{ Key = 'ExecutionEngines:Judge0:BaseUrl'; Prompt = 'Judge0 BaseUrl'; Default = '' },
    @{ Key = 'ExecutionEngines:Judge0:ApiKey'; Prompt = 'Judge0 ApiKey'; Default = ''; Secret = $true },
    @{ Key = 'ExecutionEngines:Judge0:Host'; Prompt = 'Judge0 Host'; Default = '' },
    @{ Key = 'ExecutionEngines:Judge0:ShouldWait'; Prompt = 'Judge0 ShouldWait (true/false)'; Default = 'false' },
    @{ Key = 'ExecutionEngines:Judge0:IsEncoded'; Prompt = 'Judge0 IsEncoded (true/false)'; Default = 'true' },
    @{ Key = 'ExecutionEngines:Judge0:DefaultTimeoutInSeconds'; Prompt = 'Judge0 timeout in seconds'; Default = '10' }
)

$rabbitMqSettings = @(
    @{ Key = 'MessageBus:RabbitMQ:Host'; Prompt = 'RabbitMQ Host'; Default = 'localhost' },
    @{ Key = 'MessageBus:RabbitMQ:VirtualHost'; Prompt = 'RabbitMQ VirtualHost'; Default = '/' },
    @{ Key = 'MessageBus:RabbitMQ:Username'; Prompt = 'RabbitMQ Username'; Default = 'guest' },
    @{ Key = 'MessageBus:RabbitMQ:Password'; Prompt = 'RabbitMQ Password'; Default = 'guest'; Secret = $true }
)

$azureServiceBusSettings = @(
    @{ Key = 'MessageBus:AzureServiceBus:ConnectionString'; Prompt = 'Azure Service Bus ConnectionString'; Default = ''; Secret = $true }
)

foreach ($setting in $commonSettings) {
    $value = Read-SettingValue -Prompt $setting.Prompt -Default $setting.Default -Secret:([bool]($setting.Secret))
    Set-UserSecret -Key $setting.Key -Value $value
}

$transport = Read-SettingValue -Prompt 'MessageBus Transport (RabbitMQ/AzureServiceBus)' -Default 'RabbitMQ'
Set-UserSecret -Key 'MessageBus:Transport' -Value $transport

if ($transport -ieq 'AzureServiceBus') {
    foreach ($setting in $azureServiceBusSettings) {
        $value = Read-SettingValue -Prompt $setting.Prompt -Default $setting.Default -Secret:([bool]($setting.Secret))
        Set-UserSecret -Key $setting.Key -Value $value
    }
}
else {
    foreach ($setting in $rabbitMqSettings) {
        $value = Read-SettingValue -Prompt $setting.Prompt -Default $setting.Default -Secret:([bool]($setting.Secret))
        Set-UserSecret -Key $setting.Key -Value $value
    }
}

Write-Host ''
Write-Host 'Done. User secrets have been applied to PublicApi.' -ForegroundColor Cyan
