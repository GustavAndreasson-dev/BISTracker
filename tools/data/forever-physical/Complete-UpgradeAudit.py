"""Verify exact-spec Forever alternatives; raw source responses stay in TEMP."""
import html
import json
import os
import re
from pathlib import Path

ROOT = Path(__file__).resolve().parents[3]
TEMP = Path(os.environ['TEMP']) / 'BISTracker-forever-physical-research'
CATALOG = ROOT / 'BISTracker.Infrastructure/Catalog/Data/Forever/level30'
FACTS = ROOT / 'docs/data/forever-physical'
SLOTS = ['Head','Neck','Shoulder','Back','Chest','Wrist','Hands','Waist','Legs','Feet','Finger1','Finger2','Trinket1','Trinket2','MainHand','OffHand','Ranged']
KEYS = dict(zip(SLOTS, ['head','neck','shoulder','back','chest','wrist','hands','waist','legs','feet','finger','finger','trinket','trinket','main-hand','off-hand','ranged']))
F = {1:['Alliance'],2:['Horde'],3:['Alliance','Horde']}

def read(path):
    return json.loads(path.read_text(encoding='utf-8-sig'))

def write(path, value):
    path.write_text(json.dumps(value, indent=2, ensure_ascii=False)+'\n', encoding='utf-8')

quests = read(ROOT/'docs/data/forever-casters/quest-metadata.json')
if (FACTS/'upgrade-quest-metadata.json').exists():quests.update(read(FACTS/'upgrade-quest-metadata.json'))
guide_quests = {}
for path in TEMP.glob('*.json'):
    if re.match(r'^(hunter|rogue|warrior)-',path.stem):
        guide_quests.update(read(path)['entities'].get('5',{}))
for qid, q in quests.items():
    guide_quests[qid] = {'name_enus':q['name'],'_side':q['factionCode']}

old_items = {}
for path in CATALOG.glob('*.json'):
    for row in read(path)['items']:
        if row.get('requiredSuffix') is None:
            old_items.setdefault(row['itemId'], row)

def tip(item_id):
    for path in [TEMP/'wowtbc/tips'/f'{item_id}.json', TEMP/'tips'/f'{item_id}.json']:
        if path.exists():
            return read(path)
    raise ValueError(f'Missing Forever tooltip {item_id}')

def meta(item_id):
    t = tip(item_id)
    tt = t['tooltip']
    slot = re.search(r'<td>(Head|Neck|Shoulder|Back|Chest|Wrist|Wrists|Hands|Waist|Legs|Feet|Finger|Trinket|One-Hand|Main Hand|Off Hand|Two-Hand|Ranged|Thrown|Held In Off-hand)</td>', tt)
    if not slot:
        raise ValueError(f'Unreviewed slot {item_id}')
    level = re.search(r'Requires Level\s*(?:<!--rlvl-->)?(\d+)',tt)
    required = int(level.group(1)) if level else 0
    old = old_items.get(item_id)
    if old and not level:
        required = old.get('requiredLevel',0)
    requirements = re.findall(r'Requires\s*(?:<[^>]+>)*(Engineering|Blacksmithing|Leatherworking|Tailoring|Enchanting)\s*(?:<[^>]+>)*\s*\((\d+)\)',tt)
    return {'itemId':item_id,'name':t['name'],'itemUrl':f'https://www.wowhead.com/forever/item={item_id}',
            'tooltipUrl':f'https://nether.wowhead.com/tooltip/item/{item_id}?dataEnv=16&locale=0',
            'observedSlot':slot.group(1),'requiredLevel':required,
            'iconUrl':f"https://wow.zamimg.com/images/wow/icons/large/{t['icon']}.jpg",
            'uniqueEquipped':bool(re.search(r'Unique-Equipped|<br>Unique(?:<|-)',tt)),
            'bindsWhenPickedUp':'Binds when picked up' in tt,
            'equipProfessionRequirements':[{'profession':p,'skill':int(s)} for p,s in requirements],
            'tooltipText':html.unescape(re.sub(r'<[^>]+>',' ',tt))}

def annotate_quests(c):
    for row in c['items']:
        if row['acquisitionType'] != 'Quest':
            continue
        matched = [qid for qid,q in guide_quests.items() if q['name_enus'] in row['source']]
        factions = set()
        for qid in matched:
            factions.update(F.get(int(guide_quests[qid]['_side']),[]))
        row['availableFactions'] = [f for f in ['Alliance','Horde'] if f in factions]
        if not factions:
            row['note'] = row.get('note','')+' Faction availability is unverified; excluded from a faction-complete target set.' if 'Faction availability is unverified' not in row.get('note','') else row['note']

