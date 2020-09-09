// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Identity.Client;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.Test.Common;
using Microsoft.Identity.Web.Test.LabInfrastructure;

namespace IntegrationTestService
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        private KeyVaultSecretsProvider _keyVault;

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            _keyVault = new KeyVaultSecretsProvider();
            string ccaSecret = _keyVault.GetSecret(TestConstants.OBOClientKeyVaultUri).Value;

            var builder = services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                                  .AddMicrosoftIdentityWebApi(Configuration, subscribeToJwtBearerMiddlewareDiagnosticsEvents: true)
                                  .EnableTokenAcquisitionToCallDownstreamApi()
                                    .AddInMemoryTokenCaches()
                                        .AddDownstreamWebApi(
                                            TestConstants.SectionNameCalledApi,
                                            Configuration.GetSection(TestConstants.SectionNameCalledApi))
                                        .AddMicrosoftGraph(Configuration.GetSection("GraphBeta"));

            services.Configure<MicrosoftIdentityOptions>(options =>
            {
                options.ClientSecret = ccaSecret;
            });
            services.Configure<ConfidentialClientApplicationOptions>(options =>
            {
                options.ClientSecret = ccaSecret;
            });
            services.AddRazorPages(options =>
            {
                options.Conventions.AuthorizePage("/SecurePage");
            });
            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
