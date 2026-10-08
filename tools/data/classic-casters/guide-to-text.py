"""Convert a saved guide HTML page into plain text with table rows and item IDs.

Usage: python -I guide-to-text.py <input.html> <output.txt>

Table cells keep linked Wowhead/ClassicDB item IDs as [#id] so the research
files can record the exact item the guide links to.
"""
import re
import sys
from html.parser import HTMLParser


class GuideText(HTMLParser):
    def __init__(self):
        super().__init__()
        self.out = []
        self.cell = None
        self.row = None
        self.skip = 0

    def handle_starttag(self, tag, attrs):
        attrs = dict(attrs)
        if tag in ("script", "style", "noscript"):
            self.skip += 1
        if tag in ("h1", "h2", "h3", "h4"):
            self.out.append("\n## ")
        if tag == "tr":
            self.row = []
        if tag in ("td", "th"):
            self.cell = []
        key = attrs.get("data-z-name-key") or ""
        zmatch = re.search(r"items:(\d+)$", key)
        if zmatch:
            target = self.cell if self.cell is not None else self.out
            target.append("[#%s]" % zmatch.group(1))
        elif tag == "a" and attrs.get("href"):
            match = re.search(r"item[=/](\d+)", attrs["href"])
            if match:
                target = self.cell if self.cell is not None else self.out
                target.append("[#%s]" % match.group(1))
        if tag in ("p", "li", "br", "div"):
            self.out.append("\n")

    def handle_endtag(self, tag):
        if tag in ("script", "style", "noscript"):
            self.skip -= 1
        if tag in ("td", "th") and self.row is not None and self.cell is not None:
            self.row.append(" ".join("".join(self.cell).split()))
            self.cell = None
        if tag == "tr" and self.row is not None:
            self.out.append("\n| " + " | ".join(self.row) + "\n")
            self.row = None

    def handle_data(self, data):
        if self.skip:
            return
        if self.cell is not None:
            self.cell.append(data)
        else:
            self.out.append(data)


def main():
    parser = GuideText()
    with open(sys.argv[1], encoding="utf-8", errors="replace") as handle:
        parser.feed(handle.read())
    text = "".join(parser.out)
    text = re.sub(r"[ \t]+", " ", text)
    text = re.sub(r"\n\s*\n+", "\n", text)
    with open(sys.argv[2], "w", encoding="utf-8") as handle:
        handle.write(text)


if __name__ == "__main__":
    main()
