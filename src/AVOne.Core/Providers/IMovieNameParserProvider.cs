// Copyright (c) 2023 Weloveloli. All rights reserved.
// Licensed under the Apache V2.0 License.
#nullable disable

namespace AVOne.Providers
{
    using AVOne.Enum;

    public interface IPornMovieNameParserProvider : IOrderProvider
    {
        public MovieId Parse(string filePath);
    }

    /// <summary>
    /// 番号
    /// </summary>
    public class MovieId
    {
        /// <summary>
        /// 类型
        /// </summary>
        public MovieIdCategory Category { get; set; }

        /// <summary>
        /// Gets or sets the flags.
        /// </summary>
        /// <value>
        /// The flags.
        /// </value>
        public PornMovieFlags Flags { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="MovieId"/> is valid.
        /// </summary>
        /// <value>
        ///   <c>true</c> if valid; otherwise, <c>false</c>.
        /// </value>
        public bool Valid { get; set; } = true;

        /// <summary>
        /// 解析到的id
        /// </summary>
        public string Id
        {
            get { return _id; }
            set { _id = value; }
        }
        private string _id;

        /// <summary>
        /// 文件名
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// 转换为字符串
        /// </summary>
        /// <returns></returns>
        public override string ToString() => Id;

        /// <summary>
        /// 转换
        /// </summary>
        /// <param name="id"></param>
        public static implicit operator MovieId(string id)
            => new MovieId() { Id = id };

        /// <summary>
        /// 转换
        /// </summary>
        /// <param name="id"></param>
        public static implicit operator string(MovieId id)
            => id?.Id;


        public static MovieId Empty(string name) => new() { Valid = false, Category = MovieIdCategory.None, Id = string.Empty, FileName = name, Flags = PornMovieFlags.None};
    }
}
