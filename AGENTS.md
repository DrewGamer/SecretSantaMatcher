# Secret Santa Matcher - Agent Registry & Guidelines

This project uses the modular Extreme Programming (XP) lifecycle workflow configured inside the `.agents/` directory, synchronized from [DrewGamer/agent-xp-workflow](https://github.com/DrewGamer/agent-xp-workflow).

## XP Workflow & Persona Setup
Development tasks and feature lifecycles are governed by the **XP Orchestrator** and its agentic personas and skills:

1. **xp-orchestrator** (`.agents/skills/xp-orchestrator/SKILL.md`): Primary lifecycle engine managing Phase 0 (Init) -> Phase 1 (Architecture & Approval) -> Phase 2 (XP Dev Loop & TDD) -> Phase 3 (Continuous Release Packaging) -> Phase 4 (Manual Testing) -> Phase 5 (PR & Release Gate).
2. **xp-architect** (`.agents/personas/xp-architect.md`): Systems Architect persona responsible for high-level structure, boundaries, and tech stack proposals.
3. **xp-developer** (`.agents/personas/xp-developer.md`): Senior Developer persona practicing pair programming, Test-Driven Development (TDD), small commits, and incremental delivery.
4. **Supporting Skills**:
   - `human-checkpoint`: Human-in-the-loop authorization gates for architecture approvals, package testing, and release creation.
   - `environment-manager`: Supervised tool acquisition and fallback escalation for missing compilers, libraries, and CLI tools.
   - `release-packager`: Deterministic build & package verification tool for continuous and release artifacts.
   - `genesis`: Agentic primitive design and architecture discipline.

Project state, active goals, constraints, backlog, and checkpoints are persisted in `.agents/plans/xp-state.md`.

## Engineering & Quality Standards
When implementing features or refactors, all personas must adhere to the engineering standards detailed in `.agents/plans/xp-state.md`:
- **Presentation (WPF)**: Slate-dark theme, fluid `*`/`Auto` grid sizing, caret focus retention on interactive elements, styles centralized in `App.xaml` or resource dictionaries.
- **Backend (C#)**: Separation of UI from core models/solvers, $O(1)$ pruning for backtracking algorithms, obfuscation/stripping of sensitive credentials, non-blocking `async/await` for SMTP and I/O.
- **Verification (xUnit)**: AAA test structure, sandboxed I/O (no real network emails or live disk overwrites; use fixtures/mocks), deterministic seed validation, circular chain and exclusion validation.

## Git Branching & Workflow Policy (CRITICAL)
To maintain project stability and track development cleanly, all AI agents must adhere to the following Git workflow strictly:

1. **Before Making Any Code Changes**:
   - Synchronize with the remote repository: Run `git fetch --all --prune` and pull the latest changes on `main` to ensure you are up to date.
   - Branch off `main`: Create and check out a new local branch named descriptively (prefixed with `feature/` or `bugfix/`, e.g., `feature/confirm-delete-clear` or `bugfix/fix-smtp-crash`).
   - **Never** perform work or commit directly to the `main` branch.

2. **During Development**:
   - Ensure the personas perform their work on the active feature branch.
   - Keep commits granular and structured. Use conventional commit messages (e.g. `feat: add clear list confirmation dialog`, `test: verify delete rejection flow`).

3. **Upon Completion**:
   - Verify the build and run all unit tests locally (`dotnet test`) to ensure everything is functioning correctly before pushing.
   - Stage and commit all modifications on the feature/bugfix branch.
   - Push the branch to the remote repository.
   - Create a pull request using GitHub CLI (`gh pr create`) with a detailed description.
