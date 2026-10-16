<!-- START doctoc generated TOC please keep comment here to allow auto update -->
<!-- DON'T EDIT THIS SECTION, INSTEAD RE-RUN doctoc TO UPDATE -->

**Table of Contents** _generated with [DocToc](https://github.com/thlorenz/doctoc)_

- [Master Test Plan (MTP) — Standard Terminal Hello World](#master-test-plan-mtp--standard-terminal-hello-world)
  - [1. Introduction and scope](#1-introduction-and-scope)
    - [1.1 Objectives](#11-objectives)
    - [1.2 In scope / out of scope](#12-in-scope--out-of-scope)
  - [2. Test items](#2-test-items)
  - [3. Test levels and approach](#3-test-levels-and-approach)
  - [4. Test design](#4-test-design)
    - [4.1 Equivalence partitions and boundaries](#41-equivalence-partitions-and-boundaries)
    - [4.2 Additional test cases (beyond the supplied suite)](#42-additional-test-cases-beyond-the-supplied-suite)
    - [4.3 Negative and robustness testing](#43-negative-and-robustness-testing)
    - [4.4 Performance and resource testing](#44-performance-and-resource-testing)
  - [5. Test environments](#5-test-environments)
  - [6. Entry and exit criteria](#6-entry-and-exit-criteria)
  - [7. Defect management](#7-defect-management)
  - [8. Schedule and deliverables](#8-schedule-and-deliverables)
  - [9. Risks and contingencies](#9-risks-and-contingencies)
  - [10. Test summary (complete at the end)](#10-test-summary-complete-at-the-end)

<!-- END doctoc generated TOC please keep comment here to allow auto update -->

# Master Test Plan (MTP) — Standard Terminal Hello World

| Field                   | Value                                                  |
| :---------------------- | :----------------------------------------------------- |
| Project                 | `00-hello-world`                                       |
| Tools                   | dp-hello                                               |
| Implementation language | Visual Basic (VB.NET, .NET 10)                         |
| Author                  | Brandon Perkins (with Claude Code)                     |
| Version / date          | 1.0 — 2026-10-13                                       |
| Standard                | Structure adapted from IEEE 829 / ISO/IEC/IEEE 29119-3 |

## 1. Introduction and scope

### 1.1 Objectives

"Tested enough" means: every SRS acceptance criterion (FR-01 … FR-11) is exercised by an
automated test, every CONV-13 diagnostic is checked byte for byte, the public and hidden
black-box suites pass on Linux, macOS, and Windows, and the RTM shows every applicable
requirement as Verified.

### 1.2 In scope / out of scope

| In scope                                                                            | Out of scope                                                                            |
| :---------------------------------------------------------------------------------- | :-------------------------------------------------------------------------------------- |
| Every FR/NFR in the SRS and the applicable CONV rules, on Linux, macOS, and Windows | File, path, and stdin behavior (CONV-04, CONV-16 … CONV-21: not applicable)             |
| The `make` contract (all BASE-MAKEFILE targets, install/uninstall round trip)       | Terminal rendering of non-ASCII names (a console concern, not the tool's)               |
| Start-up time and memory (NFR-02, CONV-31)                                          | Invalid UTF-8 byte sequences in POSIX arguments (decoded to U+FFFD by .NET; see HLD §8) |

## 2. Test items

| Item                   | Version / location                                    |
| :--------------------- | :---------------------------------------------------- |
| dp-hello               | `00-hello-world/src/` on branch `feat/00-hello-world` |
| Unit tests             | `src/DpHello.Tests/` (40 methods, 97 cases)           |
| Public black-box suite | `tests/public/` (10 tests)                            |
| Hidden black-box suite | `tests/private/` (10 tests, run by CI)                |

## 3. Test levels and approach

| Level              | Purpose                                                        | Technique(s)                                              | Tooling                                                                             | Owner          |
| :----------------- | :------------------------------------------------------------- | :-------------------------------------------------------- | :---------------------------------------------------------------------------------- | :------------- |
| Unit               | Parser, validator, greeting, output in isolation               | Equivalence partitioning, boundary values, error guessing | xUnit 2.9 (`dotnet test`, `make test`)                                              | Developer      |
| Integration        | Parser → validator → greeting → encoding through the pure core | Specification-based, byte-exact assertions                | `CommandLineToolTests` (xUnit)                                                      | Developer      |
| System (black-box) | The specified CLI behavior, byte-exact                         | Specification-based                                       | `make test` → `run-tests.py`                                                        | Developer      |
| Acceptance         | Hidden suite on all OSes                                       | —                                                         | CI `make check` (runs `test-hidden`)                                                | CI             |
| Static analysis    | Compiler and analyzer warnings, formatting                     | —                                                         | `make lint`: build with TreatWarningsAsErrors + `dotnet format --verify-no-changes` | Developer / CI |

## 4. Test design

### 4.1 Equivalence partitions and boundaries

| Input         | Valid partitions                                                                         | Invalid partitions                                                                       | Boundaries                                                     |
| :------------ | :--------------------------------------------------------------------------------------- | :--------------------------------------------------------------------------------------- | :------------------------------------------------------------- |
| COUNT         | ASCII digits with value 1–1000, leading zeros                                            | empty; non-digits (sign, space, `.`, `e`, Unicode digits); 0; > 1000; huge digit strings | 0, 1, 1000, 1001, `000`, `0001000`, 23 digits                  |
| NAME          | any non-empty string (spaces, `!`, `-x`, non-ASCII)                                      | empty                                                                                    | `''`, `' '`                                                    |
| Option tokens | `-n`, `-c`, `--name`, `--count`, `--help`, `--version`, with attached or separate values | unknown short/long, abbreviations, `=value` on flags, missing values                     | `-` (operand), `--` (terminator), `--name=` (empty), `---name` |
| Operands      | none                                                                                     | any word, any token after `--`                                                           | `--` alone (no operand)                                        |
| Order         | `--help`/`--version` before any syntax error                                             | a syntax error before `--help`                                                           | `extra --help`, `--count 0 --help`, `-x --help`                |

### 4.2 Additional test cases (beyond the supplied suite)

| ID   | Requirement(s)  | Preconditions | Steps / command                                                             | Expected result                                            | Level                                                                                                                       |
| :--- | :-------------- | :------------ | :-------------------------------------------------------------------------- | :--------------------------------------------------------- | :-------------------------------------------------------------------------------------------------------------------------- |
| T-01 | FR-07, CONV-12  | —             | `Run("extra", "--help")`, `Run("--count", "0", "--help")`                   | Help text, status 0                                        | unit (`CommandLineToolTests.HelpIsTheUserManualText`)                                                                       |
| T-02 | CONV-12         | —             | `Parse("-x", "--help")`                                                     | `invalid option -- 'x'`                                    | unit (`ArgumentParserTests.SyntaxErrorBeforeHelpIsReported`)                                                                |
| T-03 | FR-06           | —             | `Run("-c","0","-c","1","-n","","-n","B")`                                   | `Hello, B!`, status 0                                      | unit (`CommandLineToolTests.OnlyTheWinningValueIsValidated`)                                                                |
| T-04 | FR-04           | —             | counts `007`, `0001000`, `1000`                                             | 7, 1000, 1000                                              | unit (`ValidatorTests.ValidCounts`)                                                                                         |
| T-05 | FR-05, NFR-03   | —             | count `99999999999999999999999`, `٣`, `３`, `+1`, `1`                       | `invalid argument '<v>' for '--count'`                     | unit (`ValidatorTests.InvalidCountsAreRejectedWithTheValueAndSpelling`)                                                     |
| T-06 | FR-02           | —             | `--name=-x`, `-n-x`, `--name --help`                                        | name `-x` / `--help`                                       | unit (`ArgumentParserTests.AttachedValuesAreVerbatim`, `.SeparateValueMayStartWithDash`)                                    |
| T-07 | CONV-07         | —             | `-nc3`                                                                      | name `c3`                                                  | unit (`ArgumentParserTests.ShortOptionConsumesRestOfBundle`)                                                                |
| T-08 | CONV-09, FR-09  | —             | `--`, `-- -n`, `-- --help`                                                  | no operand; `extra operand '-n'`; `extra operand '--help'` | unit (`ArgumentParserTests.TerminatorMakesFollowingTokensOperands`, `CommandLineToolTests.UsageErrorsWriteNothingToStdout`) |
| T-09 | CONV-05         | —             | `--name Zoë`                                                                | bytes `Hello, Zo\xC3\xAB!\n`                               | unit (`CommandLineToolTests.NonAsciiNameIsUtf8`)                                                                            |
| T-10 | CONV-13         | —             | `-ë`, `-😀`                                                                 | `invalid option -- 'ë'`, whole code point                  | unit (`ArgumentParserTests.SyntaxErrors`)                                                                                   |
| T-11 | CONV-25         | —             | write to a stream that throws `IOException`; `dp-hello -c 1000 > /dev/full` | `WriteAll` returns False; silent exit 141                  | unit (`OutputTests.WriteAllReportsAFailingStream`) + manual                                                                 |
| T-12 | NFR-01, CONV-03 | —             | `--count=1000`                                                              | 14 000 bytes, no CR                                        | unit (`CommandLineToolTests.MaximumCountPrintsOneThousandLines`)                                                            |
| T-13 | NFR-06          | built         | `make roundtrip`                                                            | install then uninstall leave the prefix empty              | system (CI, 3 OSes)                                                                                                         |

### 4.3 Negative and robustness testing

Every CONV-13 row is a `SyntaxErrors`/`ValidatorTests` case and an end-to-end case in
`UsageErrorsWriteNothingToStdout`, which also asserts empty stdout and status 2. Robustness
cases: overlong digit strings (saturating parser), unpaired surrogates
(`EncodeReplacesUnpairedSurrogateInsteadOfThrowing`), non-BMP option letters, a broken or
closed output stream. Missing files, permissions, and stdin do not apply (the tool reads
none).

### 4.4 Performance and resource testing

Measured in the toolchain container (Fedora host, x86-64) with a Python harness that runs
each command 20 times and reports the median wall time and the children's peak RSS
(`resource.getrusage(RUSAGE_CHILDREN).ru_maxrss`):

| Program                                                        | Median time | Peak RSS |
| :------------------------------------------------------------- | :---------- | :------- |
| Empty VB program (same publish settings): the CONV-31 baseline | 19.5 ms     | 21.5 MiB |
| `dp-hello`                                                     | 28.4 ms     | 24.8 MiB |
| `dp-hello -c 1000 -n Zoë`                                      | 29.3 ms     | 24.9 MiB |

Result: well under the 300 ms managed-runtime budget (and the 100 ms native one), and
about 3.4 MiB above the runtime baseline, far below 32 MiB (even the absolute peak is).

## 5. Test environments

| Environment       | OS / version                                                                    | Toolchain                                                     | How provisioned                                     |
| :---------------- | :------------------------------------------------------------------------------ | :------------------------------------------------------------ | :-------------------------------------------------- |
| Local / container | Debian (mcr.microsoft.com/dotnet/sdk:10.0, digest-pinned) on Fedora with Podman | .NET SDK 10.0.4xx, make, python3                              | `make check` (`Containerfile`)                      |
| CI                | ubuntu-latest, macos-latest, windows-latest                                     | .NET SDK 10.0.x (actions/setup-dotnet), Python 3.12, GNU Make | `.github/workflows/ci.yml` (`make check roundtrip`) |

## 6. Entry and exit criteria

- **Entry:** HLD/LLD written; `make all` succeeds; `make lint` is clean.
- **Exit:** all unit tests and all public and hidden black-box tests pass on all three OSes
  in CI; `make lint` and `make format` leave no diff; `make roundtrip` passes natively;
  every applicable RTM row is Verified.
- **Suspension / resumption:** suspend on any CI toolchain failure unrelated to the code
  (runner image change); resume after pinning or adapting the setup step.

## 7. Defect management

Defects are GitHub issues in `dialectic-playground/vb` labeled with severity; the fix PR
references the issue, adds a regression unit test that fails before the fix, and must pass
CI on all three OSes.

| Severity     | Definition                                                              |
| :----------- | :---------------------------------------------------------------------- |
| 1 — Critical | Crash, stack trace, or wrong stdout bytes on a mainline path            |
| 2 — Major    | Wrong diagnostic text or exit status; any OS-specific output difference |
| 3 — Minor    | Documentation, build ergonomics, or performance within budget           |

## 8. Schedule and deliverables

| Milestone               | Deliverable                        | Target date |
| :---------------------- | :--------------------------------- | :---------- |
| Design complete         | `hld.md`, `lld.md`                 | 2026-10-13  |
| Implementation complete | `src/`, unit tests, Makefile hooks | 2026-10-13  |
| Verification complete   | RTM all Verified, CI green         | 2026-10-13  |

## 9. Risks and contingencies

| Risk                                                                        | Mitigation                                                                                                  |
| :-------------------------------------------------------------------------- | :---------------------------------------------------------------------------------------------------------- |
| Windows-only failure discovered late                                        | CI runs all three OSes on every push                                                                        |
| Apphost cannot find the .NET runtime under the runner's minimal environment | CI registers `/etc/dotnet/install_location`; Windows uses the default install location                      |
| SDK patch drift between the container and native CI breaks locked restore   | No SDK-versioned implicit packages (single-file analyzer disabled); `global.json` rolls forward within 10.0 |

## 10. Test summary (complete at the end)

| Metric                                | Value                                                                           |
| :------------------------------------ | :------------------------------------------------------------------------------ |
| Unit tests (pass / total)             | 97 / 97                                                                         |
| Coverage (line / branch)              | Not measured with a tool; every public function and every diagnostic has a test |
| Public black-box (pass / 10)          | 10 / 10 (container, Linux, macOS, Windows)                                      |
| Hidden black-box (pass / 10, from CI) | 10 / 10                                                                         |
| Open defects by severity              | 0 / 0 / 0                                                                       |
