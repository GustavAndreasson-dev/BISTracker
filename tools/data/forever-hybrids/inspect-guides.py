import json, re, sys
from pathlib import Path

for path in Path(sys.argv[1]).glob('*.html'):
    html = path.read_text(encoding='utf-8-sig')
    decoder = json.JSONDecoder()
    data = {}
    for match in re.finditer(r'WH\.Gatherer\.addData\((\d+), 16, ', html):
        data.setdefault(int(match[1]), {}).update(decoder.raw_decode(html[match.end():])[0])
    marker = 'WH.markup.printHtml("'
    if marker not in html:
        Path(path.with_suffix('.metadata.json')).write_text(json.dumps(data, ensure_ascii=False, indent=2), encoding='utf-8')
        continue
    start = html.index(marker) + len(marker) - 1
    markup = decoder.raw_decode(html[start:])[0]
    print('\nGUIDE', path.stem)
    for section in re.split(r'(?=\[h2 )', markup):
        if not re.search(r'\[h2[^\]]*\]Best Level 30 ', section):
            continue
        print(re.sub(r'\[[^\]]*\]', '', section.split('\n')[0]))
        for table in re.findall(r'\[table[^\]]*\](.*?)\[/table\]', section, re.S):
            for row in re.findall(r'\[tr[^\]]*\](.*?)\[/tr\]', table, re.S):
                cells = re.findall(r'\[td[^\]]*\](.*?)\[/td\]', row, re.S)
                parts = []
                for cell in cells:
                    def name(m):
                        category = {'item': 3, 'quest': 5, 'zone': 7, 'faction': 8, 'skill': 15}.get(m[1])
                        record = data.get(category, {}).get(m[2], {})
                        return f'{m[1]}={m[2]}:{record.get("name_enus", "?")}'
                    clean = re.sub(r'\[(item|quest|zone|faction|skill)=(\d+)[^\]]*\]', name, cell)
                    clean = re.sub(r'\[[^\]]*\]', ' ', clean)
                    parts.append(' '.join(clean.split()))
                print(' | '.join(parts))
    Path(path.with_suffix('.metadata.json')).write_text(json.dumps(data, ensure_ascii=False, indent=2), encoding='utf-8')
    Path(path.with_suffix('.markup.txt')).write_text(markup, encoding='utf-8')
