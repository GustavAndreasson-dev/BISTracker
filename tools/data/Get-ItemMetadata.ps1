#Requires -Version 7.0
[CmdletBinding()]
param(
    [string]$InputPath = (Join-Path $PSScriptRoot '../../docs/data/phase1-candidates.json'),
    [string]$OutputPath = (Join-Path $PSScriptRoot '../../docs/data/phase1-item-metadata.json')
)

$ErrorActionPreference = 'Stop'
$manifest = Get-Content -LiteralPath $InputPath -Raw | ConvertFrom-Json
if ($manifest.schemaVersion -ne 1 -or @($manifest.items).Count -eq 0) {
    throw 'Expected a version 1 candidate manifest containing items.'
}

$results = [System.Collections.Generic.List[object]]::new()
foreach ($candidate in $manifest.items) {
    $itemId = 0
    if (-not [int]::TryParse([string]$candidate.itemId, [ref]$itemId) -or $itemId -le 0) {
        throw 'Every candidate must have a positive numeric item ID.'
    }
    if ($candidate.acquisitionType -notin @('Dungeon', 'Quest')) {
        throw "Item $itemId does not match the dungeon/quest scope."
    }

    # Experimental, observed Wowhead tooltip endpoint. Not a stable API contract.
    $metadataUrl = "https://nether.wowhead.com/classic/tooltip/item/${itemId}?dataEnv=4&locale=0"
    $metadata = Invoke-RestMethod -Uri $metadataUrl -TimeoutSec 20
    if ([string]::IsNullOrWhiteSpace($metadata.name) -or $metadata.icon -notmatch '^[a-z0-9_]+$') {
        throw "Item $itemId returned invalid name or icon metadata."
    }
    if ($metadata.name -cne $candidate.expectedName) {
        throw "Item $itemId name mismatch: expected '$($candidate.expectedName)', received '$($metadata.name)'."
    }

    $iconUrl = "https://wow.zamimg.com/images/wow/icons/large/$($metadata.icon).jpg"
    $image = Invoke-WebRequest -Uri $iconUrl -Method Head -TimeoutSec 20
    if ($image.StatusCode -ne 200 -or [string]$image.Headers['Content-Type'] -notmatch '^image/') {
        throw "Item $itemId has an unusable icon URL."
    }

    $results.Add([ordered]@{
        itemId = $itemId
        name = $metadata.name
        quality = $metadata.quality
        slot = $candidate.slot
        alternative = [bool]$candidate.alternative
        requiredSuffix = $candidate.requiredSuffix
        acquisitionType = $candidate.acquisitionType
        acquisition = $candidate.acquisition
        itemUrl = "https://www.wowhead.com/classic/item=$itemId"
        iconName = $metadata.icon
        iconUrl = $iconUrl
        metadataUrl = $metadataUrl
        recommendationSourceUrl = $candidate.recommendationSourceUrl
        phaseVerification = $candidate.phaseVerification
        iconHttpStatus = [int]$image.StatusCode
        retrievedAtUtc = [DateTimeOffset]::UtcNow.ToString('O')
    })
    Write-Output "Verified item $itemId : $($metadata.name) (metadata and icon URL)"
}

# Write only after all requests and validation succeed; preserve previous output on failure.
$resolvedOutput = [System.IO.Path]::GetFullPath($OutputPath)
[System.IO.Directory]::CreateDirectory([System.IO.Path]::GetDirectoryName($resolvedOutput)) | Out-Null
$temporaryOutput = "$resolvedOutput.$([guid]::NewGuid().ToString('N')).tmp"
try {
    $output = [ordered]@{
        schemaVersion = 1
        status = 'Candidate metadata, not an approved complete BiS catalog'
        context = $manifest.context
        caveats = @(
            'Tooltip metadata does not verify historic phase availability or the acquisition source.',
            'Random-suffix items require the stated suffix; the base tooltip does not identify that variant.',
            'Icon URLs were checked with HEAD requests; images were not downloaded or redistributed.',
            'Wowhead tooltip endpoints and CDN URLs are external dependencies without a guaranteed stable contract.'
        )
        items = $results.ToArray()
    }
    [System.IO.File]::WriteAllText($temporaryOutput, ($output | ConvertTo-Json -Depth 10) + "`n", [System.Text.UTF8Encoding]::new($false))
    [System.IO.File]::Move($temporaryOutput, $resolvedOutput, $true)
}
finally {
    if ([System.IO.File]::Exists($temporaryOutput)) { [System.IO.File]::Delete($temporaryOutput) }
}
Write-Output "Saved $($results.Count) candidate metadata records to $resolvedOutput"
