# Release Notes

Below is a summary of the the main features/bug fixes in the most recent releases.

**Note:** For .NET Core, please refer details found [here](configuration.md#using-session-state-with-aspnet-core).

## Session State Provider Release Notes

### v5.0.1

Removed try-catch around serialization so it now throws an error. Updated the assembly verions to match the release versions. Downgraded Microsoft.Bcl.AsyncInterfaces to version 5.0.0

### v5.0.0
This release provides an update for the RedisSessionStateProvider nuget package. As a BREAKING CHANGE, the underlying serialization method has changed. SessionStateItemCollection objects are now treated as an atomic unit. The ability to add custom serialization has been removed. These changes were made for necessary security improvements.

**Note:** v4.0+ requires .NET Framework 4.6.2 or higher. v3.0+ requires .NET Framework 4.5.2 or higher. If you are using .net 4.0, 4.5.0 or 4.5.1, then please use an older version of Session State Provider (i.e. 2.x).

### v4.0.1
Updates .NET target framework to 4.6.2, StackExchange.Redis to v2.0.519.

### v3.0.2
[Provider throws InvalidOperationException at random timing](https://github.com/Azure/aspnet-redis-providers/issues/80)

### v3.0.0-Preview
This package is in preview as version 3.0.0 is created on top of the new ASP.Net _async_ session state module, and so it has some major changes. 

This package supports lock-free session state provide with .net 4.6.2 or higher. To use session state in lock-free mode please include the following setting in your `web.config`. If you want to continue to use session state with locks, then no `web.config` changes are needed.

```xml
    <appSettings>
        <add key="aspnet:AllowConcurrentRequestsPerSession" value="true"/>
    </appSettings>
```

### v2.2.6
Recreate stack exchange redis connection multiplexer, if it goes into a bad state. Also, this is the last release that supports .NET 4.0. 

### v2.2.5
https://github.com/Azure/aspnet-redis-providers/issues/69

### v2.2.4
Lazy deserialize of session data: Deserialize session data only when accessed by application and not when fetched from Redis. As well as, Updated [StackExchange.Redis](https://github.com/StackExchange/StackExchange.Redis).   
Issues fixed: https://github.com/Azure/aspnet-redis-providers/issues/46  

### v2.2.3
Updated [StackExchange.Redis](https://github.com/StackExchange/StackExchange.Redis) as part of one of the pull request merged.   
Merged following pull request:   
https://github.com/Azure/aspnet-redis-providers/pull/43   
https://github.com/Azure/aspnet-redis-providers/pull/47   
https://github.com/Azure/aspnet-redis-providers/pull/53   

### v2.2.2
This contains custom/extensible serialization/deserialization mechanism  
Issues fixed: https://github.com/Azure/aspnet-redis-providers/issues/29  
Merged following pull request: https://github.com/Azure/aspnet-redis-providers/pull/38  

### v2.2.1
Issues fixed.  
https://github.com/Azure/aspnet-redis-providers/issues/2  
https://github.com/Azure/aspnet-redis-providers/issues/10  

Merged following pull request.  
https://github.com/Azure/aspnet-redis-providers/pull/26  
https://github.com/Azure/aspnet-redis-providers/pull/3  

### v2.2.0
Redis ConnectionString `connectionString` can be provided in the following ways:
You can use only one of the following: `connectionString` or `settingsClassName` to provide connection string.

#### Using `connectionString` parameter to provide connection string. 
1. `connectionString` literal value will map to an AppSetting Key. The value of the AppSetting Key will be the connectionstring.
2. If AppSetting is not found, `connectionString` literal value will map to the name of the ConnectionString in the connectionstring section. 
3. If the ConnectionString is not found in the connectionStrings section, then the literal value of `connectionString` will be used as it is.

#### Using `settingsClassName` and `settingsMethodName` to provide connection string
`settingsClassName` should be a fully qualified class name that contains method specified by `settingsMethodName`. `settingsMethodName` should be public, static, should not take any parameters and should have a return type of `String`, which is basically actual connection string value. You can get connection string from anywhere in this method.


### v2.1.0
Minor bug fixes. Updated `StackExchange.Redis.StrongName` to version 1.0.488 from 1.0.481. 

### v2.0.1-Preview
Updated `StackExchange.Redis.StrongName` to version 1.0.481 from 1.0.394. Due to clustering related bugs in version 1.0.394.

### v2.0.0-Preview
`2.*` versions of this package contain a **breaking change** from `1.*` versions in the format of key names used to store session data.  In order to support Redis Clusters, key names now include brackets. As a result of this change, existing session data will not be recognized by this session state provider. 
[Details and migration information](v2.0.0_breaking_change.md)

## Output Cache Provider Release Notes

### v4.0.1

Removed try-catch around serialization so it now throws an error. Updated the assembly verions to match the release versions. Downgraded Microsoft.Bcl.AsyncInterfaces to version 5.0.0

### v4.0.0
This release provides an update for the RedisSessionStateProvider nuget package. As a BREAKING CHANGE, the underlying serialization method has changed. The ability to add custom serialization has been removed. These changes were made for necessary security improvements.

**Note:** v4.0+ requires .NET Framework 4.6.2 or higher. v3.0+ requires .NET Framework 4.5.2 or higher. If you are using .net 4.0, 4.5.0 or 4.5.1, then please use an older version of Session State Provider (i.e. 2.x).

### v3.0.1
Updates .NET target framework to 4.6.2, [StackExchange.Redis](https://github.com/StackExchange/StackExchange.Redis) to v2.0.519. This version also uses the ASP.NET Async OutputCache Module.

### v2.0.1-Preview
This package is in preview as version 2.0.1 is created on top of the new ASP.Net _async_ output cache module, and so it has some major changes. 

### v1.7.6
Recreate stack exchange redis connection multiplexer, if it goes into bad state. Also, this is the last release that supports .NET 4.0. 

### v1.7.5
https://github.com/Azure/aspnet-redis-providers/issues/69

### v1.7.4
Updated [StackExchange.Redis](https://github.com/StackExchange/StackExchange.Redis).   

### v1.7.3
Updated StackExchange.Redis as part of one of the pull request merged.   
Merged following pull request:   
https://github.com/Azure/aspnet-redis-providers/pull/43   
https://github.com/Azure/aspnet-redis-providers/pull/47    
https://github.com/Azure/aspnet-redis-providers/pull/53   

### v1.7.2
This contains custom/extensible serialization/deserialization mechanism  
Issues fixed: https://github.com/Azure/aspnet-redis-providers/issues/29  
Merged following pull request: https://github.com/Azure/aspnet-redis-providers/pull/38  

### v1.7.1
Issues fixed.  
https://github.com/Azure/aspnet-redis-providers/issues/39  

Merged following pull request.  
https://github.com/Azure/aspnet-redis-providers/pull/31  

### v1.6.6
Updated `StackExchange.Redis.StrongName` to version 1.0.488 from 1.0.481. 

### v1.6.5
Updated `StackExchange.Redis.StrongName` to version 1.0.481 from 1.0.394. Due to clustering related bugs in version 1.0.394.

### v1.7.0
Redis ConnectionString `connectionString` can be provided in the following ways:
You can use only one of the following: `connectionString` OR `settingsClassName` to provide connection string.

#### Using `connectionString` parameter to provide connection string. 
1. `connectionString` literal value will map to an AppSetting Key. The value of the AppSetting Key will be the connectionstring.
2. If AppSetting is not found, `connectionString` literal value will map to the name of the ConnectionString in the connectionstring section. 
3. If the ConnectionString is not found in the connectionStrings section, then the literal value of `connectionString` will be used as it is.

#### Using `settingsClassName` and `settingsMethodName` to provide connection string
`settingsClassName` should be a fully qualified class name that contains method specified by `settingsMethodName`. `settingsMethodName` should be public, static, should not take any parameters and should have a return type of `String`, which is basically actual connection string value. You can get connection string from anywhere in this method.
