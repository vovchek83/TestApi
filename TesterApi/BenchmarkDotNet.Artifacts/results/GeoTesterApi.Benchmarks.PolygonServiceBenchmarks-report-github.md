``` ini

BenchmarkDotNet=v0.13.2, OS=Windows 11 (10.0.26200.8246)
AMD Ryzen AI 7 350 w/ Radeon 860M, 1 CPU, 16 logical and 8 physical cores
.NET SDK=10.0.202
  [Host]     : .NET 8.0.26 (8.0.2626.16921), X64 RyuJIT AVX2
  Job-TVZODC : .NET 8.0.26 (8.0.2626.16921), X64 RyuJIT AVX2

IterationCount=5  WarmupCount=3  

```
|                                    Method |         Mean |        Error |      StdDev |          Min |          Max |       Median |   Gen0 | Allocated |
|------------------------------------------ |-------------:|-------------:|------------:|-------------:|-------------:|-------------:|-------:|----------:|
|       &#39;Create simple rectangular polygon&#39; |     175.2 ns |      8.61 ns |     1.33 ns |     173.6 ns |     176.8 ns |     175.2 ns | 0.0060 |     992 B |
|  &#39;Create multi-polygon with 2 rectangles&#39; |  16,073.5 ns |  1,079.45 ns |   280.33 ns |  15,801.6 ns |  16,479.2 ns |  15,922.5 ns | 0.1831 |   31704 B |
|          &#39;Create donut polygon with hole&#39; |   2,286.7 ns |    132.09 ns |    20.44 ns |   2,263.9 ns |   2,304.1 ns |   2,289.5 ns | 0.0534 |    9120 B |
| &#39;Create and parse multi-polygon from WKT&#39; | 406,263.0 ns | 24,257.60 ns | 6,299.62 ns | 400,928.8 ns | 416,196.5 ns | 403,157.3 ns | 2.9297 |  543271 B |
