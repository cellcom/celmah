# Elmah.AspNetCore UI Modernization

## Overview

The Elmah SPA frontend was migrated from a legacy Vue 2 / Vue CLI stack to a modern **Vue 3 + Vite** stack built entirely with **Bun**, eliminating the Node.js runtime dependency. The goal was faster builds, fewer dependencies, smaller output, and a clean modern codebase.

---

## What Changed

### Build Toolchain

| Aspect | Before | After |
|--------|--------|-------|
| Runtime | Node.js | **Bun** |
| Bundler | Vue CLI (Webpack) | **Vite 8** |
| Transpiler | Babel | None (Vite handles it) |
| Linter | ESLint + Prettier | Removed (not needed for this project) |
| Config files | `babel.config.js`, `vue.config.js` | `vite.config.js` |
| Build command | `cross-env NODE_OPTIONS=--openssl-legacy-provider NODE_ENV=production vue-cli-service build` | `bun run build` |

The old build required the `--openssl-legacy-provider` workaround due to an outdated Webpack version. The new Vite build has no workarounds.

### Framework Migration

| Aspect | Before | After |
|--------|--------|-------|
| Vue | Vue 2 (`new Vue()`, Options API) | **Vue 3** (`<script setup>`, Composition API) |
| Router | vue-router 3 | **vue-router 5** |
| State | Vuex 3 | **Pinia 3** |
| Entry HTML | `public/index.html` | `index.html` (Vite root) |

### Dependency Replacements

| Old Dependency | New Solution | Reason |
|----------------|--------------|--------|
| `bootstrap-vue` (heavy JS component library) | **Bootstrap 5** CSS only + native HTML components | No need for a Vue wrapper; tabs, modals, etc. are trivial with native Bootstrap classes |
| `@fortawesome/vue-fontawesome` + `@fortawesome/free-solid-svg-icons` | **lucide-vue-next** | Tree-shakeable, lighter, modern icon library |
| `vue-flag-icon` + ~500 country flag SVG files in `wwwroot/img/` | **Emoji flags** via `String.fromCodePoint()` | Eliminates hundreds of static asset files |
| `vue-highlight.js` + `vue-hljs-with-line-number` + `vue-highlightjs` | Custom **HighlightCode** component + `highlight.js` (selective languages) | Only registers csharp, json, sql, xml — smaller bundle |
| `vue-moment` + `moment.js` | **Native JS** date formatting | Removes the massive moment.js dependency |
| `typeface-roboto` + 40 font files in `wwwroot/fonts/` | **System font stack** (`-apple-system, BlinkMacSystemFont, Segoe UI, ...`) | No custom web fonts needed for an error log viewer |
| `vue-async-computed` | Removed | Not needed with Vue 3 Composition API (`computed`, `watch`) |
| `core-js` | Removed | Vue 3 / Vite handle polyfills as needed |
| `dateformat` | Native `Date` methods | Trivial formatting, no library needed |
| OS/Browser SVG icons (Windows, Linux, Mac, iPhone, Android, Chrome) | **Inline SVGs** in the component | No icon pack needed — these are static brand logos |

### Toast Notifications

The `bootstrap-vue` toast system (`this.$bvToast.toast(...)`) was replaced with a lightweight custom implementation:

- `src/components/toast-service.js` — reactive toast state
- `src/components/ToastContainer.vue` — renders Bootstrap 5 toasts

Usage: `showToast('message', 'variant')` from anywhere in the app.

### Filter Modal

The `bootstrap-vue` modal (`<b-modal>`) was replaced with a native Bootstrap 5 modal using standard HTML:

```html
<div v-if="showModal" class="modal d-block" @click.self="close">
  <div class="modal-dialog modal-lg modal-dialog-centered">
    <div class="modal-content">...</div>
  </div>
</div>
```

### Tabs

The `<b-tabs>` / `<b-tab>` components were replaced with native Bootstrap nav tabs:

```html
<ul class="nav nav-tabs">
  <li class="nav-item" v-for="tab in visibleTabs">
    <a class="nav-link" :class="{ active: selectedTab === idx }" @click.prevent="selectedTab = idx">...</a>
  </li>
</ul>
```

Tab visibility is computed dynamically from the error data (sources, stack trace, body, logs, SQL, etc.).

### ELMAH_ROOT Path Handling

The ASP.NET middleware performs a text replacement of the literal string `ELMAH_ROOT` in `index.html` with the actual configured path (e.g., `/elmah`). The original Vue CLI build generated paths like `ELMAH_ROOT/css/...` (no leading slash), which after replacement became `/elmah/css/...` — correct.

Vite always prepends `/` to the base path, producing `/ELMAH_ROOT/assets/...`. After replacement this would become `//elmah/assets/...` — a broken protocol-relative URL.

**Solution**: A custom Vite plugin (`elmah-root-base`) in `vite.config.js` that post-processes the HTML output to strip the leading `/`:

