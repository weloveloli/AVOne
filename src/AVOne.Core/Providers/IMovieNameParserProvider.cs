// Copyright (c) 2023 Weloveloli. All rights reserved.
// Licensed under the Apache V2.0 License.

namespace AVOne.Providers
{

    public interface IMovieNameParserProvider : IOrderProvider
    {
        public MovieId Parse(string movieName);
    }

    /// <summary>
    /// 番号
    /// </summary>
    public class MovieId
    {
        /// <summary>
        /// 类型
        /// </summary>
        public MovieIdType type { get; set; }

        /// <summary>
        /// 解析到的id
        /// </summary>
        public string id { get; set; }

        /// <summary>
        /// 文件名
        /// </summary>
        public string file { get; set; }

        /// <summary>
        /// 匹配器
        /// </summary>
        public string matcher { get; set; }

        /// <summary>
        /// 转换为字符串
        /// </summary>
        /// <returns></returns>
        public override string ToString() => id;

        /// <summary>
        /// 转换
        /// </summary>
        /// <param name="id"></param>
        public static implicit operator MovieId(string id)
            => new MovieId() { id = id };

        /// <summary>
        /// 转换
        /// </summary>
        /// <param name="id"></param>
        public static implicit operator string(MovieId id)
            => id?.id;

    }

    /// <summary>
    /// 类型
    /// </summary>
    public enum MovieIdType
    {
        /// <summary>
        /// 不确定
        /// </summary>
        none,

        censored,
        uncensored,
        suren
    }
}
