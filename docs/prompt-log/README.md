# Prompt Log

AI-native workflow: we keep a record of the prompts that steered the project and what the AI did in response.

## Convention
- One file per working session: `YYYY-MM-DD-NN-short-topic.md` (NN = 01, 02, ... per day).
- Humans' prompts are quoted **verbatim** (trim only secrets). AI adds a short summary of actions, files touched, open questions, and links to ADRs/issues/PRs.
- Agents append to the log in the same commit/PR as the work. Don't rewrite history; add a correction note instead.
- Template: `_template.md`.
