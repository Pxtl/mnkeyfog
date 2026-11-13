# Contributions

PRs are welcome, but may get rejected if not in line with my plans for the
project.  See [TODO.md](TODO.md) (if any) for things that are ready to work on,
[PLAN.md](PLAN.md) (if any) for longer-term plans that require discussion and
consideration before starting work.

## Agents

3rd-party agentic code (as in agentic code not made by "pxtl-clod") is accepted
if a human operator has fully reviewed and understands and can defend every line
of the change before submitting a PR. "Vibe coding" is prohibited. See
[AGENTS.md](AGENTS.md).

## Coding Standards

This document captures the dotnet coding style patterns used in this project.

Note to LLMs/Agents: Read the linked coding standards documents completely before
making any changes or actions relevant to them.

### Common

- Indent 4 spaces for C# or SQL source code, 2 spaces for Markdown, html, xml, json, or
  other document or serialization or configuration files.
- In files (`.md`, `.cs`, `.ts`, etc.) that practically support line length limits:
  - soft length limit at 80 char
  - hard limit at 120 char

### Project Structure

See [file-structure.md](doc/standards/file-structure.md).

### C# Code

See [c-sharp.md](doc/standards/c-sharp.md).

### GitHub

See [github.md](doc/standards/github.md).
