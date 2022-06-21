# Redis Output Cache Middleware

## Overview 
The output cache middleware uses Redis to cache output responses for each unique set of request inputs observed.

### Middleware 
ASP.NET Core introduced the concept of [middleware](https://docs.microsoft.com/aspnet/core/fundamentals/middleware) to handle HTTP requests and responses. Middleware components can evaluate logical conditions and can perform work on the input data. 

### Output Cache 
An output cache operates at the application layer instead of caching HTTP responses. For example, a user might wish to cache a view to avoid performing redundant work when a user executes a duplicate controller action. ASP.NET Core does not currently provide support for an output cache and instead relies on [response caching](https://docs.microsoft.com/aspnet/core/performance/caching/response).

Building Middleware to cache the response exists as one method to achieve the desired functionality.  

## Usage 
In the `Startup.cs` file of a ASP.NET Core application, one can add custom middleware as shown below:

```csharp
public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    string redisConnectionString = "localhost";
    app.UseMiddleware<RedisOutputCache>(redisConnectionString);
}
```

The default TTL (Time to Live) for a cache entry is 24 hours. One can also set an expiration for the cache entires by providing an optional TTL parameter (in seconds):

```csharp
app.UseMiddleware<RedisOutputCache>("localhost", 300); // TTL = 300 seconds
```

### Cache Key
The default key for the output cache middleware is a concatenation of the request URL, headers, and body. Developers are encouraged to clone the repository and tailor the caching functionality for their own purposes. 

### Upcoming ASP.NET 7.0 Support 
Microsoft has recognized the demand for output caching. ASP.NET 7.0 will have native support for output caching, so this middleware should no longer be necessary in ASP.NET applications targeting 7.0+.