using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Web.Redis.FunctionalTests
{
    public class RedisOutputCachingMiddleWareFunctionalTests
    {
        private string GetUnixTimeSeconds()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
        }

        [Fact()]
        private async Task TestWithoutCacheAsync()
        {
            bool isResponseCurrent = await ResponseIsCurrentAysnc();
            Assert.True(isResponseCurrent);
        }

        [Fact()]
        private async Task TestWithCacheAsync()
        {
            using (RedisServer Server = new RedisServer())
            {
                bool isResponseCurrent = await ResponseIsCurrentAysnc();
                Assert.False(isResponseCurrent);
            }
        }

        [Fact()]
        private async Task TtlTestLessAsync()
        {
            using (RedisServer Server = new RedisServer())
            {
                bool isResponseCurrent = await ResponseIsCurrentAysnc(1);
                Assert.True(isResponseCurrent);
            }
        }

        [Fact()]
        private async Task TtlTestGreaterAsync()
        {
            using (RedisServer Server = new RedisServer())
            {
                bool isResponseCurrent = await ResponseIsCurrentAysnc(7);
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
            Thread.Sleep(4000);

            var secondResponse = await host.GetTestClient().GetAsync("/");
            var secondResponseBody = await secondResponse.Content.ReadAsStringAsync();
            return secondResponseBody == GetUnixTimeSeconds();
        }
    }
}