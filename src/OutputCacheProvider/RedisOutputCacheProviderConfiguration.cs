//
// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.
//

using System;
using System.Collections.Specialized;
using static Microsoft.Web.Redis.ProviderConfigurationExtension;

namespace Microsoft.Web.Redis
{
    internal class OutputCacheProviderConfiguration : IProviderConfiguration
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

        internal OutputCacheProviderConfiguration(NameValueCollection config)
        {
            GetIProviderConfiguration(config, this);

            // No retry login for output cache provider
            RetryTimeout = TimeSpan.Zero;

            // Session state specific attribute which are not applicable to output cache
            ThrowOnError = true;
            RequestTimeout = TimeSpan.Zero;
            SessionTimeout = TimeSpan.Zero;

            LogUtility.LogInfo($"Host: {Host}, Port: {Port}, UseSsl: {UseSsl}, DatabaseId: {DatabaseId}, ApplicationName: {ApplicationName}");
        }
    }
}