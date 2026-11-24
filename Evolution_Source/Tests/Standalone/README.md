# Standalone checks

`dotnet run --project Evolution_Source/Tests/Standalone/Evolution.Core.Tests.csproj`
compiles and exercises the actual C# core with .NET 8, without Unity or test packages.
Do not copy this console project into Unity Assets.

`python3 Evolution_Source/Tests/verify_geometry.py` runs an independent mathematical
reference and structural checks. It is NOT C# compilation or Unity validation.

CI additionally compiles the core against .NET Standard 2.1 with C# 9:

`dotnet build Evolution_Source/Tests/Compatibility/Evolution.Core.Compatibility.csproj -c Release`

RegressionTests covers float-quantized geometry at every supported resolution,
polygon/fan area, invalid numeric inputs, 2,000 target changes and 10,000 velocity cases.
Neither standalone project compiles UnityEngine-facing adapters.
