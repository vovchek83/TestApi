``` ini

BenchmarkDotNet=v0.13.2, OS=Windows 11 (10.0.26200.8246)
AMD Ryzen AI 7 350 w/ Radeon 860M, 1 CPU, 16 logical and 8 physical cores
.NET SDK=10.0.202
  [Host]     : .NET 8.0.26 (8.0.2626.16921), X64 RyuJIT AVX2
  Job-TVZODC : .NET 8.0.26 (8.0.2626.16921), X64 RyuJIT AVX2

IterationCount=5  WarmupCount=3  

```
|                                              Method |        Mean |        Error |     StdDev |         Min |         Max |      Median |   Gen0 | Allocated |
|---------------------------------------------------- |------------:|-------------:|-----------:|------------:|------------:|------------:|-------:|----------:|
|               &#39;Single elevation lookup (Kyiv area)&#39; |    56.69 ns |     4.226 ns |   1.098 ns |    55.16 ns |    57.91 ns |    57.14 ns | 0.0006 |     112 B |
|                 &#39;Grid of 9 elevation lookups (3x3)&#39; |   346.22 ns |    38.568 ns |   5.968 ns |   338.25 ns |   352.68 ns |   346.97 ns | 0.0038 |     672 B |
|       &#39;Dense grid of 100 elevation lookups (10x10)&#39; | 6,388.17 ns |   567.521 ns |  87.825 ns | 6,272.78 ns | 6,465.32 ns | 6,407.29 ns | 0.0763 |   12320 B |
|      &#39;Sequential lookups along a line (100 points)&#39; | 5,989.08 ns | 1,348.723 ns | 350.259 ns | 5,609.75 ns | 6,527.79 ns | 5,934.12 ns | 0.0687 |   11200 B |
|            &#39;Boundary test: corner and edge lookups&#39; |   294.49 ns |    46.604 ns |   7.212 ns |   285.03 ns |   300.34 ns |   296.29 ns | 0.0033 |     560 B |
| &#39;Same tile cache hit (multiple lookups, same tile)&#39; | 3,005.87 ns |   510.799 ns | 132.653 ns | 2,820.21 ns | 3,121.46 ns | 3,062.26 ns | 0.0343 |    5600 B |
