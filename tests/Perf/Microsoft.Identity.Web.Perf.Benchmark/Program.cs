using System;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Toolchains.InProcess.Emit;

namespace Microsoft.Identity.Web.Perf.Benchmark
{
    class Program
    {
        static void Main(string[] args)
        {
            //BenchmarkRunner.Run<TokenAcquisitionTests>(new DebugInProcessConfig());
            BenchmarkSwitcher
                .FromAssembly(typeof(Program).Assembly)
                .Run(args, DefaultConfig.Instance.AddJob(Job.InProcess.WithToolchain(InProcessEmitToolchain.Instance)));
            Console.ReadKey();
        }
    }
}
