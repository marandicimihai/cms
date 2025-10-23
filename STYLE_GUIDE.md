# 📐 App Style Guide

**Purpose:**  
This app prioritizes **functionality** and **fast navigation** with **compact layouts** that maintain readability and breathing room.  
Design choices aim for a professional, modern look with a dark-mode-first experience and reusable UI components.

---

## 🎨 Colors (Dark Mode-first)

This project favors a dark-mode-first design. All components use Tailwind utilities tuned to a dark theme. Prefer the reusable components in `src/CMS.Main/Components/Utilities` for consistent surfaces.

### Brand Palette
- **Primary (Interactive)**: `purple-500` — Buttons, links, highlights, focus rings
- **Purple Variants**: `purple-400` (lighter hover), `purple-500/40` (semi-transparent rings), `purple-500/20` (gradient overlays)
- **Danger**: `rose-600` — Errors, destructive actions, delete buttons
- **Neutral**: `neutral-500` — Neutral variant for secondary actions
- **Success**: `emerald-500` / `emerald-600` — Confirmations, success states (in status messages)
- **Background**: `neutral-900` — Main app background (can use opacity variants like `neutral-900/80` for semi-transparent overlays)
- **Surface / Panels**: `neutral-800` / `neutral-700` — Cards, panels, elevated surfaces, borders
- **Text Primary**: `neutral-100` — Main readable text on dark background
- **Text Secondary**: `neutral-400` / `neutral-300` — Secondary text, subdued labels, muted content
- **Text Disabled**: `neutral-500` — Disabled input text
- **Borders**: `neutral-700` / `neutral-800` / `neutral-600` — Component borders and dividers

### Gradient Usage
Gradients are used sparingly for depth and interactive feedback:
- **Hover gradients**: `hover:bg-gradient-to-tl hover:from-purple-500/20 hover:to-transparent` — Subtle overlay on interactive elements
- **Panel backgrounds**: `bg-gradient-to-b from-neutral-800 to-neutral-800/80` — Adds depth to cards/papers
- **Backdrop overlays**: `bg-black/40 backdrop-blur` — Modal/dialog backgrounds

---

## 🖋 Typography

Text should be **clear and unintrusive**.

| Element          | Class Example                      | Notes |
|------------------|------------------------------------|-------|
| App Title / H1   | `text-2xl font-semibold`           | Minimal emphasis |
| Section Titles   | `text-lg font-medium`              | Compact headers |
| Body Text        | `text-sm`                          | Default text size |
| Labels           | `text-xs uppercase tracking-wide`  | Use for form and table labels |

---

## 📏 Spacing & Layout

We use a **tight but breathable** spacing scale. Stick to Tailwind’s scale: `1` = `0.25rem` (4px).

- **Component padding**: `p-2` (8px) to `p-4` (16px)
- **Element gaps**:
    - Small: `gap-2` (8px)
    - Medium: `gap-4` (16px)
- **Form controls**:
    - Height: `h-9` (36px) or `h-10` (40px) for better touch targets
    - Input padding: `px-3 py-2`
- **Card / Panel padding**: `p-4`
- **Table cell padding**: `px-3 py-2`

Built-in stack components: Prefer using reusable layout components (e.g. `VerticalStack`, `HorizontalStack`, `GridStack`) from `src/CMS.Main/Components/Utilities/Layout` (or create them when missing). These stack components encapsulate spacing and responsive behavior. If a needed layout component does not exist, the developer should first ask whether to create a component or to use Tailwind utility classes directly.

---

## 🧩 Components

### Buttons
- Preferred: apply Tailwind utility classes directly on the HTML element. This keeps classnames explicit and ensures Tailwind's JIT picks up the utilities at build time.

