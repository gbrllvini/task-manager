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

function Invoke-MigrationWithRetry {
    $maxAttempts = 10
    $delaySeconds = 3

    for ($attempt = 1; $attempt -le $maxAttempts; $attempt++) {
        try {
            dotnet ef database update `
                --project "$repoRoot/backend/TaskManager.Infrastructure/TaskManager.Infrastructure.csproj" `
                --startup-project "$repoRoot/backend/TaskManager.API/TaskManager.API.csproj"
            return
        }
        catch {
            if ($attempt -eq $maxAttempts) {
                throw
            }

            Write-Host "Migration attempt $attempt failed. Retrying in $delaySeconds seconds..." -ForegroundColor Yellow
            Start-Sleep -Seconds $delaySeconds
        }
    }
}

Set-Location $repoRoot

if ($Mode -eq "docker") {
    Invoke-Step "Starting PostgreSQL container" {
        docker compose up -d postgres
    }

    Invoke-Step "Restoring .NET dependencies" {
        dotnet restore "$repoRoot/backend/TaskManager.sln"
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

        docker @composeArgs
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

Invoke-Step "Restoring .NET dependencies" {
    dotnet restore "$repoRoot/backend/TaskManager.sln"
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
    }

    Invoke-Step "Installing frontend dependencies when needed" {
        if (-not (Test-Path "$repoRoot/frontend/node_modules")) {
            Push-Location "$repoRoot/frontend"
            npm install
            Pop-Location
        }
    }

    Invoke-Step "Starting frontend at http://localhost:4200" {
        Set-Location "$repoRoot/frontend"
        npm start
    }
}
finally {
    if ($null -ne $apiProcess -and -not $apiProcess.HasExited) {
        Stop-Process -Id $apiProcess.Id -Force
    }
}
