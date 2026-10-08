"""Review explicit original Forever alternatives, then optionally apply to owned catalogs.

Requires downloaded original guide/page-data/item HTML. Full source pages remain temporary.
No ranking or generic stat selection is performed. --apply is deliberately explicit.
"""
import html
import json
import re
import shutil
import sys
import urllib.request
from concurrent.futures import ThreadPoolExecutor
from pathlib import Path

root = Path(__file__).resolve().parents[3]
sources = Path(sys.argv[1])
apply = '--apply' in sys.argv[2:]
evidence = root/'docs/data/forever-hybrids'
out = root/'BISTracker.Infrastructure/Catalog/Data/Forever/level30'
candidates = json.loads((evidence/'slot-supplement-candidates.json').read_text(encoding='utf-8-sig'))
decoder = json.JSONDecoder()
slotmap = {1:'Head',2:'Neck',3:'Shoulder',5:'Chest',6:'Waist',7:'Legs',8:'Feet',9:'Wrist',10:'Hands',11:'Finger1',12:'Trinket1',13:'MainHand',14:'OffHand',15:'Ranged',16:'Back',17:'MainHand',20:'Chest',21:'MainHand',22:'OffHand',23:'OffHand',25:'Ranged',26:'Ranged',28:'Ranged'}
sourcekeys = {'Head':'head','Neck':'neck','Shoulder':'shoulder','Back':'back','Chest':'chest','Wrist':'wrist','Hands':'hands','Waist':'waist','Legs':'legs','Feet':'feet','Finger1':'finger','Trinket1':'trinket','MainHand':'main-hand','OffHand':'off-hand','Ranged':'ranged'}
allslots = list(sourcekeys)[:10]+['Finger1','Finger2','Trinket1','Trinket2','MainHand','OffHand','Ranged']

def gather(text):
    result = {}
    for match in re.finditer(r'WH\.Gatherer\.addData\((\d+), 16, ', text):
        result.setdefault(int(match[1]),{}).update(decoder.raw_decode(text[match.end():])[0])
    return result

def listview(text, view):
    match = re.search(r"id:\s*['\"]"+re.escape(view)+r"['\"][\s\S]*?data:\s*",text)
    return decoder.raw_decode(text[match.end():])[0] if match else []

metadata = {}
for path in sources.glob('*.metadata.json'):
    for category, records in json.loads(path.read_text(encoding='utf-8-sig')).items():
        metadata.setdefault(int(category),{}).update(records)
for path in sources.glob('slot-item-*.html'):
    for category, records in gather(path.read_text(encoding='utf-8-sig')).items():
        metadata.setdefault(category,{}).update(records)

questrecords = {}
for path in sources.glob('slot-quest-*.html'):
    text = path.read_text(encoding='utf-8-sig')
    for match in re.finditer(r'\$\.extend\(g_quests\[(\d+)\],\s*',text):
        questrecords[int(match[1])] = decoder.raw_decode(text[match.end():])[0]
for path in sources.glob('slot-item-*.html'):
    for quest in listview(path.read_text(encoding='utf-8-sig'),'reward-from-q'):
        questrecords.setdefault(quest['id'],quest)

def factions(quest):
    return ['Alliance'] if quest.get('side')==1 else ['Horde'] if quest.get('side')==2 else ['Alliance','Horde'] if quest.get('side')==3 else []

