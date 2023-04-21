// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

#nullable disable


namespace AVOne.Providers.Metadata
{
    using AVOne.Enum;
    using AVOne.Models.Info;
    using AVOne.Models.Item;

    /// <summary>
    /// Interface IImageProvider.
    /// </summary>
    public interface IRemoteImageProvider : IImageProvider
    {
        /// <summary>
        /// Gets the supported images.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>IEnumerable{ImageType}.</returns>
        IEnumerable<ImageType> GetSupportedImages(BaseItem item);

        /// <summary>
        /// Gets the images.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task{IEnumerable{RemoteImageInfo}}.</returns>
        Task<IEnumerable<RemoteImageInfo>> GetImages(BaseItem item, CancellationToken cancellationToken);

        /// <summary>
        /// Gets the image response.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task{HttpResponseInfo}.</returns>
        Task<HttpResponseMessage> GetImageResponse(string url, CancellationToken cancellationToken);
    }
}
