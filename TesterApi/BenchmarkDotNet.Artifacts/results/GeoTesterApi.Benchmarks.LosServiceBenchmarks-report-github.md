``` ini

BenchmarkDotNet=v0.13.2, OS=Windows 11 (10.0.26200.8246)
AMD Ryzen AI 7 350 w/ Radeon 860M, 1 CPU, 16 logical and 8 physical cores
.NET SDK=10.0.202
  [Host]     : .NET 8.0.26 (8.0.2626.16921), X64 RyuJIT AVX2
  Job-TVZODC : .NET 8.0.26 (8.0.2626.16921), X64 RyuJIT AVX2

IterationCount=5  WarmupCount=3  

```
|                                         Method |       Mean |      Error |    StdDev |        Min |        Max |     Median |   Gen0 |   Gen1 | Allocated |
|----------------------------------------------- |-----------:|-----------:|----------:|-----------:|-----------:|-----------:|-------:|-------:|----------:|
|              &#39;LOS calculation with 50 samples&#39; |  15.942 μs |  0.5780 μs | 0.1501 μs |  15.705 μs |  16.072 μs |  16.018 μs | 0.0610 |      - |  10.52 KB |
|             &#39;LOS calculation with 500 samples&#39; | 152.168 μs | 17.0325 μs | 4.4233 μs | 145.777 μs | 157.363 μs | 153.462 μs | 0.6104 |      - | 101.93 KB |
|            &#39;LOS calculation with 2000 samples&#39; | 341.320 μs | 26.7568 μs | 4.1406 μs | 336.637 μs | 346.613 μs | 341.015 μs | 2.4414 | 0.4883 | 406.62 KB |
| &#39;LOS calculation with high frequency (28 GHz)&#39; |   8.544 μs |  0.6192 μs | 0.0958 μs |   8.401 μs |   8.607 μs |   8.583 μs | 0.0610 |      - |  10.65 KB |
| &#39;LOS calculation with low frequency (400 MHz)&#39; |   8.720 μs |  1.4512 μs | 0.3769 μs |   8.428 μs |   9.297 μs |   8.495 μs | 0.0610 |      - |  10.65 KB |
