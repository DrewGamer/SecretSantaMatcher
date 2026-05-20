# Secret Santa Matcher

A WPF desktop application for coordinating Secret Santa gift exchanges. The application supports relationship exclusions, prevents mirror matches, provides a live email template preview, and automates match distribution using Gmail SMTP.

---

## Features

- **Backtracking Solver**: Uses a recursive backtracking search to generate valid gift-giving circles that respect participant exclusions.
- **Mirror Match Prevention**: Optional setting to exclude reciprocal matches (where A buys for B and B buys for A) to increase variety.
- **Slate Dark UI**: A dark-themed layout with aligned input fields and tabbed navigation.
- **Live Email Template Preview**: Renders a live preview of the drafted email subject and body as you type, using template tokens.
- **Gmail SMTP Integration**: Automatically sends individual matches via Gmail using Google App Passwords. Includes progress tracking and runtime logs.
- **Session Auto-Save**: Saves your session locally on exit. Supports importing and exporting configurations via JSON.
- **Bidirectional Exclusions Management**: Adding, editing, or deleting a participant dynamically updates mutual significant other links to keep the solver's state consistent.

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
- **Exclusions**: Use the **Significant Other** dropdown to pair couples or partners who should not buy for each other. Setting B as A's partner automatically updates B to link back to A.
- Edit or delete participants directly. Deletions and edits automatically clean up mutual exclusions.

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
