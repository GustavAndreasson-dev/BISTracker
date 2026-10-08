import json, re, sys, html
from pathlib import Path

if len(sys.argv) != 3 or sys.argv[2] != '--update-working-catalogs':
    raise SystemExit('Working-catalog revision requires a source directory and --update-working-catalogs; preserve and review existing files first.')

root=Path(__file__).resolve().parents[3]
evidence=root/'docs/data/forever-hybrids'
sources=Path(sys.argv[1])
out=root/'BISTracker.Infrastructure/Catalog/Data/Forever/level30'
metadata={}
for file in sources.glob('*.metadata.json'):
    metadata.update(json.loads(file.read_text(encoding='utf-8')).get('3',{}))
slot_map={1:'Head',2:'Neck',3:'Shoulder',5:'Chest',6:'Waist',7:'Legs',8:'Feet',9:'Wrist',10:'Hands',11:'Finger1',12:'Trinket1',13:'MainHand',14:'OffHand',15:'Ranged',16:'Back',17:'MainHand',20:'Chest',21:'MainHand',22:'OffHand',23:'OffHand',25:'Ranged',26:'Ranged',28:'Ranged'}

extra=json.loads((evidence/'enhancement-icy-alternatives.json').read_text(encoding='utf-8'))
audit=[]
for file in sorted(out.glob('*.json')):
    if not file.stem.startswith(('druid-','paladin-','shaman-')): continue
    catalog=json.loads(file.read_text(encoding='utf-8'))
    catalog['patch']='1.60.1'
    if file.stem=='paladin-retribution':
        # Verified separately in Icy Veins Forever Retribution level-30 table,
        # Finger lines 325-334: Reedknot Ring, Dustwallow Marsh quest.
        # Wowhead Forever quest 1203 / tooltip 9622 verifies reward identity.
        source_catalog=json.loads((out/'druid-restoration.json').read_text(encoding='utf-8'))
        for template in source_catalog['items']:
            if template['itemId']!=9622: continue
            entry=dict(template)
            entry['id']=f'forever-beta-level30-paladin-retribution-{entry["slot"].lower()}-9622'
            entry['recommendationUrl']='https://www.icy-veins.com/wow-forever/retribution-paladin-melee-dps-pve-guide/'
            if not any(i['id']==entry['id'] for i in catalog['items']): catalog['items'].append(entry)
    if file.stem=='shaman-enhancement':
        for candidate in extra['items']:
            iid=candidate['itemId']; meta=metadata[str(iid)]
            invslot=meta['jsonequip']['slotbak']; slot=slot_map[invslot]
            for placement in [slot,'Finger2' if slot=='Finger1' else 'Trinket2' if slot=='Trinket1' else None]:
                if not placement: continue
                rid=f'forever-beta-level30-shaman-enhancement-{placement.lower()}-{iid}'
                if any(i['id']==rid for i in catalog['items']): continue
                catalog['items'].append({'id':rid,'slot':placement,'itemId':iid,'name':meta['name_enus'],'requiredSuffix':None,'acquisitionType':candidate['acquisitionType'],'source':candidate['source'],'note':candidate.get('note','')+(' Alternative slot placement; not a recommendation to obtain two copies.' if slot in ['Finger1','Trinket1'] else ''),'iconUrl':None,'itemUrl':f'https://www.wowhead.com/forever/item={iid}','recommendationUrl':extra['recommendationUrl'],'weaponKind':'TwoHanded' if invslot==17 else 'OneHanded' if invslot==13 else 'MainHand' if invslot==21 else 'OffHand' if invslot in [14,22,23] else 'None','uniqueEquipped':False,'requiredLevel':meta['jsonequip'].get('reqlevel',0)})
    reviewed=[]
    for item in catalog['items']:
        iid=item['itemId']; proof=json.loads((sources/f'tooltip-{iid}.json').read_text(encoding='utf-8-sig'))
        tooltip=html.unescape(re.sub('<[^>]+>',' ',proof['metadata']['tooltip']))
        tooltip=' '.join(tooltip.split())
        if proof['metadata']['name']!=item['name']: raise ValueError(f'Name mismatch: {iid}')
        if proof['iconHttpStatus']!=200: raise ValueError(f'Icon unavailable: {iid}')
        required=re.search(r'Requires Level (\d+)',tooltip)
        level=int(required[1]) if required else metadata.get(str(iid),{}).get('jsonequip',{}).get('reqlevel',0)
        if level>30:
            audit.append({'catalog':catalog['catalogId'],'itemId':iid,'reason':f'Tooltip requires level {level}'});continue
        item['requiredLevel']=level
        item['iconUrl']=proof['iconUrl']
        item['uniqueEquipped']=bool(re.search(r'\bUnique(?:-Equipped)?\b',tooltip))
        if item['uniqueEquipped'] and 'Unique' not in item['note']: item['note']= (item['note']+' Unique; one copy may be equipped.').strip()
        if item['acquisitionType']=='Crafting' and 'Binds when picked up' in tooltip:
            item['note']=(item['note']+' Crafted item binds on pickup.').strip()
        profession=re.search(r'Requires (Leatherworking|Blacksmithing|Tailoring|Engineering|Enchanting)\s*\((\d+)\)',tooltip)
        if profession: item['note']=(item['note']+f' Requires {profession[1]} {profession[2]}.').strip()
        if item['itemId'] in [280604,280605,280607,280696]: item['note']=(item['note']+' Level 30 Shaman quest chain; group needed for elite enemies.').strip()
        item['note']=' '.join(dict.fromkeys(re.split(r'(?<=\.)\s+',item['note'])))
        reviewed.append(item)
    catalog['items']=reviewed
    assert len({i['id'] for i in reviewed})==len(reviewed)
    file.write_text(json.dumps(catalog,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
    sources_present=sorted({i['acquisitionType'] for i in reviewed})
    missing=sorted(set(slot_map.values())-{i['slot'] for i in reviewed})
    print(file.name,len(reviewed),'items;',len({i['itemId'] for i in reviewed}),'identities; missing',','.join(missing),'; sources',','.join(sources_present))
audit+=json.loads((sources/'rejected-records.json').read_text(encoding='utf-8'))
(evidence/'excluded-records.json').write_text(json.dumps(audit,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
proof=[]
for file in sources.glob('tooltip-*.json'):
    record=json.loads(file.read_text(encoding='utf-8-sig'))
    proof.append({'itemId':record['itemId'],'name':record['metadata']['name'],'metadataUrl':f'https://nether.wowhead.com/forever/tooltip/item/{record["itemId"]}?dataEnv=16&locale=0','iconUrl':record['iconUrl'],'iconHttpStatus':record['iconHttpStatus'],'reviewedOn':record['reviewedOn']})
(evidence/'item-verification.json').write_text(json.dumps(sorted(proof,key=lambda r:r['itemId']),ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
quests=[json.loads(file.read_text(encoding='utf-8-sig')) for file in sources.glob('quest-proof-*.json')]
assert all(q['requiredLevel'] is not None and q['requiredLevel']<=30 for q in quests)
(evidence/'quest-verification.json').write_text(json.dumps(sorted(quests,key=lambda q:int(q['questId'])),ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
