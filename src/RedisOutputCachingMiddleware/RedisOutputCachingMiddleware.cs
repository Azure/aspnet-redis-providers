using System;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.VisualBasic;
using Microsoft.Web.Redis;
using StackExchange.Redis;

namespace RedisOutputCachingMiddleware
{
    public class OutputCachingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IDatabase _cache;
        private int _ttl;

        // optional expiration time, default to 1 day if not defined 
        public OutputCachingMiddleware(RequestDelegate next, string redisConnectionString, int ttl = TimeSpan.FromDays(1).TotalSeconds)
        {
            _next = next;
            _cache = ConnectAsync(redisConnectionString).Result;
            _ttl = ttl;
        }

        public int GetTtl()
        {
            return _ttl;
        }

        public void SetTtl(int ttl)
        {
            _ttl = ttl;
        }

        public static async Task<IDatabase> ConnectAsync(string redisConnectionString)
        {
            try
            {
                var connection = await ConnectionMultiplexer.ConnectAsync(redisConnectionString);
                return connection.GetDatabase();
            }
            catch (Exception ex)
            {
                LogUtility.LogError("Cannot connect to Redis at " + redisConnectionString + " : " + ex.Message);
                return null;
            }

        }

        public async Task InvokeAsync(HttpContext context)
        {
            // use the url, header, and request body as a key 
            string pathAndQuery = context.Request.GetEncodedPathAndQuery();
            string headers = context.Request.Headers.ToString();
            string body = context.Request.Body.ToString();
            RedisKey key = pathAndQuery + headers + body;
            RedisValue value = await GetCache(key);

            if (!value.IsNullOrEmpty)
            {
                await context.Response.WriteAsync(value);
                return;
            }

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
                    RedisValue redisValue = bytes;
                    await SetCache(key, redisValue);

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

        private async Task<RedisValue> GetCache(RedisKey key)
        {
            try
            {
                return await _cache.StringGetAsync(key);
            }
            catch (Exception ex)
            {
                LogUtility.LogError("Error in Get: " + ex.Message);
                return RedisValue.Null;
            }

        }

        private async Task SetCache(RedisKey key, RedisValue value)
        {
            try
            {
                await _cache.StringSetAsync(key, value, TimeSpan.FromSeconds(_ttl));
            }
            catch (Exception ex)
            {
                LogUtility.LogError("Error in Set: " + ex.Message);
            }
        }

    }
}