using FakeItEasy;
using Xunit;
using Microsoft.AspNetCore.Http;

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
    }
}
