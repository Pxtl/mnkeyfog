# File Structure Standards

This document describes the file layout for dotnet-based repos.

## Directory Tree

Source files go in `src/{{ ProjName }}`, tests go in `tests/{{ TestProjName }}`,
there shall be a single `.slnx` file at the repo root, supplementary markdown
documents go in `doc/`, experiments go in the `.gitignore`d `temp/` dir.  If a
directory needs commentary that can't be associated to a single file (for
example contains files that are not practically commentable, such as images),
then createa directory-local `README.md`.

## Subdirectories

- In dotnet projects, directories shall correspond to namespaces.

## Files

- In dotnet projects, code files shall be named after the dominant class in the
  file.

## Docs

- Repo-standard markdown docs go at root in ALLCAPS. Additional docs go in
  `doc/` and are `kebab-cased.md`.

## .csproj files

Proj files should import `Standard.props` where possible, which will provide the
default C# compiler settings and style enforcement.