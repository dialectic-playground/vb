<!-- START doctoc generated TOC please keep comment here to allow auto update -->
<!-- DON'T EDIT THIS SECTION, INSTEAD RE-RUN doctoc TO UPDATE -->

**Table of Contents** _generated with [DocToc](https://github.com/thlorenz/doctoc)_

- [vb](#vb)
  - [Visual Basic at a glance](#visual-basic-at-a-glance)
  - [Toolchain](#toolchain)
  - [Layout](#layout)
  - [Development](#development)
  - [Implementing a project](#implementing-a-project)
  - [Contributing](#contributing)
  - [License](#license)

<!-- END doctoc generated TOC please keep comment here to allow auto update -->

<p align="center">
  <picture>
    <source media="(prefers-color-scheme: dark)" srcset="https://raw.githubusercontent.com/dialectic-playground/.github/main/assets/vb/dist/logo-dark.svg">
    <img alt="dialectic playground / vb" src="https://raw.githubusercontent.com/dialectic-playground/.github/main/assets/vb/dist/logo.svg" height="96">
  </picture>
</p>

# vb

Visual Basic implementation of the Dialectic Playground SDLC curriculum.

📖 **Documentation:** <https://dialectic-playground.github.io/docs/reference/languages/vb/>

This repository implements the eleven projects of the
[SDLC curriculum](https://github.com/dialectic-playground/sdlc-curriculum) in Visual Basic. Each project is
specified to the byte by the curriculum and verified by its black-box tests on Linux, macOS,
and Windows.

**Status:** Implemented projects: 00-hello-world.

## Visual Basic at a glance

| Aspect                  | Visual Basic                                            |
| :---------------------- | :------------------------------------------------------ |
| File extension          | `.vb`                                                   |
| CLI classification      | Included                                                |
| Image category          | First-Class                                             |
| Access to OS primitives | Medium-High (.NET platform hooks)                       |
| Entry-point mechanism   | Runtime initializes and maps arguments to Main function |

## Toolchain

|                      |                                                                                          |
| :------------------- | :--------------------------------------------------------------------------------------- |
| Release line         | VB.NET on .NET 10 (LTS)                                                                  |
| Build / package tool | dotnet CLI (NuGet)                                                                       |
| Container image      | `mcr.microsoft.com/dotnet/sdk:10.0`, pinned by digest ([`Containerfile`](Containerfile)) |
| Native CI            | actions/setup-dotnet (10.0.x)                                                            |

`make toolchain` verifies that the container and native toolchains are on this release line.

| Tool                      | Use                                                                                   | Source                                               |
| :------------------------ | :------------------------------------------------------------------------------------ | :--------------------------------------------------- |
| xUnit 2.9 (`dotnet test`) | Unit tests                                                                            | NuGet, locked in each project's `packages.lock.json` |
| Compiler + .NET analyzers | Lint: `TreatWarningsAsErrors`, `Option Strict On`, `AnalysisLevel` latest-recommended | the SDK                                              |
| `dotnet format`           | Lint (`--verify-no-changes`) and format                                               | the SDK (`make toolchain` prints its version)        |

Each project directory holds a solution, `global.json` (SDK 10.0, rolling forward within
10.0), and `Directory.Build.props`. NuGet restores run with `--locked-mode`; Dependabot keeps
the packages current. `make all` publishes a framework-dependent, single-file apphost (for
example `00-hello-world/bin/dp-hello`, `.exe` on Windows), so running or installing a tool
needs the .NET 10 runtime. The apphost finds it through `DOTNET_ROOT`, the registered
install location (`/etc/dotnet/install_location` on Linux and macOS, written by the
official installers), or the default location. If you installed .NET elsewhere (for example
with Homebrew or `dotnet-install`), set `DOTNET_ROOT` or register the location; native CI
does the latter because the black-box runner clears the environment.

## Layout

| Path                                  | Contents                                                                                                          |
| :------------------------------------ | :---------------------------------------------------------------------------------------------------------------- |
| `00-hello-world/` … `10-sys-monitor/` | One workspace per curriculum project: `Makefile` (with `DP_*` hooks), HLD, LLD, RTM, master test plan, and `src/` |
| `sdlc-curriculum/`                    | The curriculum, as a git submodule pinned to a release tag                                                        |
| `Makefile`                            | `make check` (repository, toolchain, implemented projects), `toolchain`, `projects`, `roundtrip`                  |
| `Containerfile`, `.devcontainer/`     | The toolchain image and the matching dev container                                                                |
| `tools/dp-run`, `tools/container.mk`  | Shared container tooling (copies of the canonical files in `dialectic-playground/.github`)                        |
| `.github/workflows/ci.yml`            | CI: container and native (Linux, macOS, Windows) `make` targets, shared-tooling check, DCO                        |

## Development

Container-first: `make` targets and `tools/dp-run <command>` run in the toolchain container
via Podman, then Docker, then the local system. `DP_RUNTIME=podman|docker|local` overrides
the choice.

```sh
git clone --recurse-submodules https://github.com/dialectic-playground/vb.git
cd vb
make check                    # repository, toolchain, and every implemented project
DP_RUNTIME=local make check   # the same with your native toolchain
make -C 00-hello-world help   # a project's configuration and hooks
```

## Implementing a project

1. Complete the project's `hld.md` and `lld.md` before writing code.
2. Write the implementation and its unit tests under the project's `src/`, add the
   repository's dotnet CLI (NuGet) configuration, and fill in `BIN_DIR` and the `DP_*` hooks in
   the project's `Makefile`.
3. Pass `make -C <project> test` (container) and `DP_RUNTIME=local make -C <project> test`.
4. Add the project directory to `IMPLEMENTED` in the root `Makefile`. CI then builds, lints,
   and tests it in the container and natively on all three operating systems.
5. Keep `rtm.md` and `master-test-plan.md` current.

Specifications: [curriculum reference](https://dialectic-playground.github.io/docs/reference/curriculum/). Language setup:
[Set up a language repository](https://dialectic-playground.github.io/docs/how-to/set-up-a-language-repository/).

## Contributing

See the [organization contributing guide](https://github.com/dialectic-playground/.github/blob/main/CONTRIBUTING.md).
Pull requests are squash-merged, titles follow Conventional Commits, and every commit needs a
DCO sign-off.

## License

[MIT](LICENSE)
