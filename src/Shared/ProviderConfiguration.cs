//
// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.
//

using System;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Web.Configuration;
using System.Web.Hosting;

namespace Microsoft.Web.Redis
{
    internal class ProviderConfiguration
    {
        public TimeSpan RequestTimeout { get; set; }
        public TimeSpan SessionTimeout { get; set; }
        public int Port { get; set; }
        public string Host { get; set; }
        public string AccessKey { get; set; }
        public TimeSpan RetryTimeout { get; set; }
        public bool ThrowOnError { get; set; }
        public bool UseSsl { get; set; }
        public int DatabaseId { get; set; }
        public string ApplicationName { get; set; }
        public int ConnectionTimeoutInMilliSec { get; set; }
        public int OperationTimeoutInMilliSec { get; set; }
        public string ConnectionString { get; set; }
        public string RedisSerializerType { get; set; }

        /* Empty constructor required for testing */
        internal ProviderConfiguration()
        {}

        internal static ProviderConfiguration ProviderConfigurationForSessionState(NameValueCollection config)
        {
            ProviderConfiguration configuration = new ProviderConfiguration(config);
            
            configuration.ThrowOnError = GetBoolSettings(config, "throwOnError", true);
            int retryTimeoutInMilliSec = GetIntSettings(config, "retryTimeoutInMilliseconds", 5000);
            configuration.RetryTimeout = new TimeSpan(0, 0, 0, 0, retryTimeoutInMilliSec);
            
            // Get request timeout from config
            HttpRuntimeSection httpRuntimeSection = ConfigurationManager.GetSection("system.web/httpRuntime") as HttpRuntimeSection;
            configuration.RequestTimeout = httpRuntimeSection.ExecutionTimeout;

            // Get session timeout from config
            SessionStateSection sessionStateSection = (SessionStateSection)WebConfigurationManager.GetSection("system.web/sessionState");
            configuration.SessionTimeout = sessionStateSection.Timeout;

            LogUtility.LogInfo("Host: {0}, Port: {1}, ThrowOnError: {2}, UseSsl: {3}, RetryTimeout: {4}, DatabaseId: {5}, ApplicationName: {6}, RequestTimeout: {7}, SessionTimeout: {8}",
                                            configuration.Host, configuration.Port, configuration.ThrowOnError, configuration.UseSsl, configuration.RetryTimeout, configuration.DatabaseId, configuration.ApplicationName, configuration.RequestTimeout, configuration.SessionTimeout);
            return configuration;
        }

        internal static ProviderConfiguration ProviderConfigurationForOutputCache(NameValueCollection config)
        {
            ProviderConfiguration configuration = new ProviderConfiguration(config);
            
            // No retry login for output cache provider
            configuration.RetryTimeout = TimeSpan.Zero;
            
            // Session state specific attribute which are not applicable to output cache
            configuration.ThrowOnError = true;
            configuration.RequestTimeout = TimeSpan.Zero;
            configuration.SessionTimeout = TimeSpan.Zero;

            LogUtility.LogInfo("Host: {0}, Port: {1}, UseSsl: {2}, DatabaseId: {3}, ApplicationName: {4}",
                                            configuration.Host, configuration.Port, configuration.UseSsl, configuration.DatabaseId, configuration.ApplicationName);
            return configuration;
        }

        private ProviderConfiguration(NameValueCollection config)
        {
            EnableLoggingIfParametersAvailable(config);
            // Get connection host, port and password.
            // host, port, accessKey and ssl are firest fetched from appSettings if not found there than taken from web.config
            ConnectionString = GetConnectionString(config);
            Host = GetStringSettings(config, "host", "127.0.0.1");
            Port = GetIntSettings(config, "port", 0);
            AccessKey = GetStringSettings(config, "accessKey", null);
            UseSsl = GetBoolSettings(config, "ssl", true);
            RedisSerializerType = GetStringSettings(config, "redisSerializerType", null);
            // All below parameters are only fetched from web.config
            DatabaseId = GetIntSettings(config, "databaseId", 0);
            ApplicationName = GetStringSettings(config, "applicationName", null);
            if (ApplicationName == null)
            {
                try
                {
                    ApplicationName = HostingEnvironment.ApplicationVirtualPath;
                    if (String.IsNullOrEmpty(ApplicationName))
                    {
                        ApplicationName = System.Diagnostics.Process.GetCurrentProcess().MainModule.ModuleName;

                        int indexOfDot = ApplicationName.IndexOf('.');
                        if (indexOfDot != -1)
                        {
                            ApplicationName = ApplicationName.Remove(indexOfDot);
                        }
                    }

                    if (String.IsNullOrEmpty(ApplicationName))
                    {
                        ApplicationName = "/";
                    }

                }
                catch (Exception e)
                {
                    ApplicationName = "/";
                    LogUtility.LogInfo(e.Message);
                }
            }

            ConnectionTimeoutInMilliSec = GetIntSettings(config, "connectionTimeoutInMilliseconds", 0);
            OperationTimeoutInMilliSec = GetIntSettings(config, "operationTimeoutInMilliseconds", 0);
        }

