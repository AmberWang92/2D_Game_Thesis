#!/usr/bin/env python3
"""Interactive HTML dashboard for Windsurf telemetry logs."""

import json
import http.server
import webbrowser
from collections import Counter, defaultdict
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


ERROR_CATEGORIES = ["Logic Bug", "Physics/Movement", "UI/Display", "Config/Setup", "Runtime Error", "Other"]

def classify_debug_error(preview):
    p = (preview or "").lower()
    if any(k in p for k in ["panel", "gameover", "game over", " ui ", "display", "show", "visible", "menu", "button", "hud", "canvas", "screen"]):
        return "UI/Display"
    if any(k in p for k in ["trajectory", "movement", "direction", "position", "velocity", "aim", "rotate", "bullet", "physics", "move", "jump", "fly", "look"]):
        return "Physics/Movement"
    if any(k in p for k in ["set up", "setup", "configure", "instruction", "how to"]):
        return "Config/Setup"
    if any(k in p for k in ["error", "exception", "crash", "null ref", "console", "ignored", "tag", "missing"]):
        return "Runtime Error"
    if any(k in p for k in ["die", "damage", "health", "kill", "dead", "doesnt", "doesn't", "not work", "wrong", "incorrect", "fail", "cannot"]):
        return "Logic Bug"
    return "Other"


def build_thesis_metrics(responses, code):
    """Per-model thesis metrics aligned with the research data form."""
    PROMPT_TYPES = ["contextual_prompting", "task_prompt", "debug_prompt", "refinement_prompt", "evaluation_prompt", "normal"]
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
                "debug_errors": {c: 0 for c in ERROR_CATEGORIES},
            }
        d = by_model[m]
        d["total_prompts"] += 1
        d["type_counts"][ptype] += 1
        d["total_ms"] += r.get("total_ms", 0)
        if ptype == "debug_prompt":
            d["debug_errors"][classify_debug_error(r.get("prompt_preview", ""))] += 1
        d["milestones"].append({
            "ms": r.get("total_ms", 0),
            "completed_at": r.get("completed_at", ""),
            "prompt_type": ptype,
        })

        traj = r.get("trajectory_id", "?")
        if traj not in d["trajectories"]:
            d["trajectories"][traj] = {p: 0 for p in PROMPT_TYPES}
        d["trajectories"][traj][ptype] += 1

    code_by_model = {}
    for c in code:
        m = c.get("model", "unknown")
        code_by_model[m] = code_by_model.get(m, 0) + c.get("lines_added", 0)

    result = {}
    for m, d in by_model.items():
        debug_cycles = [t["debug_prompt"] for t in d["trajectories"].values()]
        refinement_cycles = [t["refinement_prompt"] for t in d["trajectories"].values()]
        contextual_trajs = sum(1 for t in d["trajectories"].values() if t["contextual_prompting"] > 0)
        task_trajs = sum(1 for t in d["trajectories"].values() if t["task_prompt"] > 0)
        firstshot_trajs = contextual_trajs + task_trajs
        firstshot_only = sum(
            1 for t in d["trajectories"].values()
            if (t["contextual_prompting"] > 0 or t["task_prompt"] > 0) and t["debug_prompt"] == 0 and t["refinement_prompt"] == 0
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
            "error_type_counts": d["debug_errors"],
        }
    return result