```js
function elmahRootBasePlugin() {
  return {
    name: 'elmah-root-base',
    transformIndexHtml: {
      order: 'post',
      handler(html) {
        return html.replace(/(src|href)="\/ELMAH_ROOT\//g, '$1="ELMAH_ROOT/')
      },
    },
  }
}
```

The `window.$elmah_root = "ELMAH_ROOT"` line is left as-is (no leading `/`), so it gets replaced to `"/elmah"` which is correct for the Vue router base.

### Favicon

The default Vue favicon was replaced with a custom Elmah favicon:

- **`favicon.svg`** — scalable vector icon: dark rounded square with a bold red "E" and yellow notification dot
- **`favicon.ico`** — 32×32 raster fallback generated via a Node.js script

Both are placed in `public/` and automatically copied to `wwwroot/` by Vite.

---

## Dependency Count

| | Before | After |
|---|---|---|
| **Runtime dependencies** | 16 | **7** |
| **Dev dependencies** | 11 | **3** |
| **Total declared** | 27 | **10** |
| **Installed packages** | ~hundreds | **~69** |

### Current `package.json`

```json
{
  "dependencies": {
    "vue": "^3.5.13",
    "vue-router": "^5.0.4",
    "pinia": "^3.0.2",
    "axios": "^1.8.4",
    "bootstrap": "^5.3.5",
    "lucide-vue-next": "^1.0.0",
    "highlight.js": "^11.11.1"
  },
  "devDependencies": {
    "@vitejs/plugin-vue": "^6.0.5",
    "vite": "^8.0.7",
    "sass": "^1.87.0"
  }
}
```

---

## Output Size

| Metric | Before | After |
|--------|--------|-------|
| **wwwroot total** | Several MB (fonts, flag SVGs, JS, CSS, maps) | **~444 KB** |
| **JS bundle** | ~large (chunk-vendors + index) | **~206 KB** |
| **CSS bundle** | ~large (chunk-vendors + index) | **~241 KB** |
| **Static assets** | ~500 flag SVGs + 40 font files | **2 files** (favicon.ico + favicon.svg) |

---

## Build Performance

| Metric | Before | After |
|--------|--------|-------|
| Build time | Slow (Webpack, required OpenSSL legacy flag) | **~10–25 seconds** (Vite/Rolldown) |
| Warning/Error free | Required `NODE_OPTIONS=--openssl-legacy-provider` | **Clean** (only an informational note about `ELMAH_ROOT` base) |

---

## File Structure

```
ui/
├── index.html                  # Vite entry (was public/index.html)
├── package.json                # Minimal deps, "type": "module"
├── vite.config.js              # Vite config + ELMAH_ROOT plugin
├── public/
│   ├── favicon.ico             # 32x32 raster favicon
│   └── favicon.svg             # Scalable vector favicon
└── src/
    ├── main.js                 # App bootstrap (createApp, Pinia, Router)
    ├── App.vue                 # Root component (navbar, search, filter trigger)
    ├── api.js                  # Axios instance + getElmahRoot helper
    ├── store.js                # Pinia store (searchText, filterTags)
    ├── utils.js                # Helpers (flags, time formatting, highlight.js setup)
    ├── router/
    │   └── index.js            # Vue Router config (history mode, ELMAH_ROOT base)
    ├── styles/
    │   ├── main.scss           # Global styles (filter-link, pre)
    │   └── variables.scss      # SCSS variables (colors, shared)
    ├── components/
    │   ├── ErrorDetail.vue     # Error detail panel (inline OS/Browser SVGs)
    │   ├── ErrorListFilter.vue # Filter tags + modal (replaces b-modal)
    │   ├── ErrorListItem.vue   # Single error row in list
    │   ├── ErrorsList.vue      # Scrollable error list with infinite scroll
    │   ├── ErrorsView.vue      # Split layout (list + detail)
    │   ├── HighlightCode.vue   # Lightweight highlight.js wrapper
    │   ├── ToastContainer.vue  # Bootstrap 5 toast renderer
    │   └── toast-service.js    # Reactive toast state
    └── views/
        ├── About.vue           # About page
        ├── Detail.vue          # Standalone error detail (direct link)
        ├── Errors.vue          # Errors page wrapper
        └── List.vue            # Main list view
```

---

## Build Commands

```bash
# Install dependencies
bun install

# Development server
bun run dev

# Production build → outputs to ../src/Elmah.AspNetCore/wwwroot/
bun run build

# Preview production build
bun run preview
```

The `build.ps1` script at the repo root runs `bun install && bun run build` followed by `dotnet build`.

---

## Sass Migration

All `@import` statements were migrated to `@use` to avoid Dart Sass 3.0 deprecation warnings:

```scss
// Before
@import '../styles/variables';

// After
@use '../styles/variables' as *;
```

The `darken()` function was replaced with `color.adjust()` from `sass:color`:

```scss
// Before
color: darken($filter-link-color, 10%);

// After
@use 'sass:color';
color: color.adjust($filter-link-color, $lightness: -10%);
```