        // 1) Use key available inside AppSettings
        // 2) Use literal value as given in config
        // 3) Both are null than use default value.
        private static string GetStringSettings(NameValueCollection config, string attrName, string defaultVal)
        {
            string literalValue = GetFromConfig(config, attrName);
            if (string.IsNullOrEmpty(literalValue))
            {
                return defaultVal;
            }

            string appSettingsValue = GetFromAppSetting(literalValue);
            if (!string.IsNullOrEmpty(appSettingsValue))
            {
                return appSettingsValue;
            }
            return literalValue;
        }

        // 1) Use key available inside AppSettings
        // 2) Use key available inside ConnectionStrings section
        // 3) Use literal value as given in config
        // 4) Both are null than use default value.
        private static string GetConnectionStringFromConfig(NameValueCollection config, string attrName, string defaultVal)
        {
            string literalValue = GetFromConfig(config, attrName);
            if (string.IsNullOrEmpty(literalValue))
            {
                return defaultVal;
            }

            string appSettingsValue = GetFromAppSetting(literalValue);
            if (!string.IsNullOrEmpty(appSettingsValue))
            {
                return appSettingsValue;
            }

            if (!string.IsNullOrWhiteSpace(literalValue) 
                && ConfigurationManager.ConnectionStrings[literalValue] != null
                && !string.IsNullOrWhiteSpace(ConfigurationManager.ConnectionStrings[literalValue].ConnectionString))
            {
                return ConfigurationManager.ConnectionStrings[literalValue].ConnectionString;
            }
            return literalValue;
        }

        // 1) Check if literal value is valid integer than use it as it is
        // 2) Use app setting value corrosponding to this string
        // 3) Both are null than use default value.
        private static int GetIntSettings(NameValueCollection config, string attrName, int defaultVal)
        {
            string literalValue = null;
            try
            {
                literalValue = GetFromConfig(config, attrName);
                if (literalValue == null)
                {
                    return defaultVal;
                }
                return int.Parse(literalValue);
            }
            catch (FormatException)
            {}

            string appSettingsValue = GetFromAppSetting(literalValue);
            if (appSettingsValue == null)
            {
                // This will blow up but gives right message to customer
                return int.Parse(literalValue);
            }
            return int.Parse(appSettingsValue);
        }

        // 1) Check if literal value is valid bool than use it as it is
        // 2) Use app setting value corrosponding to this string
        // 3) Both are null than use default value.
        private static bool GetBoolSettings(NameValueCollection config, string attrName, bool defaultVal)
        {
            string literalValue = null;
            try
            {
                literalValue = GetFromConfig(config, attrName);
                if (literalValue == null)
                {
                    return defaultVal;
                }
                return bool.Parse(literalValue);
            }
            catch (FormatException)
            { }

            string appSettingsValue = GetFromAppSetting(literalValue);
            if (appSettingsValue == null)
            {
                // This will blow up but gives right message to customer
                return bool.Parse(literalValue);
            }
            return bool.Parse(appSettingsValue);
        }

        // Reads value from app settings (mostly azure app settings)
        private static string GetFromAppSetting(string attrName)
        {
            if (!string.IsNullOrEmpty(attrName))
            {
                string paramFromAppSetting = ConfigurationManager.AppSettings[attrName];
                if (!string.IsNullOrEmpty(paramFromAppSetting))
                {
                    return paramFromAppSetting;
                }
            }
            return null;
        }

        // Reads string value from web.config session state section
        private static string GetFromConfig(NameValueCollection config, string attrName)
        {
            string[] attrValues = config.GetValues(attrName);
            if (attrValues != null && attrValues.Length > 0 && !string.IsNullOrEmpty(attrValues[0]))
            {
                return attrValues[0];
            }
            return null;
        }

