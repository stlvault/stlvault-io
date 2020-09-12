# StlVault.IO
[![NuGet Badge](https://buildstats.info/nuget/StlVault.IO)](https://www.nuget.org/packages/StlVault.IO/)

The 3d model file parsers extracted from [STL Vault](http://stlvault.com).

## Capabilities
Currently this package only supports reading [STL files](https://en.wikipedia.org/wiki/STL_(file_format)).
Both, binary and ASCII STL files are supported.

Future versions will support a wider range of 3d model files (`.obj`, etc.) as well as writing of some formats.

## NuGet Packages
NuGet Packages are available on [NuGet.org](https://www.nuget.org/packages/StlVault.IO/). Current version is still a preview release and might get breaking API changes.

## Examples / Usage
TBD - look at `src/Benchmarks` for now.

## Performance
The parsers in this package are designed to operate with good throughput and low memory usage:

```
BenchmarkDotNet=v0.12.1, OS=Windows 10.0.19041.450 (2004/?/20H1)
AMD Ryzen 7 3700X, 1 CPU, 16 logical and 8 physical cores
.NET Core SDK=3.1.302
  [Host]     : .NET Core 3.1.7 (CoreCLR 4.700.20.36602, CoreFX 4.700.20.37001), X64 RyuJIT
  Job-ICKZDG : .NET Core 3.1.7 (CoreCLR 4.700.20.36602, CoreFX 4.700.20.37001), X64 RyuJIT
```

|   Method |      File |      Mean |     Error |   StdDev | Gen 0 | Allocated |
|--------- |---------- |----------:|----------:|---------:|------:|----------:|
| ParseSTL | bin-030mb |  15.66 ms |  4.733 ms | 1.688 ms |     - |   4.61 KB |
| ParseSTL | bin-350mb | 169.88 ms | 12.175 ms | 4.342 ms |     - |   4.63 KB |
| ParseSTL | txt-020mb |  31.22 ms |  0.846 ms | 0.302 ms |     - |   4.98 KB |
| ParseSTL | txt-220mb | 322.18 ms | 14.137 ms | 5.041 ms |     - |   4.98 KB |