def eligible(c,faction=None,policy=('Dungeon','Quest','Crafting')):
    result = []
    for row in c['items']:
        if row['acquisitionType'] not in policy or row['requiredLevel'] > 30:
            continue
        if c['weaponSetup']=='OneHandAndOffHand' and row['weaponKind']=='TwoHanded':
            continue
        if c['weaponSetup']=='TwoHanded' and row['slot']=='MainHand' and row['weaponKind']!='TwoHanded':
            continue
        if row['acquisitionType']=='Quest' and faction and faction not in row.get('availableFactions',[]):
            continue
        result.append(row)
    return result

def missing(c,faction=None):
    rows = eligible(c,faction)
    covered = set(r['slot'] for r in rows)
    exempt = {'OffHand'} if c['weaponSetup']=='TwoHanded' else set()
    for a,b in [('Finger1','Finger2'),('Trinket1','Trinket2')]:
        distinct = {(r['itemId'],r.get('requiredSuffix')) for r in rows if r['slot'] in (a,b)}
        if len(distinct)<2:
            covered.discard(b)
    # Hunter's named two-handed alternative supplies both melee hands.
    if c['characterClass']=='Hunter' and any(r['slot']=='MainHand' and r['weaponKind']=='TwoHanded' for r in rows):
        exempt.add('OffHand')
    return [s for s in SLOTS if s not in covered and s not in exempt]

def row_slots(observed):
    if observed=='Finger':return ['Finger1','Finger2'],'None'
    if observed=='Trinket':return ['Trinket1','Trinket2'],'None'
    if observed=='One-Hand':return ['MainHand','OffHand'],'OneHanded'
    if observed=='Main Hand':return ['MainHand'],'MainHand'
    if observed in ('Off Hand','Held In Off-hand'):return ['OffHand'],'OffHand'
    if observed=='Two-Hand':return ['MainHand'],'TwoHanded'
    if observed in ('Ranged','Thrown'):return ['Ranged'],'None'
    if observed=='Wrists':return ['Wrist'],'None'
    return [observed],'None'

verified_items = read(FACTS/'upgrade-item-metadata.json') if (FACTS/'upgrade-item-metadata.json').exists() else {}
verified_quests = read(FACTS/'upgrade-quest-metadata.json') if (FACTS/'upgrade-quest-metadata.json').exists() else {}
supplements = read(FACTS/'upgrade-recommendations.json') if (FACTS/'upgrade-recommendations.json').exists() else []
rejected = read(FACTS/'upgrade-exclusions.json') if (FACTS/'upgrade-exclusions.json').exists() else []
recipes = {}
for item_id in [250518,250528,250556,252508,252520,252521,277041,250498,250523,5964,7374,18948,4381]:
    path=Path(os.environ['TEMP'])/'bistracker-forever-hybrids'/f'slot-item-{item_id}.html'
    if not path.exists():continue
    raw=path.read_text(encoding='utf-8-sig')
    match=re.search(r"id: 'created-by-spell'.*?data:\s*(\[[^\r\n]+\])",raw,re.S)
    if not match:raise ValueError(f'Missing recipe proof {item_id}')
    found=[r for r in json.loads(match.group(1)) if r.get('creates',[None])[0]==item_id]
    if not found:raise ValueError(f'No matching crafted item {item_id}')
    r=found[0]
    recipes[str(item_id)]={'itemId':item_id,'spellId':r['id'],'skill':r['learnedat'],
      'profession':{164:'Blacksmithing',165:'Leatherworking',202:'Engineering'}[r['skill'][0]],
      'itemUrl':f'https://www.wowhead.com/forever/item={item_id}',
      'recipeUrl':f"https://www.wowhead.com/forever/spell={r['id']}",
      'recipeCharacterLevel':r['level'],'reviewedOn':'2026-10-08'}
    if r['learnedat']>225 or r['level']>30:raise ValueError(f'Inaccessible crafting recipe {item_id}')

