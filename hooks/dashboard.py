#!/usr/bin/env python3
"""Interactive HTML dashboard for Windsurf telemetry logs."""

import json
import http.server
import webbrowser
from pathlib import Path

HOOKS_DIR = Path(__file__).parent
METRICS_FILE = HOOKS_DIR / "response-metrics.jsonl"
CODE_FILE = HOOKS_DIR / "code-metrics.jsonl"
READ_FILE = HOOKS_DIR / "read-metrics.jsonl"
COMMAND_FILE = HOOKS_DIR / "command-metrics.jsonl"
PORT = 8787


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


def build_file_graphs(code, responses):
    """Build directed graph data per file, correlating edits with response timing."""
    response_by_exec = {r.get("execution_id"): r for r in responses}

    files = {}
    for edit in code:
        fp = edit.get("file_path", "")
        if fp not in files:
            files[fp] = []
        files[fp].append(edit)

    graphs = {}
    for fp, edits in files.items():
        edits_sorted = sorted(edits, key=lambda e: e.get("timestamp", ""))
        nodes = []
        edges = []
        for i, edit in enumerate(edits_sorted):
            exec_id = edit.get("execution_id", "")
            resp = response_by_exec.get(exec_id, {})
            total_ms = resp.get("total_ms")
            cumulative_lines = sum(e.get("net_lines", 0) for e in edits_sorted[:i+1])
            nodes.append({
                "id": i,
                "lines_added": edit.get("lines_added", 0),
                "lines_removed": edit.get("lines_removed", 0),
                "total_lines": cumulative_lines,
                "timestamp": edit.get("timestamp", ""),
                "model": edit.get("model", "unknown"),
            })
            if i > 0:
                edges.append({
                    "from": i - 1,
                    "to": i,
                    "label": f"{total_ms/1000:.1f}s" if total_ms else "?",
                })
        graphs[fp] = {"nodes": nodes, "edges": edges}

    return graphs


