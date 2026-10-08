$ErrorActionPreference='Stop'
$researchDir=Join-Path $env:TEMP 'BISTracker-forever-physical-research/wowtbc'
New-Item -ItemType Directory -Path $researchDir -Force | Out-Null
$audit=Get-Content -Raw (Join-Path $PSScriptRoot '../../../docs/data/forever-physical/slot-audit.json') | ConvertFrom-Json -AsHashtable
$slotKeys=@{Head='head';Neck='neck';Shoulder='shoulder';Back='back';Chest='chest';Wrist='wrist';Hands='hands';Waist='waist';Legs='legs';Feet='feet';Finger1='finger';Finger2='finger';Trinket1='trinket';Trinket2='trinket';MainHand='main-hand';OffHand='off-hand';Ranged='ranged'}
foreach($spec in $audit.perSpec) {
 $slug=$spec.specializationId+'-'+$spec.characterClass.ToLowerInvariant()
 $url='https://wowtbc.gg/warcraftforever/bis-list/'+$slug+'/'
 $html=(Invoke-WebRequest $url).Content
 if($html -notmatch 'Items are not in order') {throw "Missing unranked disclaimer: $url"}
 $path='/page-data/warcraftforever/bis-list/'+$slug+'/page-data.json'
 if(-not $html.Contains($path)) {throw "Unobserved page data: $url"}
 $jsonUrl='https://wowtbc.gg'+$path
 $context=(Invoke-RestMethod $jsonUrl).result.pageContext
 $gear=@{};foreach($row in $context.gearData) {$gear["$($row.id)"]=$row}
 $candidates=@()
 foreach($key in ($slotKeys.Values|Sort-Object -Unique)) {
  foreach($id in $context.bisList.$key) {
   $row=$gear["$id"]
   if(-not $row) {throw "Missing item identity: $id"}
   if($row.source_type -like 'Quest*' -or $row.source_type -in 'Crafted','Dungeons','Scarlet Monastery','Gnomeregan','Razorfen Kraul','The Stockade','Shadowfang Keep','Blackfathom Deeps','Deadmines','The Deadmines','Ruins of Lordaeron','Hall of Thanes','Excavation Site: Wetlands','Razorfen Downs') {
    $candidates+=@{guideSlot=$key;itemId=[int]$id;name=$row.name;source=$row.source;sourceType=$row.source_type;declaredSlot=$row.slot;declaredMinimumLevel=$row.other_stats.min_level;declaredWeaponType=$row.type;recommendationUrl=$url;pageDataUrl=$jsonUrl}
   }
  }
 }
 [ordered]@{catalogId=$spec.catalogId;sourceUrl=$url;pageDataUrl=$jsonUrl;reviewedOn='2026-10-08';selectionMethod='Named unranked upgrades for this exact Forever spec; dungeon/quest candidates retained for separate verification, including faction-specific target gaps.';candidates=$candidates}|ConvertTo-Json -Depth 15|Set-Content (Join-Path $researchDir ($slug+'.json'))
 Write-Output "$slug : $($candidates.Count) unranked dungeon/quest candidates"
}
