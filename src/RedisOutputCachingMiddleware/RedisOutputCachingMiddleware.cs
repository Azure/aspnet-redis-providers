using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Web.Redis;
using StackExchange.Redis;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace RedisOutputCachingMiddleware
{
    public class RedisOutputCache
    {
        private readonly RequestDelegate _next;
        private readonly IDatabase _cache;
        // optional expiration time, default to 1 day if not defined 
        private int _ttl = Convert.ToInt32(TimeSpan.FromDays(1).TotalSeconds);
        
        public RedisOutputCache(RequestDelegate next, string redisConnectionString, [Optional] int ttl)
        {
            _next = next;
            try
            {
                _cache = ConnectionMultiplexer.Connect(redisConnectionString).GetDatabase();
            }
            catch (Exception ex)
            {
                LogUtility.LogError($"Cannot connect to Redis: {ex.Message}");
            }
            
            _ttl = ttl;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // use the url, header, and request body as a key 
            RedisKey key = $"{context.Request.GetEncodedPathAndQuery()}{context.Request.Headers}{context.Request.Body}";
            RedisValue value = await GetCacheAsync(key);

            if (!value.IsNullOrEmpty)
            {
                await context.Response.WriteAsync(value);
                return;
            }

            HttpResponse response = context.Response;
            Stream responseStream = response.Body;

            try
            {
                using (var ms = new MemoryStream())
                {
                    // record the output stream
                    response.Body = ms;
                    // invokes the next middleware (or action)
                    await _next(context);
                    // convert the output to a byte array
                    byte[] bytes = ms.ToArray();
                    // cache the output
                    RedisValue redisValue = bytes;
                    await SetCacheAsync(key, redisValue);

                    if (ms.Length > 0)
                    {
                        ms.Seek(0, SeekOrigin.Begin);
                        await ms.CopyToAsync(responseStream);
                    }
                }
            }
            finally
            {
                // write the original stream as a response
                response.Body = responseStream;
            }
        }

        private async Task<RedisValue> GetCacheAsync(RedisKey key)
        {
            try
            {
                return await _cache?.StringGetAsync(key);
            }
            catch (Exception ex)
            {
                LogUtility.LogError($"Failed to retrieve cached value: {ex.Message}");
                return RedisValue.Null;
            }

        }

        private async Task SetCacheAsync(RedisKey key, RedisValue value)
        {
            try
            {
                await _cache?.StringSetAsync(key, value, TimeSpan.FromSeconds(_ttl));
            }
            catch (Exception ex)
            {
                LogUtility.LogError($"Failed to set cache value: {ex.Message}");
            }
        }

    }
}