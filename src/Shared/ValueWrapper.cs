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

        public static ValueWrapper GetValueWrapperFromSerializedvalue(byte[] serializedvalue)
        {
            return new ValueWrapper(serializedvalue);
        }

        private ValueWrapper(byte[] serializedvalue)
        {
            Serializedvalue = serializedvalue;
        }

        public static ValueWrapper GetValueWrapperFromActualValue(object actualValue)
        {
            return new ValueWrapper(actualValue);
        }

        private ValueWrapper(object actualValue)
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
