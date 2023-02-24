// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Providers.Official.Download.Extensions
{
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;

    internal static class ProcessExtension
    {
        public static async Task WaitForExitPatchAsync(this Process process,
            CancellationToken cancellationToken = default)
        {
#if NET48 || NET47 || NET46 || NET45 || NETSTANDARD2_1 || NETSTANDARD2_0
            await process._WaitForExitAsync(cancellationToken);
#else
            await process.WaitForExitAsync(cancellationToken);
#endif
        }
    }
}
