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
            var outputCachingMiddleware = new OutputCachingMiddleware(reqeustDelegate, "localhost");
            var ttl = outputCachingMiddleware.GetTtl();
            Assert.Equal(86400, ttl);
        }

        [Fact]
        public void TestConstructorWithTtl()
        {
            var reqeustDelegate = A.Fake<RequestDelegate>();
            var outputCachingMiddleware = new OutputCachingMiddleware(reqeustDelegate, "localhost", 123);
            var ttl = outputCachingMiddleware.GetTtl();
            Assert.Equal(123, ttl);
        }

        [Fact]
        public void TestSetTtl()
        {
            var reqeustDelegate = A.Fake<RequestDelegate>();
            var outputCachingMiddleware = new OutputCachingMiddleware(reqeustDelegate, "localhost");
            outputCachingMiddleware.SetTtl(123);
            var ttl = outputCachingMiddleware.GetTtl();
            Assert.Equal(123, ttl);
        }
    }
}
