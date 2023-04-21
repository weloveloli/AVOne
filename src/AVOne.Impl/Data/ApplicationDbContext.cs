// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Impl.Data
{
    using System.IO;
    using AVOne.Configuration;
    using LiteDB;

    /// <summary>
    /// 数据库访问实体.
    /// </summary>
    public class ApplicationDbContext : LiteDatabase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationDbContext"/> class.
        /// </summary>
        /// <param name="connectionString">.</param>
        public ApplicationDbContext(string connectionString)
            : base(connectionString)
        {
        }

        /// <summary>
        /// 创建数据库实体.
        /// </summary>
        /// <param name="applicationPaths">.</param>
        /// <returns>.</returns>
        public static ApplicationDbContext Create(IApplicationPaths applicationPaths)
        {
            var path = Path.Combine(applicationPaths.DataPath, "avone.db");
            return new ApplicationDbContext(path);
        }
    }
}
