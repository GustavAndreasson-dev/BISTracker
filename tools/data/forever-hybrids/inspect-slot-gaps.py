"""Read downloaded original Forever lists; never modify product catalogs."""
import json
import sys
from pathlib import Path

root = Path(__file__).resolve().parents[3]
sources = Path(sys.argv[1])
slots = ['Head', 'Neck', 'Shoulder', 'Back', 'Chest', 'Wrist', 'Hands', 'Waist', 'Legs', 'Feet', 'Finger1', 'Finger2', 'Trinket1', 'Trinket2', 'MainHand', 'OffHand', 'Ranged']
keys = dict(zip(slots, ['head','neck','shoulder','back','chest','wrist','hands','waist','legs','feet','finger','finger','trinket','trinket','main-hand','off-hand','ranged']))
for path in sorted((root/'BISTracker.Infrastructure/Catalog/Data/Forever/level30').glob('*.json')):
    if not path.stem.startswith(('druid-', 'paladin-', 'shaman-')):
        continue
    catalog = json.loads(path.read_text(encoding='utf-8'))
    cls, spec = path.stem.split('-')
    slug = f'{spec}-{cls}' if spec != 'feral' else 'feral-dps-druid'
    page = json.loads((sources/f'wowtbc-{slug}-data.json').read_text(encoding='utf-8-sig'))['result']['pageContext']
    metadata = {i['id']:i for i in page['gearData']}
    used = {i['slot'] for i in catalog['items'] if i['acquisitionType'] in ['Dungeon','Quest']}
    print('\n'+path.stem)
    for slot in slots:
        if slot in used or slot in ['Finger2','Trinket2']:
            continue
        print(slot)
        for iid in page['bisList'].get(keys[slot], []):
            item = metadata[iid]
            print(iid, item['name'], '|', item.get('source','?'), '|', item.get('source_type','?'), '|', item.get('other_stats',{}).get('min_level', '?'))
