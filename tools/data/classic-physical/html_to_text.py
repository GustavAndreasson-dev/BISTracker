"""Render a saved guide HTML page (Icy Veins / Warcraft Tavern) to reviewable text.

Usage: python -I html_to_text.py <saved.html> <out.txt>
Item links become [item=ID] tokens; table cells are separated by ' | '.
"""
import html
import re
import sys
from pathlib import Path

t = Path(sys.argv[1]).read_text(encoding='utf-8', errors='replace')
t = re.sub(r'<(script|style|noscript)\b.*?</\1>', '', t, flags=re.S | re.I)
t = re.sub(r'<a[^>]*?item[=/](\d+)[^>]*>', r'[item=\1]', t)
t = re.sub(r'<a[^>]*?quest[=/](\d+)[^>]*>', r'[quest=\1]', t)
t = re.sub(r'<a[^>]*?spell[=/](\d+)[^>]*>', r'[spell=\1]', t)
t = re.sub(r'</t[dh]>', ' | ', t)
t = re.sub(r'</tr>|<br\s*/?>|</p>|</h\d>|</li>|</div>', '\n', t)
t = re.sub(r'<h(\d)[^>]*>', lambda m: '\n' + '#' * int(m.group(1)) + ' ', t)
t = re.sub(r'<[^>]+>', '', t)
t = html.unescape(t)
t = re.sub(r'[ \t]+', ' ', t)
t = re.sub(r'\n\s*\n+', '\n', t)
Path(sys.argv[2]).write_text(t, encoding='utf-8')
print(sys.argv[2], len(t), 'chars;', len(set(re.findall(r'\[item=(\d+)', t))), 'distinct items')
