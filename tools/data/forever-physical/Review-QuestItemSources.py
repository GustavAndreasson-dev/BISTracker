"""Check individual Forever item reward-source links, including stale quest choice tables."""
import concurrent.futures
import json
import os
import re
import subprocess
from pathlib import Path

ROOT=Path(__file__).resolve().parents[3]
TEMP=Path(os.environ['TEMP'])/'BISTracker-forever-physical-research'
ids=set()
for path in (ROOT/'BISTracker.Infrastructure/Catalog/Data/Forever/level30').glob('*.json'):
    if re.match(r'^(hunter|rogue|warrior)-',path.stem):
        ids.update(r['itemId'] for r in json.loads(path.read_text(encoding='utf-8-sig'))['items'] if r['acquisitionType']=='Quest')

def collect(item_id):
    url=f'https://www.wowhead.com/forever/item={item_id}'
    path=TEMP/f'item-{item_id}.html'
    if not path.exists():
        raw=subprocess.check_output(['pwsh','-NoProfile','-Command',f"[Console]::OutputEncoding=[System.Text.Encoding]::UTF8; (Invoke-WebRequest '{url}' -ErrorAction Stop).Content"],encoding='utf-8')
        path.write_text(raw,encoding='utf-8')
    else:raw=path.read_text(encoding='utf-8-sig')
    match=re.search(r"id: 'reward-from-q'.*?data:\s*(\[[^\r\n]+\])",raw,re.S)
    sources=json.loads(match[1]) if match else []
    return str(item_id),{'itemId':item_id,'itemUrl':url,'observedRewardQuestIds':[q['id'] for q in sources],
      'questSources':[{'questId':q['id'],'name':q['name'],'minimumLevel':q.get('reqlevel'),'factionCode':q.get('side')} for q in sources],
      'reviewedOn':'2026-10-08'}

with concurrent.futures.ThreadPoolExecutor(max_workers=3) as pool:
    results=dict(pool.map(collect,sorted(ids)))
(ROOT/'docs/data/forever-physical/quest-item-source-metadata.json').write_text(json.dumps(results,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
print(len(results),'individual Forever item reward-source tables reviewed')
