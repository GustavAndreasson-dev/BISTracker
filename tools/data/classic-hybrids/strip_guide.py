"""Convert a downloaded guide HTML page to plain text with item IDs inline.

Usage: python -I tools/data/classic-hybrids/strip_guide.py <page.html> <out.txt>
Links to wowhead items become "[<itemId>]" so slot tables can be reviewed.
"""
import html
import re
import sys


def strip(text):
    text = re.sub(r"(?s)<script.*?</script>|<style.*?</style>", "", text)
    text = re.sub(r"<a[^>]*item=(\d+)[^>]*>", r"[\1]", text)
    text = re.sub(r"</(tr|p|h\d|li|div)>", "\n", text)
    text = re.sub(r"</t[dh]>", " | ", text)
    text = re.sub(r"<[^>]+>", "", text)
    text = html.unescape(text)
    text = re.sub(r"[ \t]+", " ", text)
    return re.sub(r"\n\s*\n+", "\n", text)


if __name__ == "__main__":
    source = open(sys.argv[1], encoding="utf-8", errors="replace").read()
    with open(sys.argv[2], "w", encoding="utf-8") as out:
        out.write(strip(source))
