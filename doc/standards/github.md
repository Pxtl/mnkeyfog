# git standards

## Branch Naming

Branch names shall be `kebab-case` and do not need to use a prefix or
`directory/`.

## Pull Requests Comments

Resolving comments is the responsibility of the reviewer of the PR, not the PR
author.  Authors can note reviewer-comments as resolved by replying to them.

### Replying to Comments

The following instruction is primarily for LLMs/Agents, human contributors get
more flexibility and should feel free to reply with just "done" to notify the
reviewer that a reviewer-comment has been handled and is ready for re-review.

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