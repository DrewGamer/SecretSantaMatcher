# Project XP State (B4 Plan Memento)

## 1. Project Context
**Project Name:** Secret Santa Matcher
**Project Type:** Application
**Current Stage:** Architecture
**Primary Tech Stack:** C# (.NET 10.0-windows, WPF), xUnit

## 2. Active Goal & Constraints (B8 Attention Anchor)
**Current Objective:** Overhaul the agentic workflow of the project, integrating the XP lifecycle orchestrator, skills, and personas from DrewGamer/agent-xp-workflow.
**Hard Constraints:** 
- MUST pass human checkpoint for architecture approval.
- MUST pass human checkpoint for PR reviews.
- MUST use environment-manager for new dependencies.
- Adhere strictly to the Git Branching & Workflow Policy in AGENTS.md (never commit directly to main; work on descriptive feature/bugfix branches; verify build and run `dotnet test` before push).
- Maintain separation of concerns across specialized agents (testing_agent, backend_logic_agent, ui_ux_agent).

## 3. Architecture & Tooling
**Approved Architecture:**
WPF desktop application for Secret Santa participant management, constraint-based pairing algorithms (with exclusions and reciprocity rules), draft email template generation with interactive tokens, and SMTP email dispatch.
**Dependencies / Frameworks / Tools:**
- .NET 10 SDK (`dotnet`)
- Git CLI (`git`)
- GitHub CLI (`gh`)
**Build / Packaging Command:** `dotnet publish SecretSantaMatcher.csproj -c Release -o bin/Release/publish`
**Verification / Test Command:** `dotnet test`

## 4. Work Backlog (B7 Todo Commands)
| ID | Title | Status | Assigned Persona | Dependencies |
|---|---|---|---|---|
| T1 | Agentic Workflow Architecture & Integration Blueprint | pending | xp-architect | - |
| T2 | Agentic Workflow & Persona Registry Implementation | pending | xp-developer | T1 |

## 5. Sub-Agent Coordination
- Synchronized skills (`xp-orchestrator`, `environment-manager`, `genesis`, `human-checkpoint`, `release-packager`) and personas (`xp-architect`, `xp-developer`) from `DrewGamer/agent-xp-workflow`.
- Registered custom subagents (`testing_agent`, `backend_logic_agent`, `ui_ux_agent`) per `AGENTS.md`.
- Active working branch: `feature/overhaul-agentic-workflow`.

## 6. Checkpoints & History
- [ ] Architecture Approved
- [ ] PR 1 Reviewed & Approved
- [ ] Release Package Generated

## 7. Release Configuration
**Continuous Release Tag:** continuous-build
**Continuous Release Name:** Continuous Build
**Target Output Artifact:** bin/Release/publish/*
**Build Type Override:** 
