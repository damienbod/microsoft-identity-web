using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using IntegrationTestService;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Client;
using Microsoft.Identity.Web.Test.Common;
using Microsoft.Identity.Web.Test.LabInfrastructure;

namespace Microsoft.Identity.Web.Perf.Benchmark
{
    //[SimpleJob(RunStrategy.Throughput, launchCount: 1, warmupCount: 0, targetCount: 1)]
    //[InProcess]
    public class TokenAcquisitionTests
    {
        private ITokenAcquisition _tokenAcquisition;
        private ServiceProvider _serviceProvider;
        private WebApplicationFactory<Startup> _factory;
        private IServiceScope _requestScope;
        private HttpRequestMessage _httpRequestMessage;
        private HttpClient _client;

        public TokenAcquisitionTests()
        {
            _factory = new WebApplicationFactory<Startup>();
        }

        [GlobalSetup]
        public void GlobalSetup()
        {
            _client = _factory
            .CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false,
            });
        }

        [GlobalCleanup]
        public void GlobalCleanup()
        {
            _client.Dispose();
            _factory.Dispose();
        }

        [IterationSetup]
        public void IterationSetup()
        {
            var result = AcquireTokenForLabUserAsync().GetAwaiter().GetResult();

            _httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, TestConstants.SecurePageGetTokenAsync);
            _httpRequestMessage.Headers.Add(
                "Authorization",
                string.Format(
                    CultureInfo.InvariantCulture,
                    "{0} {1}",
                    "Bearer",
                    result.AccessToken));
        }

        [IterationCleanup]
        public void IterationCleanup()
        {
            _httpRequestMessage.Dispose();
        }

        [Benchmark]
        public void GetAccessTokenForUserAsync()
        {
            HttpResponseMessage response = _client.SendAsync(_httpRequestMessage).GetAwaiter().GetResult();
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Failed.");
            }
        }

        private static async Task<AuthenticationResult> AcquireTokenForLabUserAsync()
        {
            var labResponse = await LabUserHelper.GetSpecificUserAsync(TestConstants.OBOUser).ConfigureAwait(false);
            var msalPublicClient = PublicClientApplicationBuilder
               .Create(TestConstants.OBOClientSideClientId)
               .WithAuthority(labResponse.Lab.Authority, TestConstants.Organizations)
               .Build();

            AuthenticationResult authResult = await msalPublicClient
                .AcquireTokenByUsernamePassword(
                TestConstants.OBOApiScope,
                TestConstants.OBOUser,
                new NetworkCredential(
                    TestConstants.OBOUser,
                    labResponse.User.GetOrFetchPassword()).SecurePassword)
                .ExecuteAsync(CancellationToken.None)
                .ConfigureAwait(false);

            return authResult;
        }
    }
}
