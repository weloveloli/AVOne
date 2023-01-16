// Copyright (c) 2023 Weloveloli. All rights reserved.
// Licensed under the Apache V2.0 License.

namespace AVOne.Models.Info
{
    using AVOne.Enum;

    /// <summary>
    /// Represents a single video file.
    /// </summary>
    public class VideoFileInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VideoFileInfo"/> class.
        /// </summary>
        /// <param name="name">Name of file.</param>
        /// <param name="path">Path to the file.</param>
        /// <param name="extraType">Extra type.</param>
        /// <param name="isDirectory">Is directory.</param>
        public VideoFileInfo(string name, string path, ExtraType? extraType = default, bool isDirectory = default)
        {
            Path = path;
            Name = name;
            ExtraType = extraType;
            IsDirectory = isDirectory;
        }

        /// <summary>
        /// Gets or sets the path.
        /// </summary>
        /// <value>The path.</value>
        public string Path { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the type of the extra, e.g. trailer, theme song, behind the scenes, etc.
        /// </summary>
        /// <value>The type of the extra.</value>
        public ExtraType? ExtraType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is a directory.
        /// </summary>
        /// <value>The type.</value>
        public bool IsDirectory { get; set; }

        /// <summary>
        /// Gets the file name without extension.
        /// </summary>
        /// <value>The file name without extension.</value>
        public ReadOnlySpan<char> FileNameWithoutExtension => !IsDirectory
            ? System.IO.Path.GetFileNameWithoutExtension(Path.AsSpan())
            : System.IO.Path.GetFileName(Path.AsSpan());

        /// <inheritdoc />
        public override string ToString()
        {
            return "VideoFileInfo(Name: '" + Name + "')";
        }
    }
}
