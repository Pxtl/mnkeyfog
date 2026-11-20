# MnkeyFog.Bsky.WorkerService

Worker host that runs the Bluesky bot as a background service. It reads its
settings from the `Bsky` configuration section (bound to `BskyBotOptions`).

## Providing credentials

The bot's `Handle` and `AppPassword` are **not** stored in `appsettings.json`
(see the comment in that file). Provide them at runtime so no secret is ever
committed. The standard host layers configuration providers and **later
providers win**, so any of these override `appsettings.json`:

1. **Environment variables** (typical for a server) — note `__` maps to `:`:
   ```bash
   export Bsky__Handle=mybot.bsky.social
   export Bsky__AppPassword=app-password-value
   ```
2. **User secrets** (local development, `Development` environment only):
   ```bash
   dotnet user-secrets set Bsky:Handle mybot.bsky.social
   dotnet user-secrets set Bsky:AppPassword app-password-value
   ```
3. **Command-line args** (one-off / debugging):
   ```bash
   dotnet run --project . -- --Bsky:Handle mybot.bsky.social --Bsky:AppPassword app-password-value
   ```

Full provider order, later wins:

`appsettings.json` → `appsettings.{Environment}.json` → user secrets
(Development) → environment variables → command-line args.

If `Handle`/`AppPassword` are missing they bind to null and login throws, so a
misconfigured worker fails fast at startup instead of silently misbehaving.

## Persistence

The bot's game-state persistence is a blob store (`IGameSessionStore`). The
registered implementation is chosen from the `Bsky:GamePersistenceDirectory`
setting:

- **Set** (e.g. `Bsky__GamePersistenceDirectory=/var/mnkeyfog/persist`) → a
  filesystem-backed store (`FileSystemGameSessionStore`), one JSON file per active
  game in that directory. Set it the same ways as credentials (env var
  `Bsky__GamePersistenceDirectory`, user secret `Bsky:GamePersistenceDirectory`,
  or `--Bsky:GamePersistenceDirectory <path>`).
- **Unset** → a no-op store (`NoOpGameSessionStore`); games run in memory only and
  are lost on restart.

A different backend (e.g. Azure Blob Storage) can be injected by registering a
different `IGameSessionStore` — no change to the bot or store call-sites.