for path in sorted(CATALOG.glob('*.json')):
    if not re.match(r'^(hunter|rogue|warrior)-',path.stem):continue
    c=read(path)
    annotate_quests(c)
    excluded=[r for r in c['items'] if r['acquisitionType']=='Quest' and not r.get('availableFactions')]
    for r in excluded:
        rejected.append({'catalogId':c['catalogId'],'itemId':r['itemId'],'reason':'Quest faction unverified; removed from active catalog','historicalRecommendation':r})
    c['items']=[r for r in c['items'] if r not in excluded]
    for row in c['items']:
        if row['acquisitionType']=='Crafting':
            recipe=recipes.get(str(row['itemId']))
            if not recipe:raise ValueError(f'Unreviewed active crafting recipe {row["itemId"]}')
            m=meta(row['itemId'])
            verified_items[str(row['itemId'])]=dict(m,reviewedOn='2026-10-08')
            proof=f" Recipe: {recipe['recipeUrl']}; {recipe['profession']} {recipe['skill']}, available within the level-30 Expert skill cap of 225."
            if proof not in row.get('note',''):row['note']=row.get('note','')+proof
            if m['bindsWhenPickedUp'] and 'Self-crafted' not in row['note']:
                row['note']+=' Self-crafted bind-on-pickup item; choose compatible professions.'
    gaps=set(missing(c)) | set(missing(c,'Alliance')) | set(missing(c,'Horde'))
    data=read(TEMP/'wowtbc'/f"{c['specializationId']}-{c['characterClass'].lower()}.json")
    needed={KEYS[s] for s in gaps}
    for candidate in data['candidates']:
        if candidate['guideSlot'] not in needed:continue
        item_id=candidate['itemId']
        is_craft=candidate['sourceType']=='Crafted'
        if is_craft and str(item_id) not in recipes:
            rejected.append({'catalogId':c['catalogId'],'itemId':item_id,'reason':'Crafting recipe availability not individually reviewed'});continue
        m=meta(item_id)
        verified_items[str(item_id)]=dict(m,reviewedOn='2026-10-08')
        if m['requiredLevel']>30:
            rejected.append({'catalogId':c['catalogId'],'itemId':item_id,'reason':'Forever equipment level above beta cap','requiredLevel':m['requiredLevel']});continue
        tt=m['tooltipText']
        if 'Classes:' in tt and c['characterClass'] not in tt.split('Classes:',1)[1].split('Requires')[0]:
            rejected.append({'catalogId':c['catalogId'],'itemId':item_id,'reason':'Class restriction'});continue
        slots,kind=row_slots(m['observedSlot'])
        if c['characterClass']=='Warrior' and c['specializationId']=='protection' and kind=='OneHanded':slots=['MainHand']
        if c['weaponSetup']=='OneHandAndOffHand' and kind=='TwoHanded':continue
        if c['weaponSetup']=='TwoHanded' and kind in ('OneHanded','MainHand','OffHand'):continue
        if c['characterClass']=='Rogue' and c['specializationId']=='assassination' and candidate['guideSlot'] in ('main-hand','off-hand') and 'Dagger' not in tt:continue
        if not any(KEYS.get(s)==candidate['guideSlot'] for s in slots):
            rejected.append({'catalogId':c['catalogId'],'itemId':item_id,'reason':'Guide placement disagrees with actual Forever slot'});continue
        is_quest=candidate['sourceType'].startswith('Quest')
        qmatches=[]
        if is_quest:
            for qid,q in quests.items():
                rewards=q.get('itemChoices',[])+q.get('itemRewards',[])
                if any(int(pair[0])==item_id for pair in rewards) and q['minimumLevel']<=30 and q['factionCode'] in F:
                    qmatches.append(q)
            if not qmatches:
                rejected.append({'catalogId':c['catalogId'],'itemId':item_id,'reason':'Quest reward/minimum level/faction not individually verified','guideUrl':data['sourceUrl']});continue
        factions = [f for f in ['Alliance','Horde'] if any(f in F[q['factionCode']] for q in qmatches)]
        for q in qmatches:verified_quests[str(q['questId'])]=q
        note=''
        if qmatches:
            note+='Quest: '+ '; '.join(q['questUrl']+f" (minimum level {q['minimumLevel']}; {'/'.join(F[q['factionCode']])})" for q in qmatches)+'.'
        if kind=='TwoHanded':note+=' Occupies both melee hands.'
        if m['uniqueEquipped']:note+=' Unique-equipped.'
        if is_craft:
            recipe=recipes[str(item_id)]
            note+=f" Recipe: {recipe['recipeUrl']}; {recipe['profession']} {recipe['skill']}, available within the level-30 Expert skill cap of 225."
            if m['bindsWhenPickedUp']:note+=' Self-crafted bind-on-pickup item; choose compatible professions.'
        for s in slots:
            # We retain both legal placements, never use duplicated rows as proof of two targets.
            if any(r['slot']==s and r['itemId']==item_id and r.get('requiredSuffix') is None for r in c['items']):continue
            r={'id':f"{c['characterClass'].lower()}-{c['specializationId']}-{s.lower()}-{item_id}",
               'slot':s,'itemId':item_id,'name':m['name'],'requiredSuffix':None,'requiredLevel':m['requiredLevel'],
               'acquisitionType':'Quest' if is_quest else 'Crafting' if is_craft else 'Dungeon',
               'source':'; '.join(q['name']+f" ({'/'.join(F[q['factionCode']])})" for q in qmatches) if is_quest else candidate['source'] if is_craft else candidate['sourceType']+' — '+candidate['source'],
               'note':note+(' Alternative placement; choose one slot per owned copy.' if len(slots)>1 else ''),
               'iconUrl':m['iconUrl'],'itemUrl':m['itemUrl'],'recommendationUrl':data['sourceUrl'],
               'weaponKind':kind,'uniqueEquipped':m['uniqueEquipped']}
            if is_quest:r['availableFactions']=factions
            c['items'].append(r)
            supplements.append({'catalogId':c['catalogId'],'recommendationId':r['id'],'itemId':item_id,'guideUrl':data['sourceUrl'],'pageDataUrl':data['pageDataUrl'],'guideDeclaredName':candidate['name'],'questIds':[q['questId'] for q in qmatches]})
    c['selectionMethod']='Named alternatives from original Forever level-30 guides and this exact spec’s unranked WOWTBC Forever upgrade list. Dungeon, quest and approved crafting sources only are active. Metadata and quest availability checked separately. No independent ranking or equivalent-value claim.'
    write(path,c)
    print(path.stem, 'global',missing(c),'Alliance',missing(c,'Alliance'),'Horde',missing(c,'Horde'))

