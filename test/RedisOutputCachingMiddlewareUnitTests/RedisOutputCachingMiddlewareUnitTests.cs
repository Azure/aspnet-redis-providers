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
        }

        [Fact]
        public void TestConstructorWithTtl()
        {
            var reqeustDelegate = A.Fake<RequestDelegate>();
            var outputCachingMiddleware = new OutputCachingMiddleware(reqeustDelegate, "localhost", 123);
        }
    }
}
