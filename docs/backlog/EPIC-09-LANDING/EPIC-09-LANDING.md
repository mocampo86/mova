# EPIC-09 — Landing Page and Public Site

## Objective

Build a public-facing landing page and shared site shell that explains the platform, showcases complexes, and guides both players and owners to register or list their complex.

## Scope

- Public home page.
- Explanation of how the platform works.
- Section for players (users).
- Section for owners (complexes).
- Featured complexes.
- User login / profile access.
- Admin access.
- Responsive and SEO-friendly design.

## User stories

| ID | Story |
|----|-------|
| [US-048](US-048.md) | As a visitor, I want to understand what the platform offers. |
| [US-049](US-049.md) | As a player, I want to see how to find and book a court. |
| [US-050](US-050.md) | As an owner, I want to see how to list my complex. |
| [US-051](US-051.md) | As a visitor, I want to see featured or recently added complexes. |
| [US-052](US-052.md) | As a visitor, I want to access login and registration from the landing page. |
| [US-053](US-053.md) | As a search engine, I want basic SEO metadata on public pages. |

## Acceptance criteria

- [ ] The landing page loads quickly and is responsive on mobile.
- [ ] Sections clearly target players and owners.
- [ ] Featured complexes are pulled from active public data.
- [ ] Login and admin access are visible and functional.
- [ ] Basic SEO tags (title, meta description, Open Graph) are present.

## Dependencies

- EPIC-01 — Technical Foundation.
- EPIC-03 — Sports Complex Administration (for featured data).

## Technical notes

- Use React Router for public routes.
- Lazy load below-the-fold content where beneficial.
- Implement a `PublicLayout` separate from `UserLayout` and `AdminLayout`.
- SEO can be static for the MVP; dynamic rendering is future work.
