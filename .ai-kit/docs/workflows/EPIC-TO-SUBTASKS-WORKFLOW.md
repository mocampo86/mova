# WORKFLOW --- Epic to User Stories and Technical Subtasks

## Purpose

Define a repeatable workflow for decomposing a product or technical Epic
into actionable User Stories and technical subtasks.

The workflow is designed to support AI-assisted development while
preserving human review and approval before implementation begins.

------------------------------------------------------------------------

## Workflow Overview

``` text
EPIC
  |
  v
Analyze Epic
  |
  +-- Goals
  +-- Scope
  +-- Constraints
  +-- Success Criteria
  +-- Out of Scope
  |
  v
Generate / Refine User Stories
  |
  v
Validate User Stories
  |
  +-- Valuable?
  +-- Testable?
  +-- Small enough?
  +-- Aligned with Epic?
  |
  v
Generate Acceptance Criteria
  |
  v
Analyze Repository and Technical Context
  |
  v
Generate Technical Subtasks
  |
  v
Detect Dependencies
  |
  v
Prioritize and Sequence
  |
  v
Human Review Gate
  |
  +-- APPROVED --> Ready for Development
  |
  +-- CHANGES --> Regenerate affected items
  |
  v
Implementation
  |
  v
Validation
  |
  v
Pull Request / Completion Report
```

------------------------------------------------------------------------

## 1. Epic Intake

### Input

A Markdown Epic document, for example:

``` text
/epics/EPIC-14-VISUAL-MODERNIZATION.md
```

### Extract

The planning agent must identify:

-   Epic identifier and title.
-   Business objective.
-   User or business value.
-   Context.
-   Functional scope.
-   Non-functional requirements.
-   Constraints.
-   Success criteria.
-   Explicit out-of-scope items.
-   Existing preliminary User Stories, if present.

### Rules

-   Do not invent requirements that contradict the Epic.
-   Treat `Out of Scope` as a hard boundary.
-   Identify ambiguity instead of silently making significant
    assumptions.
-   If User Stories already exist, refine them before creating
    replacements.

### Output

A structured Epic analysis used as context for the next stage.

------------------------------------------------------------------------

## 2. User Story Decomposition

Break the Epic into independently understandable increments of user or
business value.

### Naming Convention

For Epic `EPIC-14`:

``` text
US-14.1
US-14.2
US-14.3
...
```

Example:

``` text
EPIC-14
├── US-14.1 Modernize Hero Section
├── US-14.2 Implement Sports Image Carousel
├── US-14.3 Improve Main Navigation
├── US-14.4 Implement Responsive Design
├── US-14.5 Create Reusable Visual Design System
└── US-14.6 Improve Basic Accessibility
```

### User Story Format

``` markdown
# US-14.2 — Implement Sports Image Carousel

## User Story

As a MOVA visitor,
I want to see different sports facilities and activities on the homepage,
so that I can quickly understand the experiences available through the platform.

## Business Value

Explain why this story matters.

## Acceptance Criteria

...

## Dependencies

...

## Out of Scope

...
```

### Validation

Each User Story should be:

-   Aligned with the Epic.
-   Valuable on its own or as a clearly defined enabling story.
-   Testable.
-   Small enough to implement and review independently.
-   Free of unnecessary implementation detail.

------------------------------------------------------------------------

## 3. Acceptance Criteria Generation

Generate measurable acceptance criteria for every User Story.

Prefer Given / When / Then for behavioral requirements.

Example:

``` gherkin
Given a visitor opens the MOVA homepage
When the Hero section is displayed
Then a sports-related background image should be visible

Given the carousel contains multiple slides
When the configured transition interval expires
Then the next slide should become visible

Given the background image changes
When a new slide is displayed
Then the Hero text and primary CTAs should remain readable
```

### Rules

Acceptance criteria must:

-   Be observable.
-   Be testable.
-   Describe expected behavior rather than implementation.
-   Cover important responsive and accessibility behavior when
    applicable.
-   Remain inside the Epic scope.

------------------------------------------------------------------------

