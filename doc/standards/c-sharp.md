# C-Sharp Coding Standards

## Files

- One type per file, avoid nested types except for low-visibility modes like
  `private`.
- Simple one-line types can be defined in another type's file IFF the type is a
  simple one-line declaration and is primarily used by that class.  Bonus points
  if it is prefixed with the class name (eg: `SomeClass` and simple type
  `SomeClassException`).
  - These secondary types should be at the *end* of the file.
- Namespaces match directory path.
- Filenames match type name.
- Exception to the "one-line-types" rule exists for extension classes as
  described in Extension Methods section.

## Variable Naming

- Variable names should be clear and informative. While abbreviations are
  permitted, they should be common jargon and not invented (eg "Ctl" for
  "Control", or "Config" for "Configuration" are good).
- Hyper-brief one-or-two character names are okay for loop indexes and
  lambda parameters, but they should still be informative: `i` for a
  for-loop counter, or the first char of the object's name
  (`bookCollection.Select(b => b.Title)`).

### Namespace Imports

- **Global usings**: Use `global using` directives in `GlobalUsings.cs` to
  import namespaces that are heavily used across most files..
- **File-level usings**: Place `using` directives before the namespace
  declaration.
- Do not redundantly `using` the namespaces already provided by
  `<ImplicitUsings>enable</ImplicitUsings>` in `Standard.props`.


## Indentation

- Use **spaces** for indentation (not tabs).
- When breaking up a expression of peer-level tokens across multiple lines, indent the successive lines. Eg:

  ```csharp
  result = myCollection
      .SomeOperation()
      .SomeOtherOperation()
      .ToList();
  ```

- Multi-keyword declaration statements can live at the same indentation level,
  eg class declarations

  ```csharp
  public class MyClass<T>
  : MyBaseClass<T>, IMyInterface<T>
  where T : ISomeInterface {
      ...
  }
  ```

- Child levels of the parse-tree should almost always be indented.
- Avoid horizontal alignment.

## Braces (OTBS - Open on Same Line)

- Open braces on the same line as the declaration, closing brace on its own
  line.
- For methods:

  ```csharp
  public void MethodName(...) {
      // body
  }
  ```

- For property accessors / initializers:

  ```csharp
  public SomeType Prop { get; set; } = defaultValue;
  public SomeType Prop2 { get; init; }
  public int SingleValueProp => expression;
  ```

- If a single value property has has non-trivial complexity:

  ```csharp
  public int SingleValueProp 
  => longAndComplicatedExpression;
  ```

- Always brace ifs.
- Single-line braced statements should only be done in hyper-trivial cases, like
  empty class bodies `{ }`.
- Use `dotnet format` to apply styling to files.

## Parens

- Use parens where operators are mixed and order-of-operations is
  less-well-known, even if it is not necessary.
- `||` operations should almost always be in parens in expressions with mixed
  operators.
- Existing code is inconsistent for trailing parens in multi-line blocks,
  whether or not it should be linebreak-before `)` or not.  Use whichever
  convention is common in the codebase.  So both of the following examples are
  valid, but a codebase should pick one and stick to it.
  
  ```csharp
  longFunctionCall(
    param,
    secondParam,
    thirdParam
  );
  ```

  ```csharp
  longFunctionCall(
    param,
    secondParam,
    thirdParam);

## Linebreaks

- Break logical statements on new lines if they're too long. A line of code
  should have only one or two concerns.  Multiple semicoloned statements is a
  red flag.
- Use whitespace after commas in multi-parameter calls.
- If a multi-parameter signature exceeds the line-length limit, put each
  parameter on its own line.  Short signatures that fit on one line are fine
  to keep on one line.
- Soft length limit at 80 char, hard limit at 120 char.  Avoid complicated logic
  on a single line.
- Put one blank line between any sibling multi-line expressions or declarations, eg:

  ```csharp
  var myResult = myCollection
      .DoSomething()
      .DoSomethingElse();
      .ToList();

  SomeSecondOperationAfterABlankLine();
  ```
    
- For multi-line property expressions, break before operators:

  ```csharp
  public OneOf<NotFound, BoardIsDone, Result<int>> SelectBoard(int boardCode)
  => (boardCode <= 0 || boardCode > Boards.Count)
      ? new NotFound()
      : GetBoardByCode(boardCode).IsDone
      ? new BoardIsDone()
      : new Result<int>(boardCode - 1);
  ```

- For ternary expressions, unless it is absolutely the simplest possible case,
  break onto multiple lines with the `?` and `:` at the start of their own
  lines:

  ```csharp
  var result = someCondition
      ? DoSomething()
      : DoSomethingElse();
  ```

## Comments

- Use **XML-style comments** (`///`) for documentation on classes, methods,
  properties, and public APIs.
- Use **inline comments** (`//`) for brief in-line explanations.
- XML-style comments include XML tags like `<summary>`, `<returns>`,
  `<remarks>`.
