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
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace RedisOutputCachingMiddleWare.FunctionalTests
{
    public class RedisOutputCachingMiddleWareFunctionalTests
    {
        private string GetUnixTimeSeconds()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
        }

        [Fact]
        public void TestWithoutCache()
        {
            Assert.True(ResponseIsCurrent().Result);
        }

        [Fact]
        public void TestWithCache()
        {
            using (RedisServer Server = new RedisServer())
            {
                Assert.False(ResponseIsCurrent().Result);
            }
        }

        [Fact]
        public void TtlTestLess()
        {
            using (RedisServer Server = new RedisServer())
            {
                Assert.True(ResponseIsCurrent(1).Result);
            }
        }

        [Fact]
        public void TtlTestGreater()
        {
            using (RedisServer Server = new RedisServer())
            {
                Assert.False(ResponseIsCurrent(3).Result);
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
                        app.UseMiddleware<OutputCachingMiddleware>("localhost", ttl);
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