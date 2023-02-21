// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

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
