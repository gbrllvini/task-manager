param(
    [ValidateSet("local", "docker")]
    [string]$Mode = "local",
    [switch]$WithFrontend,
    [switch]$Detached
)

$ErrorActionPreference = "Stop"
$repoRoot = Split-Path -Parent $PSScriptRoot

function Invoke-Step {
    param(
        [string]$Name,
        [scriptblock]$Action
    )

    Write-Host "==> $Name" -ForegroundColor Cyan
    & $Action
}

function Invoke-NativeCommand {
    param(
        [string]$Description,
        [scriptblock]$Command
    )

    & $Command

    if ($LASTEXITCODE -ne 0) {
        throw "$Description failed with exit code $LASTEXITCODE."
    }
}

function Get-ListeningPortInfo {
    param(
        [int]$Port
    )

    $connections = Get-NetTCPConnection -State Listen -LocalPort $Port -ErrorAction SilentlyContinue

    if (-not $connections) {
        return $null
    }

    $processId = $connections[0].OwningProcess
    $process = Get-Process -Id $processId -ErrorAction SilentlyContinue
    $processName = if ($null -ne $process) { $process.ProcessName } else { "unknown" }

    return @{
        Port = $Port
        ProcessId = $processId
        ProcessName = $processName
    }
}

function Assert-PortIsFree {
    param(
        [int]$Port
    )

    $portInfo = Get-ListeningPortInfo -Port $Port
    if ($null -eq $portInfo) {
        return
    }

    if ($portInfo.ProcessName -eq "com.docker.backend") {
        throw "Port $($portInfo.Port) is in use by Docker. Stop containers first with 'docker compose down' and retry."
    }

    throw "Port $($portInfo.Port) is already in use by PID $($portInfo.ProcessId) ($($portInfo.ProcessName)). Stop this process and retry."
}

function Invoke-MigrationWithRetry {
    $maxAttempts = 10
    $delaySeconds = 3

    for ($attempt = 1; $attempt -le $maxAttempts; $attempt++) {
        dotnet ef database update `
            --project "$repoRoot/backend/TaskManager.Infrastructure/TaskManager.Infrastructure.csproj" `
            --startup-project "$repoRoot/backend/TaskManager.API/TaskManager.API.csproj"

        if ($LASTEXITCODE -eq 0) {
            return
        }

        if ($attempt -eq $maxAttempts) {
            throw "EF migration update failed after $maxAttempts attempts."
        }

        Write-Host "Migration attempt $attempt failed. Retrying in $delaySeconds seconds..." -ForegroundColor Yellow
        Start-Sleep -Seconds $delaySeconds
    }
}

Set-Location $repoRoot

if ($Mode -eq "docker") {
    Invoke-Step "Starting PostgreSQL container" {
        Invoke-NativeCommand -Description "docker compose up postgres" -Command {
            docker compose up -d postgres
        }
    }

    Invoke-Step "Restoring .NET dependencies" {
        Invoke-NativeCommand -Description "dotnet restore" -Command {
            dotnet restore "$repoRoot/backend/TaskManager.sln"
        }
    }

    Invoke-Step "Applying database migrations" {
        Invoke-MigrationWithRetry
    }

    Invoke-Step "Starting application containers" {
        $composeArgs = @("compose")

        if ($WithFrontend) {
            $composeArgs += @("--profile", "frontend")
        }

        $composeArgs += @("up", "--build")

        if ($Detached) {
            $composeArgs += "-d"
        }

        $composeArgs += "api"

        if ($WithFrontend) {
            $composeArgs += "frontend"
        }

        Invoke-NativeCommand -Description "docker compose up" -Command {
            docker @composeArgs
        }
    }

    if ($Detached) {
        Write-Host ""
        Write-Host "Containers started in detached mode." -ForegroundColor Green
        Write-Host "- API: http://localhost:5134/swagger"
        if ($WithFrontend) {
            Write-Host "- Frontend: http://localhost:4200"
        }
    }

    return
}

Invoke-Step "Checking local ports" {
    Assert-PortIsFree -Port 5134
    Assert-PortIsFree -Port 4200
}

Invoke-Step "Restoring .NET dependencies" {
    Invoke-NativeCommand -Description "dotnet restore" -Command {
        dotnet restore "$repoRoot/backend/TaskManager.sln"
    }
}

Invoke-Step "Applying database migrations" {
    Invoke-MigrationWithRetry
}

$apiProcess = $null

try {
    Invoke-Step "Starting API at http://localhost:5134" {
        $apiProcess = Start-Process `
            -FilePath "dotnet" `
            -ArgumentList @("run", "--project", "$repoRoot/backend/TaskManager.API", "--urls", "http://localhost:5134") `
            -PassThru

        Start-Sleep -Seconds 3
        if ($apiProcess.HasExited) {
            throw "API process exited right after startup. Start it manually to inspect logs."
        }
    }

    Invoke-Step "Installing frontend dependencies when needed" {
        if (-not (Test-Path "$repoRoot/frontend/node_modules")) {
            Push-Location "$repoRoot/frontend"
            Invoke-NativeCommand -Description "npm install" -Command {
                npm install
            }
            Pop-Location
        }
    }

    Invoke-Step "Starting frontend at http://localhost:4200" {
        Set-Location "$repoRoot/frontend"
        Invoke-NativeCommand -Description "npm start" -Command {
            npm start
        }
    }
}
finally {
    if ($null -ne $apiProcess -and -not $apiProcess.HasExited) {
        Stop-Process -Id $apiProcess.Id -Force
    }
}
