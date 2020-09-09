using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using IntegrationTestService;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Identity.Web.Perf.Benchmark
{
    [SimpleJob(RunStrategy.Throughput, launchCount: 1, warmupCount: 0, targetCount: 1)]
    public class TokenAcquisitionTests
    {
        private ITokenAcquisition _tokenAcquisition;
        private ServiceProvider _serviceProvider;
        private WebApplicationFactory<Startup> _factory;
        private IServiceScope _requestScope;

        public TokenAcquisitionTests()
        {
            _factory = new WebApplicationFactory<Startup>();
        }

        [GlobalSetup]
        public void GlobalSetup()
        {
            //System.Diagnostics.Debugger.Launch();
            _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    _serviceProvider = services.BuildServiceProvider();
                });
            })
            .CreateClient();
            _requestScope = _serviceProvider.CreateScope();
            _tokenAcquisition = _requestScope.ServiceProvider.GetService<ITokenAcquisition>();
        }

        [GlobalCleanup]
        public void GlobalCleanup()
        {
            _serviceProvider.Dispose();
        }

        [Benchmark]
        public async Task<string> GetAccessTokenForUserAsync()
        {
            return await _tokenAcquisition.GetAccessTokenForUserAsync(
                new[] { "user.read" }).ConfigureAwait(false);
        }
    }
}
