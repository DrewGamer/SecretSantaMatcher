# Changelog

All notable changes to the **Secret Santa Matcher** application since version 1.0 will be documented in this file.

---

## [1.1.0] - 2026-06-09

### Added (New Features)
- **Interactive Email Template Tokens**: Clickable placeholder buttons (`{Giver}`, `{Receiver}`, `{Wishlist}`, `{Organizer}`) are now available. Clicking a token inserts it directly at your text cursor (caret) position within the email subject or body text boxes.
- **Smart Focus-Caret Behavior**: The application remembers which input box was last focused and inserts the token there. Once clicked, the text box is focused automatically and the cursor is placed right after the inserted token so you can continue typing immediately.
- **Action Confirmation Prompts**: Added a safety pop-up window confirming if you want to delete a participant or clear the entire participant list to prevent accidental deletions.
- **Asymmetric Directed Exclusions**: Expanded relation restrictions to support multiple custom exclusions per participant, allowing complex constraint configurations.
- **Exclusion Autocompletion**: Added autocomplete suggestions in the exclusions manager dropdown list to quickly find and add participants.

### Changed (UI & UX Updates)
- **Interactive Badges**: Replaced static, label-only placeholder badges under the email template with interactive, styled buttons that provide visual highlight feedback when hovered.
- **Streamlined Exclusion Entry**: Pressing `Enter` while choosing a participant in the exclusions dropdown now automatically adds the exclusion and resets the selection field for faster data entry.
- **Clean Alignment**: Improved vertical grid layouts, margins, and control spacing across UI panels to prevent visual clipping.

### Fixed (Bug Fixes)
- **Exclusion Clearing Flow**: Fixed an issue where the exclusions ComboBox state was not properly reset after adding a participant exclusion.
- **Automatic Reference Cleanups**: Deleting a participant now dynamically scans and removes all other exclusions that referenced the deleted participant, keeping the session data clean and avoiding crashes.
- **Legacy Configuration Import**: Supported migrating older mutual significant-other configurations to bidirectional directed exclusions automatically upon loading.

---

*Note: For developer guidelines and details on testing automation or agent configuration, please see the [README.md](file:///C:/SourceCode/SecretSantaMatcher/README.md) or the [AGENTS.md](file:///C:/SourceCode/SecretSantaMatcher/AGENTS.md) documentation.*
