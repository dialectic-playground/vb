# Requirements Traceability Matrix (RTM) — Standard Terminal Hello World

| Field | Value |
| :-- | :-- |
| Project | `00-hello-world` |
| Tools | dp-hello |
| Author | Brandon Perkins (with Claude Code) |
| Version / date | 1.0 — 2026-10-13 |

Code paths are relative to `src/`. Unit tests are `Class.Method` in `src/DpHello.Tests/`;
`P01` … `P10` are the public black-box tests. Hidden tests are verified by CI and not cited.

## 1. User requirements to system requirements

| User requirement (URD) | Derived system requirements (SRS) |
| :-- | :-- |
| UR-01 | FR-01 |
| UR-02 | FR-02 |
| UR-03 | FR-04 |
| UR-04 | FR-07 |
| UR-05 | FR-08 |
| UR-06 | FR-03, FR-05, FR-09, FR-10, FR-11 |
| UR-07 | FR-01, SRS §3.7 |
| UR-08 | NFR-01, CONV-03 |
| UR-09 | FR-02, CONV-05 |
| UR-10 | FR-06, FR-10, CONV-07 … CONV-15 |
| UR-11 | NFR-02 |
| UR-12 | NFR-06 |

## 2. Forward traceability

| Req. ID | Summary | Design (HLD/LLD) | Code (file::function) | Verification (unit / black-box) | Status |
| :-- | :-- | :-- | :-- | :-- | :-- |
| UR-01 … UR-12 | User needs | HLD §2 | via the derived FR/NFR rows | via the derived FR/NFR rows; P01–P10 | Verified |
| FR-01 | Default greeting | HLD §3.3, LLD §3.2, §3.4 | `DpHello/Validator.vb::Validate`, `DpHello/Greeting.vb::BuildText` | `ValidatorTests.DefaultsAreWorldAndOne`, `CommandLineToolTests.DefaultGreeting`, P01 | Verified |
| FR-02 | Named greeting, verbatim UTF-8 | LLD §3.1, §3.4 | `DpHello/ArgumentParser.vb::Parse`, `Greeting.vb::BuildLine` | `ArgumentParserTests.NameSpellingsAreEquivalent`, `.AttachedValuesAreVerbatim`, `.SeparateValueMayStartWithDash`, `GreetingTests.NameIsVerbatim`, `CommandLineToolTests.NonAsciiNameIsUtf8`, P02, P03, P04 | Verified |
| FR-03 | Empty name rejected | LLD §3.2 | `DpHello/Validator.vb::Validate` | `ValidatorTests.EmptyNameIsRejected`, `CommandLineToolTests.UsageErrorsWriteNothingToStdout` | Verified |
| FR-04 | Repetition 1–1000, leading zeros | LLD §3.3, §3.4 | `DpHello/Validator.vb::ParseCount`, `Greeting.vb::BuildText` | `ValidatorTests.ValidCounts`, `GreetingTests.TextRepeatsTheLineCountTimes`, `CommandLineToolTests.MaximumCountPrintsOneThousandLines`, P05, P06 | Verified |
| FR-05 | Invalid count rejected | LLD §3.3 | `DpHello/Validator.vb::ParseCount` | `ValidatorTests.InvalidCountsAreRejectedWithTheValueAndSpelling`, `CommandLineToolTests.UsageErrorsWriteNothingToStdout` | Verified |
| FR-06 | Last occurrence wins; only it is validated | LLD §3.1, §3.2 | `DpHello/ArgumentParser.vb::Parse` | `ArgumentParserTests.LastOccurrenceWins`, `CommandLineToolTests.OnlyTheWinningValueIsValidated` | Verified |
| FR-07 | Help text, scan order | HLD D3, LLD §3.1, §3.6 | `DpHello/CommandLineTool.vb::HelpText`, `::Run` | `ArgumentParserTests.HelpWinsWhenReachedBeforeASyntaxError`, `CommandLineToolTests.HelpIsTheUserManualText`, P07 | Verified |
| FR-08 | Version line | LLD §3.6 | `DpHello/CommandLineTool.vb::VersionText` | `CommandLineToolTests.Version`, P08 | Verified |
| FR-09 | Operands rejected after the scan | HLD D3, LLD §3.1, §3.2 | `ArgumentParser.vb::Parse`, `Validator.vb::Validate` | `ArgumentParserTests.OperandsAreCollectedNotRejected`, `.TerminatorMakesFollowingTokensOperands`, `ValidatorTests.OperandIsReportedBeforeInvalidValues`, P10 | Verified |
| FR-10 | Unknown and malformed options | LLD §2.3, §3.1 | `ArgumentParser.vb::ParseLong`, `::ParseShort`, `UsageError.vb` | `ArgumentParserTests.SyntaxErrors`, `.SyntaxErrorBeforeHelpIsReported`, P09 | Verified |
| FR-11 | Nothing on stdout on error | HLD D1, LLD §3.6 | `DpHello/CommandLineTool.vb::Run` | `CommandLineToolTests.UsageErrorsWriteNothingToStdout`, P09, P10 | Verified |
| NFR-01 | Byte-identical, LF only, 3 OSes | HLD §5, D5 | `DpHello/Output.vb::Encode`, `DpHello.Cli/Program.vb::Main` | `OutputTests.EncodeIsUtf8WithoutBom`, `CommandLineToolTests.MaximumCountPrintsOneThousandLines`; P01–P10 in CI on Linux, macOS, Windows | Verified |
| NFR-02 | < 300 ms (managed), < 32 MiB above baseline | HLD §7, D6, D7 | `DpHello.Cli/DpHello.Cli.vbproj` (single-file, invariant globalization) | Measurement (MTP §4.4): ~29 ms median, 25 MiB peak RSS vs 22 MiB baseline | Verified |
| NFR-03 | No crash or stack trace | HLD §6, D2 | result types throughout; `Output.vb::WriteAll` | `ArgumentParserTests.SyntaxErrors`, `ValidatorTests.InvalidCountsAreRejectedWithTheValueAndSpelling` (overflow case), `OutputTests.EncodeReplacesUnpairedSurrogateInsteadOfThrowing`, `OutputTests.WriteAllReportsAFailingStream` | Verified |
| NFR-04 | Separate units | HLD §3.2 | one module per unit in `DpHello/` | Design review (LLD §8) | Verified |
| NFR-05 | Pure, tested greeting | LLD §3.4 | `DpHello/Greeting.vb` | `GreetingTests.*` | Verified |
| NFR-06 | Every BASE-MAKEFILE target | HLD §1 | `../Makefile` (DP_* hooks) | CI `make check roundtrip` (3 OSes), `make check` (container) | Verified |
| NFR-07 | No files, environment, processes | HLD §7 | whole library and `Program.vb` | Inspection: no `System.IO.File`, `Environment`, or `Process` use | Verified |
| CONV-01 | Canonical name | LLD §3.5 | `Output.vb::ToolName`, `CommandLineTool.vb::VersionText` | `OutputTests.DiagnosticHasTwoLfTerminatedLines`, P08, P09 | Verified |
| CONV-02 | Version format | LLD §3.6 | `CommandLineTool.vb::VersionText` | `CommandLineToolTests.Version`, P08 | Verified |
| CONV-03 | LF only, binary output | HLD §5 | `Output.vb::WriteAll`, `Program.vb::Main` | `OutputTests.WriteAllWritesTheExactBytes`, P01–P10 (3 OSes) | Verified |
| CONV-05 | UTF-8 arguments | HLD §5, LLD §5 | `Output.vb::Encode`; .NET argument decoding | `OutputTests.EncodeIsUtf8WithoutBom`, `CommandLineToolTests.NonAsciiNameIsUtf8` | Verified |
| CONV-06 | Locale independent, no colors | HLD D7 | `../Directory.Build.props` (`InvariantGlobalization`) | Inspection; P01–P10 run with a minimal environment | Verified |
| CONV-07 | Short options, attached values | LLD §3.1 | `ArgumentParser.vb::ParseShort` | `ArgumentParserTests.BundledShortOptionsWithAttachedValues`, `.ShortOptionConsumesRestOfBundle`, P06 | Verified |
| CONV-08 | Long options, no abbreviations | LLD §3.1 | `ArgumentParser.vb::ParseLong` | `ArgumentParserTests.NameSpellingsAreEquivalent`, `.SyntaxErrors` (`--nam`), P04 | Verified |
| CONV-09 | `--` terminator | LLD §3.1 | `ArgumentParser.vb::Parse` | `ArgumentParserTests.TerminatorMakesFollowingTokensOperands` | Verified |
| CONV-10 | Operands, `-` is an operand | LLD §3.1 | `ArgumentParser.vb::Parse` | `ArgumentParserTests.SingleDashIsAnOperand`, `.OperandsAreCollectedNotRejected`, P10 | Verified |
| CONV-11 | Repetition, last wins | LLD §3.1 | `ArgumentParser.vb::Parse` | `ArgumentParserTests.LastOccurrenceWins` | Verified |
| CONV-12 | Scan order | HLD D3 | `ArgumentParser.vb::Parse`, `CommandLineTool.vb::Run` | `ArgumentParserTests.HelpWinsWhenReachedBeforeASyntaxError`, `.FirstOfHelpAndVersionWins`, `.SyntaxErrorBeforeHelpIsReported`, `CommandLineToolTests.HelpIsTheUserManualText` | Verified |
| CONV-13 | Exact usage messages | LLD §2.3 | `DpHello/UsageError.vb` | `ArgumentParserTests.SyntaxErrors`, `ValidatorTests.*`, P09, P10 | Verified |
| CONV-14 | Try line, status 2 | LLD §3.5, §3.6 | `Output.vb::Diagnostic`, `CommandLineTool.vb::Run` | `CommandLineToolTests.UsageErrorsWriteNothingToStdout`, P09, P10 | Verified |
| CONV-15 | Values verbatim, `''` for empty | LLD §2.3 | `UsageError.vb::InvalidArgument` | `ValidatorTests.EmptyNameIsRejected`, `.InvalidCountsAreRejectedWithTheValueAndSpelling` | Verified |
| CONV-18 | Exit statuses 0 / 2 | HLD §6 | `CommandLineTool.vb::ExitSuccess`, `::ExitUsage` | `CommandLineToolTests.*`, P01–P10 | Verified |
| CONV-22 | `--help`, `--version`, no argument | LLD §3.1 | `ArgumentParser.vb::ParseLong` | `ArgumentParserTests.SyntaxErrors` (`--help=`, `--version=1`) | Verified |
| CONV-23 | Help verbatim, trailing LF | LLD §3.6 | `CommandLineTool.vb::HelpText` | `CommandLineToolTests.HelpIsTheUserManualText`, P07 | Verified |
| CONV-24 | No crash, no stack trace | HLD §6 | as NFR-03 | as NFR-03 | Verified |
| CONV-25 | Broken pipe: quiet exit | HLD §6, LLD §3.5 | `Output.vb::WriteAll`, `Program.vb::Main` | `OutputTests.WriteAllReportsAFailingStream`, `.WriteAllReportsAClosedStream`; manual check `dp-hello -c 1000 > /dev/full` exits 141 silently | Verified |
| CONV-26 | stdin not read | LLD §3.7 | `Program.vb::Main` (never opens stdin) | Inspection; P01–P10 (stdin is the null device) | Verified |
| CONV-27 … CONV-30 | Harness conditions | HLD §5 | `bin/dp-hello` apphost; `../Makefile::BIN_DIR` | P01–P10 under `run-tests.py` | Verified |
| CONV-31 | Memory above runtime baseline | HLD §7 | — | MTP §4.4 measurement | Verified |