def build_model_comparison(responses, code, reads, commands):
    """Build per-model comparison stats."""
    models = {}
    for r in responses:
        m = r.get("model", "unknown")
        if m not in models:
            models[m] = {"responses": [], "total_ms": [], "prompt_lens": [], "response_lens": []}
        models[m]["responses"].append(r)
        if "total_ms" in r:
            models[m]["total_ms"].append(r["total_ms"])
        models[m]["prompt_lens"].append(r.get("prompt_length", 0))
        models[m]["response_lens"].append(r.get("response_length_chars", 0))

    code_by_exec = {}
    for c in code:
        eid = c.get("execution_id", "")
        if eid not in code_by_exec:
            code_by_exec[eid] = {"lines_added": 0, "writes": 0}
        code_by_exec[eid]["lines_added"] += c.get("lines_added", 0)
        code_by_exec[eid]["writes"] += 1

    reads_by_exec = {}
    for rd in reads:
        eid = rd.get("execution_id", "")
        reads_by_exec[eid] = reads_by_exec.get(eid, 0) + 1

    cmds_by_exec = {}
    for cmd in commands:
        eid = cmd.get("execution_id", "")
        cmds_by_exec[eid] = cmds_by_exec.get(eid, 0) + 1

    result = {}
    for m, data in models.items():
        times = data["total_ms"]
        n = len(data["responses"])
        total_lines = 0
        total_writes = 0
        total_reads = 0
        total_cmds = 0
        for r in data["responses"]:
            eid = r.get("execution_id", "")
            ci = code_by_exec.get(eid, {})
            total_lines += ci.get("lines_added", 0)
            total_writes += ci.get("writes", 0)
            total_reads += reads_by_exec.get(eid, 0)
            total_cmds += cmds_by_exec.get(eid, 0)

        avg_ms = sum(times) / len(times) if times else 0
        total_time_s = sum(times) / 1000 if times else 0
        avg_resp_len = sum(data["response_lens"]) / n if n else 0
        avg_prompt_len = sum(data["prompt_lens"]) / n if n else 0
        lines_per_sec = total_lines / total_time_s if total_time_s > 0 else 0
        output_ratio = avg_resp_len / avg_prompt_len if avg_prompt_len > 0 else 0
        reads_per_write = total_reads / total_writes if total_writes > 0 else 0
        actions_per_resp = (total_reads + total_writes + total_cmds) / n if n else 0

        result[m] = {
            "responses": n,
            "avg_latency_s": round(avg_ms / 1000, 1),
            "p50_latency_s": round(sorted(times)[len(times)//2] / 1000, 1) if times else 0,
            "lines_generated": total_lines,
            "lines_per_sec": round(lines_per_sec, 2),
            "output_ratio": round(output_ratio, 1),
            "reads_per_write": round(reads_per_write, 1),
            "actions_per_response": round(actions_per_resp, 1),
            "total_reads": total_reads,
            "total_writes": total_writes,
            "total_commands": total_cmds,
        }
    return result


def build_model_comparison_html(comparison):
    """Render model comparison as an HTML table + bar charts."""
    if not comparison:
        return '<p style="color:#64748b;">Not enough data yet.</p>'

    models = list(comparison.keys())
    rows = ""
    for m in models:
        d = comparison[m]
        rows += (f'<tr><td style="color:#38bdf8;font-weight:600">{m}</td>'
                 f'<td>{d["responses"]}</td>'
                 f'<td>{d["avg_latency_s"]}s</td>'
                 f'<td>{d["p50_latency_s"]}s</td>'
                 f'<td>{d["lines_generated"]}</td>'
                 f'<td>{d["lines_per_sec"]}</td>'
                 f'<td>{d["output_ratio"]}x</td>'
                 f'<td>{d["reads_per_write"]}</td>'
                 f'<td>{d["actions_per_response"]}</td></tr>')

    table = (f'<table><thead><tr>'
             f'<th>Model</th><th>Responses</th><th>Avg Latency</th><th>p50 Latency</th>'
             f'<th>Lines Generated</th><th>Lines/sec</th><th>Output Ratio</th>'
             f'<th>Reads/Write</th><th>Actions/Response</th>'
             f'</tr></thead><tbody>{rows}</tbody></table>')

    chart_data = json.dumps({"models": models, "data": {m: comparison[m] for m in models}})

    chart_html = f'''<div class="charts" style="margin-top:16px">
      <div class="chart-box"><h2>Avg Latency by Model (s)</h2><canvas id="modelLatencyChart"></canvas></div>
      <div class="chart-box"><h2>Lines/sec by Model</h2><canvas id="modelLinesChart"></canvas></div>
    </div>
    <script>
    (function() {{
      const cd = {chart_data};
      const models = cd.models;
      const colors = ['#38bdf8','#4ade80','#fbbf24','#f87171','#a78bfa','#fb923c'];

      new Chart(document.getElementById('modelLatencyChart'), {{
        type: 'bar',
        data: {{ labels: models, datasets: [{{
          label: 'Avg Latency (s)',
          data: models.map(m => cd.data[m].avg_latency_s),
          backgroundColor: colors.slice(0, models.length), borderRadius: 6
        }}] }},
        options: {{ responsive: true, plugins: {{ legend: {{ display: false }} }},
          scales: {{ x: {{ ticks: {{ color: '#94a3b8' }}, grid: {{ display: false }} }},
                    y: {{ grid: {{ color: '#334155' }}, ticks: {{ color: '#94a3b8' }} }} }} }}
      }});

      new Chart(document.getElementById('modelLinesChart'), {{
        type: 'bar',
        data: {{ labels: models, datasets: [{{
          label: 'Lines/sec',
          data: models.map(m => cd.data[m].lines_per_sec),
          backgroundColor: colors.slice(0, models.length), borderRadius: 6
        }}] }},
        options: {{ responsive: true, plugins: {{ legend: {{ display: false }} }},
          scales: {{ x: {{ ticks: {{ color: '#94a3b8' }}, grid: {{ display: false }} }},
                    y: {{ grid: {{ color: '#334155' }}, ticks: {{ color: '#94a3b8' }} }} }} }}
      }});
    }})()
    </script>'''

    return table + chart_html


def build_file_graph_svgs(graphs):
    """Render SVG directed graphs for each file's edit lifecycle."""
    if not graphs:
        return '<p style="color:#64748b;">No code edits recorded yet.</p>'

    html_parts = []
    for fp, graph in graphs.items():
        nodes = graph["nodes"]
        edges = graph["edges"]
        filename = Path(fp).name

        node_w = 140
        node_h = 60
        gap_x = 100
        padding = 20
        svg_w = padding * 2 + len(nodes) * node_w + max(0, len(nodes) - 1) * gap_x
        svg_h = padding * 2 + node_h + 40

        svg = f'<svg width="{svg_w}" height="{svg_h}" xmlns="http://www.w3.org/2000/svg">'
        svg += '''<defs><marker id="arrowhead" markerWidth="8" markerHeight="6" refX="8" refY="3" orient="auto">
          <polygon points="0 0, 8 3, 0 6" fill="#475569"/>
        </marker></defs>'''

        for i, node in enumerate(nodes):
            x = padding + i * (node_w + gap_x)
            y = padding + 20
            added = node["lines_added"]
            removed = node["lines_removed"]
            total = node["total_lines"]
            label_top = f"+{added} / -{removed}"
            label_bot = f"total: {total} lines"

            svg += f'<rect class="node-rect" x="{x}" y="{y}" width="{node_w}" height="{node_h}"/>'
            svg += f'<text class="node-text" x="{x + node_w//2}" y="{y + 22}" text-anchor="middle">Edit #{i+1}</text>'
            svg += f'<text class="node-text-small" x="{x + node_w//2}" y="{y + 37}" text-anchor="middle">{label_top}</text>'
            svg += f'<text class="node-text-small" x="{x + node_w//2}" y="{y + 50}" text-anchor="middle">{label_bot}</text>'

        for edge in edges:
            from_i = edge["from"]
            to_i = edge["to"]
            x1 = padding + from_i * (node_w + gap_x) + node_w
            x2 = padding + to_i * (node_w + gap_x)
            y_mid = padding + 20 + node_h // 2
            label = edge["label"]

            svg += f'<line class="edge-line" x1="{x1}" y1="{y_mid}" x2="{x2 - 4}" y2="{y_mid}"/>'
            label_x = (x1 + x2) // 2
            svg += f'<text class="edge-label" x="{label_x}" y="{y_mid - 8}" text-anchor="middle">{label}</text>'

        svg += '</svg>'

        html_parts.append(
            f'<div class="file-graph">'
            f'<h3>{filename} <span style="color:#64748b;font-weight:normal;">({fp})</span></h3>'
            f'<div class="graph-container">{svg}</div>'
            f'</div>'
        )

    return "\n".join(html_parts)


def build_thesis_metrics(responses, code):
    """Per-model thesis metrics aligned with the research data form."""
    PROMPT_TYPES = ["firstshot", "debug", "refinement", "normal"]
    by_model = {}
    for r in responses:
        m = r.get("model", "unknown")
        ptype = r.get("prompt_type", "normal")
        if ptype not in PROMPT_TYPES:
            ptype = "normal"
        if m not in by_model:
            by_model[m] = {
                "total_prompts": 0,
                "type_counts": {p: 0 for p in PROMPT_TYPES},
                "trajectories": {},
                "total_ms": 0,
                "milestones": [],
            }
        d = by_model[m]
        d["total_prompts"] += 1
        d["type_counts"][ptype] += 1
        d["total_ms"] += r.get("total_ms", 0)
        d["milestones"].append({
            "ms": r.get("total_ms", 0),
            "completed_at": r.get("completed_at", ""),
            "prompt_type": ptype,
        })

        traj = r.get("trajectory_id", "?")
        if traj not in d["trajectories"]:
            d["trajectories"][traj] = {"firstshot": 0, "debug": 0, "refinement": 0, "normal": 0}
        d["trajectories"][traj][ptype] += 1

    code_by_model = {}
    for c in code:
        m = c.get("model", "unknown")
        code_by_model[m] = code_by_model.get(m, 0) + c.get("lines_added", 0)

    result = {}
    for m, d in by_model.items():
        debug_cycles = [t["debug"] for t in d["trajectories"].values()]
        refinement_cycles = [t["refinement"] for t in d["trajectories"].values()]
        firstshot_trajs = sum(1 for t in d["trajectories"].values() if t["firstshot"] > 0)
        firstshot_only = sum(
            1 for t in d["trajectories"].values()
            if t["firstshot"] > 0 and t["debug"] == 0 and t["refinement"] == 0
        )
        first_shot_success_rate = (
            round(firstshot_only / firstshot_trajs * 100, 1) if firstshot_trajs else 0
        )
        avg_debug = round(sum(debug_cycles) / len(debug_cycles), 2) if debug_cycles else 0
        avg_refinement = round(sum(refinement_cycles) / len(refinement_cycles), 2) if refinement_cycles else 0

        d["milestones"].sort(key=lambda x: x["completed_at"])
        cumulative = []
        running = 0
        for ms in d["milestones"]:
            running += ms["ms"] / 60000.0
            cumulative.append(round(running, 2))

        result[m] = {
            "total_prompts": d["total_prompts"],
            "type_counts": d["type_counts"],
            "first_shot_attempts": firstshot_trajs,
            "first_shot_success": firstshot_only,
            "first_shot_success_rate": first_shot_success_rate,
            "avg_debug_cycles": avg_debug,
            "avg_refinement_cycles": avg_refinement,
            "total_time_min": round(d["total_ms"] / 60000.0, 2),
            "avg_time_min": round(d["total_ms"] / 60000.0 / d["total_prompts"], 2) if d["total_prompts"] else 0,
            "lines_generated": code_by_model.get(m, 0),
            "milestones_cumulative_min": cumulative,
        }
    return result


def build_thesis_section_html(thesis):
    if not thesis:
        return '<p style="color:#64748b;">No tagged prompts yet. Use #firstshot, #debug, or #refinement.</p>'

    models = list(thesis.keys())
    rows = ""
    for m in models:
        d = thesis[m]
        tc = d["type_counts"]
        rows += (f'<tr><td style="color:#38bdf8;font-weight:600">{m}</td>'
                 f'<td>{d["total_prompts"]}</td>'
                 f'<td>{tc["firstshot"]}</td>'
                 f'<td>{tc["debug"]}</td>'
                 f'<td>{tc["refinement"]}</td>'
                 f'<td>{tc["normal"]}</td>'
                 f'<td>{d["first_shot_success_rate"]}%</td>'
                 f'<td>{d["avg_debug_cycles"]}</td>'
                 f'<td>{d["avg_refinement_cycles"]}</td>'
                 f'<td>{d["total_time_min"]}m</td>'
                 f'<td>{d["lines_generated"]}</td></tr>')

    table = ('<table><thead><tr>'
             '<th>Model</th><th>Total Prompts</th><th>First-Shot</th><th>Debug</th>'
             '<th>Refinement</th><th>Normal</th><th>First-Shot Success</th>'
             '<th>Avg Debug Cycles</th><th>Avg Refinement Cycles</th>'
             '<th>Total Time</th><th>Lines Generated</th>'
             f'</tr></thead><tbody>{rows}</tbody></table>')

    payload = json.dumps({"models": models, "data": thesis})

    charts = f'''
    <div class="charts" style="margin-top:16px">
      <div class="chart-box"><h2>Total Prompts per Model</h2><canvas id="thesisTotalPrompts"></canvas></div>
      <div class="chart-box"><h2>Prompt Type Breakdown</h2><canvas id="thesisPromptTypes"></canvas></div>
    </div>
    <div class="charts">
      <div class="chart-box"><h2>First-Shot Success Rate (%)</h2><canvas id="thesisFirstShot"></canvas></div>
      <div class="chart-box"><h2>Avg Debug Cycles per Trajectory</h2><canvas id="thesisDebugCycles"></canvas></div>
    </div>
    <div class="charts">
      <div class="chart-box"><h2>Total Development Time (min)</h2><canvas id="thesisTotalTime"></canvas></div>
      <div class="chart-box"><h2>Time per Milestone (cumulative min)</h2><canvas id="thesisMilestones"></canvas></div>
    </div>
    <script>
    (function() {{
      const td = {payload};
      const models = td.models;
      const colors = ['#38bdf8','#4ade80','#fbbf24','#f87171','#a78bfa','#fb923c'];
      const axisOpts = {{
        x: {{ ticks: {{ color: '#94a3b8' }}, grid: {{ display: false }} }},
        y: {{ grid: {{ color: '#334155' }}, ticks: {{ color: '#94a3b8' }} }}
      }};

      new Chart(document.getElementById('thesisTotalPrompts'), {{
        type: 'bar',
        data: {{ labels: models, datasets: [{{
          label: 'Total Prompts',
          data: models.map(m => td.data[m].total_prompts),
          backgroundColor: colors.slice(0, models.length), borderRadius: 6
        }}] }},
        options: {{ responsive: true, plugins: {{ legend: {{ display: false }} }}, scales: axisOpts }}
      }});

      const types = ['firstshot','debug','refinement','normal'];
      const typeColors = {{ firstshot: '#4ade80', debug: '#f87171', refinement: '#fbbf24', normal: '#64748b' }};
      new Chart(document.getElementById('thesisPromptTypes'), {{
        type: 'bar',
        data: {{
          labels: models,
          datasets: types.map(t => ({{
            label: t, data: models.map(m => td.data[m].type_counts[t]),
            backgroundColor: typeColors[t], borderRadius: 4
          }}))
        }},
        options: {{ responsive: true, plugins: {{ legend: {{ position: 'top', labels: {{ color: '#f1f5f9' }} }} }},
          scales: {{ x: {{ stacked: true, ticks: {{ color: '#94a3b8' }}, grid: {{ display: false }} }},
                    y: {{ stacked: true, grid: {{ color: '#334155' }}, ticks: {{ color: '#94a3b8' }} }} }} }}
      }});

      new Chart(document.getElementById('thesisFirstShot'), {{
        type: 'bar',
        data: {{ labels: models, datasets: [{{
          label: 'First-Shot Success Rate (%)',
          data: models.map(m => td.data[m].first_shot_success_rate),
          backgroundColor: '#4ade80', borderRadius: 6
        }}] }},
        options: {{ responsive: true, plugins: {{ legend: {{ display: false }} }},
          scales: {{ x: axisOpts.x, y: {{ ...axisOpts.y, max: 100 }} }} }}
      }});

      new Chart(document.getElementById('thesisDebugCycles'), {{
        type: 'bar',
        data: {{ labels: models, datasets: [{{
          label: 'Avg Debug Cycles',
          data: models.map(m => td.data[m].avg_debug_cycles),
          backgroundColor: '#f87171', borderRadius: 6
        }}] }},
        options: {{ responsive: true, plugins: {{ legend: {{ display: false }} }}, scales: axisOpts }}
      }});

      new Chart(document.getElementById('thesisTotalTime'), {{
        type: 'bar',
        data: {{ labels: models, datasets: [{{
          label: 'Total Time (min)',
          data: models.map(m => td.data[m].total_time_min),
          backgroundColor: '#a78bfa', borderRadius: 6
        }}] }},
        options: {{ responsive: true, plugins: {{ legend: {{ display: false }} }}, scales: axisOpts }}
      }});

      const maxLen = Math.max(...models.map(m => td.data[m].milestones_cumulative_min.length), 0);
      const labels = Array.from({{ length: maxLen }}, (_, i) => 'Milestone ' + (i + 1));
      new Chart(document.getElementById('thesisMilestones'), {{
        type: 'line',
        data: {{
          labels: labels,
          datasets: models.map((m, i) => ({{
            label: m,
            data: td.data[m].milestones_cumulative_min,
            borderColor: colors[i % colors.length],
            backgroundColor: colors[i % colors.length] + '33',
            tension: 0.2,
            fill: false
          }}))
        }},
        options: {{ responsive: true, plugins: {{ legend: {{ position: 'top', labels: {{ color: '#f1f5f9' }} }} }},
          scales: axisOpts }}
      }});
    }})()
    </script>
    '''

    return table + charts


def build_html():
    responses = load_jsonl(METRICS_FILE)
    code = load_jsonl(CODE_FILE)
    reads = load_jsonl(READ_FILE)
    commands = load_jsonl(COMMAND_FILE)
    file_graphs = build_file_graphs(code, responses)
    model_comparison = build_model_comparison(responses, code, reads, commands)
    thesis_metrics = build_thesis_metrics(responses, code)

    return f"""<!DOCTYPE html>
<html lang="en">
<head>
<meta charset="UTF-8">
<title>Windsurf Analytics Dashboard</title>
<script src="https://cdn.jsdelivr.net/npm/chart.js@4"></script>
<style>
  * {{ margin: 0; padding: 0; box-sizing: border-box; }}
  body {{ font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif; background: #0f172a; color: #e2e8f0; padding: 24px; }}
  h1 {{ font-size: 1.8rem; margin-bottom: 8px; color: #38bdf8; }}
  .subtitle {{ color: #94a3b8; margin-bottom: 24px; }}
  .grid {{ display: grid; grid-template-columns: repeat(auto-fit, minmax(200px, 1fr)); gap: 16px; margin-bottom: 32px; }}
  .card {{ background: #1e293b; border-radius: 12px; padding: 20px; }}
  .card .label {{ font-size: 0.75rem; text-transform: uppercase; color: #64748b; letter-spacing: 0.05em; }}
  .card .value {{ font-size: 1.8rem; font-weight: 700; margin-top: 4px; color: #f1f5f9; }}
  .card .unit {{ font-size: 0.9rem; color: #94a3b8; }}
  .charts {{ display: grid; grid-template-columns: 1fr 1fr; gap: 24px; margin-bottom: 32px; }}
  .chart-box {{ background: #1e293b; border-radius: 12px; padding: 20px; }}
  .chart-box h2 {{ font-size: 1rem; color: #94a3b8; margin-bottom: 12px; }}
  canvas {{ max-height: 280px; }}
  table {{ width: 100%; border-collapse: collapse; font-size: 0.85rem; }}
  th {{ text-align: left; padding: 8px 12px; color: #64748b; border-bottom: 1px solid #334155; }}
  td {{ padding: 8px 12px; border-bottom: 1px solid #1e293b; }}
  tr:hover td {{ background: #1e293b; }}
  @media (max-width: 900px) {{ .charts {{ grid-template-columns: 1fr; }} }}
  .graph-section {{ margin-bottom: 32px; }}
  .graph-section h2 {{ font-size: 1.2rem; color: #38bdf8; margin-bottom: 16px; }}
  .file-graph {{ background: #1e293b; border-radius: 12px; padding: 20px; margin-bottom: 16px; }}
  .file-graph h3 {{ font-size: 0.85rem; color: #94a3b8; margin-bottom: 12px; font-family: monospace; }}
  .graph-container {{ overflow-x: auto; }}
  .graph-container svg {{ display: block; }}
  .node-rect {{ fill: #334155; stroke: #475569; stroke-width: 1.5; rx: 8; ry: 8; }}
  .node-rect:hover {{ fill: #3b4f6b; stroke: #38bdf8; }}
  .node-text {{ fill: #e2e8f0; font-size: 11px; font-family: -apple-system, sans-serif; }}
  .node-text-small {{ fill: #94a3b8; font-size: 10px; font-family: -apple-system, sans-serif; }}
  .edge-line {{ stroke: #475569; stroke-width: 2; marker-end: url(#arrowhead); }}
  .edge-label {{ fill: #fbbf24; font-size: 10px; font-weight: 600; font-family: -apple-system, sans-serif; }}
</style>
</head>
<body>
<h1>Windsurf Analytics</h1>
<p class="subtitle">Cascade response timing &amp; code generation metrics</p>

<div class="grid">
  <div class="card">
    <div class="label">Total Responses</div>
    <div class="value">{len(responses)}</div>
  </div>
  <div class="card">
    <div class="label">Avg Latency</div>
    <div class="value">{sum(r.get('total_ms',0) for r in responses) / max(len(responses),1) / 1000:.1f}<span class="unit">s</span></div>
  </div>
  <div class="card">
    <div class="label">Lines Generated</div>
    <div class="value">{sum(r.get('lines_added',0) for r in code)}</div>
  </div>
  <div class="card">
    <div class="label">Files Edited</div>
    <div class="value">{len(set(r.get('file_path','') for r in code))}</div>
  </div>
</div>

<div class="charts">
  <div class="chart-box">
    <h2>Response Latency (seconds)</h2>
    <canvas id="latencyChart"></canvas>
  </div>
  <div class="chart-box">
    <h2>Lines of Code per Edit</h2>
    <canvas id="codeChart"></canvas>
  </div>
</div>

<div class="chart-box" style="margin-bottom:32px">
  <h2>Thesis Metrics (per Model)</h2>
  <p style="color:#64748b;font-size:0.85rem;margin-bottom:12px">Tag prompts with #firstshot, #debug, or #refinement. Use #nolog to skip logging.</p>
  {build_thesis_section_html(thesis_metrics)}
</div>

<div class="chart-box" style="margin-bottom:32px">
  <h2>Model Performance Comparison</h2>
  {build_model_comparison_html(model_comparison)}
</div>

<div class="graph-section">
  <h2>File Lifecycle Graphs</h2>
  {build_file_graph_svgs(file_graphs)}
</div>

<div class="chart-box" style="margin-bottom:32px">
  <h2>Recent Responses</h2>
  <table>
    <thead><tr><th>Prompt</th><th>Prompt Size</th><th>Model</th><th>Latency</th><th>Response Size</th></tr></thead>
    <tbody>
      {''.join(f'<tr><td>{r.get("prompt_preview","")[:70]}</td><td>{r.get("prompt_length",0)} chars</td><td>{r.get("model","?")}</td><td>{r.get("total_ms",0)/1000:.1f}s</td><td>{r.get("response_length_chars",0):,} chars</td></tr>' for r in reversed(responses[-10:]))}
    </tbody>
  </table>
</div>

<script>
const latencyData = {json.dumps([r.get('total_ms',0)/1000 for r in responses])};
const latencyLabels = {json.dumps([r.get('prompt_preview','')[:30] for r in responses])};
const latencyModels = {json.dumps([r.get('model','unknown') for r in responses])};
const modelColors = ['#38bdf8','#4ade80','#fbbf24','#f87171','#a78bfa','#fb923c'];
const uniqueModels = [...new Set(latencyModels)];
const modelColorMap = {{}};
uniqueModels.forEach((m, i) => {{ modelColorMap[m] = modelColors[i % modelColors.length]; }});
const latencyColors = latencyModels.map(m => modelColorMap[m]);

new Chart(document.getElementById('latencyChart'), {{
  type: 'bar',
  data: {{
    labels: latencyLabels,
    datasets: [{{
      label: 'Seconds',
      data: latencyData,
      backgroundColor: latencyColors,
      borderRadius: 6,
    }}]
  }},
  options: {{
    responsive: true,
    plugins: {{ 
      legend: {{ 
        display: true,
        position: 'top',
        labels: {{
          color: '#f1f5f9',
          font: {{ size: 12 }},
          generateLabels: function(chart) {{
            return uniqueModels.map((model, i) => ({{
              text: model,
              fillStyle: modelColorMap[model],
              strokeStyle: modelColorMap[model],
              fontColor: '#f1f5f9',
              hidden: false,
              index: i
            }}));
          }}
        }}
      }} 
    }},
    scales: {{
      x: {{ ticks: {{ display: false }}, grid: {{ display: false }} }},
      y: {{ grid: {{ color: '#334155' }}, ticks: {{ color: '#94a3b8' }} }}
    }}
  }}
}});

const codeLabels = {json.dumps([Path(r.get('file_path','')).name for r in code])};
const codeAdded = {json.dumps([r.get('lines_added',0) for r in code])};
const codeRemoved = {json.dumps([r.get('lines_removed',0) for r in code])};

new Chart(document.getElementById('codeChart'), {{
  type: 'bar',
  data: {{
    labels: codeLabels,
    datasets: [
      {{ label: 'Added', data: codeAdded, backgroundColor: '#4ade80', borderRadius: 6 }},
      {{ label: 'Removed', data: codeRemoved, backgroundColor: '#f87171', borderRadius: 6 }}
    ]
  }},
  options: {{
    responsive: true,
    plugins: {{ legend: {{ position: 'top', labels: {{ color: '#f1f5f9' }} }} }},
    scales: {{
      x: {{ ticks: {{ color: '#94a3b8' }}, grid: {{ display: false }} }},
      y: {{ grid: {{ color: '#334155' }}, ticks: {{ color: '#94a3b8' }} }}
    }}
  }}
}});
</script>
</body>
</html>"""


class Handler(http.server.BaseHTTPRequestHandler):
    def do_GET(self):
        self.send_response(200)
        self.send_header("Content-Type", "text/html; charset=utf-8")
        self.end_headers()
        self.wfile.write(build_html().encode())

    def log_message(self, format, *args):
        pass


def main():
    print(f"Dashboard at http://localhost:{PORT}")
    print("Press Ctrl+C to stop.\n")
    webbrowser.open(f"http://localhost:{PORT}")
    server = http.server.HTTPServer(("127.0.0.1", PORT), Handler)
    try:
        server.serve_forever()
    except KeyboardInterrupt:
        print("\nStopped.")


if __name__ == "__main__":
    main()
