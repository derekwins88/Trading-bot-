import json
import os
from pathlib import Path

REPO_ROOT = Path(__file__).resolve().parents[3]
CONFIG_DIR = REPO_ROOT / 'config'
DOCS_DIR = REPO_ROOT / 'docs'

IDS_FILE = CONFIG_DIR / 'axiom_ids.json'
EMOJI_FILE = CONFIG_DIR / 'axiom_emoji_map.json'
OUTPUT_FILE = DOCS_DIR / 'axiom_badges.md'


def load_json(path):
    with open(path, 'r') as f:
        return json.load(f)


def generate_table(ids, emoji_map):
    rows = ["| Axiom | Emoji |", "|-------|-------|"]
    for axiom in ids:
        emoji = emoji_map.get(axiom, '')
        rows.append(f"| {axiom} | {emoji} |")
    return '\n'.join(rows) + '\n'


def main():
    ids = load_json(IDS_FILE)
    emoji_map = load_json(EMOJI_FILE)
    DOCS_DIR.mkdir(parents=True, exist_ok=True)
    table = generate_table(ids, emoji_map)
    OUTPUT_FILE.write_text(table)


if __name__ == '__main__':
    main()
