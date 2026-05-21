# Secret Santa Matcher - Agent Registry

Welcome to your project's custom **Agent Registry**! This directory contains the system profiles, behavioral prompts, and boundaries for your specialized team of development subagents. 

By delegating tasks to these specialized agents, you maintain high code quality, rapid development times, and a clear separation of concerns across the application.

---

## Meet Your Specialized Team

| Agent Profile | Role / Focus | Primary Files Managed |
| :--- | :--- | :--- |
| [UI/UX Agent](ui_ux_agent.md) | Presentation, visual design, responsiveness, grids, themes, and micro-animations. | `MainWindow.xaml`, `App.xaml`, resource dictionaries. |
| [Backend & Logic Agent](backend_logic_agent.md) | Math solvers, data structures, local saving, JSON configurations, cryptography, and SMTP networking. | `Services/*`, `Models/*`, `MainWindow.xaml.cs`. |
| [Testing Agent](testing_agent.md) | Verification scripts, unit tests, bounds checking, data templates integrity, and user flow UI automation. | `tests/*` (proposed), test reports, and verification plans. |

---

## How to Launch These Agents

When you want to kick off a new feature or optimization, you can ask your primary assistant to launch one of these agents.

### Example Commands:
- *"Launch the **UI/UX Agent** to style the matching statistics container with a subtle cyan glow."*
- *"Spin up the **Backend Agent** to optimize our backtracking solver to check for sub-groups in larger lists."*

The primary assistant will read the respective `.md` profile, programmatically configure a subagent using the system prompt inside, and set it to work in a parallel background workspace.

---

## Collaborative Rules of Engagement

To prevent conflict and code regressions, the agents adhere to these strict boundary rules:
1. **No Shared Overwrite**: The UI/UX Agent never touches C# solver/network code, and the Backend Agent never touches XAML grids/styles.
2. **Contract-First State**: If a backend logic update requires a new UI element, the Backend Agent outlines the model bindings first, then coordinates with the UI/UX Agent to implement the visual XAML.