def build_thesis_section_html(thesis):
    if not thesis:
        return '<p style="color:#64748b;">No tagged prompts yet. Use #context, #task, #debug, #refine, or #evaluation.</p>'

    models = list(thesis.keys())
    rows = ""
    for m in models:
        d = thesis[m]
        tc = d["type_counts"]
        rows += (f'<tr><td style="color:#38bdf8;font-weight:600">{m}</td>'
                 f'<td>{d["total_prompts"]}</td>'
                 f'<td>{tc["contextual_prompting"]}</td>'
                 f'<td>{tc["task_prompt"]}</td>'
                 f'<td>{tc["debug_prompt"]}</td>'
                 f'<td>{tc["refinement_prompt"]}</td>'
                 f'<td>{tc["evaluation_prompt"]}</td>'
                 f'<td>{tc["normal"]}</td>'
                 f'<td>{d["first_shot_success_rate"]}%</td>'
                 f'<td>{d["avg_debug_cycles"]}</td>'
                 f'<td>{d["avg_refinement_cycles"]}</td>'
                 f'<td>{d["total_time_min"]}m</td>'
                 f'<td>{d["lines_generated"]}</td></tr>')

    table = ('<table><thead><tr>'
             '<th>Model</th><th>Total</th><th>Context</th><th>Task</th>'
             '<th>Debug</th><th>Refine</th><th>Eval</th><th>Normal</th>'
             '<th>First-Shot Success</th><th>Avg Debug</th><th>Avg Refine</th>'
             '<th>Time</th><th>Lines</th>'
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
    <div class="charts">
      <div class="chart-box" style="grid-column:1/-1"><h2>Lines Generated per Model</h2><canvas id="thesisLinesGenerated" style="max-height:260px"></canvas></div>
    </div>
    <div class="charts">
      <div class="chart-box" style="grid-column:1/-1"><h2>Error / Bug Type Breakdown per Model</h2><canvas id="thesisErrorTypes" style="max-height:320px"></canvas></div>
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

      const types = ['contextual_prompting','task_prompt','debug_prompt','refinement_prompt','evaluation_prompt','normal'];
      const typeColors = {{ contextual_prompting: '#4ade80', task_prompt: '#38bdf8', debug_prompt: '#f87171', refinement_prompt: '#fbbf24', evaluation_prompt: '#a78bfa', normal: '#64748b' }};
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

      const _manualTimes = {{ gemini: 35.5, gpt: 48.67, claude: 98.63 }};
      function getManualTime(model) {{
        const ml = model.toLowerCase();
        for (const [key, val] of Object.entries(_manualTimes)) {{
          if (ml.includes(key)) return val;
        }}
        return null;
      }}
      new Chart(document.getElementById('thesisTotalTime'), {{
        type: 'bar',
        data: {{
          labels: models,
          datasets: [
            {{
              label: 'AI Response Time (min)',
              data: models.map(m => td.data[m].total_time_min),
              backgroundColor: '#a78bfa', borderRadius: 6
            }},
            {{
              label: 'Manual Implementation Time (min)',
              data: models.map(m => getManualTime(m)),
              backgroundColor: '#fb923c', borderRadius: 6
            }}
          ]
        }},
        options: {{
          responsive: true,
          plugins: {{ legend: {{ position: 'top', labels: {{ color: '#f1f5f9' }} }} }},
          scales: {{
            x: {{ stacked: true, ticks: {{ color: '#94a3b8' }}, grid: {{ display: false }} }},
            y: {{ stacked: true, grid: {{ color: '#334155' }}, ticks: {{ color: '#94a3b8' }} }}
          }}
        }}
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

      new Chart(document.getElementById('thesisLinesGenerated'), {{
        type: 'bar',
        data: {{
          labels: models,
          datasets: [{{
            label: 'Lines Generated',
            data: models.map(m => td.data[m].lines_generated),
            backgroundColor: colors.slice(0, models.length),
            borderRadius: 6
          }}]
        }},
        options: {{
          responsive: true,
          plugins: {{ legend: {{ display: false }} }},
          scales: axisOpts
        }}
      }});

      const errorCats = ['Logic Bug','Physics/Movement','UI/Display','Config/Setup','Runtime Error','Other'];
      const errorColors = {{
        'Logic Bug': '#f87171', 'Physics/Movement': '#38bdf8', 'UI/Display': '#4ade80',
        'Config/Setup': '#fbbf24', 'Runtime Error': '#fb923c', 'Other': '#64748b'
      }};
      new Chart(document.getElementById('thesisErrorTypes'), {{
        type: 'bar',
        data: {{
          labels: models,
          datasets: errorCats.map(cat => ({{
            label: cat,
            data: models.map(m => ((td.data[m].error_type_counts || {{}})[cat] || 0)),
            backgroundColor: errorColors[cat],
            borderRadius: 4
          }}))
        }},
        options: {{
          responsive: true,
          plugins: {{ legend: {{ position: 'top', labels: {{ color: '#f1f5f9' }} }} }},
          scales: {{
            x: {{ stacked: true, ticks: {{ color: '#94a3b8' }}, grid: {{ display: false }} }},
            y: {{ stacked: true, grid: {{ color: '#334155' }}, ticks: {{ color: '#94a3b8' }} }}
          }}
        }}
      }});
    }})()
    </script>
    '''

    return table + charts


def build_data_visualization_html(responses, code, reads, commands):
    total_actions = len(responses) + len(code) + len(reads) + len(commands)
    if total_actions == 0:
        return '<p style="color:#64748b;">No telemetry data yet.</p>'

    prompt_types = Counter(r.get("prompt_type", "normal") for r in responses)
    languages = Counter((r.get("language") or "directory") for r in code + reads)
    action_counts = {
        "Responses": len(responses),
        "Code Edits": len(code),
        "Reads": len(reads),
        "Commands": len(commands),
    }

    daily = defaultdict(lambda: {"responses": 0, "code": 0, "reads": 0, "commands": 0})
    for r in responses:
        day = (r.get("completed_at") or r.get("timestamp") or "")[:10] or "unknown"
        daily[day]["responses"] += 1
    for r in code:
        day = (r.get("timestamp") or "")[:10] or "unknown"
        daily[day]["code"] += 1
    for r in reads:
        day = (r.get("timestamp") or r.get("completed_at") or "")[:10] or "unknown"
        daily[day]["reads"] += 1
    for r in commands:
        day = (r.get("timestamp") or r.get("completed_at") or "")[:10] or "unknown"
        daily[day]["commands"] += 1

    top_files = Counter()
    for r in code:
        fp = r.get("file_path", "")
        if fp:
            top_files[fp] += r.get("lines_added", 0) + r.get("lines_removed", 0)

    payload = json.dumps({
        "prompt_types": dict(prompt_types),
        "languages": dict(languages.most_common(8)),
        "actions": action_counts,
        "daily_labels": sorted(daily.keys()),
        "daily": {k: daily[k] for k in sorted(daily.keys())},
        "top_files": [{"file": Path(k).name, "changes": v} for k, v in top_files.most_common(10)],
    })

    return f'''
    <div class="charts" style="margin-top:16px">
      <div class="chart-box"><h2>Activity by Day</h2><canvas id="dataActivityChart"></canvas></div>
      <div class="chart-box"><h2>Telemetry Mix</h2><canvas id="dataActionsChart"></canvas></div>
    </div>
    <div class="charts">
      <div class="chart-box"><h2>Prompt Types</h2><canvas id="dataPromptTypesChart"></canvas></div>
      <div class="chart-box"><h2>Languages / Read Targets</h2><canvas id="dataLanguagesChart"></canvas></div>
    </div>
    <div class="chart-box" style="margin-top:-16px"><h2>Most Changed Files</h2><canvas id="dataTopFilesChart"></canvas></div>
    <script>
    (function() {{
      const vd = {payload};
      const colors = ['#38bdf8','#4ade80','#fbbf24','#f87171','#a78bfa','#fb923c','#22d3ee','#c084fc'];
      const axisOpts = {{
        x: {{ ticks: {{ color: '#94a3b8' }}, grid: {{ display: false }} }},
        y: {{ grid: {{ color: '#334155' }}, ticks: {{ color: '#94a3b8' }} }}
      }};

      new Chart(document.getElementById('dataActivityChart'), {{
        type: 'line',
        data: {{
          labels: vd.daily_labels,
          datasets: ['responses','code','reads','commands'].map((k, i) => ({{
            label: k,
            data: vd.daily_labels.map(day => vd.daily[day][k]),
            borderColor: colors[i],
            backgroundColor: colors[i] + '33',
            tension: 0.25,
            fill: false
          }}))
        }},
        options: {{ responsive: true, plugins: {{ legend: {{ position: 'top', labels: {{ color: '#f1f5f9' }} }} }}, scales: axisOpts }}
      }});

      new Chart(document.getElementById('dataActionsChart'), {{
        type: 'doughnut',
        data: {{ labels: Object.keys(vd.actions), datasets: [{{ data: Object.values(vd.actions), backgroundColor: colors }}] }},
        options: {{ responsive: true, plugins: {{ legend: {{ position: 'right', labels: {{ color: '#f1f5f9' }} }} }} }}
      }});

      new Chart(document.getElementById('dataPromptTypesChart'), {{
        type: 'bar',
        data: {{ labels: Object.keys(vd.prompt_types), datasets: [{{ data: Object.values(vd.prompt_types), backgroundColor: colors, borderRadius: 6 }}] }},
        options: {{ responsive: true, plugins: {{ legend: {{ display: false }} }}, scales: axisOpts }}
      }});

      new Chart(document.getElementById('dataLanguagesChart'), {{
        type: 'bar',
        data: {{ labels: Object.keys(vd.languages), datasets: [{{ data: Object.values(vd.languages), backgroundColor: colors, borderRadius: 6 }}] }},
        options: {{ responsive: true, plugins: {{ legend: {{ display: false }} }}, scales: axisOpts }}
      }});

      new Chart(document.getElementById('dataTopFilesChart'), {{
        type: 'bar',
        data: {{ labels: vd.top_files.map(x => x.file), datasets: [{{ label: 'Changed Lines', data: vd.top_files.map(x => x.changes), backgroundColor: '#38bdf8', borderRadius: 6 }}] }},
        options: {{ indexAxis: 'y', responsive: true, plugins: {{ legend: {{ display: false }} }}, scales: axisOpts }}
      }});
    }})()
    </script>
    '''


def build_html():
    responses = load_jsonl(METRICS_FILE)
    code = load_jsonl(CODE_FILE)
    reads = load_jsonl(READ_FILE)
    commands = load_jsonl(COMMAND_FILE)
    file_graphs = build_file_graphs(code, responses)
    model_comparison = build_model_comparison(responses, code, reads, commands)
    thesis_metrics = build_thesis_metrics(responses, code)
    data_visualization = build_data_visualization_html(responses, code, reads, commands)

    return f"""<!DOCTYPE html>
<html lang="en">
<head>
<meta charset="UTF-8">
<title>Windsurf Analytics Dashboard</title>
<script src="https://cdn.jsdelivr.net/npm/chart.js@4"></script>
<style>
  * {{ margin: 0; padding: 0; box-sizing: border-box; }}
  :root {{ --bg:#0f172a; --panel:#1e293b; --panel-hover:#334155; --text:#e2e8f0; --strong:#f1f5f9; --muted:#94a3b8; --faint:#64748b; --border:#334155; --accent:#38bdf8; --node-fill:#334155; --node-stroke:#475569; --node-text:#e2e8f0; --edge:#475569; }}
  body.light {{ --bg:#f1f5f9; --panel:#ffffff; --panel-hover:#e2e8f0; --text:#1e293b; --strong:#0f172a; --muted:#475569; --faint:#52616e; --border:#cbd5e1; --accent:#0284c7; --node-fill:#dde3ea; --node-stroke:#94a3b8; --node-text:#1e293b; --edge:#94a3b8; }}
  body {{ font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif; background: var(--bg); color: var(--text); padding: 24px; transition: background .25s, color .25s; }}
  h1 {{ font-size: 1.8rem; margin-bottom: 8px; color: var(--accent); }}
  .subtitle {{ color: var(--muted); margin-bottom: 24px; }}
  .theme-toggle {{ position:fixed; top:16px; right:20px; z-index:50; display:inline-flex; align-items:center; gap:8px; background:var(--panel); color:var(--text); border:1.5px solid var(--border); border-radius:999px; padding:7px 16px 7px 12px; cursor:pointer; box-shadow:0 4px 20px rgba(0,0,0,.3); font-size:.85rem; font-weight:600; transition:background .25s, border-color .25s, color .25s, box-shadow .25s; }}
  .theme-toggle:hover {{ border-color:var(--accent); box-shadow:0 4px 20px rgba(0,0,0,.45); }}
  .icon-moon {{ display:inline-flex; align-items:center; color:#a78bfa; }}
  .icon-sun {{ display:none; align-items:center; color:#fbbf24; }}
  body.light .icon-moon {{ display:none; }}
  body.light .icon-sun {{ display:inline-flex; }}
  .grid {{ display: grid; grid-template-columns: repeat(auto-fit, minmax(200px, 1fr)); gap: 16px; margin-bottom: 32px; }}
  .card {{ background: var(--panel); border-radius: 12px; padding: 20px; }}
  .card .label {{ font-size: 0.75rem; text-transform: uppercase; color: var(--faint); letter-spacing: 0.05em; }}
  .card .value {{ font-size: 1.8rem; font-weight: 700; margin-top: 4px; color: var(--strong); }}
  .card .unit {{ font-size: 0.9rem; color: var(--muted); }}
  .charts {{ display: grid; grid-template-columns: 1fr 1fr; gap: 24px; margin-bottom: 32px; }}
  .chart-box {{ background: var(--panel); border-radius: 12px; padding: 20px; }}
  .chart-box h2 {{ font-size: 1rem; color: var(--muted); margin-bottom: 12px; }}
  canvas {{ max-height: 280px; }}
  table {{ width: 100%; border-collapse: collapse; font-size: 0.85rem; }}
  th {{ text-align: left; padding: 8px 12px; color: var(--faint); border-bottom: 1px solid var(--border); }}
  td {{ padding: 8px 12px; border-bottom: 1px solid var(--border); color: var(--text); }}
  tr:hover td {{ background: var(--panel-hover); }}
  @media (max-width: 900px) {{ .charts {{ grid-template-columns: 1fr; }} }}
  .graph-section {{ margin-bottom: 32px; }}
  .graph-section h2 {{ font-size: 1.2rem; color: var(--accent); margin-bottom: 16px; }}
  .file-graph {{ background: var(--panel); border-radius: 12px; padding: 20px; margin-bottom: 16px; }}
  .file-graph h3 {{ font-size: 0.85rem; color: var(--muted); margin-bottom: 12px; font-family: monospace; }}
  .graph-container {{ overflow-x: auto; }}
  .graph-container svg {{ display: block; }}
  .node-rect {{ fill: var(--node-fill); stroke: var(--node-stroke); stroke-width: 1.5; rx: 8; ry: 8; }}
  .node-rect:hover {{ fill: var(--panel-hover); stroke: var(--accent); }}
  .node-text {{ fill: var(--node-text); font-size: 11px; font-family: -apple-system, sans-serif; }}
  .node-text-small {{ fill: var(--muted); font-size: 10px; font-family: -apple-system, sans-serif; }}
  .edge-line {{ stroke: var(--edge); stroke-width: 2; marker-end: url(#arrowhead); }}
  .edge-label {{ fill: #d97706; font-size: 10px; font-weight: 600; font-family: -apple-system, sans-serif; }}
</style>
</head>
<body>
<button id="themeToggle" class="theme-toggle" type="button" aria-label="Toggle colour theme">
  <span class="icon-moon"><svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="currentColor"><path d="M21 12.79A9 9 0 1 1 11.21 3 7 7 0 0 0 21 12.79z"/></svg></span>
  <span class="icon-sun"><svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round"><circle cx="12" cy="12" r="4"/><path d="M12 2v2M12 20v2M4.93 4.93l1.41 1.41M17.66 17.66l1.41 1.41M2 12h2M20 12h2M4.93 19.07l1.41-1.41M17.66 6.34l1.41-1.41"/></svg></span>
  <span id="themeLabel">Dark</span>
</button>
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
  <h2>Data Visualization Overview</h2>
  {data_visualization}
</div>

<div class="chart-box" style="margin-bottom:32px">
  <h2>Thesis Metrics (per Model)</h2>
  <p style="color:#64748b;font-size:0.85rem;margin-bottom:12px">Tag prompts with #context, #task, #debug, #refine, or #evaluation. Use #nolog to skip logging.</p>
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
const _isLight = () => document.body.classList.contains('light');
function applyChartTheme() {{
  const tc = _isLight() ? '#334155' : '#94a3b8';
  const gc = _isLight() ? '#e2e8f0' : '#334155';
  const lc = _isLight() ? '#1e293b' : '#f1f5f9';
  Object.values(Chart.instances).forEach(chart => {{
    if (chart.options.scales) {{
      ['x','y'].forEach(axis => {{
        const s = chart.options.scales[axis];
        if (!s) return;
        if (s.ticks) s.ticks.color = tc;
        if (s.grid) s.grid.color = gc;
      }});
    }}
    if (chart.options.plugins && chart.options.plugins.legend && chart.options.plugins.legend.labels)
      chart.options.plugins.legend.labels.color = lc;
    chart.update('none');
  }});
}}
(function() {{
  const saved = localStorage.getItem('ws-theme') || 'dark';
  if (saved === 'light') document.body.classList.add('light');
  document.getElementById('themeLabel').textContent = _isLight() ? 'Light' : 'Dark';
  document.getElementById('themeToggle').addEventListener('click', () => {{
    document.body.classList.toggle('light');
    document.getElementById('themeLabel').textContent = _isLight() ? 'Light' : 'Dark';
    localStorage.setItem('ws-theme', _isLight() ? 'light' : 'dark');
    applyChartTheme();
  }});
}})();

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
if (_isLight()) applyChartTheme();
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
