# Changelog

All notable changes to this project are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.1] - 2026-07-29

### Changed
- Added NuGet, downloads, and MIT license badges to the README, and shipped the
  updated README to the nuget.org package page. No functional or API changes.

## [1.0.0] - 2026-07-28

### Added
- Initial release of `DualListbox<TItem>` — a generic, accessible dual-listbox
  (pick-list) component for Blazor.
- Move items via buttons, double-click, and keyboard (`Enter`); move-all support.
- Multi-select with `Ctrl`/`Cmd`+click, `Shift`+click range, and `Ctrl`/`Cmd`+`A`.
- Keyboard navigation (`↑`/`↓`/`Home`/`End`, `Space` to toggle, `Enter` to move).
- Two-way binding for `Source` and `Selected`, with `OnChanged` / `OnMoved` callbacks.
- Optional per-list filtering, custom `ItemTemplate`, headers, and button styling hooks.
- Accessibility: `role="listbox"`/`role="option"`, `aria-multiselectable`,
  `aria-selected`, and `aria-activedescendant` focus management.
- CSS isolation with `--dl-*` custom properties for theming; no CSS framework dependency.
- Multi-targets `net6.0`, `net8.0`, and `net10.0`.

[1.0.1]: https://github.com/olufemioyedepo/blazor-dual-listbox/releases/tag/v1.0.1
[1.0.0]: https://github.com/olufemioyedepo/blazor-dual-listbox/releases/tag/v1.0.0
