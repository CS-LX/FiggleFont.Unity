# Changelog

## Unreleased

- Keep invalid-font import diagnostics within the yielding test before checking expected logs.
- Use separate temporary asset paths per test and yield after cleanup to isolate import notifications.

## 0.1.0

- Import `.flf` files as Unity `TextAsset` resources with Figgle format validation.
- Preview arbitrary text in a UI Toolkit inspector with a monospaced font, size control, dimensions, and copy button.
- Show invalid font diagnostics while keeping the source available for repair.
- Support font sources in Assets and UPM packages.
- Use the consumer's `Figgle.dll`, installed through NuGetForUnity or copied manually.
