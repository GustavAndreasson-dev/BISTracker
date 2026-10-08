"""Explicitly reviewed source import; downloaded guide HTML stays in the temp directory.

Only equipment from the original Forever level-30 gearing tables is retained.
This script is research tooling, not a runtime or an automatic guide scraper.
"""
import json, re, sys, html as html_module
from pathlib import Path

root = Path(__file__).resolve().parents[3]
sources = Path(sys.argv[1])
out = root / 'BISTracker.Infrastructure/Catalog/Data/Forever/level30'
out.mkdir(parents=True, exist_ok=True)
decoder = json.JSONDecoder()
slot_map = {1:'Head',2:'Neck',3:'Shoulder',5:'Chest',6:'Waist',7:'Legs',8:'Feet',9:'Wrist',10:'Hands',11:'Finger1',12:'Trinket1',13:'MainHand',14:'OffHand',15:'Ranged',16:'Back',17:'MainHand',20:'Chest',21:'MainHand',22:'OffHand',23:'OffHand',25:'Ranged',26:'Ranged',28:'Ranged'}
all_data = {}
for path in sources.glob('*.metadata.json'):
    for kind, records in json.loads(path.read_text(encoding='utf-8')).items():
        all_data.setdefault(int(kind), {}).update(records)

def clean(text):
    def replace(m):
        kind = {'item':3,'quest':5,'zone':7,'faction':8,'skill':15,'spell':6}.get(m[1])
        return all_data.get(kind,{}).get(m[2],{}).get('name_enus','')
    text = re.sub(r'\[(item|quest|zone|faction|skill|spell)=(\d+)[^\]]*\]',replace,text)
    return ' '.join(re.sub(r'\[[^\]]*\]',' ',text).split())

def side(ids):
    sides = {all_data.get(5,{}).get(str(i),{}).get('_side',0) for i in ids}
    return 'Alliance only.' if sides == {1} else 'Horde only.' if sides == {2} else ''

