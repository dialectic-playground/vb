# Low-Level Design (LLD) — Standard Terminal Hello World

| Field | Value |
| :-- | :-- |
| Project | `00-hello-world` |
| Tools | dp-hello |
| Implementation language | Visual Basic (VB.NET, .NET 10) |
| Author | Brandon Perkins (with Claude Code) |
| Version / date | 1.0 — 2026-10-13 |
| Status | Approved |
| Inputs | `hld.md`, `specs/SRS.md`, `shared/CLI-CONVENTIONS.md` |

## 1. Source layout

```text
00-hello-world/
├── DpHello.sln                 solution: the three projects below
├── global.json                 SDK 10.0 (roll forward to the latest 10.0 feature band)
├── Directory.Build.props       Option Strict/Explicit/Infer, warnings as errors, analyzers, lock files
└── src/
    ├── DpHello/                class library (all logic)
    │   ├── ArgumentParser.vb   ArgumentParser.Parse (CONV-07 … CONV-12)
    │   ├── ParsedArguments.vb  ParseAction, OptionValue, ParsedArguments, ParseResult
    │   ├── Validator.vb        Validator.Validate / ParseCount, GreetingSettings, ValidationResult
    │   ├── Greeting.vb         Greeting.BuildLine / BuildText (pure)
    │   ├── UsageError.vb       UsageError and its CONV-13 factories
    │   ├── Output.vb           Output.Encode / Diagnostic / WriteAll
    │   ├── CommandLineTool.vb  CommandLineTool.Run, RunResult, HelpText, VersionText
    │   └── packages.lock.json
    ├── DpHello.Cli/            console app; assembly and apphost name dp-hello
    │   ├── Program.vb          Program.Main
    │   └── packages.lock.json
    └── DpHello.Tests/          xUnit tests, one class per library unit
        ├── ArgumentParserTests.vb  ValidatorTests.vb  GreetingTests.vb
        ├── OutputTests.vb          CommandLineToolTests.vb
        └── packages.lock.json
```

## 2. Data types

### 2.1 `ParsedArguments` (result of a successful scan)
| Field | Type | Default | Constraint / invariant | SRS |
| :-- | :-- | :-- | :-- | :-- |
| `Action` | `ParseAction` (`Greet`, `Help`, `Version`) | `Greet` | For Help/Version all other fields are Nothing | FR-07, FR-08 |
| `Name` | `OptionValue` | Nothing | The last `-n`/`--name` occurrence; unvalidated | FR-02, FR-06 |
| `Count` | `OptionValue` | Nothing | The last `-c`/`--count` occurrence; unvalidated | FR-04, FR-06 |
| `FirstOperand` | `String` | Nothing | The first operand, including the first token after `--` | FR-09 |

`OptionValue` holds `Value` (verbatim, possibly empty) and `Spelling` (`-n`, `--name`, `-c`,
or `--count`, as written; CONV-13 "option as written"). `ParseResult` holds either
`Arguments` or `Failure`.

### 2.2 `GreetingSettings` (result of validation)
| Field | Type | Default | Constraint / invariant | SRS |
| :-- | :-- | :-- | :-- | :-- |
| `Name` | `String` | `World` | Non-empty | FR-01, FR-02, FR-03 |
| `Count` | `Integer` | 1 | 1 ≤ Count ≤ 1000 | FR-04, FR-05 |

`ValidationResult` holds either `Settings` or `Failure`. `RunResult` holds
`StandardOutput` and `StandardError` (byte arrays) and `ExitCode`.

### 2.3 Error type
`UsageError.Message` is the diagnostic without the prefix. `Output.Diagnostic` renders it as
`dp-hello: <Message>` LF `Try 'dp-hello --help' for more information.` LF.

| Factory | Diagnostic (first stderr line) | Exit status |
| :-- | :-- | :-- |
| `InvalidOption(letter)` | `dp-hello: invalid option -- 'x'` | 2 |
| `UnrecognizedOption(token)` | `dp-hello: unrecognized option '--nme'` | 2 |
| `ShortOptionRequiresArgument(letter)` | `dp-hello: option requires an argument -- 'n'` | 2 |
| `LongOptionRequiresArgument(name)` | `dp-hello: option '--name' requires an argument` | 2 |
| `ArgumentNotAllowed(name)` | `dp-hello: option '--help' doesn't allow an argument` | 2 |
| `InvalidArgument(value, spelling)` | `dp-hello: invalid argument '0' for '--count'` | 2 |
| `ExtraOperand(operand)` | `dp-hello: extra operand 'extra'` | 2 |
| (write failure, not a `UsageError`) | none | 141 (CONV-25: unspecified) |

