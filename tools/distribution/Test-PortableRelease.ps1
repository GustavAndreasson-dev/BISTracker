param([Parameter(Mandatory=$true)][string]$ZipPath,
    [Parameter(Mandatory=$true)][string]$CheckHostDirectory)
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.IO.Compression.FileSystem
Add-Type -AssemblyName UIAutomationClient
Add-Type -AssemblyName UIAutomationTypes
$zip = (Resolve-Path -LiteralPath $ZipPath).Path
$checkHost = (Resolve-Path -LiteralPath $CheckHostDirectory).Path
$reports = Join-Path (Split-Path $zip) ('verification-' + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $reports | Out-Null
$reportPath = Join-Path $reports 'distribution-verification.json'
[ordered]@{ result = 'PENDING'; zipFile = [IO.Path]::GetFileName($zip); startedAtUtc = [DateTime]::UtcNow.ToString('o') } |
    ConvertTo-Json | Set-Content -LiteralPath $reportPath -Encoding UTF8
try {
$testRoot = Join-Path $env:TEMP ('BISTracker distribution ' + [Guid]::NewGuid().ToString('N'))
$app = Join-Path $testRoot 'Extracted application'
$updatedApp = Join-Path $testRoot 'Updated application'
$profile = Join-Path $testRoot 'Isolated player data'
New-Item -ItemType Directory -Path $testRoot | Out-Null
[IO.Compression.ZipFile]::ExtractToDirectory($zip, $app)
$manifest = Get-Content -LiteralPath (Join-Path $app 'release-manifest.json') -Raw | ConvertFrom-Json
$fileCount = @(Get-ChildItem -LiteralPath $app -File -Recurse).Count
if ($fileCount -ne $manifest.files.Count + 1) { throw 'Unexpected ZIP file count.' }
foreach ($file in $manifest.files) {
    $actual = Join-Path $app $file.path
    if ((Get-FileHash -LiteralPath $actual -Algorithm SHA256).Hash -ne $file.sha256) { throw "ZIP hash mismatch: $($file.path)" }
    if ($file.path -match '(?i)(^|/)(characters-v1\.json|.*progress\.json|bin|obj|checks)(/|$)|\.pdb$|\.pfx$') {
        throw "Private or development content in ZIP: $($file.path)"
    }
}
foreach ($name in @('Domain', 'Application', 'Infrastructure')) {
    $file = "BISTracker.$name.dll"
    if ((Get-FileHash (Join-Path $app $file)).Hash -ne (Get-FileHash (Join-Path $checkHost $file)).Hash) {
        throw "Check host uses different $file."
    }
}
if ([Text.Encoding]::UTF8.GetString([IO.File]::ReadAllBytes((Join-Path $app 'BISTracker.Presentation.dll'))).Contains('DraftPreview')) {
    throw 'Draft preview code must not be shipped.'
}
$repo = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
& python (Join-Path $PSScriptRoot 'Inspect-NativeDependencies.py') $app (Join-Path $reports 'native-dependencies.json')
if ($LASTEXITCODE -ne 0) { throw 'Native dependency inspection failed.' }
$nativeAudit = Get-Content -LiteralPath (Join-Path $reports 'native-dependencies.json') -Raw | ConvertFrom-Json
$previousPreference = $ErrorActionPreference
try {
    $ErrorActionPreference = 'Continue'
    $checksOutput = @(& (Join-Path $checkHost 'BISTracker.Checks.exe') 2>&1)
    $checksExitCode = $LASTEXITCODE
}
finally { $ErrorActionPreference = $previousPreference }
$checksOutput | Set-Content -LiteralPath (Join-Path $reports 'behavior-checks.txt') -Encoding UTF8
if ($checksExitCode -ne 0) { throw 'Published behavior checks failed; see behavior-checks.txt.' }
$passed = @($checksOutput | Where-Object { $_ -match '^PASS:' }).Count
if ($passed -ne 49 -or @($checksOutput | Where-Object { $_ -match '^PASS: Distribution:' }).Count -ne 3) {
    throw 'Expected all 49 behavior checks, including the three distribution scenarios.'
}
& (Join-Path $checkHost 'BISTracker.Checks.exe') --catalog-release-audit (Join-Path $reports 'catalog-release-audit.json')
if ($LASTEXITCODE -ne 0) { throw 'Published catalog audit failed.' }
$catalogAudit = Get-Content -LiteralPath (Join-Path $reports 'catalog-release-audit.json') -Raw | ConvertFrom-Json
if (!$catalogAudit.releaseReady -or $catalogAudit.checkedCatalogs -ne 27 -or $catalogAudit.blockedCatalogs -ne 0) {
    throw 'Expected 27 complete Forever catalogs.'
}

function Wait-Until([scriptblock]$Condition, [string]$Message) {
    $limit = [DateTime]::UtcNow.AddSeconds(35)
    while ([DateTime]::UtcNow -lt $limit) {
        if (& $Condition) { return }
        Start-Sleep -Milliseconds 200
    }
    throw $Message
}
function Start-TestApp([string]$Directory) {
    $info = New-Object Diagnostics.ProcessStartInfo
    $info.FileName = Join-Path $Directory 'BISTracker.Presentation.exe'
    $info.WorkingDirectory = $Directory
    $info.UseShellExecute = $false
    $info.WindowStyle = [Diagnostics.ProcessWindowStyle]::Hidden
    $info.EnvironmentVariables['BISTRACKER_DATA_DIRECTORY'] = $profile
    $info.EnvironmentVariables['DOTNET_ROOT'] = Join-Path $testRoot 'No global runtime'
    $info.EnvironmentVariables['DOTNET_ROOT_X64'] = Join-Path $testRoot 'No global runtime'
    $info.EnvironmentVariables['DOTNET_MULTILEVEL_LOOKUP'] = '0'
    $process = [Diagnostics.Process]::Start($info)
    try {
        Wait-Until { $process.Refresh(); if ($process.HasExited) { throw "Published application exited: $($process.ExitCode)" }; $process.MainWindowHandle -ne [IntPtr]::Zero } 'Published window did not open.'
        return $process
    }
    catch { if (!$process.HasExited) { $process.Kill() }; throw }
}
function Find-CheckBox($Process, [string]$Name) {
    $Process.Refresh()
    $root = [Windows.Automation.AutomationElement]::FromHandle($Process.MainWindowHandle)
    if ($root.Current.ProcessId -ne $Process.Id) { throw 'Automation window belongs to another process.' }
    return $root.FindFirst([Windows.Automation.TreeScope]::Descendants,
        (New-Object Windows.Automation.PropertyCondition([Windows.Automation.AutomationElement]::NameProperty, $Name)))
}
function Close-TestApp($Process) {
    if ($Process -and !$Process.HasExited) {
        if (!$Process.CloseMainWindow() -or !$Process.WaitForExit(10000)) {
            $Process.Kill()
            throw 'Test application did not close gracefully.'
        }
    }
}
$process = $null
$normalProfile = Join-Path $env:LOCALAPPDATA 'BISTracker'
function Get-NormalProfileHashes {
    $hashes = @{}
    if (Test-Path -LiteralPath $normalProfile) {
        Get-ChildItem -LiteralPath $normalProfile -File -Recurse | ForEach-Object { $hashes[$_.FullName] = (Get-FileHash -LiteralPath $_.FullName).Hash }
    }
    return $hashes
}
$before = Get-NormalProfileHashes
$playerDataUnchanged = $null
try {
    $process = Start-TestApp $app
    $script:checkbox = $null
    Wait-Until { $script:checkbox = Find-CheckBox $process 'Crimson Felt Hat, equipped'; $null -ne $script:checkbox } 'Classic equipment checkbox was not available.'
    $toggle = $checkbox.GetCurrentPattern([Windows.Automation.TogglePattern]::Pattern)
    if ($toggle.Current.ToggleState -ne [Windows.Automation.ToggleState]::Off) { throw 'Fresh test profile must have no equipment.' }
    $toggle.Toggle()
    $progressPath = Join-Path $profile 'characters-v1.json'
    Wait-Until { if (!(Test-Path -LiteralPath $progressPath)) { return $false };
        try { $data = Get-Content -LiteralPath $progressPath -Raw | ConvertFrom-Json }
        catch [IO.IOException] { return $false }
        $character = @($data.characters | Where-Object { $_.id -eq $data.activeCharacterId })[0];
        @($character.ownedItemKeys).Count -eq 1 -and (($character.equippedBySpec | ConvertTo-Json -Depth 8) -match 'head') } 'Actual checkbox did not persist owned and equipped state.'
    $modulePaths = @($process.Modules | Where-Object { $_.ModuleName -in @('coreclr.dll', 'hostfxr.dll', 'hostpolicy.dll', 'Microsoft.UI.Xaml.dll', 'Microsoft.WindowsAppRuntime.dll') } | ForEach-Object { $_.FileName })
    foreach ($path in $modulePaths) {
        if (!$path.StartsWith($app + '\', [StringComparison]::OrdinalIgnoreCase)) { throw 'Observed runtime module was loaded outside the package.' }
    }
    foreach ($name in @('coreclr.dll', 'Microsoft.UI.Xaml.dll', 'Microsoft.WindowsAppRuntime.dll')) {
        $loaded = @($modulePaths | Where-Object { [IO.Path]::GetFileName($_) -eq $name })
        if ($loaded.Count -ne 1 -or !$loaded[0].StartsWith($app + '\', [StringComparison]::OrdinalIgnoreCase)) { throw "Runtime is not loaded from extracted package: $name" }
    }
    Close-TestApp $process
    $process = $null
    $savedHash = (Get-FileHash -LiteralPath $progressPath).Hash
    $process = Start-TestApp $app
    Wait-Until { $box = Find-CheckBox $process 'Crimson Felt Hat, equipped'; $box -and $box.GetCurrentPattern([Windows.Automation.TogglePattern]::Pattern).Current.ToggleState -eq [Windows.Automation.ToggleState]::On } 'Equipment did not survive application restart.'
    Close-TestApp $process
    $process = $null
    [IO.Compression.ZipFile]::ExtractToDirectory($zip, $updatedApp)
    $process = Start-TestApp $updatedApp
    Wait-Until { $box = Find-CheckBox $process 'Crimson Felt Hat, equipped'; $box -and $box.GetCurrentPattern([Windows.Automation.TogglePattern]::Pattern).Current.ToggleState -eq [Windows.Automation.ToggleState]::On } 'Changing the application folder lost equipment.'
    Close-TestApp $process
    $process = $null
    if ((Get-FileHash -LiteralPath $progressPath).Hash -ne $savedHash) { throw 'Restart or folder update unexpectedly changed saved data.' }
    $os = Get-CimInstance Win32_OperatingSystem
    [ordered]@{ result = 'PASS'; version = $manifest.version; sourceCommit = $manifest.sourceCommit;
        sourceWorkingTreeDirty = $manifest.sourceWorkingTreeDirty; zipFile = [IO.Path]::GetFileName($zip);
        sha256 = (Get-FileHash -LiteralPath $zip).Hash.ToLowerInvariant(); bytes = (Get-Item -LiteralPath $zip).Length;
        files = $fileCount; behaviorChecksPassed = $passed; identicalApplicationLayerAssemblies = $true;
        readyForeverCatalogs = $catalogAudit.checkedCatalogs; blockedForeverCatalogs = $catalogAudit.blockedCatalogs;
        inspectedPeBinaries = $nativeAudit.inspectedBinaryCount; missingAppLocalVcDependencies = $nativeAudit.missingAppLocalVcDependencies.Count;
        zipIntegrityVerified = $true; normalProductionUiTested = $true; actualCheckboxSaveAndRestart = $true;
        programFolderUpdatePreservesData = $true; normalPlayerDataUnchanged = $true;
        loadedRuntimeFiles = @($modulePaths | ForEach-Object { $_.Substring($app.Length + 1) });
        operatingSystem = $os.Caption; operatingSystemVersion = $os.Version;
        cleanPcTested = $false; windowsSandboxAvailable = (Test-Path "$env:WINDIR/System32/WindowsSandbox.exe");
        unsignedPortableZip = $true; testedAtUtc = [DateTime]::UtcNow.ToString('o') } |
        ConvertTo-Json -Depth 6 | Set-Content -LiteralPath $reportPath -Encoding UTF8
}
finally {
    if ($process -and !$process.HasExited) { $process.Kill(); if (!$process.WaitForExit(5000)) { throw 'Test application did not stop.' } }
    $after = Get-NormalProfileHashes
    $playerDataUnchanged = $before.Count -eq $after.Count
    foreach ($path in $before.Keys) { if ($after[$path] -ne $before[$path]) { $playerDataUnchanged = $false } }
    if (!$playerDataUnchanged) { throw 'Normal player data was modified.' }
}
Write-Output "PASS: ZIP integrity; $passed checks; published UI save/restart/folder update; local .NET and WinUI runtimes; player data unchanged."
Write-Output "Verification report: $reportPath"
}
catch {
    [ordered]@{ result = 'FAIL'; zipFile = [IO.Path]::GetFileName($zip); error = $_.Exception.Message;
        normalPlayerDataUnchanged = $playerDataUnchanged;
        testedAtUtc = [DateTime]::UtcNow.ToString('o') } |
        ConvertTo-Json | Set-Content -LiteralPath $reportPath -Encoding UTF8
    throw
}
