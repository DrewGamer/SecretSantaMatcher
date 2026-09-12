# Secret Santa Matcher - Agentic Workflow & XP Orchestrator

This directory contains the skills, personas, scripts, and plans powering the **Extreme Programming (XP)** development lifecycle for the Secret Santa Matcher project, synchronized from [DrewGamer/agent-xp-workflow](https://github.com/DrewGamer/agent-xp-workflow).

---

## Directory Structure

```
.agents/
├── hooks.json            # Tool hooks and pre-execution safety gates
├── personas/             # Specialized agent personas (xp-architect, xp-developer)
├── plans/                # Active project state (xp-state.md) and task backlogs
├── scripts/              # Workflow synchronization and security gate scripts
└── skills/               # Reusable agentic skills (xp-orchestrator, release-packager, etc.)
```

---

## Key Personas & Skills

### Personas (`personas/`)
* **`xp-architect`**: Lead Systems Architect responsible for high-level structure, technical boundary setting, and creating actionable blueprints.
* **`xp-developer`**: Senior Developer practicing Test-Driven Development (TDD), pair programming, small reviewable commits, and incremental delivery.

### Skills (`skills/`)
* **`xp-orchestrator`**: The primary lifecycle engine guiding features through:
  1. Phase 0: Initialization & Branch Alignment
  2. Phase 1: Architecture & Design Approval
  3. Phase 2: XP Development Loop (TDD & Pair Programming)
  4. Phase 3: Continuous Release Packaging
  5. Phase 4: Manual Testing
  6. Phase 5: GitHub PR, Changelog & Release Gates
* **`human-checkpoint`**: Explicit gates requiring user approval for architecture decisions, package testing, and release creation.
* **`release-packager`**: Deterministic build & package verification tool for both continuous builds and final releases.
* **`environment-manager`**: Supervised dependency acquisition and fallback escalation.
* **`genesis`**: Agentic module design and benchmarking discipline.

---

## Project State (`plans/xp-state.md`)
The single source of truth for current active goals, constraints, backlog tasks, and engineering standards is persisted in:
👉 [`.agents/plans/xp-state.md`](plans/xp-state.md)

---

## Keeping Up to Date
To synchronize upstream updates to shared skills and personas from `DrewGamer/agent-xp-workflow`:
```powershell
powershell -ExecutionPolicy Bypass -File .agents/scripts/sync_workflow.ps1
```
*(This updates shared engine files while preserving local plans in `.agents/plans/` and custom hooks).*