## 3. Module and function specifications

### 3.1 `ArgumentParser.Parse`
- **Signature:** `Public Function Parse(args As IReadOnlyList(Of String)) As ParseResult`
- **Responsibility:** recognize option syntax from left to right.
- **Preconditions:** `args` is not Nothing (excludes the program name).
- **Postconditions / return value:** `Help`/`Version` for the first `--help`/`--version`
  reached; otherwise `Greet` with the last name and count values and the first operand.
- **Errors:** the first syntax error before any `--help`/`--version`: `InvalidOption`,
  `UnrecognizedOption`, `ShortOptionRequiresArgument`, `LongOptionRequiresArgument`,
  `ArgumentNotAllowed`. Never returns value or operand errors.
- **Algorithm:**
  1. For each token: `--` → the next token (if any) becomes the first operand if none yet; stop.
  2. Starts with `--` → `ParseLong`: split at the first `=`; `help`/`version` with `=` →
     `ArgumentNotAllowed`, otherwise return the action; `name`/`count` take the attached
     value or the next token (any token, even one starting with `-`), else
     `LongOptionRequiresArgument`; any other name (no abbreviations) → `UnrecognizedOption(token)`.
  3. Starts with `-` and is longer than 1 → `ParseShort`: the letter is the code point after
     `-` (a surrogate pair counts as one); `n`/`c` take the rest of the token or the next
     token, else `ShortOptionRequiresArgument`; any other letter → `InvalidOption`.
  4. Otherwise (including `-`) → an operand; remember only the first.
  5. A recorded value overwrites the earlier one (last wins).
- **Complexity:** time O(total argument length), memory O(1) beyond the result.
- **Satisfies:** FR-02, FR-06, FR-07, FR-08, FR-10, CONV-07 … CONV-12, CONV-22.
- **Unit tests:** `ArgumentParserTests.*` (15 methods, 44 cases).

### 3.2 `Validator.Validate`
- **Signature:** `Public Function Validate(arguments As ParsedArguments) As ValidationResult`
- **Responsibility:** turn a Greet parse result into settings.
- **Errors (in this order):** `ExtraOperand(FirstOperand)`; `InvalidArgument('', spelling)`
  for an empty name; `InvalidArgument(value, spelling)` when `ParseCount` fails.
- **Algorithm:** defaults `World` and 1; check operand, then name, then count.
- **Satisfies:** FR-03, FR-05, FR-06 (only the winning value is validated), FR-09.
- **Unit tests:** `ValidatorTests.*`, `CommandLineToolTests.OnlyTheWinningValueIsValidated`.

### 3.3 `Validator.ParseCount`
- **Signature:** `Public Function ParseCount(text As String) As Integer?`
- **Algorithm:** empty → Nothing; every char must be `0`–`9` (ASCII only; Unicode digits are
  rejected); accumulate `value = Min(value * 10 + digit, 1001)` so any length is safe;
  return the value if 1 ≤ value ≤ 1000, else Nothing. Leading zeros are accepted.
- **Complexity:** O(length), no overflow.
- **Satisfies:** FR-04, FR-05. **Unit tests:** `ValidatorTests.ValidCounts`,
  `ValidatorTests.InvalidCountsAreRejectedWithTheValueAndSpelling`.

### 3.4 `Greeting.BuildLine` / `Greeting.BuildText`
- **Signatures:** `BuildLine(name As String) As String`; `BuildText(name As String, count As Integer) As String`
- **Responsibility:** pure greeting construction: `"Hello, " & name & "!" & vbLf`, repeated
  `count` times (empty for count ≤ 0) with a pre-sized `StringBuilder`.
- **Satisfies:** FR-01, FR-02, FR-04, NFR-05. **Unit tests:** `GreetingTests.*`.

### 3.5 `Output.Encode` / `Output.Diagnostic` / `Output.WriteAll`
- `Encode(text) As Byte()`: UTF-8 without BOM; unpaired surrogates become U+FFFD (no throw).
- `Diagnostic(failure As UsageError) As String`: the two CONV-13/CONV-14 lines, LF-terminated.
- `WriteAll(stream As Stream, data As Byte()) As Boolean`: write and flush; returns False on
  `IOException`, `ObjectDisposedException`, or `NotSupportedException` (CONV-25).
- **Satisfies:** CONV-01, CONV-03, CONV-05, CONV-13, CONV-14, CONV-25.
- **Unit tests:** `OutputTests.*`.

