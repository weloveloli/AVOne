﻿// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Helper
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Class EventHelper.
    /// </summary>
    public static class AVOneEventHelper
    {
        /// <summary>
        /// Fires the event.
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="EventArgs" /> instance containing the event data.</param>
        /// <param name="logger">The logger.</param>
        public static void QueueEventIfNotNull(EventHandler? handler, object sender, EventArgs args, ILogger logger)
        {
            if (handler is not null)
            {
                Task.Run(() =>
                {
                    try
                    {
                        handler(sender, args);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Error in event handler");
                    }
                });
            }
        }

        /// <summary>
        /// Queues the event.
        /// </summary>
        /// <typeparam name="T">Argument type for the <c>handler</c>.</typeparam>
        /// <param name="handler">The handler.</param>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The args.</param>
        /// <param name="logger">The logger.</param>
        public static void QueueEventIfNotNull<T>(EventHandler<T>? handler, object sender, T args, ILogger logger)
        {
            if (handler is not null)
            {
                Task.Run(() =>
                {
                    try
                    {
                        handler(sender, args);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Error in event handler");
                    }
                });
            }
        }
    }
}
