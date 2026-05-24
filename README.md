# AI-Assisted Unity Game Development Research

## Thesis Overview

This repository contains the implementation, experimental data, and research materials for a bachelor thesis focused on **AI-assisted game development using Large Language Models (LLMs)** in the Unity game engine.

The research investigates how different LLMs perform as programming assistants during the development of a small-scale Unity 2D Top-Down Shooter game. The study compares:

- Claude Opus 4.7
- GPT-5.5
- Gemini 3.1 Pro

The project evaluates:
- Code correctness
- First-shot success rate
- Debugging frequency
- Human implementation workload
- Architectural quality
- Prompt workflow efficiency

---

# Research Goal

This thesis explores whether **“Vibe Coding” workflows** can successfully support Unity game development and which LLM performs best under real production-like conditions.

The study focuses on:
- AI-assisted Unity development workflows
- Prompt engineering strategies
- Maintainable game architecture generation
- Human-AI collaboration efficiency

---

# Research Questions

1. Can “Vibe Coding” workflows successfully support Unity game development?
2. Which LLM is the most suitable for AI-assisted Unity game development?
3. What prompting strategies lead to more maintainable and scalable game code?

---

# Experimental Setup

## Development Environment

- Unity 2D
- C#
- Windsurf IDE
- Git + GitHub version control

## Compared Models

| Model | Reasoning Mode |
|---|---|
| Claude Opus 4.7 | Medium |
| GPT-5.5 | Low Thinking |
| Gemini 3.1 Pro | High Thinking |

---

# Prototype Scope

A small-scale **2D Top-Down Shooter** prototype was developed with three core gameplay milestones:

## Milestone 1 — Player Movement
- WASD movement
- Shooting system
- Physics setup

## Milestone 2 — Enemy AI
- Enemy movement
- Enemy attack behavior
- Chaser enemy logic

## Milestone 3 — Survival Loop
- Enemy spawning
- Health system
- UI system
- Survival gameplay loop

---

# Research Workflow

The experiment followed a strict AI-assisted workflow:

1. Same contextual prompt given to all models
2. Task prompts executed milestone-by-milestone
3. Manual implementation of AI instructions inside Unity
4. Debug prompts used only when issues occurred
5. Repeat until milestone completion

---

# No-Touch Policy

A strict **“No-Touch Policy”** was applied:

> Human developers were not allowed to manually modify source code.

All fixes, changes, and implementations had to be performed through AI prompting only.

This policy improved fairness and reduced hidden human intervention.

---

# Data Collection

The following data categories were collected:

## Interaction Metrics
- Total prompt count
- Prompt type breakdown
- Debug prompt frequency

## Performance Metrics
- First-shot success rate
- Compilation failures
- Runtime errors
- Error type categories

## Effort Metrics
- AI response time
- Manual implementation time
- Lines of code generated

---

# Key Findings

## Claude Opus 4.7
- Highest first-shot success rate
- Fewest debugging prompts
- Strongest Unity contextual understanding
- Most scalable architecture
- Largest code volume
- Highest manual implementation workload

## GPT-5.5
- Strong clean architecture tendencies
- Moderate implementation effort
- Balanced performance/workload trade-off
- Weaker Unity-specific engine awareness

## Gemini 3.1 Pro
- Fastest implementation workflow
- Smallest code volume
- Highest bug frequency
- Best suited for rapid prototyping

---

# Main Research Insight

> Higher code correctness does not necessarily lead to higher development efficiency.

Claude generated the most reliable code, but its large architecture significantly increased human implementation time.

Gemini generated less reliable code, but faster implementation due to smaller code volume.

GPT-5.5 provided a middle-ground balance between architecture quality and implementation effort.

---

# Ethical Considerations

This research:
- Did not involve personal data collection
- Followed academic transparency principles
- Clearly documented AI usage
- Included both successful and failed AI outputs
- Applied systematic evaluation methods

AI-generated code was always manually reviewed and tested inside Unity before evaluation.

---

# Sustainability

The research reflects sustainable development principles by exploring:
- Faster prototyping workflows
- Reduced repetitive programming tasks
- Lower technical barriers for small development teams

The study also acknowledges the computational cost of modern LLM systems.

---

# Technologies Used

## Game Development
- Unity
- C#
- Git
- GitHub

## Data Analysis
- Python
- Pandas
- Matplotlib

---

# Future Research

Possible future directions:
- Larger-scale Unity projects
- Multiplayer game development
- AI-assisted graphics programming
- Long-term maintainability studies
- Advanced prompt engineering workflows
- Multi-developer experimental environments

---

# Author

Bachelor Thesis Project  
Game Development / AI-Assisted Software Engineering  
Jamk University of Applied Sciences

---

# License

This repository is for academic and research purposes.
