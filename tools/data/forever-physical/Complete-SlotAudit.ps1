$ErrorActionPreference = 'Stop'
$researchDir = Join-Path $env:TEMP 'BISTracker-forever-physical-research'
$catalogDir = Join-Path $PSScriptRoot '../../../BISTracker.Infrastructure/Catalog/Data/Forever/level30'
$factsDir = Join-Path $PSScriptRoot '../../../docs/data/forever-physical'
New-Item -ItemType Directory -Path $factsDir -Force | Out-Null
$slots = @('Head','Neck','Shoulder','Back','Chest','Wrist','Hands','Waist','Legs','Feet','Finger1','Finger2','Trinket1','Trinket2','MainHand','OffHand','Ranged')
$slotMap = @{1='Head';5='Chest';7='Legs';10='Hands';17='MainHand'}
$questIds = @{7133=1848;7130=1845;7129=1843;7132=1847;6972=1782;6971=1706;6974=1709;6973=1711}
$itemFacts = @()
$perSpec = @()

function Add-VerifiedItem($catalog, [int]$itemId, [string]$guideUrl, [string]$type, [string]$source, [string]$note) {
    $metadataPath = Join-Path $researchDir "$itemId.meta.json"
    $meta = Get-Content -Raw $metadataPath | ConvertFrom-Json -AsHashtable
    $tip = Get-Content -Raw (Join-Path $researchDir "tips/$itemId.json") | ConvertFrom-Json -AsHashtable
    if ($meta.name_enus -cne $tip.name) { throw "Item name mismatch: $itemId" }
    $slot = $slotMap[[int]$meta.jsonequip.slotbak]
    if (-not $slot) { throw "Unreviewed slot: $itemId" }
    $level = [int]$meta.jsonequip.reqlevel
    if ($level -gt 30) { throw "Above beta cap: $itemId" }
    $iconUrl = 'https://wow.zamimg.com/images/wow/icons/large/'+$tip.icon+'.jpg'
    $response = Invoke-WebRequest -Method Head $iconUrl
    if ($response.StatusCode -ne 200 -or $response.Headers['Content-Type'] -notmatch 'image') { throw "Invalid icon: $itemId" }
    $unique = [bool]($tip.tooltip -match 'Unique-Equipped|<br>Unique(?:<|-)')
    $row = [ordered]@{
        id=($catalog.characterClass.ToLowerInvariant()+'-'+$catalog.specializationId+'-'+$slot.ToLowerInvariant()+'-'+$itemId)
        slot=$slot;itemId=$itemId;name=$tip.name;requiredSuffix=$null;requiredLevel=$level
        acquisitionType=$type;source=$source;note=$note;iconUrl=$iconUrl
        itemUrl=('https://www.wowhead.com/forever/item='+$itemId);recommendationUrl=$guideUrl
        weaponKind=$(if ($slot -eq 'MainHand') {'TwoHanded'} else {'None'});uniqueEquipped=$unique
    }
    if (-not ($catalog.items | Where-Object { $_.id -eq $row.id })) { $catalog.items += $row }
    return [ordered]@{itemId=$itemId;name=$tip.name;slot=$slot;requiredLevel=$level;itemUrl=$row.itemUrl;recommendationUrl=$guideUrl;acquisitionType=$type;source=$source;availabilityEvidence=$note}
}