### 3.6 `CommandLineTool.Run`
- **Signature:** `Public Function Run(args As IReadOnlyList(Of String)) As RunResult`
- **Algorithm:** parse; on failure → `(empty, Encode(Diagnostic(e)), 2)`; Help/Version →
  `(Encode(HelpText or VersionText), empty, 0)`; otherwise validate (failure as above) and
  return `(Encode(BuildText(name, count)), empty, 0)`.
- **Satisfies:** FR-01 … FR-11, CONV-02, CONV-12, CONV-14, CONV-23.
- **Unit tests:** `CommandLineToolTests.*`.

### 3.7 `Program.Main`
- **Signature:** `Public Function Main(args As String()) As Integer`
- **Algorithm:** `Run(args)`; write `StandardError` to `Console.OpenStandardError()`, then
  `StandardOutput` to `Console.OpenStandardOutput()`; return 141 if the stdout write failed,
  otherwise `ExitCode`. (On every run exactly one of the two byte arrays is non-empty.)
- **Satisfies:** CONV-03, CONV-25, CONV-26 (stdin is never opened). **Tests:** P01–P10.

## 4. Algorithms
The only non-trivial algorithm is the left-to-right scan (§3.1). It follows GNU `getopt_long`
semantics restricted to CONV-07 … CONV-12, with one deliberate difference required by
CONV-12: operands are never errors during the scan. Because both short options take a
value, a short-option token never needs bundle iteration: the first letter decides, and the
rest of the token is its value (`-nc3` greets `c3`). Count parsing (§3.3) saturates instead
of using `Integer.TryParse`, which would accept signs, white space, and non-ASCII digits.

## 5. Platform-specific details

| Concern | Linux | macOS | Windows | Abstraction |
| :-- | :-- | :-- | :-- | :-- |
| Arguments | `argv` bytes decoded as UTF-8 by the runtime | same as Linux | UTF-16 command line (`GetCommandLineW`) | `String()` in `Main` |
| stdout/stderr | fd 1/2, raw | fd 1/2, raw | `STD_OUTPUT_HANDLE`/`STD_ERROR_HANDLE`, no text mode | `Console.OpenStandard*` + `Output.WriteAll` |
| Broken pipe | `IOException` (the runtime ignores SIGPIPE) | same | `IOException` (`ERROR_NO_DATA`) | `Output.WriteAll` returns False |
| Executable | `bin/dp-hello` | `bin/dp-hello` | `bin/dp-hello.exe` | `$(EXE)` in the Makefile |
| Runtime lookup | `DOTNET_ROOT`, `/etc/dotnet/install_location`, `/usr/share/dotnet` | same, default `/usr/local/share/dotnet` | `DOTNET_ROOT`, registry, `C:\Program Files\dotnet` | the .NET apphost |

## 6. Resource management
No files, sockets, locks, or processes. The two standard streams are opened once each in
`Program.Main` inside `Using` blocks, so they are flushed and released on every path. The
largest buffer is the greeting text: 1000 × (9 + name length) characters, encoded once.

## 7. Sequence of one invocation
P06, `dp-hello -c2 -n Ken`: `Main({"-c2", "-n", "Ken"})` → `Run` → `Parse`: `-c2` is a short
option `c` with attached value `2` → `Count = ("2", "-c")`; `-n` takes the next token `Ken`
→ `Name = ("Ken", "-n")`; end → `Greet` → `Validate`: no operand, name non-empty,
`ParseCount("2") = 2` → `BuildText("Ken", 2)` = `Hello, Ken!␊Hello, Ken!␊` → encoded to
26 bytes → `Main` writes nothing to stderr, the 26 bytes to stdout, and returns 0.

## 8. Design-to-code checklist

| HLD component | LLD section | Source file(s) | Unit-test file(s) |
| :-- | :-- | :-- | :-- |
| `ArgumentParser` | §3.1 | `src/DpHello/ArgumentParser.vb`, `ParsedArguments.vb` | `ArgumentParserTests.vb` |
| `Validator` | §3.2, §3.3 | `src/DpHello/Validator.vb` | `ValidatorTests.vb` |
| `Greeting` | §3.4 | `src/DpHello/Greeting.vb` | `GreetingTests.vb` |
| `UsageError` | §2.3 | `src/DpHello/UsageError.vb` | `ArgumentParserTests.vb`, `ValidatorTests.vb` |
| `Output` | §3.5 | `src/DpHello/Output.vb` | `OutputTests.vb` |
| `CommandLineTool` | §3.6 | `src/DpHello/CommandLineTool.vb` | `CommandLineToolTests.vb` |
| `Program` | §3.7 | `src/DpHello.Cli/Program.vb` | black-box P01–P10 |
