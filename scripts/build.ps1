[CmdletBinding()]
param(
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Release'
)

$ErrorActionPreference = 'Stop'
$projectRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$localDotnet = Join-Path $projectRoot '.tools\dotnet\dotnet.exe'
$dotnet = if (Test-Path -LiteralPath $localDotnet) { $localDotnet } else { 'dotnet' }
$env:DOTNET_CLI_TELEMETRY_OPTOUT = '1'
$env:DOTNET_GENERATE_ASPNET_CERTIFICATE = 'false'
$env:DOTNET_CLI_HOME = Join-Path $projectRoot '.tools\dotnet-home'
$env:NUGET_PACKAGES = Join-Path $projectRoot '.tools\nuget'
$testProject = Join-Path $projectRoot 'tests\GMap.NET.CMANO.SmokeTests\GMap.NET.CMANO.SmokeTests.csproj'

Push-Location $projectRoot
try {
    & $dotnet restore $testProject --locked-mode --nologo
    if ($LASTEXITCODE -ne 0) { throw 'Dependency restore failed. See the NuGet errors above; NU1004 indicates that the project dependencies do not match packages.lock.json.' }
    & $dotnet build $testProject -c $Configuration --no-restore --nologo
    if ($LASTEXITCODE -ne 0) { throw 'Build failed.' }

    $runId = [Guid]::NewGuid().ToString('N')
    $testOutput = Join-Path $projectRoot ('artifacts\smoke-' + $runId)
    New-Item -ItemType Directory -Path $testOutput -Force | Out-Null
    $testExe = Join-Path $projectRoot ('tests\GMap.NET.CMANO.SmokeTests\bin\' + $Configuration + '\net48\GMap.NET.CMANO.SmokeTests.exe')
    $testRun = Start-Process -FilePath $testExe -ArgumentList ('"' + $testOutput + '"') -WorkingDirectory $projectRoot -WindowStyle Hidden -RedirectStandardOutput (Join-Path $testOutput 'stdout.log') -RedirectStandardError (Join-Path $testOutput 'stderr.log') -PassThru
    if (-not $testRun.WaitForExit(30000)) {
        Stop-Process -Id $testRun.Id
        throw ('Smoke tests timed out. Logs: ' + $testOutput)
    }
    $testRun.Refresh()
    Get-Content -LiteralPath (Join-Path $testOutput 'stdout.log')
    Get-Content -LiteralPath (Join-Path $testOutput 'stderr.log')
    if ($testRun.ExitCode -ne 0) { throw 'Smoke tests failed.' }

    $appOutput = Join-Path $projectRoot ('src\GMap.NET.CMANO\bin\' + $Configuration + '\net48')
    $packageName = 'GMap.NET.CMANO-2.1.7-cmano.2-preview'
    $staging = Join-Path $projectRoot ('artifacts\package-' + $runId)
    $package = Join-Path $staging $packageName
    $dist = Join-Path $projectRoot 'dist'
    New-Item -ItemType Directory -Path $package, $dist -Force | Out-Null
    Get-ChildItem -LiteralPath $appOutput | Where-Object { $_.Extension -ne '.pdb' } | Copy-Item -Destination $package -Recurse
    Copy-Item -LiteralPath (Join-Path $projectRoot 'README.md'), (Join-Path $projectRoot 'THIRD_PARTY_NOTICES.md') -Destination $package
    Copy-Item -LiteralPath (Join-Path $projectRoot 'licenses'), (Join-Path $projectRoot 'docs') -Destination $package -Recurse
    foreach ($library in @('GMap.NET.Core.dll', 'GMap.NET.WindowsForms.dll')) {
        $version = [Reflection.AssemblyName]::GetAssemblyName((Join-Path $package $library)).Version
        if ($version -ne [Version]'2.1.7.0') { throw ('Incorrect packaged library: ' + $library) }
    }
    $packagedConfig = [xml](Get-Content -LiteralPath (Join-Path $package 'GMap.NET_CMANO.exe.config') -Raw)
    $dpiSetting = $packagedConfig.SelectSingleNode('/configuration/System.Windows.Forms.ApplicationConfigurationSection/add[@key="DpiAwareness"]')
    if ($null -eq $dpiSetting -or $dpiSetting.GetAttribute('value') -ne 'PerMonitorV2') {
        throw 'The packaged application is missing its PerMonitorV2 configuration.'
    }
    foreach ($architecture in @('x86', 'x64')) {
        if (-not (Test-Path -LiteralPath (Join-Path $package ($architecture + '\SQLite.Interop.dll')))) {
            throw ('Missing SQLite native dependency for ' + $architecture)
        }
    }
    $release = Join-Path $projectRoot 'Release'
    New-Item -ItemType Directory -Path $release -Force | Out-Null
    Get-ChildItem -LiteralPath $package | Copy-Item -Destination $release -Recurse -Force
    Write-Output ('Executable: ' + (Join-Path $release 'GMap.NET_CMANO.exe'))
    $archive = Join-Path $dist ($packageName + '.zip')
    Compress-Archive -LiteralPath $package -DestinationPath $archive -Force
    $digest = (Get-FileHash -LiteralPath $archive -Algorithm SHA256).Hash.ToLowerInvariant()
    [IO.File]::WriteAllText($archive + '.sha256', $digest + '  ' + [IO.Path]::GetFileName($archive) + [Environment]::NewLine)
    Write-Output ('Package: ' + $archive)
    Write-Output ('SHA256: ' + $digest)
}
finally {
    Pop-Location
}
