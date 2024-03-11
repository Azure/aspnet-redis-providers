//
// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.
//


using System;

namespace Microsoft.Web.Redis
{
    // Interface for Redis provider configuration
    interface IProviderConfiguration
    {
        TimeSpan RequestTimeout { get; set; } // Timeout for individual Redis requests
        TimeSpan SessionTimeout { get; set; } // Timeout for Redis session
        int Port { get; set; } // Redis server port
        string Host { get; set; } // Redis server host
        string AccessKey { get; set; } // Access key for Redis server
        TimeSpan RetryTimeout { get; set; } // Timeout for retrying failed Redis requests
        bool ThrowOnError { get; set; } // Flag to indicate whether to throw an exception on Redis errors
        bool UseSsl { get; set; } // Flag to indicate whether to use SSL for Redis connection
        int DatabaseId { get; set; } // Redis database ID
        string ApplicationName { get; set; } // Name of the application using Redis
        int ConnectionTimeoutInMilliSec { get; set; } // Connection timeout in milliseconds
        int OperationTimeoutInMilliSec { get; set; } // Operation timeout in milliseconds
        string ConnectionString { get; set; } // Redis connection string
    }
}

