# Secret Santa Matcher

A WPF desktop application for coordinating Secret Santa gift exchanges. The application supports relationship exclusions, prevents mirror matches, provides a live email template preview, and automates match distribution using Gmail SMTP.

---

## Features

- **Backtracking Solver**: Uses a recursive backtracking search to generate valid gift-giving circles that respect participant exclusions.
- **Asymmetric Directed Exclusions**: Allows participants to hold multiple directed exclusions (for example, A cannot buy for B, and B cannot buy for C) to support complex relational constraints.
- **Mirror Match Prevention**: Optional setting to exclude reciprocal matches (where A buys for B and B buys for A) to increase variety.
- **Live Email Template Preview**: Renders a live preview of the drafted email subject and body as you type, using template tokens.
- **Gmail SMTP Integration**: Automatically sends individual matches via Gmail using Google App Passwords. Includes progress tracking and runtime logs.
- **Session Auto-Save & Legacy Migration**: Saves your session locally on exit. Supports importing and exporting configurations via JSON. Automatically migrates legacy mutual significant-other configurations to bidirectional directed exclusions upon loading.
- **Dynamic Exclusions Management**: Real-time validation removes existing exclusions and the participant itself from available selection candidates. Deleting a participant automatically sweeps and removes their reference from all other exclusion lists to maintain state consistency.

---

## Getting Started

### Prerequisites

- **.NET 10.0 SDK** or higher.
- **Windows OS** (WPF requires a Windows desktop environment).

### Run Locally

1. Clone or download the repository.
2. Open a terminal in the project directory.
3. Run:
   ```powershell
   dotnet run
   ```

### Compile Standalone Executable

To publish a self-contained Windows executable that runs on any modern Windows PC without requiring a system-wide .NET installation, run:

```powershell
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:PublishReadyToRun=true -p:IncludeNativeLibrariesForSelfExtract=true
```

The compiled standalone executable will be generated at:
`.\bin\Release\net10.0-windows\win-x64\publish\SecretSantaMatcher.exe`

---

## Usage Guide

### 1. Manage Participants
- Go to the **Manage Participants** tab.
- Enter a participant's Name, Email, and optionally a Wishlist URL.
- **Exclusions**: Use the **Exclusions Manager** panel to add multiple directed exclusions. Select another participant from the dropdown and click Add. You can remove existing exclusions by clicking the Remove button next to their name.
- **Directed Behavior**: These exclusions prevent the current participant from buying for the selected recipient. They are asymmetric (directed) by default, meaning that excluding B for participant A does not automatically exclude A for participant B unless explicitly added.
- **Auto-Cleanup**: Editing or deleting participants automatically sweeps all exclusion lists in the session and removes references to the affected participant to keep data clean.

### 2. Customize Email Template
- Go to the **Email Template** tab.
- Write your email subject and body using the following dynamic tokens:
  - `{Giver}`: The name of the sender who buys the gift.
  - `{Receiver}`: The name of the recipient who gets the gift.
  - `{Wishlist}`: The wishlist URL of the recipient.
  - `{Organizer}`: The name of the exchange coordinator.
- Inspect the **Live Sandbox Preview** card to see the rendered output.

### 3. Configure SMTP Settings
- Go to the **Gmail Settings** tab.
- Enter the **Organizer's Name**, **Gmail Address**, and a **Google App Password** (16-character passcode).
- Click **Test Connection** to send a test email and verify your SMTP credentials.

### 4. Solve & Distribute
- Go to the **Match & Send Emails** tab.
- Toggle **Prevent mirror matches** to exclude reciprocal pairings.
- Click **Generate Secret Santa Matching**.
- Toggle **Reveal calculated matches list** if you want to inspect pairings before sending.
- Click **Distribute Match Emails Now** to send individual matching invitations.

---

## Security & Privacy

- **Password Obfuscation**: The Gmail App Password is obfuscated locally on disk using a symmetric XOR key.
- **Safe Exports**: Session exports to JSON automatically strip SMTP passwords to prevent credential leaks.
- **Local Processing**: All data and match calculations remain strictly on your local machine.

---

## AI Agent Guidelines

This project is built using specialized, domain-isolated development subagents. In future conversations, the AI assistant will automatically discover and use these agents for all development tasks.

Please refer to the profiles and boundaries defined in the .agents directory:
- **UiUxAgent**: Handles WPF XAML presentation, layouts, grids, visual styles, and themes. Managed files: MainWindow.xaml, App.xaml, resource dictionaries.
- **BackendLogicAgent**: Handles C# core algorithms, solvers, models, state data structure updates, local saving, JSON configurations, and serializations. Managed files: Services/*, Models/*, MainWindow.xaml.cs.
- **TestingAgent**: Handles unit testing, boundary condition verification, mock validation, and UI integration test scripts. Managed files: tests/*, test reports, and verification plans.

All agents collaborate under strict boundaries. UI/UX Agent does not modify core algorithms, Backend Agent does not modify visual styling, and the Testing Agent verifies changes without modifying production code.

### Git Branching Strategy for AI Agents
To keep the main branch clean and protect production stability, all agents must adhere to the following Git workflow:
1. **Never Work on Main**: Agents must never commit directly to the main branch or leave modified files in the working directory of the main branch.
2. **Descriptive Feature Branches**: At the start of any development task, the agent must immediately create and switch to a descriptive feature branch prefixed with `feature/` or `bugfix/` (for example, `feature/multiple-exclusions`).
3. **Stage and Commit**: When all changes are complete and verified by the test suite, the agent must stage and commit the changes cleanly to the feature branch before concluding the turn.