## 3. Backward traceability (test → requirement)

| Test | Type | Requirement(s) verified |
| :-- | :-- | :-- |
| P01 | black-box, public | FR-01, CONV-03 |
| P02, P03 | black-box, public | FR-02 |
| P04 | black-box, public | FR-02, CONV-08 |
| P05 | black-box, public | FR-04 |
| P06 | black-box, public | FR-04, CONV-07 |
| P07 | black-box, public | FR-07, CONV-23 |
| P08 | black-box, public | FR-08, CONV-02 |
| P09 | black-box, public | FR-10, FR-11, CONV-13, CONV-14 |
| P10 | black-box, public | FR-09, FR-11, CONV-10 |
| `ArgumentParserTests.*` (15 methods) | unit | FR-02, FR-06, FR-07, FR-08, FR-09, FR-10, CONV-07 … CONV-13, CONV-22 |
| `ValidatorTests.*` (7 methods) | unit | FR-01, FR-03, FR-04, FR-05, FR-06, FR-09, CONV-15 |
| `GreetingTests.*` (4 methods) | unit | FR-01, FR-02, FR-04, NFR-05 |
| `OutputTests.*` (6 methods) | unit | CONV-01, CONV-03, CONV-05, CONV-14, CONV-25, NFR-03 |
| `CommandLineToolTests.*` (8 methods) | unit (end-to-end core) | FR-01 … FR-11, CONV-02, CONV-12, CONV-14, CONV-23 |

## 4. Coverage summary

| Category | Total | Designed | Implemented | Verified |
| :-- | :-- | :-- | :-- | :-- |
| User requirements (UR) | 12 | 12 | 12 | 12 |
| Functional (FR) | 11 | 11 | 11 | 11 |
| Non-functional (NFR) | 7 | 7 | 7 | 7 |
| Conventions (CONV) | 25 applicable | 25 | 25 | 25 |

## 5. Gaps and justifications
No applicable requirement is unverified. Not applicable to `dp-hello`, which reads no input
and uses no paths: CONV-04 (copying input), CONV-16 and CONV-17 (file errors), and
CONV-19 … CONV-21 (paths and traversal). The CONV-13 "missing operand" row does not apply
(no operand is required).
