# mnkeyfog

A command-line implementation of kriegspiel and/or synchronous [m,n,k
games](https://en.wikipedia.org/wiki/M,n,k-game) such as Zach Weinersmith's
proposed game 'Kriegspiel Tic Tac Toe'.

see https://mastodon.social/@ZachWeinersmith/111890121393299096

Basically, the idea is that it's blind tic-tac-toe where you can only see your
opponent's spaces if you hit the same space.

I (Pxtl) got carried away gold-plating, so this is now a generalized tool for
command-line kriegspiel and synchronous m,n,k games.

1. Arbitrary square board size.
2. Arbitrary player count. Players must be unique single-character names.
3. Both hotseat *and* file-based network multiplayer.

# Install and Run

This program is implemented in C# using dotnet core 10.

to install dotnet core 10 SDK on Windows:

> `winget install dotnet-sdk-10`

but possibly dotnet runtime 10 will be sufficient.

> `winget install dotnet-runtime-10`

to run it in basic gameplay from source (Weinersmith's Kriegspiel Tic-Tac-Toe, hotseat
mode).

> `dotnet run --project ./mnkeyfog/src/MnkeyFog -- game mnkeyfog`

```
Description:
  Start a new game using a pre-defined game template.

Usage:
  MnkeyFog game [command] [options]

Options:
  -p, --players <players>  Players mark characters.  Provide them space-separated, eg '-p A B C X Y Z' for a 6-player game. [default: X|O]
  -r, --random             Randomize player order.
  -?, -h, --help           Show help and usage information
  -f, --file <file>        Path to the json file where gamestate is stored.  Will be resumed automatically if you kill the game (ctrl-C). Use a fileshare for network multiplayer. [default: /home/pxtl/.config/MnkeyFog.json]
  -j, --join <join>        Join as given player char mark. Must match a mark in players list. Hotseat mode if not provided.

Commands:
  mnkeyfog  Zach Weinersmith's Kriegspiel Tic-Tac-Toe.
  tictactoe             Basic simple tic-tac-toe.
```

to see the other commands to launch the game for play, run

> `dotnet run --project ./mnkeyfog/src/MnkeyFog -- -?`

Command `game` is for predefined gametypes, `custom` for custom game, and `load`
to load a previous game.  Each command can be called with a `-?` parameter to
see its respective options.

Alternately, if you build the dotnet executable file and place it so you can
call it from elsewhere, you can skip the `dotnet run` formality and just run it
directly.

So, to start a simple 3-player hotseat game between Alice, Bob, and Carol on a
4x4 screen, the command would be

> `mnkeyfog custom -p A B C --boards 1 --size 4 --kriegspiel`

Conversely, to start a multiplayer game with the default rules (2 players X and
O on a 3x3 board) on a fileshare named `\\kosmos\storage` with random
start-player, the command would be 

> `mnkeyfog -f \\kosmos\storage\temp\ksttt.json -j X -r`

And then your friend (on another computer with similar access to `\\kosmos\storage\`) can join with 

> `mnkeyfog -f \\kosmos\storage\temp\ksttt.json -j O`

any game-rule options you pass in when joining an existing game like size,
players, or randomization will be ignored.

# Synchronous Mode

Use the `--synchronous` flag to enable synchronous mode.

In synchronous mode:
- All moves in a round are buffered until every player in the round takes their turn
- If two players move to the same square in the same round, it becomes an impasse marker (█)
- Impasse markers cannot be used to win and block all players from that square

See [Synchronous Mode Usage](doc/sync-mode-usage.md)

# Contributing

See [CONTRIB.md](CONTRIB.md)

# Bugs

## If I do something crazy like have 3 players all running as player X, or join an online game in hotseat mode, the game is broken and bad

Don't do that.

## Other Bugs

Infinite.  I have no idea.  Half of this was a one-evening hackathon.  The other half was experimenting with a pretty weak local-hosted AI.
