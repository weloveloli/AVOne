// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

#pragma warning disable CA1813 // Avoid unsealed attributes
namespace AVOne.Api.Attributes
{
    /// <summary>
    /// Internal produces image attribute.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class ProducesFileAttribute : Attribute
    {
        private readonly string[] _contentTypes;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProducesFileAttribute"/> class.
        /// </summary>
        /// <param name="contentTypes">Content types this endpoint produces.</param>
        public ProducesFileAttribute(params string[] contentTypes)
        {
            _contentTypes = contentTypes;
        }

        /// <summary>
        /// Gets the configured content types.
        /// </summary>
        /// <returns>the configured content types.</returns>
        public string[] GetContentTypes() => _contentTypes;
    }
}
