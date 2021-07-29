// ----------------------------------------------------------------------
// -----------------------------------------------------------------------
// <copyright file="BackgroundTaskUtils.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
// ----------------------------------------------------------------------

namespace BackgroundTasks
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.Contracts;
    using System.Threading;
    using System.Threading.Tasks;
    using Windows.ApplicationModel.Background;

    /// <summary>
    /// Handle tasks common to all background tasks.
    /// </summary>
    internal class BackgroundTaskUtils
    {
        /// <summary>
        /// Run the background task and then run common post background task functions (telemetry flush and SurfaceApp update)
        /// </summary>
        /// <param name="taskInstance">The background task instance data.</param>
        /// <param name="taskFunc">The background task code.</param>
        /// <returns>The <see cref="Task"/>.</returns>
        internal static async Task RunBackgroundTaskAsync(
            IBackgroundTaskInstance taskInstance,
            Func<CancellationToken, Task> taskFunc)
        {
            int startTicks = Environment.TickCount;

            using (CancellationTokenSource cancellationSource = new CancellationTokenSource())
            {
                void TaskInstanceCanceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
                {
                    cancellationSource.Cancel();
                }

                BackgroundTaskDeferral deferral = taskInstance.GetDeferral();
                try
                {
                    taskInstance.Canceled += TaskInstanceCanceled;
                    await taskFunc(cancellationSource.Token).ConfigureAwait(false);
                }
                finally
                {
                    taskInstance.Canceled -= TaskInstanceCanceled;
                    deferral.Complete();
                }
            }
        }
    }
}