foreach ($file in Get-ChildItem $catalogDir -Filter '*.json' | Where-Object BaseName -Match '^(hunter|rogue|warrior)-') {
    $c = Get-Content -Raw $file.FullName | ConvertFrom-Json -AsHashtable
    $icyUrl = 'https://www.icy-veins.com/wow-forever/'+$c.specializationId+'-'+$c.characterClass.ToLowerInvariant()+$(if($c.characterClass -eq 'Hunter' -and $c.specializationId -ne 'survival') {'-ranged-dps-pve-guide'} elseif ($c.specializationId -eq 'protection') {'-tank-pve-guide'} else {'-melee-dps-pve-guide'})
    $c.weaponSetup = 'Flexible'
    $c.Remove('weaponSetupSourceUrl') | Out-Null
    $exemptSlots = @()
    $exemptionEvidence = @()
    $weaponNotes = @()
    if ($c.characterClass -eq 'Hunter') {
        $itemFacts += Add-VerifiedItem $c 6679 $icyUrl 'Dungeon' 'Razorfen Kraul' 'Two-handed melee stat weapon. Requires level 24; occupies both melee hands.'
        $weaponNotes += 'The guides permit two-handed and dual-wield melee alternatives. OffHand is conditional on the chosen MainHand, not a permanent missing or exempt slot.'
    }
    elseif ($c.characterClass -eq 'Rogue') {
        $c.weaponSetup = 'OneHandAndOffHand'
        $c.weaponSetupSourceUrl = $c.sourceUrl
        $weaponNotes += 'Two one-handed weapons are required by the recommended playstyle. Slot alternatives are not claimed to have equivalent damage.'
        if ($c.specializationId -eq 'assassination') {
            $c.items = @($c.items | Where-Object itemId -NE 277246)
            $weaponNotes += 'Mutilate recommendations use a dagger in each hand. Ruby-Adorned Blade is excluded because the original Assassination guide explicitly says this spec does not benefit much from it; the guide recommends it as best for Combat and Subtlety only.'
        }
    }
    elseif ($c.specializationId -eq 'arms') {
        $c.weaponSetup = 'TwoHanded'
        $c.weaponSetupSourceUrl = $c.sourceUrl
        $exemptSlots = @('OffHand')
        $exemptionEvidence += [ordered]@{slot='OffHand';reason='Recommended Arms build and named weapons use a two-handed MainHand, occupying both melee hands.';sourceUrl=$c.sourceUrl;conditional=$false}
    }
    elseif ($c.specializationId -eq 'fury') {
        $c.weaponSetup = 'OneHandAndOffHand'
        $c.weaponSetupSourceUrl = $c.sourceUrl
        $weaponNotes += 'The level-30 guide recommends dual-wield talents but its named gear table contains only two-handed weapons. Those rows remain historical alternatives and cannot complete the requested dual-wield target.'
    }
    elseif ($c.specializationId -eq 'protection') {
        $c.weaponSetup = 'OneHandAndOffHand'
        $c.weaponSetupSourceUrl = $c.sourceUrl
        $g = Get-Content -Raw (Join-Path $researchDir 'warrior-protection.json') | ConvertFrom-Json -AsHashtable
        foreach ($id in 7133,7130,7129,7132,6972,6971,6974,6973) {
            $questId = $questIds[$id]
            $q = $g.entities['5']["$questId"]
            $faction = $(if ([int]$q._side -eq 1) {'Alliance'} elseif ([int]$q._side -eq 2) {'Horde'} else {throw "Unreviewed faction: $questId"})
            $source = $q.name_enus+' ('+$faction+'); Warrior class quest chain'
            $note = 'Chain starts at level 20 and requires Razorfen Kraul plus travel/combat in multiple zones. '+$faction+' only; Warrior only. Final quest: https://www.wowhead.com/forever/quest='+$questId+'.'
            $itemFacts += Add-VerifiedItem $c $id $c.sourceUrl 'Quest' $source $note
        }
        $weaponNotes += 'Protection explicitly uses a one-handed weapon with a shield; the two-handed class-quest weapon discussed as optional DPS is not imported as a tank target.'
    }
    $c.selectionMethod = 'Named alternatives from original Forever level-30 guides, including explicitly recommended class quests and separately linked supplemental guides. Dungeon/quest policy applied by the app. No independent ranking, equivalence, or complete slot set is implied.'
    $c.status = 'Partial'
    $c | ConvertTo-Json -Depth 30 | Set-Content $file.FullName
    $allowed = @($c.items | Where-Object acquisitionType -In 'Dungeon','Quest')
    $eligible = @($allowed | Where-Object { $c.weaponSetup -ne 'OneHandAndOffHand' -or $_.weaponKind -ne 'TwoHanded' })
    $covered = @($eligible.slot | Sort-Object -Unique)
    $pairGaps = @()
    foreach ($pair in @(@('Finger1','Finger2'),@('Trinket1','Trinket2'))) {
        $variants = @($eligible | Where-Object slot -In $pair | ForEach-Object { [string]$_.itemId+'|'+$_.requiredSuffix } | Sort-Object -Unique)
        if ($variants.Count -eq 1) {
            $covered = @($covered | Where-Object { $_ -ne $pair[1] })
            $pairGaps += [ordered]@{slot=$pair[1];physicalVariantKeys=$variants;reason='Only one distinct physical recommendation was verified for this pair. Alternative placement does not establish a second target or two obtainable copies.'}
        }
    }
    $missing = @($slots | Where-Object { $_ -notin $covered -and $_ -notin $exemptSlots })
    $gaps = @()
    foreach ($slot in $missing) {
        $excluded = @($c.items | Where-Object { $_.slot -eq $slot -and $_.acquisitionType -notin 'Dungeon','Quest' } | ForEach-Object { [ordered]@{itemId=$_.itemId;name=$_.name;acquisitionType=$_.acquisitionType;recommendationUrl=$_.recommendationUrl} })
        $reason = $(if ($excluded.Count) {'Named guide alternatives for this slot are excluded by the dungeon/quest policy; no named allowed fallback was verified.'} else {'No named Forever level-30 dungeon/quest recommendation for this slot was verified in the original guides reviewed.'})
        if ($c.specializationId -eq 'fury' -and $slot -in 'MainHand','OffHand') { $reason = 'The recommended level-30 dual-wield setup has no explicitly named one-handed alternative in the original Fury gear table.' }
        $pairGap = $pairGaps | Where-Object slot -EQ $slot
        if ($pairGap) { $reason = $pairGap.reason }
        $gaps += [ordered]@{slot=$slot;blocking=$true;reason=$reason;excludedGuideAlternatives=$excluded;sourceUrls=@($c.sourceUrl,$icyUrl)}
    }
    $bySlot = @()
    foreach ($slot in $slots | Where-Object { $_ -in $covered }) {
        $bySlot += [ordered]@{slot=$slot;alternatives=@($eligible | Where-Object slot -EQ $slot | ForEach-Object {[ordered]@{recommendationId=$_.id;itemId=$_.itemId;requiredSuffix=$_.requiredSuffix;recommendationUrl=$_.recommendationUrl}});equivalenceEstablished=$false}
    }
    $perSpec += [ordered]@{catalogId=$c.catalogId;characterClass=$c.characterClass;specializationId=$c.specializationId;weaponSetup=$c.weaponSetup;weaponSetupSourceUrl=$c.weaponSetupSourceUrl;expectedSlots=@($slots|Where-Object {$_ -notin $exemptSlots});coveredSlots=$covered;missingSlots=$missing;exemptSlots=$exemptSlots;exemptionEvidence=$exemptionEvidence;gaps=$gaps;singlePhysicalVariantPairs=$pairGaps;alternativesBySlot=$bySlot;equivalentGroups=@();weaponNotes=$weaponNotes;setCompleteness='Blocked';setCompletenessReason='Missing relevant target slots remain. Alternative rows do not establish a complete simultaneously obtainable set or ownership of two copies.';reviewedSources=@($c.sourceUrl,$icyUrl)}
    Write-Output ($file.BaseName+': '+$covered.Count+' covered; missing '+($missing -join ', '))
}
$audit = [ordered]@{schemaVersion=1;reviewedOn='2026-10-08';gameVersion='Forever';levelCap=30;releaseStage='Beta';patch='1.60.1';acquisitionPolicy=@('Dungeon','Quest');countingMethod='One target per relevant equipment slot; recommendations are alternatives inside each target. Duplicate placement rows do not add slots.';quantityLimitation='Owned copy count is not modelled; non-unique duplicate placements do not prove two owned copies.';perSpec=$perSpec}
$audit | ConvertTo-Json -Depth 35 | Set-Content (Join-Path $factsDir 'slot-audit.json')
$itemFacts | Sort-Object itemId,recommendationUrl -Unique | ConvertTo-Json -Depth 10 | Set-Content (Join-Path $factsDir 'supplemental-items.json')
