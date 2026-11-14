# git standards

## Branch Naming

Branch names shall be `kebab-case` and do not need to use a prefix or
`directory/`.

## Pull Requests Comments

The following instruction is primarily for LLMs/Agents, human contributors get
more flexibility.

When handling pull request comments, they shall be replied to. If a particular
commit is relevant to the reply (such as "this issue is resolved in commit X"),
include a commit-hash, file(s), and line number(s) in the reply, either as a
deep-link or as text or as a combination thereof.

All non-resolved comments having replies is the Definition Of Done for PR
comments.

## History

Clean history is not necessary in PRs, as long as commit messages are reasonably
informative.  Linearizing the history is appreciated.

LLMs/Agents should include model metadata (provider URL and model ID) where
available in commit message body. "pi coding agent" agents can use
[pi-introspect](https://pi.dev/packages/pi-introspect) to obtain this info.