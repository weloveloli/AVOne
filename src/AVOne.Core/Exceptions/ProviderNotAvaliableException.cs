// Copyright (c) 2023 Weloveloli. All rights reserved.
// Licensed under the Apache V2.0 License.

namespace AVOne.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    public class ProviderNotAvaliableException : NotSupportedException
    {
        public ProviderNotAvaliableException(string providerName, string message) : base(message)
        {
            ProviderName = providerName;
        }

        public ProviderNotAvaliableException(string providerName, string message, Exception e) : base(message, e)
        {

        }
        protected ProviderNotAvaliableException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
        public string ProviderName { get; }
    }
}
