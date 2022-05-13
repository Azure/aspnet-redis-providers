using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using RedisOutputCachingMiddleware;
using System;
using Microsoft.AspNetCore.Http;
using System.Threading;
using Microsoft.Web.Redis.FunctionalTests;

namespace RedisOutputCachingMiddleWare.FunctionalTests
{
    public class RedisOutputCachingMiddleWareFunctionalTests
    {
        private string GetUnixTimeSeconds()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
        }

        [Fact]
        public async void TestWithoutCacheAsync()
        {
            bool isResponseCurrent = await ResponseIsCurrent();
            Assert.True(isResponseCurrent);
        }

        [Fact]
        public async void TestWithCacheAsync()
        {
            using (RedisServer Server = new RedisServer())
            {
            bool isResponseCurrent = await ResponseIsCurrent();
            Assert.False(isResponseCurrent);
            }
        }

        [Fact]
        public async void TtlTestLessAsync()
        {
            using (RedisServer Server = new RedisServer())
            {
                bool isResponseCurrent = await ResponseIsCurrent(1);
                Assert.True(isResponseCurrent);
            }
        }

        [Fact]
        public async void TtlTestGreaterAsync()
        {
            using (RedisServer Server = new RedisServer())
            {
                bool isResponseCurrent = await ResponseIsCurrent(3);
                Assert.False(isResponseCurrent);
            }
        }

        public async Task<bool> ResponseIsCurrent(int ttl = int.MaxValue)
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
            var firstResponseBody = firstResponse.Content.ReadAsStringAsync().Result;
            Assert.Equal(firstResponseBody, GetUnixTimeSeconds());

            Thread.Sleep(2000);

            var secondResponse = await host.GetTestClient().GetAsync("/");
            var secondResponseBody = secondResponse.Content.ReadAsStringAsync().Result;
            return secondResponseBody == GetUnixTimeSeconds();
        }

    }
}