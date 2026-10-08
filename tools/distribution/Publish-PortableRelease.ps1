param([ValidatePattern('^\d+\.\d+\.\d+$')][string]$Version = '1.0.0')
$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$source = & git -C $repo rev-parse HEAD
if ($LASTEXITCODE -ne 0) { throw 'Could not record source revision.' }
if (& git -C $repo status --porcelain) { throw 'Portable releases require committed source and a clean working tree.' }
$output = Join-Path $repo ('artifacts/distribution-' + [Guid]::NewGuid().ToString('N'))
$app = Join-Path $output 'app'
$checks = Join-Path $output 'checks'
New-Item -ItemType Directory -Path $output | Out-Null
$common = @('-c', 'Release', '-r', 'win-x64', '--self-contained', 'true',
    '-p:Platform=x64', '-p:PublishTrimmed=false', '-p:PublishReadyToRun=false',
    '-p:PublishSingleFile=false', '-p:DebugType=None', '-p:DebugSymbols=false', "-p:Version=$Version")
& dotnet publish (Join-Path $repo 'BISTracker.Presentation/BISTracker.Presentation.csproj') @common '-p:WindowsPackageType=None' '-p:WindowsAppSDKSelfContained=true' '-p:EnableDraftPreview=false' '-o' $app
if ($LASTEXITCODE -ne 0) { throw 'Application publish failed.' }
& dotnet publish (Join-Path $repo 'BISTracker.Checks/BISTracker.Checks.csproj') @common '-o' $checks
if ($LASTEXITCODE -ne 0) { throw 'Check host publish failed.' }
foreach ($name in @('Domain', 'Application', 'Infrastructure')) {
    $file = "BISTracker.$name.dll"
    if ((Get-FileHash (Join-Path $app $file)).Hash -ne (Get-FileHash (Join-Path $checks $file)).Hash) {
        throw "Check host does not use the same $file as the application."
    }
}
@'
BISTracker - Windows x64

Extract the entire ZIP into a new folder, then run BISTracker.Presentation.exe.
Keep all files and subfolders together. No installer is included.

Characters and imported catalogs are saved in %LOCALAPPDATA%\BISTracker.
To update, close the app and extract the new release into a new program folder.
Keep your BISTracker data folder. Do not run two instances using the same data.

Includes Classic Holy Priest phase 1 and all 27 Forever beta level 30 catalogs.
Real Forever level 60 catalogs are pending. Tracking works locally; icons and
source links use the internet. Missing icons do not affect saved progress.

This unsigned portable package requires Windows x64. Consult the accompanying
verification report for tested systems. Clean-computer testing is pending.
'@ | Set-Content -LiteralPath (Join-Path $app 'START-HERE.txt') -Encoding UTF8
$files = @(Get-ChildItem -LiteralPath $app -File -Recurse | Sort-Object FullName | ForEach-Object {
    [ordered]@{ path = $_.FullName.Substring($app.Length + 1).Replace('\', '/'); length = $_.Length;
        sha256 = (Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash.ToLowerInvariant() }
})
if ((& git -C $repo rev-parse HEAD) -ne $source -or (& git -C $repo status --porcelain)) { throw 'Source changed during publication.' }
[ordered]@{ version = $Version; sourceCommit = $source; sourceWorkingTreeDirty = $false;
    runtime = 'win-x64'; selfContained = $true; windowsAppSdkSelfContained = $true;
    trimmed = $false; draftPreview = $false; files = $files } |
    ConvertTo-Json -Depth 6 | Set-Content -LiteralPath (Join-Path $app 'release-manifest.json') -Encoding UTF8
Add-Type -AssemblyName System.IO.Compression.FileSystem
$zip = Join-Path $output "BISTracker-$Version-win-x64.zip"
[System.IO.Compression.ZipFile]::CreateFromDirectory($app, $zip)
$hash = (Get-FileHash -LiteralPath $zip -Algorithm SHA256).Hash.ToLowerInvariant()
"$hash  $([IO.Path]::GetFileName($zip))" | Set-Content -LiteralPath ($zip + '.sha256') -Encoding ASCII
Write-Output "Candidate ZIP: $zip"
Write-Output "Check host: $checks"
Write-Output 'Run Test-PortableRelease.ps1 before treating this candidate as verified.'
