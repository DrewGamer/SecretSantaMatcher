# Secret Santa Matcher - Agent Registry & Guidelines

This project uses a specialized team of development subagents configured inside the `.agents/` directory.

## Custom Subagents Setup
At the start of the session, please automatically read the profiles inside the `.agents/` directory and use the `define_subagent` tool to register:
1. **testing_agent**: Testing Specialist Agent (from `.agents/testing_agent.md`)
2. **backend_logic_agent**: Backend & Logic Agent (from `.agents/backend_logic_agent.md`)
3. **ui_ux_agent**: UI/UX Specialist Agent (from `.agents/ui_ux_agent.md`)

Delegate tasks to these subagents to maintain separation of concerns and verify code correctness before merge.
