# Standalone checks

`dotnet run --project Evolution_Source/Tests/Standalone/Evolution.Core.Tests.csproj`
compiles and exercises the actual C# core with .NET 8, without Unity or test packages.
Do not copy this console project into Unity Assets.

`python3 Evolution_Source/Tests/verify_geometry.py` runs an independent mathematical
reference and structural checks. It is NOT C# compilation or Unity validation.
