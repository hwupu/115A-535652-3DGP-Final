# 2026-10-25 01 — Course requirements and grading gap analysis

- **Who:** Henry (@hwupu) + Claude (Opus 5.5, Claude Code)
- **Related:** `docs/references/course/`, `docs/scrum/grading-gap-analysis.md`, branch `feature/proposal`

## Prompt (verbatim)
> I see. I think we need to update a bit. I just got the instructions from professor and I have included the files in references. Please don't commit the docx, I'll delete it later. Please read them and organize in to markdown for you (and other AI agents) to use. (I see some are form, you just need to know the rubrics, no need to orgnized into form format; we will do form filling manually in Word.) Next step, we will see how can we update our plan to be aligned with grading requirments. Each of use need to do 4 mini exercise, however, it may not be align with our project. I was thinking that we can still do the mini-execrise, but make the code generic so that it can sit alone side, and perhaps reuse some portion of it? We should have seperate scenes for mini-project to demo. Please check if there are any conflicts, or missing, from your proposal/plan/project that is required by profressor.

## What the AI did
- Converted the 5 instructor `.docx` handouts into markdown under `docs/references/course/`: instruction, Form A rubric, Form B rubric, report template, mini-exercises. The `.docx` files were left untracked and not committed.
- Wrote `docs/scrum/grading-gap-analysis.md`: conflicts, missing Form B/A/report items, a mini-exercise strategy (standalone scenes plus generic Core code, enforced with asmdefs) and open questions.
- Added a pointer to the course requirements in `CLAUDE.md`.
- Did **not** change issues, the roadmap or the proposal. That waits for PO decisions.

## Decisions / open questions
- See §5 of the gap analysis. The biggest open item is whether AI-written code is acceptable for individual mini-exercises ("implement on your own").
- Running mini-exercise scenes with loop-spawned objects needs an ADR (an exception to the no-runtime-scene-building rule) once the PO agrees.
