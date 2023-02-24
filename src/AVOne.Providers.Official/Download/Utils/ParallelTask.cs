// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Providers.Official.Download.Utils
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    internal class ParallelTask
    {
        public static async Task Run<TSource>(IEnumerable<TSource> source,
            Func<TSource, CancellationToken, Task> worker,
            int maxThreads, int delay, CancellationToken token = default)
        {
            var cts = CancellationTokenSource.CreateLinkedTokenSource(token);

            var queue = new ConcurrentQueue<TSource>(source);

            var tasks = new List<Task>();

            while (true)
            {
                if (tasks.Count == 0 && queue.Count == 0)
                {
                    break;
                }

                if (!token.IsCancellationRequested)
                {
                    if (tasks.Count < maxThreads)
                    {
                        if (queue.TryDequeue(out var next))
                        {
                            var task = Task.Run(async () =>
                            {
                                await worker(next, cts.Token);
                            });
                            tasks.Add(task);
                            await Task.Delay(delay);
                            continue;
                        }
                    }
                }

                if (tasks.Count == 0)
                {
                    token.ThrowIfCancellationRequested();
                    continue;
                }

                var finish = await Task.WhenAny(tasks.ToArray());

                try
                {
                    await finish;
                }
                catch
                {
                    try
                    {
                        cts.Cancel();
                        await Task.WhenAll(tasks.ToArray());
                    }
                    catch { }
                    throw;
                }
                finally
                {
                    _ = tasks.Remove(finish);
                }
            }
        }
    }
}