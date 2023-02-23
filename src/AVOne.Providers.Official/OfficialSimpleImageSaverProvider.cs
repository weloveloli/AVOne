// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Providers.Official
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

    public class OfficialSimpleImageSaverProvider : IImageSaverProvider
    {
        public string Name => "Official";

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
            string filename;
            var extension = MimeTypesHelper.ToExtension(mimeType);
            switch (type)
            {
                case ImageType.Art:
                    filename = "-clearart";
                    break;
                case ImageType.BoxRear:
                    filename = "-back";
                    break;
                case ImageType.Thumb:
                    filename = "-landscape";
                    break;
                case ImageType.Primary:
                    filename = string.Empty;
                    break;
                case ImageType.Backdrop:
                    filename = "-backdrop";
                    break;
                default:
                    filename = "-" + type.ToString().ToLowerInvariant();
                    break;
            }
            var path = Path.Join(Directory.GetParent(item.TargetPath)!.FullName, item.TargetName + filename + extension);
            var fileStreamOptions = FileOptionsHelper.AsyncWriteOptions;
            fileStreamOptions.Mode = FileMode.Create;
            fileStreamOptions.PreallocationSize = source.Length;
            var fs = new FileStream(path, fileStreamOptions);
            await using (fs.ConfigureAwait(false))
            {
                await source.CopyToAsync(fs, cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
