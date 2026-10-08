$ErrorActionPreference='Stop'
$researchDir=Join-Path $env:TEMP 'BISTracker-forever-physical-research/wowtbc'
$itemDir=Join-Path $researchDir 'items'
New-Item -ItemType Directory -Path $itemDir -Force | Out-Null
$ids=@(Get-ChildItem $researchDir -Filter '*.json'|ForEach-Object {(Get-Content -Raw $_.FullName|ConvertFrom-Json).candidates.itemId}|Sort-Object -Unique)
$ids | ForEach-Object -Parallel {
 $ErrorActionPreference='Stop';$id=$_;$dir=$using:itemDir
 $html=(Invoke-WebRequest ('https://www.wowhead.com/forever/item='+$id)).Content
 [IO.File]::WriteAllText((Join-Path $dir "$id.html"),$html)
 $entities=@{}
 foreach($m in [regex]::Matches($html,'WH.Gatherer.addData\(3,\s*16,\s*(\{[^\r\n]+\})\);')) {
  $part=ConvertFrom-Json $m.Groups[1].Value -AsHashtable
  foreach($key in $part.Keys) {$entities[$key]=$part[$key]}
 }
 $meta=$entities["$id"]
 if(-not $meta) {throw "No Forever metadata: $id"}
 $tip=Invoke-RestMethod ('https://nether.wowhead.com/tooltip/item/'+$id+'?dataEnv=16&locale=0')
 if($meta.name_enus -cne $tip.name) {throw "Name mismatch: $id"}
 $questRows=@()
 foreach($m in [regex]::Matches($html,"(?s)new Listview\(\{\s*template:\s*'quest',(?:(?!new Listview).)*?data:\s*(\[[^\r\n]+\]),")) {
  $questRows+=@(ConvertFrom-Json $m.Groups[1].Value -AsHashtable)
 }
 $rewards=@($questRows|Where-Object { @($_.itemchoices|ForEach-Object {$_[0]}) -contains $id -or @($_.itemrewards|ForEach-Object {$_[0]}) -contains $id })
 [ordered]@{itemId=$id;metadata=$meta;tooltip=$tip;rewardQuests=$rewards;metadataUrl=('https://www.wowhead.com/forever/item='+$id)}|ConvertTo-Json -Depth 30|Set-Content (Join-Path $dir "$id.json")
} -ThrottleLimit 6
Write-Output ($ids.Count.ToString()+' Forever item references reviewed')
