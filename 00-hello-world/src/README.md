# dp-hello in Visual Basic: sources

The solution `../DpHello.sln` (with `../global.json` and `../Directory.Build.props`) holds
three projects. Design: [`../hld.md`](../hld.md) and [`../lld.md`](../lld.md).

| Project | Kind | Contents |
| :-- | :-- | :-- |
| `DpHello/` | Class library | `ArgumentParser.vb` (parsing), `Validator.vb` (validation), `Greeting.vb` (pure greeting construction), `Output.vb` (UTF-8 encoding, diagnostics, raw-stream writes), `UsageError.vb` (every CONV-13 message), `ParsedArguments.vb` (parse result types), `CommandLineTool.vb` (pure `Run(args)` core) |
| `DpHello.Cli/` | Console app (`dp-hello`) | `Program.vb`: runs the core and writes its bytes to the raw standard streams |
| `DpHello.Tests/` | xUnit tests | One test class per library unit, plus end-to-end tests of `CommandLineTool.Run` |

Each project keeps its NuGet lock file (`packages.lock.json`) next to it. Build, test, lint,
and format through the project `Makefile` (`make all test lint format`).
