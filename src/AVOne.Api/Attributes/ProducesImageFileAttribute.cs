// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Api.Attributes
{
    /// <summary>
    /// Produces file attribute of "image/*".
    /// </summary>
    public sealed class ProducesImageFileAttribute : ProducesFileAttribute
    {
        private const string ContentType = "image/*";

        /// <summary>
        /// Initializes a new instance of the <see cref="ProducesImageFileAttribute"/> class.
        /// </summary>
        public ProducesImageFileAttribute()
            : base(ContentType)
        {
        }
    }
}
