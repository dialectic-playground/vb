# High-Level Design (HLD) — Standard Terminal Hello World

| Field | Value |
| :-- | :-- |
| Project | `00-hello-world` |
| Tools | dp-hello |
| Implementation language | Visual Basic (VB.NET, .NET 10) |
| Author | Brandon Perkins (with Claude Code) |
| Version / date | 1.0 — 2026-10-13 |
| Status | Approved |
| Inputs | `specs/URD.md`, `specs/SRS.md`, `specs/user-manual.md`, `shared/CLI-CONVENTIONS.md` |

## 1. Introduction

### 1.1 Scope
`dp-hello` writes the greeting `Hello, World!` (or `Hello, NAME!`) to standard output, 1 to
1000 times, prints help and version information, and reports usage errors with the exact
CONV-13 diagnostics and exit status 2 (SRS §1.2). It reads no input, takes no operands,
touches no files or environment variables, and starts no processes.

### 1.2 Definitions and acronyms
| Term | Meaning |
| :-- | :-- |
| Apphost | The small native executable the .NET SDK generates to start a managed program |
| Framework-dependent | Uses the .NET runtime installed on the machine instead of bundling it |
| Single-file | The apphost and the managed assemblies bundled into one executable |
| Core | The pure `CommandLineTool.Run` function: arguments in, output bytes and status out |

### 1.3 References
- SRS §3 (interfaces), §4 (FR-01 … FR-11), §5 (NFR-01 … NFR-07), §6 (diagnostics)
- CLI-CONV §2 (bytes and encodings), §3 (option syntax), §4.1 (usage errors), §8 (robustness)

## 2. Requirements summary

| Area | SRS requirements | Architectural impact |
| :-- | :-- | :-- |
| Option parsing | FR-02, FR-06, FR-07, FR-08, FR-10, CONV-07 … CONV-12 | A hand-written, syntax-only scanner (`ArgumentParser`) that stops at `--help`/`--version` |
| Validation | FR-03, FR-04, FR-05, FR-09 | A separate `Validator` that runs after the scan, so value and operand errors never beat `--help` |
| Greeting | FR-01, FR-02, FR-04, NFR-05 | A pure `Greeting` module, unit-tested in isolation |
| Diagnostics and exit status | FR-11, §3.7, §6, CONV-13 … CONV-15 | One `UsageError` type builds every message; the core maps it to stderr bytes and status 2 |
| Byte-exact output | NFR-01, CONV-03, CONV-05 | `Output` encodes UTF-8 without BOM and writes raw bytes to the standard streams |
| Robustness and security | NFR-03, NFR-07, CONV-24, CONV-25 | No exceptions for expected cases; write failures are caught; no file, environment, or process APIs |

## 3. Architectural overview

### 3.1 Context diagram

```text
                      ┌──────────────────────────┐ ──▶ stdout (UTF-8 bytes, LF)
  argv (UTF-16 strings)│        dp-hello          │ ──▶ stderr (diagnostics)
  ───────────────────▶ │ apphost + .NET 10 runtime│ ──▶ exit status 0 / 2
                      └──────────────────────────┘
   (stdin, files, environment, network, and child processes are not used)
```

### 3.2 Component (module) decomposition

| Component | Responsibility | Depends on |
| :-- | :-- | :-- |
| `ArgumentParser` (library) | Scan the arguments left to right into `ParsedArguments`, or the first syntax `UsageError` | `UsageError`, `ParsedArguments` |
| `Validator` (library) | Reject operands, an empty name, and invalid counts; produce `GreetingSettings` | `UsageError`, `ParsedArguments` |
| `Greeting` (library) | Build the greeting text (pure) | — |
| `UsageError` (library) | The single home of every CONV-13 message text | — |
| `Output` (library) | UTF-8 encoding, the CONV-14 two-line diagnostic, raw-stream writes | `UsageError` |
| `CommandLineTool` (library) | Pure core: wire parser → validator → greeting; help and version texts; exit statuses | all of the above |
| `Program` (console app) | Thin shell: call the core, write its bytes to the standard streams, return the status | `CommandLineTool`, `Output` |

### 3.3 Data flow
`Program.Main(args)` → `CommandLineTool.Run(args)` → `ArgumentParser.Parse` returns a syntax
error, Help, Version, or Greet with the raw values → for Greet, `Validator.Validate` returns
`GreetingSettings` or a value/operand error → `Greeting.BuildText(name, count)` → the core
encodes the text (or the diagnostic) as UTF-8 into a `RunResult` → `Program` writes
`StandardError` and `StandardOutput` to the raw streams and returns `ExitCode`.

## 4. Key design decisions

