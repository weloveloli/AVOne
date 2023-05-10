// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Providers.Official.Metadata
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using AVOne.Common.Helper;
    using AVOne.Enum;
    using AVOne.IO;
    using AVOne.Models.Item;
    using AVOne.Providers.Metadata;

    public class DefaultImageSaverProvider : IImageSaverProvider
    {
        public string Name => "DefaultImageSaver";

        /// <inheritdoc/>
        public async Task SaveImage(BaseItem item, Stream source, string mimeType, ImageType type, int? imageIndex, CancellationToken cancellationToken)
        {
            ArgumentException.ThrowIfNullOrEmpty(mimeType);
            var index = imageIndex ?? 0;
            if (item is PornMovie)
            {
                await SaveImageForPornMovie(item, source, mimeType, type, index, cancellationToken);
            }
        }

        private async Task SaveImageForPornMovie(BaseItem item, Stream source, string mimeType, ImageType type, int imageIndex, CancellationToken cancellationToken)
        {
            var extension = MimeTypesHelper.ToExtension(mimeType);
            var filename = type switch
            {
                ImageType.Art => "-clearart",
                ImageType.BoxRear => "-back",
                ImageType.Thumb => "-landscape",
                ImageType.Primary => string.Empty,
                ImageType.Backdrop => "-backdrop",
                _ => "-" + type.ToString().ToLowerInvariant(),
            };
            var path = Path.Join(Directory.GetParent(item.TargetPath)!.FullName, Path.GetFileNameWithoutExtension(item.TargetPath) + filename + extension);
            var fileStreamOptions = FileOptionsHelper.AsyncWriteOptions;
            fileStreamOptions.Mode = FileMode.Create;
            fileStreamOptions.PreallocationSize = source.Length;
            var fs = new FileStream(path, fileStreamOptions);
            await using (fs.ConfigureAwait(false))
            {
                await source.CopyToAsync(fs, cancellationToken).ConfigureAwait(false);
            }
            item.AddImage(new Models.Info.ItemImageInfo
            {
                Path = path,
                Type = type,
                DateModified = DateTime.Now
            });
        }
    }
}
