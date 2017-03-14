//
// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Web.SessionState;

namespace Microsoft.Web.Redis.Tests
{
    internal static class Utility
    {
        internal static void SetConfigUtilityToDefault()
        {
            RedisSessionStateProvider.configuration = GetDefaultConfigUtility(); 
        }

        internal static ChangeTrackingSessionStateItemCollection GetChangeTrackingSessionStateItemCollection()
        {
            return new ChangeTrackingSessionStateItemCollection(new RedisUtility(GetDefaultConfigUtility()));
        }

        internal static ProviderConfiguration GetDefaultConfigUtility()
        {
            ProviderConfiguration configuration = new ProviderConfiguration();
            configuration.SessionTimeout = new TimeSpan(0, 15, 0); //15 min
            configuration.RequestTimeout = new TimeSpan(0, 1, 30); //1.5 min
            configuration.Host = "127.0.0.1";
            configuration.Port = 0;
            configuration.AccessKey = null;
            configuration.UseSsl = false;
            configuration.DatabaseId = 0;
            configuration.ApplicationName = null;
            configuration.ConnectionTimeoutInMilliSec = 5000;
            configuration.OperationTimeoutInMilliSec = 1000;
            configuration.RetryTimeout = TimeSpan.Zero;
            configuration.ThrowOnError = true;
            configuration.RedisSerializerType = null;
            return configuration;
        }

        internal static bool CompareSessionStateStoreData(SessionStateStoreData obj1, SessionStateStoreData obj2)
        {
            if ((obj1 == null && obj2 != null) || (obj1 != null && obj2 == null))
            {
                return false;
            }
            else if (obj1 != null && obj2 != null)
            {
                if (obj1.Timeout != obj2.Timeout)
                {
                    return false;
                }

                System.Collections.Specialized.NameObjectCollectionBase.KeysCollection keys1 = obj1.Items.Keys;
                System.Collections.Specialized.NameObjectCollectionBase.KeysCollection keys2 = obj2.Items.Keys;

                if ((keys1 != null && keys2 == null) || (keys1 == null && keys2 != null))
                {
                    return false;
                }
                else if (keys1 != null && keys2 != null)
                {
                    foreach (string key in keys1)
                    {
                        if (obj2.Items[key] == null)
                        {
                            return false;
                        }
                    }
                }

            }
            return true;
        }
    }
}
