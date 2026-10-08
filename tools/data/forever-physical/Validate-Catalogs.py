"""Check saved source facts, faction witnesses and cross-catalog physical metadata."""
import json
import re
from collections import defaultdict
from pathlib import Path

ROOT=Path(__file__).resolve().parents[3]
DIR=ROOT/'BISTracker.Infrastructure/Catalog/Data/Forever/level30'
FACTS=ROOT/'docs/data/forever-physical'
read=lambda p:json.loads(p.read_text(encoding='utf-8-sig'))
audit=read(FACTS/'slot-audit.json')
own={row['catalogId']:row for row in audit['perSpec']}
metadata=read(FACTS/'upgrade-item-metadata.json')
variants=defaultdict(list)
verified=[]
for path in sorted(DIR.glob('*.json')):
    c=read(path)
    for r in c['items']:
        variants[(r['itemId'],r.get('requiredSuffix'))].append((path.name,r))
    if c['catalogId'] not in own:continue
    assert c['levelCap']==30 and c['releaseStage']=='Beta' and c['patch']=='1.60.1' and c['status']=='Reviewed'
    rows={r['id']:r for r in c['items']}
    assert len(rows)==len(c['items'])
    for r in rows.values():
        assert 0<=r['requiredLevel']<=30
        assert r['itemUrl'].startswith('https://www.wowhead.com/forever/')
        assert r['recommendationUrl'].startswith('https://')
        if r['acquisitionType']=='Quest':assert r['availableFactions'] and all(f in ('Alliance','Horde') for f in r['availableFactions'])
        assert 'Named unranked upgrade' not in r['note']
        m=metadata[str(r['itemId'])]
        assert r['requiredLevel']==m['requiredLevel'] and r['uniqueEquipped']==m['uniqueEquipped']
        assert r['name']==m['name'] if not r.get('requiredSuffix') else r['name']==m['name']+' '+r['requiredSuffix']
    for f in own[c['catalogId']]['factions']:
        assert f['fullSetValidated'] and not f['missingSlots']
        witness=f['legalSetWitness']
        assert len({w['itemId'] for w in witness})==len(witness)
        assert len(f['requiredProfessions'])<=2
        choices={}
        for w in witness:
            r=rows[w['recommendationId']]
            assert r['itemId']==w['itemId'] and r['slot']==w['slot']
            if r['acquisitionType']=='Quest':
                assert f['faction'] in r['availableFactions']
                q=w['questProof']
                assert q['minimumLevel']<=30
                if not any(pair[0]==r['itemId'] for pair in q['itemRewards']):
                    assert q['questId'] not in choices or choices[q['questId']]==r['itemId']
                    choices[q['questId']]=r['itemId']
        main=next(w for w in witness if w['slot']=='MainHand')
        assert (main['weaponKind']=='TwoHanded')==('OffHand' in f['exemptSlots'])
    verified.append({'catalogId':c['catalogId'],'rawPlacementRows':len(rows),'factionWitnesses':2,'validated':True})
conflicts=[]
for (item_id,suffix), entries in variants.items():
    for key in ('name','requiredLevel','weaponKind','uniqueEquipped'):
        values=defaultdict(list)
        for path,r in entries:values[json.dumps(r.get(key),ensure_ascii=False)].append(path)
        if len(values)>1:
            conflicts.append({'itemId':item_id,'requiredSuffix':suffix,'field':key,'values':dict(values)})
report={'reviewedOn':'2026-10-08','ownCatalogsValidated':verified,'physicalMetadataConflictsAcrossAvailableCatalogs':conflicts,
  'validationScope':'Saved JSON and individual source facts, both faction feasibility witnesses, duplicate IDs, distinct item capacity, quest alternative rewards, profession count and hands. Root performs the .NET/WinUI release checks.'}
(FACTS/'validation-report.json').write_text(json.dumps(report,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
print(len(verified),'own catalogs passed; cross-catalog metadata conflicts:',len(conflicts))
for conflict in conflicts:print(conflict)
