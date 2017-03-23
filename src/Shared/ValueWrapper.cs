//
// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.
//

namespace Microsoft.Web.Redis
{
    internal class ValueWrapper
    {
        internal object ActualValue { get; set; }
        internal byte[] Serializedvalue { get; set; }

        public ValueWrapper(byte[] serializedvalue)
        {
            Serializedvalue = serializedvalue;
        }

        public ValueWrapper(object actualValue)
        {
            ActualValue = actualValue;
        }

        public object GetActualValue(RedisUtility utility)
        {
            if (ActualValue == null)
            {
                ActualValue = utility.GetObjectFromBytes(Serializedvalue);
            }
            return ActualValue;
        }
    }
}