        // Preference for fetching connection string
        // Either use "settingsClassName" and "settingsMethodName" to provide connectionString
        // Or use "connectionString" web.config settings to fetch value
        // If using "connectionString" then it trys to do following in order
        // 1) Fetch value from App Settings section for key which is value for "connectionString"
        // 2) If option 1 is not working, Fetch value from Web.Config ConnectionStrings section for key which is value for "connectionString"
        // 3) If option 1 and 2 is not working, use value of "connectionString" as it is
        internal static string GetConnectionString(NameValueCollection config)
        {
            string SettingsClassName = GetStringSettings(config, "settingsClassName", null);
            string SettingsMethodName = GetStringSettings(config, "settingsMethodName", null);
            string connectionString = GetConnectionStringFromConfig(config, "connectionString", null);

            if (!string.IsNullOrWhiteSpace(connectionString) && (!string.IsNullOrEmpty(SettingsClassName) || !string.IsNullOrEmpty(SettingsMethodName)))
            {
                throw new ConfigurationErrorsException(RedisProviderResource.ConnectionStringException);
            }

            if (!string.IsNullOrEmpty(SettingsClassName) && !string.IsNullOrEmpty(SettingsMethodName))
            {
                // Find 'Type' that is same as fully qualified class name if not found than also don't throw error and ignore case while searching
                Type SettingsClass = Type.GetType(SettingsClassName, throwOnError: false, ignoreCase: true);

                if (SettingsClass == null)
                {
                    // If class name is not assembly qualified name than look for class in all assemblies one by one
                    SettingsClass = GetClassFromAssemblies(SettingsClassName);
                }

                if (SettingsClass == null)
                {
                    // All ways of loading assembly are failed so throw
                    throw new TypeLoadException(string.Format(RedisProviderResource.ClassNotFound, SettingsClassName));
                }

                MethodInfo SettingsMethod = SettingsClass.GetMethod(SettingsMethodName, new Type[] { });
                if (SettingsMethod == null)
                {
                    throw new MissingMethodException(string.Format(RedisProviderResource.MethodNotFound, SettingsMethodName, SettingsClassName));
                }
                if ((SettingsMethod.Attributes & MethodAttributes.Static) == 0)
                {
                    throw new MissingMethodException(string.Format(RedisProviderResource.MethodNotStatic, SettingsMethodName, SettingsClassName));
                }
                if (!(typeof(String)).IsAssignableFrom(SettingsMethod.ReturnType))
                {
                    throw new MissingMethodException(string.Format(RedisProviderResource.MethodWrongReturnType, SettingsMethodName, SettingsClassName, "String"));
                }
                connectionString = (String)SettingsMethod.Invoke(null, new object[] { });
            }
            return connectionString;
        }

        internal static void EnableLoggingIfParametersAvailable(NameValueCollection config)
        {
            string LoggingClassName = GetStringSettings(config, "loggingClassName", null);
            string LoggingMethodName = GetStringSettings(config, "loggingMethodName", null);
            
            if( !string.IsNullOrEmpty(LoggingClassName) && !string.IsNullOrEmpty(LoggingMethodName) )
            {
                // Find 'Type' that is same as fully qualified class name if not found than also don't throw error and ignore case while searching
                Type LoggingClass = Type.GetType(LoggingClassName, throwOnError: false, ignoreCase: true);
                
                if (LoggingClass == null)
                {
                    // If class name is not assembly qualified name than look for class in all assemblies one by one
                    LoggingClass = GetClassFromAssemblies(LoggingClassName);
                }

                if (LoggingClass == null)
                {
                    // All ways of loading assembly are failed so throw
                    throw new TypeLoadException (string.Format(RedisProviderResource.ClassNotFound, LoggingClassName));
                }

                MethodInfo LoggingMethod = LoggingClass.GetMethod(LoggingMethodName, new Type[] { });
                if (LoggingMethod == null)
                {
                    throw new MissingMethodException(string.Format(RedisProviderResource.MethodNotFound, LoggingMethodName, LoggingClassName));
                }
                if ((LoggingMethod.Attributes & MethodAttributes.Static) == 0)
                {
                    throw new MissingMethodException(string.Format(RedisProviderResource.MethodNotStatic, LoggingMethodName, LoggingClassName));
                }
                if (!(typeof(System.IO.TextWriter)).IsAssignableFrom(LoggingMethod.ReturnType))
                {
                    throw new MissingMethodException(string.Format(RedisProviderResource.MethodWrongReturnType, LoggingMethodName, LoggingClassName, "System.IO.TextWriter"));
                }
                LogUtility.logger = (TextWriter) LoggingMethod.Invoke(null, new object[] {});
            }
        }

        internal static Type GetClassFromAssemblies(string ClassName)
        {
            foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies())
            {
                // If class name is not assembly qualified name than look for class name in all assemblies one by one
                Type ClassType = a.GetType(ClassName, throwOnError: false, ignoreCase: true);
                if (ClassType == null)
                {
                    // If class name is not assembly qualified name and it also doesn't contain namespace (it is just class name) than
                    // try to use assembly name as namespace and try to load class from all assemblies one by one 
                    ClassType = a.GetType(a.GetName().Name + "." + ClassName, throwOnError: false, ignoreCase: true);
                }
                if (ClassType != null)
                {
                    return ClassType;
                }
            }
            return null;
        }
    }
}
