# 📐 App Style Guide

**Purpose:**  
This app prioritizes **functionality** and **fast navigation** with **compact layouts** that still maintain readability and breathing room.  
Design choices aim for a professional, modern look with a focus on a dark-mode-first experience and reusable UI components.

---

## 🎨 Colors (Dark Mode-first)

This project favors a dark-mode-first design. Use Tailwind v4 utilities tuned to a dark theme and prefer the reusable components in `src/CMS.Main/Components/Utilities` for consistent surfaces.

### Brand Palette
- **Primary (Interactive)**: `purple-500` / `purple-600` — Buttons, links, highlights (use `purple-600` for hover/active)
- **No Secondary Color**: The system intentionally avoids a separate secondary color to keep emphasis and interactions consistent; use shades of gray, white/near-white, or accent variants of purple when necessary.
- **Success**: `emerald-500` — Confirmations, success states
- **Warning**: `amber-500` — Cautions, pending actions
- **Error**: `rose-600` — Errors, destructive actions
- **Background**: `slate-900` / `slate-800` — App background and page surfaces
- **Surface / Panels**: `slate-800` / `slate-700` — Cards, panels, and elevated surfaces
- **Text Primary**: `slate-100` — Main readable text on dark background
- **Text Secondary**: `slate-400` — Secondary text and subdued labels

Gradients: tasteful gradients using purple tones (for example `from-purple-600 via-purple-500 to-purple-400`) are allowed to add depth to headers, cards, and interactive elements. Do not overuse gradients — reserve them for high-value surfaces and call-to-actions.

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

- Base (apply on the `<button>`): `inline-flex items-center justify-center rounded-full font-medium`
- Sizes:
    - Small: `text-xs px-2.5 py-1.5`
    - Medium: `text-sm px-3 py-2`
- Colors:
    - Primary: `bg-purple-500 text-white hover:bg-purple-600` (use gradients on hero CTAs when appropriate)
    - Danger: `bg-rose-600 text-white hover:bg-rose-700`
- **Icons:** Allowed and encouraged on buttons for clarity and visual cues.

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
- Base: `block w-full rounded-full border border-slate-700 bg-slate-800 shadow-sm focus:border-purple-500 focus:ring-purple-500`
- Sizes: Default `text-sm px-3 py-2`
- Disabled: `bg-slate-700 text-slate-500 cursor-not-allowed`
- **No icons on field labels.** Keep field labels clean and text-only for clarity and compactness.

### Cards / Panels
- Base: `rounded-xl border border-slate-700 bg-gradient-to-b from-slate-800 to-slate-800/90 shadow-sm` (or use `bg-slate-800` for very simple panels)
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
- Use **max-w-screen-lg** for main content width.
- Sidebars: min width `w-64` (256px).
- Content sections separated by `mb-4` (16px).
- Avoid full-width text blocks; keep line length readable.

---

## ### Icons
- Use **Google Material Symbols** for all icons in the app.
- Add the `icons-default` class to every icon for consistent sizing and alignment.
- Add any new icon names to the `iconNames` array in `App.razor` for consistency and easy management.
- Icons are allowed and encouraged on buttons for clarity and visual cues.

Component-first rule: Prefer using the components located in `src/CMS.Main/Components/Utilities` (for buttons, layout, tables, icons etc.). If the component you need does not exist in that folder, the developer should first ask the project owner whether to create the new component or to fall back to Tailwind utility classes directly. This keeps UI consistent and maintainable.

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
<button class="bg-purple-500 hover:bg-purple-600 text-white text-sm font-medium px-3 py-2 rounded-full">
    <span class="material-symbols-outlined icons-default text-base mr-2">save</span>
    Save Changes
</button>
<!-- Good: icon on button -->

<label class="text-xs uppercase tracking-wide">Project Name</label>
<!-- Good: no icon on field label -->
```

If a developer can't find the matching component in `src/CMS.Main/Components/Utilities`, they must ask whether to:
- Create the component in `src/CMS.Main/Components/Utilities` (preferred), or
- Use Tailwind utility classes inline for a one-off (only with approval).

Small follow-up: If you'd like, I can create common layout components (VerticalStack/HorizontalStack) and a small `Button` wrapper that maps to the new color tokens. Tell me if you want me to proceed and what default props you'd like (gap, alignment, responsive behavior).
