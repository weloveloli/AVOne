// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Providers.Official.Download.Utils
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    internal class RetryTask
    {
        public static async Task Run(Func<int, Exception?, Task> func,
            int delay, int? retry = null, CancellationToken token = default)
        {
            var _count = 0;
            var _retry = retry ?? int.MaxValue;
            var exception = null as Exception;
            while (true)
            {
                try
                {
                    await func(_count, exception);
                    break;
                }
                catch (Exception ex)
                {
                    token.ThrowIfCancellationRequested();
                    if (_count >= _retry)
                    {
                        throw;
                    }

                    _count++;
                    exception = ex;
                    await Task.Delay(delay, token);
                }
            }
        }
    }
}
