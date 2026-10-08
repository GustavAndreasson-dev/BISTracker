$ErrorActionPreference='Stop'
$researchDir = Join-Path $env:TEMP 'BISTracker-forever-physical-research'
New-Item -ItemType Directory -Path $researchDir -Force | Out-Null
$targets=@(
 @('Hunter','beast-mastery','dps'),@('Hunter','marksmanship','dps'),@('Hunter','survival','dps'),
 @('Rogue','assassination','dps'),@('Rogue','combat','dps'),@('Rogue','subtlety','dps'),
 @('Warrior','arms','dps'),@('Warrior','fury','dps'),@('Warrior','protection','tank'))
foreach($target in $targets) {
 $url='https://www.wowhead.com/forever/guide/classes/{0}/{1}/level-30-{2}-overview' -f $target[0].ToLowerInvariant(),$target[1],$target[2]
 $html=(Invoke-WebRequest $url).Content
 [IO.File]::WriteAllText((Join-Path $researchDir ($target[0].ToLowerInvariant()+'-'+$target[1]+'.html')),$html)
 $contentMatch=[regex]::Matches($html,'WH\.markup\.printHtml\(("(?:[^"\\]|\\.)*")') | Where-Object { $_.Value.Contains('Best Level 30') } | Select-Object -First 1
 if(-not $contentMatch) { throw "Content missing $url" }
 $content=ConvertFrom-Json $contentMatch.Groups[1].Value
 $sets=@{}
 foreach($type in @(3,5,6,7,11)) {
  $match=[regex]::Match($html,"WH.Gatherer.addData\($type,\s*\d+,\s*(\{[^\r\n]+\})\);")
  if($match.Success) { $sets["$type"]=ConvertFrom-Json $match.Groups[1].Value -AsHashtable }
 }
 $sections=[regex]::Matches($content,'(?s)\[h2([^\]]*)\](.*?)\[/h2\](.*?)(?=\[h2|\z)') | Where-Object { $_.Groups[2].Value -like 'Best Level 30 *' }
 $rows=@()
 foreach($section in $sections) {
   foreach($row in [regex]::Matches($section.Groups[3].Value,'(?s)\[tr(?:[^\]]*)\](.*?)\[/tr\]')) {
    $cells=[regex]::Matches($row.Groups[1].Value,'(?s)\[td(?:[^\]]*)\](.*?)\[/td\]')
    if($cells.Count -lt 2) {continue}
    $source=$cells[0].Groups[1].Value
    foreach($im in [regex]::Matches($cells[1].Groups[1].Value,'\[?item=(\d+)[^\]]*\]')) {
      $id=$im.Groups[1].Value; $meta=$sets['3'][$id]
      $rows+=@{section=$section.Groups[2].Value;source=$source;id=[int]$id;name=$meta.name_enus;icon=$meta.icon;metadata=$meta;slot=$meta.jsonequip.slotbak;requiredLevel=$meta.jsonequip.reqlevel;itemCell=$cells[1].Groups[1].Value}
    }
   }
 }
 $object=@{characterClass=$target[0];specializationId=$target[1];sourceUrl=$url;rows=$rows;entities=$sets}
 $object | ConvertTo-Json -Depth 30 | Set-Content (Join-Path $researchDir ($target[0].ToLowerInvariant()+'-'+$target[1]+'.json'))
 Write-Output ($target[0]+' '+$target[1]+': '+$rows.Count+' rows')
 $rows | ForEach-Object { "{0} | {1} | slot={2} req={3} | {4}" -f $_.id,$_.name,$_.slot,$_.requiredLevel,$_.source }
}
$tipDir=Join-Path $researchDir 'tips'
New-Item -ItemType Directory -Path $tipDir -Force | Out-Null
$ids=@(Get-ChildItem $researchDir -Filter '*.json' | Where-Object BaseName -Match '^(hunter|rogue|warrior)-' | ForEach-Object { (Get-Content -Raw $_.FullName | ConvertFrom-Json).rows.id })
foreach($id in ($ids + @(6687,277246) | Sort-Object -Unique)) {
 $tip=Invoke-RestMethod ('https://nether.wowhead.com/tooltip/item/{0}?dataEnv=16&locale=0' -f $id)
 $tip | ConvertTo-Json -Depth 10 | Set-Content (Join-Path $tipDir "$id.json")
}
Write-Output $researchDir
