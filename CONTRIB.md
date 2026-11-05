# Contributions

PRs are welcome, but may get rejected if not in line with my plans for the
project.  See [TODO.md](TODO.md) for things that are ready to work on,
[PLAN.md](PLAN.md) for longer-term plans that require discussion and
consideration before starting work.

# Coding Standards

This document captures the coding style patterns used in this project.

## General

- **Global usings**: Use `global using` directives at the file top for common namespaces (`System`, `System.Linq`, `System.Collections.Generic`, `System.IO`, etc.). See `.NETCoreApp,Version=v10.0.AssemblyAttributes.cs`.
- **File-level usings**: Place `using` directives before the namespace declaration, before class definitions.
- **OneOf types**: Use the `OneOf` library for result types (`ActionQueuedSuccessfully`, `NotFound`, `AlreadyPlayed`, `BoardIsDone`).

## Indentation

- Use **spaces** for indentation (not tabs).

## Braces (OTBS - Open on Same Line)

- Open braces on the same line as the declaration, closing brace on its own line.
- For methods:

    ```csharp
    public void MethodName(...) {
        // body
    }
    ```

- For property accessors / initializers:

    ```csharp
    public SomeType Prop {get;set;} = defaultValue;
    public SomeType Prop2 {get;init;}
    public int SingleValueProp => expression;
    ```

- If a single value property has has non-trivial complexity:

    ```csharp
    public int SingleValueProp 
        => longAndComplicatedExpression;
    ```

- For records:

    ```csharp
    public record StructType(int A, int B) {}


    public record ClassType {
    #region constructors
        public ClassType() {}
    #endregion
    };
    ```

## Linebreaks

- Break logical statements on new lines if they're too long.
- Use whitespace after commas in multi-parameter calls.
- Soft length limit at 80 char, hard limit at 120 char.  Avoid
  complicated logic on a single line.
- Put one blank line between any sibling multi-line expressions or declarations.
- For multi-line property expressions, break at operators:

    ```csharp
    public OneOf<NotFound, BoardIsDone, Result<int>> SelectBoard(int boardCode)
        => (boardCode <= 0 || boardCode > Boards.Count)
            ? new NotFound()
            : GetBoardByCode(boardCode).IsDone
            ? new BoardIsDone()
            : new Result<int>(boardCode - 1);
    ```

## Comments

- Use **XML-style comments** (`///`) for documentation on classes, methods,
  properties, and public APIs.
- Use **inline comments** (`//`) for brief in-line explanations.
- XML-style comments include XML tags like `<summary>`, `<returns>`, `<remarks>`.

    ```csharp
    /// <summary>
    /// Helper function to draw a border row of the board.
    /// </summary>
    /// <returns>
    /// Returns the index of the next board to draw if it needs another row of
    /// boards.
    /// </returns>
    private static int DrawBorderRow(...) {
        ...
    }
    ```

- Keep inline comments brief and relevant.

## Boolean Parameters

- Boolean parameters should be at the end of the parameter-set.
- Use `is`, `can`, `are`, or `do` prefix for boolean parameters where helpful
  for readability:

    ```csharp
    public void RunGame(
        char[] players,
        bool isRandomPlayerOrder,
        bool isSynchronousMode) {
        ...
    }
    
    public bool CanTakeTurn(char? player) {
        ...
    }
    ```
- When calling complex parameter-sets with boolean parameters, use named
  parameters for the booleans.

## Boolean Return Values

- For boolean return values, prefer `is` prefix for the return type name when
  naming conventions allow:

  ```csharp
  public bool IsSynchronousMode {get; init;} = false;
  public bool IsResignedPlayer(char player) => ...
  public bool IsGameOver => ...;
  ```

## Properties

- Use property initializers when possible:

  ```csharp
  public IReadOnlyList<char> Players {get;init;} = new List<char>();
  public int CurrentTurnPlayerIndex {
      get { return _currentTurnPlayerIndex; }
      set {
          _currentTurnPlayerIndex = value;
          RefreshCurrentPlayerTurnIndex(); 
      }
  }
  ```

- Use auto-implemented properties for simple cases:

  ```csharp
  public string GameStateText
      => $"Player '{CurrentTurnPlayer}' turn.";
  ```

- For computed properties, use expression bodied members:

  ```csharp
  public IEnumerable<int> ActiveBoardIndices { get {
      for(int i = 0; i < Boards.Count; i+=1) {
          if(!Boards[i].IsDone) {
              yield return i; 
          }
      }
  }}
  ```

## C# Features

### Records

- Prefer `record` types for immutable data models:

    ```csharp
    public record ScoreCard {
        ... 
    }

    public record BoardBuilder(sbyte Width, sbyte Height);
    ```

- Use discriminated unions via the `OneOf` library for result types that can
  fail or succeed.

### Generics and Constraints

- Specify generic constraints explicitly when helpful:

    ```csharp
    public static TItem? MaxByStrict<TItem, TProperty>(
        this IEnumerable<TItem> items, 
        Func<TItem, TProperty> getter) 
        where TItem : struct 
    {
        ...
    }
    ```

## Extension Methods

- Define extensions as `static class` with extension method syntax:

    ```csharp
    public static class ExtensionMethods {
        public static TItem? MaxByStrict<TItem, TProperty>(
            this IEnumerable<TItem> items, 
            Func<TItem, TProperty> getter) where TItem : struct 
        {
            ...
        }
    }
    ```

## `var` Usage

- Use `var` for local variables where the type is obvious from context (common
  after foreach, LINQ, etc.):

    ```csharp
    for (sbyte col = 0; col < Spaces.GetLength(0); col+=1) { ... }
    var boardBuilders = new Model.BoardBuilder[boardsNumber!.Value];
    ```

