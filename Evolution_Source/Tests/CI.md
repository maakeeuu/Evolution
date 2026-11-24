# Editor-independent CI

`.github/workflows/ev-cp1-core.yml` is the minimal GitHub-required workflow entry point.
All game/test source remains in Evolution_Source. It runs on relevant pushes, PRs,
or manual dispatch, with read-only repository permissions and no project secrets.

The matrix compiles and runs actual C# sources with .NET 8 for each of the three
published checkpoints and the current revision. It also runs the independent Python
suite. Checkout does not fetch submodules or persist credentials. Logs are retained
as Actions artifacts for 14 days. Any build error or failed assertion fails the job.

A successful core job proves standalone C# compilation/tests only. It cannot prove
Unity API/shader compilation, Box2D contacts, rendering or Unity 6000.6.10f1 integration.
Current run conclusions must be checked in Actions; workflow configuration alone is
not a passing result. No workflow automatically edits code or commits results.

Current revisions also compile Code/Core using C# 9 against netstandard2.1 in
Tests/Compatibility. This checks portable API usage, not UnityEngine compatibility.
Runs #1 and #2 completed successfully on 2026-11-24:
- https://github.com/maakeeuu/Evolution/actions/runs/45410854846
- https://github.com/maakeeuu/Evolution/actions/runs/45412090992

Tests/Unity/PlayMode is excluded from these standalone projects. Its tests are pending
real Unity compilation/Play Mode, not part of the successful CI assertion count.

## Integration Readiness checks

Every current job verifies its actual checkout SHA, builds the netstandard2.1 core,
runs the net8.0 core tests, builds/runs AdapterSyntax and runs Python checks. Progress
and decisions changes also trigger this same workflow, preventing an untested docs-only
HEAD at a checkpoint. Historical jobs intentionally skip newer projects they lack.

AdapterSyntax uses Roslyn assemblies shipped with the installed .NET 8 SDK; no NuGet
or Unity package is added. It parses actual adapters/PlayMode source using C# 9 and
checks MonoBehaviour filenames, callback signatures and RequireComponent arity.
It does NOT resolve UnityEngine types, compile against Unity APIs, run callbacks, or
simulate resources/physics. No Unity mocks/stubs are used or claimed as validation.

The core allocation probe warms up reusable buffers then measures only the core math/
validation hot path. A zero result is not a measurement of Unity APIs or the renderer.

SDK selection is now enforced with Evolution_Source/global.json, not merely installed
by setup-dotnet: hosted runners can also have a newer SDK that otherwise wins resolution.
Run commands from Evolution_Source, e.g.:

```sh
cd Evolution_Source
dotnet build Tests/Compatibility/Evolution.Core.Compatibility.csproj -c Release
dotnet run --project Tests/Standalone/Evolution.Core.Tests.csproj -c Release
dotnet run --project Tests/AdapterSyntax/Evolution.AdapterSyntax.csproj -c Release -- .
```

Historical checkout jobs receive the same ephemeral SDK policy because their old source
has no global.json. Their original repository history is not modified. CI asserts 8.0.x
before compilation and preserves the SDK version alongside the output logs.

## Integrated Assets gate

Current revisions also parse actual Assets C# sources (including legacy/editor source)
with UNITY_EDITOR, ENABLE_LEGACY_INPUT_MANAGER and UNITY_INCLUDE_TESTS defined. This
still performs syntax/contracts only, not Unity API compilation. The Python integration
suite checks the exact runtime mirror, assembly separation, Resources key uniqueness,
legacy preservation, engine version and required built-in Physics 2D module. CI triggers
on Assets/Packages/ProjectSettings/README/manual-checklist changes as well.


## EV-CP3 current gate
See docs/EV_CP3_VALIDATION.md at repository root. Actual standalone rod geometry assertions run from Tests/Articulated with a dedicated CI annotation. Local references/structure do not execute Unity. Two CP3 PlayMode cases are prepared only (read-only native state, force-driven bend changes published Mesh); native acceptance and shell-only straight/bent capture remain pending. Existing nine CP2 native cases are not replaced or declared passed.
