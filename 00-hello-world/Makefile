# 00-hello-world: Standard Terminal Hello World, in Visual Basic (VB.NET on .NET 10).
#
# Extends the universal build contract in shared/templates/BASE-MAKEFILE:
#
#   make all | install | uninstall | test | check | lint | format | clean | distclean
#   make test-hidden | grade | help
#
# Toolchain: the dotnet CLI (SDK 10.0, pinned by global.json). DpHello.sln holds three projects
# under src/: DpHello (class library: parsing, validation, greeting, output encoding),
# DpHello.Cli (the console app, assembly and apphost name dp-hello), and DpHello.Tests (xUnit).
# Directory.Build.props enables Option Strict/Explicit/Infer, TreatWarningsAsErrors, the
# recommended .NET analyzers, and NuGet lock files (packages.lock.json, restored in locked mode).
#
# Build: a framework-dependent, single-file publish for the current platform puts one native
# apphost, bin/dp-hello (bin/dp-hello.exe on Windows), into BIN_DIR. It needs the .NET 10
# runtime, found through DOTNET_ROOT or the standard install location.
#
# Install copies that executable to $(BINDIR); uninstall removes it.
#
# Container-first: goals run in the toolchain container built from the repository's
# Containerfile (podman, then docker, then local; override with DP_RUNTIME).

PROJECT := 00-hello-world
TOOLS   := dp-hello

# Path to the sdlc-curriculum checkout (the specs, test suites, and runner live there).
CURRICULUM_ROOT ?= ../sdlc-curriculum

DOTNET   ?= dotnet
SOLUTION := DpHello.sln
CLI      := src/DpHello.Cli/DpHello.Cli.vbproj

# Directory that contains the built executables after `make all`.
BIN_DIR := bin

RESTORE = $(DOTNET) restore $(SOLUTION) --locked-mode

define DP_BUILD
$(RESTORE)
$(DOTNET) publish $(CLI) --configuration Release --use-current-runtime --no-restore --output $(BIN_DIR)
endef

define DP_UNIT_TEST
$(RESTORE)
$(DOTNET) test $(SOLUTION) --configuration Release --no-restore
endef

# The build fails on any compiler or analyzer warning (TreatWarningsAsErrors), and
# dotnet format fails on any whitespace, style, or analyzer change it would make.
define DP_LINT
$(RESTORE)
$(DOTNET) build $(SOLUTION) --configuration Release --no-restore
$(DOTNET) format $(SOLUTION) --verify-no-changes --no-restore
endef

define DP_FORMAT
$(RESTORE)
$(DOTNET) format $(SOLUTION) --no-restore
endef

define DP_INSTALL
mkdir -p "$(BINDIR)"
cp "$(BIN_DIR)/dp-hello$(EXE)" "$(BINDIR)/dp-hello$(EXE)"
chmod +x "$(BINDIR)/dp-hello$(EXE)"
"$(BINDIR)/dp-hello$(EXE)" --version
endef

DP_UNINSTALL = rm -f "$(BINDIR)/dp-hello$(EXE)"

define DP_CLEAN
rm -rf $(BIN_DIR) src/*/bin src/*/obj TestResults src/*/TestResults
endef

# The NuGet package cache lives in the user's home (~/.nuget), not in the project.
DP_DISTCLEAN = rm -rf .vs

include $(CURRICULUM_ROOT)/shared/templates/BASE-MAKEFILE
