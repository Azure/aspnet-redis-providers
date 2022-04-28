using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Caching.Memory;

namespace RedisOutputCachingMiddleware
{
    public class OutputCachingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IMemoryCache _memoryCache;

        public OutputCachingMiddleware(RequestDelegate next,
                                            IMemoryCache memoryCache)
        {
            _next = next;
            _memoryCache = memoryCache;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (CanCacheRedisRequest(context))
                await HandleRedisRequest(context);
            else
                await _next(context);
        }

        private static bool CanCacheRedisRequest(HttpContext context)
        {
            return true;
        }

        private async Task HandleRedisRequest(HttpContext context)
        {
            await context.Response.WriteAsync("Hello World!");
        }
    }
}