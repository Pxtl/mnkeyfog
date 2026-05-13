# kriegspiel-tictactoe

A simple command-line implementation of Zach Weinersmith's proposed game 'Kriegspiel Tic Tac Toe'.

see https://mastodon.social/@ZachWeinersmith/111890121393299096

Basically, the idea is that it's blind tic-tac-toe where you can only see your
opponent's spaces if you hit the same space.

I (Pxtl) got carried away gold-plating, so there are 3 notable features:

1. Arbitrary square board size.
2. Arbitrary player count. Players must be unique single-character names.
3. Both hotseat *and* file-based network multiplayer.

# Install and Run

This program is implemented in C# using dotnet core 10.

to install dotnet core 10 SDK on Windows:

> `winget install dotnet-sdk-10`

but possibly dotnet runtime 10 will be sufficient.

> `winget install dotnet-runtime-10`

to run it in basic gameplay (traditional tic-tac-toe but Kriegspiel, hotseat
mode).

> `dotnet run KriegspielTicTacToe`

to see other options for play, run

> `dotnet run KriegspielTicTacToe --?`

note that dotnet-script will intercept all other variations like `-h` or
`--help` or `-?`, WIP.  Alternately you can supply any garbage like 

> `dotnet run KriegspielTicTacToe --justshowmethehelp`

and it will insult you but show you the help.

```
Description:
  This is a simple command-line implementation of Zach Weinersmith's proposed game 'Kriegspiel Tic Tac Toe'

Usage:
  dotnet-script [options]

Options:
  -f, --file <file>        Path to the json file where gamestate is stored.  Will be resumed automatically if you kill
                           the game (ctrl-C).  Use a fileshare for network multiplayer. [default:
                           C:\Users\<yourname>\AppData\Roaming\KriegspielTicTacToe.json]
  -F, --force              Force a new game instead of loading the game at the gamestate file.  Will replace gamestate 
                           file.
  -p, --players <players>  Players mark characters.  Provide them space-separated, eg '-p A B C X Y Z' for a 6-player
                           game. [default: X|O]
  -r, --random             Randomize player order.
  -s, --size <size>        Board size.  Default is 3x3. [default: 3]
  -j, --join <join>        Join as given player char mark. Must match a mark in players list. Hotseat mode if not
                           provided.
  --version                Show version information
  -?, -h, --help           Show help and usage information
```

So, to start a simple 3-player hotseat game between Alice, Bob, and Carol on a 4x4 screen, the command would be

> `dotnet run KriegspielTicTacToe -p A B C -s 4`

Conversely, to start a multiplayer game with the default rules (2 players X and
O on a 3x3 board) on a fileshare named `\\kosmos\storage` with random
start-player, the command would be 

> `dotnet run KriegspielTicTacToe -f \\kosmos\storage\temp\ksttt.json -j X -r`

And then your friend (on another computer with similar access to `\\kosmos\storage\`) can join with 

> `dotnet run KriegspielTicTacToe -f \\kosmos\storage\temp\ksttt.json -j O`

any game-rule options you pass in when joining an existing game like size,
players, or randomization will be ignored.

# Contributing

I haven't thought that far ahead.

# Bugs

## If I do something crazy like have 3 players all running as player X, or join an online game in hotseat mode, the game is broken and bad

Don't do that.

## Other Bugs

Infinite.  I have no idea.  This was a one-evening hackathon.

# TODO

Zach's proposal was fleshed out a bit later, and has some things that aren't included in this game.

## Scoring

Zach's KTTT does not end when 3 in a row has been claimed.  Rather, a point is scored in that case.  The game *ends* when the board is full.