- Base (apply on the `<button>`): `inline-flex items-center justify-center rounded-full font-medium text-neutral-100`
- Core styling (all buttons):
    - Initial ring state: `ring-0 ring-purple-500/0` (or `ring-rose-600/0` for danger)
    - Hover gradient: `hover:bg-gradient-to-tl hover:from-purple-500/20 hover:to-transparent` (creates subtle gradient overlay)
    - Hover ring: `hover:ring-1 hover:ring-purple-500/40` (animated ring appearance)
    - Focus: `focus:outline-none focus:ring-2 focus:ring-purple-500 focus:ring-offset-2 focus:ring-offset-neutral-900`
    - Disabled: `disabled:opacity-50 disabled:cursor-not-allowed`
    - Transition: `transition-all duration-300`
- Sizes:
    - Small: `text-xs px-2.5 py-1.5`
    - Medium: `text-sm px-3 py-2`
- Icon-only buttons (IconButton):
    - Small: `w-8 h-8 p-1.5` with `text-2xl` icon
    - Medium: `w-10 h-10 p-2` with `text-2xl` icon
- Variants:
    - Primary: Uses purple ring/gradient colors (as shown above)
    - Danger: Replace purple with rose-600 in ring and gradient classes
- **Icons:** Allowed and encouraged on buttons for clarity and visual cues. Standard icon sizing is `text-base` with `mr-2` spacing when paired with text.
- **Link Buttons:** When buttons require navigation (href), use the LinkButton component instead of a standard button.

### Navigation Links (SideBarButton pattern)
- Base styling for sidebar navigation items:
    - Container: `flex items-center gap-2 p-2 min-h-10 text-sm font-medium rounded-full text-neutral-100 w-full`
    - Ring setup: `ring-0 ring-purple-500/0`
    - Hover state: `hover:bg-gradient-to-tl hover:from-purple-500/20 hover:to-transparent hover:ring-1 hover:ring-purple-500/40`
    - Active state: `bg-gradient-to-tl from-purple-500/20 to-transparent ring-1 ring-purple-500/40`
    - Transition: `transition-all duration-300`
- Icons in nav links use default sizing with `gap-2` spacing from text
- Text content should use `whitespace-nowrap` to prevent wrapping in sidebar

Notes on conditional classes and C# code:
- Prefer literal Tailwind classes in markup whenever possible. Example (good):

```html
<button class="inline-flex items-center justify-center rounded-full font-medium bg-purple-500 text-white hover:bg-purple-600 text-sm px-3 py-2">Save</button>
```

- If conditional sizing or small sets of variations are required, keep the conditional logic in C# but map to literal class strings (so Tailwind can see them). Example (acceptable):

```csharp
private string SizeClass => Size switch
{
    ButtonSize.Small => "text-xs px-2.5 py-1.5",
    ButtonSize.Medium => "text-sm px-3 py-2",
    _ => "text-sm px-3 py-2"
};
```

- Avoid building class names dynamically via string interpolation or concatenation in C# (for example `var classes = $"gap-{Gap} p-2";`) because Tailwind's JIT won't reliably include those in the generated CSS. If you must support a wide dynamic range, add a narrowly scoped safelist to `tailwind.config.js` and document it in the PR.

Component-first rule still applies: prefer using components from `src/CMS.Main/Components/Utilities` but when a one-off is needed, use inline Tailwind utilities in the HTML as shown above.


### Inputs

**Text Inputs:**
- Base: `block w-full rounded-full border border-neutral-600 bg-neutral-700 shadow-sm focus:border-purple-500 focus:ring-purple-500 text-neutral-100`
- Sizes: Default `text-sm px-4 py-3`
- Disabled: `bg-neutral-700 text-neutral-500 cursor-not-allowed`
- **No icons on field labels.** Keep field labels clean and text-only for clarity and compactness.

**Checkboxes:**
- **Global styling applied automatically** - No need to add classes to basic checkboxes
- Default size: `h-5 w-5`
- Checked state: Purple (`bg-purple-500 border-purple-500`)
- Focus: Purple ring with offset for dark backgrounds
- Hover: Lighter purple border (`border-purple-400`)
- Usage: `<input type="checkbox" />` or `<InputCheckbox />`
- For custom layouts, wrap in label: `<label class="flex items-center gap-2"><input type="checkbox" /><span>Label text</span></label>`

