"""Extract the Wowhead guide markup (WH.markup.printHtml argument) from a saved HTML page.

Usage: python -I extract_wowhead_guide.py <saved.html> <out.txt>
Writes the raw markup text so item tags ([item=ID]) and tables stay reviewable.
"""
import json
import re
import sys
from pathlib import Path

html = Path(sys.argv[1]).read_text(encoding='utf-8', errors='replace')
chunks = []
for m in re.finditer(r'WH\.markup\.printHtml\(\s*"', html):
    start = m.end() - 1
    # decode the JSON string literal that starts at `start`
    value, _ = json.JSONDecoder().raw_decode(html[start:])
    chunks.append(value)
text = '\n\n=====\n\n'.join(chunks)
text = re.sub(r'\[/?(tr|td|table|tab|tabs|ul|li|b|i|u|center|h\d|toc|span|color|small|div|pad|url)(?=[\s\]=])[^\]]*\]', lambda t: '\n' if t.group(1) in ('tr', 'li') else ' | ' if t.group(1) == 'td' else '', text)
Path(sys.argv[2]).write_text(text, encoding='utf-8')
print(len(chunks), 'markup blocks;', len(set(re.findall(r'\[item=(\d+)', text))), 'distinct item tags')
