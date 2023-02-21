// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

#nullable disable
namespace AVOne.Configuration
{
    public class NameValue
    {
        public string Name { get; set; }

        public string Value { get; set; }

        public NameValue()
        {

        }
        public NameValue(string name, string value)
        {
            Name = name;
            Value = value;
        }
    }
}