- Avoid `var` for members and properties where the type is declared at the declaration point.

## `#region` and `#endregion`

- Use `#region` / `#endregion` to organize code blocks:

    ```csharp
    #region constructors
    public Board() {}
    public Board(BoardBuilder builder) : this(builder.Width, builder.Height) {}
    #endregion

    #region main data properties
    public Space[,] Spaces {get;set;} = new Space[1,1]{{new Space()}};
    #endregion

    #region Methods
    public int GetSpaceIndexCode(int col, int row) { ... }
    #endregion

    #region helper properties
    public int ColumnCount
        => Spaces.GetLength(0);
    #endregion
    ```

## Null Handling

- Use `!` (null-forgiving) operator when non-null is guaranteed:

    ```csharp
    var file = parseResult.GetValue(Options.StateFileOption)!;
    ```

- Use `?.` (null-conditional) for safe navigation where needed:

    ```csharp
    (joinAsPlayer ?? "").Cast<char?>().SingleOrDefault();
    ```

- Prefer `Nullable<T>` for APIs that may return null:

    ```csharp
    public char? Winner { ... }
    ```

## Attributes

- Place attributes above the declaration.
- Use `[JsonIgnore]` from Newtonsoft.Json to exclude properties from serialization.
- Use `[Summary]` or similar to mark XML documentation tags.

## OneOf Usage

- Return type of methods that may fail should use `OneOf` types:

    ```csharp
    public OneOf<ActionQueuedSuccessfully, Result<char>, AlreadyPlayed> Enqueue(new TicTacToePlayAction(...)) {
        ...
    }
    ```

- Convert between `OneOf` types using `.Match<>()` when needed.

- Prefer `OneOf` over a nullable return (`T?` / `(A, B)?`) when a null result
  means a *very different event* has occurred rather than merely an absent
  value. Name the alternative case (e.g. `OneOf<CommandToken, None>` for "this
  message is not a command"), so callers switch on meaning instead of testing
  for null:

    ```csharp
    public OneOf<CommandToken, None> ParseCommand(string message) { ... }
    ```

- Prefer the lambda-based `Switch`/`Match` methods for branching between
  outcomes; avoid `IsT0`/`AsT0`-style accessors, which scattered-type-check the
  union and lose exhaustiveness. Use them only where necessary (e.g. a quick
  `IsT1` bail-out in non-branching code).

- For async handlers, have each `Match` lambda return a `Task` and await the
  `Match` result once — this keeps both outcomes async-safe (never `Switch`
  with async lambdas, which is fire-and-forget):

    ```csharp
    await result.Match(
        success => HandleSuccessAsync(success),
        _ => HandleFailureAsync());
    ```

- An alternative pattern for "found or not" cases is to return an
  `IEnumerable` of parsed results (1 or 0 items expected) and have the caller
  simply `foreach` across the results — the empty case then falls out
  naturally as a no-op. This also extends cleanly to multiple results per
  call (e.g. several commands in one message, where game rules allow).

## LINQ and Query Usage

- Use `var` with LINQ expressions.
- Use `yield return` for enumeration in properties/getters:

    ```csharp
    [JsonIgnore()]
    public IEnumerable<int> ActiveBoardIndices {get {
        for(int i = 0; i < Boards.Count; i+=1) {
            if(!Boards[i].IsDone) {
                yield return i; 
            }
        }
    }}
    ```

## JSON Attributes

- Use `[JsonIgnore()]` to exclude properties from serialization.

    ```csharp
    [JsonIgnore()]
    public int ColumnCount
        => Spaces.GetLength(0);
    ```

## Miscellaneous Patterns

- Use string-interpolation for building strings:

    ```csharp
    Console.Out.WriteLine($" - player '{resignedPlayer}' is resigned.");
    ```

- Use `using` statement for short-lived resources like `StreamReader` and `StreamWriter`.

- Use `using` with `var` to defer type resolution until needed:

    ```csharp
    using var fileStream = new FileStream(...);
    ```

- Use `GetCursorPosition()` to manage console output positioning.

- Use `Thread.Sleep` with `using` or careful disposal when needed for timing.


## Testing

- **Do not test private/protected members via reflection** (`Type.GetMethod(...)`
  + `Invoke`). Reflection-based test accessors are fragile, untyped, and bypass the
  compiler. Instead:
  - Prefer making the member `internal` and exposing it to the test assembly with
    `[assembly: InternalsVisibleTo("...")]` (see `src/**/AssemblyInfo.cs`), then call
    it directly.
  - If the member must stay inaccessible (sealed/private by design), use
    `System.Runtime.CompilerServices.UnsafeAccessor`
    (`FromMethodAccess` / `FromStaticMethodAccess` / `FromFieldAccess`) rather than
    raw reflection.


## Files

- One type per file, avoid nested types.  
- Simple one-line types can be defined in another type's file IFF the type is a
  simple one-line declaration (like the `OneOf` structs such as
  `AlreadyPlayed`).
- Namespaces match directory path.
- Use `internal static` for private API classes.
- Use `public static` for public classes.

## Docs

- Repo-standard docs go at root in ALLCAPS. Additional docs go in `/doc/` and
  are `kebab-cased.md`.

## Template Files

- The repo-template files — `CONTRIB.md`, `AGENTS.md`, `.gitignore`, and
  `.editorconfig` — originate from `Pxtl/dotnet-repo-template`. Changes to them
  should be PR'd back to that template repo so other projects inherit them.