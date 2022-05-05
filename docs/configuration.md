## How to use configuration parameters of Session State Provider and Output Cache Provider

There are certain settings that are specific to session state provider (like applicationName, throwOnError, retryTimeoutInMilliseconds, databaseId, settingsClassName, settingsMethodName, loggingClassName, loggingMethodName). 

“connectionString” must be a valid StackExchange.Redis connection string. No matter how you are passing connection string (either by providing in web.config or by returning from settingsMethodName), You can’t provide session state specific parameters through connectionString. 

Only way to provide parameters (applicationName, throwOnError, retryTimeoutInMilliseconds, databaseId, settingsClassName, settingsMethodName, loggingClassName, loggingMethodName) is either through web.config or through AppSettings. 

All parameters values will be used as key to find actual value from appSettings. If it is not found inside appSettings than literal value provided inside appSettings will be used as it is. Add actual value as appSettings like below.

    <appSettings>
        <add key="SomeHostKey" value="actual host value" />
        <add key="SomeAccessKey" value="actual access key" />
    </appSettings>

In web.config use above key as parameter value instead of actual value. For example, 
    
    <sessionState mode="Custom" customProvider="MySessionStateStore">
        <providers>
            <add type = "Microsoft.Web.Redis.RedisSessionStateProvider"
                 name = "MySessionStateStore" 
                 host = "SomeHostKey"
                 accessKey = "SomeAccessKey"
                 ssl = "true"/>
        </providers>
    </sessionState>`

## Redis Provider Configuration Settings

#### host [String]
The IP address or host name of your Redis server. By default it’s localhost. 
 
#### port [number]
The port of your Redis server. By default it’s 6379 for non-ssl and 6380 for ssl (if you are using Azure Redis Cache).  

#### accessKey [String]
The password of your Redis server when Redis authorization is enabled. By default is empty, which means the session state provider won’t use any password when connecting to Redis server. If your Redis server is in a publicly accessible network, like Azure Redis Cache, be sure to enable Redis authorization to improve security. 

#### ssl [true|false]
Whether to connect to Redis server via ssl or not. By default is false because Redis doesn’t support SSL out of the box. If you are using Azure Redis Cache which supports SSL out of the box,  be sure to set this to true to improve security. 

#### databaseId [number]
Only way to provide this parameter is either through web.config or through AppSettings. 
Specify which database to use from Redis. Default is 0.

#### connectionTimeoutInMilliseconds [number]
This value will be used to set 'ConnectTimeout' when creating StackExchange.Redis.ConnectionMultiplexer. Default is whatever provided by StackExchange.Redis.

#### operationTimeoutInMilliseconds [number]
This value will be used to set 'SyncTimeout' when creating StackExchange.Redis.ConnectionMultiplexer. Default is whatever provided by StackExchange.Redis.

#### connectionString (Valid StackExchange.Redis connection string) [String]
'connectionString' must be a valid StackExchange.Redis connection string which means it can provide values for parameters like 'host', 'port', 'accessKey', 'ssl' and other valid StackExchange.Redis parameters. 

'connectionString' literal value will be used as key to fetch actual string from AppSettings if it exists. If not found inside AppSettings than literal value will be used as key to fetch actual string from web.config ConnectionString section if it exists. If it does not exists in AppSettings or web.config ConnectionString section than literal value will be used as it is as a "ConnectionString" when creating StackExchange.Redis.ConnectionMultiplexer.

Example 1:
    
    <connectionStrings>
        <add name="MyRedisConnectionString" connectionString="mycache.redis.cache.windows.net:6380,password=actual access key,ssl=True,abortConnect=False" />
    </connectionStrings>

In web.config use above key as parameter value instead of actual value.  
    
    <sessionState mode="Custom" customProvider="MySessionStateStore">
        <providers>
            <add type = "Microsoft.Web.Redis.RedisSessionStateProvider"
                 name = "MySessionStateStore" 
                 connectionString = "MyRedisConnectionString"/>
        </providers>
    </sessionState>

Example 2:
    
    <appSettings>
        <add key="MyRedisConnectionString" value="mycache.redis.cache.windows.net:6380,password=actual access key,ssl=True,abortConnect=False" />
    </appSettings>

In web.config use above key as parameter value instead of actual value.  

    <sessionState mode="Custom" customProvider="MySessionStateStore">
        <providers>
            <add type = "Microsoft.Web.Redis.RedisSessionStateProvider"
                 name = "MySessionStateStore" 
                 connectionString = "MyRedisConnectionString"/>
        </providers>
    </sessionState>

Example 3:
    
    <sessionState mode="Custom" customProvider="MySessionStateStore">
        <providers>
            <add type = "Microsoft.Web.Redis.RedisSessionStateProvider"
                 name = "MySessionStateStore" 
                 connectionString = "mycache.redis.cache.windows.net:6380,password=actual access key,ssl=True,abortConnect=False"/>
        </providers>
    </sessionState>


#### settingsClassName [String]
#### settingsMethodName [String]
Only way to provide this parameter is either through web.config or through AppSettings. 

Using 'settingsClassName' and 'settingsMethodName' to provide connection string: 'settingsClassName' should be assembly qualified class name that contains method specified by  'settingsMethodName'. 'settingsMethodName' should be public, static, should not take any parameters and should have a return type of 'String', which is basically actual connection string value. You can get connection string from anywhere in this method.

#### loggingClassName [String]
#### loggingMethodName [String]
Only way to provide this parameter is either through web.config or through AppSettings. 

This will allow you to debug your application by providing logs from Session State/Output Cache along with logs from StackExchange.Redis. 'loggingClassName' should be assembly qualified class name that contains method specified by 'loggingMethodName'. 'loggingMethodName' should be public, static, should not take any parameters and should have a return type of 'System.IO.TextWriter'.

#### applicationName (This parameter is only available for Session State Provider) [String] 
Only way to provide this parameter is either through web.config or through AppSettings. 
It is possible that customer is using same Redis Cache for difference purpose. To make sure that session key do not collied with other we try to prifix it with application name. Default is ModuleName of current process or "/".

#### throwOnError (This parameter is only available for Session State Provider) [true|false] 
Only way to provide this parameter is either through web.config or through AppSettings. 

Whether or not to throw an exception when some error occurs. The default is true. 

When we talk to developers about the current available ASP.NET session state providers, one of the top complaints is that with the current available session state providers, if an error occurs during a session operation, the session state provider will throw an exception, which will blow up the entire application. 

We want to address this in a way that it won’t surprise existing ASP.NET session state provider users and at the same time, provide the ability to opt-in the advanced behaviours. As you can see, the default behaviour will still throw an exception when some error occurs. This is consistent with the other ASP.NET session state providers we provide so there won’t be any surprise and your existing code will just work.

If you set throwOnError to false, then instead of throwing an exception when some error occurs, it will fail silently. If you need to check if there was some error and if there was one, what the exception was, you can check it using static property "Microsoft.Web.Redis.RedisSessionStateProvider.LastException"

#### retryTimeoutInMilliseconds (This parameter is only available for Session State Provider) [number] 
Only way to provide this parameter is either through web.config or through AppSettings. 

How long it will retry when an operation fails. Default is 5000.
retrytimeoutInMilliseconds should be higher that operationTimeoutinMilliseonds, otherwise the provider won't retry.

We also want to provide some retry logic to simplify the case where some session operation should retry on failure because of things like network glitch. At the same time, we also heard from developers that they want the ability to control the retry timeout or opt-out of retry entirely because they know retry won’t solve the issue in their cases. 

If you set retryTimeoutInMilliseconds to a number, say 5000, then when a session operation fails, it will retry for 5000 milliseconds before treating it as an error. So if you would like to have the session state provider to apply this retry logic for you, you can simply configure the timeout. The first retry will happen after 20 milliseconds since that is good enough in most cases when a network glitch happens. After that, it will retry every 1 second till it times out. Right after the time out, it will retry one more time to make sure that it won’t cut off the timeout by (at most) 1 second.

If you don’t think you need retry (like when you are running the Redis server on the same machine as your application) or if you want to handle the retry logic yourself, you can just make it 0.

#### redisSerializerType [String] 
By default, the serialization to store the values on Redis, is done in a binary format provided by the [BinaryFormatter](https://msdn.microsoft.com/en-us/library/system.runtime.serialization.formatters.binary.binaryformatter(v=vs.110).aspx) class.

But if you need a different serialization mechanism, you can use the 'redisSerializerType' parameter to specify the [assembly qualified type name](https://msdn.microsoft.com/en-us/library/system.type.assemblyqualifiedname(v=vs.110).aspx#Anchor_1) of a class that implements Microsoft.Web.Redis.ISerializer and has the custom logic to serialize/deserialize the values.

For example, a Json serializer using [JSON.NET](http://www.newtonsoft.com/json):

	namespace MyCompany.Redis
	{
		public class JsonSerializer : ISerializer
		{
			private static JsonSerializerSettings _settings = new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.All };

			public byte[] Serialize(object data)
			{
				return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data, _settings));
			}

			public object Deserialize(byte[] data)
			{
				if (data == null)
				{
					return null;
				}
				return JsonConvert.DeserializeObject(Encoding.UTF8.GetString(data), _settings);
			}
		}
	}

Assuming this class is defined in an assembly with name "MyCompanyDll", you can set the parameter 'redisSerializerType':

    <sessionState mode="Custom" customProvider="MySessionStateStore">
        <providers>
            <add type = "Microsoft.Web.Redis.RedisSessionStateProvider"
                 name = "MySessionStateStore"
                 redisSerializerType = "MyCompany.Redis.JsonSerializer,MyCompanyDll"
                 ... />
        </providers>
    </sessionState>


## Related ASP.NET Settings


#### system.web/httpRuntime/executionTimeout 
This value is used as request timeout and we use it to put expiry time on lock taken by any request. Check [this](https://msdn.microsoft.com/en-us/library/ms178587.aspx#Anchor_1) to understand what locking means. Check [this](https://msdn.microsoft.com/en-us/library/e1f13641(v=vs.100).aspx) to understand how to set executionTimeout and what is default value.

#### system.web/sessionState/timeout
This value is used as session timeout and we use it to put expiry time on session data inside redis. Check [this](https://msdn.microsoft.com/en-us/library/h6bb9cz9(v=vs.100).aspx) to understand how to set timeout and what is default value.


## lock-free session state
`Note:` Lock-free session state provider is only supported by Microsoft.Web.RedisSessionStateProvider NuGet version v3.0.0-Preview or higher. 

It supports lock-free session state provide with .net 4.6.2 or higher. To use session state in lock-free mode please include the following setting in your web.config. If you want to continue to use session state with locks, then no web.config changes are needed.
 
    <appSettings>
        <add key="aspnet:AllowConcurrentRequestsPerSession" value="true"/>
    </appSettings>

## Using Session State with ASP.NET Core

Session works very differently in .NET Core as compared to the standard version of .NET. For more information look at the following links.

[Configure session state](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/app-state?view=aspnetcore-2.1#configure-session-state)

[Using a Redis distributed cache](https://docs.microsoft.com/en-us/aspnet/core/performance/caching/distributed?view=aspnetcore-2.1#using-a-redis-distributed-cache)
