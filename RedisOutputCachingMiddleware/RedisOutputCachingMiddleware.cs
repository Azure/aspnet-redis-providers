using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Caching.Memory;

using StackExchange.Redis;

namespace RedisOutputCachingMiddleware
{
    public class OutputCachingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IMemoryCache _memoryCache;
        private readonly string _redisConnectionString;

        public OutputCachingMiddleware(RequestDelegate next,
                                            IMemoryCache memoryCache, string redisConnectionString)
        {
            _next = next;
            _memoryCache = memoryCache;
            // store the connection string 
            _redisConnectionString = redisConnectionString;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // use the path and query as a key
            var pathAndQuery = context.Request.GetEncodedPathAndQuery();

            // true if using cache, false otherwise
            if (await TryToUseCachedData(context, pathAndQuery))
                return;

            HttpResponse response = context.Response;
            Stream originalStream = response.Body;

            try
            {
                using (var ms = new MemoryStream())
                {
                    // record the output stream
                    response.Body = ms;
                    // get the new page
                    await _next(context);
                    // convert the output to a byte array
                    byte[] bytes = ms.ToArray();
                    // cache the output
                    await CacheResponseOnSuccess(context, pathAndQuery, bytes);

                    if (ms.Length > 0)
                    {
                        ms.Seek(0, SeekOrigin.Begin);
                        await ms.CopyToAsync(originalStream);
                    }
                }
            }
            finally
            {
                // write the original stream as a response
                response.Body = originalStream;
            }
        }

        private async Task<bool> TryToUseCachedData(HttpContext context, string pathAndQuery)
        {
            using (var connection = await ConnectionMultiplexer.ConnectAsync(_redisConnectionString))
            {
                var cache = connection.GetDatabase();
                var value = await cache.StringGetAsync(pathAndQuery);
                // boolean to determine if key exists
                var found = !value.IsNullOrEmpty;

                if (found)
                {
                    await context.Response.WriteAsync(value);
                }

                return found;
            }
        }

        private async Task CacheResponseOnSuccess(HttpContext context, string pathAndQuery, byte[] bytes)
        {
            using (var connection = await ConnectionMultiplexer.ConnectAsync(_redisConnectionString))
            {
                var cache = connection.GetDatabase();
                // cache the result
                var result = await cache.StringSetAsync(pathAndQuery, bytes);
            }
        }

    }
}