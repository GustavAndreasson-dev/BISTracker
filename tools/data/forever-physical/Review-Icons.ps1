$ErrorActionPreference='Stop'
$factsDir=Join-Path $PSScriptRoot '../../../docs/data/forever-physical'
$items=Get-Content -Raw (Join-Path $factsDir 'upgrade-item-metadata.json')|ConvertFrom-Json -AsHashtable
$urls=@($items.Values.iconUrl|Sort-Object -Unique)
$results=@($urls|ForEach-Object -Parallel {
 $r=Invoke-WebRequest -Method Head $_
 if($r.StatusCode -ne 200 -or $r.Headers['Content-Type'] -notmatch 'image') {throw "Invalid item icon $_"}
 [ordered]@{url=$_;httpStatus=[int]$r.StatusCode;contentType=[string]$r.Headers['Content-Type'];reviewedOn='2026-10-08'}
} -ThrottleLimit 6)
$results|Sort-Object url|ConvertTo-Json -Depth 4|Set-Content (Join-Path $factsDir 'icon-status.json')
Write-Output "$($results.Count) distinct item icon URLs HTTP200 with image content type"
