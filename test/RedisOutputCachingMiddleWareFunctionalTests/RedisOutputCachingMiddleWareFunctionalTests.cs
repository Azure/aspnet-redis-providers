using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Web.Redis.FunctionalTests;
using RedisOutputCachingMiddleware;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace RedisOutputCachingMiddleWare.FunctionalTests
{
    public class RedisOutputCachingMiddleWareFunctionalTests
    {
        private string GetUnixTimeSeconds()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
        }

        [Fact(Skip = "Functional Tests not yet enabled")]
        private async Task TestWithoutCacheAsync()
        {
            bool isResponseCurrent = await ResponseIsCurrentAysnc();
            Assert.True(isResponseCurrent);
        }

        [Fact(Skip = "Functional Tests not yet enabled")]
        private async Task TestWithCacheAsync()
        {
            using (RedisServer Server = new RedisServer())
            {
                bool isResponseCurrent = await ResponseIsCurrentAysnc();
                Assert.False(isResponseCurrent);
            }
        }

        [Fact(Skip = "Functional Tests not yet enabled")]
        private async Task TtlTestLessAsync()
        {
            using (RedisServer Server = new RedisServer())
            {
                bool isResponseCurrent = await ResponseIsCurrentAysnc(1);
                Assert.True(isResponseCurrent);
            }
        }

        [Fact(Skip = "Functional Tests not yet enabled")]
        private async Task TtlTestGreaterAsync()
        {
            using (RedisServer Server = new RedisServer())
            {
                bool isResponseCurrent = await ResponseIsCurrentAysnc(3);
                Assert.False(isResponseCurrent);
            }
        }

        private async Task<bool> ResponseIsCurrentAysnc(int ttl = int.MaxValue)
        {
            using var host = await new HostBuilder()
            .ConfigureWebHost(webBuilder =>
            {
                webBuilder
                    .UseTestServer()
                    .ConfigureServices(services =>
                    {
                        services.AddControllersWithViews();
                    })
                    .Configure(app =>
                    {
                        app.UseMiddleware<RedisOutputCache>("localhost", ttl);
                        app.Run(async context =>
                        {
                            var time = GetUnixTimeSeconds();
                            await context.Response.WriteAsync(time);
                        });
                    });
            })
            .StartAsync();

            var firstResponse = await host.GetTestClient().GetAsync("/");
            var firstResponseBody = await firstResponse.Content.ReadAsStringAsync();
            Assert.Equal(firstResponseBody, GetUnixTimeSeconds());

            // sleep is needed to ensure second read will retrieve a cached value
            Thread.Sleep(2000);

            var secondResponse = await host.GetTestClient().GetAsync("/");
            var secondResponseBody = await secondResponse.Content.ReadAsStringAsync();
            return secondResponseBody == GetUnixTimeSeconds();
        }

    }
}