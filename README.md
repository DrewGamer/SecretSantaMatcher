# 🎅 Secret Santa Matcher

A premium, elegant, and secure **WPF desktop application** written in C# and .NET 10 to coordinate Secret Santa gift exchanges. It handles complex relationship exclusions, prevents mirror matches, provides a real-time email template sandbox preview, and automates safe distribution using Gmail SMTP.

---

## ✨ Features

- **🧠 Atomic Backtracking Exclusions Engine**: Leverages a highly-efficient recursive backtracking DFS solver that instantly computes valid derangement circular chains under strict significant other exclusions.
- **🔄 Toggleable Mirror Match Prevention**: Optional setting to prevent reciprocal pairings (e.g., A buying for B, and B buying for A) to maximize match randomness and gift variety.
- **🎨 Premium Slate-Dark UI Design**: A stunning, responsive interface utilizing cohesive dark backgrounds, glassmorphism-inspired borders, modern typography, and hover micro-animations.
- **📝 Live Sandbox Email Preview**: Real-time rendering card that shows exactly what your emails will look like as you draft subjects and bodies with active placeholders.
- **📧 Automated SMTP Dispatcher**: Distributes individual matching invitations securely via Gmail using a secure 16-character **Google App Password**. Includes real-time progress bars and operation logs.
- **💾 Auto-Save & Clean Configurations**: Automatically persists your session locally. Supports exporting and importing fully anonymous JSON session configurations (excludes SMTP app passwords for safety).
- **🛠️ Atomically Bound Relationships**: Clean participant addition, editing, and deletion cycles. Setting B as A's significant other mutually binds A as B's significant other, clearing any previous stale associations to guarantee zero dangling solver references.

---

## 🚀 Getting Started

### Prerequisites

- **.NET 10.0 SDK** or higher.
- **Windows OS** (WPF requires Windows Desktop environment).

### Run Locally

1. Clone or download the repository.
2. Open your terminal in the project directory.
3. Run the following command to restore packages and launch the application:
   ```powershell
   dotnet run
   ```

### Compile Standalone Executable

To compile a single, optimized, self-contained Windows executable (`.exe`) that runs on any modern Windows PC without requiring a .NET installation:

```powershell
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:PublishReadyToRun=true -p:IncludeNativeLibrariesForSelfExtract=true
```

The compiled standalone executable will be generated at:
`.\bin\Release\net10.0-windows\win-x64\publish\SecretSantaMatcher.exe`

---

## 🛠️ Usage Guide

### Step 1: Manage Participants
- Navigate to the **Manage Participants** tab.
- Add participants by entering their Name, Email, and optionally an online Wishlist URL.
- **Bidirectional Exclusions**: If two participants are couples/partners and shouldn't buy for each other, select them in the **Significant Other** dropdown. Setting B as A's partner automatically updates B to link back to A.
- You can **Edit** or **Delete** participants on the fly. Deletions and edits automatically clean up mutual exclusions safely.

### Step 2: Customize Email Template
- Navigate to the **Email Template** tab.
- Draft your email subject and body. Use the following dynamic placeholder tokens which will be automatically replaced for each participant:
  - `{Giver}`: The name of the person receiving the email (who buys the gift).
  - `{Receiver}`: The name of the person they matched with (who gets the gift).
  - `{Wishlist}`: The wishlist URL of the recipient.
  - `{Organizer}`: The name of the exchange coordinator.
- Inspect the **Live Sandbox Preview** card to see the beautiful live rendering as you type.

### Step 3: Configure SMTP credentials
- Navigate to the **Gmail Settings** tab.
- Enter the **Organizer's Name** and **Gmail Address**.
- Enter your **Google App Password** (16-digit passcode). 
- *Note: Your standard Google login password will not work. Follow the step-by-step App Password guide provided inside the tab.*
- Click **Test Connection** to verify SMTP connectivity (this sends a test email to yourself to confirm credentials).

### Step 4: Run Solver & Distribute
- Navigate to the **Match & Send Emails** tab.
- **Mirror Matches**: Check **Prevent mirror matches** if you want to avoid reciprocal pairings.
- Click **Generate Secret Santa Matching**. If there are no unsolvable bottlenecks, the solver will succeed instantly.
- **Organizer Eye Only Review**: Toggle **Reveal calculated matches list** if you want to inspect pairings before sending.
- Click **Distribute Match Emails Now** to dispatch matching invitations individually.

---

## 🔒 Security & Privacy

- **App Password Protection**: Your Gmail App Password is obfuscated locally on disk using symmetric XOR obfuscation. It is never stored in plaintext.
- **Config Export Safety**: When you export your session to a JSON file to share with others, SMTP App Passwords and credentials are automatically stripped out, ensuring zero risk of credential leaks.
- **Local Sandbox Execution**: All calculation, solving, and saving is performed locally on your machine. No participant names, emails, wishlists, or matches are ever uploaded to third-party servers.
