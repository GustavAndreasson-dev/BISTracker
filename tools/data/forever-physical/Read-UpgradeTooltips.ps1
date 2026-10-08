$ErrorActionPreference='Stop'
$researchDir=Join-Path $env:TEMP 'BISTracker-forever-physical-research/wowtbc'
$tipDir=Join-Path $researchDir 'tips'
New-Item -ItemType Directory $tipDir -Force|Out-Null
$ids=@(Get-ChildItem $researchDir -Filter '*.json'|ForEach-Object{(Get-Content -Raw $_.FullName|ConvertFrom-Json).candidates.itemId}|Sort-Object -Unique)
$ids|ForEach-Object -Parallel {
 $id=$_;$out=Join-Path $using:tipDir "$id.json"
 if(-not(Test-Path $out)) {
  $tip=Invoke-RestMethod ('https://nether.wowhead.com/tooltip/item/{0}?dataEnv=16&locale=0' -f $id)
  if(-not $tip.name -or -not $tip.tooltip) {throw "Missing Forever item tooltip $id"}
  $tip|ConvertTo-Json -Depth 15|Set-Content $out
 }
} -ThrottleLimit 6
Write-Output "$($ids.Count) distinct Forever tooltips retained in research TEMP"
