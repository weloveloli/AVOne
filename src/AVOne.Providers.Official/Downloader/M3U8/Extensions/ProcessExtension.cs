// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Providers.Official.Downloader.M3U8.Extensions
{
    using System.Diagnostics;

    internal static class ProcessExtension
    {
        public static async Task WaitForExitPatchAsync(this Process process,
            CancellationToken cancellationToken = default)
        {
            await process.WaitForExitAsync(cancellationToken);
        }

        // https://stackoverflow.com/questions/470256/process-waitforexit-asynchronously
        private static Task _WaitForExitAsync(this Process process,
            CancellationToken cancellationToken = default)
        {
            if (process.HasExited)
                return Task.FromResult(true);

            var tcs = new TaskCompletionSource<bool>();
            process.EnableRaisingEvents = true;
            process.Exited += (sender, args) => tcs.TrySetResult(true);
            if (cancellationToken != default)
                cancellationToken.Register(() => tcs.SetCanceled());

            return process.HasExited ? Task.FromResult(true) : tcs.Task;
        }
    }
}
