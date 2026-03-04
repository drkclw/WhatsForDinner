<!--
SYNC IMPACT REPORT
==================
Version change: 0.0.0 → 1.0.0 (MAJOR: Initial constitution adoption)
Modified principles: N/A (first version)
Added sections:
  - I. Code Quality
  - II. Testing Standards
  - III. User Experience Consistency
  - IV. Performance Requirements
  - Technical Decision Framework
  - Implementation Guidelines
  - Governance
Removed sections: N/A
Templates requiring updates:
  - .specify/templates/plan-template.md: ✅ Already has Constitution Check section
  - .specify/templates/spec-template.md: ✅ Compatible with requirements structure
  - .specify/templates/tasks-template.md: ✅ Phase structure supports testing discipline
Follow-up TODOs: None
-->

# WhatsForDinner Constitution

## Core Principles

### I. Code Quality

All code MUST adhere to these non-negotiable standards:

- **Readability First**: Code MUST be self-documenting with clear naming conventions; comments explain "why", not "what"
- **Single Responsibility**: Each module, class, and function MUST have one clear purpose
- **DRY Enforcement**: Duplication MUST be extracted into shared utilities; three occurrences trigger mandatory refactoring
- **Consistent Style**: All code MUST pass linting and formatting checks before merge
- **Type Safety**: Strong typing MUST be used where the language supports it; avoid `any` or equivalent untyped patterns
- **Error Handling**: All errors MUST be handled explicitly; no silent failures or swallowed exceptions

**Rationale**: Maintainable code reduces long-term costs and enables confident refactoring.

### II. Testing Standards

Testing is mandatory for all production code:

- **Coverage Requirement**: All new features MUST have associated tests; minimum 80% code coverage for critical paths
- **Test Pyramid**: Unit tests form the base (fast, isolated); integration tests verify component interactions; E2E tests cover critical user journeys
- **Test Independence**: Each test MUST be isolated and repeatable; no test dependencies on execution order or external state
- **Failing Tests Block Merge**: No code MUST be merged with failing tests; CI pipeline enforces this gate
- **Test Naming**: Tests MUST clearly describe the scenario and expected outcome (e.g., `shouldReturnErrorWhenRecipeNotFound`)

**Rationale**: Comprehensive testing prevents regressions and enables confident deployments.

### III. User Experience Consistency

The user interface MUST provide a cohesive, predictable experience:

- **Design System Compliance**: All UI components MUST follow the established design system; no ad-hoc styling
- **Responsive Design**: All interfaces MUST work across mobile, tablet, and desktop viewports
- **Loading States**: All async operations MUST display appropriate loading indicators
- **Error Feedback**: All user-facing errors MUST provide clear, actionable messages; no technical jargon
- **Accessibility**: All features MUST meet WCAG 2.1 AA standards; keyboard navigation and screen reader support required
- **Interaction Patterns**: Similar actions MUST have consistent behavior across the application

**Rationale**: Consistent UX builds user trust and reduces cognitive load.

### IV. Performance Requirements

The application MUST meet these performance benchmarks:

- **Initial Load**: First Contentful Paint MUST occur within 2 seconds on 3G connections
- **Interaction Response**: User interactions MUST provide feedback within 100ms
- **API Response**: Backend endpoints MUST respond within 500ms for standard operations; paginated for large datasets
- **Bundle Size**: Frontend bundles MUST be lazy-loaded; critical path under 200KB compressed
- **Memory Management**: No memory leaks; components MUST clean up resources on unmount
- **Database Queries**: No N+1 queries; all database access MUST be optimized with appropriate indexes

**Rationale**: Performance directly impacts user satisfaction and accessibility.

## Technical Decision Framework

Technical decisions MUST align with constitutional principles:

- **Principle Priority**: When trade-offs arise, prioritize in this order: (1) User Experience, (2) Code Quality, (3) Performance, (4) Development Speed
- **New Dependencies**: Third-party libraries MUST be evaluated for: maintenance status, bundle size impact, security vulnerabilities, and license compatibility
- **Architecture Changes**: Significant architectural changes MUST be documented with rationale and reviewed against constitution compliance
- **Technology Selection**: New technologies MUST demonstrate clear benefits over existing solutions; avoid "resume-driven development"
- **Technical Debt**: Debt MUST be tracked and scheduled for resolution; no debt without a cleanup plan

## Implementation Guidelines

All implementation work MUST follow these practices:

- **Branch Strategy**: Feature branches from main; descriptive naming (`feature/###-recipe-search`, `fix/###-login-error`)
- **Commit Messages**: Conventional commits format; reference issue numbers
- **Code Review**: All changes require review; reviewers verify constitution compliance
- **Documentation**: Public APIs MUST have documentation; complex logic requires inline explanations
- **Feature Flags**: New features SHOULD be behind flags for controlled rollout
- **Monitoring**: Production features MUST have error tracking and key metric monitoring

## Governance

This constitution supersedes all other development practices:

- **Compliance Verification**: All pull requests MUST include a constitution compliance check; reviewers verify adherence
- **Amendment Process**: Changes require documented rationale, team review, and migration plan for affected code
- **Exception Handling**: Temporary exceptions MUST be documented with justification and remediation timeline
- **Version Control**: Constitution follows semantic versioning (MAJOR.MINOR.PATCH)
- **Periodic Review**: Constitution MUST be reviewed quarterly for relevance and updated as needed

**Version**: 1.0.0 | **Ratified**: 2026-03-03 | **Last Amended**: 2026-03-03