write(FACTS/'upgrade-item-metadata.json',verified_items)
write(FACTS/'upgrade-quest-metadata.json',verified_quests)
write(FACTS/'upgrade-recommendations.json',supplements)
write(FACTS/'upgrade-exclusions.json',rejected)
write(FACTS/'crafting-metadata.json',recipes)
print(len(supplements),'new source-backed placement rows')

def chain_valid(qid, seen=None):
    seen=set() if seen is None else seen
    if qid in seen:return True
    seen.add(qid)
    q=quests.get(str(qid))
    return bool(q and q['minimumLevel'] is not None and q['minimumLevel']<=30 and
      all(chain_valid(prior,seen) for prior in q.get('publishedPrerequisiteQuestIds',[])))

def quest_proof(item_id,faction):
    result=[]
    for qid,q in quests.items():
        if faction not in F.get(q['factionCode'],[]):continue
        if not chain_valid(q['questId']):continue
        sources=read(FACTS/'quest-item-source-metadata.json').get(str(item_id),{}).get('observedRewardQuestIds',[]) if (FACTS/'quest-item-source-metadata.json').exists() else []
        if any(int(pair[0])==item_id for pair in q.get('itemChoices',[])+q.get('itemRewards',[])) or q['questId'] in sources:
            result.append(q)
    return sorted(result,key=lambda q:q['questId'])

