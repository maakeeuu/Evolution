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
