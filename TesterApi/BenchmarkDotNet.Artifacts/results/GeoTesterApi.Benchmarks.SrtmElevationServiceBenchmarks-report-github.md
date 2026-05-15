``` ini

BenchmarkDotNet=v0.13.2, OS=Windows 11 (10.0.26200.8246)
AMD Ryzen AI 7 350 w/ Radeon 860M, 1 CPU, 16 logical and 8 physical cores
.NET SDK=10.0.202
  [Host]     : .NET 8.0.26 (8.0.2626.16921), X64 RyuJIT AVX2
  Job-OXATKJ : .NET 8.0.26 (8.0.2626.16921), X64 RyuJIT AVX2

IterationCount=5  WarmupCount=3  

```
|                                              Method |        Mean |       Error |      StdDev |         Min |         Max |      Median |   Gen0 | Allocated |
|---------------------------------------------------- |------------:|------------:|------------:|------------:|------------:|------------:|-------:|----------:|
|               &#39;Single elevation lookup (Kyiv area)&#39; |    174.1 ns |    37.81 ns |     9.82 ns |    164.3 ns |    186.4 ns |    168.6 ns | 0.0005 |     112 B |
|                 &#39;Grid of 9 elevation lookups (3x3)&#39; |    937.9 ns |   194.68 ns |    50.56 ns |    873.9 ns |    992.0 ns |    942.3 ns | 0.0038 |     672 B |
|       &#39;Dense grid of 100 elevation lookups (10x10)&#39; | 19,820.0 ns | 3,853.72 ns | 1,000.80 ns | 18,747.9 ns | 21,252.4 ns | 19,656.6 ns | 0.0610 |   12320 B |
|      &#39;Sequential lookups along a line (100 points)&#39; | 17,920.1 ns | 1,945.67 ns |   505.28 ns | 17,362.6 ns | 18,691.5 ns | 17,722.3 ns | 0.0610 |   11200 B |
|            &#39;Boundary test: corner and edge lookups&#39; |    821.5 ns |    58.47 ns |    15.18 ns |    796.3 ns |    834.3 ns |    825.4 ns | 0.0019 |     560 B |
| &#39;Same tile cache hit (multiple lookups, same tile)&#39; |  8,499.9 ns | 2,540.12 ns |   659.66 ns |  7,761.5 ns |  9,209.3 ns |  8,202.3 ns | 0.0305 |    5600 B |
