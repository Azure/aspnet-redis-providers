using FakeItEasy;
using Xunit;
using Microsoft.AspNetCore.Http;
using System.Net;
using System.IO;
using System;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace RedisOutputCachingMiddleware.UnitTests
{
    public class RedisOutputCachingMiddlewareUnitTests
    {
        [Fact]
        public void TestConstructorDefault()
        {
            var reqeustDelegate = A.Fake<RequestDelegate>();
            var outputCachingMiddleware = new RedisOutputCache(reqeustDelegate, "localhost");
            Assert.NotNull(outputCachingMiddleware);
        }

        [Fact]
        public void TestConstructorWithTtl()
        {
            var reqeustDelegate = A.Fake<RequestDelegate>();
            var outputCachingMiddleware = new RedisOutputCache(reqeustDelegate, "localhost", 123);
            Assert.NotNull(outputCachingMiddleware);
        }

        [Fact]
        public async Task InvokeAsyncTestAsync()
        {
            var middleware = A.Fake<RedisOutputCache>();

            HttpContext context = A.Fake<HttpContext>();
            context.Request.Method = HttpMethods.Post;
            context.Request.Path = "/path";
            context.Request.QueryString = new QueryString("?query=bar");
            context.Request.Body = new MemoryStream(Convert.ToByte(0));

            await middleware.InvokeAsync(context);

            Assert.Equal("POST", context.Request.Method);
            Assert.Equal("/path", context.Request.Path.Value);
            Assert.Equal("?query=bar", context.Request.QueryString.Value);
            Assert.NotNull(context.Request.Body);
            Assert.NotNull(context.Request.Headers);
            Assert.NotNull(context.Response.Headers);
            Assert.NotNull(context.Response.Body);
        }
    }
}
