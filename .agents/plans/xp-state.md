# Project XP State (B4 Plan Memento)

## 1. Project Context
**Project Name:** Secret Santa Matcher
**Project Type:** Application
**Current Stage:** Development
**Primary Tech Stack:** C# (.NET 10.0-windows, WPF), xUnit

## 2. Active Goal & Constraints (B8 Attention Anchor)
**Current Objective:** Update SecretSantaMatcher to allow duplicate emails with participants, presenting a warning dialog detailing the email address and who it is attached to, prompting to continue anyway or go back and change it.
**Hard Constraints:** 
- MUST pass human checkpoint for architecture approval.
- MUST pass human checkpoint for PR reviews.
- MUST use environment-manager for new dependencies.
- Adhere strictly to the Git Branching & Workflow Policy in AGENTS.md (never commit directly to main; work on descriptive feature/bugfix branches; verify build and run `dotnet test` before push).
- Adhere to project-specific engineering standards (below).

### Project Engineering Standards
1. **Presentation & UI/UX (WPF)**:
   - Adhere strictly to the slate-dark theme, curated HSL-tailored colors, and modern typography (Segoe UI / Inter).
   - Maintain fluid geometry using proportional (`*`) and content-driven (`Auto`) row/column sizing; avoid hardcoded pixel heights/widths that clip on varying DPIs.
   - Interactive tokens and non-input buttons must specify `Focusable="False"` or preserve caret focus in the target TextBox to prevent input disruption.
   - Centralize reusable styles and templates in `App.xaml` or dedicated resource dictionaries instead of inline element styling.
2. **Backend & Domain Logic (C#)**:
   - Strict separation of concerns: Models, solvers, and serialization services must never reference WPF presentation controls.
   - Constraint-based backtracking solvers must prune invalid states early with $O(1)$ lookups to eliminate exponential slowdowns.
   - Sensitive credentials (passwords, API tokens) must be obfuscated at rest and stripped automatically during export.
   - All network and disk I/O (e.g. SMTP operations, session serialization) must be non-blocking (`async/await`).
3. **Quality Assurance & Verification (xUnit)**:
   - Follow Arrange-Act-Assert (AAA) conventions for test cases.
   - Sandboxed I/O: Never perform real SMTP transmissions or overwrite active disk state during test runs; use mocks, test doubles, and sandbox fixtures (e.g. `SessionBackupFixture`).
   - Deterministic verification: Seed or isolate random shuffling in matching tests to guarantee reproducibility.
   - Mathematical constraint validation: Strictly verify circular chains, symmetric/asymmetric exclusion adherence, and mirror-match prevention.

## 3. Architecture & Tooling
**Approved Architecture:**
Allow duplicate emails for participants with a confirmation warning dialog in `AddParticipant_Click` via `MessageBoxShowHandler(..., "Duplicate Email Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning)`. When Yes is chosen, participant is added/saved; when No is chosen, inputs are preserved and email field is focused. Detailed in `.agents/plans/duplicate-email-architecture.md`.
**Dependencies / Frameworks / Tools:**
- .NET 10 SDK (`dotnet`)
- Git CLI (`git`)
- GitHub CLI (`gh`)
**Build / Packaging Command:** `dotnet publish SecretSantaMatcher.csproj -c Release -o bin/Release/publish`
**Verification / Test Command:** `dotnet test`

## 4. Work Backlog (B7 Todo Commands)
| ID | Title | Status | Assigned Persona | Dependencies |
|---|---|---|---|---|
| T1 | Duplicate Email Allowance Architecture Blueprint | completed | xp-architect | - |
| T2 | Implement Duplicate Email Warning Confirmation Dialog in MainWindow | completed | xp-developer | T1 |
| T3 | Unit & UI Transition Tests for Duplicate Email Scenarios | completed | xp-developer | T2 |
| T4 | Verification & Continuous Packaging | pending | xp-orchestrator | T3 |

## 5. Sub-Agent Coordination
- Personas: `xp-architect` (system architecture and boundaries), `xp-developer` (TDD, pair programming, incremental delivery).
- Core skills: `xp-orchestrator`, `environment-manager`, `human-checkpoint`, `release-packager`.
- Active working branch: `feature/allow-duplicate-emails`.

## 6. Checkpoints & History
- [x] Architecture Approved (human approval)
- [ ] PR 1 Reviewed & Approved
- [ ] Release Package Generated

## 7. Release Configuration
**Continuous Release Tag:** continuous-build
**Continuous Release Name:** Continuous Build
**Target Output Artifact:** bin/Release/publish/*
**Build Type Override:** 
