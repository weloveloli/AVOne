// Copyright (c) 2023 Weloveloli. All rights reserved.
// Licensed under the Apache V2.0 License.

namespace MediaBrowser.Controller.Library
{
    using System.Threading;
    using System.Threading.Tasks;
    using AVOne.Enum;
    using AVOne.Models.Item;
    using AVOne.Providers;

    /// <summary>
    /// Interface IMetadataSaver.
    /// </summary>
    public interface IMetadataSaverProvider : IOrderProvider
    {
        /// <summary>
        /// Determines whether [is enabled for] [the specified item].
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="updateType">Type of the update.</param>
        /// <returns><c>true</c> if [is enabled for] [the specified item]; otherwise, <c>false</c>.</returns>
        bool IsEnabledFor(BaseItem item, ItemUpdateType updateType);

        /// <summary>
        /// Saves the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        Task SaveAsync(BaseItem item, CancellationToken cancellationToken);
    }
}
