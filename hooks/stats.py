#!/usr/bin/env python3
"""Quick stats viewer for Windsurf telemetry logs."""

import json
from pathlib import Path

HOOKS_DIR = Path(__file__).parent
METRICS_FILE = HOOKS_DIR / "response-metrics.jsonl"
CODE_FILE = HOOKS_DIR / "code-metrics.jsonl"


def load_jsonl(filepath):
    records = []
    if not filepath.exists():
        return records
    for line in filepath.read_text().splitlines():
        line = line.strip()
        if line:
            try:
                records.append(json.loads(line))
            except json.JSONDecodeError:
                continue
    return records


def percentile(values, p):
    if not values:
        return 0
    s = sorted(values)
    idx = int(len(s) * p / 100)
    return s[min(idx, len(s) - 1)]


def print_response_stats(records):
    print("=" * 60)
    print("  RESPONSE METRICS")
    print("=" * 60)
    print(f"  Total responses: {len(records)}")

    times = [r["total_ms"] for r in records if "total_ms" in r]
    if times:
        print(f"\n  Latency (ms):")
        print(f"    Min:  {min(times):,.0f}")
        print(f"    p50:  {percentile(times, 50):,.0f}")
        print(f"    p90:  {percentile(times, 90):,.0f}")
        print(f"    p95:  {percentile(times, 95):,.0f}")
        print(f"    Max:  {max(times):,.0f}")
        print(f"    Avg:  {sum(times)/len(times):,.0f}")

    models = {}
    for r in records:
        m = r.get("model", "unknown")
        models[m] = models.get(m, 0) + 1
    if models:
        print(f"\n  Models used:")
        for m, count in sorted(models.items(), key=lambda x: -x[1]):
            print(f"    {m}: {count}")

    print(f"\n  Recent responses:")
    for r in records[-5:]:
        t = r.get("total_ms", "?")
        prompt = r.get("prompt_preview", "")[:60]
        print(f"    {t:>6}ms | {prompt}")

    print()


def print_code_stats(records):
    print("=" * 60)
    print("  CODE METRICS")
    print("=" * 60)
    print(f"  Total edits: {len(records)}")

    if not records:
        print("  (no code edits yet)")
        print()
        return

    total_added = sum(r.get("lines_added", 0) for r in records)
    total_removed = sum(r.get("lines_removed", 0) for r in records)
    net = total_added - total_removed

    print(f"\n  Lines added:   {total_added}")
    print(f"  Lines removed: {total_removed}")
    print(f"  Net lines:     {'+' if net >= 0 else ''}{net}")

    langs = {}
    for r in records:
        lang = r.get("language", "?") or "?"
        langs[lang] = langs.get(lang, 0) + r.get("lines_added", 0)
    if langs:
        print(f"\n  Lines added by language:")
        for lang, lines in sorted(langs.items(), key=lambda x: -x[1]):
            print(f"    .{lang}: {lines}")

    print(f"\n  Recent edits:")
    for r in records[-5:]:
        fp = Path(r.get("file_path", "?")).name
        added = r.get("lines_added", 0)
        removed = r.get("lines_removed", 0)
        print(f"    {fp}: +{added} -{removed}")

    print()


def main():
    print()
    responses = load_jsonl(METRICS_FILE)
    code = load_jsonl(CODE_FILE)

    print_response_stats(responses)
    print_code_stats(code)


if __name__ == "__main__":
    main()
