// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Common.Helper
{
    public static class ExecutableHelper
    {
        /// <summary>
        /// find the path of executable
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string? FindExecutable(string name)
        {
            var fileExt = OperatingSystem.IsWindows() ? ".exe" : "";
            var searchPath = new[] { Environment.CurrentDirectory, Path.GetDirectoryName(Environment.ProcessPath) };
            var envPath = Environment.GetEnvironmentVariable("PATH")?.Split(Path.PathSeparator) ??
                          Array.Empty<string>();
            return searchPath.Concat(envPath).Select(p => Path.Combine(p, name + fileExt)).FirstOrDefault(File.Exists);
        }

        public static bool IsExecutable(string? path)
        {
            return !string.IsNullOrEmpty(path) && File.Exists(path);
        }
    }
}