## 4. Repository and Technical Context Analysis

Before generating technical subtasks, inspect the existing codebase.

### Analyze

-   Solution and project structure.
-   Frameworks and runtime versions.
-   Existing components.
-   Architecture and design patterns.
-   API conventions.
-   Database and Entity Framework usage.
-   Cloud/AWS integrations.
-   Testing frameworks.
-   Existing reusable abstractions.
-   Coding conventions.
-   CI/CD configuration when relevant.

### Important Rule

Do not create technical work simply because a technology exists in the
stack.

For example, a visual-only User Story should not automatically produce:

-   Database changes.
-   Entity Framework migrations.
-   API endpoints.
-   AWS infrastructure changes.

Only generate tasks required to satisfy the User Story.

### Reuse First

Prefer:

1.  Existing components.
2.  Existing patterns.
3.  Existing abstractions.
4.  Small extensions.

Create new abstractions only when justified.

------------------------------------------------------------------------

## 5. Technical Subtask Generation

Convert each approved User Story into implementation-oriented tasks.

### Naming Convention

For `US-14.2`:

``` text
TASK-14.2.1
TASK-14.2.2
TASK-14.2.3
...
```

Example:

``` text
US-14.2 — Implement Sports Image Carousel

├── TASK-14.2.1 Create reusable HeroCarousel component
├── TASK-14.2.2 Add carousel image configuration
├── TASK-14.2.3 Implement automatic slide transition
├── TASK-14.2.4 Implement manual navigation controls
├── TASK-14.2.5 Add readability overlay
├── TASK-14.2.6 Implement responsive behavior
├── TASK-14.2.7 Add accessibility and keyboard support
├── TASK-14.2.8 Add automated tests
└── TASK-14.2.9 Optimize image loading
```

### Required Task Structure

``` markdown
# TASK-14.2.3 — Implement Automatic Slide Transition

## Parent

US-14.2 — Implement Sports Image Carousel

## Description

Describe the implementation goal.

## Affected Area

List the expected project, module, component, or layer.

## Implementation Notes

Provide relevant technical guidance without unnecessarily prescribing every code-level decision.

## Done When

- Condition 1.
- Condition 2.
- Condition 3.

## Testing

Describe expected automated and/or manual validation.

## Dependencies

- TASK-14.2.1

## Risks / Notes

Document relevant implementation risks or assumptions.
```

------------------------------------------------------------------------

## 6. Dependency Analysis

Create a dependency graph before implementation begins.

Example:

``` text
TASK-14.2.1 Create Carousel Component
        |
        +--------------------+
        |                    |
        v                    v
TASK-14.2.3             TASK-14.2.4
Auto Transition        Manual Controls
        |                    |
        +----------+---------+
                   |
                   v
             TASK-14.2.8
                Tests
```

### Rules

-   Avoid artificial dependencies.
-   Identify tasks that can run in parallel.
-   Detect blocking tasks.
-   Place foundational changes before dependent work.

------------------------------------------------------------------------

## 7. Prioritization and Sequencing

Assign an implementation sequence based on:

1.  Dependencies.
2.  Business value.
3.  Risk.
4.  Foundational work.
5.  Ability to validate incrementally.

Where possible, mark tasks as:

``` text
BLOCKING
PARALLELIZABLE
OPTIONAL
```

------------------------------------------------------------------------

## 8. Human Review Gate

No implementation should begin automatically after decomposition.

Present the generated planning artifacts for human review.

### Review Checklist

-   [ ] User Stories correctly represent the Epic.
-   [ ] Acceptance Criteria are testable.
-   [ ] No requirements were invented.
-   [ ] Out-of-scope boundaries are respected.
-   [ ] Technical tasks are necessary.
-   [ ] Existing architecture is respected.
-   [ ] Dependencies are correct.
-   [ ] Tasks are reasonably sized.
-   [ ] Testing is included.
-   [ ] No unnecessary infrastructure or database work was introduced.

### Outcomes

#### APPROVED

Move the selected User Story to:

``` text
READY FOR DEVELOPMENT
```