**Select Inputs:**
- **Global styling applied automatically** - No need to add classes to basic selects
- Base: Full-width, rounded-full, dark-mode styled with custom dropdown arrow
- Background: `neutral-700` with `neutral-600` border
- Focus: Purple ring (`focus:ring-purple-500`) with offset
- Hover: Lighter purple border (`border-purple-400`)
- Custom SVG arrow icon (neutral-400 color)
- Usage: `<select>...</select>` or `<InputSelect>`
- Disabled state automatically styled with reduced opacity

### Cards / Panels
- Base: `rounded-xl border border-neutral-800 bg-gradient-to-b from-neutral-800 to-neutral-800/90 shadow-sm` (or use `bg-neutral-800` for very simple panels)
- Padding: `p-4`
- Compact layout: reduce padding to `p-3`

When a custom surface style is needed prefer adding or reusing a component in `src/CMS.Main/Components/Utilities/Layout` or `.../Paper.razor` rather than sprinkling custom utility classes. If the needed component is missing, ask whether you'd like the component created or if Tailwind classes should be used inline.

---

## ♿ Accessibility Rules
- All interactive elements **must** have `aria-label` or visible text.
- Ensure sufficient color contrast (use Tailwind’s `text-*` colors against `bg-*` for AA/AAA where possible). Dark-mode foregrounds should meet contrast against the chosen dark surfaces.
- Focus states should always be visible (Tailwind's `focus:ring-*` utilities).

---

## 💡 Layout Principles
- Main content should take all available width and it will be on a solid background so it doesn't need any sort of separation from the background when styling.
- Sidebars: 
    - Collapsed width: `w-14` (56px)
    - Expanded width: `w-64` (256px)
    - Background: `bg-neutral-900/80` with `border-r border-neutral-800`
    - Use smooth transitions: `transition-all duration-300` for collapse/expand animations
    - Fixed positioning: `fixed left-0 top-0 h-full` with appropriate z-index
- Content sections separated by `mb-4` (16px).
- Avoid full-width text blocks; keep line length readable.

---

## ### Icons
- Use **Google Material Symbols** for all icons in the app.
- Add the `icons-default` class to every icon for consistent sizing and alignment.
- Add any new icon names to the `iconNames` array in `App.razor` for consistency and easy management.
- Icons are allowed and encouraged on buttons for clarity and visual cues.

Component-first rule: Prefer using the components located in `src/CMS.Main/Components/Utilities` (for buttons, layout, tables, icons etc.). If the component you need does not exist in that folder, the developer should first ask the project owner whether to create the new component or to fall back to Tailwind utility classes directly. This keeps UI consistent and maintainable.

- **Icon Component:** Where icons are required, use the Icon component for consistency.

---

## ⚠️ Tailwind class generation and C# code

Avoid generating Tailwind classes inside C# code blocks (for example inside `@code { }` in a Razor component) using string concatenation or interpolation like:

```csharp
// Bad - Tailwind will likely not generate `gap-1..gap-8` at build time
var classes = $"gap-{Gap} p-2";
```

Why: Tailwind's JIT generation scans source files at build time for literal class names and does not reliably pick up classes that are constructed dynamically at runtime from C# string interpolation or concatenation. That means the runtime-generated classes will not be present in the generated CSS and styling will silently fail.

Recommended alternatives:
- Use explicit class mappings in C#: expose an enum or small set of allowed values and map them to classes in code so the literal classes appear in source and Tailwind picks them up. Example:

```csharp
// AllowedGap.cs
public enum AllowedGap { G0, G1, G2, G4 }

// In Razor
@code {
    private string GapClass => AllowedGap switch
    {
        AllowedGap.G0 => "gap-0",
        AllowedGap.G1 => "gap-1",
        AllowedGap.G2 => "gap-2",
        AllowedGap.G4 => "gap-4",
        _ => "gap-4"
    };
}

<div class="flex flex-col @GapClass">...</div>
```

- If truly dynamic classes are unavoidable, ensure Tailwind generates them by adding a safelist to your Tailwind config that covers the range of classes you may need (for example `gap-0`..`gap-12`). Keep the safelist as small and specific as possible to avoid bloat.

Example safelist snippet (tailwind.config.js):

```js
module.exports = {
    // ...existing config
    safelist: [
        'gap-0','gap-1','gap-2','gap-3','gap-4','gap-5','gap-6','gap-7','gap-8'
    ]
}
```

Document and review any safelist additions in a PR so the team knows which dynamic classes are allowed. Prefer explicit mappings over safelists when feasible.


## ✅ Example Usage

```html
<!-- Primary Button with Icon -->
<button class="inline-flex items-center justify-center rounded-full font-medium text-neutral-100 ring-0 ring-purple-500/0 hover:bg-gradient-to-tl hover:from-purple-500/20 hover:to-transparent hover:ring-1 hover:ring-purple-500/40 focus:outline-none focus:ring-2 focus:ring-purple-500 focus:ring-offset-2 focus:ring-offset-neutral-900 disabled:opacity-50 disabled:cursor-not-allowed transition-all duration-300 text-sm px-3 py-2">
    <span class="material-symbols-outlined icons-default text-base mr-2">save</span>
    Save Changes
</button>
<!-- Good: icon on button with proper spacing and gradient hover effect -->

<!-- Icon-only Button (Medium) -->
<button class="inline-flex items-center justify-center rounded-full text-neutral-100 ring-0 ring-purple-500/0 hover:bg-gradient-to-tl hover:from-purple-500/20 hover:to-transparent hover:ring-1 hover:ring-purple-500/40 focus:outline-none focus:ring-2 focus:ring-purple-500 focus:ring-offset-2 focus:ring-offset-neutral-900 disabled:opacity-50 disabled:cursor-not-allowed transition-all duration-300 w-10 h-10 p-2">
    <span class="material-symbols-outlined text-2xl">add</span>
</button>
<!-- Good: icon-only button with fixed dimensions -->

<!-- Text Input with Label -->
<div class="mb-4">
    <label for="project-name" class="block text-xs font-medium uppercase tracking-wide text-neutral-400 mb-1">Project Name</label>
    <input type="text" id="project-name" class="block w-full rounded-full border border-neutral-600 bg-neutral-700 shadow-sm text-sm px-3 py-2 focus:border-purple-500 focus:ring-purple-500 text-neutral-100" placeholder="Enter name" />
</div>
<!-- Good: no icon on field label, proper spacing -->

<!-- Checkbox (Global Styles Applied) -->
<label class="flex items-center gap-2">
    <input type="checkbox" />
    <span class="text-sm text-neutral-100">Enable feature</span>
</label>
<!-- Good: No custom classes needed for checkbox, automatically styled -->

<!-- Select Input (Global Styles Applied) -->
<div class="mb-4">
    <label for="status" class="block text-xs font-medium uppercase tracking-wide text-neutral-400 mb-1">Status</label>
    <select id="status">
        <option>Active</option>
        <option>Inactive</option>
        <option>Pending</option>
    </select>
</div>
<!-- Good: No custom classes needed for select, automatically styled with custom arrow -->
```

If a developer can't find the matching component in `src/CMS.Main/Components/Utilities`, they must ask whether to:
- Create the component in `src/CMS.Main/Components/Utilities` (preferred), or
- Use Tailwind utility classes inline for a one-off (only with approval).

Small follow-up: If you'd like, I can create common layout components (VerticalStack/HorizontalStack) and a small `Button` wrapper that maps to the new color tokens. Tell me if you want me to proceed and what default props you'd like (gap, alignment, responsive behavior).
