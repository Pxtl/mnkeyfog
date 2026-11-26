# AGENTS.md

See [CONTRIB.md](CONTRIB.md) for coding style guide(s). Read the CONTRIB.md and
any appropriate linked documents fully before making changes.

See [TODO.md](TODO.md) for the TODO list (if there is any, some tasks will be
controlled from other mechanisms).  Delete items from the TODO list when
complete.  Each major TODO heading should be implemented as its own PR with its
own branch from main.

# Dev Environment

Codebase is using dotnet 10.0.  Use `dotnet build` to compile, and `dotnet test`
to run unit test suite.

# Testing

Unit tests are in `tests`.

# Refactoring

Do not attempt to refactor or otherwise clean-up the code unless specifically
instructed to do so. Do not remove comments unless they are untrue.

# MnkeyFog.Model

Do not modify `MnkeyFog.Model` internals without maintainer approval; see
[CONTRIB.md](CONTRIB.md).

# Scratch space

Create any needed working files such as experiments within the `temp/` dir, which
is `.gitignore`d.  Create personal notes for future reference in `MEMORY.md`, which
is also gitignored.