#### CHANGES REQUESTED

Only regenerate or modify the affected planning artifacts.

Do not unnecessarily regenerate already approved work.

------------------------------------------------------------------------

## 9. Implementation

Implement one approved User Story or a clearly defined set of related
tasks at a time.

The implementation agent receives:

-   Parent Epic context.
-   Selected User Story.
-   Acceptance Criteria.
-   Approved technical tasks.
-   Dependency information.
-   Relevant repository context.

### Rules

-   Do not expand scope without approval.
-   Follow existing repository conventions.
-   Keep changes focused.
-   Add or update tests.
-   Do not mark work complete solely because it compiles.
-   Document significant deviations from the approved plan.

------------------------------------------------------------------------

## 10. Validation

Before considering a User Story complete, verify:

### Functional

-   All Acceptance Criteria pass.

### Technical

-   Project builds successfully.
-   Automated tests pass.
-   New behavior has appropriate test coverage.
-   Existing behavior has not regressed.
-   Static analysis/linting passes when configured.

### Non-Functional

When applicable:

-   Responsive behavior is verified.
-   Accessibility requirements are verified.
-   Performance impact is acceptable.
-   Security implications are reviewed.
-   Cloud resources follow existing project conventions.

------------------------------------------------------------------------

## 11. Completion Report

Generate a concise implementation report after completing the User
Story.

Example:

``` markdown
# Implementation Report — US-14.2

## Status

COMPLETED

## Implemented Tasks

- TASK-14.2.1
- TASK-14.2.2
- TASK-14.2.3
- TASK-14.2.4

## Acceptance Criteria

- AC-01: PASS
- AC-02: PASS
- AC-03: PASS

## Tests

- Unit tests: PASS
- Component tests: PASS
- Build: PASS

## Key Changes

Summarize the implementation.

## Deviations

Document differences from the approved technical plan.

## Known Issues

List remaining issues, or `None`.

## Follow-up

List recommended future work that is outside the current User Story.
```

------------------------------------------------------------------------

## Recommended Repository Structure

``` text
/docs
├── epics
│   └── EPIC-14-VISUAL-MODERNIZATION.md
│
├── stories
│   └── EPIC-14
│       ├── US-14.1-HERO-SECTION.md
│       ├── US-14.2-SPORTS-CAROUSEL.md
│       └── ...
│
├── tasks
│   └── US-14.2
│       ├── TASK-14.2.1-CAROUSEL-COMPONENT.md
│       ├── TASK-14.2.2-IMAGE-CONFIG.md
│       └── ...
│
└── implementation-reports
    └── US-14.2-IMPLEMENTATION-REPORT.md
```

------------------------------------------------------------------------

## Definition of Ready --- User Story

A User Story is ready for implementation when:

-   [ ] Parent Epic is identified.
-   [ ] Business/user value is clear.
-   [ ] Scope is understood.
-   [ ] Acceptance Criteria are defined.
-   [ ] Dependencies are identified.
-   [ ] Technical analysis has been performed.
-   [ ] Required subtasks have been generated.
-   [ ] Testing expectations are documented.
-   [ ] Human approval has been received.

------------------------------------------------------------------------

## Definition of Done --- User Story

A User Story is done when:

-   [ ] Approved tasks are completed.
-   [ ] Acceptance Criteria pass.
-   [ ] Code builds successfully.
-   [ ] Automated tests pass.
-   [ ] Appropriate new tests have been added.
-   [ ] No known regressions were introduced.
-   [ ] Relevant documentation is updated.
-   [ ] Significant deviations are documented.
-   [ ] Implementation report is generated.
-   [ ] Changes are ready for Pull Request review.

------------------------------------------------------------------------

## Core Principle

``` text
Epic
  -> Understand
  -> Decompose
  -> Define behavior
  -> Inspect repository
  -> Plan technical work
  -> Review
  -> Implement
  -> Test
  -> Validate
  -> Report
```

AI can accelerate planning and implementation, but scope decisions and
the transition from planning to development remain explicit and
reviewable.
