param([switch]$RegenerateOwnCatalogs)
$ErrorActionPreference='Stop'
$researchDir=Join-Path $env:TEMP 'BISTracker-forever-physical-research'
$outputDir=Join-Path $PSScriptRoot '../../../BISTracker.Infrastructure/Catalog/Data/Forever/level30'
New-Item -ItemType Directory -Path $outputDir -Force | Out-Null
$slotNames=@{1='Head';2='Neck';3='Shoulder';5='Chest';6='Waist';7='Legs';8='Feet';9='Wrist';10='Hands';11='Finger1';12='Trinket1';13='MainHand';14='OffHand';15='Ranged';16='Back';17='MainHand';21='MainHand';22='OffHand';23='OffHand';25='Ranged';26='Ranged'}
$skillNames=@{164='Blacksmithing';165='Leatherworking';202='Engineering'}
$iconResults=@{}
foreach($file in Get-ChildItem $researchDir -Filter '*.json' | Where-Object BaseName -Match '^(hunter|rogue|warrior)-') {
 $guide=Get-Content -Raw $file.FullName | ConvertFrom-Json -AsHashtable
 $items=@();$rows=@($guide.rows)
 if($guide.characterClass -eq 'Rogue' -and $guide.specializationId -ne 'assassination') {
  $meta=$guide.entities['3']['277246']
  $rows+=@{id=277246;name=$meta.name_enus;metadata=$meta;slot=$meta.jsonequip.slotbak;requiredLevel=$meta.jsonequip.reqlevel;source='The Eye of Bhossca';section='Best Level 30 Quest Rewards';itemCell='[item=277246]'}
 }
 foreach($row in $rows) {
  $id=[int]$row.id; $tipPath=Join-Path $researchDir "tips/$id.json"
  $tip=Get-Content -Raw $tipPath | ConvertFrom-Json -AsHashtable
  if($id -eq 6687 -and -not $row.name -and $guide.specializationId -eq 'fury') {
    $arms=Get-Content -Raw (Join-Path $researchDir 'warrior-arms.json') | ConvertFrom-Json -AsHashtable
    $armsItem=$arms.rows | Where-Object id -eq 6687
    $row.name=$armsItem.name;$row.slot=$armsItem.slot;$row.requiredLevel=$armsItem.requiredLevel;$row.metadata=$armsItem.metadata
  }
  if($tip.name -cne $row.name) { throw "Name mismatch $id '$($tip.name)' '$($row.name)'" }
  if($row.requiredLevel -gt 30) { throw "Above beta cap $id" }
  $suffix=$null;$noteParts=@()
  $suffixMatch=[regex]::Match($row.itemCell,'\b(of the [A-Za-z]+)\b')
  if($suffixMatch.Success) { $suffix=$suffixMatch.Groups[1].Value; $noteParts+='Requires the '+$suffix+' variant.' }
  $type=switch -Regex ($row.section) {
   'Dungeon Drops' {'Dungeon';break}
   'Quest Rewards' {'Quest';break}
   'Crafted|crafted' {'Crafting';break}
   'World Drop' {'WorldDrop';break}
   'Reputation' {'Reputation';break}
   default {throw "Unknown section $($row.section)"}
  }
  $source=[regex]::Replace($row.source,'\[(quest|zone|skill)=(\d+)[^\]]*\]', {
   param($match)
   $key=$match.Groups[2].Value
   switch($match.Groups[1].Value) {
    'quest' {
     $entity=$guide.entities['5'][$key]
     if(-not $entity) {throw "Missing quest $key"}
     $faction=switch([int]$entity._side) {1 {'Alliance'} 2 {'Horde'} default {''}}
     if($faction) {return $entity.name_enus+" ($faction)"}
     return $entity.name_enus
    }
    'zone' {return $guide.entities['7'][$key].name_enus}
    'skill' {return $skillNames[[int]$key]}
   }
  })
  $source=$source -replace ' \(Alliance\) \(Alliance\)',' (Alliance)' -replace ' \(Horde\) \(Horde\)',' (Horde)'
  if($type -eq 'WorldDrop') { $source='World drop' }
  if($source -eq 'Multiple') { $source='Multiple dungeons (guide)'; $noteParts+='Guide does not identify each drop location.' }
  if([string]::IsNullOrWhiteSpace($source)) { throw "Missing source $id" }
  if($id -eq 6975) { $noteParts+='Class quest starts at level 30; requires help against higher-level enemies.' }
  if($id -eq 4381) { $noteParts+='Requires Engineering 140; 10 charges.' }
  if($type -eq 'Crafting' -and $tip.tooltip -match 'Binds when picked up') { $noteParts+='Bind on pickup.' }
  $req=[int]$row.requiredLevel
  if($req -gt 0) { $noteParts+='Requires level '+$req+'.' }
  $weaponKind=switch([int]$row.slot) {13 {'OneHanded'} 17 {'TwoHanded'} 21 {'MainHand'} 22 {'OffHand'} 14 {'OffHand'} 23 {'OffHand'} default {'None'}}
  $slots=@($slotNames[[int]$row.slot])
  if(-not $slots[0]) {throw "Unknown slot $($row.slot)"}
  if([int]$row.slot -eq 11) {$slots=@('Finger1','Finger2')}
  if([int]$row.slot -eq 12) {$slots=@('Trinket1','Trinket2')}
  if([int]$row.slot -eq 13 -and ($guide.characterClass -eq 'Rogue' -or $guide.characterClass -eq 'Hunter' -or ($guide.characterClass -eq 'Warrior' -and $guide.specializationId -eq 'fury'))) {
    $slots=@('MainHand','OffHand')
  }
  $iconUrl='https://wow.zamimg.com/images/wow/icons/large/'+$tip.icon+'.jpg'
  if(-not $iconResults.ContainsKey($iconUrl)) {
    $response=Invoke-WebRequest -Method Head $iconUrl
    if($response.StatusCode -ne 200 -or $response.Headers['Content-Type'] -notmatch 'image') { throw "Invalid icon $iconUrl" }
    $iconResults[$iconUrl]=200
  }
  $unique=[bool]($tip.tooltip -match 'Unique-Equipped|<br>Unique(?:<|-)')
  foreach($slot in $slots) {
   $note=@($noteParts)
   if($slots.Count -gt 1) { $note+='Alternative placement; choose one slot per owned copy.' }
   if($unique) { $note+='Unique-equipped.' }
   $suffixKey=if($suffix) {'-'+($suffix.ToLowerInvariant() -replace ' ','-')} else {''}
   $items+= [ordered]@{
    id=($guide.characterClass.ToLowerInvariant()+'-'+$guide.specializationId+'-'+$slot.ToLowerInvariant()+'-'+$id+$suffixKey)
    slot=$slot;itemId=$id;name=($row.name+$(if($suffix){' '+$suffix}));requiredSuffix=$suffix;requiredLevel=$req
    acquisitionType=$type;source=$source;note=($note -join ' ')
    iconUrl=$iconUrl;itemUrl=('https://www.wowhead.com/forever/item='+$id)
    recommendationUrl=$guide.sourceUrl;weaponKind=$weaponKind;uniqueEquipped=$unique
   }
  }
 }
 $catalog=[ordered]@{schemaVersion=1;catalogId=('forever-beta-level30-'+$guide.characterClass.ToLowerInvariant()+'-'+$guide.specializationId);gameVersion='Forever';characterClass=$guide.characterClass;specializationId=$guide.specializationId;levelCap=30;releaseStage='Beta';patch='1.60.1';phase='Level 30 beta';sourceUrl=$guide.sourceUrl;reviewedOn='2026-10-08';selectionMethod='Named equipment alternatives from the original level-30 guide; item identity and restrictions checked against Forever metadata. No independent ranking or complete slot set is implied.';status='Partial';items=$items}
 $destination=Join-Path $outputDir ($file.BaseName+'.json')
 if(Test-Path $destination) {
   $existing=Get-Content -Raw $destination | ConvertFrom-Json
   if(-not $RegenerateOwnCatalogs -or $existing.catalogId -ne $catalog.catalogId -or $existing.sourceUrl -ne $catalog.sourceUrl) {throw "Catalog already exists: $destination"}
 }
 $catalog | ConvertTo-Json -Depth 12 | Set-Content $destination
 Write-Output ($file.BaseName+': '+$items.Count+' placements / '+$rows.Count+' guide items')
}
Write-Output ('Verified icon URLs: '+$iconResults.Count)
