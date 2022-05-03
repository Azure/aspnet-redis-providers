﻿using System;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.VisualBasic;
using StackExchange.Redis;

namespace RedisOutputCachingMiddleware
{
    public class OutputCachingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IDatabase _cache;

        public OutputCachingMiddleware(RequestDelegate next, string redisConnectionString)
        {
            _next = next;
            _cache = ConnectAsync(redisConnectionString).Result;
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
                return null;
            }

        }

        public async Task InvokeAsync(HttpContext context)
        {
            // use the path and query as a key
            // TODO url, header, request body
            RedisKey key = context.Request.GetEncodedPathAndQuery() + context.Request.Headers + context.Request.Body;
            RedisValue value = await GetCache(key);

            if (!value.IsNullOrEmpty)
            {
                await context.Response.WriteAsync(value);
                return;
            }

            HttpResponse response = context.Response;
            // TODO check if response created
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
                return RedisValue.Null;
            }

        }

        private async Task SetCache(RedisKey key, RedisValue value)
        {
            try
            {
                await _cache.StringSetAsync(key, value);
            }
            catch (Exception ex)
            {

            }
        }

    }
}