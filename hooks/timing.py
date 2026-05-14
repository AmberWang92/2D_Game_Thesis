#!/usr/bin/env python3

import sys
import json
import time
from pathlib import Path

HOOKS_DIR = Path("c:/Users/27492/Projects/2D_Game_Thesis/hooks")
STATE_FILE = HOOKS_DIR / "pending_prompts.json"
METRICS_FILE = HOOKS_DIR / "response-metrics.jsonl"
CODE_FILE = HOOKS_DIR / "code-metrics.jsonl"
READ_FILE = HOOKS_DIR / "read-metrics.jsonl"
COMMAND_FILE = HOOKS_DIR / "command-metrics.jsonl"
NOLOG_TAG = "#nolog"
PROMPT_TYPE_TAGS = [
    ("#context", "contextual_prompting"),
    ("#task", "task_prompt"),
    ("#debug", "debug_prompt"),
    ("#refine", "refinement_prompt"),
    ("#evaluation", "evaluation_prompt"),
]


def read_state():
    if STATE_FILE.exists():
        try:
            raw_state = json.loads(STATE_FILE.read_text())
            if "pending" in raw_state:
                return raw_state
            return {
                "pending": raw_state if isinstance(raw_state, dict) else {},
            }
        except (json.JSONDecodeError, IOError):
            return {"pending": {}}
    return {"pending": {}}


def write_state(state):
    STATE_FILE.write_text(json.dumps(state))


def is_nolog_prompt(entry):
    return bool(entry and entry.get("nolog"))


def get_prompt_type(prompt_text):
    prompt_lower = prompt_text.lower()
    for tag, prompt_type in PROMPT_TYPE_TAGS:
        if tag in prompt_lower:
            return prompt_type
    return "normal"


def get_pending_entry(state, trajectory_id, execution_id):
    pending = state.get("pending", {})
    return pending.get(trajectory_id) or pending.get(execution_id) or {}


def append_log(filepath, record):
    with open(filepath, "a", encoding="utf-8") as f:
        f.write(json.dumps(record) + "\n")


def count_lines(s):
    return len(s.strip().split("\n")) if s.strip() else 0


def main():
    input_data = sys.stdin.read()
    try:
        data = json.loads(input_data)
    except json.JSONDecodeError:
        sys.exit(1)

    action = data.get("agent_action_name", "")
    trajectory_id = data.get("trajectory_id", "unknown")
    execution_id = data.get("execution_id", "unknown")
    model_name = data.get("model_name", "unknown")
    timestamp = data.get("timestamp", "")
    now_ms = int(time.time() * 1000)
    tool_info = data.get("tool_info", {})

    # --- TIMING: pre_user_prompt ---
    if action == "pre_user_prompt":
        state = read_state()
        prompt_text = tool_info.get("user_prompt", "")
        prompt_is_nolog = NOLOG_TAG in prompt_text.lower()
        prompt_type = get_prompt_type(prompt_text)

        pending = state.get("pending", {})
        pending[trajectory_id] = {
            "started_at_ms": now_ms,
            "started_at": timestamp,
            "model": model_name,
            "prompt_preview": prompt_text[:120],
            "prompt_length": len(prompt_text),
            "nolog": prompt_is_nolog,
            "prompt_type": prompt_type,
        }
        if len(pending) > 100:
            oldest_keys = list(pending.keys())[:-100]
            for k in oldest_keys:
                del pending[k]
        state["pending"] = pending
        write_state(state)

    # --- TIMING: post_cascade_response ---
    elif action == "post_cascade_response":
        state = read_state()
        pending = state.get("pending", {})
        entry = pending.get(trajectory_id)
        if entry is None:
            entry = pending.get(execution_id)

        if is_nolog_prompt(entry):
            state["pending"] = pending
            write_state(state)
            return

        pending.pop(trajectory_id, None)
        pending.pop(execution_id, None)
        state["pending"] = pending
        write_state(state)

        response_text = tool_info.get("response", "")

        metric = {
            "trajectory_id": trajectory_id,
            "execution_id": execution_id,
            "model": model_name,
            "completed_at": timestamp,
            "response_length_chars": len(response_text),
        }

        if entry:
            metric["started_at"] = entry["started_at"]
            metric["total_ms"] = now_ms - entry["started_at_ms"]
            metric["prompt_preview"] = entry.get("prompt_preview", "")
            metric["prompt_length"] = entry.get("prompt_length", 0)
            metric["prompt_type"] = entry.get("prompt_type", "normal")

        append_log(METRICS_FILE, metric)

    # --- CODE METRICS: post_write_code ---
    elif action == "post_write_code":
        state = read_state()
        entry = get_pending_entry(state, trajectory_id, execution_id)
        if is_nolog_prompt(entry):
            return

        file_path = tool_info.get("file_path", "")
        edits = tool_info.get("edits", [])

        lines_added = 0
        lines_removed = 0

        for edit in edits:
            old = edit.get("old_string", "")
            new = edit.get("new_string", "")
            lines_removed += count_lines(old)
            lines_added += count_lines(new)

        ext = Path(file_path).suffix.lstrip(".")

        record = {
            "trajectory_id": trajectory_id,
            "execution_id": execution_id,
            "model": model_name,
            "timestamp": timestamp,
            "file_path": file_path,
            "language": ext,
            "lines_added": lines_added,
            "lines_removed": lines_removed,
            "net_lines": lines_added - lines_removed,
            "num_edits": len(edits),
            "prompt_type": entry.get("prompt_type", "normal"),
        }
        append_log(CODE_FILE, record)

    # --- READ METRICS: post_read_code ---
    elif action == "post_read_code":
        state = read_state()
        entry = get_pending_entry(state, trajectory_id, execution_id)
        if is_nolog_prompt(entry):
            return

        file_path = tool_info.get("file_path", "")
        ext = Path(file_path).suffix.lstrip(".") if file_path else ""

        record = {
            "trajectory_id": trajectory_id,
            "execution_id": execution_id,
            "model": model_name,
            "timestamp": timestamp,
            "file_path": file_path,
            "language": ext,
            "prompt_type": entry.get("prompt_type", "normal"),
        }
        append_log(READ_FILE, record)

    # --- COMMAND METRICS: post_run_command ---
    elif action == "post_run_command":
        state = read_state()
        entry = get_pending_entry(state, trajectory_id, execution_id)
        if is_nolog_prompt(entry):
            return

        command_line = tool_info.get("command_line", "")
        cwd = tool_info.get("cwd", "")

        record = {
            "trajectory_id": trajectory_id,
            "execution_id": execution_id,
            "model": model_name,
            "timestamp": timestamp,
            "command": command_line,
            "cwd": cwd,
            "prompt_type": entry.get("prompt_type", "normal"),
        }
        append_log(COMMAND_FILE, record)


if __name__ == "__main__":
    main()