- Do not write trivial comments that just restate the name of the object. Do not
  provide empty parameter XML comments. For example, if a parameter has a clear
  name and does not require elaboration, do not provide an xml-style comment for
  it.

  ```csharp
  /// <summary>
  /// Helper function to draw a border row of the board.
  /// </summary>
  /// <returns>
  /// Returns the index of the next board to draw if it needs another row of
  /// boards.
  /// </returns>
  private static int DrawBorderRow(int someWellDocumentedIntParameter) {
      ...
  }
  ```

- Keep inline comments brief and relevant.

## Public API Naming

When making members or types that will be used in another language (eg.
`System.CommandLine` option names or JSON members), follow the convention of the
other platform.  For example, `System.CommandLine` boolean options will
generally not have the `is` prefix common in C#.

### Boolean Parameters

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
  parameters for the Booleans if using raw `true` or `false` values.

### Boolean Return Values

- For boolean return values, prefer `Is` prefix for the return type name when
  naming conventions allow:

  ```csharp
  public bool IsSynchronousMode { get; init; } = false;
  public bool IsResignedPlayer(char player) => ...
  public bool IsGameOver => ...;
  ```

- Other simple verbs are also allowed (non-exclusive list): `Has`, `Can`,
  `Does`.

## Properties

- Use property initializers when possible:

  ```csharp
  public IReadOnlyList<char> Players { get; init; } = new List<char>();
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
  public string ExampleProperty { get; } = "Default Value";
  ```

- For computed properties, use expression bodied members or blocks as
  appropriate:

  ```csharp
  public int BoardSize
  => Boards.Count();

  public IEnumerable<int> ActiveBoardIndices { get {
      for(int i = 0; i < Boards.Count; i+=1) {
          if(!Boards[i].IsDone) {
              yield return i; 
          }
      }
  }}
  ```

## Special C# Features

### Records

- Prefer `record` types for immutable data models:

  ```csharp
  public record class ScoreCard {
      ... 
  }

  public record struct BoardBuilder(sbyte Width, sbyte Height);
  ```

### Generics and Constraints

- Specify generic constraints explicitly when helpful:

  ```csharp
  public static TItem? MaxByStrict<TItem, TProperty>(
      this IEnumerable<TItem> items, 
      Func<TItem, TProperty> getter) 
  where TItem : struct {
      ...
  }
  ```

### Extension Methods

- Use the new C# static `extension()` statement when using a C# version where it
  is available.
- Define extensions as `static class` with extension method syntax (note,
  example assumes older C# version without `extension()` keyword. TODO: fix that):

  ```csharp
  public static class ExtensionMethods {
      public static TItem? MaxByStrict<TItem, TProperty>(
          this IEnumerable<TItem> items, 
          Func<TItem, TProperty> getter) 
      where TItem : struct {
          ...
      }
  }
  ```

- It is permitted to piggyback an extension class at the bottom of the same
  `.cs` file as its related class `RelatedClass` (particularly for something
  like an enum or an interface), as long as the name of the extension class is
  `RelatedClassExtensions` or `RelatedClassHelper`. "Helper" is preferred suffix
  if the extension also provides non-extension static methods for the
  `RelatedClass`.

### `var` Usage

- Use `var` for local variables wherever practical.

### `#region` and `#endregion`

- Use `#region` / `#endregion` to organize code blocks:

  ```csharp
  #region constructors
  public Board() {}
  public Board(BoardBuilder builder) : this(builder.Width, builder.Height) {}
  #endregion

  #region data properties
  public Space[,] Spaces {get;set;} = new Space[1,1]{{new Space()}};
  #endregion

  #region methods
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

- Prefer `Nullable<T>` for APIs that may return null if the null meaningfully
  means "none":

  ```csharp
  public char? Winner { ... }
  ```

- Prefer `https://github.com/mcintyre321/OneOf` over a nullable return (`T?` /
  `(A, B)?`) when a null result means a *very different event* has occurred
  rather than merely an absent value, such as expected failures. Do not use
  `null` for such cases.  Name the alternative case (e.g. `OneOf<CommandToken,
  None>` for "this message is not a command"), so callers switch on meaning
  instead of testing for null:

  ```csharp
  public OneOf<CommandToken, None> ParseCommand(string message) { ... }
  ```

- An alternative pattern for "found or not" cases is to return an
  `IEnumerable` of parsed results (1 or 0 items expected) and have the caller
  simply `foreach` across the results - the empty case then falls out
  naturally as a no-op. This also extends cleanly to multiple results per
  call.

## OneOf Usage

- Convert between `OneOf` types using `.Match<>()` when needed.

- Prefer the lambda-based `Switch`/`Match` methods for branching between
  outcomes; avoid `IsT0`/`AsT0`-style accessors, which scattered-type-check the
  union and lose exhaustiveness. Use them only where necessary (e.g. a quick
  `IsT1` bail-out in non-branching code).

## Miscellaneous Patterns

- Use string-interpolation for building strings:

    ```csharp
    Console.Out.WriteLine($" - player '{resignedPlayer}' is resigned.");
    ```

- Use `using` statement for short-lived resources like `StreamReader` and
  `StreamWriter`.

- Use `using` with `var` to defer type resolution until needed:

    ```csharp
    using var fileStream = new FileStream(...);
    ```

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
