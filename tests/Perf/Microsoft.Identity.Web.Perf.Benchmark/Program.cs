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
            BenchmarkRunner.Run(typeof(Program).Assembly, 
                DefaultConfig.Instance.AddJob(
                    Job.InProcess
                        .WithToolchain(InProcessEmitToolchain.Instance)
                        .WithLaunchCount(1)
                        .WithInvocationCount(1)
                        .WithIterationCount(1)
                        .WithWarmupCount(0)
                        .WithUnrollFactor(1)
                        ));
            //BenchmarkSwitcher
            //    .FromAssembly(typeof(Program).Assembly)
            //    .RunAll(DefaultConfig.Instance.AddJob(Job.InProcess.WithToolchain(InProcessEmitToolchain.Instance)));
            Console.ReadKey();
        }
    }
}
