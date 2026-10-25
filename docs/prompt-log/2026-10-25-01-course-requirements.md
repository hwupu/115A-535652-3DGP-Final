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

## Follow-up prompt (verbatim)
> conflicts:
> 1. Still use AI-native workflow. We will do manual polish/refinement afterward by hand. That way, we understand the code, and contribure to manual coding too. Yes, each of us to should four seperately, this should not be an issue.
> 2. Note it down in the ticket.
> 3. It is good idea, maybe we can start with standard club, then as it levels, it gain other types.
> 4. Note it down in the ticket.
> 5. ignore for now
> 6. ok
>
> I believe we can propose our own grading items in the rubrics? What will you propose?
>
> for Missing from the plan, know it down in tickets
>
> we follow current plan for mini-exercises [@proposal.md#L139:143](file:///Users/phwu/Documents/Unity%20Projects/115A-535652-3DGP-Final/docs/references/proposal.md#L139:143). make sure the tickets are assigned correctly for each members. let agents know what they should do for mini-exercises (generic, seperate scene...)
>
> questions:
> 1. can help
> 2. mini-1 & mini-2 demo on Nov 16. mini-3 & mini-4 demo on Dec 21.
> 3. yes, you can ignore it.
> 4. you can note it down in the ticket. I believe Pongpat is in charge of it.
> 5. ask me later for that ticket.
> 6. [@proposal.md#L139:143](file:///Users/phwu/Documents/Unity%20Projects/115A-535652-3DGP-Final/docs/references/proposal.md#L139:143)
>
> please check with sprint board too, if it is aligned with current plan

### What the AI did
- ADR-0010 (mini-exercises as standalone scenes, plan aligned with rubrics, runtime-spawn exception for mini-exercise scenes only). Agent guide `docs/mini-exercises.md`; CLAUDE.md points to it.
- Created label `area:mini-exercise` and 12 mini-exercise issues #57–#68 per proposal §12 (first two per member → Sprint 4 / Nov 16, last two → Sprint 9 / Dec 21; order provisional).
- Created gap stories #69–#84: scaffolding tool, leveling (start with the standard club), god keys, Garden and Lake audio, 3 art passes, misc menus, HUD additions, FSM inventory, manual artwork, optional enhancements (needs-po), LOC report, final report, and the Form B tracker with proposed "Others" items.
- Updated #9–#11, #13, #16, #20–#24, #26, #29, #30, #34, #36–#42, #44, #45, #52, #54. Fixed mini-exercise references that pointed at the wrong member (#21, #26, #30, #39) and 4.B.4 → 4.B.1 (#34). Noted the non-cat L3 guardian (#26, #37, Pongpat) and NPC skills (#21, #24, #26). #42 narrowed to the shared audio manager plus Desert SFX.
- Board: all new issues added as Todo; #53 (closed) set to Done. Roadmap, gap analysis (resolutions table) and process (evidence capture at sprint end) updated.

### Open
- Sprint 0 leftovers: #1 (clone with LFS) and #3 (asset sources, overlaps #12) are still open and the Sprint 0 milestone is past due.
- Walk mode: ask Henry later. Teaser video length: deferred.

## Follow-up prompt (verbatim)
> we can add Golf shot system and Hidden time-budget danger system. the rest i'm not sure. maybe you can note it down in the tickets first, can discusss later.
>
> We need a master plan for use to review before we run the sprint. I'm not sure what it called. reqirments? roadmap?

### What the AI did
- #84: "Others" items 1–2 accepted; 3–8 marked to discuss. Comments added on #7, #15, #16 (accepted) and #8, #17, #19, #23, #25, #42, #47 (candidates).
- `docs/scrum/roadmap.md` restructured into the **Release Plan (master plan)**, status DRAFT for PO review: Product Goal, release targets, key dates, phases, workload per member, review points, risks, then the sprint plan and backlog snapshot.
