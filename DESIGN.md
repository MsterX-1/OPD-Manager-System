---
name: OPD Manager
description: ERP Hospital System for Outpatient Management
colors:
  primary: "#3b82f6"
  primary-hover: "#2563eb"
  bg-primary: "#0f172a"
  bg-secondary: "#1e293b"
  bg-tertiary: "#334155"
  text-primary: "#f8fafc"
  text-secondary: "#cbd5e1"
  success: "#10b981"
  warning: "#f59e0b"
  danger: "#ef4444"
  info: "#0ea5e9"
typography:
  body:
    fontFamily: "'Inter', -apple-system, sans-serif"
    fontWeight: 400
  headline:
    fontFamily: "'Inter', -apple-system, sans-serif"
    fontWeight: 700
rounded:
  md: "0.375rem"
  lg: "0.5rem"
spacing:
  sm: "0.5rem"
  md: "1rem"
  lg: "1.5rem"
components:
  button-primary:
    backgroundColor: "{colors.primary}"
    textColor: "#ffffff"
    rounded: "{rounded.md}"
    padding: "0.625rem 1.25rem"
  surface:
    backgroundColor: "{colors.bg-secondary}"
    rounded: "{rounded.lg}"
---

# Design System: OPD Manager

## 1. Overview

**Creative North Star: "The Clinical Command Center"**

OPD Manager is built for high-stakes, fast-paced clinical environments. The aesthetic is extremely Restrained: deep, cool-toned neutrals anchor the interface, allowing medical data and actionable status indicators to stand out instantly. It rejects generic SaaS aesthetics—no playful blobs, no glassmorphism, no gradient text. It feels like a piece of high-end medical equipment: authoritative, precise, and instantly legible.

**Key Characteristics:**
- High information density with clear typographic hierarchy.
- Restrained color strategy: UI is monochromatic slate, reserving saturated colors strictly for state (success, warning, danger).
- Flat-by-default surfaces with functional elevation only when interacting.

## 2. Colors

The palette relies on tinted slate neutrals to reduce eye strain during long shifts, with vibrant semantic colors for instant status recognition.

### Primary
- **Clinical Blue** (#3b82f6): Used exclusively for primary actions (buttons) and active states. It should never be used decoratively.

### Neutral
- **Deep Slate Background** (#0f172a): The canvas. Deep enough to make surfaces pop without being pure `#000`.
- **Surface Slate** (#1e293b): The standard background for cards, tables, and panels.

### Semantic
- **Success Green** (#10b981): Completed tasks, available slots.
- **Warning Amber** (#f59e0b): Waiting patients, pending actions.
- **Danger Red** (#ef4444): Cancellations, critical errors.

**The Semantic Strictness Rule.** Saturated colors (red, green, amber) are reserved strictly for status indicators and badges. They may not be used as decorative background elements.

## 3. Typography

**Body Font:** Inter (with system-ui fallback)
**RTL Font:** Tajawal

**Character:** Inter provides the highly legible, geometric clarity required for dense medical data.

### Hierarchy
- **Headline** (700, 2rem): Page titles.
- **Title** (600, 1.25rem): Panel and card headers.
- **Body** (400, 1rem): Standard data rows and text. Max line length 75ch.
- **Label** (500, 0.875rem, uppercase tracking): Table headers and tiny metadata.

**The Functional Type Rule.** Typography scales must use fixed `rem` values, not fluid `clamp`. Clinical staff use consistent monitors and need predictable layouts, not scaling text.

## 4. Elevation

The system is flat by default to maximize data density, relying on borders (`1px solid var(--border-color)`) to define boundaries.

### Shadow Vocabulary
- **Resting:** No shadow.
- **Hover/Interactive:** (`var(--shadow-md)`) Used when hovering over clickable cards.
- **Floating/Modal:** (`var(--shadow-lg)`) Used for modals and dropdowns to strongly detach from the background.

**The Flat-By-Default Rule.** Surfaces are flat at rest. Shadows appear only as a response to state (hover, elevation, focus) or z-index isolation (modals).

## 5. Components

### Buttons
- **Shape:** Soft square (0.375rem radius).
- **Primary:** Clinical Blue background, white text.
- **Hover:** Exponential ease-out transition (`cubic-bezier(0.23, 1, 0.32, 1)`) to a deeper blue, no bouncing.

### Surfaces (Cards / Panels)
- **Corner Style:** 0.5rem radius.
- **Background:** Surface Slate.
- **Border:** 1px solid slate border.

## 6. Do's and Don'ts

### Do:
- **Do** cap paragraph line-lengths to 75 characters.
- **Do** use `margin-inline` and `padding-inline` logical properties to ensure perfect RTL layout flips for Arabic.
- **Do** use Skeleton states for loading instead of spinners inside content.

### Don't:
- **Don't** use `border-left` or `border-right` greater than 1px as a colored stripe on cards. Use a full border or badge instead.
- **Don't** use glassmorphism (backdrop blurs) as a default background. It is a consumer cliché.
- **Don't** use gradient text (`background-clip: text`).
- **Don't** reinvent standard affordances (like custom scrollbars or weird form controls) just for flavor.
