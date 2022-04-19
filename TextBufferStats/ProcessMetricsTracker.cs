// <copyright file="ProcessMetricsTracker.cs" company="PeaceMaker">
// Copyright (c) PeaceMaker Corporation. All rights reserved.
// </copyright>

using System;
using System.Threading;
using System.Threading.Tasks;

namespace TextBufferStats
{
    /// <summary>
    /// A tracker that will run in the background and will be responsible for capturing
    /// the statistics about a text-buffer and how it's performing based on various
    /// operations.
    /// </summary>
    public class ProcessMetricsTracker
    {
        /// <summary>
        /// The statistics capturing interface implementation. We use this
        /// to capture whatever custom statistics we need for the text buffer.
        /// Any common system-wide statistics will be capturued inside the "ProcessMetricsTracker"
        /// class itself since they are buuffer implementation agnostic.
        /// </summary>
        private readonly IEmitStatistics bufferStatistics;

        /// <summary>
        /// Time span between two consecutive metrics capture.
        /// </summary>
        private readonly TimeSpan delayBetweenCaptures;

        /// <summary>
        /// Initializes an object for the tracker.
        /// </summary>
        /// <param name="bufferStats">The statistics implementation that will be usued for capturing the buffer specific metrics.</param>
        /// <param name="delayBetweenCaptures">Defines the frequency of capturing these metrics.</param>
        public ProcessMetricsTracker(IEmitStatistics bufferStats, TimeSpan delayBetweenCaptures)
        {
            this.bufferStatistics = bufferStats;
            this.delayBetweenCaptures = delayBetweenCaptures;
        }

        /// <summary>
        /// Starts the tracker.
        /// </summary>
        /// <param name="cancellationToken">Token used to track the execution amongst threads/tasks.</param>
        /// <returns></returns>
        public async Task StartTracking(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                // Capture the current state of the system.
                this.bufferStatistics.CaptureStatistics();

                // Sleep between invocations.
                await Task.Delay(this.delayBetweenCaptures);
            }
        }
    }
}