catalogs = {}
audit = []
for path in sources.glob('*.markup.txt'):
    if not path.stem.startswith(('druid-','paladin-','shaman-')): continue
    class_name, spec, role = path.name.split('.')[0].split('-')
    url = f'https://www.wowhead.com/forever/guide/classes/{class_name}/{spec}/level-30-{role}-overview'
    key = (class_name,spec)
    catalog = catalogs.setdefault(key, {'schemaVersion':1,'catalogId':f'forever-beta-level30-{class_name}-{spec}','gameVersion':'Forever','characterClass':class_name.title(),'specializationId':spec,'levelCap':30,'releaseStage':'Beta','phase':'Level 30 beta','sourceUrl':url,'reviewedOn':'2026-10-08','selectionMethod':'Equipment alternatives from the original Forever level-30 guide; source categories retained, no new stat weights or invented slot replacements.','status':'Partial','items':[]})
    markup = path.read_text(encoding='utf-8')
    if class_name=='paladin' and spec=='holy':
        for spell,item in {1252250:250501,1252275:250526,1252270:250521,1252308:250559}.items():
            markup=markup.replace(f'[spell={spell}]',f'[item={item}]')
    for section in re.split(r'(?=\[h2 )',markup):
        title = clean(section.split('\n')[0])
        if not title.startswith('Best Level 30 ') or 'Talent' in title: continue
        acquisition = 'Dungeon' if 'Dungeon' in title else 'Quest' if 'Quest' in title else 'Crafting' if 'Crafted' in title else 'Reputation' if 'Reputation' in title else None
        if not acquisition: continue
        for table in re.findall(r'\[table[^\]]*\](.*?)\[/table\]',section,re.S):
            for row in re.findall(r'\[tr[^\]]*\](.*?)\[/tr\]',table,re.S):
                cells = re.findall(r'\[td[^\]]*\](.*?)\[/td\]',row,re.S)
                item_cells = [i for i,c in enumerate(cells) if re.search(r'\[item=',c)]
                if not item_cells: continue
                item_cell = item_cells[0]
                matches = list(re.finditer(r'\[item=(\d+)[^\]]*\]',cells[item_cell]))
                source_cell = cells[2] if len(cells)==3 and item_cell==1 else cells[0]
                quests = re.findall(r'\[quest=(\d+)',source_cell)
                for n,m in enumerate(matches):
                    iid = int(m[1])
                    meta = all_data[3].get(str(iid))
                    if not meta: audit.append({'guide':url,'itemId':iid,'reason':'Missing item metadata'}); continue
                    invslot = meta.get('jsonequip',{}).get('slotbak')
                    if invslot not in slot_map: continue
                    if meta.get('jsonequip',{}).get('reqlevel',0)>30:
                        audit.append({'guide':url,'itemId':iid,'reason':'Verified required level exceeds beta cap 30'});continue
                    if class_name=='paladin' and spec=='retribution' and acquisition=='Dungeon' and iid==252455:
                        audit.append({'guide':url,'itemId':iid,'reason':'Crafted Defender helm mapped to several unrelated dungeon bosses; source mismatch rejected'});continue
                    # Never derive a precise quest-to-reward relation from an ambiguous multi-reward table row.
                    if acquisition=='Quest' and len(matches)>1 and len(quests)!=len(matches):
                        overrides={9623:[2929,2841],9624:[2929,2841],9625:[2929,2841],4197:[1101,1102],279841:[92458],279844:[96984],279835:[92456],279837:[92457],271766:[98824],271716:[98824],271719:[98823]}
                        if iid not in overrides:
                            audit.append({'guide':url,'itemId':iid,'reason':'Ambiguous multi-quest row; precise reward relation requires separate review'});continue
                        quest_ids=overrides[iid]
                    else: quest_ids=[int(quests[n])] if len(quests)==len(matches) and quests else [int(x) for x in quests]
                    source = clean(source_cell)
                    tail = clean(cells[item_cell][m.end():matches[n+1].start() if n+1<len(matches) else len(cells[item_cell])]).strip(' -/')
                    if acquisition=='Quest': source = ' / '.join(all_data[5][str(i)]['name_enus'] for i in quest_ids) if quest_ids else source
                    if acquisition=='Dungeon' and tail: source += ' · '+tail
                    if acquisition=='Crafting':
                        if not source or source in slot_map.values() or source in ['Two-Hand','Back','Finger','Trinket','Shoulder']:
                            source='Blacksmithing' if iid==7956 else 'Leatherworking'
                    note_parts=[side(quest_ids)] if acquisition=='Quest' else []
                    if class_name=='druid' and spec=='feral': note_parts.append('Feral tank alternative.' if role=='tank' else 'Feral damage alternative.')
                    if 'Spellbear' in tail: note_parts.append('Spellbear alternative in the Balance guide.')
                    if 'Arcane spellpower only' in tail: note_parts.append('Arcane damage build alternative.')
                    slot=slot_map[invslot]
                    weapon = 'TwoHanded' if invslot==17 else 'OneHanded' if invslot==13 else 'MainHand' if invslot==21 else 'OffHand' if invslot in [14,22,23] else 'None'
                    for placement in [slot, 'Finger2' if slot=='Finger1' else 'Trinket2' if slot=='Trinket1' else None]:
                        if not placement:continue
                        entry={'id':f'forever-beta-level30-{class_name}-{spec}-{placement.lower()}-{iid}','slot':placement,'itemId':iid,'name':meta['name_enus'],'requiredSuffix':None,'acquisitionType':acquisition,'source':source,'note':' '.join(p for p in note_parts if p),'iconUrl':None,'itemUrl':f'https://www.wowhead.com/forever/item={iid}','recommendationUrl':url,'weaponKind':weapon,'uniqueEquipped':False,'requiredLevel':meta.get('jsonequip',{}).get('reqlevel',0)}
                        if placement in ['Finger1','Finger2','Trinket1','Trinket2']: entry['note']+=' Alternative slot placement; not a recommendation to obtain two copies.'
                        existing=next((x for x in catalog['items'] if x['id']==entry['id']),None)
                        if existing:
                            if class_name=='druid' and spec=='feral' and role=='tank': existing['note']='Feral tank and damage alternative.'+(' Alternative slot placement; not a recommendation to obtain two copies.' if placement.startswith(('Finger','Trinket')) else '')
                        else: catalog['items'].append(entry)
for key,catalog in catalogs.items():
    file=out/f'{key[0]}-{key[1]}.json'
    if file.exists() and '--replace-working-import' not in sys.argv: raise RuntimeError(f'Refusing to overwrite existing catalog: {file}')
    file.write_text(json.dumps(catalog,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
    print(file.name, len(catalog['items']), sorted({x['slot'] for x in catalog['items']}))
(sources/'rejected-records.json').write_text(json.dumps(audit,ensure_ascii=False,indent=2),encoding='utf-8')