def solve(c,faction):
    modes=['TwoHanded','OneHandAndOffHand'] if c['characterClass']=='Hunter' else [c['weaponSetup']]
    for mode in modes:
        expected=[s for s in SLOTS if not(mode=='TwoHanded' and s=='OffHand')]
        rows=[]
        for r in eligible(c,faction):
            if r['slot']=='MainHand' and ((mode=='TwoHanded') != (r['weaponKind']=='TwoHanded')):continue
            if r['slot']=='OffHand' and mode=='TwoHanded':continue
            if c['characterClass']=='Rogue' and c['specializationId']=='assassination' and r['slot'] in ('MainHand','OffHand') and 'Dagger' not in meta(r['itemId'])['tooltipText']:continue
            if r['acquisitionType']=='Quest' and not quest_proof(r['itemId'],faction):continue
            rows.append(r)
        groups={s:sorted([r for r in rows if r['slot']==s],key=lambda r:({'Dungeon':0,'Quest':1,'Crafting':2}[r['acquisitionType']],r['itemId'],r['id'])) for s in expected}
        if any(not group for group in groups.values()):
            print('Witness unavailable proof',c['catalogId'],faction,mode,[s for s,g in groups.items() if not g])
        order=sorted(expected,key=lambda s:len(groups[s]))
        def visit(index,selected,choices,professions,selected_quests):
            if index==len(order):return selected,professions,selected_quests
            slot=order[index]
            for row in groups[slot]:
                if any(r['itemId']==row['itemId'] for r in selected.values()):continue
                requirements=dict(professions)
                m=meta(row['itemId'])
                for req in m['equipProfessionRequirements']:
                    requirements[req['profession']]=max(req['skill'],requirements.get(req['profession'],0))
                if row['acquisitionType']=='Crafting' and m['bindsWhenPickedUp']:
                    recipe=recipes[str(row['itemId'])]
                    requirements[recipe['profession']]=max(recipe['skill'],requirements.get(recipe['profession'],0))
                if len(requirements)>2 or any(skill>225 for skill in requirements.values()):continue
                options=quest_proof(row['itemId'],faction) if row['acquisitionType']=='Quest' else [None]
                for q in options:
                    updated=dict(choices)
                    selected_q=dict(selected_quests)
                    if q:
                        if any(int(pair[0])==row['itemId'] for pair in q.get('itemChoices',[])) or not any(int(pair[0])==row['itemId'] for pair in q.get('itemRewards',[])):
                            qid=str(q['questId'])
                            if qid in updated and updated[qid]!=row['itemId']:continue
                            updated[qid]=row['itemId']
                        selected_q[slot]=q
                    found=visit(index+1,dict(selected,**{slot:row}),updated,requirements,selected_q)
                    if found:return found
            return None
        found=visit(0,{},{},{},{})
        if found:
            selected,professions,selected_q=found
            witness=[]
            for slot in expected:
                r=selected[slot]
                entry={key:r.get(key) for key in ('slot','id','itemId','name','requiredSuffix','requiredLevel','acquisitionType','weaponKind','uniqueEquipped','itemUrl','recommendationUrl')}
                entry['recommendationId']=entry.pop('id')
                if slot in selected_q:
                    q=selected_q[slot]
                    entry['questProof']={key:q[key] for key in ('questId','questUrl','minimumLevel','factionCode','itemChoices','itemRewards','publishedPrerequisiteQuestIds')}
                    verified_quests[str(q['questId'])]=q
                    for qid in q['publishedPrerequisiteQuestIds']:
                        verified_quests[str(qid)]=quests[str(qid)]
                if r['acquisitionType']=='Crafting':entry['craftingProof']=recipes[str(r['itemId'])]
                witness.append(entry)
            return {'faction':faction,'missingSlots':[],'exemptSlots':['OffHand'] if mode=='TwoHanded' else [],
                    'exemptionEvidence':[{'slot':'OffHand','reason':'The selected, source-backed two-handed MainHand occupies both melee hands.','itemId':selected['MainHand']['itemId'],'sourceUrl':selected['MainHand']['recommendationUrl']}] if mode=='TwoHanded' else [],
                    'weaponSetup':mode,'fullSetValidated':True,'witnessSelectionMethod':'Feasibility only: deterministic search over allowed source-backed candidates, favoring dungeon/quest feasibility before optional self-crafted rewards. No priority, rank, or equal-strength claim.',
                    'requiredProfessions':[{'profession':p,'skill':s} for p,s in sorted(professions.items())],
                    'incompatibleProfessionRequirements':False,'questAlternativeRewardExclusivityChecked':True,
                    'distinctPhysicalItemsChecked':True,'legalSetWitness':witness}
    return {'faction':faction,'missingSlots':missing(c,faction),'fullSetValidated':False,'legalSetWitness':[],'reason':'No simultaneous set passes distinct-item, hand, quest-choice and profession checks.'}

