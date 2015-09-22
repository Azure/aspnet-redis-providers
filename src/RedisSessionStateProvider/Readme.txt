Microsoft.Web.Redis.RedisSessionStateProvider Nuget package has been added to your project.

A new <sessionstate> entry has been added to your web.config. However, any existing session state entries will not have been modified. 
If you believe you had an existing sessionstate entry, you will need to manually modify the web.config to make the Redis Session State Provider as the default.

More information on Azure Redis Session State Provider can be found here - 
Blog - http://blogs.msdn.com/b/webdev/archive/2014/05/12/announcing-asp-net-session-state-provider-for-redis-preview-release.aspx
MSDN - http://msdn.microsoft.com/en-us/library/azure/dn690522.aspx

More info on Azure Redis Cache service can be found here - 
Blog - http://azure.microsoft.com/blog/2014/06/04/lap-around-azure-redis-cache-preview/
MSDN - http://msdn.microsoft.com/en-us/library/azure/dn690523.aspx

## Version 2.0.0
 * This is a breaking change. Key names (used to stored session data in redis) are modified to contains brackets to support clustering. As a result of that old session data will become invalid when you update your app. You can not use clustering without this update.
