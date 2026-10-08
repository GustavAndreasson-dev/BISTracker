"""Retain minimum-level, faction, reward and published chain evidence for own quests."""
import concurrent.futures
import json
import os
import re
import subprocess
from pathlib import Path

ROOT=Path(__file__).resolve().parents[3]
TEMP=Path(os.environ['TEMP'])/'BISTracker-forever-physical-research'
TARGET=ROOT/'docs/data/forever-physical/upgrade-quest-metadata.json'
data=json.loads(TARGET.read_text(encoding='utf-8')) if TARGET.exists() else {}
shared=json.loads((ROOT/'docs/data/forever-casters/quest-metadata.json').read_text(encoding='utf-8'))
guide_quests={}
for path in TEMP.glob('*.json'):
    if re.match(r'^(hunter|rogue|warrior)-',path.stem):
        guide_quests.update(json.loads(path.read_text(encoding='utf-8-sig'))['entities'].get('5',{}))
ids=set()
for path in (ROOT/'BISTracker.Infrastructure/Catalog/Data/Forever/level30').glob('*.json'):
    if not re.match(r'^(hunter|rogue|warrior)-',path.stem):continue
    for row in json.loads(path.read_text(encoding='utf-8-sig'))['items']:
        if row['acquisitionType']=='Quest':
            ids.update(int(qid) for qid,q in guide_quests.items() if q['name_enus'] in row['source'])
            ids.update(int(qid) for qid in re.findall(r'forever/quest=(\d+)',row['note']))

def collect(qid):
    url=f'https://www.wowhead.com/forever/quest={qid}'
    path=TEMP/f'quest-{qid}.html'
    if not path.exists():
        raw=subprocess.check_output(['pwsh','-NoProfile','-Command',f"[Console]::OutputEncoding=[System.Text.Encoding]::UTF8; (Invoke-WebRequest '{url}' -ErrorAction Stop).Content"],encoding='utf-8')
        path.write_text(raw,encoding='utf-8')
    else:raw=path.read_text(encoding='utf-8-sig')
    marker=f'$.extend(g_quests[{qid}], '
    idx=raw.find(marker)
    if idx<0:raise ValueError(f'Missing quest metadata {qid}')
    q=json.JSONDecoder().raw_decode(raw[idx+len(marker):])[0]
    series=re.search(r'<table class="series">([\s\S]*?)</table>',raw)
    prior=series[1].split('<b>',1)[0] if series else ''
    return str(qid),{'questId':qid,'name':q['name'],'minimumLevel':q.get('reqlevel'),'factionCode':q.get('side'),
      'itemChoices':q.get('itemchoices',[]),'itemRewards':q.get('itemrewards',[]),
      'publishedPrerequisiteQuestIds':sorted({int(i) for i in re.findall(r'/forever/quest=(\d+)',prior)}),'questUrl':url}

while ids:
    for qid in list(ids):
        key=str(qid)
        if key in shared:data[key]=shared[key]
        if key in data:ids.remove(qid)
    if ids:
        with concurrent.futures.ThreadPoolExecutor(max_workers=3) as pool:
            for key,value in pool.map(collect,sorted(ids)):
                data[key]=value
                TARGET.write_text(json.dumps(data,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
    ids={int(qid) for q in data.values() for qid in q.get('publishedPrerequisiteQuestIds',[]) if str(qid) not in data}
TARGET.write_text(json.dumps(data,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
print(len(data),'individual quest and published chain facts retained')
print('Above cap:',[(q['questId'],q['name'],q['minimumLevel']) for q in data.values() if (q['minimumLevel'] or 0)>30])