per_spec=[]
for path in sorted(CATALOG.glob('*.json')):
    if not re.match(r'^(hunter|rogue|warrior)-',path.stem):continue
    c=read(path)
    for r in c['items']:
        note=r.get('note','')
        for phrase in ['Named unranked upgrade in this exact Forever spec guide; no ranking or equivalence is asserted.',
          'Named in the Forever level-30 Protection class-quest recommendations. ',
          'Named level-30 Hunter recommendation; ',
          'No item level requirement is shown in the Forever metadata.',
          'The guide describes these as lower-power green rewards with useful stats, not the strongest level-30 choice.',
          'Use a legal MainHand/OffHand combination for the chosen spec.']:
            note=note.replace(phrase,'')
        note=note.replace('Verified Forever quest rewards: ','Quest: ').replace('Self-crafted bind-on-pickup item; choose compatible professions.','Self-crafted bind-on-pickup item.')
        note=note.replace(', available within the level-30 Expert skill cap of 225.', '.')
        r['note']=' '.join(note.strip().split())
    factions=[solve(c,f) for f in ['Alliance','Horde']]
    if not all(f['fullSetValidated'] for f in factions):raise ValueError(f'Blocked simultaneous set {path.stem}: {factions}')
    for r in c['items']:
        m=meta(r['itemId'])
        verified_items[str(r['itemId'])]={k:v for k,v in dict(m,reviewedOn='2026-10-08').items() if k!='tooltipText'}
    c['status']='Reviewed'
    write(path,c)
    quest_rows=[{'recommendationId':r['id'],'itemId':r['itemId'],'availableFactions':r['availableFactions'],
      'factionProof':'Exact source quest faction from original Forever guide Gatherer and/or individually reviewed quest metadata.',
      'questProof':[q for q in quests.values() if any(int(p[0])==r['itemId'] for p in q.get('itemChoices',[])+q.get('itemRewards',[]))]} for r in c['items'] if r['acquisitionType']=='Quest']
    crafting_rows=[]
    for r in c['items']:
        if r['acquisitionType']!='Crafting':continue
        m=meta(r['itemId'])
        crafting_rows.append(dict(recipes[str(r['itemId'])],recommendationId=r['id'],bindsWhenPickedUp=m['bindsWhenPickedUp'],requiresCrafterProfession=m['bindsWhenPickedUp'],equipProfessionRequirements=m['equipProfessionRequirements'],compatibleWithinExpertCap=True))
    alternatives=[{'slot':s,'alternatives':[{'recommendationId':r['id'],'itemId':r['itemId'],'recommendationUrl':r['recommendationUrl']} for r in eligible(c) if r['slot']==s],'equivalenceEstablished':False} for s in SLOTS if any(r['slot']==s for r in eligible(c))]
    sources=sorted({r['recommendationUrl'] for r in c['items']})
    per_spec.append({'catalogId':c['catalogId'],'characterClass':c['characterClass'],'specializationId':c['specializationId'],'weaponSetup':c['weaponSetup'],'weaponSetupSourceUrl':c.get('weaponSetupSourceUrl'),
      'missingSlots':[],'exemptSlots':['OffHand'] if c['weaponSetup']=='TwoHanded' else [],
      'conditionalExemptions':'Hunter OffHand is occupied by the witnessed two-handed MainHand; dual-wield alternatives require a separate OffHand.' if c['characterClass']=='Hunter' else None,
      'fullSetValidated':True,'setCompleteness':'Source-backed simultaneous set verified separately for each faction',
      'factions':factions,'alternativesBySlot':alternatives,'equivalentGroups':[],
      'questAvailability':quest_rows,'unresolvedQuestFactions':[],'craftingAudit':crafting_rows,
      'reviewedSources':sources,'rawPlacementRows':len(c['items']),'physicalVariants':len({(r['itemId'],r.get('requiredSuffix')) for r in c['items']}),
      'quantityLimitation':'Each witness uses distinct physical base item IDs; alternative placements do not establish ownership of two copies.'})
    print(path.stem,'PASS separate faction witnesses',[(f['faction'],len(f['legalSetWitness']),f['requiredProfessions']) for f in factions])
write(FACTS/'slot-audit.json',{'schemaVersion':1,'reviewedOn':'2026-10-08','gameVersion':'Forever','levelCap':30,'releaseStage':'Beta','patch':'1.60.1','acquisitionPolicy':['Dungeon','Quest','Crafting'],
  'countingMethod':'One target per relevant equipment slot; source alternatives grouped inside each target. Alternative-placement rows do not add slots.',
  'validationScope':'Separate simultaneously obtainable feasibility witness for Alliance and Horde, distinct physical IDs, legal melee hands, quest-choice exclusivity, published quest-chain level requirements and at most two compatible equip/self-crafting professions. No game-client playthrough or independent gear optimization.',
  'professionCapEvidence':{'expertSkillCap':225,'characterLevelRequired':20,'sourceUrls':['https://www.icy-veins.com/wow-forever/blacksmithing','https://www.icy-veins.com/wow-forever/leatherworking','https://www.icy-veins.com/wow-forever/engineering']},
  'perSpec':per_spec})
write(FACTS/'upgrade-item-metadata.json',{key:{k:v for k,v in value.items() if k!='tooltipText'} for key,value in verified_items.items()})
write(FACTS/'upgrade-quest-metadata.json',verified_quests)
