# Secret Santa Matcher - Agent Registry & Guidelines

This project uses a specialized team of development subagents configured inside the `.agents/` directory.

## Custom Subagents Setup
At the start of the session, please automatically read the profiles inside the `.agents/` directory and use the `define_subagent` tool to register:
1. **testing_agent**: Testing Specialist Agent (from `.agents/testing_agent.md`)
2. **backend_logic_agent**: Backend & Logic Agent (from `.agents/backend_logic_agent.md`)
3. **ui_ux_agent**: UI/UX Specialist Agent (from `.agents/ui_ux_agent.md`)

Delegate tasks to these subagents to maintain separation of concerns and verify code correctness before merge.

## Git Branching & Workflow Policy (CRITICAL)
To maintain project stability and track development cleanly, all AI agents must adhere to the following Git workflow strictly:

1. **Before Making Any Code Changes**:
   - Synchronize with the remote repository: Run `git fetch --all --prune` and pull the latest changes on `main` to ensure you are up to date.
   - Branch off `main`: Create and check out a new local branch named descriptively (prefixed with `feature/` or `bugfix/`, e.g., `feature/confirm-delete-clear` or `bugfix/fix-smtp-crash`).
   - **Never** perform work or commit directly to the `main` branch.

2. **During Development**:
   - Ensure the subagents perform their work on the same checked-out branch.
   - Keep commits granular and structured. Use conventional commit messages (e.g. `feat: add clear list confirmation dialog`, `test: verify delete rejection flow`).

3. **Upon Completion**:
   - Verify the build and run all unit tests locally (`dotnet test`) to ensure everything is functioning correctly before pushing.
   - Stage and commit all modifications on the feature/bugfix branch.
   - Push the branch to the remote repository.
   - Generate a clear Pull Request Title and a detailed Markdown Pull Request Description (matching the PR template if one exists) and save it to a file `pr_details.md` or present it to the user.

