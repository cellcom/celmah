# SPA Base Path Strategy

## Current approach: Relative build + server-side absolute rewrite

### How it works

1. **Vite** builds with `base: './'` (relative paths in HTML)
   - Output: `<script src="./assets/index-abc.js">`, `<link href="./assets/index-abc.css">`
   - No custom Vite plugin needed

2. **Backend** (`ErrorResourceHandler.cs`) rewrites the HTML at serve time:
   - Injects `<meta name="celmah-root" content="...">` into `<head>` for JS to read
   - Rewrites `src="./` → `src="{celmahRoot}/"` and `href="./` → `href="{celmahRoot}/"`
   - `celmahRoot` comes from `context.GetCelmahRelativeRoot()` which includes `PathBase`

3. **JS** reads the meta tag for API calls:
   ```js
   const meta = document.querySelector('meta[name="celmah-root"]')
   const root = meta ? meta.content : '/celmah'
   ```

4. **Why server-side rewrite is needed:** Client-side routing means the browser can be at
   `/celmah/detail/123`, so `./assets/index.js` would resolve to `/celmah/detail/assets/index.js`
   (wrong). The server rewrites to absolute paths like `/celmah/assets/index.js` (correct).

5. **Reverse proxy support:** `GetCelmahRelativeRoot()` returns `PathBase + celmahPrefix`.
   If the app runs at `https://domain.com/aspnetapp` (PathBase = `/aspnetapp`) and Celmah
   is mapped to `/celmah`, assets resolve to `/aspnetapp/celmah/assets/index.js`. ✅

### Files involved

- `ui/vite.config.js` — `base: './'`, no custom plugin
- `ui/src/api.js` — reads `<meta name="celmah-root">`
- `ui/src/router/index.js` — reads same meta tag for Vue Router base
- `src/Celmah/Handlers/ErrorResourceHandler.cs` — injects meta tag + rewrites asset paths
- `src/Celmah/HttpContextExtensions.cs` — `GetCelmahRelativeRoot()` includes `PathBase`
- `ui/index.html` — clean HTML, no magic tokens

### What gets rewritten at runtime

For an app at root with `app.MapCelmah()` (prefix = `/celmah`):
```html
<!-- Built by Vite -->
<head>
  <title>Celmah</title>
  <script src="./assets/index-abc.js"></script>
  <link href="./assets/index-abc.css">
</head>

<!-- After server-side rewrite -->
<head>
  <meta name="celmah-root" content="/celmah">
  <title>Celmah</title>
  <script src="/celmah/assets/index-abc.js"></script>
  <link href="/celmah/assets/index-abc.css">
</head>
```

For an app behind reverse proxy (PathBase = `/aspnetapp`, prefix = `/celmah`):
```html
<head>
  <meta name="celmah-root" content="/aspnetapp/celmah">
  <title>Celmah</title>
  <script src="/aspnetapp/celmah/assets/index-abc.js"></script>
  <link href="/aspnetapp/celmah/assets/index-abc.css">
</head>
```

---

## Previous approach: Magic string replacement (CELMAH_ROOT)

### How it worked

1. Vite built with `base: 'CELMAH_ROOT'`
2. Custom Vite plugin (`celmahRootBasePlugin`) rewrote `/CELMAH_ROOT/...` → `CELMAH_ROOT/...` in HTML
3. Backend did `html.Replace("CELMAH_ROOT", actualPrefix)` at runtime
4. JS used `window.$celmah_root = "CELMAH_ROOT"` which got replaced

### Why it was fragile

- Coupled Vite build output to a backend magic string
- Custom Vite plugin was needed to fix the leading `/` issue
- Easy to break during renames (exactly what happened)
- Risk of accidental replacement if user content contained the magic string

### To switch back

1. `ui/vite.config.js` — `base: 'CELMAH_ROOT'`, restore custom plugin
2. `ui/index.html` — use `CELMAH_ROOT` in favicon links, add `window.$celmah_root = "CELMAH_ROOT"`
3. `ui/src/api.js` — use `window.$celmah_root`
4. `ui/src/router/index.js` — use `window.$celmah_root`
5. `src/Celmah/Handlers/ErrorResourceHandler.cs` — revert to `html.Replace("CELMAH_ROOT", root)`
