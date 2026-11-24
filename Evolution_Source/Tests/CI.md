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