| # | Decision | Alternatives considered | Rationale |
| :-- | :-- | :-- | :-- |
| D1 | Pure core `Run(args) -> RunResult(stdout bytes, stderr bytes, status)` with a thin I/O shell | Writing from inside the parser and validator | End-to-end behavior is unit-testable byte for byte; error atomicity (FR-11) is structural: output is produced only after validation |
| D2 | Result objects (`ParseResult`, `ValidationResult`) instead of exceptions for usage errors | Throwing a custom exception | Usage errors are expected outcomes, not exceptional ones; no stack trace can leak (CONV-24) |
| D3 | Syntax-only scan; operands collected and values recorded, validated after the scan | Validating each value as it is parsed | Required by CONV-12 (v1.4.0): `dp-hello --count 0 --help` must print the help |
| D4 | Buffer the whole output (≤ 1000 × short lines) and write it once | Streaming line by line | At most ~1 MB even for long names; one write keeps the code simple and the error path trivial |
| D5 | Encode with `UTF8Encoding(False)` and write to `Console.OpenStandardOutput()` / `OpenStandardError()` | `Console.Out` / `Console.Error` | The raw streams have no newline translation and no console code page, so the bytes are identical on every OS (CONV-03, CONV-05) |
| D6 | Framework-dependent, single-file publish for the current RID | Folder publish; self-contained; Native AOT | One small native executable (~110 KB) to install; self-contained is ~70 MB; Native AOT needs a native linker toolchain on every OS and is not a documented scenario for VB |
| D7 | `InvariantGlobalization`, `UseSystemResourceKeys`, `MyType=Empty` | Defaults | Locale independence (CONV-06) and less start-up work (NFR-02) |

## 5. Cross-platform strategy
- **Line endings (CONV-03):** every generated line ends with `vbLf`, never `vbCrLf` or
  `Environment.NewLine`; the help text is joined with `vbLf` instead of relying on the source
  file's line endings.
- **Binary output:** the core produces bytes; `Program` writes them to the raw standard
  streams, which on Windows are the unmodified `HANDLE`s (no text mode in .NET).
- **Arguments (CONV-05):** on Windows the .NET host reads the UTF-16 command line
  (`GetCommandLineW`), so `args` holds the exact characters; on Linux and macOS the runtime
  decodes `argv` as UTF-8. `Output.Encode` turns them back into UTF-8.
- **Paths (CONV-19 … CONV-21):** not applicable; the tool uses no paths.
- **Runtime location:** the apphost finds .NET through `DOTNET_ROOT`, the registered install
  location (`/etc/dotnet/install_location`), or the default location; native CI registers
  the location because the black-box runner clears the environment.
- The only platform-specific code is the stream handling in `Program`/`Output`; the library
  is otherwise platform-neutral.

## 6. Error handling and diagnostics
`UsageError` has one factory per CONV-13 row and holds the message without the tool prefix.
`ArgumentParser` and `Validator` return it inside their result objects.
`CommandLineTool.Run` is the one place that maps it to output: `Output.Diagnostic` adds the
`dp-hello: ` prefix and the CONV-14 `Try 'dp-hello --help' …` line, stdout stays empty,
and the status is 2. A failed write (EPIPE, closed stream) makes `Output.WriteAll` return
False, and `Program` exits with 141 and prints nothing (CONV-25). There is no other runtime
failure mode, so the tool never exits with status 1.

## 7. Non-functional design

| Quality attribute | SRS reference | How the design achieves it |
| :-- | :-- | :-- |
| Performance / memory | NFR-02, CONV-31 | Single-file apphost with invariant globalization; measured ~29 ms median and 25 MiB peak RSS (about 3.5 MiB above the 22 MiB baseline of an empty .NET program) in the container |
| Portability | NFR-01 | LF-only text, UTF-8 bytes on raw streams; the same code on all three OSes; CI on Linux, macOS, Windows |
| Security / input validation | NFR-07 | No file, environment, or process APIs; all input is the argument list, validated before use |
| Maintainability | NFR-04, NFR-05 | One unit per responsibility (§3.2); pure greeting and core functions; warnings are errors |
| Robustness | NFR-03 | Every argument list maps to a result; counts are parsed with saturation (no overflow); unpaired surrogates are replaced, not thrown |

## 8. Risks and open issues

| Risk / issue | Likelihood | Impact | Mitigation |
| :-- | :-- | :-- | :-- |
| .NET start-up time exceeds the 100 ms native budget on slow CI machines | L | M | Measured ~29 ms; .NET is a managed runtime, so the 300 ms allowance applies |
| Apphost cannot find the runtime without `DOTNET_ROOT` | M | H | Register the install location in CI; documented in the README |
| Invalid UTF-8 bytes in POSIX arguments are decoded to U+FFFD by .NET | L | L | Accepted: CONV-05 defines arguments as UTF-8 text; reading `/proc` to recover raw bytes would violate NFR-07 |

## 9. Approval

| Role | Name | Date | Decision |
| :-- | :-- | :-- | :-- |
| Author | Brandon Perkins (with Claude Code) | 2026-10-13 | Submitted |
| Reviewer | CI (container and native jobs) | 2026-10-13 | Approved when all checks pass |