def tooltip(iid):
    path = sources/f'tooltip-{iid}.json'
    if path.exists():
        return json.loads(path.read_text(encoding='utf-8-sig'))
    url = f'https://nether.wowhead.com/forever/tooltip/item/{iid}?dataEnv=16&locale=0'
    data = json.load(urllib.request.urlopen(url,timeout=30))
    icon = f'https://wow.zamimg.com/images/wow/icons/large/{data["icon"].lower()}.jpg'
    response = urllib.request.urlopen(urllib.request.Request(icon,method='HEAD'),timeout=30)
    record = {'itemId':iid,'metadata':data,'iconUrl':icon,'iconHttpStatus':response.status,'reviewedOn':'2026-10-08'}
    path.write_text(json.dumps(record,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
    return record

ids = {i['itemId'] for spec in candidates['catalogs'].values() for i in spec['items']}|{i['itemId'] for i in candidates['balanceQuestRows']}
with ThreadPoolExecutor(max_workers=5) as pool:
    proofs = dict(zip(sorted(ids),pool.map(tooltip,sorted(ids))))

reviews = []
catalogs = {}
for name, supplement in candidates['catalogs'].items():
    catalog = json.loads((out/f'{name}.json').read_text(encoding='utf-8'))
    cls,spec = name.split('-')
    slug = f'{spec}-{cls}' if spec != 'feral' else 'feral-dps-druid'
    page = json.loads((sources/f'wowtbc-{slug}-data.json').read_text(encoding='utf-8-sig'))['result']['pageContext']
    originalitems = {i['id']:i for i in page['gearData']}
    selected = list(supplement['items'])
    if name=='druid-balance':
        selected += [dict(i,slot='Hands',recommendationUrl=catalog['sourceUrl']) for i in candidates['balanceQuestRows']]
    for candidate in selected:
        iid,slot = candidate['itemId'],candidate['slot']
        recurl = candidate.get('recommendationUrl',supplement['recommendationUrl'])
        original = originalitems.get(iid,{})
        if 'wowtbc.gg' in recurl:
            assert iid in page['bisList'][sourcekeys[slot]], (name,slot,iid,'not recommended')
        elif name=='druid-balance':
            assert f'[item={iid}]' in (sources/'druid-balance-dps.markup.txt').read_text(encoding='utf-8')
        # Icy Veins candidates are individually observed in its original level-30 table;
        # their exact recommendation URL/source are part of the reviewed candidate file.
        meta = metadata[3][str(iid)]
        invslot = meta['jsonequip']['slotbak']
        assert slotmap[invslot]==slot, (iid,slot,invslot)
        proof = proofs[iid]
        plain = ' '.join(html.unescape(re.sub('<[^>]+>',' ',proof['metadata']['tooltip'])).split())
        required = re.search(r'Requires Level (\d+)',plain)
        level = int(required[1]) if required else meta['jsonequip'].get('reqlevel',0)
        assert level<=30 and proof['iconHttpStatus']==200, (iid,level)
        assert meta['name_enus']==proof['metadata']['name'], (iid,'name disagreement')
        note = []
        questids = [candidate['questId']] if 'questId' in candidate else []
        if iid in [263411,263412]:
            questids = [31,5061] if iid==263411 else [1527,94468,97257]
            rewards = {q['id']:q for q in listview((sources/f'slot-item-{iid}.html').read_text(encoding='utf-8-sig'),'reward-from-q')}
            assert set(questids)<=set(rewards), (iid,'class quest relation missing')
            note.append('Beta reward moved from Meddlesome Mages to the class quest chain. Characters that completed the class quest before the change may be unable to obtain it.')
        available = []
        if candidate.get('acquisitionType')=='Crafting':
            recipes=[r for r in listview((sources/f'slot-item-{iid}.html').read_text(encoding='utf-8-sig'),'created-by-spell') if r.get('creates',[0])[0]==iid]
            assert recipes and recipes[0]['learnedat']<=225, (iid,'unverified crafting')
            acquisition='Crafting'
            source=original['source']
            if 'Binds when picked up' in plain: note.append('Crafted item binds on pickup.')
        elif questids:
            relation = {q['id'] for q in listview((sources/f'slot-item-{iid}.html').read_text(encoding='utf-8-sig'),'reward-from-q')}
            assert set(questids)<=relation, (iid,questids,'quest reward relation not observed')
            for qid in questids:
                quest = questrecords[qid]
                assert 'reqlevel' in quest and quest['reqlevel']<=30, (qid,'quest exceeds cap')
                assert factions(quest), (qid,'unknown faction')
                available += factions(quest)
            available = sorted(set(available))
            source = ' / '.join(dict.fromkeys(questrecords[q]['name'] for q in questids))
            if available==['Horde']: note.append('Horde only.')
            elif available==['Alliance']: note.append('Alliance only.')
            acquisition = 'Quest'
        else:
            acquisition = 'Dungeon'
            source = candidate.get('source',original.get('source_type','')+' · '+original.get('source',''))
            drops = listview((sources/f'slot-item-{iid}.html').read_text(encoding='utf-8-sig'),'dropped-by')
            assert drops, (iid,'no observed Forever drop sources')
            if proof['metadata']['tooltip'].find('Binds when equipped')>=0:
                note.append('Dungeon-farmed BoE alternative; auction-house acquisition is outside this list.')
        if name=='druid-feral': note.append('Feral damage alternative; guide does not establish equivalence with tank recommendations.')
        unique = bool(re.search(r'\bUnique(?:-Equipped)?\b',plain))
        if unique: note.append('Unique; one copy may be equipped.')
        weapon = 'TwoHanded' if invslot==17 else 'OneHanded' if invslot==13 else 'MainHand' if invslot==21 else 'OffHand' if invslot in [14,22,23] else 'None'
        placements = [slot,'Trinket2'] if slot=='Trinket1' else [slot,'Finger2'] if slot=='Finger1' else [slot]
        for placement in placements:
            rid = f'{catalog["catalogId"]}-{placement.lower()}-{iid}'
            if any(i['id']==rid for i in catalog['items']): continue
            row = {'id':rid,'slot':placement,'itemId':iid,'name':proof['metadata']['name'],'requiredSuffix':None,'acquisitionType':acquisition,'source':source,'note':' '.join(note+(['Alternative slot placement; not a recommendation to obtain two copies.'] if slot in ['Trinket1','Finger1'] else [])),'iconUrl':proof['iconUrl'],'itemUrl':f'https://www.wowhead.com/forever/item={iid}','recommendationUrl':recurl,'weaponKind':weapon,'uniqueEquipped':unique,'requiredLevel':level}
            if acquisition=='Quest': row['availableFactions']=available
            catalog['items'].append(row)
        reviews.append({'catalogId':catalog['catalogId'],'slot':slot,'itemId':iid,'name':proof['metadata']['name'],'recommendationUrl':recurl,'itemUrl':f'https://www.wowhead.com/forever/item={iid}','metadataUrl':f'https://nether.wowhead.com/forever/tooltip/item/{iid}?dataEnv=16&locale=0','requiredLevel':level,'questIds':questids,'questDetails':[{'questId':q,'name':questrecords[q]['name'],'requiredLevel':questrecords[q]['reqlevel'],'side':questrecords[q]['side'],'url':f'https://www.wowhead.com/forever/quest={q}'} for q in questids],'availableFactions':available if acquisition=='Quest' else ['Alliance','Horde'],'source':source,'iconUrl':proof['iconUrl'],'iconHttpStatus':proof['iconHttpStatus']})
    catalogs[name] = catalog

# Annotate all existing quest rows from original Forever quest metadata, not from
# inferred class/race stereotypes. Source strings can name alternate faction quests.
questname = {}
for qid,record in metadata.get(5,{}).items():
    questname.setdefault(record['name_enus'],[]).append((int(qid),record.get('_side')))
for qid,record in questrecords.items():
    questname.setdefault(record['name'],[]).append((qid,record.get('side')))
unknown = []
factionreviews = []
prioraudit=evidence/'slot-audit.json'
excluded=json.loads(prioraudit.read_text(encoding='utf-8-sig')).get('excludedItems',[]) if prioraudit.exists() else []
for name,catalog in catalogs.items():
    catalog['selectionMethod']='Unranked equipment alternatives explicitly recommended by original Forever guides and original WOWTBC Forever class/spec lists; no invented stat weights or slot replacements.'
    catalog['items']=[i for i in catalog['items'] if not (name=='druid-balance' and i['itemId']==279837)]
    unresolved=[i for i in catalog['items'] if i['itemId'] in [271767,270036]]
    for item in unresolved:
        excluded.append({'catalogId':catalog['catalogId'],'itemId':item['itemId'],'name':item['name'],'recommendationUrl':item['recommendationUrl'],'itemUrl':item['itemUrl'],'reason':'Healer staff Alliance acquisition fix remains unverified.' if item['itemId']==271767 else 'Item reward relation and older quest reward ID table disagree; acquisition remains unverified.'})
    catalog['items']=[i for i in catalog['items'] if i['itemId'] not in [271767,270036]]
    for item in catalog['items']:
        item['note']=item['note'].replace(' Feral damage alternative; guide does not establish equivalence with tank recommendations.',' Feral damage alternative.').replace('Feral damage alternative; guide does not establish equivalence with tank recommendations.','Feral damage alternative.').replace('Spellbear alternative in the Balance guide.','Spellbear alternative.').replace('Alternative slot placement; not a recommendation to obtain two copies.','').strip()
        item['note']=' '.join(item['note'].split())
    if name=='druid-balance':
        excluded.append({'catalogId':catalog['catalogId'],'itemId':279837,'name':"Fallen Guard's Pendant",'recommendationUrl':catalog['sourceUrl'],'questId':92457,'questUrl':'https://www.wowhead.com/forever/quest=92457/starving-arcane','reason':'Quest metadata reports Side None; no explicit faction availability was verified. Removed from the active pack pending evidence, even though other Neck alternatives cover both factions.'})
    for item in catalog['items']:
        if item['acquisitionType']!='Quest': continue
        found = []
        for qname,records in questname.items():
            if qname and qname in item['source']:
                found += records
        # The initial Balance three-column table carried a dungeon label as source
        # for this pair of explicitly named Alliance/Horde quests.
        if name=='druid-balance' and item['itemId']==4197:
            found = [(1101,1),(1102,2)]
            item['source']='The Crone of the Kraul / A Vengeful Fate'
        sides = {s for _,s in found}
        item['availableFactions'] = ['Alliance','Horde'] if sides & {3} or {1,2}<=sides else ['Alliance'] if sides=={1} else ['Horde'] if sides=={2} else []
        if item['itemId']==271740:
            # Wowhead's original Excavation Site report (Oct 6) lists Open the
            # Maw explicitly under Horde Quests, despite Gatherer's _side=0.
            item['availableFactions']=['Horde']
            if 'Horde only.' not in item['note']: item['note']=(item['note']+' Horde only.').strip()
        factionreviews.append({'catalogId':catalog['catalogId'],'itemId':item['itemId'],'source':item['source'],'availableFactions':item['availableFactions'],'questMetadata':[{'questId':qid,'side':side,'url':f'https://www.wowhead.com/forever/quest={qid}'} for qid,side in sorted(set(found))],'factionSourceOverride':'https://www.wowhead.com/ptr/news/every-quest-in-excavation-site-wetlands-wow-forever-383239' if item['itemId']==271740 else None})
        if not item['availableFactions']: unknown.append({'catalogId':catalog['catalogId'],'itemId':item['itemId'],'source':item['source']})

professions={164:'Blacksmithing',165:'Leatherworking',197:'Tailoring',202:'Engineering',333:'Enchanting'}
craftingreviews={}
for name,catalog in catalogs.items():
    for item in catalog['items']:
        if item['acquisitionType']!='Crafting': continue
        iid=item['itemId']
        proof=tooltip(iid)
        plain=' '.join(html.unescape(re.sub('<[^>]+>',' ',proof['metadata']['tooltip'])).split())
        recipes=[r for r in listview((sources/f'slot-item-{iid}.html').read_text(encoding='utf-8-sig'),'created-by-spell') if r.get('creates',[0])[0]==iid and r.get('skill',[0])[0] in professions]
        assert recipes, (iid,'missing original Forever recipe data')
        recipe=min(recipes,key=lambda r:r['learnedat'])
        skill=recipe['learnedat']
        assert skill<=225, (iid,'profession requires Artisan at level35')
        profession=professions[recipe['skill'][0]]
        bind='BindOnPickup' if 'Binds when picked up' in plain else 'BindOnEquip' if 'Binds when equipped' in plain else 'Unbound'
        equip=re.search(r'Requires (Blacksmithing|Leatherworking|Tailoring|Engineering|Enchanting)\s*\((\d+)\)',plain)
        item['source']=f'{profession} {skill}'
        addition=f'Self-crafting requires {profession} {skill}.' if bind=='BindOnPickup' else f'Crafting requires {profession} {skill}.'
        if addition not in item['note']: item['note']=(item['note']+' '+addition).strip()
        if iid in [249396,249397,249398]:
            recipe_access="Recipe: 45 Merchant's Favor from Alynsia (Alliance) or Beneris (Horde) at the supply camp."
            if recipe_access not in item['note']: item['note']=(item['note']+' '+recipe_access).strip()
        craftingreviews[iid]={'itemId':iid,'name':item['name'],'profession':profession,'requiredCraftingSkill':skill,'craftingSpellId':recipe['id'],'craftingSpellUrl':f'https://www.wowhead.com/forever/spell={recipe["id"]}','itemUrl':item['itemUrl'],'binding':bind,'requiredEquipProfession':equip[1] if equip else None,'requiredEquipSkill':int(equip[2]) if equip else None,'professionRank':'Journeyman' if skill<=150 else 'Expert','minimumCharacterLevelForRank':10 if skill<=150 else 20,'rankSourceUrl':f'https://www.icy-veins.com/wow-forever/{profession.lower()}','reagents':[{'itemId':r[0],'quantity':r[1]} for r in recipe.get('reagents',[])]}

excluded=list({(i['catalogId'],i['itemId']):i for i in excluded}.values())
assert not unknown, ('Unknown quest factions must not be published',unknown)
audit = {'reviewedOn':'2026-10-08','gameVersion':'Forever','levelCap':30,'releaseStage':'Beta','policy':['Dungeon','Quest','Crafting'],'groupingMethod':'One equipment target per slot; original alternatives are grouped by placement. No guide declares these alternatives equal or supplies a complete ranked set.','perSpec':[],'newItemReviews':reviews,'existingQuestFactionReviews':factionreviews,'excludedItems':excluded,'unknownQuestFactions':unknown,'craftingReviews':sorted(craftingreviews.values(),key=lambda i:i['itemId']),'limitations':['Crafting is authorized for Forever. Self-crafted BoP items from different professions are alternatives, not a universally simultaneous optimal set.','Relic acquisition moved to class quests during beta. Existing beta characters that completed those quests before the change may be unable to obtain the rewards. Full quest chains have not been played through in the client.']}

def legal_witness(catalog,faction):
    available=[i for i in catalog['items'] if i['acquisitionType'] in audit['policy'] and (i['acquisitionType']!='Quest' or faction in i.get('availableFactions',[]))]
    def quest_options(item):
        if item['acquisitionType']!='Quest': return [None]
        ids=[]
        for qname,records in questname.items():
            if qname and qname in item['source']:
                ids += [qid for qid,side in records if side==3 or side==(1 if faction=='Alliance' else 2)]
        if item['itemId']==271740: ids=[95682] if faction=='Horde' else []
        observed=sources/f'slot-item-{item["itemId"]}.html'
        if observed.exists():
            rewardids={q['id'] for q in listview(observed.read_text(encoding='utf-8-sig'),'reward-from-q')}
            if rewardids: ids=[qid for qid in ids if qid in rewardids]
        return sorted(set(ids))
    plans=[]
    choices={s:sorted([i for i in available if i['slot']==s],key=lambda i:({'Dungeon':0,'Quest':1,'Crafting':2}[i['acquisitionType']],i['itemId'])) for s in allslots}
    order=['MainHand','OffHand']+sorted([s for s in allslots if s not in ['MainHand','OffHand']],key=lambda s:len(choices[s]))
    def solve(index,useditems,usedquests,craftprof,equipprof):
        if index==len(order): return True
        slot=order[index]
        twohand=any(i['slot']=='MainHand' and i['weaponKind']=='TwoHanded' for i,_ in plans)
        if slot=='OffHand' and twohand: return solve(index+1,useditems,usedquests,craftprof,equipprof)
        for item in choices[slot]:
            if item['itemId'] in useditems: continue
            if slot=='MainHand' and catalog.get('weaponSetup')=='TwoHanded' and item['weaponKind']!='TwoHanded': continue
            craft=craftingreviews.get(item['itemId']) if item['acquisitionType']=='Crafting' else None
            newcraft=craftprof|({craft['profession']} if craft and craft['binding']=='BindOnPickup' else set())
            newequip=equipprof|({craft['requiredEquipProfession']} if craft and craft['requiredEquipProfession'] else set())
            if len(newcraft|newequip)>2: continue
            for quest in quest_options(item):
                if quest is not None and quest in usedquests: continue
                plans.append((item,quest))
                if solve(index+1,useditems|{item['itemId']},usedquests|({quest} if quest is not None else set()),newcraft,newequip): return True
                plans.pop()
        return False
    success=solve(0,set(),set(),set(),set())
    if not success:
        print('Witness failure',catalog['catalogId'],faction,[(s,[(i['itemId'],quest_options(i),i['acquisitionType']) for i in choices[s]]) for s in allslots])
    witness=[{'slot':i['slot'],'recommendationId':i['id'],'itemId':i['itemId'],'name':i['name'],'weaponKind':i['weaponKind'],'uniqueEquipped':i['uniqueEquipped'],'itemUrl':i['itemUrl'],'recommendationUrl':i['recommendationUrl'],'acquisitionType':i['acquisitionType'],'selectedQuestId':qid} for i,qid in sorted(plans,key=lambda pair:allslots.index(pair[0]['slot']))] if success else []
    crafts=[craftingreviews[i['itemId']] for i,qid in plans if i['acquisitionType']=='Crafting']
    questproof=[]
    for item,qid in plans:
        if qid is None: continue
        quest=questrecords[qid]
        assert quest['reqlevel']<=30, (item['itemId'],qid,'witness quest exceeds cap')
        relations=listview((sources/f'slot-item-{item["itemId"]}.html').read_text(encoding='utf-8-sig'),'reward-from-q')
        assert qid in {q['id'] for q in relations}, (item['itemId'],qid,'witness reward relation unverified')
        questproof.append({'questId':qid,'itemId':item['itemId'],'questUrl':f'https://www.wowhead.com/forever/quest={qid}','itemUrl':item['itemUrl'],'requiredLevel':quest['reqlevel'],'rewardRelationObserved':True})
    required=sorted({r['profession'] for r in crafts if r['binding']=='BindOnPickup'}|{r['requiredEquipProfession'] for r in crafts if r['requiredEquipProfession']})
    return {'faction':faction,'fullSetValidated':success,'witnessSourcePolicy':audit['policy'],'selectionMethod':'Feasibility witness only, not a ranking. Dungeon alternatives are considered first to avoid unnecessary self-crafting requirements; distinct item identities and at most one chosen reward per quest.','requiredProfessions':required,'incompatibleProfessionRequirements':[] if success else ['No legal full-set witness found.'],'legalSetWitness':witness,'craftingRequirements':crafts,'questRewardChoices':questproof}
for name,catalog in catalogs.items():
    if name in ['druid-feral','paladin-retribution','shaman-enhancement']:
        assert all(i['weaponKind']=='TwoHanded' for i in catalog['items'] if i['slot']=='MainHand'), name
        catalog['weaponSetup']='TwoHanded'
        catalog['weaponSetupSourceUrl']=catalog['sourceUrl']
    rows = [i for i in catalog['items'] if i['acquisitionType'] in audit['policy']]
    missing = [s for s in allslots if not any(i['slot']==s for i in rows)]
    exemptions = []
    if 'OffHand' in missing and all(i['weaponKind']=='TwoHanded' for i in rows if i['slot']=='MainHand'):
        missing.remove('OffHand')
        exemptions.append({'slot':'OffHand','condition':'All source-selected main-hand alternatives are two-handed. A two-handed selection occupies both hands.','sourceUrls':sorted({i['recommendationUrl'] for i in rows if i['slot']=='MainHand'})})
    perFaction = {}
    for faction in ['Alliance','Horde']:
        available = [i for i in rows if i['acquisitionType']!='Quest' or faction in i.get('availableFactions',[])]
        perFaction[faction] = [s for s in allslots if s not in {e['slot'] for e in exemptions} and not any(i['slot']==s for i in available)]
    witnesses=[legal_witness(catalog,f) for f in ['Alliance','Horde']]
    assert all(w['fullSetValidated'] for w in witnesses), (name,'No legal full set for both factions')
    audit['perSpec'].append({'catalogId':catalog['catalogId'],'characterClass':catalog['characterClass'],'specializationId':catalog['specializationId'],'expectedSlots':allslots,'coveredSlots':[s for s in allslots if any(i['slot']==s for i in rows)],'missingSlots':missing,'exemptSlots':exemptions,'missingSlotsByFaction':perFaction,'slotGroups':[{'slot':s,'alternativeItemIds':sorted({i['itemId'] for i in rows if i['slot']==s}),'declaredEquivalent':False} for s in allslots if any(i['slot']==s for i in rows)],'factions':witnesses,'rawRows':len(catalog['items']),'activeRows':len(rows),'dungeonQuestRows':len([i for i in rows if i['acquisitionType'] in ['Dungeon','Quest']])})
    print(name,len(catalog['items']),len(rows),'missing:',missing,'by faction:',perFaction)
print('Unverified quest factions:',unknown)
if apply:
    backup = sources/'before-slot-supplements'
    backup.mkdir(exist_ok=True)
    for name,catalog in catalogs.items():
        path = out/f'{name}.json'
        if not (backup/path.name).exists(): shutil.copy2(path,backup/path.name)
        path.write_text(json.dumps(catalog,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
    (evidence/'slot-audit.json').write_text(json.dumps(audit,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
else:
    (sources/'slot-audit-proposed.json').write_text(json.dumps(audit,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
