using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;

namespace Benchmarks
{
    public static class Program
    {
        public static void Main()
        {
            BenchmarkRunner.Run<ParserBenchmarks>();
        }
    }
}