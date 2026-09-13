# Project XP State (B4 Plan Memento)

## 1. Project Context
**Project Name:** Secret Santa Matcher
**Project Type:** Application
**Current Stage:** Development
**Primary Tech Stack:** C# (.NET 10.0-windows, WPF), xUnit

## 2. Active Goal & Constraints (B8 Attention Anchor)
**Current Objective:** Configure release packaging to produce a truly standalone, self-contained single .exe (SecretSantaMatcher.exe) for win-x64 with zero external dependencies and zero loose companion files.
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
Standalone single-file executable packaging (Option A) with embedded CLR runtime and WPF native binaries, Deflate compression, embedded debug symbols, and isolated dev loop. Detailed in `.agents/plans/single-file-executable-architecture.md`.
**Dependencies / Frameworks / Tools:**
- .NET 10 SDK (`dotnet`)
- Git CLI (`git`)
- GitHub CLI (`gh`)
**Build / Packaging Command:** `dotnet publish SecretSantaMatcher.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true -p:DebugType=embedded -o bin/Release/publish`
**Verification / Test Command:** `dotnet test SecretSantaMatcher.Tests/SecretSantaMatcher.Tests.csproj`

## 4. Work Backlog (B7 Todo Commands)
| ID | Title | Status | Assigned Persona | Dependencies |
|---|---|---|---|---|
| T1 | Single-File Executable Architecture Blueprint | completed | xp-architect | - |
| T2 | Configure csproj & Publish Profile for Standalone win-x64 Packaging | completed | xp-developer | T1 |
| T3 | Verification Test of Single-File Packaging & Inner-Loop Integrity | completed | xp-developer | T2 |
| T4 | Verification & Continuous Packaging | pending | xp-orchestrator | T3 |
| T5 | PR Merge, Release Packaging & Tagging | pending | xp-orchestrator | T4 |

## 5. Sub-Agent Coordination
- Personas: `xp-architect` (system architecture and boundaries), `xp-developer` (TDD, pair programming, incremental delivery).
- Core skills: `xp-orchestrator`, `environment-manager`, `human-checkpoint`, `release-packager`.
- Working branch: `feature/single-file-executable`.

## 6. Checkpoints & History
- [x] Architecture Approved (human approval)
- [ ] Continuous Build Uploaded
- [ ] PR Reviewed & Approved
- [ ] Release Package Generated

## 7. Release Configuration
**Continuous Release Tag:** continuous-build
**Continuous Release Name:** Continuous Build
**Target Output Artifact:** bin/Release/SecretSantaMatcher-continuous-win-x64.zip
**Latest Release Tag:** v1.2.0
**Build Type Override:**  
