// Copyright (c) 2023 Weloveloli. All rights reserved.
// Licensed under the Apache V2.0 License.

#nullable disable

namespace AVOne.Models.Info
{
    using System.ComponentModel;
    using AVOne.Enum;
    using AVOne.Naming;

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
        /// <param name="container">Container type.</param>
        /// <param name="extraType">Extra type.</param>
        /// <param name="isDirectory">Is directory.</param>
        public VideoFileInfo(string name, string path, string? container, ExtraType? extraType = default, bool isDirectory = default, bool isStub = default, string? stubType = default, int? year = default, ExtraRule? extraRule = default)
        {
            Path = path;
            Container = container;
            Name = name;
            ExtraType = extraType;
            IsDirectory = isDirectory;
            IsStub = isStub;
            StubType = stubType;
            Year = year;
            ExtraRule = extraRule;
        }

        /// <summary>
        /// Gets or sets the path.
        /// </summary>
        /// <value>The path.</value>
        public string Path { get; set; }

        /// <summary>
        /// Gets or sets the container.
        /// </summary>
        /// <value>The container.</value>
        public string? Container { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the extra rule.
        /// </summary>
        /// <value>The extra rule.</value>
        public ExtraRule? ExtraRule { get; set; }

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

        public int? Year { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is stub.
        /// </summary>
        /// <value><c>true</c> if this instance is stub; otherwise, <c>false</c>.</value>
        public bool IsStub { get; set; }

        /// <summary>
        /// Gets or sets the type of the stub.
        /// </summary>
        /// <value>The type of the stub.</value>
        public string? StubType { get; set; }

        /// <inheritdoc />
        public override string ToString()
        {
            return "VideoFileInfo(Name: '" + Name + "')";
        }
    }
}
