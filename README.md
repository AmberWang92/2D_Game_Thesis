# 2D_Game_Thesis

This repository pairs a Unity URP project with a small telemetry toolkit that logs
Windsurf activity while iterating on the game. The hooks (Python scripts inside
`/hooks`) capture every prompt/response cycle, code edit, file read, and shell
command so you can analyze how Cascade contributes to the thesis work.

## Repo layout (high level)

| Path | Purpose |
| ---- | ------- |
| `Assets/` | Unity scenes, scripts, and rendering assets for the 2D thesis game. |
| `hooks/` | Python utilities that Windsurf calls before/after prompts, code edits, reads, and commands. |
| `hooks/*.jsonl` | Append-only metric logs produced by the hook scripts. |

## What the hooks do

### `hooks/timing.py`

This is the primary entry point invoked by Windsurf. It reacts to four
`agent_action_name` values and writes JSONL records:

1. **`pre_user_prompt`** – caches metadata about an incoming prompt (model, start
   timestamp, preview text, prompt type, and whether `#nolog` was present).
2. **`post_cascade_response`** – finalizes a response entry by computing total
   latency, response length, prompt classification, and writing to
   `response-metrics.jsonl`.
3. **`post_write_code`** – aggregates added/removed lines per edit and logs them
   to `code-metrics.jsonl` along with language, file path, and prompt type.
4. **`post_read_code` / `post_run_command`** – captures each read or shell
   command issued by Cascade so you can count supporting actions per response.

Key features:

- **Prompt tags** – the script inspects the text of each user request for the
  tags listed below and stores the normalized `prompt_type` in every metric
  record:

  | Tag | Stored prompt type |
  | --- | ------------------ |
  | `#context` | `contextual_prompting` |
  | `#task` | `task_prompt` |
  | `#debug` | `debug_prompt` |
  | `#refine` | `refinement_prompt` |
  | `#evaluation` | `evaluation_prompt` |
  | *(none of the above)* | `normal` |

- **Opt-out switch** – including `#nolog` anywhere in the prompt suppresses all
  logging for that trajectory.

### `hooks/stats.py`

Runs a quick terminal summary over the JSONL files: total responses, latency
percentiles, models used, code line deltas per language, and recent edits.

```powershell
python hooks/stats.py
```

### `hooks/dashboard.py`

Serves an interactive Chart.js dashboard at `http://localhost:8787`. The page
renders:

- KPI cards (responses, latency, lines generated, files touched)
- Latency & code-volume charts per prompt
- Thesis prompt-type breakdowns using the tags above
- Model-to-model comparisons
- SVG “file lifecycle” graphs showing sequential edits per file
- Recent response table for quick inspection

Start it with:

```powershell
python hooks/dashboard.py
```

> Tip: Windsurf automatically opens a preview tab when the server starts.

## Working with the metrics

Each log entry is plain JSON, so you can ingest it with pandas, SQLite, or
anything else that understands JSON Lines. Typical workflow:

1. Reproduce a series of prompts or coding sessions in Windsurf.
2. Inspect progress live via the dashboard, or run `stats.py` for a terminal
   snapshot.
3. Use the prompt tags to segment data—for example, contrast `#task` prompts
   with `#debug` prompts to see how long different phases take.

This documentation should give you (or future collaborators) enough context to
extend the hooks or replace them with your own telemetry if needed.
