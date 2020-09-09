using System;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;

namespace Microsoft.Identity.Web.Perf.Benchmark
{
    class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run<TokenAcquisitionTests>();
            Console.ReadKey();
        }
    }
}
